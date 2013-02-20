module Mixture.JSUtils

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
    if context = nativeint(-1) then
        create_context() |> ignore
        JSEngine.execute_string(context, s)
    else
        JSEngine.execute_string(context, s)

/// <summary>Loads a JavaScript file and executes it, returning its value</summary>
let load_file =
    System.IO.File.ReadAllText >> execute_string

let setProperty (o: nativeint) (name: string) (value: nativeint) =
    JSEngine.setProperty(context, o, name, value)

let makeObjectLiteral = JSEngine.makeObjectLiteral
