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

            // Right side navigation button depending on page
            Html.div [
              prop.className "w-10 flex items-center justify-end"
              prop.children [
                match state.CurrentPage with
                | HomePage | GameSetupPage -> Html.none
                | GamePage ->
                  Html.button [
                    prop.className "bg-purple-600 hover:bg-purple-500 focus:ring-2 focus:ring-purple-300 text-white rounded-full w-10 h-10 inline-flex items-center justify-center shadow transition-colors"
                    prop.ariaLabel "Team Management"
                    prop.title "Manage Teams and Players"
                    prop.onClick (fun _ -> dispatch (NavigateToPage ManageTeamPage))
                    prop.children [ Html.img [ prop.src "assets/team.svg"; prop.alt "Team Management"; prop.className "w-5 h-5" ] ]
                  ]
                | ManageTeamPage ->
                  Html.button [
                    prop.className "bg-purple-600 hover:bg-purple-500 focus:ring-2 focus:ring-purple-300 text-white rounded-full w-10 h-10 inline-flex items-center justify-center shadow transition-colors"
                    prop.ariaLabel "Back to Game"
                    prop.title "Back to Soccer Game"
                    prop.onClick (fun _ -> dispatch (NavigateToPage GamePage))
                    prop.children [ Html.img [ prop.src "assets/soccer.svg"; prop.alt "Soccer Game"; prop.className "w-5 h-5" ] ]
                  ]
              ]
            ]
          ]
        ]
      ]
    ]


    
