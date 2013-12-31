namespace Mixture

open System
open System.Collections.Generic
open Microsoft.FSharp.NativeInterop
open Microsoft.FSharp.Reflection

module LumpEmbedding =

    type IdType = nativeint

    type Lump =
        abstract Native: bool
        abstract Pointer: IdType
        abstract Print: unit
    
    type FSValuesStorage () =
        let _dict = new Dictionary<IdType, Lump>()
        // only allow positive integers
        let mutable _pointer_counter = 0
        let nextPointerCounter () =
            _pointer_counter <- _pointer_counter + 1
                    
        member this.Dict = _dict
        member this.Insert (lump: FSLump<_>) =
            this.Dict.Add(IdType(_pointer_counter), lump)
            nextPointerCounter ()
            // _pointer_counter
        member this.CurrentPointer = _pointer_counter
        member this.Lookup pointer = this.Dict.[pointer]

    
    and 'a FSLump(value : 'a, fscontext: FSValuesStorage) as this =
        let _fscontext = fscontext
        let _pointer = _fscontext.CurrentPointer
        do _fscontext.Insert(this)
        member this.Value = value

        interface Lump with
            member this.Native = true
            member this.Pointer = IdType(_pointer)
            member this.Print = printf "%A\n" this
    
    let fscontext = new FSValuesStorage()

    type JSLump(pointer: nativeint) =
        let mutable disposed = false
        let cleanup disposing =
            printf "cleanup!\n"
            if not disposed then
                disposed <- true
                if disposing then
                    printf "disposing is true!\n"
//                JSEngine.disposeHandle(pointer, nativeint(0))
                ()

        static let _context = JSEngine.createContext()


        // override this.Finalize() =
            // commented out because it doesn't work (exception on the c++ side)
            // cleanup(false)
        
        static member Context = _context

        member this.Print = JSEngine.print_result(JSLump.Context, pointer)

        // constructor initialized by executing some code in a string
        new (code) =
            let mutable is_exception = false
            let pointer = JSEngine.execute_string(JSLump.Context, code, &is_exception)
            if is_exception then failwith "Exception happened in JavaScript.\nExiting..."
            new JSLump(pointer)

        interface Lump with
            member this.Native = false
            member this.Pointer = pointer
            member this.Print = this.Print

        interface IDisposable with
            member this.Dispose() =
                cleanup(true)
                GC.SuppressFinalize(this)

    
    let apply_func f arg =
        f.GetType().GetMethod("Invoke", [| arg.GetType() |]).Invoke(f, [| arg |])


    /// <summary>Extracts the value from an <c>FSLump</c>
    /// <param name="l">The <c>Lump</c> value that is an <c>FSLump</c>
    /// <returns>The Value in an <c>FSLump</c> as an <c>object</c></return>
    // THIS SHOULD BE A METHOD OF FSLump<_>, but can't make it so!
    let get_fslump_value (l:Lump) =
        l.GetType().GetProperty("Value").GetValue(l, null)
        
            
    // since func can be a curried function, it acts as an accumulator argument
    let rec eval_array (func: Lump) (arg: Lump list) =

        // This function takes two Lumps: a function (which will be an FSLump)
        // and an argument (it could be a JSLump or FSLump). It returns the result as an
        // FSLump upcasted to a Lump
        let eval (func: Lump) (arg: Lump) =
            if not func.Native then failwith("the function is not an FSLump")
    
            // check func contains a function
            let is_function_okay = FSharpType.IsFunction (func.GetType().GetGenericArguments().[0])
    
            if not is_function_okay then failwith("the first argument doesn't contain a function inside the FSLump or the function is not appropriate for the arguments it was called with")

    
            // the domain and range of func
            let (domain, range) = func.GetType().GetGenericArguments().[0] |> FSharpType.GetFunctionElements
    
            // the type of arg
            let actual_in_type =
                // if an FSLump
                if arg.Native then arg.GetType().GetGenericArguments().[0]
                else arg.GetType()
        
            // type mismatch
            if domain <> actual_in_type then failwith("type mismatch")

            // get f as object
            let f = get_fslump_value func
            printf "eval called once\n"
            // if the argument is a FSLump, extract its value as an object,
            // otherwise box the JSLump into an object
            let a = if arg.Native then (get_fslump_value arg) else box(arg)

            let result = apply_func f a
        
            let generic_lump = typedefof<FSLump<_>>
            // Type of the FSLump<return_type>
            let return_lump_type = generic_lump.MakeGenericType(range)
            let return_constructor = return_lump_type.GetConstructors().[0]
            
            // construct FSLump<return_type>(result)
            let return_lump = return_constructor.Invoke([| result; fscontext |])
            return_lump :?> Lump


        match arg with
            | x::xs -> eval_array (eval func x) xs
            | [] -> func


    let (|FS|JS|) (l: Lump) =
        if l.Native then
            FS(l.Pointer)
        else
            JS(l.Pointer)
    
    
    module JS =
        // auxiliary function used to extract the FSLump in fs_values
        // that arg contains, where arg is a JavaScript object wrapping
        // a FSLump
        let extract_fslump (arg: JSLump) =
            let index = IdType(JSEngine.get_pointer_lump((arg :> Lump).Pointer))
            fscontext.Lookup index

        // process_arg returns the FSLump (as a Lump) that JSLump contains as an FLump JavaScript object or the argument upcasted as a Lump if it's not an FLump object
        let process_arg (arg: JSLump) =
            let lumpy_arg = arg :> Lump
            if (JSEngine.get_pointer_lump(lumpy_arg.Pointer)) = -1 then lumpy_arg
            else extract_fslump(arg)

        let lumpify (pointer: IdType) = new JSLump(pointer)

        let evaluator context args =
            // the JSLumps that contain a pointer to a FSLump are processed into that FSLump
            let processed_args = args |> JSUtils.get_all_JS_arguments JSLump.Context |> Array.map lumpify |> Array.map process_arg
            printf "these are processed_args %A\n" <| Array.toList processed_args
            // the first argument is the function
            let result = eval_array (processed_args.[0]) (Array.toList (processed_args.[1..]))
            printf "this is the result %A\n" <| get_fslump_value(result)
            // return the pointer to a FLump object (in JavaScript)
            // containing the pointer to the index in fs_values of the
            // result
            // TODO substitute this to a simple new JSLump(new FLump(result.Pointer))
            JSEngine.make_FLump(context, result.Pointer)
    
        // this function, given a Lump, returns the Pointer if it's a
        // JSLump, and creates a FLump JS object if the lump is a FSLump
        let delumpify context lump =
            match lump with
                | JS(p) -> p
                | FS(p) -> JSEngine.make_FLump(context, p)

        let apply_JS_func_arr context func args =
            let arg_pointers = Array.map (delumpify context) args
            let mutable is_exception = Unchecked.defaultof<bool>
            new JSLump(JSEngine.apply_function_arr(context, (func:>Lump).Pointer, Array.length args, arg_pointers, &is_exception))

        // TODO: get rid of this (moving it to the finalizer of JSLump)
        let dispose_handle handle =
            match handle with
 //               | JS(pointer) -> JSEngine.disposeHandle(pointer, nativeint(0))
                | _ -> failwith "disposeHandle only implemented for JSLump"

