namespace BenchBossApp.Components.BenchBoss

module Layout =
  open Feliz
  open BenchBossApp.Components.BenchBoss.Types

  let private navButton iconSrc alt ariaLabel title onClick =
    Html.button [
      prop.className "bg-purple-600 hover:bg-purple-500 focus:ring-2 focus:ring-purple-300 text-white rounded-full w-10 h-10 inline-flex items-center justify-center shadow transition-colors"
      prop.ariaLabel ariaLabel
      prop.title title
      prop.onClick (fun _ -> onClick ())
      prop.children [
        Html.img [
          prop.src iconSrc
          prop.alt alt
          prop.className "w-5 h-5"
        ]
      ]
    ]

  let private navigationButtons (dispatch: Msg -> unit) (state: State) =
    let manageTeamButton () =
      navButton
        "assets/team.svg"
        "Team Management"
        "Team Management"
        "Manage Teams and Players"
        (fun () -> dispatch (NavigateToPage ManageTeamPage))

    let startGameButton () =
      navButton
        "assets/play.svg"
        "Start Game Setup"
        "Start Game Setup"
        "Start Game Setup"
        (fun () -> dispatch (NavigateToPage StartGamePage))

    let gameButton () =
      navButton
        "assets/soccer.svg"
        "Soccer Game"
        "Back to Soccer Game"
        "Back to Soccer Game"
        (fun () -> dispatch (NavigateToPage GamePage))

    let landingButton () =
      navButton
        "assets/play.svg"
        "Main Menu"
        "Main Menu"
        "Main Menu"
        (fun () -> dispatch (NavigateToPage LandingPage))

    match state.CurrentPage with
    | LandingPage -> [ manageTeamButton (); startGameButton () ]
    | StartGamePage -> [ manageTeamButton (); landingButton () ]
    | GamePage -> [ manageTeamButton (); landingButton (); startGameButton () ]
    | ManageTeamPage -> [ landingButton (); startGameButton (); gameButton () ]

  let render (dispatch: Msg -> unit) (state: State) =
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

            match state.CurrentPage with
            | LandingPage ->
              Html.div [
                prop.className "px-4 py-2 rounded-lg bg-purple-600 bg-opacity-80"
                prop.children [
                  Html.span [
                    prop.className "text-white text-2xl font-semibold tracking-wide"
                    prop.text "BenchBoss"
                  ]
                ]
              ]
            | _ ->
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

            // Navigation icons in top right (changes based on current page)
            Html.div [
              prop.className "flex items-center gap-2"
              prop.children (navigationButtons dispatch state)
            ]
          ]
        ]

        if state.CurrentPage = GamePage || state.CurrentPage = ManageTeamPage then
          Html.div [
            prop.className "flex bg-purple-200 gap-4 p-4 justify-center"
            prop.children [
              Html.button [
                prop.className "bg-purple-200 text-3xl font-bold text-center"
                prop.onClick (fun _ -> dispatch (ShowModal TimeManagerModal))
                prop.text (Time.formatMMSS (State.getElapsedSecondsInHalf state))
              ]
            ]
          ]
        else
          Html.none
      ]
    ]


    
