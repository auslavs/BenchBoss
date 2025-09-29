namespace BenchBossApp.Components.BenchBoss

open BenchBossApp.Components.BenchBoss.Types

module LandingPage =

  let render (state: State) (dispatch: Msg -> unit) =
    GameSetupView.render state dispatch
