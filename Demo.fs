#r "Mixture.dll"
open Mixture
open Mixture.NEmbedding

let js_rev<'a> : 'a list -> 'a list =
    "(function(arr) {return arr.reverse();})"
    |> public_execute_string
    |> public_project


type Website =
    {Url: string;
    Code: int}

let w = {
    Url = "www.google.com";
    Code = 200}

let w2 =
    """({Url : 'www.cl.cam.ac.uk', Code : 200})"""
    |> public_execute_string
    |> public_project<Website>


let jw = public_embed w
let google : string= public_project(jw +> "Url")

js_rev [1;2;3]


let fail_greater_than5 =
    """
    (function(x) {
        if (x<=5) {
            return 10;
        }
        else {
            throw {Url : "www.google.com", Code : 200};
        }
    })
    """
    |> public_execute_string

let a : int =
    try
        fail_greater_than5 *@ [public_embed 6]
        |> public_project
    with
        | JSException(v) ->
            printfn "this was thrown: %A" <| project v
            2
