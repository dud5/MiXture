open Mixture

type RList = { head : int; tail : RList }

let list_id : NEmbedding.JSValue2->unit = NEmbedding.public_project<RList> >> ignore


let jfooList =
    """(function fooList(n) {
    if (n==0) {
	return undefined;
    }
    else {
	return { head : n, tail : fooList(n-1)};
    }
})"""
    |> JSUtils.execute_string

let fooList = new NEmbedding.JSValue2(jfooList)

[<EntryPoint>]
let main args =
    let length = int args.[0]
    // let t = new Thread((fun () ->
    //            fooList *@ [NEmbedding.public_embed length]
    //            |> list_id), 104857600)

    // t.Start()
    // t.Join()
    fooList *@ [NEmbedding.public_embed length]
    |> list_id
    exit 0
  
