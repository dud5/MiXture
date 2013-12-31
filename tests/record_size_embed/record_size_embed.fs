open Mixture

type RList = { head : int; tail : RList }

let list_id :RList->unit = NEmbedding.public_embed >> ignore


let rec fooList n =   
  if n = 0 then Unchecked.defaultof<_>
  else { head = n; tail = fooList (n - 1) }



[<EntryPoint>]
let main args =
    let length = int args.[0]
    fooList length
    |> list_id
    exit 0
  
