open Mixture

let float_id :float -> unit = NEmbedding.public_embed >> NEmbedding.public_project<float> >> ignore

[<EntryPoint>]
let main args =
    let max = int args.[0]
    for i = 1 to max do
        float_id (float i)
    exit 0
