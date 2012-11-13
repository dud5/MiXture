open System.Runtime.InteropServices
open System
open System.Collections.Generic



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

// Move all these type declarations to a module


// if Lump is parameterized, we can't build a Dictionary<'a Lump , 'a>
// if Lump is not parameterized, the can't downcast it to 'a FSLump
type Lump =
    abstract Native: bool
    // abstract Pointer: nativeint

type 'a FSLump(value : 'a) =
    // have a static property with a counter that keeps incrementing
    // for each FSLump created, which will be the index in the
    // dictionary fs_values
    member x.Value = value
    // static member PointerCounter = 0
    // PointerCounter = FSLump.PointerCounter+1
    // member x.Pointer = FSLump.PointerCounter
    interface Lump with
        member x.Native = true


type JSLump(pointer: nativeint) =
    member x.Pointer = pointer
    interface Lump with
        member x.Native = false


let (|FS|JS|) (l: Lump) =
    // change this to return l.Pointer (no necessary casts)
    if l.Native then
        FS(l)
    else
        let jlump = l :?> JSLump
        JS(jlump.Pointer)


module JS =
    let fs_values = new Dictionary<int, Lump>()

    let create_fun context code name =
        let pointer = V8.create_function(context, code, name)
        in JSLump(pointer)

    let execute_JS context code =
        let pointer = V8.execute_string(context, code)
        in JSLump(pointer)

    let print_JS context (lump: Lump) =
        match lump with
            | FS(e) ->
                // add it to the dic somewhere else (in the creation of the FSLump)
                // DELETE
                fs_values.Add(12, e)
                printf "%A" e
            | JS(pointer) -> V8.print_result(context, pointer)

    let apply_JS_func context func arg =
        match (func, arg) with
            | (JS(p1), JS(p2)) -> JSLump(V8.apply_function(context, p1, p2))
            | (_,_) -> failwith "apply_JS_func only implemented for JSLump"

    let dispose_handle handle =
        match handle with
            | JS(pointer) -> V8.dispose_handle(pointer)
            | _ -> failwith "dispose_handle only implemented for JSLump"




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

    JS.print_JS context (new FSLump<int>(3));
    printf "\n"
    // V8.dispose_handle(num._pointer);
    // V8.dispose_handle(add._pointer);
    // V8.dispose_handle(result._pointer);

    let er = JS.fs_values.[12]
    let er_casted = er :?> FSLump<int>
    printf "%d\n" er_casted.Value;

do
    main()
    V8.dispose_handle(context);
