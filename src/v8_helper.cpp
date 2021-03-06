#include <iostream>
#include <limits>
#include <v8.h>
#include <string.h>

using namespace v8;
using namespace std;
typedef void (*CALLBACK)(Persistent<Value>);

extern "C" void disposeHandle(Persistent<Value> object, void* parameter) {
//    cout << "Hello from disposeHandle c++" << endl;
    CALLBACK cleanup = reinterpret_cast<CALLBACK>(reinterpret_cast<long>(parameter));
//    cout << "After the cast" << endl;
	(cleanup)(object);
//    cout << "After the call" << endl;
    object.Dispose();
//    cout << "After the dipose" << endl;
    object.Clear();
//    cout << "After the clear" << endl;
}


extern "C" void makeWeak(Persistent<Value> prst, CALLBACK* cleanup) {
    prst.MakeWeak(cleanup, disposeHandle);
//    cout << "Hello from makeWeak c++" << endl;
}

extern "C" void forceGC() {
	while (!v8::V8::IdleNotification()); 
}


extern "C" Persistent<Context> createContext() {
    return Context::New();
}

extern "C" void test(WeakReferenceCallback* cleanup) {
    Persistent<Context> c = createContext();
    HandleScope handle_scope;
    Persistent<Object> o = Persistent<Object>::New(Object::New());
    o.MakeWeak(cleanup, disposeHandle);
}

// Extracts a C string from a V8 Utf8Value.
const char* ToCString(const v8::String::Utf8Value& value) {
    return *value ? *value : "<string conversion failed>";
}


extern "C" void print_result(Persistent<Context> context, Handle<Value> val) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    String::Utf8Value str(val);
    cout << *str << endl;
}

extern "C" Handle<Value> execute_string(Persistent<Context> context,
					const char* s,
					bool* is_exception) {
    // Create a stack-allocated handle scope.
    HandleScope handle_scope;

    // Enter the created context for compiling and
    // running the hello world script.
    Context::Scope context_scope(context);

    // Create a string containing the JavaScript source code.
    Handle<String> source = String::New(s);

    // Compile it
    Handle<Script> script = Script::Compile(source);

    // try-catch handler
    TryCatch trycatch;
    // Run it
    Persistent<Value> result = Persistent<Value>::New(script->Run());

    // Script->Run() returns an empty handle if the code threw an exception
    if (result.IsEmpty()) {
	*is_exception = true;
	Handle<Value> exception = trycatch.Exception();
	// String::AsciiValue exception_str(exception);
	return Persistent<Value>::New(exception);	
    }
    
    return result;
}



extern "C" Handle<Value> create_function(Persistent<Context> context,
                                         char* source_s,
                                         char* name) {

    // Create a stack-allocated handle scope.
    HandleScope handle_scope;

    Context::Scope context_scope(context);

    // Create a string containing the JavaScript source code.
    Handle<String> source = String::New(source_s);
    // Compile the source code.
    Handle<Script> script = Script::Compile(source);

    // Run the script to get the result.
    Handle<Value> result = script->Run();

    // magic
    Handle<Object> global = context->Global();
    Handle<Value> value = global->Get(String::New(name));

    Handle<Function> func = v8::Handle<Function>::Cast(value);
    func->SetName(String::New(name));

    Persistent<Function> persistent_func = Persistent<Function>::New(func);
    return persistent_func;
}


extern "C" Handle<Value> apply_function(Persistent<Context> context,
                                        Handle<Function> func,
                                        Handle<Value> arg1) {
    HandleScope handle_scope;

    Context::Scope context_scope(context);

    Handle<Value> result = func->Call(context->Global(), 1, &arg1);

    Persistent<Value> js_result = Persistent<Value>::New(result);

    return js_result;
}

extern "C" Handle<Value> apply_function_arr(Handle<Context> context,
					    Handle<Function> func, int argc,
					    Handle<Value>* argv, bool* is_exception) {
    HandleScope handle_scope;

    Context::Scope context_scope(context);

    TryCatch trycatch;
    
    Handle<Value> result = func->Call(context->Global(), argc, argv);

    if (result.IsEmpty()) {
	*is_exception = true;
	Handle<Value> exception = trycatch.Exception();
	// String::AsciiValue exception_str(exception);
	return Persistent<Value>::New(exception);
    }

    Persistent<Value> js_result = Persistent<Value>::New(result);

    return js_result;
}



// v8::Handle<v8::Value> guard_function(CALLBACK function, Contract ctc) {
    
// }

extern "C" void register_function(Persistent<Context> context, InvocationCallback cb) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);

    Handle<Object> global = context->Global();
    
    global->Set(String::New("apply_fsharp"), FunctionTemplate::New(cb)->GetFunction());
}


extern "C" int arguments_length(Persistent<Context> context, const Arguments& args) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    return args.Length();
}

extern "C" Handle<Value> get_argument(Persistent<Context> context, const Arguments& args, int index) {
    HandleScope handle_scope;

    Context::Scope context_scope(context);

    Handle<Value> local_result = args[index];

    Persistent<Value> result = Persistent<Value>::New(local_result);
    return result;    
}

extern "C" Handle<Value> make_FLump(Persistent<Context> context, int pointer) {
    bool* is_ex = false;
    execute_string(context, "function FLump(pointer) { this.pointer = pointer; }", is_ex);
    const char* s = "new FLump(%d);";
    char script[100];
    int n = sprintf(script, s, pointer);
    if (n < 0) {
	cerr << "something went wrong when creating an FLump in JavaScript" << endl;
	return Undefined();
    }
    return execute_string(context, script, is_ex);
}


/*
 * Returns -1 if lump is not an FLump object
 * otherwise it returns the Pointer attribute in lump
 *
 */
extern "C" int get_pointer_lump(Handle<Value> lump) {
    int result = -1;
    if (lump->IsObject()) {
	Handle<Object> lump_as_object = Handle<Object>::Cast(lump);
	Handle<Value> pointer = lump_as_object->Get(String::New("pointer"));
	if (!pointer.IsEmpty()) {
	    result = pointer->Uint32Value();
	}	
    }
    return result;
}


// ______________________________________________________________
// NATURAL EMBEDDING
// ______________________________________________________________


extern "C" double extractFloat(Handle<Number> x) {
    return x->Value();
}

extern "C" int extractInt(Handle<Int32> n) {
    return n->Value();
}

extern "C" int stringLength(Handle<String> s) {
    return s->Length();
}

extern "C" void extractString(Handle<String> s, char* dest) {
    String::Utf8Value str(s);
    strncpy(dest, ToCString(str), s->Length());
    dest[s->Length()] = 0;
}

extern "C" bool extractBoolean(Handle<Boolean> b) {
    return b->Value();
}

extern "C" Handle<Value> makeFloat(Persistent<Context> context, double x) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    Persistent<Value> result = Persistent<Value>::New(Number::New(x));
    return result;
}

extern "C" Handle<Value> makeInfinity(Persistent<Context> context, bool isNegative) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    double infinity = std::numeric_limits<double>::infinity();
    if (isNegative)
	infinity = -infinity;
    
    Persistent<Value> result = Persistent<Value>::New(Number::New(infinity));
    return result;
}

extern "C" Handle<Value> makeString(Persistent<Context> context, const char* s) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    Persistent<Value> result = Persistent<Value>::New(String::New(s));
    return result;
}

extern "C" Handle<Value> makeBoolean(Persistent<Context> context, bool b) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    Persistent<Value> result = Persistent<Value>::New(Boolean::New(b));
    return result;
}

extern "C" Handle<Value> makeUndefined() {
    return Undefined();
}

extern "C" Handle<Value> makeNull() {
    return Null();
}

extern "C" Handle<Value> makeFunction(Persistent<Context> context, InvocationCallback cb) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    Persistent<Object> result = Persistent<Object>::New(FunctionTemplate::New(cb)->GetFunction());
    return result;
}

extern "C" Handle<Value> makeException(Persistent<Context> context, Handle<Array> array) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    Persistent<Object> exception = Persistent<Object>::New(Object::New());
    exception->Set(String::New("Message"), String::New("An F# exception occurred"));
    exception->Set(String::New("Values"), array);
    Persistent<Value> result = Persistent<Value>::New(exception);
    return result;
}

extern "C" Handle<Value> throwException(Persistent<Context> context, Handle<Value> exception) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    ThrowException(exception);
    return Persistent<Value>::New(String::New("hello, ed"));
}

extern "C" Handle<Value> makeArray(Persistent<Context> context, int size, void* array) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    Persistent<Array> result = Persistent<Array>::New(Array::New(size));
    return result;
}

extern "C" void setElementArray(Persistent<Context> context, Handle<Array> array, int index, Handle<Value> element) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    array->Set(index, element);
}

extern "C" Handle<Value> getElementArray(Persistent<Context> context, Handle<Array> array, int index) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    return array->Get(index);
}

extern "C" int getArrayLength(Persistent<Context> context, Handle<Array> array) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    return array->Get(String::New("length"))->ToObject()->Uint32Value();
}

extern "C" void extractArray(Persistent<Context> context,
				       Handle<Array> array,
				       int length,
				       Handle<Value>* result) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    for (int i = 0; i < length; i++) {
	result[i] = array->Get(i);
    }
}

extern "C" void getProperties(Persistent<Context> context,
			     Handle<Object> obj,
			     Handle<Array> names,
			     int length,
			     Handle<Value>* result) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    for (int i = 0; i < length; i++) {
	result[i] = Persistent<Value>::New(obj->Get(names->Get(i)));
    }
}


extern "C" Handle<Value> getProperty(Persistent<Context> context,
			     Handle<Object> obj,
			    const char* s) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    Handle<Value> result =  obj->Get(String::New(s));
    if (result.IsEmpty()) {
	return Undefined();;
    }
    else
	return Persistent<Value>::New(result);
}


extern "C" Handle<Value> getOwnPropertyNames(Persistent<Context> context, Handle<Object> object) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    Persistent<Array> names = Persistent<Array>::New(object->GetOwnPropertyNames());
    return names;
}


extern "C" Handle<Value> makeObjectLiteral(Persistent<Context> context) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    Persistent<Object> object = Persistent<Object>::New(Object::New());
    return object;
}

extern "C" void setProperty(Persistent<Context> context,
			    Handle<Object> object,
			    const char* str,
			    Handle<Value> value,
                            bool writable) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    int attrs = writable ? 0: ReadOnly;
    object->Set(String::New(str), value, PropertyAttribute(attrs | DontDelete));
}


extern "C" bool isBoolean(Handle<Value> b) {
    return b->IsBoolean();
}

extern "C" bool isNumber(Handle<Value> n) {
    return n->IsNumber();
}

extern "C" bool isString(Handle<Value> n) {
    return n->IsString();
}

extern "C" bool isFunction(Handle<Value> n) {
    return n->IsFunction();
}

extern "C" bool isNull(Handle<Value> n) {
    return n == Null();
}

extern "C" bool isUndefined(Handle<Value> n) {
    // return n->IsUndefined();
    return n == Undefined();
}

extern "C" bool isInt32(Handle<Value> n) {
    return n->IsInt32();
}

extern "C" bool isArray(Handle<Value> arr) {
    return arr->IsArray();
}

extern "C" bool isObject(Handle<Value> obj) {
    return obj->IsObject();
}

extern "C" void registerValue(Persistent<Context> context, const char* name, Handle<Value> val) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);

    Handle<Object> global = context->Global();
    
    global->Set(String::New(name), val);
}

extern "C" bool strictComparison(Persistent<Context> context, Handle<Value> val1, Handle<Value> val2) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);
    return val1->StrictEquals(val2);
}

