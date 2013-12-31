open Mixture

// let arr_id : NEmbedding.JSValue2 -> unit = NEmbedding.public_project<(int->int->int) list> >> ignore

// let jadd = "(function(x,y) {return x+y;}),"

// [<EntryPoint>]
// let main args =
//     let length = int args.[0]
//     "[" + String.replicate length jadd + "]"
//     |> NEmbedding.public_execute_string
//     |> arr_id
//     exit 0


let arr_id : NEmbedding.JSValue2 -> unit = NEmbedding.public_project<int[]> >> ignore

let jadd = "1,"

[<EntryPoint>]
let main args =
    let length = int args.[0]
    let s = "[" + String.replicate length jadd + "]"
    let js = NEmbedding.public_execute_string s

    let a = 
      try
        NEmbedding.public_project<int list> js
      with
        | _ -> [1]

    if a = [1] then
      NEmbedding.public_project<int list> js |> ignore
      printfn "yeah"
    else ()

    exit 0
