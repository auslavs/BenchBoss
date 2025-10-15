namespace BenchBossApp.Components.BenchBoss

module Component =
  open Feliz
  open Feliz.UseElmish
  open BenchBossApp.Components.BenchBoss.Types
  open BenchBossApp.Components.BenchBoss
  open Feliz.Router
  open Browser.Dom
  [<ReactComponent>]
  let Render () =
    let state, dispatch = React.useElmish(State.init, State.updateWithSave)

    // --- Routing helpers (inlined) ---
    let pageSegments = function
      | HomePage -> [ "" ]
      | GamePage -> [ "game" ]
      | ManageTeamPage -> [ "manage-team" ]
      | GameSetupPage -> [ "game-setup" ]

    let pageHref p = pageSegments p |> Router.format

    let parseUrl segments =
      match segments with
      | []
      | ["" ] -> HomePage
      | [ "game" ] -> GamePage
      | [ "manage-team" ] -> ManageTeamPage
      | [ "game-setup" ] -> GameSetupPage
      | _ -> HomePage

    let currentPageFromUrl () = Router.currentPath() |> List.map (fun s -> s.Trim().ToLower()) |> parseUrl

    // On initial mount, align state with URL
    React.useEffectOnce(fun () ->
      let initial = currentPageFromUrl()
      if initial <> state.CurrentPage then dispatch (NavigateToPage initial)
      ())

    // Listen to future url changes (back/forward) via popstate
    React.useEffectOnce(fun () ->
      let handler = fun (_:obj) ->
        let p = currentPageFromUrl()
        if p <> state.CurrentPage then dispatch (NavigateToPage p)
      window.addEventListener("popstate", handler)
      { new System.IDisposable with member _.Dispose() = window.removeEventListener("popstate", handler) })

    // Push state.CurrentPage to URL when it changes
    React.useEffect((fun () ->
      let desired = pageHref state.CurrentPage
      let current = Router.currentPath() |> Router.format
      if current <> desired then Router.navigatePath(desired)
      ), [| box state.CurrentPage |])

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
        (match state.CurrentPage with
         | GamePage -> GamePage.render state dispatch
         | ManageTeamPage -> ManageTeamPage.render state dispatch
         | GameSetupPage -> BenchBossApp.Components.BenchBoss.GameSetupPage.View state dispatch
         | HomePage -> BenchBossApp.Components.BenchBoss.HomePage.View state dispatch)

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
