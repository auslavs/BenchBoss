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
    let hideModal = fun _ -> dispatch HideModal
    let goalUs = GoalUs >> dispatch
    let goalThem = fun _ -> dispatch GoalThem

    // Check which modal to show
    let isHomeTeamModalOpen = 
      match state.CurrentModal with
      | Some OurTeamScoreModal -> true
      | _ -> false
    
    let isAwayTeamModalOpen = 
      match state.CurrentModal with
      | Some OpposingTeamScoreModal -> true
      | _ -> false

    Html.div [
      prop.className "flex flex-col min-h-screen"
      prop.children [
        Layout.render startTimer pauseTimer dispatch state

        // Page content based on current page
        match state.CurrentPage with
        | GamePage -> GamePage.render state dispatch
        | ManageTeamPage -> ManageTeamPage.render state dispatch

        ScoreModal.OurTeam.View(
          isHomeTeamModalOpen,
          state.GamePlayers,
          hideModal,
          goalUs
        )

        ScoreModal.OpposingTeam.View(
          isAwayTeamModalOpen,
          hideModal,
          goalThem
        )
      ]
    ]
