module Mixture.NEmbedding

open System.Runtime.InteropServices
open Microsoft.FSharp.Reflection
open System.Collections
open System

type JSValue = nativeint

let mutable context = nativeint(-1)

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
    let length = JSEngine.stringLength(s)
    let sb = new System.Text.StringBuilder(length)
    JSEngine.extractString(s, sb, length)
    sb.ToString()

let (|String|_|) (s:JSValue) =
    if JSEngine.isString(s) then Some(getString s)
    else None

let (|Function|_|) (f:JSValue) =
    if JSEngine.isFunction(f) then
        Some(fun (args: JSValue list) -> JSEngine.apply_function_arr(JSUtils.context, f, List.length args, List.toArray args))
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
// JAVASCRIPT HAS INFINITY; ADD THIS!!!!
let float =
    { embed = fun x ->
          if System.Double.IsPositiveInfinity x then
              JSEngine.makeInfinity(JSUtils.context, false)
          elif System.Double.IsNegativeInfinity x then
              JSEngine.makeInfinity(JSUtils.context, true)
          else JSEngine.makeFloat(JSUtils.context, x) ;

      project = function Number x -> x | _ -> failwith "tried to project a nonfloat" }


let string =
    { embed = fun s -> JSEngine.makeString(JSUtils.context, s);
      project = function String s -> s | _ -> failwith "tried to project a nonstring" }


let int =
    { embed = fun n -> JSEngine.makeFloat(JSUtils.context, to_float n);
      project = function
          | Integer n ->
              // printf "USing Integer %d\n" n
              n
          | Number x ->
              if x = to_float (int x) then (int x) else failwith "tried to project a float or it's outside the valid range"
          | _ -> failwith "tried to project a non int" }

let boolean =
    { embed = fun (b: bool) -> JSEngine.makeBoolean(JSUtils.context, b) ;
      project = function
          | Boolean b -> b
          | String s -> s <> ""
          | Number x -> x <> 0.0
          | Null -> false
          | Undefined -> false
          | _ -> true }

let unit =
    { embed  = fun () ->JSEngine.makeUndefined() ;
      project = function
          | Undefined -> ()
          | _ -> failwith "tried to project a not undefined" }


// let jnull =
//     { embed () = JSEngine.makeNull() ;
//       project = function
//           | Null -> null
//           | _ -> failwith "tried to project a not null" }
        

let func =
    { embed = fun (f: JSValue -> JSValue) ->
          let nativef (args: JSValue) =
              let processed_args = args |> JSUtils.get_all_JS_arguments JSUtils.context
              f (processed_args.[0])
          JSEngine.makeFunction(JSUtils.context, new JSEngine.FSharpFunction(nativef)) ;

      // don't need project
      project = fun r x -> failwith "Cannot call project for 'func'" }

// let generate_JS_object x =
//     let fieldNames = FSharpType.GetRecordFields (o.GetType());
//     let fieldValues = FSharpValue.GetRecordFields o;
//     let source = new StringBuilder()
//     source.append("{")
    
    

    
// let record =
//     { embed = fun (o:obj) ->
//           let fieldNames = FSharpType.GetRecordFields (o.GetType());
//           let fieldValues = FSharpValue.GetRecordFields o;
      
//       project }



/// <summary>Embeds a value into an <c>JSvalue</c>, using type information provided
/// <param name="ty">The <c>Type</c> value that specifies the type of the value being embedded</param>
/// <param name="x">The value that is being embedded</param>
/// <return>A value of type <c>JSValue</c> which is the JavaScript equivalent of <c>x</c></return>
let rec embed_reflection ty (x:obj) =
    if ty = typeof<string> then string.embed(x:?>string)
    elif ty = typeof<float> then float.embed(x:?>float)
    elif ty = typeof<int> then int.embed(x:?>int)
    elif ty = typeof<bool> then boolean.embed(x:?>bool)
    elif ty.IsGenericType && ty.GetGenericTypeDefinition() = typedefof<list<_>> then embed_ienumerable(x :?> IEnumerable)
    elif ty.IsArray then embed_ienumerable(x :?> IEnumerable)
    elif (FSharpType.IsFunction ty) then
        let domain = FSharpType.GetFunctionElements ty |> fst
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
// this fails with null (gettype)
and embed x : JSValue =
    if box x = null then unit.embed()
    else embed_reflection (x.GetType()) x

// and embed (x:'T) : JSValue = embed_reflection (typeof<'T>) x

and project_func ty (f:JSValue list -> JSValue) : obj =
    let range = FSharpType.GetFunctionElements ty |> snd
    if FSharpType.IsFunction range then
        FSharpValue.MakeFunction(ty, fun arg -> project_hack range (fun t -> f (embed arg :: t)))
    else
        FSharpValue.MakeFunction(ty, fun arg -> project_hack range ([embed arg] |> f))

/// <summary>Projects a <c>JSValue</c> into an F# value, using type information provided</summary>
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

    elif ty.IsGenericType && ty.GetGenericTypeDefinition() = typedefof<list<_>> then
        let el_ty = ty.GetGenericArguments().[0]
        box <| project_list el_ty x
        
    elif ty.IsArray then
        // GetMethod.MageGeneric.Invoke
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
        JSEngine.print_result(JSUtils.context, x)
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
    let res = JSEngine.makeArray(JSUtils.context, length)
    let index = ref 0
    for el in x do
        JSEngine.setElementArray(JSUtils.context, res, !index, embed el)
        index := !index + 1
    res

and project_array ty (jarr: JSValue) =
    let length = JSEngine.getArrayLength(JSUtils.context, jarr)
    let farr: JSValue array = Array.zeroCreate length
    JSEngine.extractArray(JSUtils.context, jarr, length, farr)
    let result = System.Array.CreateInstance(ty, length)
    Array.iteri (fun index el -> result.SetValue(project_hack ty el, index)) farr
    result


// TODO: how to specify the type of list I want from a System.Type?
// temporarily using floats just to test it works
// and project_list : JSValue -> float list = (project_array typeof<obj>) >> Array.toList
// and project_list ty = (project_array ty ) >> Array.toList
and project_list ty : JSValue -> float list = fun x -> [1.1]
// and project_list<'T> : JSValue -> 'T list = (project_array typeof<'T>) >> Array.toList<'T>

let array ty =
    { embed = embed_ienumerable ;
      // might need to pass some type information to project
      // TODO get rid of float, is this even useful?
      project = project_array ty }

let list ty=
    { embed = embed_ienumerable ;
      project = project_list ty}


/// <summary>Registers a <c>JSValue</c> in the global object of JavaScript</summary>
/// <param name="l">The list of tuples containing the name and the <c/>JSValue> to be assigned to</param>
/// <return><c>Unit</c></return>
let register_values (l: (string * JSValue) list) =
    let register_value (name, value) =
        JSEngine.registerValue(JSUtils.context, name, value)
    List.iter register_value l


// let main() =

//     printf "Done\n"
// do
//     main()

// let a ty (x:obj) =
//     if ty = typeof<string> then 1
//     elif ty = typeof<float> then 2
//     elif ty = typeof<int> then 3
//     elif (x = null) then 4
//     else 10

// let b x =
//     if box x = null then -1
//     else a (x.GetType()) x
