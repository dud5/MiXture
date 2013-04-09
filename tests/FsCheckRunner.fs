
module FsCheck.NUnit
open FsCheck
open NUnit.Framework

let private nUnitRunner silent =
    { new IRunner with
        member x.OnStartFixture t = ()
        member x.OnArguments(ntest, args, every) = ()
        member x.OnShrink(args, everyShrink) = ()
        member x.OnFinished(name, result) = 
            match result with 
            | TestResult.True data ->
                if silent then
                    ()
                else printfn "%s" (Runner.onFinishedToString name result)
            | _ -> Assert.Fail(Runner.onFinishedToString name result) }
   
let private nUnitConfig silent = { Config.Default with Runner = nUnitRunner silent }

let full_fsCheck issilent name testable =
    FsCheck.Check.One (name, nUnitConfig issilent, testable)


let silent_fsCheck name testable= full_fsCheck true name testable

let fsCheck name testable = full_fsCheck false name testable

//module Gen = 
//    let ap x = flip Gen.apply x
