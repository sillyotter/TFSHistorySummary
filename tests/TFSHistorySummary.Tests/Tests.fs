module TFSHistorySummary.Tests

open NUnit.Framework

[<Test>]
let ``Parse default file doesnt crash`` () =
    let res = TFSHistorySummary.main [| "C:\\Projects\\TFSHistorySummary\\tfhistory.txt" |]
    Assert.AreEqual(0, res)
