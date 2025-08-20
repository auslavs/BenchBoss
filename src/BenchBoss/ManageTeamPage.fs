namespace BenchBossApp.Components.BenchBoss

open Feliz
open BenchBossApp.Components.BenchBoss.Types

module ManageTeamPage =

  let private renderPlayerRow (state: State) (dispatch: Msg -> unit) (player: TeamPlayer) =
    let isAvailableForGame = state.GamePlayers |> List.map _.Id |> List.contains player.Id

    Html.div [
      prop.className "flex items-center justify-between p-4 bg-white rounded-lg shadow-sm border border-gray-200 hover:shadow-md transition-shadow duration-200"
      prop.children [
        Html.div [
          prop.className "flex items-center space-x-3"
          prop.children [
            Html.input [
              prop.type' "checkbox"
              prop.className "h-5 w-5 text-purple-600 bg-purple-100 accent-purple-500 focus:ring-purple-500 rounded-lg"
              prop.isChecked isAvailableForGame
              prop.onChange (fun (_isChecked: bool) -> dispatch (TogglePlayerGameAvailability player.Id))
            ]
            Html.div [
              prop.className "flex-1"
              prop.children [
                Html.h3 [
                  prop.className "text-lg font-medium text-gray-900"
                  prop.text player.Name
                ]
              ]
            ]
          ]
        ]
        Html.div [
          prop.className "flex items-center space-x-2"
          prop.children [
            Html.button [
              prop.className "px-3 py-1 text-sm bg-white text-purple-600 border border-purple-500 rounded hover:bg-purple-50 transition-colors duration-200"
              prop.text "Edit"
              prop.onClick (fun _ -> EditPlayerModal player |> ShowModal |> dispatch)
            ]
            Html.button [
              prop.className "px-3 py-1 text-sm bg-white text-purple-600 border border-purple-500 rounded hover:bg-purple-50 transition-colors duration-200"
              prop.text "Remove"
              prop.onClick (fun _ -> ConfirmRemoveTeamPlayer player.Id |> dispatch)
            ]
          ]
        ]
      ]
    ]

  [<RequireQualifiedAccess>]
  module private Modal =

    [<ReactComponent>]
    let AddPlayer (dispatch: Msg -> unit) =

      let newPlayerName, setNewPlayerName = React.useState ""

      Html.div [
        prop.className "fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
        prop.children [
          Html.div [
            prop.className "bg-white rounded-lg p-6 w-full max-w-md mx-4"
            prop.children [
              Html.h3 [
                prop.className "text-xl font-bold text-gray-800 mb-4"
                prop.text "Add New Player"
              ]
              Html.input [
                prop.type' "text"
                prop.className "w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                prop.placeholder "Player name"
                prop.value newPlayerName
                prop.onChange setNewPlayerName
                prop.onKeyPress (fun e -> if e.key = "Enter" then ConfirmAddTeamPlayer newPlayerName |> dispatch)
              ]
              Html.div [
                prop.className "flex justify-end space-x-3 mt-6"
                prop.children [
                  Html.button [
                    prop.className "px-4 py-2 text-gray-600 hover:text-gray-800"
                    prop.text "Cancel"
                    prop.onClick (fun _ -> dispatch HideModal)
                  ]
                  Html.button [
                    prop.className "px-4 py-2 bg-purple-500 text-white rounded-lg hover:bg-purple-600 disabled:opacity-50"
                    prop.text "Add Player"
                    prop.disabled (newPlayerName.Trim() = "")
                    prop.onClick (fun _ -> ConfirmAddTeamPlayer newPlayerName |> dispatch)
                  ]
                ]
              ]
            ]
          ]
        ]
      ]

    [<ReactComponent>]
    let EditPlayer (dispatch: Msg -> unit) (player: TeamPlayer) =
      let updatedPlayerName, setUpdatedPlayerName = React.useState player.Name

      Html.div [
        prop.className "fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
        prop.children [
          Html.div [
            prop.className "bg-white rounded-lg p-6 w-full max-w-md mx-4"
            prop.children [
              Html.h3 [
                prop.className "text-xl font-bold text-gray-800 mb-4"
                prop.text "Edit Player"
              ]
              Html.input [
                prop.type' "text"
                prop.className "w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                prop.placeholder "Player name"
                prop.value updatedPlayerName
                prop.onChange setUpdatedPlayerName
                prop.onKeyPress (fun e -> if e.key = "Enter" then dispatch (ConfirmUpdateTeamPlayer { player with Name = updatedPlayerName }))
              ]
              Html.div [
                prop.className "flex justify-end space-x-3 mt-6"
                prop.children [
                  Html.button [
                    prop.className "px-4 py-2 text-gray-600 hover:text-gray-800"
                    prop.text "Cancel"
                    prop.onClick (fun _ -> dispatch HideModal)
                  ]
                  Html.button [
                    prop.className "px-4 py-2 bg-purple-500 text-white rounded-lg hover:bg-purple-600 disabled:opacity-50"
                    prop.text "Save Changes"
                    prop.disabled (updatedPlayerName.Trim() = "")
                    prop.onClick (fun _ -> { player with Name = updatedPlayerName } |> ConfirmUpdateTeamPlayer |> dispatch)
                  ]
                ]
              ]
            ]
          ]
        ]
      ]

  let private renderSummary (state: State) =
    let fieldPlayers = state.FieldSlots |> Array.choose id |> Array.length
    let benchPlayers = state.Bench.Length
    let totalPlayers = state.TeamPlayers.Length
    let availableForGame = state.GamePlayers.Length

    Html.div [
      prop.className "bg-gradient-to-r from-blue-500 to-purple-600 rounded-lg p-6 text-white"
      prop.children [
        Html.div [
          prop.className "flex justify-between items-center"
          prop.children [
            Html.div [
              prop.children [
                Html.h3 [
                  prop.className "text-xl font-bold mb-2"
                  prop.text "Team Overview"
                ]
                Html.div [
                  prop.className "grid grid-cols-2 md:grid-cols-4 gap-4 text-sm"
                  prop.children [
                    Html.div [
                      prop.className "text-center"
                      prop.children [
                        Html.div [
                          prop.className "text-2xl font-bold"
                          prop.text (string totalPlayers)
                        ]
                        Html.div [
                          prop.text "Total Players"
                        ]
                      ]
                    ]
                    Html.div [
                      prop.className "text-center"
                      prop.children [
                        Html.div [
                          prop.className "text-2xl font-bold"
                          prop.text (string availableForGame)
                        ]
                        Html.div [
                          prop.text "Available for Game"
                        ]
                      ]
                    ]
                    Html.div [
                      prop.className "text-center"
                      prop.children [
                        Html.div [
                          prop.className "text-2xl font-bold"
                          prop.text (string fieldPlayers)
                        ]
                        Html.div [
                          prop.text "On Field"
                        ]
                      ]
                    ]
                    Html.div [
                      prop.className "text-center"
                      prop.children [
                        Html.div [
                          prop.className "text-2xl font-bold"
                          prop.text (string benchPlayers)
                        ]
                        Html.div [
                          prop.text "On Bench"
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

  let private renderPlayersSection (state: State) (dispatch: Msg -> unit) =
    Html.div [
      prop.className "bg-white rounded-lg shadow-lg p-6"
      prop.children [
        Html.h3 [
          prop.className "text-xl font-semibold text-gray-800 mb-4"
          prop.text "Players"
        ]
        
        if state.TeamPlayers.IsEmpty then
          Html.div [
            prop.className "text-center py-12 text-gray-500"
            prop.children [
              Html.div [
                prop.className "text-6xl mb-4"
                prop.text "⚽"
              ]
              Html.h4 [
                prop.className "text-lg font-semibold mb-2"
                prop.text "No players yet"
              ]
              Html.p [
                prop.text "Add your first player to get started!"
              ]
            ]
          ]
        else
          Html.div [
            prop.className "space-y-2"
            prop.children (
              state.TeamPlayers 
              |> List.map (renderPlayerRow state dispatch)
            )
          ]
      ]
    ]

  let addPlayerButton addPlayer =
    Html.button [
      prop.className "flex-1 px-6 py-3 bg-purple-500 text-white rounded-lg font-medium hover:bg-purple-600 transition-colors duration-200 items-center space-x-2"
      prop.onClick addPlayer
      prop.children [
        Html.span [ prop.text "+" ]
        Html.span [ prop.text "Add Player" ]
      ]
    ]

  let resetGameButton resetGame =
    Html.button [
      prop.className "flex-1 px-6 py-3 bg-purple-500 text-white rounded-lg font-medium hover:bg-purple-600 transition-colors duration-200"
      prop.onClick resetGame
      prop.text "Reset Game"
    ]

  let render (state: State) (dispatch: Msg -> unit) =
    Html.div [
      prop.className "flex-1 p-8 bg-gradient-to-b from-purple-50 to-indigo-50"
      prop.children [
        Html.div [
          prop.className "max-w-6xl mx-auto flex flex-col gap-6"
          prop.children [

            renderSummary state
            renderPlayersSection state dispatch

            Html.div [
              prop.className "flex gap-4"
              prop.children [
                resetGameButton (fun _ -> dispatch ResetGame)
                addPlayerButton (fun _ -> dispatch (ShowModal AddPlayerModal))
              ]
            ]

            // Modals
            match state.CurrentModal with
            | Some AddPlayerModal -> Modal.AddPlayer dispatch
            | Some (EditPlayerModal player) -> Modal.EditPlayer dispatch player
            | _ -> Html.div []
          ]
        ]
      ]
    ]
