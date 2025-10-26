namespace BenchBossApp.Components.BenchBoss

module Component =
  open Feliz
  open Feliz.UseElmish
  open BenchBossApp.Components.BenchBoss.Types
  open BenchBossApp.Components.BenchBoss
  open Browser.Dom
  [<ReactComponent>]
  let Render () =
    let initialPage = Routing.currentPageFromUrl()
    let state, dispatch = React.useElmish(State.init initialPage, State.updateWithSave)

    // Centralized routing via Routing module
    let currentPageFromUrl () = Routing.currentPageFromUrl()

    // Listen to future url changes (back/forward) via popstate
    React.useEffectOnce(fun () ->
      Browser.Dom.console.log("Setting up popstate listener")
      let handler = fun (_:obj) ->
        let p = currentPageFromUrl()
        if p <> state.CurrentPage then dispatch (NavigateToPage p)
      window.addEventListener("popstate", handler)
      { new System.IDisposable with member _.Dispose() = window.removeEventListener("popstate", handler) })

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
        match state.CurrentPage with
         | GamePage -> GamePage.RenderGamePage state dispatch
         | ManageTeamPage -> ManageTeamPage.render state dispatch
         | GameSetupPage ->
             GameSetupPage.View
              {| TeamPlayers = state.TeamPlayers;
                 GamePlayers = state.Game.Players;
                 FieldPlayerTarget = state.FieldPlayerTarget;
                 Cancel = fun () -> dispatch (NavigateToPage HomePage);
                 SetFieldPlayerTarget = fun v -> dispatch (SetFieldPlayerTarget v);
                 StartNewGame = fun (starting, bench) -> dispatch (StartNewGame (starting, bench)) |}
         | HomePage -> HomePage.View state dispatch
         | NotFoundPage -> NotFoundPage.View dispatch

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
