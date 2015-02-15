open System.IO
open System.Text.RegularExpressions

type ParserState = 
    | LookingForStartOfRecord
    | LookingForUser
    | LookingForItemsGroup
    | LookingForItem

type Change = 
    { changeType : string list
      filePath : string }

type ChangeSetInfo = 
    { user : string
      items : Change list }

type ParserData = 
    { state : ParserState
      changes : ChangeSetInfo list
      current : ChangeSetInfo }

let (|StartsWith|_|) (target : string) (source : string) = 
    if source.StartsWith(target) then Some()
    else None

let (|Regex|_|) (regex : Regex) (str : string) = 
    let m = regex.Match(str)
    if m.Success then 
        Some(m.Groups
             |> Seq.cast<Group>
             |> Seq.skip 1
             |> Seq.map (fun x -> 
                    x.Captures
                    |> Seq.cast<Capture>
                    |> Seq.map (fun x -> x.Value)
                    |> Seq.toList)
             |> Seq.toList)
    else None

let validNames = ["Fankhauser"; "Hunt"; "Vance"; "Oliver"; "Bennett"]
let userRegex = Regex(@"^User:\s+(.+)$", RegexOptions.Compiled|||RegexOptions.Singleline);
let editRegex = Regex(@"^\s+(?:([a-z ]+)(?:[, ]+)?)+\s+\$(.+)", RegexOptions.Compiled|||RegexOptions.Singleline)

let statemachine (s : ParserData) (l : string) = 
    match s.state with
    | ParserState.LookingForStartOfRecord -> 
        match l with
        | StartsWith @"---" -> { s with state = ParserState.LookingForUser }
        | _ -> s
    | ParserState.LookingForUser -> 
        match l with
        | Regex userRegex [ [ u ] ] -> 
            if validNames |> Seq.map (fun x -> u.Contains(x)) |> Seq.exists (fun x -> x) then
                { s with state = ParserState.LookingForItemsGroup
                         current = { s.current with user = u } }
            else
                { s with state = ParserState.LookingForStartOfRecord
                         current = 
                             { user = ""
                               items = List.empty }  }
        | _ -> s
    | ParserState.LookingForItemsGroup -> 
        match l with
        | StartsWith @"Items:" -> { s with state = ParserState.LookingForItem }
        | _ -> s
    | ParserState.LookingForItem -> 
        match l with
        | Regex editRegex [ edits; [ path ] ] -> 
            let cleanEdits = edits |> List.map (fun x -> x.Trim())
            if List.exists (fun x -> x = "add" || x = "edit") cleanEdits && not(path.Contains("-Release")) then 
                { s with current = 
                             { s.current with items = 
                                                  { changeType = cleanEdits
                                                    filePath = path }
                                                  :: s.current.items } }
            else
                s
        | _ -> 
            if s.current.user <> "" && s.current.items.Length > 0 then
                { s with state = ParserState.LookingForStartOfRecord
                         changes = s.current :: s.changes
                         current = 
                             { user = ""
                               items = List.empty } }
            else
                { s with state = ParserState.LookingForStartOfRecord
                         current = 
                             { user = ""
                               items = List.empty } }
[<EntryPoint>]
let main _ = 
    let initialState = 
        { state = ParserState.LookingForStartOfRecord
          changes = List.empty
          current = 
              { user = ""
                items = List.empty } }
    File.ReadLines "c:\\Projects\\TFSHistorySummary\\tfhistory.txt"
    |> Seq.map (fun x -> x.TrimEnd())
    |> Seq.fold statemachine initialState
    |> fun s -> 
        if s.current.user <> "" then
            { s with state = ParserState.LookingForStartOfRecord
                     changes = s.current :: s.changes
                     current = 
                         { user = ""
                           items = List.empty } }
        else
            s
    |> fun s -> s.changes
    |> Seq.groupBy (fun x -> x.user)
    |> Seq.map (fun (k, x) -> 
           k, 
           x |> Seq.length, 
           x |> Seq.collect (fun y -> y.items) |> Seq.length)
    |> Seq.sortBy (fun (_, c, _) -> -c)
    |> Seq.iter (fun (name, commits, edits) -> printfn "%s\t%d\t%d" name commits edits)
    0
