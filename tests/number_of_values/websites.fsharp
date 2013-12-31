open Mixture

type website =
       { Title : string;
   mutable Url : string }

let website_id :website -> unit = NEmbedding.public_embed >> NEmbedding.public_project<website> >> ignore


let homepage = { Title = "Google"; Url = "http://www.google.com"; }

[<EntryPoint>]
let main args =
    let max = int args.[0]
    for i = 1 to max do
      website_id homepage
    exit 0
  
