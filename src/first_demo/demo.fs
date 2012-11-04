open System.Runtime.InteropServices
open System

type Lump = System.IntPtr

module V8 =
        [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
        extern Lump create_function(IntPtr, string, string)
        [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
        extern void dispose_handle(IntPtr)
        [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
        extern IntPtr apply_function(IntPtr, IntPtr, IntPtr)
        [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
        extern IntPtr execute_string(IntPtr, string)
        [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
        extern void print_result(IntPtr, IntPtr)
        [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
        extern void print_string(string)
        [<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
        extern IntPtr get_context()


let context = V8.get_context();

// V8.print_string("hello from f#");

let num = V8.execute_string(context, "3")

let name = "add"
let add = V8.create_function(context, "var add = function(arg) {return arg+1;};", "add")

printf "Calling function: %s\n" name;
printf "Called with argument: ";
V8.print_result(context, num);


let result = V8.apply_function(context, add, num)

printf "This is the result: ";
V8.print_result(context, result);

V8.dispose_handle(num);
V8.dispose_handle(add);
V8.dispose_handle(result);


V8.dispose_handle(context);
