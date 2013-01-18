open FsCheck
open Mixture.NEmbedding

let doubleCrossInt (x:int) =
    let fx: int = x |> embed |> project
    fx = x

let doubleCrossString (x:string) =
    let fx: string = x |> embed |> project
    fx = x

let doubleCrossFloat (x:float) =
    let fx: float = x |> embed |> project
    fx = x

let doubleCrossBool (x:bool) =
    let fx: bool = x |> embed |> project
    fx = x

let doubleCrossArray (x:int[]) =
    let fx: int[] = x |> embed |> project
    fx = x


type NEmbeddingTests =
    static member ``Double crossing of ints`` x = doubleCrossInt x
    static member ``Double crossing of strings`` x = doubleCrossString x
    static member ``Double crossing of floats`` x = doubleCrossFloat x
    static member ``Double crossing of bools`` x = doubleCrossBool x
    static member ``Double crossing of arrays`` x = doubleCrossArray x


let main() =
    Check.QuickAll<NEmbeddingTests>()
    


do
    main()
