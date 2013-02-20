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
    let jfib = NEmbedding.embed fib
    NEmbedding.register_values(["jfib", jfib])
    printf "jfib(45): %d\n" (NEmbedding.project (JSUtils.execute_string("jfib(45)")))
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

    printf "fib(45): %d\n" result

let test_function_project() =
    let jfib = JSUtils.execute_string("var jfib = (function fib(n) { if (n<2) {return 1;} else {return fib(n-1) + fib(n-2);}}); jfib;")
    let jfib = JSUtils.execute_string("var jfib = (function fib(a,b) { return a+b; }); jfib;")

    let fib: int->int->int = project jfib
    // printf "jfib(45): %d\n" (NEmbedding.project (JSUtils.execute_string("jfib(45)")))
    let result =
        try
            fib 10 5
        with
            | Mixture.NEmbedding.JSException(x) -> NEmbedding.project x

    printf "fib(45): %d\n" result

// let test_function_project_unit() =
//     let jlt2 = JSUtils.execute_string("var lt2 = (function fib(n) {if (n<2) {return undefined} else {return 3;}}); lt2;")
//     let lt2: int->unit = project jlt2
//     (NEmbedding.project (JSUtils.execute_string("lt2(1)"))) |> ignore
//     (lt2 1) |> ignore
    


let main() =
    JSUtils.create_context() |> ignore
    Check.QuickAll<NEmbeddingTests>()
    test_function_embed()
    test_function_embed_ex()
    test_function_project_ex()
    test_function_project()
    // test_function_project_unit()
do
    main()

