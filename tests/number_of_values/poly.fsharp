open Mixture
open Microsoft.FSharp.Quotations


let id_id :(Expr) -> unit = NEmbedding.embed_poly_func >> NEmbedding.project<obj->obj> >> ignore

let id x = x


[<EntryPoint>]
let main args =
    let max = int args.[0]
    for i = 1 to max do
        id_id <@ id @>
    exit 0
  
