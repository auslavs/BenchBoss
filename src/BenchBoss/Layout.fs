namespace BenchBossApp.Components.BenchBoss

module Layout =
  open Feliz
  open BenchBossApp.Components.BenchBoss.Types

  let render (dispatch: Msg -> unit) (state: State) =
    Html.div [
      prop.className "flex flex-col"
      prop.children [
        // Global navigation bar
        BenchBossApp.Components.Navigation.NavBar(NavigateToPage >> dispatch, state.CurrentPage)

        // Hide scoreboard on Home / Manage Team when no active game (no players).
        let hasActiveGame = not (state.Game.Players |> List.isEmpty)
        let showScoreboard =
          match state.CurrentPage with
          | Page.HomePage | Page.ManageTeamPage -> hasActiveGame
          | _ -> true

        if showScoreboard then
          Html.div [
            prop.className "w-full bg-gradient-to-r from-indigo-600 via-purple-600 to-purple-700 text-white"
            prop.children [
              Html.div [
                prop.className "max-w-7xl mx-auto px-4 py-2 flex items-center justify-center relative"
                prop.children [
                  // Score centered
                  Html.div [
                    prop.className "flex items-center gap-6 text-3xl font-bold select-none"
                    prop.children [
                      Html.button [
                        prop.className "px-4 py-1 rounded-lg transition-colors hover:bg-purple-700/40 focus:outline-none focus:ring-2 focus:ring-white/50"
                        prop.text (string state.OurScore)
                        prop.onClick (fun _ -> dispatch (ShowModal OurTeamScoreModal))
                        prop.title "Add goal for our team"
                        prop.ariaLabel "Our team score"
                      ]
                      Html.span [ prop.className "opacity-80"; prop.text "-" ]
                      Html.button [
                        prop.className "px-4 py-1 rounded-lg transition-colors hover:bg-purple-700/40 focus:outline-none focus:ring-2 focus:ring-white/50"
                        prop.text (string state.OppScore)
                        prop.onClick (fun _ -> dispatch (ShowModal OpposingTeamScoreModal))
                        prop.title "Add goal for opposing team"
                        prop.ariaLabel "Opposing team score"
                      ]
                    ]
                  ]
                  // Timer pill positioned on right
                  Html.button [
                    prop.className "absolute right-4 top-1/2 -translate-y-1/2 bg-purple-600/40 backdrop-blur-sm px-6 py-2 rounded-full text-xl font-semibold tracking-wider hover:bg-purple-600/60 transition-colors focus:outline-none focus:ring-2 focus:ring-white/50"
                    prop.onClick (fun _ -> dispatch (ShowModal TimeManagerModal))
                    prop.text (Time.formatMMSS (State.getElapsedSecondsInHalf state))
                    prop.title "Manage game timer"
                    prop.ariaLabel "Game timer"
                  ]
                ]
              ]
            ]
          ]
        else Html.none
      ]
    ]
