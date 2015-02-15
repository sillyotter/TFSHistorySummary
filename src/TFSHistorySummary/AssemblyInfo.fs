namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("TFSHistorySummary")>]
[<assembly: AssemblyProductAttribute("TFSHistorySummary")>]
[<assembly: AssemblyDescriptionAttribute("Scans TFS history dump to extract statistics")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
