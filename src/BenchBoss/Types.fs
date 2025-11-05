namespace BenchBossApp.Components.BenchBoss

[<AutoOpen>]
module Types =

  open System

  type PlayerId = Guid

  type TeamPlayer = {
    Id: PlayerId
    Name: string
  }

  type InGameStatus =
    | OnField
    | OnBench

  type GamePlayer = {
    Id: PlayerId
    Name: string
    InGameStatus: InGameStatus
    PlayedSeconds: int
    BenchedSeconds: int
  }

  module GamePlayer =
    let ofTeamPlayer (p: TeamPlayer) : GamePlayer =
      { Id = p.Id
        Name = p.Name
        InGameStatus = OnBench
        PlayedSeconds = 0
        BenchedSeconds = 0 }

  type TimerStatus =
    | Running
    | Paused
    | Stopped

  type Half =
    | First
    | Second

  type ScoreEvent = {
    TimeSeconds: int
    Half: Half
    OurTeam: bool
    Scorer: PlayerId option
  }

  type ModalType =
    | NoModal
    | TimeManagerModal
    | OurTeamScoreModal
    | OpposingTeamScoreModal
    | AddPlayerModal
    | EditPlayerModal of TeamPlayer

  type Page =
    | HomePage
    | GamePage
    | ManageTeamPage
    | GameSetupPage
    | NotFoundPage

  [<RequireQualifiedAccess>]
  module Page =
    let toString = function
      | HomePage -> "HomePage"
      | GamePage -> "GamePage"
      | ManageTeamPage -> "ManageTeamPage"
      | GameSetupPage -> "GameSetupPage"
      | NotFoundPage -> "NotFoundPage"

  type RunningTimerState =
    {| Half: Half; Elapsed: int; LastTick: DateTime |}

  type PausedTimerState =
    {| Half: Half; Elapsed: int |}

  type BreakTimerState =
    {| Half: Half; Elapsed: int |}

  type Timer =
    | Running of RunningTimerState
    | Paused of PausedTimerState
    | Break of BreakTimerState
    | Stopped

    type AddToFieldError = 
    | FieldFull
    | PlayerAlreadyOnField

  type TeamSetupData = private  {
    onField: Map<PlayerId, TeamPlayer>
    onBench: Map<PlayerId, TeamPlayer>
    maxOnField: int
  }

  with 
    member this.OnField = this.onField
    member this.OnBench = this.onBench
    member this.MaxOnField = this.maxOnField

  module TeamSetupData =
    open FsToolkit.ErrorHandling
    let create maxOnField selectedPlayers =
      { onField = Map.empty
        onBench = selectedPlayers
        maxOnField = maxOnField }

    let addToField (player: TeamPlayer) (team: TeamSetupData) = result {
      let playerId = player.Id
      if Map.containsKey playerId team.onField then
        return! Error PlayerAlreadyOnField
      else
        let currentFieldSize = Map.count team.onField
        if currentFieldSize >= team.maxOnField then
          return! Error FieldFull
        else
          return
            { team with 
                onField = Map.add playerId player team.onField
                onBench = Map.remove playerId team.onBench }
    }

    let addToBench (player: TeamPlayer) (team: TeamSetupData) =
      let playerId = player.Id
      { team with 
          onField = Map.remove playerId team.onField
          onBench = Map.add playerId player team.onBench }

    let isFieldFull team =
        Map.count team.onField >= team.maxOnField

    let setMaxOnField newMax team =
        // ensure we don't have more players on field than the new max
      let adjustedOnField, adjustedOnBench =
        if Map.count team.onField <= newMax then
          team.onField, team.onBench
        else
          let onFieldList = Map.toList team.onField
          let toKeep, toMove = List.splitAt newMax onFieldList
          
          let updatedOnField = Map.ofList toKeep
          let updatedOnBench = 
            (team.onBench, toMove)
            ||> List.fold (fun acc (_, player) -> Map.add player.Id player acc)
          
          updatedOnField, updatedOnBench

      { onField = adjustedOnField; onBench = adjustedOnBench; maxOnField = newMax }

  type Game = {
    Id: Guid
    Name: string
    Players: GamePlayer list
    ElapsedSecondsInHalf: int
    Timer: Timer
  }

  module Game =

    let create name players =
      { Id = Guid.NewGuid()
        Name = name
        Players = players
        ElapsedSecondsInHalf = 0
        Timer = Stopped }

    let addPlayer (teamPlayer: TeamPlayer) game =
      let gamePlayer = GamePlayer.ofTeamPlayer teamPlayer
      { game with Players = gamePlayer :: game.Players }

    let removePlayer (playerId: PlayerId) game =
      { game with Players = List.filter (fun p -> p.Id <> playerId) game.Players }

    let changePlayer (benchedPlayer: PlayerId) (onFieldPlayer: PlayerId) game =
      let updatedPlayers =
        game.Players |> List.fold(
          fun (acc: GamePlayer list) (p: GamePlayer) ->
            if p.Id = benchedPlayer then
              { p with InGameStatus = OnField } :: acc
            elif p.Id = onFieldPlayer then
              { p with InGameStatus = OnBench } :: acc
            else
              p :: acc
        ) []
      { game with Players = List.rev updatedPlayers }


  type State = {
    CurrentPage: Page
    TeamPlayers: TeamPlayer list
    Game: Game
    FieldPlayerTarget: int
    OurScore: int
    OppScore: int
    Events: ScoreEvent list
    LastTick: DateTime option
    CurrentModal: ModalType
  }


  type Msg =
    | Tick
    | StartTimer
    | PauseTimer
    | StopTimer
    | StartNewHalf
    | EndHalf
    | UpdatePlayerName of PlayerId * string
    | DropOnField of targetSlot:int * playerId:PlayerId
    | DropOnBench of playerId:PlayerId
    | GoalUs of PlayerId option
    | GoalThem
    | LoadFromStorage of State
    | ShowModal of ModalType
    | HideModal
    | NavigateToPage of Page
    | ConfirmAddTeamPlayer of string
    | ConfirmUpdateTeamPlayer of TeamPlayer
    | ConfirmRemoveTeamPlayer of PlayerId
    | TogglePlayerGameAvailability of PlayerId
    | ResetGame
    | StartNewGame of TeamSetupData

  module Constants =
    [<Literal>]
    let StorageKey = "benchboss-state-v7"

  // Helpers
  module Time =
    let formatMMSS (seconds:int) =
      let m = seconds / 60
      let s = seconds % 60
      $"%02i{m}:%02i{s}"

  module TeamPlayer =
    let create name = { Id = Guid.NewGuid(); Name = name }

