open FsCheck
open Mixture
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
    
let main() =
    JSUtils.create_context() |> ignore
    Check.QuickAll<NEmbeddingTests>()
    
do
    main()

let rec fib n =
    if n < 2 then 1
    else fib (n-1) + fib(n-2)

let rec factorial x =
    match x with
        | 0 | 1 -> 1
        | n -> n * factorial (n-1)
