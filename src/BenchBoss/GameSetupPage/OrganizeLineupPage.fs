namespace BenchBossApp.Components.BenchBoss.GameSetupPage

module OrganizeLineupPage =
  open Feliz
  open BenchBossApp.Components.BenchBoss.Types

  [<ReactComponent>]
  let Component (props:
    {| startingPlayers: TeamPlayer list
       benchedPlayers: TeamPlayer list
       toggleStarter: PlayerId -> unit

       CurrentFieldPlayerTarget: int
       SetFieldPlayerTarget: int -> unit
       onBack: unit -> unit
       onStartGame: unit -> unit |}) =

    let starters = props.startingPlayers
    let bench = props.benchedPlayers
    let onBack = props.onBack
    let onStartGame = props.onStartGame

    Html.div [
      prop.className "max-w-lg mx-auto bg-white rounded-3xl p-4"
      prop.children [
        Html.h2 [ prop.className "mb-2 text-purple-900"; prop.text "Organize Lineup" ]
        Html.p [
          prop.className "text-gray-600 mb-6"
          prop.text "Set who starts on the field and who begins on the bench."
        ]

        Html.div [
          prop.className "grid grid-cols-2 gap-3 mb-6"
          prop.children [
            Html.div [
              prop.className "rounded-xl p-4 border-2 border-purple-200"
              prop.children [
                Html.label [
                  prop.className "block text-sm text-gray-700 mb-2"
                  prop.text "Players on Field"
                ]
                Select {|
                  defaultValue = $"{props.CurrentFieldPlayerTarget} players"
                  options = [ for size in 4 .. 11 do string size, $"{size} players" ]
                |}
              ]
            ]
            Html.div [
              prop.className "bg-purple-50 rounded-xl p-4 flex flex-col justify-center items-center border-2 border-purple-200"
              prop.children [
                Html.div [
                  prop.className "text-3xl text-purple-700"
                  prop.text (starters.Length + bench.Length |> string)
                ]
                Html.div [ prop.className "text-xs text-purple-600 mt-1"; prop.text "Playing Today" ]
              ]
            ]
          ]
        ]

        // Starters section
        Html.div [
          prop.className "mb-4"
          prop.children [
            Html.div [
              prop.className "flex items-center justify-between mb-3"
              prop.children [
                Html.div [
                  prop.className "flex items-center gap-2"
                  prop.children [
                    Star [ svg.className "w-5 h-5 text-purple-600 fill-purple-600" ]
                    Html.h3 [ prop.className "text-purple-700"; prop.text "Starting Lineup" ]
                  ]
                ]
                PrimaryBadge (string 999)
              ]
            ]
            Html.div [
              prop.className "space-y-2 bg-purple-50 rounded-xl p-3 border-2 border-purple-200"
              prop.children [
                if starters.Length = 0 then
                  Html.div [
                    prop.className "text-center py-6 text-gray-400 text-sm"
                    prop.text "No starters selected yet"
                  ]
                else
                  for player in starters do
                    Html.div [
                      prop.key player.Id
                      prop.className "flex items-center justify-between p-3 bg-white rounded-lg"
                      prop.onClick (fun e -> e.preventDefault(); player.Id |> props.toggleStarter)
                      prop.children [
                        Html.span [ prop.text player.Name ]

                        Html.button [
                          prop.className "text-gray-600 hover:text-purple-600 hover:bg-purple-50 p-1 rounded"
                          prop.children [
                            Armchair [ svg.className "w-4 h-4" ]
                          ]
                        ]
                      ]
                    ]
              ]
            ]
          ]
        ]

        // Bench section
        Html.div [
          prop.className "mb-6"
          prop.children [
            Html.div [
              prop.className "flex items-center justify-between mb-3"
              prop.children [
                Html.div [
                  prop.className "flex items-center gap-2"
                  prop.children [
                    Armchair [ svg.className "w-5 h-5 text-purple-600" ]
                    Html.h3 [ prop.className "text-purple-700"; prop.text "On Bench" ]
                  ]
                ]
                PrimaryBadge (string bench.Length)
              ]
            ]
            Html.div [
              prop.className "space-y-2 bg-white rounded-xl p-3 border-2 border-purple-200 min-h-[100px]"
              prop.children [
                if bench.Length = 0 then
                  Html.div [
                    prop.className "text-center py-6 text-gray-400 text-sm"
                    prop.text "All players in starting lineup"
                  ]
                else
                  for player in bench do
                    Html.div [
                      prop.key player.Id
                      prop.className "flex items-center justify-between p-3 bg-white rounded-lg border border-gray-200"
                      prop.onClick (fun e -> e.preventDefault(); player.Id |> props.toggleStarter)
                      prop.children [
                        Html.span [ prop.text player.Name ]

                        Html.button [
                          prop.className "text-gray-600 hover:text-purple-600 hover:bg-purple-50 p-1 rounded"
                          prop.children [
                            Star [ svg.className "w-4 h-4" ]
                          ]
                        ]
                      ]
                    ]
              ]
            ]
          ]
        ]

        Html.div [
          prop.className "flex gap-3 justify-end"
          prop.children [
            Html.button [
              prop.type'.button
              prop.className "w-full sm:w-auto h-14 px-6 py-3 text-purple-600 hover:bg-purple-50 rounded-lg"
              prop.onClick (fun _ -> onBack ())
              prop.children [ Html.text "Back" ]
            ]
            Html.button [
              prop.type'.button
              prop.className "w-full sm:w-auto h-14 px-6 py-3 bg-purple-600 text-white rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-purple-700 flex items-center justify-center"
              prop.onClick (fun _ -> onStartGame ())
              prop.children [ Html.text "Start Game" ]
            ]
          ]
        ]
      ]
    ]
