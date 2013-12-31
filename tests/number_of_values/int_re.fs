open Mixture

type int_re =
    { I : int }

let int_re_id :int_re -> int_re = NEmbedding.public_embed >> NEmbedding.public_project<int_re>

[<EntryPoint>]
let main args =
    let a = {I = 42}
    let max = 200000
    for i = 1 to max do
        int_re_id a |> ignore
    exit 0

