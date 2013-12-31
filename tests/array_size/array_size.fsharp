open Mixture

let arr_id : (int->int->int) [] -> unit = NEmbedding.public_embed  >> ignore

let add x y  = x+y

[<EntryPoint>]
let main args =
    let length = int args.[0]
    Array.create length add
    |> arr_id
    exit 0

