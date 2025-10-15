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

      // Migration / normalization: ensure FieldPlayerTarget in bounds
      let normalizedFieldTarget =
        if state.FieldPlayerTarget < 4 then 4
        elif state.FieldPlayerTarget > 11 then 11
        else state.FieldPlayerTarget

      let normalizedState = { state with FieldPlayerTarget = normalizedFieldTarget }

      if normalizedState.TeamPlayers.Length = 0 then
        Some { normalizedState with TeamPlayers = defaultPlayers}
      else
        Some normalizedState
      
    with 
    | ex -> 
      printfn $"Failed to deserialize state: %A{ex}"
      None

  let loadFromStorage validateState () : State option =
    // Clean up legacy beta key (v5) if present – we intentionally drop it during beta
    let legacyV5Key = "benchboss-state-v5"
    let legacy = getItem legacyV5Key
    if not (isNull legacy) then
      // Remove it by overwriting with empty string (cannot call removeItem via Emit atm)
      setItem(legacyV5Key, "")
      printfn "Legacy v5 state detected and cleared."

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