open Mixture
open System.Text
let string_id : NEmbedding.JSValue2->unit = NEmbedding.public_project<string> >> ignore

let s = "z"

[<EntryPoint>]
let main args =
    let length = int args.[0]
    // "'" + String.replicate length s + "'"
    let sb = new StringBuilder("'", length+4)
    for i = 1 to length do
        sb.Append(s) |> ignore
    sb.Append("'").ToString()
    |> NEmbedding.public_execute_string
    |> string_id
    exit 0
  
