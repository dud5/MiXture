open System.Runtime.InteropServices
open System
open System.Collections.Generic


type Void2 = delegate of nativeint -> nativeint


module V8 =
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint create_function(nativeint, string, string)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint apply_function(nativeint, nativeint, nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint execute_string(nativeint, string)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern void print_result(nativeint, nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern void print_string(string)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint get_context()
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern void dispose_handle(nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern void unmanaged(nativeint, Void2)



type FSharpEvaluator = delegate of nativeint * nativeint -> nativeint


type Lump =
    abstract Native: bool
    abstract Pointer: nativeint


let fs_values = new Dictionary<nativeint, Lump>()
let mutable pointer_counter = 0

type 'a FSLump(value : 'a) as this =
    let current_count = new nativeint(pointer_counter)
    do
        pointer_counter <- pointer_counter + 1
        fs_values.Add(current_count, this :> Lump)

    member this.Value = value
    interface Lump with
        member this.Native = true
        member this.Pointer = current_count

type JSLump(pointer: nativeint) =
    // member this.Pointer = pointer
    interface Lump with
        member this.Native = false
        member this.Pointer = pointer


let apply_func f arg =
    f.GetType().GetMethod("Invoke", [| arg.GetType() |]).Invoke(f, [| arg |])

// This should be called as
// let ty = func_lump.GetType().GetGenericArguments.[0]
// let meth = typeof<Utils>.GetMethod("Process").MakeGenericMethod [| ty |]
// let func = meth.Invoke(null, [| func_lump |])
// let ty2 = lump_arg.GetType().GetGenericArguments.[0]
// let meth2 = typeof<Utils>.GetMethod("Process").MakeGenericMethod [| ty2 |]
// let arg = meth2.Invoke(null, [| lump_arg |]
// let result = invoke func arg

// it could be substituted by something like (this could be abstracted into a function)
// let func = func_lump.GetType().GetProperty("Value").GetValue(func_lump, null)
// let arg = func_arg.GetType().GetProperty("Value").GetValue(func_arg, null)
    
// type Utils =
//     static member Process<'T>(l: FSLump<'T>) =
//         l.Value

let get_fslump_value (l:Lump) =
    l.GetType().GetProperty("Value").GetValue(l, null)

let eval (func: Lump) (arg: Lump)  =
    if func.Native && arg.Native then
        let f = get_fslump_value func
        let a = get_fslump_value arg
        let in_type = func.GetType().GetGenericArguments().[0].GetGenericArguments().[0]
        let actual_in_type = arg.GetType().GetGenericArguments().[0]

        // type mismatch
        if in_type <> actual_in_type then failwith("type mismatch")
        
        let result = apply_func f a
        
        // first genericarguments is the type of Lump that func is
        // second generic arguments is the input type (0) and return type of the function (1)
        let return_type = func.GetType().GetGenericArguments().[0].GetGenericArguments().[1]

        let generic_lump = typedefof<FSLump<_>>
        // Type of the FSLump<return_type>
        let return_lump_type = generic_lump.MakeGenericType(return_type)
        let return_constructor = return_lump_type.GetConstructors().[0]

        // construct FSLump<return_type>(result)
        let return_lump = return_constructor.Invoke([| result |])
        return_lump :?> Lump
    else
        func

let evaluator (func: nativeint) (arg: nativeint) =
    let result = eval (fs_values.[func]) (fs_values.[arg])
    result.Pointer


let (|FS|JS|) (l: Lump) =
    // change this to return l.Pointer (no necessary casts)
    if l.Native then
        FS(l.Pointer)
    else
        let jlump = l :?> JSLump
        JS(l.Pointer)

module JS =
    
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

    let rec apply_JS_func context func arg =
        match (func, arg) with
            | (JS(p1), JS(p2)) -> JSLump(V8.apply_function(context, p1, p2))
            | (JS(p1), FS(p2)) -> JSLump(V8.apply_function(context, p1, p2))
            // this doesn't work if x is an F# function
            // | (FS(x), y) ->
            //     match (fs_values.[x]) with
            //         | :? FSLump<JSLump> as flump ->
            //             let lump = (flump.Value) :> Lump
            //             apply_JS_func context lump.Pointer
                    // | _ as lump-> apply_JS_func context (fs_values.[l]) y
            // | (JS(p1), FS(p2)) -> JSLump(V8.apply_function(context, p1, p2))
            | (_,_) -> failwith "apply_JS_func only implemented for JSLump"

    let dispose_handle handle =
        match handle with
            | JS(pointer) -> V8.dispose_handle(pointer)
            | _ -> failwith "dispose_handle only implemented for JSLump"



let context = V8.get_context();

let main() =

    // let func = new FSLump<int->int->int>(fun x -> fun y -> x+y) :> Lump
    // let arg1 = new FSLump<int>(10) :> Lump
    // let arg2 = new FSLump<int>(32) :> Lump
    // let result = eval (eval func arg1) arg2
    // printf "%A\n" result
    // let result2 = result :?> FSLump<int>
    // printf "%d\n" result2.Value
    // let num = JS.execute_JS context "3"

    // let name = "add"
    // let add = JS.create_fun context "var add = function(arg) {return arg+1;};" "add"

    // printf "Calling function: %s\n" name;
    // printf "Called with argument: ";

    // JS.print_JS context num;

    // let result = JS.apply_JS_func context add num

    // printf "This is the result: ";
    // JS.print_JS context result;

    // JS.dispose_handle(num)
    // printf "disposed num\n"

    // // JS.print_JS context (new FSLump<int>(3));
    // printf "\n"
    // // V8.dispose_handle(num._pointer);
    // // V8.dispose_handle(add._pointer);
    // // V8.dispose_handle(result._pointer);

    // // let er = fs_values.[new nativeint(0)]
    // // let er_casted = er :?> FSLump<int>
    // // printf "%d\n" er_casted.Value;

    // let int2int = FSLump<int->int>(fun x -> x+1)
    // let intval = FSLump<int>(3)
    // printf "%A\n" fs_values
    // // let l1 = int2int :> Lump
    // // let l2 = intval :> Lump
    // // printf "%d\n" l1.Pointer
    // // printf "%d\n" l2.Pointer

    
    // let four = int2int.Value intval.Value
    // printf "%d\n" four
    let p (n: nativeint) =
        printf "Executed in F#, called from JavaScript\n"
        let result = JS.execute_JS context "4" :> Lump
        result.Pointer

    V8.unmanaged(context, new Void2(p))


do
    main()
    V8.dispose_handle(context);
