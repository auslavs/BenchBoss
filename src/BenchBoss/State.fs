namespace BenchBossApp.Components.BenchBoss

open System
open Elmish
open BenchBossApp.Components.BenchBoss.Types

[<RequireQualifiedAccess>]
module State =

  let private validateState (state: State) : State =
    // Ensure GamePlayer only contains valid player IDs
    let validPlayerIds = state.TeamPlayers |> List.map _.Id |> Set.ofList
    let validAvailablePlayers = 
      state.GamePlayers 
      |> List.filter (fun gp -> validPlayerIds.Contains gp.Id)

    // Ensure field slots only contain valid available player IDs
    let validFieldSlots = 
      state.FieldSlots 
      |> Array.map (function 
          | Some id when validPlayerIds.Contains id -> Some id 
          | _ -> None)
    
    // Ensure bench only contains valid available player IDs
    let validBench = 
      state.Bench 
      |> List.filter validPlayerIds.Contains
    
    { state with 
        GamePlayers = validAvailablePlayers
        FieldSlots = validFieldSlots  
        Bench = validBench }


  let private empty () : State =
    {
      CurrentPage = GamePage
      TeamPlayers = []
      GamePlayers = []
      FieldSlots = [| None; None; None; None |]
      Bench = []
      CurrentHalf = First
      ElapsedSecondsInHalf = 0
      Timer = Stopped
      OurScore = 0
      OppScore = 0
      SelectedScorer = None
      Events = []
      LastTick = None
      CurrentModal = None
    }

  let init () =
    match Store.loadFromStorage validateState () with
    | Some stored -> stored, Cmd.ofMsg Tick // kick a tick to normalize
    | None -> empty(), Cmd.none

  let private scheduleTickCmd : Cmd<Msg> =
    Cmd.OfAsync.perform (fun () -> async { do! Async.Sleep 1000 }) () (fun () -> Tick)

  let private isOnField (s:State) (pid:PlayerId) =
    s.FieldSlots |> Array.exists (function Some id when id = pid -> true | _ -> false)

  let private isOnBench (s:State) (pid:PlayerId) = s.Bench |> List.contains pid

  let private addToBench (pid:PlayerId) (s:State) =
    if isOnBench s pid then s else { s with Bench = pid :: s.Bench }

  let private removeFromBench (pid:PlayerId) (s:State) =
    { s with Bench = s.Bench |> List.filter ((<>) pid) }

  let removeFromField (pid:PlayerId) (s:State) =
    let rec loop slots acc =
      match slots with
      | [] -> List.rev acc
      | x :: xs when x = Some pid -> loop xs (None :: acc)
      | x :: xs -> loop xs (x :: acc)
    { s with FieldSlots = loop (Array.toList s.FieldSlots) [] |> Array.ofList }

  let private placeInFieldSlot (slot:int) (pidOpt:PlayerId option) (s:State) =
    let slots = Array.copy s.FieldSlots
    slots[slot] <- pidOpt
    { s with FieldSlots = slots }

  let isGamePlayer (pid:PlayerId) (s:State) =
    s.GamePlayers |> List.exists (fun p -> p.Id = pid)

  let removeFromGamePlayers (pid:PlayerId) (s:State) =
    { s with GamePlayers = s.GamePlayers |> List.filter (fun p -> p.Id <> pid) }
    |> removeFromBench pid
    |> removeFromField pid

  let private addToGamePlayers (pid:PlayerId) (state:State) =
    if isGamePlayer pid state then
      state
    else
      match state.TeamPlayers |> List.tryFind (fun p -> p.Id = pid) with
      | Some p ->
          let gamePlayer = GamePlayer.ofTeamPlayer p
          { state with GamePlayers = gamePlayer :: state.GamePlayers }
          |> addToBench pid
      | None ->
          state

  let private indexOfPlayerSlot (s:State) (pid:PlayerId) =
    s.FieldSlots |> Array.tryFindIndex ((=) (Some pid))

  let private accumulateSeconds (delta:int) (s:State) =
    if delta <= 0 then s
    else
      // add played to field players, benched to bench players
      let fieldIds = s.FieldSlots |> Array.choose id |> Set.ofArray
      let benchIds = s.Bench |> Set.ofList
      let updated =
        s.GamePlayers
        |> List.map (fun p ->
            if fieldIds.Contains p.Id then GamePlayer.addPlayed delta p
            elif benchIds.Contains p.Id then GamePlayer.addBenched delta p
            else p)
      { s with GamePlayers = updated; ElapsedSecondsInHalf = s.ElapsedSecondsInHalf + delta }

  let private handleTick (state:State) =
    let now = DateTime.UtcNow
    match state.Timer with
    | Running ->
        let delta =
          match state.LastTick with
          | Some last -> (now - last).TotalSeconds |> int |> max 0
          | None -> 0
        let s1 = { state with LastTick = Some now } |> accumulateSeconds delta
        s1, scheduleTickCmd
    | _ ->
        { state with LastTick = Some now }, Cmd.none
  let private startTimer s =
    match s.Timer with
    | Running -> s, Cmd.none
    | _ ->
      { s with Timer = Running; LastTick = Some DateTime.UtcNow }, scheduleTickCmd

  let private pauseTimer s =
    { s with Timer = Paused }, Cmd.none

  let private stopTimer s =
    let s1 = { s with Timer = Stopped; LastTick = None }
    s1, Cmd.none

  let private startNewHalf s =
    let nextHalf = match s.CurrentHalf with First -> Second | Second -> First
    let s1 = { s with CurrentHalf = nextHalf; ElapsedSecondsInHalf = 0; Timer = Stopped; LastTick = None }
    s1, Cmd.none

  let private removeTeamPlayer id s =
    // Remove from players, field, bench, and game available
    let players = s.TeamPlayers |> List.filter (fun p -> p.Id <> id)
    let gameAvailable = s.GamePlayers |> List.filter (fun p -> p.Id <> id)
    let slots = s.FieldSlots |> Array.map (function Some pid when pid = id -> None | x -> x)
    let bench = s.Bench |> List.filter ((<>) id)
    { 
      s with 
        TeamPlayers = players
        GamePlayers = gameAvailable
        FieldSlots = slots
        Bench = bench
        SelectedScorer = if s.SelectedScorer = Some id then None else s.SelectedScorer 
    }, Cmd.none

  let private updatePlayerName id name s =
    let teamPlayers = s.TeamPlayers |> List.map (fun p -> if p.Id = id then { p with Name = name } else p)
    let gamePlayers = s.GamePlayers |> List.map (fun p -> if p.Id = id then { p with Name = name } else p)
    { s with TeamPlayers = teamPlayers; GamePlayers = gamePlayers }, Cmd.none

  let private dropOnField slot playerId state =

    if isOnField state playerId then
      // No-op, player is already on the field
      state, Cmd.none

    // If player is on the bench, remove them from bench and place in field slot
    elif isOnBench state playerId then
      let currentAtSlot = state.FieldSlots[slot]
      match currentAtSlot with
      | Some currentPlayerId ->
          state
          |> addToBench currentPlayerId
          |> placeInFieldSlot slot (Some playerId), Cmd.none
      | None ->
          state
          |> placeInFieldSlot slot (Some playerId)
          |> removeFromBench playerId, Cmd.none
    else
      // Player is not on the field or bench, this should not happen, so we do nothing
      state, Cmd.none

  // Dropping a player onto bench means remove from their field slot (if any) and add to bench
  let private dropOnBench playerId s =
    let s1 =
      match indexOfPlayerSlot s playerId with
      | Some i -> placeInFieldSlot i None s
      | None -> s
    let s2 = if isOnBench s1 playerId then s1 else { s1 with Bench = s1.Bench @ [playerId] }
    s2, Cmd.none

  // Goals
  let private ourGoal state (scorer: PlayerId option) =
    let ev = { TimeSeconds = state.ElapsedSecondsInHalf; Half = state.CurrentHalf; OurTeam = true; Scorer = scorer }
    let s1 = { state with OurScore = state.OurScore + 1; Events = ev :: state.Events; SelectedScorer = None; CurrentModal = None }
    s1, Cmd.none

  let private theirGoal state =
    let ev = { TimeSeconds = state.ElapsedSecondsInHalf; Half = state.CurrentHalf; OurTeam = false; Scorer = None }
    let s1 = { state with OppScore = state.OppScore + 1; Events = ev :: state.Events; SelectedScorer = None; CurrentModal = None }
    s1, Cmd.none

  let togglePlayerGameAvailability playerId state =
    if isGamePlayer playerId state then
      removeFromGamePlayers playerId state, Cmd.none
    else
      addToGamePlayers playerId state, Cmd.none

  let update msg state : State * Cmd<Msg> =
    printfn $"Update: %A{msg}"
    match msg with
    | Tick ->
        handleTick state
    | StartTimer ->
        startTimer state
    | PauseTimer ->
        pauseTimer state
    | StopTimer ->
        stopTimer state
    | StartNewHalf ->
        startNewHalf state
    | UpdatePlayerName (id, name) ->
        updatePlayerName id name state
    | DropOnField (slot, pid) ->
        dropOnField slot pid state
    | DropOnBench pid ->
        dropOnBench pid state
    | GoalUs scorer ->
        ourGoal state scorer
    | GoalThem ->
        theirGoal state
    | LoadFromStorage s ->
        s, Cmd.none
    | ShowModal modalType ->
        { state with CurrentModal = Some modalType }, Cmd.none
    | HideModal ->
        { state with CurrentModal = None; }, Cmd.none
    | NavigateToPage page -> { state with CurrentPage = page }, Cmd.none
    | ConfirmAddTeamPlayer name ->
      match name.Trim() with
      | "" -> state, Cmd.none
      | trimmed ->
        let p = TeamPlayer.create trimmed
        let s1 = { 
          state with 
            TeamPlayers = state.TeamPlayers @ [p]
            Bench = state.Bench @ [p.Id]
            CurrentModal = None
        }
        s1, Cmd.none
    | ConfirmUpdateTeamPlayer updatedPlayer ->
      let trimmed = updatedPlayer.Name.Trim()
      if trimmed = "" then
        state, Cmd.none
      else
        let players = state.TeamPlayers |> List.map (fun p -> if p.Id = updatedPlayer.Id then { p with Name = trimmed } else p)
        let s1 = { state with TeamPlayers = players; CurrentModal = None }
        s1, Cmd.none
    | ConfirmRemoveTeamPlayer id ->
        removeTeamPlayer id state
    | TogglePlayerGameAvailability playerId ->
        togglePlayerGameAvailability playerId state
    | ResetGame ->
      let newState = 
        { state with 
            CurrentPage = GamePage
            GamePlayers = []
            FieldSlots = [| None; None; None; None |]
            Bench = []
            CurrentHalf = First
            ElapsedSecondsInHalf = 0
            Timer = Stopped
            OurScore = 0
            OppScore = 0
            SelectedScorer = None
            Events = []
            LastTick = None
            CurrentModal = None
        }
      Store.save newState
      newState, Cmd.none

  let updateWithSave (msg: Msg) (state: State) : State * Cmd<Msg> =
    let newState, cmd = update msg state
    Store.save newState
    newState, cmd
