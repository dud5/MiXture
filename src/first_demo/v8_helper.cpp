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

extern "C" Handle<Value> execute_string(Persistent<Context> context, char* s) {
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

// typedef Handle<Value> (*Callback)(Handle<Value>, Handle<Value>);


typedef Handle<Value> (*CALLBACK)(const Arguments&);
// typedef Handle<Value> (*CALLBACK)(int);


Handle<Value> Plus(const Arguments& args) {
    return args[0];
} 

// extern "C" void unmanaged(Persistent<Context> context, CALLBACK cb) {
//     HandleScope handle_scope;
//     Context::Scope context_scope(context);

//     Handle<Object> global = context->Global();

//     // Handle<ObjectTemplate> global = ObjectTemplate::New();
    
//     global->Set(String::New("apply_fs"), FunctionTemplate::New(Plus)->GetFunction());
    
// }

extern "C" void unmanaged(Persistent<Context> context, CALLBACK cb) {
    HandleScope handle_scope;
    Context::Scope context_scope(context);

    Handle<Object> global = context->Global();
    
    global->Set(String::New("fsharp"), FunctionTemplate::New(cb)->GetFunction());

    Handle<String> source = String::New("fsharp(1)");
    Handle<Script> script = Script::Compile(source);
    Handle<Value> result = script->Run();
    print_result(context, result);
}

