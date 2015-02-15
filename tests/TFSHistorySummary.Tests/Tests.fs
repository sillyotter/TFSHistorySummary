module TFSHistorySummary.Tests

open NUnit.Framework

[<Test>]
let ``Parse default file doesnt crash`` () =
    let res = TFSHistorySummary.main (Array.create<string> 0 "")
    Assert.AreEqual(0, res)
