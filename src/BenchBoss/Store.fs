namespace BenchBossApp.Components.BenchBoss

[<RequireQualifiedAccess>]
module Store =
  open Fable.Core
  open Thoth.Json

  let private defaultPlayers =
    [ "Alice"; "Bob"; "Cara"; "Dan"; "Eve"; "Finn" ]
    |> List.map TeamPlayer.create

  [<Emit("localStorage.getItem($0)")>]
  let private getItem (_k:string) : string = jsNative

  [<Emit("localStorage.setItem($0, $1)")>]
  let private setItem (_k:string, _v:string) : unit = jsNative

  let serialize (s: State) : string = Encode.Auto.toString(0, s)

  let tryDeserialize (json:string) : State option =
    try 
      let parsed = Decode.Auto.unsafeFromString<State> json
      let state = parsed |> unbox<State>

      if state.TeamPlayers.Length = 0 then
        Some { state with TeamPlayers = defaultPlayers}
      else
        Some state
      
    with 
    | ex -> 
      printfn $"Failed to deserialize state: %A{ex}"
      None

  let loadFromStorage validateState () : State option =
    match getItem Constants.StorageKey with
    | null -> 
      printfn "No stored state found"
      None
    | s ->
      match tryDeserialize s with
      | Some state ->
        let validatedState = validateState state
        Some validatedState
      | None ->
        None

  let save (s:State) = setItem(Constants.StorageKey, serialize s)