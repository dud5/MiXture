module Demo

open System.Runtime.InteropServices
open System
open System.Collections.Generic
open Microsoft.FSharp.NativeInterop

type Void2 = delegate of nativeint -> nativeint
type FSharpEvaluator = delegate of nativeint -> nativeint

module V8 =
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint create_function(nativeint, string, string)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint apply_function(nativeint, nativeint, nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    // for passing arrays, see
    // http://stackoverflow.com/questions/12622160/from-c-sharp-to-f-pinned-array-and-stringbuilder-in-external-function-called-f
    extern nativeint apply_function_arr(nativeint, nativeint, int, nativeint[])
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint execute_string(nativeint, string)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern void print_result(nativeint, nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern void print_string(string)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint createContext()
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern void disposeHandle(nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern void register_function(nativeint, FSharpEvaluator)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint get_argument(nativeint, nativeint, int)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern int arguments_length(nativeint, nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint make_FLump(nativeint, nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern int get_pointer_lump(nativeint)

let v8 r =
    printf "i'm executing"
    r+1

module LumpEmbedding =

    type IdType = nativeint

    type Lump =
        abstract Native: bool
        abstract Pointer: IdType
        abstract Print: unit

    let fs_values = new Dictionary<IdType, Lump>()
    let mutable pointer_counter = 0

    let nextPointerCounter () =
        pointer_counter <- pointer_counter + 1
    
    type 'a FSLump(value : 'a) as this =
        let current_count = new IdType(pointer_counter)
        do
            // pointer_counter <- pointer_counter + 1
            nextPointerCounter()
            fs_values.Add(current_count, this :> Lump)
    
        member this.Value = value
        interface Lump with
            member this.Native = true
            member this.Pointer = current_count
            member this.Print = printf "%A\n" this
    
    type JSLump(pointer: nativeint) =
        static let _context = V8.createContext()
        static member Context = _context

        member this.Print = V8.print_result(JSLump.Context, pointer)

        // constructor initialized by executing some code in a string
        new (code) =
            let pointer = V8.execute_string(JSLump.Context, code)
            JSLump(pointer)


        interface Lump with
            member this.Native = false
            member this.Pointer = pointer
            member this.Print = this.Print
    
    
    let apply_func f arg =
        f.GetType().GetMethod("Invoke", [| arg.GetType() |]).Invoke(f, [| arg |])
    
    
    /// <summary>Extracts the value from an <c>FSLump</c>
    /// <param name="l">The <c>Lump</c> value that is an <c>FSLump</c>
    /// <returns>The Value in an <c>FSLump</c> as an <c>object</c></return>
    let get_fslump_value (l:Lump) =
        l.GetType().GetProperty("Value").GetValue(l, null)
    
    let check_function_type (func: Lump) =
        // check func is a function
        let generic_func = typedefof<Microsoft.FSharp.Core.FSharpFunc<_,_>>
        try
            // the type that fun takes in
            let in_type = func.GetType().GetGenericArguments().[0].GetGenericArguments().[0]
                // the type that fun returns
            // first genericarguments is the type of Lump that func is
            // second generic arguments is the input type (0) and return type of the function (1)
            let return_type = func.GetType().GetGenericArguments().[0].GetGenericArguments().[1]
            // first genericarguments is the type of Lump that func is
            // second generic arguments is the input type (0) and return type of the function (1)
            let return_type = func.GetType().GetGenericArguments().[0].GetGenericArguments().[1]
            let supposed_func_type = generic_func.MakeGenericType(in_type, return_type)
            func.GetType().GetGenericArguments().[0] = supposed_func_type
    
        with
            | :? System.IndexOutOfRangeException -> false
        
    
    // This function takes two Lumps: a function (which will be an FSLump)
    // and an argument (it could be a JSLump). It returns the result as an
    // FSLump
    let eval (func: Lump) (arg: Lump)  =
        if not func.Native then failwith("the function is not an FSLump")
    
    
        // check func is a function
        let generic_func = typedefof<Microsoft.FSharp.Core.FSharpFunc<_,_>>
        let is_function_okay =
            try
                // the type that fun takes in
                let in_type = func.GetType().GetGenericArguments().[0].GetGenericArguments().[0]
                // the type that fun returns
                // first genericarguments is the type of Lump that func is
                // second generic arguments is the input type (0) and return type of the function (1)
                let return_type = func.GetType().GetGenericArguments().[0].GetGenericArguments().[1]
                let supposed_func_type = generic_func.MakeGenericType(in_type, return_type)
                func.GetType().GetGenericArguments().[0] = supposed_func_type
            with
                | :? System.IndexOutOfRangeException -> false
    
        if not is_function_okay then failwith("the first argument doesn't contain a function inside the FSLump or the function is not appropriate for the arguments it was called with")


        // get f as object
        let f = get_fslump_value func
        printf "eval called once\n"
    
        // if the argument is a FSLump, extract its value as an object,
        // otherwise box the JSLump into an object
        let a = if arg.Native then (get_fslump_value arg) else box(arg)
    
    
        // the type that fun takes in
        let in_type = func.GetType().GetGenericArguments().[0].GetGenericArguments().[0]
        // the type that fun returns
        // first genericarguments is the type of Lump that func is
        // second generic arguments is the input type (0) and return type of the function (1)
        let return_type = func.GetType().GetGenericArguments().[0].GetGenericArguments().[1]
    
        // the type of arg
        let actual_in_type =
            if arg.Native then arg.GetType().GetGenericArguments().[0]
            else arg.GetType()
        
        // type mismatch
        if in_type <> actual_in_type then failwith("type mismatch")
    
        let result = apply_func f a
            
        
        let generic_lump = typedefof<FSLump<_>>
        // Type of the FSLump<return_type>
        let return_lump_type = generic_lump.MakeGenericType(return_type)
        let return_constructor = return_lump_type.GetConstructors().[0]
        
        // construct FSLump<return_type>(result)
        let return_lump = return_constructor.Invoke([| result |])
        return_lump :?> Lump
    
    
    // same as eval but works on lists of arguments
    // since func can be a curried function, it acts as an accumulator argument
    let rec eval_array (func: Lump) (arg: Lump list) =
        match arg with
            | x::xs -> eval_array (eval func x) xs
            | [] -> func
    
    
    
    let (|FS|JS|) (l: Lump) =
        if l.Native then
            FS(l.Pointer)
        else
            JS(l.Pointer)
    
    
    module JS =
        // might refactor this to return the pointer itself
        let get_JS_argument context args index =
            JSLump(V8.get_argument(context, args, index))
    
        // is there an easier way of doing this in C++ with V8?
        let get_all_JS_arguments context args =
            let size = V8.arguments_length(context, args)
            // delete index?
            let init index = get_JS_argument context args index
            Array.init size init
    
        // auxiliary function used to extract the FSLump in fs_values
        // that arg contains, where arg is a JavaScript object wrapping
        // a FSLump
        let extract_fslump (arg: JSLump) =
            let index = IdType(V8.get_pointer_lump((arg :> Lump).Pointer))
            fs_values.[index]
    
        // this function evaluates a function as a FSLump (f) and all its arguments being FSLumps (f)
        // need to register the function with the argument context given
        let evaluator_ff context args =
            let arguments = get_all_JS_arguments context args
            let args_as_fslumps = Array.map extract_fslump arguments
            // pass the function lump (argument 0) and the arguments (from 1 onwards)
            // change this, a huge hack!
            printf "these are the arguments passed to eval_array: %A\n" (Array.toList (args_as_fslumps.[1..]))
            let result = eval_array (args_as_fslumps.[0]) (Array.toList ((args_as_fslumps.[1..])))
            printf "this is the result %A\n" (get_fslump_value(result))
            V8.make_FLump(context, result.Pointer)
    
    
        let evaluator_fj context args =
            let arguments = get_all_JS_arguments context args
            let func = (arguments.[0]) :> Lump
            let arg = (arguments.[0]) :> Lump
            let f = IdType(V8.get_pointer_lump(func.Pointer))
            let result = eval (fs_values.[f]) (arg)
            result.Pointer
    
        let process_arg (arg: JSLump) =
            let lumpy_arg = arg :> Lump
            if (V8.get_pointer_lump(lumpy_arg.Pointer)) = -1 then lumpy_arg
            else extract_fslump(arg)
    
        let evaluator context args =
            let arguments = get_all_JS_arguments context args
            // the JSLumps that contain a pointer to a FSLump are processed into that FSLump
            let processed_args = Array.map process_arg arguments
            printf "these are processed_args %A\n" (Array.toList processed_args)
            let result = eval_array (processed_args.[0]) (Array.toList (processed_args.[1..]))
            printf "this is the result %A\n" (get_fslump_value(result))
            // return the pointer to a FLump object (in JavaScript)
            // containing the pointer to the index in fs_values of the
            // result
            V8.make_FLump(context, result.Pointer)
    
        let create_fun context code name =
            let pointer = V8.create_function(context, code, name)
            in JSLump(pointer)
    
        let execute_JS context code =
            let pointer = V8.execute_string(context, code)
            in JSLump(pointer)
    
        let print_JS context (lump: Lump) =
            match lump with
                | FS(e) ->
                    printf "%A" e
                | JS(pointer) -> V8.print_result(context, pointer)
    
        let apply_JS_func context func arg =
            match (func, arg) with
                | (JS(p1), JS(p2)) -> JSLump(V8.apply_function(context, p1, p2))
                | (JS(p1), FS(p2)) ->
                    let flump_in_js = V8.make_FLump(context, p2)
                    JSLump(V8.apply_function(context, p1, flump_in_js))
                // do we really want this?
                | _ -> failwith("blah")
                // | (FS(p1), FS(p2)) -> JSLump(evaluator_ff p1 p2) 
                // | (FS(p1), JS(p2)) -> JSLump(evaluator_fj p1 p2)
    
        // this function, given a Lump, returns the Pointer if it's a
        // JSLump, and creates a FLump JS object if the lump is a FSLump
        let lumpify context lump =
            match lump with
                | JS(p) ->
                    // printf "done one jS\n"
                    printf "this is the jspointer: %d\n" p
                    p
                | FS(p) ->
                    // printf "done one FS\n"
                    let a = V8.make_FLump(context, p)
                    printf "this is the flump pointer: %d\n" a
                    a
    
        let apply_JS_func_arr context func args =
            let arg_pointers = Array.map (lumpify context) args
            JSLump(V8.apply_function_arr(context, (func:>Lump).Pointer, Array.length args, arg_pointers))
    
    
        let dispose_handle handle =
            match handle with
                | JS(pointer) -> V8.disposeHandle(pointer)
                | _ -> failwith "disposeHandle only implemented for JSLump"
