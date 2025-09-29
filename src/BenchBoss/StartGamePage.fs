namespace BenchBossApp.Components.BenchBoss

open BenchBossApp.Components.BenchBoss.Types

module StartGamePage =

  let render (state: State) (dispatch: Msg -> unit) =
    GameSetupView.render state dispatch
