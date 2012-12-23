module Demo

open System.Runtime.InteropServices
open System
open System.Collections.Generic
open Microsoft.FSharp.NativeInterop

type Void2 = delegate of nativeint -> nativeint


module V8 =
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint create_function(nativeint, string, string)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint apply_function(nativeint, nativeint, nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    // for passing arrays, see
    // http://stackoverflow.com/questions/12622160/from-c-sharp-to-f-pinned-array-and-stringbuilder-in-external-function-called-f
    extern nativeint apply_function_arr(nativeint, nativeint, nativeint[])
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
    extern void register_function(nativeint, Void2)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint get_argument(nativeint, nativeint, int)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern int arguments_length(nativeint, nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint make_FLump(nativeint, nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern int get_pointer_lump(nativeint, nativeint)

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
// let arg = meth2.Invoke(null, [| lump_arg |])
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
    if not func.Native then failwith("the function is not an FSLump")
    // get f as object
    let f = get_fslump_value func
    // what fun takes in
    let in_type = func.GetType().GetGenericArguments().[0].GetGenericArguments().[0]

    let a = if arg.Native then get_fslump_value arg else box(arg)

    // the type of arg
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


// same as eval but works on lists of arguments
let rec evalu (func: Lump) (arg: Lump list) =
    match arg with
        | x::xs -> evalu (eval func x) xs
        | [] -> func



let (|FS|JS|) (l: Lump) =
    if l.Native then
        FS(l.Pointer)
    else
        JS(l.Pointer)


module JS =

    let evaluator_ff (func: nativeint) (arg: nativeint) =
        let result = eval (fs_values.[func]) (fs_values.[arg])
        result.Pointer

    let evaluator_fj (func: nativeint) (arg: nativeint) =
        let result = eval (fs_values.[func]) (JSLump(arg))
        result.Pointer
    
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
            | (JS(p1), FS(p2)) -> JSLump(V8.apply_function(context, p1, p2))
            // do we really want this?
            | (FS(p1), FS(p2)) -> JSLump(evaluator_ff p1 p2) 
            | (FS(p1), JS(p2)) -> JSLump(evaluator_fj p1 p2)

    let apply_JS_func_arr context func args =
        let arg_pointers = Array.map (fun (x:Lump) -> x.Pointer) args
        // let pinned_pointers = PinnedArray.of_array(arg_pointers)
        let lumpy_func = func :> Lump
        JSLump(V8.apply_function_arr(context, func.Pointer, arg_pointers))

    let get_JS_argument context args index =
        JSLump(V8.get_argument(context, args, index))


    // is there an easier way of doing this in C++ with V8?
    let get_all_JS_arguments context args =
        let size = V8.arguments_length(context, args)
        // delete index?
        let init index = get_JS_argument context args index
        Array.init size init
        

    let dispose_handle handle =
        match handle with
            | JS(pointer) -> V8.dispose_handle(pointer)
            | _ -> failwith "dispose_handle only implemented for JSLump"

