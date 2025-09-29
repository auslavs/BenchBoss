namespace BenchBossApp.Components.BenchBoss

module Component =
  open Feliz
  open Feliz.UseElmish
  open BenchBossApp.Components.BenchBoss.Types
  open BenchBossApp.Components.BenchBoss
  [<ReactComponent>]
  let Render () =
    let state, dispatch = React.useElmish(State.init, State.updateWithSave)

    let startTimer = fun _ -> dispatch StartTimer
    let pauseTimer = fun _ -> dispatch PauseTimer
    let stopTimer = fun _ -> dispatch StopTimer
    let endHalf = fun _ -> dispatch EndHalf
    let startNewHalf = fun _ -> dispatch StartNewHalf
    let hideModal = fun _ -> dispatch HideModal
    let goalUs = GoalUs >> dispatch
    let goalThem = fun _ -> dispatch GoalThem

    Html.div [
      prop.className "flex flex-col min-h-screen"
      prop.children [
        Layout.render dispatch state

        // Page content based on current page
        match state.CurrentPage with
        | LandingPage -> LandingPage.render state dispatch
        | StartGamePage -> StartGamePage.render state dispatch
        | GamePage -> GamePage.render state dispatch
        | ManageTeamPage -> ManageTeamPage.render state dispatch

        Modals.OurTeam.View(
          state.CurrentModal = OurTeamScoreModal,
          state.Game.Players,
          hideModal,
          goalUs
        )

        Modals.OpposingTeam.View(
          state.CurrentModal = OpposingTeamScoreModal,
          hideModal,
          goalThem
        )

        Modals.TimeManager.View(
          state.CurrentModal = TimeManagerModal,
          hideModal,
          startTimer,
          pauseTimer,
          stopTimer,
          endHalf,
          startNewHalf,
          state.Game.Timer
        )
      ]
    ]
