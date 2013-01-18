module Mixture.NEmbedding

open System.Runtime.InteropServices
open Microsoft.FSharp.Reflection
open System.Collections

type JSValue = nativeint

let context = JSEngine.createContext()


let (|Boolean|_|) (b:JSValue) =
    if JSEngine.isBoolean(b) then Some(JSEngine.extractBoolean(b))
    else None

let (|Integer|_|) (n:JSValue) =
    if JSEngine.isInt32(n) then Some(JSEngine.extractInt(n))
    else None

let (|Number|_|) (n:JSValue) =
    if JSEngine.isNumber(n) then Some(JSEngine.extractFloat(n))
    else None

let getString (s:JSValue) =
    JSEngine.print_result(context, s)
    let length = JSEngine.stringLength(s)
    let sb = new System.Text.StringBuilder(length)
    JSEngine.extractString(s, sb, length)
    printf "this is fs: %s\n" <| sb.ToString()
    sb.ToString()

let (|String|_|) (s:JSValue) =
    if JSEngine.isString(s) then Some(getString s)
    else None

let (|Function|_|) (f:JSValue) =
    if JSEngine.isFunction(f) then
        Some(fun (args: JSValue list) -> JSEngine.apply_function_arr(context, f, List.length args, List.toArray args))
    else None

let (|Null|_|) (x:JSValue) =
    if JSEngine.isNull(x) then Some(Null)
    else None

let (|Undefined|_|) (x:JSValue) =
    if JSEngine.isUndefined(x) then Some(Undefined)
    else None

let (|Array|_|) (arr:JSValue) =
    if JSEngine.isArray(arr) then
        Some()
    else None



// Embed-project pair
type ('a, 'b) ep = { embed : 'a -> 'b; project: 'b -> 'a }

// float is an F#: int -> float
let to_float = float
// int is an F#: float -> int
let to_int = int


// Embed & project strategies for the different "basic" types
let float =
    { embed = fun x -> JSEngine.makeFloat(context, x);
      project = function Number x -> x | _ -> failwith "tried to project a nonfloat" }


let string =
    { embed = fun s -> JSEngine.makeString(context, s);
      project = function String s -> s | _ -> failwith "tried to project a nonstring" }


let int =
    { embed = fun n -> JSEngine.makeFloat(context, to_float n);
      project = function
          | Integer n ->
              // printf "USing Integer %d\n" n
              n
          | Number x ->
              if x = to_float (int x) then (int x) else failwith "tried to project a float or it's outside the valid range"
          | _ -> failwith "tried to project a non int" }

let boolean =
    { embed = fun (b: bool) -> JSEngine.makeBoolean(context, b) ;
      project = function
          | Boolean b -> b
          | String s -> s <> ""
          | Number x -> x <> 0.0
          | Null -> false
          | Undefined -> false
          | _ -> true }

let func =
    { embed = fun (f: JSValue -> JSValue) ->
          let nativef (args: JSValue) =
              let processed_args = args |> JSUtils.get_all_JS_arguments context
              f (processed_args.[0])
          JSEngine.makeFunction(context, new JSEngine.FSharpFunction(nativef)) ;

      // don't need project
      project = fun r x -> failwith "Cannot call project for 'func'" }




// let list =
//     { embed = embed_ienumerable ;
//       project = array.project << Array.toList }

// let array =
//     { embed = embed_ienumerable ;
//       // might need to pass some type information to project
//       project = fun (jarr: JSValue) ->
//           let length = JSEngine.getArrayLength(context, jarr)
//           let farr: JSValue array = Array.zeroCreate length
//           JSEngine.extractArray(context, jarr, length, farr)
//           Array.map project farr }

// let tuples =
//     { embed ;
//       project }

// let records =
//     { embed ;
//       project }



// let generate_JS_object x =


/// <summary>Embeds a value into an <c>JSvalue</c>, using type information provided
/// <param name="ty">The <c>Type</c> value that specifies the type of the value being embedded</param>
/// <param name="x">The value that is being embedded</param>
/// <return>A value of type <c>JSValue</c> which is the JavaScript equivalent of <c>x</c></return>
let rec embed_reflection ty (x:obj) =
    // if ty = typeof<int> then int.embed(x:?>int)
    if ty = typeof<string> then string.embed(x:?>string)
    elif ty = typeof<float> then float.embed(x:?>float)
    elif ty = typeof<int> then int.embed(x:?>int)
    elif ty = typeof<bool> then boolean.embed(x:?>bool)
    elif ty.IsGenericType && ty.GetGenericTypeDefinition() = typedefof<list<_>> then embed_ienumerable(x :?> IEnumerable)
    elif ty.IsArray then embed_ienumerable(x :?> IEnumerable)
    elif (FSharpType.IsFunction ty) then
        let domain = FSharpType.GetFunctionElements ty |> fst
        // x needs to be cast here to a function? shit
        // done it with more reflection hacks
        // TODO: get rid of this invoke madness, ask Tomas
        func.embed(fun (arg: JSValue) -> embed (x.GetType().GetMethod("Invoke", [| domain |]).Invoke(x, [| (project_reflection domain arg) |])))

    // elif (FSharpType.IsRecord ty) then
    //     let props = FSharpType.GetRecordFields ty
    //     Array.iter (fun el ->
    //         let name = el.Name
    //         let value = el.)

    else
        printf "trying to embed a value of unknown type:\n"
        printf "%A\n" ty
        failwith "trying to embed something i don't know how to"


/// <summary>Embeds a value into an <c>JSvalue</c>
/// <param name="x">The value that is being embedded</param>
/// <return>A value of type <c>JSValue</c> which is the JavaScript equivalent of <c>x</c></return>
and embed x : JSValue = embed_reflection (x.GetType()) x

and project_func ty (f:JSValue list -> JSValue) : obj =
    let range = FSharpType.GetFunctionElements ty |> snd
    if FSharpType.IsFunction range then
        FSharpValue.MakeFunction(ty, fun arg -> project_hack range (fun t -> f (embed arg :: t)))
    else
        FSharpValue.MakeFunction(ty, fun arg -> project_hack range ([embed arg] |> f))

/// <summary>Projects a <c>JSValue</c> into an F# value, using type information provided
/// <param name="ty">The <c>Type</c> value that specifies what type the F# should have
/// <param name="x">The <c>JSValue</c> that is being projected
/// <return>A value of type <c>ty</c> which is the F# equivalent of <c>x</c></return>
and project_hack ty (x:obj) : obj =
    match x with
        | :? JSValue as jx -> project_reflection ty jx
        | :? (JSValue list -> JSValue) as f ->
            if FSharpType.IsFunction (x.GetType()) && FSharpType.IsFunction ty then
                project_func ty f
            else failwith "function nonfunction"
        | _ -> failwith "faillllllll"


and project_reflection ty (x:JSValue) : obj =
    if ty = typeof<string> then string.project(x) |> box
    elif ty = typeof<float> then float.project(x) |> box
    elif ty = typeof<int> then int.project(x) |> box
    elif ty = typeof<bool> then boolean.project(x) |> box
    elif (FSharpType.IsFunction ty) then
        match x with
            | Function f -> project_func ty f
            | _ -> failwith "trying to project a nonfunction"

    elif ty.IsGenericType && ty.GetGenericTypeDefinition() = typedefof<list<_>> then project_list(x) |> box
    elif ty.IsArray then
        let el_ty = ty.GetElementType()
        (project_array el_ty x) |> box

    elif ty = typeof<obj> then
        // we don't know what we want, let's see what we can get
        match x with
            | Boolean b -> project_reflection typeof<bool> x
            | Number n -> project_reflection typeof<float> x
            | String s -> project_reflection typeof<string> x
            // | Function f ->
            | _ -> failwith ""
    else
        printf "I don't know how to project this\n"
        printf "type specified: %A\n" ty
        printf "value in JS: "
        JSEngine.print_result(context, x)
        failwith "Could not project a value"


/// <summary>Projects a <c>JSValue</c> into an F# value,
/// guided by the type inferrence of F# <c>'T</c></summary>
/// <param name="x">The <c/>JSValue> that is being projected</param>
/// <return>A value of type <c>'T</c> which is the F# equivalent of <c>x</c></return>
and project<'T> (x: JSValue) : 'T =
    project_hack (typeof<'T>) x |> unbox<'T>
    // let a = project_hack (typeof<'T>) x
    // printf "i'm okay\n"
    // printf "i'm project somthing of type %A\n" (typeof<'T>)
    // unbox<'T> a


and embed_ienumerable (x: IEnumerable) =
    let length = x.GetType().GetProperty("Length").GetValue(x, null) :?> int
    let res = JSEngine.makeArray(context, length)
    let index = ref 0
    for el in x do
        JSEngine.setElementArray(context, res, !index, embed el)
        index := !index + 1
    res

and project_array ty (jarr: JSValue) =
    let length = JSEngine.getArrayLength(context, jarr)
    let farr: JSValue array = Array.zeroCreate length
    JSEngine.extractArray(context, jarr, length, farr)
    // TODO: get rid of float
    Array.map ((project_reflection ty) >> unbox<float>) farr


// TODO: how to specify the type of list I want from a System.Type?
// temporarily using floats just to test it works
and project_list : JSValue -> float list = (project_array typeof<obj>) >> Array.toList
// and project_list<'T> : JSValue -> 'T list = (project_array typeof<'T>) >> Array.toList<'T>

let array =
    { embed = embed_ienumerable ;
      // might need to pass some type information to project
      // TODO get rid of float, is this even useful?
      project = project_array typeof<float> }

let list =
    { embed = embed_ienumerable ;
      project = project_list }


/// <summary>Registers a <c>JSValue</c> in the global object of JavaScript,
/// <param name="l">The list of tuples containing the name and the <c/>JSValue> to be assigned to</param>
/// <return><c>Unit</c></return>
let register_values (l: (string * JSValue) list) =
    let register_value (name, value) =
        JSEngine.registerValue(context, name, value)

    List.iter register_value l


let rec echo p =
    match p with
        | Boolean b -> printf "%b : Boolean\n" b
        | Number n -> printf "%f: Number\n" n
        | String s -> printf "%s: String\n" s
        | Function f ->
            printf "it's a function!\n"
            let n = embed 41.1;
            printf "i've embedded 41.1\n"
            // let n = JSEngine.execute_string(context, "3")
            let result = f [n]
            printf "i've applied the function\n"
            echo result
        | _ -> printf "something else\n"


let main() =
    // let p = JSEngine.execute_string(context, "(function(a) {return a+38.1;})")
    // let add1 : float -> float = project p
    // let r = add1 2.9
    // printf "this is r %f\n" r

    // // don't optimize context out!
    printf "this is context: %A\n" context

    let s = ""
    let js = embed s
    JSEngine.print_result(context, js)
    let fs: string = project js
    printf "This is s: %s\n" s
    printf "this is fs: %s\n" fs
    // let l = [|11.1;22.2;33.3|]
    // let jl = embed l
    // JSEngine.registerValue(context, "jl", jl)
    // let twentytwo = JSEngine.execute_string(context, "jl[1]")
    // JSEngine.print_result(context, twentytwo)

    // let jar = JSEngine.execute_string(context, "[1.1,2.2,3.3]")

    // let ar: float[] = project jar
    // printf "this is length: %f\n" (ar.[2])
    // printf "this should be three.three: %f\n" (ar.[2])

    ////////////////////////////////////
    // let id x = x
    // let jid = embed id
    // let r =
    //     match jid with
    //         | Function f -> f [embed 2.0]
    //         | _ -> failwith ""
    // JSEngine.print_result(context, r)

    // let nums = [1;2;3]
    // let jnums = embed nums
    // JSEngine.print_result(context, jnums)


    // let jadd = JSEngine.execute_string(context, "(function(x,y) {return x+y;})")
    // let add: float->float->float = project jadd
    // let add1 = add 1.0
    // let add2 = add 2.0
    // let three = add1 2.0;
    // let four = add2 2.0;
    // let five = add2 3.0;
    // printf "This should be 3: %f\n" three
    // printf "This should be 4: %f\n" four
    // printf "This should be 5: %f\n" five

    // let three = 3
    // let j3 = embed 6
    // JSEngine.print_result(context, j3)
    // // printf "this is r: %f\n" (project r)

    // let add2 x :float= x + 1.0
    // let jadd = embed add2
    // JSEngine.registerValue(context, "addd", jadd)
    // let r = JSEngine.execute_string(context, "addd(-10.0)")
    // let f:float = project r
    // printf "%f\n" f
    // JSEngine.print_result(context, r)

    // let t = embed true
    // JSEngine.registerValue(context, "tt", t)
    // let r = JSEngine.execute_string(context, "if (tt) {var a = 3.0;} else {var a = 2.0;} a")
    // let one: float = project r
    // printf "This is three: %f\n" one

    printf "Done\n"
do
    main()
