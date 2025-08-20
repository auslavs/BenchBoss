namespace BenchBossApp.Components.BenchBoss

module Layout =
  open Browser.Types
  open Feliz
  open BenchBossApp.Components.BenchBoss.Types

  let playButton startTimer =
    Html.button [
      prop.className "bg-purple-500 hover:bg-purple-600 focus:ring-2 focus:ring-purple-300 text-white rounded-full w-10 h-10 inline-flex items-center justify-center shadow";
      prop.ariaLabel "Play"
      prop.onClick startTimer
      prop.children [
        Html.img [
            prop.src "assets/play.svg";
            prop.alt "Play";
            prop.className "w-5 h-5"
          ]
      ]
    ]

  let pauseButton pauseTimer =
    Html.button [
      prop.className "bg-purple-500 hover:bg-purple-600 focus:ring-2 focus:ring-purple-300 text-white rounded-full w-10 h-10 inline-flex items-center justify-center shadow";
      prop.ariaLabel "Play"
      prop.onClick pauseTimer
      prop.children [
        Html.img [
            prop.src "assets/pause.svg";
            prop.alt "Pause";
            prop.className "w-5 h-5"
          ]
      ]
    ]

  let render (startTimer: MouseEvent -> unit) (pauseTimer: MouseEvent -> unit) (dispatch: Msg -> unit) (state: State) =
    Html.div [
      prop.className "flex flex-col"
      prop.children [
        Html.div [
          prop.className "flex bg-purple-700 p-2 text-3xl font-bold w-full text-center text-white items-center justify-between"
          prop.children [
            // Empty div for left spacing
            Html.div [
              prop.className "w-10"
            ]
            
            Html.div [
              prop.className "bg-purple-600 p-2 rounded-lg flex items-center gap-2"
              prop.children [
                // Our score (clickable)
                Html.button [
                  prop.className "hover:bg-purple-700 px-4 py-2 rounded-lg transition-colors"
                  prop.text (string state.OurScore)
                  prop.onClick (fun _ -> dispatch (ShowModal OurTeamScoreModal))
                  prop.title "Click to add score for our team"
                ]
                Html.span [
                  prop.className "mx-2"
                  prop.text "-"
                ]
                // Their score (clickable)
                Html.button [
                  prop.className "hover:bg-purple-700 px-4 py-2 rounded-lg transition-colors"
                  prop.text (string state.OppScore)
                  prop.onClick (fun _ -> dispatch (ShowModal OpposingTeamScoreModal))
                  prop.title "Click to add score for opposing team"
                ]
              ]
            ]
            
            // Navigation icon in top right (changes based on current page)
            Html.div [
              prop.className "flex items-center"
              prop.children [
                match state.CurrentPage with
                | GamePage ->
                  Html.button [
                    prop.className "bg-purple-600 hover:bg-purple-500 focus:ring-2 focus:ring-purple-300 text-white rounded-full w-10 h-10 inline-flex items-center justify-center shadow transition-colors"
                    prop.ariaLabel "Team Management"
                    prop.title "Manage Teams and Players"
                    prop.onClick (fun _ -> dispatch (NavigateToPage ManageTeamPage))
                    prop.children [
                      Html.img [
                        prop.src "assets/team.svg"
                        prop.alt "Team Management"
                        prop.className "w-5 h-5"
                      ]
                    ]
                  ]
                | ManageTeamPage ->
                  Html.button [
                    prop.className "bg-purple-600 hover:bg-purple-500 focus:ring-2 focus:ring-purple-300 text-white rounded-full w-10 h-10 inline-flex items-center justify-center shadow transition-colors"
                    prop.ariaLabel "Back to Game"
                    prop.title "Back to Soccer Game"
                    prop.onClick (fun _ -> dispatch (NavigateToPage GamePage))
                    prop.children [
                      Html.img [
                        prop.src "assets/soccer.svg"
                        prop.alt "Soccer Game"
                        prop.className "w-5 h-5"
                      ]
                    ]
                  ]
              ]
            ]
          ]
        ]

        Html.div [
          prop.className "flex bg-purple-200 gap-4 p-4";
          prop.children [
            Html.div [
              prop.className " text-3xl font-bold w-full text-center"
              //prop.text (Time.formatMMSS state.ElapsedSecondsInHalf)
            ]
            Html.div [
              prop.className "bg-purple-200 text-3xl font-bold w-full text-center";
              prop.text (Time.formatMMSS state.ElapsedSecondsInHalf)
            ]
            Html.div [
              prop.className "bg-purple-200 text-3xl font-bold w-full text-center";
              prop.children [
                Html.div [
                  prop.className "flex items-center justify-center gap-3";
                  prop.children [
                    // Play/Pause toggle button
                    match state.Timer with
                    | Running -> pauseButton pauseTimer
                    | _ -> playButton startTimer
                    // Stop button
                    Html.button [
                      prop.className "bg-red-500 hover:bg-red-600 focus:ring-2 focus:ring-red-300 text-white rounded-full w-10 h-10 inline-flex items-center justify-center shadow";
                      prop.ariaLabel "Stop";
                      prop.children [
                        Html.img [
                          prop.src "assets/stop.svg";
                          prop.alt "Stop";
                          prop.className "w-5 h-5"
                        ]
                      ]
                    ]
                  ]
                ]
              ]
            ]
          ]
        ]
      ]
    ]


    
