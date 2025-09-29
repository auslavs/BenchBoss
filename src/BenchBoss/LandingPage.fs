namespace BenchBossApp.Components.BenchBoss

open Feliz
open BenchBossApp.Components.BenchBoss.Types

module LandingPage =

  let private actionCard (title: string) (description: string) (buttonText: string) (onClick: unit -> unit) =
    Html.div [
      prop.className "bg-white rounded-xl shadow-lg p-8 space-y-4 border border-purple-100 hover:border-purple-200 transition-colors"
      prop.children [
        Html.h2 [
          prop.className "text-2xl font-semibold text-purple-900"
          prop.text title
        ]
        Html.p [
          prop.className "text-base text-purple-700"
          prop.text description
        ]
        Html.button [
          prop.className "inline-flex items-center gap-2 px-5 py-3 bg-purple-600 text-white font-semibold rounded-lg hover:bg-purple-700 transition-colors"
          prop.onClick (fun _ -> onClick())
          prop.children [
            Html.span buttonText
            Html.span [
              prop.className "text-lg"
              prop.text "→"
            ]
          ]
        ]
      ]
    ]

  let render (state: State) (dispatch: Msg -> unit) =
    let hasRoster = not state.TeamPlayers.IsEmpty

    Html.div [
      prop.className "flex-1 bg-gradient-to-b from-purple-50 to-white py-16"
      prop.children [
        Html.div [
          prop.className "max-w-4xl mx-auto px-4 space-y-12"
          prop.children [
            Html.div [
              prop.className "text-center space-y-4"
              prop.children [
                Html.h1 [
                  prop.className "text-4xl font-extrabold text-purple-900"
                  prop.text "Welcome to BenchBoss"
                ]
                Html.p [
                  prop.className "text-lg text-purple-700"
                  prop.text "Manage your team and get ready for game day. Choose where you'd like to begin."
                ]
              ]
            ]

            Html.div [
              prop.className "grid gap-8 md:grid-cols-2"
              prop.children [
                actionCard
                  "Manage your roster"
                  "Add new players, update names, and keep your bench organized."
                  "Go to team management"
                  (fun () -> dispatch (NavigateToPage ManageTeamPage))

                actionCard
                  "Set up a new game"
                  (if hasRoster then
                     "Pick today's lineup, tune the number of field positions, and launch the match."
                   else
                     "Choose field positions and get ready to add players once you've built your roster.")
                  "Start game setup"
                  (fun () -> dispatch (NavigateToPage StartGamePage))
              ]
            ]
          ]
        ]
      ]
    ]
