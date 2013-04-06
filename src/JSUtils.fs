module Mixture.JSUtils

type JSValue = nativeint
exception JSException of JSValue

let mutable context = nativeint(-1)

let create_context () =
    let c = JSEngine.createContext()
    context <- c
    c
let set_current_context c =
    context <- c

let get_JS_argument args index =
    JSEngine.get_argument(context, args, index)

let get_all_JS_arguments context args =
    let size = JSEngine.arguments_length(context, args)
    let init = get_JS_argument args
    Array.init size init

let execute_string s =
    let mutable is_exception = false
    let result = JSEngine.execute_string(context, s, &is_exception)
    if is_exception then raise (JSException(result))
    else result

/// <summary>Loads a JavaScript file and executes it, returning its value</summary>
let load_file =
    System.IO.File.ReadAllText >> execute_string

let setProperty (o: nativeint) (name: string, value: nativeint, writable: bool)=
    JSEngine.setProperty(context, o, name, value, writable)

let makeObjectLiteral = JSEngine.makeObjectLiteral

let extract_array jarr length =
    let farr: nativeint array = Array.zeroCreate length
    JSEngine.extractArray(context, jarr, length, farr)
    farr

let get_string (s:nativeint) =
    let length = JSEngine.stringLength(s)
    let sb = new System.Text.StringBuilder(length)
    JSEngine.extractString(s, sb, length)
    sb.ToString()


create_context () |> ignore
