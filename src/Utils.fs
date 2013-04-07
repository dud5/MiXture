module Mixture.Utils

open Microsoft.FSharp.Quotations
open System.Reflection
open Microsoft.FSharp.Reflection
// open Microsoft.FSharp.Linq.QuotationEvaluation
open System.Collections.Generic
open System

let is_permutation l1 l2 =
    Array.sort l1 = Array.sort l2

let get_field_names ty =
    let record_fields = FSharpType.GetRecordFields ty
    Array.map (fun (x: Reflection.PropertyInfo) -> x.Name) record_fields

let get_field_types ty =
    let record_fields = FSharpType.GetRecordFields ty
    Array.map (fun (x: Reflection.PropertyInfo) -> x.PropertyType) record_fields

let get_field_writable ty =
    let record_fields = FSharpType.GetRecordFields ty
    Array.map (fun (x: Reflection.PropertyInfo) -> x.CanWrite) record_fields

let get_field_values r =
    let record_fields = FSharpType.GetRecordFields (r.GetType())
    Array.map (fun (x: Reflection.PropertyInfo) -> x.GetValue(r, null)) record_fields

// float is an F#: int -> float
let to_float = float
// int is an F#: float -> int
let to_int = int

let call_object_function_arr f args (domain: System.Type[])  =
    f.GetType().GetMethod("Invoke", domain).Invoke(f, args)


let call_object_function f args domain  =
    call_object_function_arr f args [|domain|]

let call_generic_method_info (mi: MethodInfo) specialized_types args =
    let gmi = mi.MakeGenericMethod(specialized_types)
    gmi.Invoke((), args)

// unify types, where ty_param is a list of possibly parametric types
// and ty_spec is a list of specific types
let unify_types (ty_param: System.Type[], ty_spec: System.Type[], unique_ty: System.Type[]) =
    if ty_param.Length <> ty_spec.Length
    then None
    else
        let unify_types_dict = new Dictionary<System.Type, System.Type>()
        let unified = Array.forall2 (fun param_type deduced_type ->
                     // if same, succeed
                     if param_type = deduced_type then true
                     elif unify_types_dict.ContainsKey(param_type) then
                         // if same type in dictionary, succeed
                         if unify_types_dict.[param_type] = deduced_type then true
                         // if different value, unification fails
                         else false
                     // if the type is not in the dictionary, insert it and succeed
                     else
                        unify_types_dict.[param_type] <- deduced_type
                        true
                         ) ty_param ty_spec
        if unified then
            let specialized_types = Array.map (fun el -> unify_types_dict.[el]) unique_ty
            Some(specialized_types)
        else
            None

let create_signature (e:Expr) =
    let gmi, args =
        match e with
            | DerivedPatterns.Lambdas(_,Patterns.Call(_,mi,args)) -> mi.GetGenericMethodDefinition(), args
            | _ -> failwith "!"
    let arg_types = gmi.GetParameters() |> Array.map (fun (el:ParameterInfo) -> el.ParameterType)
    let ret_types = gmi.ReturnType
    let unique_ty_domain = gmi.GetGenericArguments()
    if FSharpType.IsTuple(ret_types) then
        (unique_ty_domain, arg_types, FSharpType.GetTupleElements(gmi.ReturnType), gmi)
    else (unique_ty_domain, arg_types, [|ret_types|], gmi)


let (_,_,_,listofseq) = create_signature <@List.ofSeq@>
