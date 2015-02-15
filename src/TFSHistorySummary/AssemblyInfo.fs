namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("TFSHistorySummary")>]
[<assembly: AssemblyProductAttribute("TFSHistorySummary")>]
[<assembly: AssemblyDescriptionAttribute("Scans TFS history dump to extract statistics")>]
[<assembly: AssemblyVersionAttribute("0.0.1")>]
[<assembly: AssemblyFileVersionAttribute("0.0.1")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.1"
