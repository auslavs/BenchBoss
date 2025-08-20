namespace BenchBossApp.Components.BenchBoss

[<AutoOpen>]
module Types =

  open System

  // Domain types
  type PlayerId = Guid

  type TeamPlayer = {
    Id: PlayerId
    Name: string
  }

  type GamePlayer = {
    Id: PlayerId
    Name: string
    PlayedSeconds: int
    BenchedSeconds: int
  }

  module GamePlayer =

    let addPlayed secs p = { p with PlayedSeconds = p.PlayedSeconds + secs }
    let addBenched secs p = { p with BenchedSeconds = p.BenchedSeconds + secs }

    let ofTeamPlayer (p: TeamPlayer) : GamePlayer =
      { Id = p.Id
        Name = p.Name
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

  // Modal types
  type ModalType =
    | OurTeamScoreModal
    | OpposingTeamScoreModal
    | AddPlayerModal
    | EditPlayerModal of TeamPlayer

  // Page navigation
  type Page =
    | GamePage
    | ManageTeamPage

  type State = {
    CurrentPage: Page
    TeamPlayers: TeamPlayer list
    GamePlayers: GamePlayer list
    FieldSlots: PlayerId option array
    Bench: PlayerId list
    CurrentHalf: Half
    ElapsedSecondsInHalf: int
    Timer: TimerStatus
    OurScore: int
    OppScore: int
    SelectedScorer: PlayerId option
    Events: ScoreEvent list
    LastTick: DateTime option
    CurrentModal: ModalType option
  }


  type Msg =
    | Tick
    | StartTimer
    | PauseTimer
    | StopTimer
    | StartNewHalf
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
  module Constants =
    [<Literal>]
    let StorageKey = "benchboss-state-v6"

  // Helpers
  module Time =
    let formatMMSS (seconds:int) =
      let m = seconds / 60
      let s = seconds % 60
      $"%02i{m}:%02i{s}"

  module TeamPlayer =
    let create name = { Id = Guid.NewGuid(); Name = name }

