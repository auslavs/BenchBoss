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
      FieldPlayerTarget = 4
      OurScore = 0
      OppScore = 0
      Events = []
      LastTick = None
      CurrentModal = NoModal
    }

  let init (initialPage: Page) =
    Browser.Dom.console.log("Initializing state with page:", Page.toString initialPage)
    match Store.loadFromStorage validateState () with
    | Some stored -> { stored with CurrentPage = initialPage }, Cmd.ofMsg Tick // kick a tick to normalize
    | None -> { empty() with CurrentPage = initialPage }, Cmd.none

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
    let players = s.TeamPlayers |> List.filter (fun p -> p.Id <> id)
    let updatedGame = Game.removePlayer id s.Game
    { s with TeamPlayers = players; Game = updatedGame }, Cmd.none

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
    let s1 = { state with OurScore = state.OurScore + 1; Events = ev :: state.Events; CurrentModal = NoModal }
    s1, Cmd.none

  let private theirGoal state =
    let currentHalf = getCurrentHalf state
    let elapsedSeconds = getElapsedSecondsInHalf state
    let ev = { TimeSeconds = elapsedSeconds; Half = currentHalf; OurTeam = false; Scorer = None }
    let s1 = { state with OppScore = state.OppScore + 1; Events = ev :: state.Events; CurrentModal = NoModal }
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
        // NOTE: We intentionally do NOT auto-add a newly created player to the current game roster.
        // Previously we called Game.addPlayer here which meant simply creating a player implicitly
        // made them available for (and visible in) an in-progress or upcoming game. This was confusing
        // and looked like "adding a player starts a game" to the user. New players should remain
        // off the game roster until explicitly:
        //  1. Toggled available on the Manage Team page (checkbox), OR
        //  2. Included during Game Setup and StartNewGame.
        let p = TeamPlayer.create trimmed
        let s1 = { 
          state with 
            TeamPlayers = state.TeamPlayers @ [p]
            // Game unchanged; player not auto-added
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
      // Preserve current page so if user ends game from Home they remain there
      let newState = 
        { state with 
            Game = newGame
            OurScore = 0
            OppScore = 0
            Events = []
            LastTick = None
            CurrentModal = NoModal
        }
      Store.save newState
      newState, Cmd.none
    | SetFieldPlayerTarget target ->
      let clamped = target |> max 4 |> min 11
      // If reducing target, bench excess on-field players (choose by highest PlayedSeconds to bench first?)
      let onFieldPlayers = state.Game.Players |> List.filter (fun p -> p.InGameStatus = OnField)
      let excess = onFieldPlayers.Length - clamped
      let updatedPlayers =
        if excess > 0 then
          let toBench =
            onFieldPlayers
            |> List.sortByDescending (fun p -> p.PlayedSeconds) // bench those who've played most to balance
            |> List.take excess
            |> List.map (fun p -> p.Id)
            |> Set.ofList
          state.Game.Players |> List.map (fun p -> if toBench.Contains p.Id then { p with InGameStatus = OnBench } else p)
        else state.Game.Players
      let newState = { state with FieldPlayerTarget = clamped; Game = { state.Game with Players = updatedPlayers } }
      Store.save newState
      newState, Cmd.none
    | StartNewGame (starting, bench) ->
      // Enforce FieldPlayerTarget at game start: if more starters are passed than allowed, overflow moves to bench.
      let maxStarting = state.FieldPlayerTarget
      let trimmedStarting, overflow =
        if List.length starting > maxStarting then starting |> List.splitAt maxStarting else starting, []
      // Combine provided bench with any overflow starters.
      let combinedBench = bench @ overflow
      let toGamePlayer status (tp:TeamPlayer) =
        { Id = tp.Id; Name = tp.Name; InGameStatus = status; PlayedSeconds = 0; BenchedSeconds = 0 }
      let startingSet = trimmedStarting |> Set.ofList
      let benchSet = combinedBench |> Set.ofList
      // Filter out any ids not in roster; remove duplicates and ensure no overlap.
      let rosterIds = state.TeamPlayers |> List.map _.Id |> Set.ofList
      let validStarting = startingSet |> Set.filter (fun id -> rosterIds.Contains id)
      let validBench = benchSet |> Set.filter (fun id -> rosterIds.Contains id && not (validStarting.Contains id))
      let gamePlayers =
        state.TeamPlayers
        |> List.choose (fun tp ->
            if validStarting.Contains tp.Id then Some (toGamePlayer OnField tp)
            elif validBench.Contains tp.Id then Some (toGamePlayer OnBench tp)
            else None)
      let newGame = { Game.create "Game" gamePlayers with Timer = Stopped }
      let newState = { state with Game = newGame; OurScore = 0; OppScore = 0; Events = []; CurrentPage = GamePage; LastTick = None; CurrentModal = NoModal }
      newState, Cmd.none

  let updateWithSave (msg: Msg) (state: State) : State * Cmd<Msg> =
    let newState, cmd = update msg state
    Store.save newState
    newState, cmd
