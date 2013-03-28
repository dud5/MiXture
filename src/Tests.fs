module Mixture.Tests

open FsCheck
open Mixture
open System.Diagnostics
open Mixture.NEmbedding
open System

let doubleCrossInt (x:int) =
    let fx: int = x |> embed |> project
    fx = x

let doubleCrossString (x:string) =
    let fx: string = x |> embed |> project
    if x.Contains("^@") then true
    else fx = x

let doubleCrossFloat (x:float) =
    let fx: float = x |> embed |> project
    if Double.IsNaN(x) then Double.IsNaN(fx)
    else fx = x

let doubleCrossBool (x:bool) =
    let fx: bool = x |> embed |> project
    fx = x

let doubleCrossArray (x:int[]) =
    let fx: int[] = x |> embed |> project
    fx = x


let fsFuncInJS (f: int->int) =
    let jf = embed f
    register_values(["jf", jf])
    let jresult = JSUtils.execute_string("jf(10)")
    (project jresult) = (f 10)

let testPositiveInfinityFS2J() =
    let inf = System.Double.PositiveInfinity
    let fjfinf = inf |> embed |> project
    fjfinf = inf

let testPositiveInfinityJ2FS() =
    let inf = JSUtils.execute_string("Number.POSITIVE_INFINITY")
    let finf:float = project inf
    let jfjinf = embed finf
    JSEngine.strictComparison(JSUtils.context, inf, jfjinf)

let testNegativeInfinityFS2J() =
    let inf = System.Double.NegativeInfinity
    let fjfinf = inf |> embed |> project
    fjfinf = inf

let testNegativeInfinityJ2FS() =
    let inf = JSUtils.execute_string("Number.NEGATIVE_INFINITY")
    let finf:float = project inf
    let jfjinf = embed finf
    JSEngine.strictComparison(JSUtils.context, inf, jfjinf)


type NEmbeddingTests =
    static member ``Double crossing of ints`` x = doubleCrossInt x
    static member ``Double crossing of strings`` x = doubleCrossString x
    static member ``Double crossing of floats`` x = doubleCrossFloat x
    static member ``Double crossing of bools`` x = doubleCrossBool x
    static member ``Double crossing of arrays`` x = doubleCrossArray x
    static member ``Functions`` x = fsFuncInJS x
    static member ``Double crossing of positive infinity`` () = testPositiveInfinityFS2J()
    static member ``Double crossing of positive infinity JS to FS`` () = testPositiveInfinityJ2FS()
    static member ``Double crossing of negative infinity`` () = testNegativeInfinityFS2J()
    static member ``Double crossing of negative infinity JS to FS`` () = testNegativeInfinityJ2FS()


let rec fib n =
    if n < 2 then 1
    else fib (n-1) + fib(n-2)

let rec factorial x =
    match x with
        | 0 | 1 -> 1
        | n -> n * factorial (n-1)


let test_function_embed() =
    // let jfib = NEmbedding.embed fib
    // NEmbedding.register_values(["jfib", jfib])
    // printf "jfib(45): %d\n" (NEmbedding.project (JSUtils.execute_string("jfib(45)")))
    printf "fib(45): %d\n" (fib 45)

exception AA of int*string

let test_function_embed_ex () =
    let f n =
        if n<2 then raise (AA(3, "Hello, Edu!"))
        else n
    let jf = NEmbedding.embed f
    NEmbedding.register_values(["jf", jf])
    // JSUtils.execute_string("jf(1)") |> ignore
    printf "jf(1): %A\n" (NEmbedding.project (JSUtils.execute_string("try { jf(1) } catch(err) {err.Values;}")))
    // printf "jf(1): %d\n" (NEmbedding.project (JSUtils.execute_string("try { throw 2 } catch(err) { 3;}")))


let test_function_embed_tupled() =
    let add (x, y) = x+y
    let jadd = NEmbedding.embed add
    NEmbedding.register_values(["jadd", jadd])
    printf "jadd(4,5): %d\n" (NEmbedding.project (JSUtils.execute_string("jadd(4,5)")))
    printf "add(4,5): %d\n" (add (4,5))


let test_function_project_ex() =
    // let jfib = JSUtils.execute_string("var jfib = (function fib(n) { if (n<2) {return 1;} else {return fib(n-1) + fib(n-2);}}); jfib;")
    let jfib = JSUtils.execute_string("var jfib = (function fib(n) { throw 34;}); jfib;")
    let fib: int->int = project jfib
    // printf "jfib(45): %d\n" (NEmbedding.project (JSUtils.execute_string("jfib(45)")))
    let result =
        try
            fib 45
        with
            | Mixture.NEmbedding.JSException(x) -> NEmbedding.project x

    printf "fib(45) with exception: %d\n" result


let make_list (x,y) = [x;y]


let test_function_embed_poly() =
    let jmake = NEmbedding.embed_poly_func <@ make_list @>
    NEmbedding.register_values(["jmake", jmake])
    let t = JSUtils.execute_string("jmake(1,2)")
    JSEngine.print_result(JSUtils.context, t)


let test_function_project() =
    let jfib = JSUtils.execute_string("var jfib = (function fib(n) { if (n<2) {return 1;} else {return fib(n-1) + fib(n-2);}}); jfib;")
    let jfib = JSUtils.execute_string("var jfib = (function fib(a,b) { return a+b; }); jfib;")

    let fib: int->int->int = project jfib
    printf "jfib(45): %d\n" (NEmbedding.project (JSUtils.execute_string("jfib(45)")))
    let result =
        try
            fib 10 5
        with
            | Mixture.NEmbedding.JSException(x) -> NEmbedding.project x

    printf "fib(45): %d\n" result

type website =
    { Title : string;
        Url : string;
      }
let test_embed_record () =
    let homepage = { Title = "Google"; Url = "http://www.google.com"; }
    let jhp = embed homepage
    JSEngine.print_result(JSUtils.context, jhp)
    NEmbedding.register_values(["jhp", jhp])
    JSEngine.print_result(JSUtils.context, JSUtils.execute_string("jhp.Title"))
    JSEngine.print_result(JSUtils.context, JSUtils.execute_string("jhp.Url"))

let test_project_record () =
    let jhp = JSUtils.execute_string("({Title:'Eduardo Munoz', Url:'edua.rdomunoz.com'})")
    let hp: website = project jhp
    printf "This is hp: %A\n" hp
    JSEngine.print_result(JSUtils.context, jhp)
    NEmbedding.register_values(["jhp", jhp])
    JSEngine.print_result(JSUtils.context, JSUtils.execute_string("jhp.Title"))
    JSEngine.print_result(JSUtils.context, JSUtils.execute_string("jhp.Url"))
    JSEngine.print_result(JSUtils.context, JSUtils.execute_string("jhp.Add1(41)"))


// let test_function_project_unit() =
//     let jlt2 = JSUtils.execute_string("var lt2 = (function fib(n) {if (n<2) {return undefined} else {return 3;}}); lt2;")
//     let lt2: int->unit = project jlt2
//     (NEmbedding.project (JSUtils.execute_string("lt2(1)"))) |> ignore
//     (lt2 1) |> ignore


let test_pol_embed () =
    let f (x:'a[]) = x.[0]
    let jf = NEmbedding.embed f
    NEmbedding.register_values(["jf", jf])
    let jx = JSUtils.execute_string("jf([1,'a']);")
    let x:int = NEmbedding.project jx
    printf "this is x: %d\n" x

// let jf = JSUtils.execute_string("(function(x) {return [x];})")
// // let f: 'a->'a = fun x -> x
// let f<'A> : 'A -> 'A[] = NEmbedding.project jf
// let x: int[] = f 3
// let y: string[] = f "a"
// printf "this is x: %A\n" x
// printf "this is a: %A\n" y

let test_pol_proj () =
    let jf = JSUtils.execute_string("(function(x) {return [x];})")
    // let f: 'a->'a = fun x -> x
    // let f<'A> : 'A -> 'A[] = NEmbedding.project jf
    // let x: int[] = f 3
    // let y: string[] = f "a"
    // printf "this is x: %A\n" x
    // printf "this is a: %A\n" y
    // let t = f 3
    // let a = f "A"
    // printf "this is type of f: %A\n" (f.GetType())
    // let projectt () : 'R = failwith "!"
    // let dfid2<'a> : 'a -> 'a = projectt ()
    // let f (x:'a) :'a = x
    // let x = f 3
    // let y = f "a"
    printf "this is x: %d\n" 3
    // printf "this is a: %s\n" y


let main() =
    // JSUtils.create_context() |> ignore
    // Check.QuickAll<NEmbeddingTests>()
    test_function_embed()
    test_function_embed_ex()
    test_function_embed_tupled()
    test_function_embed_poly()
    test_function_project_ex()
    // test_function_project()
    // test_function_project_unit()
    // test_embed_record()
    // test_project_record()
    // test_pol_proj()
do
    main()
