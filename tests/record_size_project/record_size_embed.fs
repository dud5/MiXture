open Mixture
open System.Threading

type RList = { head : int; tail : RList }

let list_id :RList->unit = NEmbedding.public_embed >> ignore


let rec fooList n =   
  if n = 0 then Unchecked.defaultof<_>
  else { head = n; tail = fooList (n - 1) }



[<EntryPoint>]
let main args =
    let length = int args.[0]
    // let t = new Thread((fun () ->
    //            fooList length
    //            |> list_id), 104857600)

    // t.Start()
    // t.Join()
    fooList length
    |> list_id
    exit 0
  
