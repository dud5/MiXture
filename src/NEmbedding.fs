module Mixture.NEmbedding

open System.Runtime.InteropServices
open Microsoft.FSharp.Reflection
open System.Collections
open System.Collections.Generic
open System
open System.Reflection
open Microsoft.FSharp.Quotations
open JSUtils

type _JSValue = JSUtils.JSValue

exception JSException of _JSValue
exception ProjectException of string
exception EmbedException of string


let pinned_handles = new Dictionary<_JSValue, GCHandle>()

let free_object x =
  if pinned_handles.ContainsKey(x) then
    pinned_handles.[x].Free(); true
  else false



let (|Boolean|_|) (b:_JSValue) =
  if JSEngine.isBoolean(b) then Some(JSEngine.extractBoolean(b))
  else None

let (|Integer|_|) (n:_JSValue) =
  if JSEngine.isInt32(n) then Some(JSEngine.extractInt(n))
  else None

let (|Number|_|) (n:_JSValue) =
  if JSEngine.isNumber(n) then Some(JSEngine.extractFloat(n))
  else None

let (|String|_|) (s:_JSValue) =
  if JSEngine.isString(s) then Some(get_string s)
  else None

let (|Object|_|) (o:_JSValue) =
  if JSEngine.isObject(o) then Some()
  else None

let (|Function|_|) (f:_JSValue) =
  if JSEngine.isFunction(f) then
    Some(fun (args: _JSValue list) ->
         let mutable is_exception = Unchecked.defaultof<bool>
         let result = JSEngine.apply_function_arr(context, f, List.length args, List.toArray args, &is_exception)
         if is_exception then raise (JSException(result))
         else result
      )
  else None

let (|Null|_|) (x:_JSValue) =
  if JSEngine.isNull(x) then Some(Null)
  else None

let (|Undefined|_|) (x:_JSValue) =
  if JSEngine.isUndefined(x) then Some(Undefined)
  else None

let (|Array|_|) (arr:_JSValue) =
  if JSEngine.isArray(arr) then
    Some()
  else None

let deduce_type = function
  | Boolean _ -> typeof<bool>
  | Integer _ -> typeof<int>
  | Number _ -> typeof<float>
  | String _ -> typeof<string>
  | _ -> failwith "!"



// let (++) (func:nativeint) (args: nativeint list) =
//   match func with
//     | Function f -> f args
//     | _ -> failwith "cannot apply a non-function"


// Embed-project pair
type ('a, 'b) ep = { embed : 'a -> 'b; project: 'b -> 'a }

// Embed & project strategies for the different "basic" types
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
    { embed = fun n -> JSEngine.makeFloat(JSUtils.context, Utils.to_float n);
      project = function
          | Integer n ->
              // printf "USing Integer %d\n" n
              n
          | Number x ->
              if x = Utils.to_float (int x) then (int x) else failwith "tried to project a float or it's outside the valid range"
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
    { embed  = fun () -> JSEngine.makeUndefined() ;
      project = function
          | Undefined -> ()
          | _ -> failwith "tried to project a not undefined" }


// let jnull =
//     { embed = fun () -> JSEngine.makeNull() ;
//       project = function
//           | Null -> null
//           | _ -> failwith "tried to project a not null" }



/// <summary>Embeds a value into an <c>JSvalue</c>, using type information provided
/// <param name="ty">The <c>Type</c> value that specifies the type of the value being embedded</param>
/// <param name="x">The value that is being embedded</param>
/// <return>A value of type <c>_JSValue</c> which is the JavaScript equivalent of <c>x</c></return>
let rec embed_reflection (x:obj) =
    let ty = x.GetType()
    if ty = typeof<string> then string.embed(x:?>string)
    elif ty = typeof<float> then float.embed(x:?>float)
    elif ty = typeof<int> then int.embed(x:?>int)
    elif ty = typeof<bool> then boolean.embed(x:?>bool)
    elif ty.IsGenericType && ty.GetGenericTypeDefinition() = typedefof<list<_>> then embed_ienumerable(x :?> IEnumerable)
    elif ty.IsArray then embed_ienumerable(x :?> IEnumerable)
    elif (FSharpType.IsFunction ty) then
        embed_func(x)

    elif (FSharpType.IsExceptionRepresentation ty) then
        let fields = FSharpValue.GetExceptionFields x
        let js_fields = embed fields
        JSEngine.makeException(JSUtils.context, js_fields)

    elif ty = typeof<System.Reflection.TargetInvocationException> then
        embed (x :?> System.Reflection.TargetInvocationException).InnerException

    elif (FSharpType.IsRecord ty) then
        embed_record x
    else
        let error_message = sprintf "Can't embed values of type %A" ty
        raise (EmbedException(error_message))


/// <summary>Embeds a value into an <c>JSvalue</c>
/// <param name="x">The value that is being embedded</param>
/// <return>A value of type <c>_JSValue</c> which is the JavaScript equivalent of <c>x</c></return>
// this fails with null (gettype)
and embed (x:obj) : _JSValue =
    if box x = null then unit.embed()
    else embed_reflection x

/// <summary>Projects a <c>_JSValue</c> into an F# value, using type information provided</summary>
/// <param name="ty">The <c>Type</c> value that specifies what type the F# should have
/// <param name="x">The <c>_JSValue</c> that is being projected
/// <return>A value of type <c>ty</c> which is the F# equivalent of <c>x</c></return>
and project_hack ty (x:obj) : obj =
    match x with
        | :? _JSValue as jx -> project_reflection ty jx
        | :? (_JSValue list -> _JSValue) as f ->
            if FSharpType.IsFunction (x.GetType()) && FSharpType.IsFunction ty then
                project_func ty f
            else failwith "function nonfunction"
        | _ -> failwith "faillllllll"


and project_reflection ty (x:_JSValue) : obj =
    // if JSEngine.isException(x) then raise JSException
    if ty = typeof<string> then string.project(x) |> box
    elif ty = typeof<float> then float.project(x) |> box
    elif ty = typeof<int> then int.project(x) |> box
    elif ty = typeof<bool> then boolean.project(x) |> box
    elif ty = typeof<unit> then null
    elif (FSharpType.IsFunction ty) then
        match x with
            | Function f -> project_func ty f
            | _ -> failwith "trying to project a nonfunction"

    elif ty.IsGenericType && ty.GetGenericTypeDefinition() = typedefof<list<_>> then
        let el_ty = ty.GetGenericArguments().[0]
        (project_list el_ty x) |> box

    elif ty.IsArray then
        // GetMethod.MageGeneric.Invoke
        let el_ty = ty.GetElementType()
        (project_array el_ty x) |> box

    elif (FSharpType.IsRecord ty) then
        project_record ty x

    elif ty = typeof<obj> then
        // we don't know what we want, let's see what we can get
        match x with
            | Boolean b -> project_reflection typeof<bool> x
            | Integer n -> project_reflection typeof<int> x
            | Number n -> project_reflection typeof<float> x
            | String s -> project_reflection typeof<string> x
            | Array -> project_reflection typeof<obj[]> x
            | Undefined -> null
            // | Function f ->
            | _ -> failwith ""
    else
//        JSEngine.print_result(JSUtils.context, x)
        let error_message = sprintf "Can't project to type %A." ty
        raise (ProjectException(error_message))


/// <summary>Projects a <c>_JSValue</c> into an F# value,
/// guided by the type inferrence of F# <c>'T</c></summary>
/// <param name="x">The <c/>_JSValue> that is being projected</param>
/// <return>A value of type <c>'T</c> which is the F# equivalent of <c>x</c></return>
and project<'T> (x: _JSValue) : 'T =
    let ty = typeof<'T>
    
    project_hack ty x |> unbox<'T>

and embed_func (x:obj) =
    // f is a function _JSValue -> _JSValue, which takes a JavaScript
    // Arguments object and returns the embedded result
    let f (args: _JSValue) =
        let arg = args |> JSUtils.get_all_JS_arguments JSUtils.context
        let domain = FSharpType.GetFunctionElements (x.GetType()) |> fst
        let projected_args =
            // if the function expects a tuple, process the array args
            // through project and build the appropriate tuple
            if FSharpType.IsTuple domain then
                let proj_args_array = Array.map2 project_reflection (FSharpType.GetTupleElements domain) arg
                [| FSharpValue.MakeTuple(proj_args_array, domain) |]
            else [| project_reflection domain (arg.[0]) |]

        try
            embed (Utils.call_object_function x projected_args domain)
        with
            | exn -> JSEngine.throwException(JSUtils.context, embed exn)

    let callback = new JSEngine.FSharpFunction(f)
    let gch = GCHandle.Alloc(callback)
    let result = JSEngine.makeFunction(JSUtils.context, callback)
    pinned_handles.[result] <- gch
    result

and embed_poly_func (e:Expr) =
    let ty_variables, domain, range, mi = Utils.create_signature e
    let f (args: _JSValue) =
        let arg = args |> JSUtils.get_all_JS_arguments JSUtils.context
        let proj_args_array = Array.map (project_reflection (typeof<obj>)) arg
        let deduced_types = Array.map deduce_type arg
        match (Utils.unify_types(domain, deduced_types, ty_variables)) with
            | Some(specialized_types) ->
                try
                    embed (Utils.call_generic_method_info mi specialized_types proj_args_array)
                with
                    | exn -> JSEngine.throwException(JSUtils.context, embed exn)
            | None ->failwith "Types not compatible"

    let callback = new JSEngine.FSharpFunction(f)
    let gch = GCHandle.Alloc(callback)
    JSEngine.makeFunction(JSUtils.context, callback)


and project_func ty (f:_JSValue list -> _JSValue) : obj =
    let range = FSharpType.GetFunctionElements ty |> snd
    if FSharpType.IsFunction range then
        FSharpValue.MakeFunction(ty, fun arg -> project_hack range (fun t -> f (embed arg :: t)))
    else
        FSharpValue.MakeFunction(ty, fun arg -> project_hack range ( f [embed arg]))

// and project_poly_func<'T> (x: _JSValue) : 'T =
//     match x with
//         | Function f -> failwith "!"
//         | _ -> failwith "Not a function!"


and embed_ienumerable (x: IEnumerable) =
    let length = x.GetType().GetProperty("Length").GetValue(x, null) :?> int
    let res = JSEngine.makeArray(JSUtils.context, length)
    let index = ref 0
    for el in x do
        JSEngine.setElementArray(JSUtils.context, res, !index, embed el)
        index := !index + 1
    res

and project_array ty (jarr: _JSValue) =
    let length = JSEngine.getArrayLength(JSUtils.context, jarr)
    let farr = JSUtils.extract_array jarr length
    let result = System.Array.CreateInstance(ty, length)
    Array.iteri (fun index el -> result.SetValue(project_hack ty el, index)) farr
    result

// TODO: how to specify the type of list I want from a System.Type?
// temporarily using floats just to test it works
// and project_list : _JSValue -> float list = (project_array typeof<obj>) >> Array.toList
// and project_list ty = (project_array ty ) >> Array.toList

and project_list (ty: System.Type) (jlist: _JSValue)  =
    let arr = project_array ty jlist
    let li = typedefof<List<_>>.MakeGenericType([|ty|]).GetConstructors().[0].Invoke([||])
    let addmi = li.GetType().GetMethod("Add", [|ty|])
    for e in arr do
        addmi.Invoke(li, [|e|]) |> ignore
    Utils.call_generic_method_info Utils.listofseq [|ty|] [|li|]


// and project_list<'T> (jlist: _JSValue): 'T list =
//   ((project_array (typeof<'T>) jlist) :?> 'T[]) |> Array.toList<'T>


and embed_record r =
    let result = JSUtils.makeObjectLiteral(JSUtils.context)
    // let (field_names, field_types, field_values) = record_to_string r
    // let's see if we can do it without the types
    let field_names, field_values = Utils.get_field_names (r.GetType()), Utils.get_field_values r
    let field_writable = Utils.get_field_writable (r.GetType())
    let field_values_embedded = Array.map embed field_values
    let name_type_writable = Array.zip3 field_names field_values_embedded field_writable
    // set properties on result with field_names as names and field_values as values
    Array.iter (JSUtils.setProperty result) name_type_writable
    result

/// <summary>Helper function for <c>project_record</c>. Creates an array of the values required to construct a
/// record of type <c>ty</c>.</summary>
/// <param name="ty">The F# <c>System.Type</c> of the record to be projected to</param>
/// <param name="x">The JavaScript object that is being projected</param>
/// <param name="r_field_names">Array of the names of the fields of the type <c>ty</c></param>
/// <return>An object array used to create a record of type <c>ty</c> using <c>FSharpValue.MakeRecord</c></return>
and project_object_properties ty x (r_field_names: string[]) =
    // project each value from x to the appropriate type according to r_field_types
    let j_field_values: nativeint array = Array.zeroCreate r_field_names.Length
    // TODO: shouldn't need to embed the names, try passing a string array?
    JSEngine.getProperties(JSUtils.context, x, embed r_field_names, r_field_names.Length, j_field_values)
    let r_field_types = Utils.get_field_types ty
    let r_values =
        try
            Array.map2 (fun value_type value -> project_hack value_type value) r_field_types j_field_values
        with
            exn -> failwith "3Project fail; the names of the JavaScript object and the F# record fields don't match"
    r_values

/// <summary>Projects a JavaScript value into an F# record specified by <c>ty</c></summary>
/// <param name="ty">The record type being used to project x</param>
/// <param name="x">The object that is being projected</param>
/// <return>A value of type <c>'T</c> which is the F# equivalent of <c>x</c></return>
and project_record ty (x:_JSValue) =
    let r_field_names = Utils.get_field_names ty
    let j_field_names: string[] = JSEngine.getOwnPropertyNames(JSUtils.context, x) |> project
    let length = r_field_names.Length
    if r_field_names.Length <> j_field_names.Length
    then
        printf "this is r_field_names: %d\n" r_field_names.Length
        printf "this is j_field_names: %d\n" j_field_names.Length
        failwith "1Project fail; the names of the JavaScript object and the F# record fields don't match"
    else
        // check that both names lists are a permutation of each other
        if (not (Utils.is_permutation r_field_names j_field_names))
        then failwith "2Project fail; the names of the JavaScript object and the F# record fields don't match"
        else
            let r_values = project_object_properties ty x r_field_names
            // this shouldn't fail because the value have been projected as directed by ty
            try
                FSharpValue.MakeRecord(ty, r_values)
            with
                exn -> failwith "Projection into a record failed when creating the record"

let array ty =
    { embed = embed_ienumerable ;
      // might need to pass some type information to project
      // TODO get rid of float, is this even useful?
      project = project_array ty }

// let list ty=
//     { embed = embed_ienumerable ;
//       project = project_list ty}

// let func =
//     { embed = fst << embed_func ;
//       // don't need project
//       project = fun x r -> failwith "can't project a fnc" }
//       // project = project_func }

/// <summary>Registers a <c>_JSValue</c> in the global object of JavaScript</summary>
/// <param name="l">The list of tuples containing the name and the <c/>_JSValue> to be assigned to</param>
/// <return><c>Unit</c></return>
let register_values (l: (string * _JSValue) list) =
    let register_value (name, value) =
        JSEngine.registerValue(JSUtils.context, name, value)
    List.iter register_value l







type JSValue2(pointer: nativeint) =
    let mutable disposed = false
    let cleanup disposing =
        printf "cleanup!\n"
        if not disposed then
            disposed <- true
            if disposing then
                printf "disposing is true!\n"
                let clean () =
                    free_object pointer |> ignore
                    printf "calling clean() from c++ most likely\n"
                    ()
                let cb = new JSEngine.VoidFSharpFunction(clean)
                JSEngine.makeWeak(pointer, cb)
                ()
    member this.Pointer = pointer
    static member ( *@ ) (func: JSValue2, args: JSValue2 list) =
        match func.Pointer with
            | Function f ->
              let pointer_args = List.map (fun (el:JSValue2) -> el.Pointer) args
              new JSValue2(f pointer_args)
            | _ -> failwith "cannot apply a non-function"

    static member ( +> ) (o: JSValue2, property: string) =
        match o.Pointer with
            | Object ->
              let r = JSEngine.getProperty(JSUtils.context, o.Pointer, property)
              if (JSEngine.isUndefined(r)) then raise (JSException(o.Pointer))
              else new JSValue2(r)
            | _ -> raise (JSException(embed  "Property non-existing"))


    interface IDisposable with
        member this.Dispose() =
            cleanup(true)
            GC.SuppressFinalize(this)







let public_embed (x:obj) : JSValue2 =
    new JSValue2(embed x)

let public_project<'T> (x:JSValue2) : 'T =
    project<'T> (x.Pointer)

let public_execute_string s=
    new JSValue2 (JSUtils.execute_string s)