open Mixture


let add_id :(int -> int -> int) -> unit = NEmbedding.public_embed >> NEmbedding.public_project<int->int->int> >> ignore

let add x y = x+y


[<EntryPoint>]
let main args =
    let max = 200
    for i = 1 to max do
        add_id add
    exit 0
  
