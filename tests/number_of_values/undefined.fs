open Mixture

let unit_id :unit->unit = NEmbedding.public_embed >> NEmbedding.public_project >> ignore

let u = ()

[<EntryPoint>]
let main args =
    let max = int args.[0]
    for i = 1 to max do
        (unit_id u)
    exit 0
  
