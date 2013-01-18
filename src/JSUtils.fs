module Mixture.JSUtils

let get_JS_argument context args index =
    JSEngine.get_argument(context, args, index)

// is there an easier way of doing this in C++ with V8?
let get_all_JS_arguments context args =
    let size = JSEngine.arguments_length(context, args)
    let init = get_JS_argument context args
    Array.init size init
