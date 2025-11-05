namespace BenchBossApp.Components.BenchBoss.GameSetupPage

module SelectPlayersPage =

  open Feliz
  open BenchBossApp.Components.BenchBoss.Types
  open BenchBossApp
  open Fable.Core.JsInterop

  [<ReactComponent>]
  let Component (props:
    {| allPlayers: TeamPlayer list
       selectedPlayers: Map<PlayerId, TeamPlayer>
       togglePlayer: TeamPlayer -> unit
       onContinue: Map<PlayerId, TeamPlayer> -> unit |}) =

    let selected = props.selectedPlayers
    let notPlaying = props.allPlayers |> List.except (Map.values selected)
    let onContinue = props.onContinue
    let togglePlayer = props.togglePlayer

    Html.div [
      prop.className "h-full max-w-lg mx-auto bg-white p-4"
      prop.children [
        Html.h2 [ prop.className "mb-2 text-purple-900"; prop.text "Select Players" ]
        Html.p [
          prop.className "text-gray-600 mb-6"
          prop.text "Choose which players are attending this game."
        ]
        Html.div [
          prop.className "bg-purple-50 rounded-xl p-4 text-center border-2 border-purple-200 mb-6"
          prop.children [
            Html.div [
              prop.className "text-3xl text-purple-700"
              prop.text (Map.count selected |> string)
            ]
            Html.div [ prop.className "text-sm text-purple-600 mt-1"; prop.text "Players Selected" ]
          ]
        ]
        // Playing section
        Html.div [
          prop.className "mb-4"
          prop.children [
            Html.div [
              prop.className "flex items-center justify-between mb-3"
              prop.children [
                Html.div [
                  prop.className "flex items-center gap-2"
                  prop.children [
                    UserCheck [ svg.className "w-5 h-5 text-purple-600" ]
                    Html.h3 [ prop.className "text-purple-700"; prop.text "Playing Today" ]
                  ]
                ]
                PrimaryBadge (string (Map.count selected))
              ]
            ]
            Html.div [
              prop.className "space-y-2 bg-purple-50 rounded-xl p-3 border-2 border-purple-200"
              prop.children [

                if Map.isEmpty selected then
                  Html.div [
                    prop.className "text-gray-600 py-4 text-center"
                    prop.text "No players selected"
                  ]

                for player in Map.values selected do
                  Html.div [
                    prop.key player.Id
                    prop.className "flex items-center justify-between p-3 bg-white rounded-lg cursor-pointer hover:bg-gray-50"
                    prop.onClick (fun _ -> togglePlayer player)
                    prop.children [
                      Html.span [ prop.text player.Name ]
                      Html.button [
                        prop.type'.button
                        prop.className "text-red-600 hover:text-red-700 hover:bg-red-50"
                        prop.onClick (fun e ->
                          e.stopPropagation ()
                          togglePlayer player
                        )
                        prop.children [
                          Heroicons.XMarkIcon [ "className" ==> "w-4 h-4"]
                        ]
                      ]
                    ]
                  ]
              ]
            ]
          ]
        ]
        // Not Playing section
        Html.div [
          prop.className "mb-6"
          prop.children [
            Html.div [
              prop.className "flex items-center justify-between mb-3"
              prop.children [
                Html.div [
                  prop.className "flex items-center gap-2"
                  prop.children [
                    UserMinus [ svg.className "w-5 h-5 text-gray-500" ]
                    Html.h3 [ prop.className "text-gray-600"; prop.text "Roster" ]
                  ]
                ]
                SecondaryBadge (string notPlaying.Length)
              ]
            ]
            Html.div [
              prop.className "space-y-2 bg-gray-50 rounded-xl p-3 border-2 border-gray-200"
              prop.children [
                if notPlaying.Length = 0 then
                  Html.div [
                    prop.className "text-center py-4 text-gray-400 text-sm"
                    prop.text "All players selected"
                  ]
                else
                  for player in notPlaying do
                    Html.div [
                      prop.key player.Id
                      prop.className "flex items-center justify-between p-3 bg-white rounded-lg border border-gray-200 cursor-pointer hover:bg-gray-50"
                      prop.onClick (fun _ -> togglePlayer player)
                      prop.children [
                        Html.span [ prop.className "text-gray-600"; prop.text player.Name ]
                        Html.button [
                          prop.type'.button
                          prop.className "text-purple-600 hover:text-purple-700 hover:bg-purple-50"
                          prop.onClick (fun e ->
                            e.stopPropagation ()
                            togglePlayer player)
                          prop.children [ UserCheck [ svg.className "w-4 h-4 text-purple-600" ] ]
                        ]
                      ]
                    ]
              ]
            ]
          ]
        ]

        Html.div [
          prop.className "mt-4 flex justify-end"
          prop.children [
            Html.button [
              prop.type'.button
              prop.className "w-full sm:w-auto h-14 px-6 py-3 bg-purple-600 text-white rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-purple-700 flex items-center justify-center"
              prop.text "Continue to Lineup"
              prop.disabled (Map.isEmpty selected)
              prop.onClick (fun _ -> onContinue selected)
              prop.title "Continue to lineup selection"
            ]
          ]
        ]
      ]
    ]