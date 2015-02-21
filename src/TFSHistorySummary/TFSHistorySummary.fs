module TFSHistorySummary

open System.IO
open System.Text.RegularExpressions

type ParserState = 
    | LookingForStartOfRecord
    | LookingForUser
    | LookingForItemsGroup
    | LookingForItem

type Change = 
    { changeType : string
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
             |> Seq.map (fun x -> x.Value)
             |> Seq.toList)
    else None

let validNames = [ "Fankhauser"; "Hunt"; "Vance"; "Oliver"; "Bennett" ]
let userRegex = Regex(@"^User:\s+(.+)$", RegexOptions.Compiled ||| RegexOptions.Singleline)
let editRegex = Regex(@"^\s+(.+)\s+\$(.+)", RegexOptions.Compiled ||| RegexOptions.Singleline)
let updateState s pd = { pd with state = s }
let updateUser u pd = { pd with current = { pd.current with user = u } }

let resetCurrent pd = 
    { pd with state = ParserState.LookingForStartOfRecord
              current = 
                  { user = ""
                    items = List.empty } }

let appendItems i pd = { pd with current = { pd.current with items = i :: pd.current.items } }

let pushCurrent pd = 
    if pd.current.user <> "" && pd.current.items.Length > 0 then 
        { pd with state = ParserState.LookingForStartOfRecord
                  changes = pd.current :: pd.changes }
        |> resetCurrent
    else resetCurrent pd

let doStep (l : string) = 
    function 
    | ParserState.LookingForStartOfRecord -> 
        match l with
        | StartsWith @"---" -> updateState ParserState.LookingForUser
        | _ -> id
    | ParserState.LookingForUser -> 
        match l with
        | Regex userRegex [ u ] -> 
            if validNames
               |> Seq.map (fun x -> u.Contains(x))
               |> Seq.exists (fun x -> x)
            then updateState ParserState.LookingForItemsGroup >> updateUser u
            else updateState ParserState.LookingForStartOfRecord >> resetCurrent
        | _ -> id
    | ParserState.LookingForItemsGroup -> 
        match l with
        | StartsWith @"Items:" -> updateState ParserState.LookingForItem
        | _ -> id
    | ParserState.LookingForItem -> 
        match l with
        | Regex editRegex [ edits; path ] -> 
            let isOk = 
                edits.Split(',')
                |> Array.map (fun x -> x.Trim())
                |> Array.exists (fun x -> x = "add" || x = "edit")
            
            let notRelease = not (path.Contains("-Release"))
            if isOk && notRelease then 
                appendItems { changeType = edits
                              filePath = path }
            else id
        | _ -> pushCurrent

let stateMachine (s : ParserData) (l : string) = doStep l s.state s

[<EntryPoint>]
let main args = 
    let initialState = 
        { state = ParserState.LookingForStartOfRecord
          changes = List.empty
          current = 
              { user = ""
                items = List.empty } }
    File.ReadLines args.[0]
    |> Seq.map (fun x -> x.TrimEnd())
    |> Seq.fold stateMachine initialState
    |> fun s -> s.changes
    |> Seq.groupBy (fun x -> x.user)
    |> Seq.map (fun (k, x) -> 
           k, x |> Seq.length, 
           x
           |> Seq.collect (fun y -> y.items)
           |> Seq.length)
    |> Seq.sortBy (fun (_, c, _) -> -c)
    |> Seq.iter (fun (name, commits, edits) -> printfn "%s\t%d\t%d" name commits edits)
    0
