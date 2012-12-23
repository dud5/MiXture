#include <iostream>
#include <v8.h>

using namespace v8;
using namespace std;


extern "C" void dispose_handle(Persistent<String> prst) {
    prst.Dispose();
    prst.Clear();
}

extern "C" Persistent<Context> get_context() {    
    return Context::New();
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

extern "C" Handle<Value> execute_string(Persistent<Context> context, const char* s) {
    // Create a stack-allocated handle scope.
    HandleScope handle_scope;

    // Enter the created context for compiling and
    // running the hello world script.
    Context::Scope context_scope(context);

    // Create a string containing the JavaScript source code.
    Handle<String> source = String::New(s);

    // Compile it
    Handle<Script> script = Script::Compile(source);

    // Run it
    Persistent<Value> result = Persistent<Value>::New(script->Run());

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
    func->SetName(String::New("add"));

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

extern "C" Handle<Value> apply_function_arr(Persistent<Context> context,
                                        Handle<Function> func,
                                        Handle<Value>* arg) {
    HandleScope handle_scope;

    Context::Scope context_scope(context);

    Handle<Value> result = func->Call(context->Global(), 2, arg);

    Persistent<Value> js_result = Persistent<Value>::New(result);
    
    print_result(context, result);    
    return js_result;
}


typedef Handle<Value> (*CALLBACK)(const Arguments&);


extern "C" void register_function(Persistent<Context> context, CALLBACK cb) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);

    Handle<Object> global = context->Global();
    
    global->Set(String::New("apply_fsharp"), FunctionTemplate::New(cb)->GetFunction());
    // up to here
    // Handle<String> source = String::New("apply_fsharp(10000, 2)");
    // Handle<Script> script = Script::Compile(source);
    // Handle<Value> result = script->Run();
    // print_result(context, result);
}


extern "C" int  arguments_length(Persistent<Context> context, const Arguments& args) {
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
    execute_string(context, "function FLump(pointer) { this.pointer = pointer; }");

    const char* s = "var a = new FLump(%d); a";
    cout << "This is the pointer " << pointer << endl;
    char script[100];
    int n = sprintf(script, s, pointer);
    if (n < 0) {
	cerr << "something went wrong when creating an FLump in JavaScript" << endl;
	return Undefined();
    }
    return execute_string(context, script);
}


extern "C" int get_pointer_lump(Persistent<Context> context, Handle<Object> lump) {
    HandleScope handle_scope;

    Context::Scope context_scope(context);

    return lump->Get(String::New("pointer"))->Uint32Value();
    
}
