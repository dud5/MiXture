module JSEngine

open System.Runtime.InteropServices

// Represents an F# function that will be accessible from JavaScript
type FSharpFunction = delegate of nativeint -> nativeint

///////
/////// GENERAL UTILS
///////

// for passing arrays, see
// http://stackoverflow.com/questions/12622160/from-c-sharp-to-f-pinned-array-and-stringbuilder-in-external-function-called-f
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint apply_function_arr(nativeint, nativeint, int, nativeint[], [<Out>] bool& is_ex)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint execute_string(nativeint, string, [<Out>] bool& is_ex)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern void print_result(nativeint, nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint createContext()
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern void disposeHandle(nativeint)

// return a nativeint pointing to a JavaScript value that is an argument
// in the arguments object (the third argument specifies the index)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint get_argument(nativeint, nativeint, int)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern int arguments_length(nativeint, nativeint)
// insert into the global object in JavaScript a value set to the name
// of the second argument with a value corresponding to the pointer in
// the third argument
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern void registerValue(nativeint, string, nativeint)

///////
/////// Lump Embedding
///////
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint make_FLump(nativeint, nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern int get_pointer_lump(nativeint)
// TODO: delete me (use registerValue instead
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern void register_function(nativeint, FSharpFunction)

///////
/////// Natural Embedding
///////
// Obtain the F# representation of each type
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern float extractFloat(nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern int extractInt(nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
// see http://stackoverflow.com/questions/5298268/returning-a-string-from-pinvoke
// for how to return strings from unmanaged code
extern void extractString(nativeint, System.Text.StringBuilder, int)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern bool extractBoolean(nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern int stringLength(nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern bool strictComparison(nativeint, nativeint, nativeint)




// Make JavaScript values representing the F# values of each type
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint makeFloat(nativeint, double)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint makeInfinity(nativeint, bool)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint makeString(nativeint, string)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint makeBoolean(nativeint, bool)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint makeUndefined()
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint makeNull()
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint makeFunction(nativeint, FSharpFunction)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint makeArray(nativeint, int)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern void setElementArray(nativeint, nativeint, int, nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint getElementArray(nativeint, nativeint, int)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern int getArrayLength(nativeint, nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern void extractArray(nativeint, nativeint, int, nativeint[])
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint makeException(nativeint, nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint throwException(nativeint, nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint makeObjectLiteral(nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern void setProperty(nativeint, nativeint, string, nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern nativeint getOwnPropertyNames(nativeint, nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern void getProperties(nativeint, nativeint, nativeint, int, nativeint[])



// Check the JavaScript types of pointers
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern bool isBoolean(nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern bool isNumber(nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern bool isInt32(nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern bool isUint32(nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern bool isString(nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern bool isFunction(nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern bool isNull(nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern bool isUndefined(nativeint)
[<DllImport("v8_helper.dylib", CallingConvention=CallingConvention.Cdecl)>]
extern bool isArray(nativeint)





// Dummies for fsi
// module JSEngine =
//     let makeFloat(x,y) = nativeint(1)
//     let makeString(a,b) = nativeint(3)
//     let makeBoolean(a,b) =nativeint(1)
//     let makeFunction(a,b) = nativeint(11)
//     let extractFloat(o:JSValue) = 1.2
//     let extractString(o:JSValue, a:System.Text.StringBuilder) = let l = a.Append("hello") in ()
//     let extractBoolean(b:JSValue)= true
//     let extractInt(n:JSValue) = 2
//     let createContext () = nativeint(2)
//     let apply_function_arr(a,b,c,d) = nativeint(3)
//     let isBoolean x = true
//     let isString x = true
//     let isNumber x = true
//     let isFunction x = true
//     let isInt32 x = true
//     let isNull x = true
//     let isUndefined x = true
//     let isArray x = true
//     let print_result(a,b) = ()


