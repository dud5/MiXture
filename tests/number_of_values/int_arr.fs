open Mixture


let int_arr_id :int[] -> int[] = NEmbedding.public_embed >> NEmbedding.public_project


[<EntryPoint>]
let main args =
    let max = int args.[0]
    for i = 1 to max do
        (int_arr_id [|i;i;i;i|]) |> ignore
    exit 0

