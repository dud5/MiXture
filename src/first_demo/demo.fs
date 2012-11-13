open System.Runtime.InteropServices
open System
open System.Collections.Generic

type 'a Lump =
    // FJ(JF(x:'a))
    | FS of 'a
    // JF(pointer)
    | JSLump of nativeint

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



module JS =
    let create_fun context code name =
        let pointer = V8.create_function(context, code, name)
        in JSLump(pointer)
        

    let execute_JS context code =
        let pointer = V8.execute_string(context, code)
        in JSLump(pointer)
            
    let print_JS context lump =
        match lump with
            | JSLump(pointer) -> V8.print_result(context, pointer)
            | FS(e) -> printf "%A" e

    let apply_JS_func context func arg =
        match (func, arg) with
            | (JSLump(p1), JSLump(p2)) -> JSLump(V8.apply_function(context, p1, p2))
            | (_,_) -> failwith "apply_JS_func only implemented for JSLump"

    let dispose_handle handle =
        match handle with
            | JSLump(pointer) -> V8.dispose_handle(pointer)
            | _ -> failwith "dispose_handle only implemented for JSLump"


// module FS =
//     let fs_values = new Dictionary<'a Lump, 'a>:

let context = V8.get_context();

let main() =
        
    let num = JS.execute_JS context "3"

    let name = "add"
    let add = JS.create_fun context "var add = function(arg) {return arg+1;};" "add"
    
    printf "Calling function: %s\n" name;
    printf "Called with argument: ";
    
    JS.print_JS context num;
    
    let result = JS.apply_JS_func context add num
    
    printf "This is the result: ";
    JS.print_JS context result;
    
    JS.dispose_handle(num)
    printf "disposed num\n"
    
    // V8.dispose_handle(num._pointer);
    // V8.dispose_handle(add._pointer);
    // V8.dispose_handle(result._pointer);


do
    main()
    V8.dispose_handle(context);
    printf "disposed context\n"
      // Collect garbage & wait until finalizers complete
    System.GC.Collect()
    System.GC.WaitForPendingFinalizers()
    // All objects died by now
    printfn "fin"
 
