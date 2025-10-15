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
    | StartNewGame of starting: PlayerId list * bench: PlayerId list
  | SetFieldPlayerTarget of int

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

