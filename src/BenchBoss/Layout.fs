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

        Html.div [
          prop.className "flex bg-purple-700 px-4 py-3 w-full text-white items-center"
          prop.children [
            // Left spacer (could hold future button)
            Html.div [ prop.className "w-10" ]

            // Center column with score + timer
            Html.div [
              prop.className "flex-1 flex flex-col items-center gap-2"
              prop.children [
                Html.div [
                  prop.className "bg-purple-600 px-6 py-2 rounded-lg flex items-center gap-4 text-3xl font-bold"
                  prop.children [
                    Html.button [
                      prop.className "hover:bg-purple-700 px-4 py-1 rounded-lg transition-colors"
                      prop.text (string state.OurScore)
                      prop.onClick (fun _ -> dispatch (ShowModal OurTeamScoreModal))
                      prop.title "Click to add score for our team"
                    ]
                    Html.span [ prop.className "opacity-80"; prop.text "-" ]
                    Html.button [
                      prop.className "hover:bg-purple-700 px-4 py-1 rounded-lg transition-colors"
                      prop.text (string state.OppScore)
                      prop.onClick (fun _ -> dispatch (ShowModal OpposingTeamScoreModal))
                      prop.title "Click to add score for opposing team"
                    ]
                  ]
                ]
                Html.button [
                  prop.className "text-3xl font-bold tracking-wider focus:outline-none"
                  prop.onClick (fun _ -> dispatch (ShowModal TimeManagerModal))
                  prop.text (Time.formatMMSS (State.getElapsedSecondsInHalf state))
                  prop.title "Manage game timer"
                ]
              ]
            ]
          ]
        ]
      ]
    ]
