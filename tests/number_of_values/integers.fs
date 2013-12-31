open Mixture

let int_id :int -> unit = NEmbedding.public_embed >> NEmbedding.public_project >> ignore

[<EntryPoint>]
let main args =
    let max = 200000
    for i = 1 to max do
        int_id i
    exit 0
