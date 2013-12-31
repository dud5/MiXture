open Mixture

let string_id :string->unit = NEmbedding.public_embed >> ignore

let s = "z"

[<EntryPoint>]
let main args =
    let length = int args.[0]
    String.replicate length s
    |> string_id
    exit 0
  
