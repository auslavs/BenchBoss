namespace BenchBossApp.Components.BenchBoss

open System
open Elmish
open BenchBossApp.Components.BenchBoss.Types

[<RequireQualifiedAccess>]
module State =

  let private validateState (state: State) : State =
    // Ensure Game Players only contains valid player IDs
    let validPlayerIds = state.TeamPlayers |> List.map _.Id |> Set.ofList
    let validGamePlayers = 
      state.Game.Players 
      |> List.filter (fun gp -> validPlayerIds.Contains gp.Id)

    let validatedGame = { state.Game with Players = validGamePlayers }
    
    { state with Game = validatedGame }


  let private empty () : State =
    {
      CurrentPage = GamePage
      TeamPlayers = []
      Game = Game.create "Default Game" []
      OurScore = 0
      OppScore = 0
      SelectedScorer = None
      Events = []
      LastTick = None
      CurrentModal = NoModal
    }

  let init () =
    match Store.loadFromStorage validateState () with
    | Some stored -> stored, Cmd.ofMsg Tick // kick a tick to normalize
    | None -> empty(), Cmd.none

  let private scheduleTickCmd : Cmd<Msg> =
    Cmd.OfAsync.perform (fun () -> async { do! Async.Sleep 1000 }) () (fun () -> Tick)

  let private isOnField (s:State) (pid:PlayerId) =
    s.Game.Players |> List.exists (fun p -> p.Id = pid && p.InGameStatus = OnField)

  let private isOnBench (s:State) (pid:PlayerId) = 
    s.Game.Players |> List.exists (fun p -> p.Id = pid && p.InGameStatus = OnBench)

  let isGamePlayer (pid:PlayerId) (s:State) =
    s.Game.Players |> List.exists (fun p -> p.Id = pid)

  let removeFromGamePlayers (pid:PlayerId) (s:State) =
    let updatedGame = Game.removePlayer pid s.Game
    { s with Game = updatedGame }

  let private addToGamePlayers (pid:PlayerId) (state:State) =
    if isGamePlayer pid state then
      state
    else
      match state.TeamPlayers |> List.tryFind (fun p -> p.Id = pid) with
      | Some p ->
          let updatedGame = Game.addPlayer p state.Game
          { state with Game = updatedGame }
      | None ->
          state

  let getElapsedSecondsInHalf (s:State) =
    match s.Game.Timer with
    | Running r -> r.Elapsed
    | Paused p -> p.Elapsed
    | Break b -> b.Elapsed
    | Stopped -> 0

  let getTimerStatus (s:State) = s.Game.Timer

  let private getCurrentHalf (s:State) =
    match s.Game.Timer with
    | Running r -> r.Half
    | Paused p -> p.Half
    | Break b -> b.Half
    | Stopped -> First

  let private accumulateSeconds (delta:int) (s:State) =
    if delta <= 0 then s
    else
      // Update played/benched time for all players
      let updatedPlayers =
        s.Game.Players
        |> List.map (fun p ->
            if p.InGameStatus = OnField then
              { p with PlayedSeconds = p.PlayedSeconds + delta }
            else
              { p with BenchedSeconds = p.BenchedSeconds + delta })
      
      // Update timer elapsed time
      let updatedTimer = 
        match s.Game.Timer with
        | Running r -> Running {| r with Elapsed = r.Elapsed + delta |}
        | Paused p -> Paused {| p with Elapsed = p.Elapsed + delta |}
        | Break b -> Break {| b with Elapsed = b.Elapsed + delta |}
        | Stopped -> Stopped
      
      let updatedGame = { s.Game with Players = updatedPlayers; Timer = updatedTimer }
      { s with Game = updatedGame }

  let private handleTick (state:State) =
    let now = DateTime.UtcNow
    match state.Game.Timer with
    | Running _r ->
        let delta =
          match state.LastTick with
          | Some last -> (now - last).TotalSeconds |> int |> max 0
          | None -> 0
        let s1 = { state with LastTick = Some now } |> accumulateSeconds delta
        s1, scheduleTickCmd
    | _ ->
        { state with LastTick = Some now }, Cmd.none

  let private startTimer s =
    match s.Game.Timer with
    | Running _ -> s, Cmd.none
    | _ ->
      let currentHalf = getCurrentHalf s
      let currentElapsed = getElapsedSecondsInHalf s
      let newTimer = Running {| Half = currentHalf; Elapsed = currentElapsed; LastTick = DateTime.UtcNow |}
      let updatedGame = { s.Game with Timer = newTimer }
      { s with Game = updatedGame; LastTick = Some DateTime.UtcNow; CurrentModal = NoModal }, scheduleTickCmd

  let private pauseTimer s =
    match s.Game.Timer with
    | Running r ->
        let newTimer = Paused {| Half = r.Half; Elapsed = r.Elapsed |}
        let updatedGame = { s.Game with Timer = newTimer }
        { s with Game = updatedGame; CurrentModal = NoModal }, Cmd.none
    | _ -> s, Cmd.none

  let private stopTimer s =
    let newTimer = Stopped
    let updatedGame = { s.Game with Timer = newTimer }
    let s1 = { s with Game = updatedGame; LastTick = None; CurrentModal = NoModal }
    s1, Cmd.none

  let private startNewHalf s =
    let newTimer = Stopped
    let updatedGame = { s.Game with Timer = newTimer }
    let s1 = { s with Game = updatedGame; LastTick = None }
    s1, Cmd.none

  let private endHalf s =
    match s.Game.Timer with
    | Running r when r.Half = First ->
        // End first half, transition to break/halftime
        let newTimer = Break {| Half = Second; Elapsed = 0 |}
        let updatedGame = { s.Game with Timer = newTimer }
        { s with Game = updatedGame; LastTick = None; CurrentModal = NoModal }, Cmd.none
    | Running r when r.Half = Second ->
        // End second half, stop the game
        let newTimer = Stopped
        let updatedGame = { s.Game with Timer = newTimer }
        { s with Game = updatedGame; LastTick = None; CurrentModal = NoModal }, Cmd.none
    | Break b ->
        // End break, move to second half stopped state
        let newTimer = Stopped
        let updatedGame = { s.Game with Timer = newTimer }
        { s with Game = updatedGame; LastTick = None; CurrentModal = NoModal }, Cmd.none
    | _ -> s, Cmd.none

  let private removeTeamPlayer id s =
    // Remove from team players and game players
    let players = s.TeamPlayers |> List.filter (fun p -> p.Id <> id)
    let updatedGame = Game.removePlayer id s.Game
    { 
      s with 
        TeamPlayers = players
        Game = updatedGame
        SelectedScorer = if s.SelectedScorer = Some id then None else s.SelectedScorer 
    }, Cmd.none

  let private updatePlayerName id name s =
    let teamPlayers = s.TeamPlayers |> List.map (fun p -> if p.Id = id then { p with Name = name } else p)
    let updatedPlayers = s.Game.Players |> List.map (fun p -> if p.Id = id then { p with Name = name } else p)
    let updatedGame = { s.Game with Players = updatedPlayers }
    { s with TeamPlayers = teamPlayers; Game = updatedGame }, Cmd.none

  let private dropOnField slot playerId state =
    // Get the current on-field players sorted by playing time (to match display order)
    let onFieldPlayers = 
      state.Game.Players 
      |> List.filter (fun p -> p.InGameStatus = OnField)
      |> List.sortBy (fun p -> p.PlayedSeconds)
    
    let playerInSlot = onFieldPlayers |> List.tryItem slot
    let draggedPlayer = state.Game.Players |> List.find (fun p -> p.Id = playerId)
    
    match playerInSlot with
    | Some existingPlayer when existingPlayer.Id = playerId ->
        // Player is already in this position, no change needed
        state, Cmd.none
    | Some existingPlayer ->
        // There's a different player in this position
        match draggedPlayer.InGameStatus with
        | OnField ->
            // Both players are on field - this is just a reordering, no action needed
            // since positions are determined by playing time
            state, Cmd.none
        | OnBench ->
            // Bench player dragged onto field player - swap them
            let updatedGame = Game.changePlayer playerId existingPlayer.Id state.Game
            { state with Game = updatedGame }, Cmd.none
    | None ->
        // Slot is empty, just put the player on field
        let updatedPlayers = 
          state.Game.Players 
          |> List.map (fun p -> if p.Id = playerId then { p with InGameStatus = OnField } else p)
        let updatedGame = { state.Game with Players = updatedPlayers }
        { state with Game = updatedGame }, Cmd.none

  // For now, dropping on bench means setting status to OnBench
  let private dropOnBench playerId s =
    let updatedPlayers = 
      s.Game.Players 
      |> List.map (fun p -> if p.Id = playerId then { p with InGameStatus = OnBench } else p)
    let updatedGame = { s.Game with Players = updatedPlayers }
    { s with Game = updatedGame }, Cmd.none

  // Goals
  let private ourGoal state (scorer: PlayerId option) =
    let currentHalf = getCurrentHalf state
    let elapsedSeconds = getElapsedSecondsInHalf state
    let ev = { TimeSeconds = elapsedSeconds; Half = currentHalf; OurTeam = true; Scorer = scorer }
    let s1 = { state with OurScore = state.OurScore + 1; Events = ev :: state.Events; SelectedScorer = None; CurrentModal = NoModal }
    s1, Cmd.none

  let private theirGoal state =
    let currentHalf = getCurrentHalf state
    let elapsedSeconds = getElapsedSecondsInHalf state
    let ev = { TimeSeconds = elapsedSeconds; Half = currentHalf; OurTeam = false; Scorer = None }
    let s1 = { state with OppScore = state.OppScore + 1; Events = ev :: state.Events; SelectedScorer = None; CurrentModal = NoModal }
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
    | EndHalf ->
        endHalf state
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
        { state with CurrentModal = modalType }, Cmd.none
    | HideModal ->
        { state with CurrentModal = NoModal }, Cmd.none
    | NavigateToPage page -> { state with CurrentPage = page }, Cmd.none
    | ConfirmAddTeamPlayer name ->
      match name.Trim() with
      | "" -> state, Cmd.none
      | trimmed ->
        let p = TeamPlayer.create trimmed
        let updatedGame = Game.addPlayer p state.Game
        let s1 = { 
          state with 
            TeamPlayers = state.TeamPlayers @ [p]
            Game = updatedGame
            CurrentModal = NoModal
        }
        s1, Cmd.none
    | ConfirmUpdateTeamPlayer updatedPlayer ->
      let trimmed = updatedPlayer.Name.Trim()
      if trimmed = "" then
        state, Cmd.none
      else
        let players = state.TeamPlayers |> List.map (fun p -> if p.Id = updatedPlayer.Id then { p with Name = trimmed } else p)
        let s1 = { state with TeamPlayers = players; CurrentModal = NoModal }
        s1, Cmd.none
    | ConfirmRemoveTeamPlayer id ->
        removeTeamPlayer id state
    | TogglePlayerGameAvailability playerId ->
        togglePlayerGameAvailability playerId state
    | ResetGame ->
      let newGame = Game.create "Default Game" []
      let newState = 
        { state with 
            CurrentPage = GamePage
            Game = newGame
            OurScore = 0
            OppScore = 0
            SelectedScorer = None
            Events = []
            LastTick = None
            CurrentModal = NoModal
        }
      Store.save newState
      newState, Cmd.none

  let updateWithSave (msg: Msg) (state: State) : State * Cmd<Msg> =
    let newState, cmd = update msg state
    Store.save newState
    newState, cmd
