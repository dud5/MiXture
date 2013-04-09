module Mixture.Test.NEmbedding
open System
open NUnit.Framework
open FsCheck
open FsCheck.NUnit
open Mixture
open Mixture.NEmbedding

type Generators =
    static member JSValuePosFloatGen =
        gen {
            let! x =
                Arb.generate
                |> Gen.suchThat (System.Double.IsInfinity >> not) 
                |> Gen.suchThat (System.Double.IsNaN >> not)
                |> Gen.suchThat ((>) (System.Double.MaxValue - 1e300))
                |> Gen.suchThat ((<) (0.0))
            return x
        }


    static member JSValueNegFloatGen =
        gen {
            let! x =
                Arb.generate
                |> Gen.suchThat (System.Double.IsInfinity >> not) 
                |> Gen.suchThat (System.Double.IsNaN >> not)
                |> Gen.suchThat ((<) (System.Double.MinValue + 1e30))
                |> Gen.suchThat ((>) (0.0))
            return x
        }

    static member JSValuePosIntGen =
        gen {
            let! x = Arb.generate<int> |> Gen.suchThat((<) 0)
            return x
        }

    static member JSValueNegIntGen =
        gen {
            let! x = Arb.generate<int> |> Gen.suchThat((>) 0)
            return x
        }

    static member JSValueStringGen =
        gen {
            let! s = Arb.generate |> Gen.suchThat(Utils.containsNull >> not)
            return s
        }

    static member JSValueRecGen =
        gen {
            let! url = Arb.generate
            let objec = sprintf "({Url: %i})" url
            let r:TestUtils.website = {Url = url;}
            return (r, objec)
        }

    static member JSValueStringArray =
        gen {
            let! x = Gen.arrayOf Generators.JSValueStringGen
            return x
        }

    static member JSValueIntArray =
        gen {
            let! x = Gen.arrayOf Arb.generate<int>
            return x
        }

    static member JSValueFunc =
        gen {
            let! x = Arb.generate<int->int>
            return x
        }


                    
[<TestFixture>]              
type NEmbedding_INVALID() =                                                                                                                              
    [<Test>]
    member this.FSObject() =
        let x = "({Url: 200})"
        let j = public_execute_string(x)
        Assert.Throws<ProjectException>(fun () ->
                                        public_project<TestUtils.Website> j |> ignore)
        |> ignore
            
    [<Test>]
    member this.FSOption() =
        let x = "3"
        let j = public_execute_string(x)
        Assert.Throws<ProjectException>(fun () ->
                                        public_project<Option<int>> j |> ignore)
        |> ignore

[<TestFixture>]
type NEmbedding_VALID() = 
    [<Test>]
    member this.PositiveIntegers () =
        fsCheck "Positive integers - check for valid" (Prop.forAll (Arb.fromGen Generators.JSValuePosIntGen)
            (fun (n:int) ->
                let j = public_execute_string(Utils.int2string n)
                let back = public_project j
                n = back))
    [<Test>]
    member this.NegativeIntegers () =
        fsCheck "Negative integers - check for valid" (Prop.forAll (Arb.fromGen Generators.JSValueNegIntGen)
            (fun (n:int) ->
                let j = public_execute_string(Utils.int2string n)
                let back = public_project j
                n = back))

                        
    [<Test>]
    member this.Object() =
        fsCheck "Records - check for valid" (Prop.forAll (Arb.fromGen Generators.JSValueRecGen)
            (fun (r:TestUtils.website,x:string) ->
                let j = public_execute_string(x)
                let back = public_project<TestUtils.website> j
                r = back))

    [<Test>]
    member this.PositiveFloats() =
        fsCheck "Floats - check for valid" (Prop.forAll (Arb.fromGen Generators.JSValuePosFloatGen)
            (fun (x:float) ->
                let j = public_embed x
                let back = public_project j
                x = back))
                    
    [<Test>]
    member this.NegativeFloats() =
        fsCheck "Floats - check for valid" (Prop.forAll (Arb.fromGen Generators.JSValueNegFloatGen)
            (fun (x:float) ->
                let j = public_embed x
                let back = public_project j
                x = back))

    [<Test>]
    member this.FloatZero() =
        let zero = public_embed (0.0)
        Assert.AreEqual(public_project zero, 0.0)

    [<Test>]
    member this.IntZero() =
        let zero = public_embed (0)
        Assert.AreEqual(public_project zero, 0)

                        
    [<Test>]
    member this.Infinities() =
        let posinf = public_embed (System.Double.PositiveInfinity)
        let neginf = public_embed (System.Double.NegativeInfinity)
        Assert.AreEqual(public_project posinf, System.Double.PositiveInfinity)
        Assert.AreEqual(public_project neginf, System.Double.NegativeInfinity)

    [<Test>]
    member this.NaN() =
        let nann = public_embed (System.Double.NaN)
        Assert.IsNaN(public_project<float> nann)


    [<Test>]
    member this.EmptyString() =
        let e = public_embed ""
        Assert.IsEmpty(public_project e)
            
    [<Test>]
    member this.Undefined() =
        let undef = public_embed ()
        Assert.AreEqual(public_project undef, ())

    [<Test>]
    member this.Strings() =
        fsCheck "Strings - check for valid" (Prop.forAll (Arb.fromGen Generators.JSValueStringGen)
            (fun (s:string) ->
                let j = public_embed(s)
                let back:string = public_project j
                s = back))
           


    [<Test>]
    member this.ArrayString() =
        fsCheck "String array - check for valid" (Prop.forAll (Arb.fromGen Generators.JSValueStringArray)
            (fun (arr) ->
                let j = public_embed(arr)
                let back: string[] = public_project j
                arr = back))


    [<Test>]
    member this.ArrayInt() =
        fsCheck "Int array - check for valid" (Prop.forAll (Arb.fromGen Generators.JSValueIntArray)
            (fun (arr) ->
                let j = public_embed(arr)
                let back: int[] = public_project<int[]> j
                arr = back))


    [<Test>]
    member this.Functions() =
        fsCheck "Functions - check for valid" (Prop.forAll (Arb.fromGen Generators.JSValueFunc)
            (fun (f:int -> int) ->
                let j = public_embed(f)
                let back:int->int = public_project j
                
                silent_fsCheck "Functions - check for valid" (Prop.forAll (Arb.fromGen Generators.JSValuePosIntGen)
                    (fun (n) ->
                        let native = f n
                        let foreign:int = public_project (j *@ [public_embed n])
                        native = foreign
                        ))
                    ))
