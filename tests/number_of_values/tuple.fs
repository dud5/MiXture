open Mixture


let float_tuple_id :(float*int) = NEmbedding.public_embed >> NEmbedding.public_project<float*int> >> ignore


[<EntryPoint>]
let main args =
    let max = int args.[0]
    for i = 1 to max do
        float_tuple_id (float i, i)
    exit 0
  
