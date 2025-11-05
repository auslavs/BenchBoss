namespace BenchBossApp.Components.BenchBoss

open Feliz
open BenchBossApp.Components.BenchBoss.Types

module HomePage =

  [<ReactComponent>]
  let View (state: State) (dispatch: Msg -> unit) =
    // Consider a game 'active' if there are any players in the current Game list
    // (so after reset it's empty; after setup it has players even if timer not started yet)
    let hasActiveGame = not state.Game.Players.IsEmpty

    let elapsed = State.getElapsedSecondsInHalf state |> Time.formatMMSS

    let currentHalfNumber =
      let half =
        match state.Game.Timer with
        | Running r -> r.Half
        | Paused p -> p.Half
        | Break b -> b.Half
        | Stopped -> First
      match half with | First -> 1 | Second -> 2

    let scoreBadge =
      Html.div [
        prop.className "flex items-center gap-3"
        prop.children [
          Html.div [
            prop.className "px-4 py-2 rounded-lg bg-purple-100 text-purple-700 font-semibold text-lg"
            prop.text ($"{state.OurScore} - {state.OppScore}")
          ]
          if hasActiveGame then
            Html.div [
              prop.className "text-sm text-gray-600"
              prop.text ($"Half {currentHalfNumber} • {elapsed}")
            ]
        ]
      ]

    let teamSnapshot =
      let totalPlayers = state.TeamPlayers.Length
      let gamePlayers = state.Game.Players.Length
      let onField = state.Game.Players |> List.filter (fun p -> p.InGameStatus = OnField) |> List.length
      let onBench = state.Game.Players |> List.filter (fun p -> p.InGameStatus = OnBench) |> List.length
      Html.div [
        prop.className "grid grid-cols-2 sm:grid-cols-4 gap-4"
        prop.children [
          let statCard (label:string) (value:string) =
            Html.div [
              prop.className "bg-white rounded-lg shadow p-4 flex flex-col items-center"
              prop.children [
                Html.div [ prop.className "text-2xl font-bold"; prop.text value ]
                Html.div [ prop.className "text-xs tracking-wide text-gray-500 mt-1"; prop.text label ]
              ]
            ]
          statCard "Roster" (string totalPlayers)
          statCard "In Game" (string gamePlayers)
          statCard "On Field" (string onField)
          statCard "On Bench" (string onBench)
        ]
      ]

    Html.div [
      prop.className "flex-1 p-6 bg-gradient-to-b from-purple-50 to-indigo-50"
      prop.children [
        Html.div [
          prop.className "max-w-5xl mx-auto flex flex-col gap-8"
          prop.children [
            Html.div [
              prop.className "bg-white rounded-xl shadow p-4 space-y-6"
              prop.children [
                Html.h1 [ prop.className "text-3xl font-bold"; prop.text "BenchBoss" ]
                Html.p [ prop.className "text-gray-600"; prop.text "Manage fair play time and keep the game organized." ]
                if hasActiveGame then scoreBadge
                Html.div [
                  // Column on mobile, row on >= sm
                  prop.className "flex flex-col sm:flex-row gap-4 w-full"
                  prop.children [
                    if hasActiveGame then
                      Html.button [
                        prop.className "w-full sm:w-auto px-6 py-3 bg-purple-600 hover:bg-purple-700 text-white rounded-lg font-medium"
                        prop.text "Current Game"
                        prop.onClick (fun _ -> dispatch (NavigateToPage GamePage))
                        prop.title "Go to the active game"
                      ]
                    if hasActiveGame then
                      Html.button [
                        prop.className "w-full sm:w-auto px-6 py-3 bg-red-50 text-red-700 border border-red-300 hover:bg-red-100 rounded-lg font-medium"
                        prop.text "End Current Game"
                        prop.onClick (fun _ -> dispatch ResetGame)
                        prop.title "End the current game and reset scores and timers"
                      ]
                    if not hasActiveGame then
                      Html.button [
                        prop.className "w-full sm:w-auto px-6 py-3 bg-purple-50 hover:bg-purple-100 text-purple-700 border border-purple-200 rounded-lg font-medium"
                        prop.text (if hasActiveGame then "Start New Game" else "Start New Game")
                        prop.onClick (fun _ -> dispatch (NavigateToPage GameSetupPage))
                        prop.title "Create a new game (will replace current if one is active)"
                      ]
                    Html.button [
                      prop.className "w-full sm:w-auto px-6 py-3 bg-white hover:bg-gray-50 text-gray-700 border rounded-lg font-medium"
                      prop.text "Manage Team"
                      prop.onClick (fun _ -> dispatch (NavigateToPage ManageTeamPage))
                    ]
                  ]
                ]
              ]
            ]
            Html.div [
              prop.className "space-y-4"
              prop.children [
                Html.h2 [ prop.className "text-xl font-semibold"; prop.text "Team Snapshot" ]
                teamSnapshot
              ]
            ]
          ]
        ]
      ]
    ]
