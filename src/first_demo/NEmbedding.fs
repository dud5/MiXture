module NEmbedding

open System.Runtime.InteropServices
open Microsoft.FSharp.Reflection

type FSFunction = delegate of nativeint -> nativeint

module V8 =
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint create_function(nativeint, string, string)
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
    extern nativeint get_argument(nativeint, nativeint, int)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern int arguments_length(nativeint, nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint make_FLump(nativeint, nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern int get_pointer_lump(nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern float extractFloat(nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    // see http://stackoverflow.com/questions/5298268/returning-a-string-from-pinvoke
    // for how to return strings from unmanaged code
    extern void extractString(nativeint, System.Text.StringBuilder)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern bool extractBoolean(nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint makeFloat(nativeint, double)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint makeString(nativeint, string)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint makeBoolean(nativeint, bool)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern nativeint makeFunction(nativeint, FSFunction)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern bool isBoolean(nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern bool isNumber(nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern bool isString(nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern bool isFunction(nativeint)
    [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
    extern void hello(System.Text.StringBuilder)

// Use as dummies for the V8 module
// module V8 =
//     let makeFloat(x,y) = nativeint(1)
//     let makeString(a,b) = nativeint(3)
//     let makeBoolean(a,b) =nativeint(1)
//     let makeFunction(a,b) = nativeint(11)
//     let extractFloat(o:JSValue) = 1.2
//     let extractString(o:JSValue, a:System.Text.StringBuilder) = let l = a.Append("hello") in ()
//     let extractBoolean(b:JSValue)= true
//     let createContext () = nativeint(2)
//     let apply_function_arr(a,b,c,d) = nativeint(3)
//     let isBoolean x = true
//     let isString x = true
//     let isNumber x = true
//     let isFunction x = true
//     let print_result(a,b) = ()


type JSValue = nativeint

module JS =
    let get_all_JS_arguments context args =
        let init x = JSValue(3)
        Array.init 1 init


let context = V8.createContext()


let (|Boolean|_|) (b:JSValue) =
    if V8.isBoolean(b) then Some(V8.extractBoolean(b))
    else None

let (|Number|_|) (n:JSValue) =
    if V8.isNumber(n) then Some(V8.extractFloat(n))
    else None


let getString (s:JSValue) =
    let sb = new System.Text.StringBuilder(15)
    V8.extractString(s, sb)
    sb.ToString()

let (|String|_|) (s:JSValue) =
    if V8.isString(s) then Some(getString s)
    else None

let (|Function|_|) (f:JSValue) =
    if V8.isFunction(f) then
        Some(fun (args: JSValue list) -> V8.apply_function_arr(context, f, List.length args, List.toArray args))
    else None

// Embed-project pair
type ('a, 'b) ep = { embed : 'a -> 'b; project: 'b -> 'a }

// // float is an F#: int -> float
// let native_float = float
// let context = V8.createContext();

let float =
    { embed = fun x -> V8.makeFloat(context, x);
    // { embed = fun x -> nativeint(3);
      project = function Number x -> x | _ -> failwith "tried to project a nonfloat" }

let string =
    { embed = fun s -> V8.makeString(context, s);
      project = function String s -> s | _ -> failwith "tried to project a nonstring" }

let int =
    { embed = fun n -> V8.makeFloat(context, n);
      project = fun n -> V8.extractFloat(n) }

let boolean =
    { embed = fun (b: bool) -> V8.makeBoolean(context, b);
      project = function Boolean b -> b | _ -> failwith "tried to project a nonboolean" }

let func =
    { embed = fun (f: JSValue -> JSValue) ->
          let nativef (args: JSValue) =
              let processed_args = args |> JS.get_all_JS_arguments context
              f (processed_args.[0])
          V8.makeFunction(context, new FSFunction(nativef));
      // don't need project
      project = fun r -> fun x -> x }


/// <summary>Embeds a value into an <c>JSvalue</c>, using type information provided
/// <param name="ty">The <c>Type</c> value that specifies the type of the value being embedded</param>
/// <param name="x">The value that is being embedded</param>
/// <return>A value of type <c>JSValue</c> which is the JavaScript equivalent of <c>x</c></return>
let rec embed_reflection ty (x:obj) =
    // if ty = typeof<int> then int.embed(x:?>int)
    if ty = typeof<string> then string.embed(x:?>string)
    elif ty = typeof<float> then float.embed(x:?>float)
    elif ty = typeof<bool> then boolean.embed(x:?>bool)
    elif (FSharpType.IsFunction ty) then
        let domain = FSharpType.GetFunctionElements ty |> fst
        // x needs to be cast here to a function? shit
        // done it with more reflection hacks
        // TODO: get rid of this invoke madness, ask Tomas
        func.embed(fun (arg: JSValue) -> embed (x.GetType().GetMethod("Invoke", [| domain |]).Invoke(x, [| (project_reflection domain arg) |])))
    
    else
        printf "trying to embed a value of unknown type:\n"
        printf "%A\n" ty
        failwith "trying to embed something i don't know how to"


/// <summary>Embeds a value into an <c>JSvalue</c>
/// <param name="x">The value that is being embedded</param>
/// <return>A value of type <c>JSValue</c> which is the JavaScript equivalent of <c>x</c></return>
and embed x : JSValue = embed_reflection (x.GetType()) x

/// <summary>Projects a <c>JSValue</c> into an F# value, using type information provided
/// <param name="ty">The <c>Type</c> value that specifies what type the F# should have
/// <param name="x">The <c>JSValue</c> that is being projected
/// <return>A value of type <c>ty</c> which is the F# equivalent of <c>x</c></return>
and project_reflection ty (x:JSValue) : obj=
    if ty = typeof<string> then string.project(x) |> box
    elif ty = typeof<float> then float.project(x) |> box
    elif (FSharpType.IsFunction ty) then
        match x with
            | Function f ->
                let range = FSharpType.GetFunctionElements ty |> snd
                FSharpValue.MakeFunction(ty, fun arg -> project_reflection range (f [embed arg]))
            | _ -> failwith "trying to project a nonfunction"
    else
        printf "This value could not be projected:\n"
        printf "type specified: %A\n" ty
        printf "value in JS: "
        V8.print_result(context, x)
        failwith "Could not project a value"


/// <summary>Projects a <c>JSValue</c> into an F# value,
/// guided by the type inferrence of F# (<c>'T</c></summary>
/// <param name="x">The <c/>JSValue> that is being projected</param>
/// <return>A value of type <c>'T</c> which is the F# equivalent of <c>x</c></return>
and project<'T> (x: JSValue) : 'T = project_reflection (typeof<'T>) x |> unbox<'T>


let rec echo p =
    match p with
        | Boolean b -> printf "%b : Boolean\n" b
        | Number n -> printf "%f: Number\n" n
        | String s -> printf "%s: String\n" s
        | Function f ->
            printf "it's a function!\n"
            let n = embed 3.0;
            printf "i've embedded 3.0\n"
            // let n = V8.execute_string(context, "3")
            let result = f [n]
            printf "i've applied the function\n"
            V8.print_result(context, result)
            echo result
        | _ -> printf "something else\n"


let main() =
    let p = V8.execute_string(context, "(function(a) {return a+38.1;})")
    let add1 : float -> float = project p
    let r = add1 2.9
    printf "this is r+1 %f\n" (r+1.0)


    let add2 x :float= x + 1.0
    let jadd = embed add2
    echo jadd
    printf "Done\n"
    
    // let r: float = add1 41.1
    // printf "this is r: %f\n" r
    // echo p
    // let s = V8.extractString(p)
    // printf "this is s %s\n" s
    // let sb = new System.Text.StringBuilder(15)
    // V8.hello(sb)
    // let s = sb.ToString()
    // printf "this is s: %s\n" s
    // let three = float.embed(3.0)
    // let fthree = float.project(three)
    // printf "this is the fsharp value %f\n" fthree
    // V8.print_result(context, three)
    // V8.echo("Hello from f@")

do
    main()



let test (a:obj) =
    printf "this is a's type: %A\n" (a.GetType())
    a.GetType().GetMethod("Invoke", [|typeof<int>|]).Invoke(a, [| (box 1) |])
