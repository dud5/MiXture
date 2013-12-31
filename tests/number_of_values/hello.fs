[<EntryPoint>]
let main args =
    // printfn "%s" args.[0]
    let f = 4
    let g = typeof<int>
    printfn "%A" g
    // let a = NEmbedding.public_embed f
        // try NEmbedding.public_embed f
        // with
        //   | exc ->
        //     printfn "%s" (exc.HelpLink)
        //     new NEmbedding.JSValue2(nativeint(19))
    // printfn "%A" a
    printfn "%s" "l"
    exit 0
