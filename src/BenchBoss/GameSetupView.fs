namespace BenchBossApp.Components.BenchBoss

open System
open Feliz
open BenchBossApp.Components.BenchBoss.Types

module GameSetupView =

  let private maxSlots = 11

  let private playerCheckbox (selected:Set<PlayerId>) (dispatch: Msg -> unit) (player: TeamPlayer) =
    let isSelected = selected.Contains player.Id

    Html.label [
      prop.key (string player.Id)
      prop.className "flex items-center justify-between gap-3 p-3 border border-purple-100 rounded-lg hover:border-purple-300 transition-colors cursor-pointer"
      prop.children [
        Html.div [
          prop.className "flex items-center gap-3"
          prop.children [
            Html.input [
              prop.type' "checkbox"
              prop.isChecked isSelected
              prop.onChange (fun (_: bool) -> dispatch (TogglePlayerGameAvailability player.Id))
              prop.className "h-5 w-5 text-purple-600 rounded focus:ring-purple-500"
            ]
            Html.span [
              prop.className "text-lg font-medium text-gray-800"
              prop.text player.Name
            ]
          ]
        ]
        if isSelected then
          Html.span [
            prop.className "text-sm font-medium text-purple-600"
            prop.text "Selected"
          ]
        else
          Html.none
      ]
    ]

  [<ReactComponent>]
  let private AddPlayerForm (dispatch: Msg -> unit) =
    let name, setName = React.useState ""

    let submit () =
      let trimmed = name.Trim()
      if not (String.IsNullOrWhiteSpace trimmed) then
        dispatch (ConfirmAddTeamPlayer trimmed)
        setName ""

    Html.form [
      prop.className "flex items-center gap-3"
      prop.onSubmit (fun ev ->
        ev.preventDefault()
        submit ())
      prop.children [
        Html.input [
          prop.type' "text"
          prop.className "flex-1 rounded-lg border border-purple-200 px-4 py-2 focus:outline-none focus:ring-2 focus:ring-purple-400"
          prop.placeholder "Add a new player"
          prop.value name
          prop.onChange setName
        ]
        Html.button [
          prop.type' "submit"
          prop.className "px-4 py-2 rounded-lg bg-purple-600 text-white font-medium hover:bg-purple-700 disabled:opacity-40 disabled:cursor-not-allowed"
          prop.disabled (String.IsNullOrWhiteSpace name)
          prop.text "Add"
        ]
      ]
    ]

  let private fieldSlotSelector (state: State) (dispatch: Msg -> unit) =
    let handleChange (value: string) =
      match Int32.TryParse value with
      | true, parsed -> dispatch (SetFieldSlots parsed)
      | _ -> ()

    Html.div [
      prop.className "space-y-3"
      prop.children [
        Html.div [
          prop.className "flex items-center justify-between"
          prop.children [
            Html.span [
              prop.className "text-lg font-semibold text-gray-800"
              prop.text "Field positions"
            ]
            Html.span [
              prop.className "text-2xl font-bold text-purple-600"
              prop.text (string state.Game.FieldSlots)
            ]
          ]
        ]
        Html.input [
          prop.type' "range"
          prop.className "w-full"
          prop.min 1
          prop.max maxSlots
          prop.step 1
          prop.value state.Game.FieldSlots
          prop.onChange handleChange
        ]
        Html.div [
          prop.className "flex items-center gap-3"
          prop.children [
            Html.input [
              prop.type' "number"
              prop.className "w-20 border border-purple-200 rounded-lg px-2 py-1"
              prop.min 1
              prop.max maxSlots
              prop.value state.Game.FieldSlots
              prop.onChange handleChange
            ]
            Html.p [
              prop.className "text-sm text-gray-500"
              prop.text "Choose how many players will start on the field."
            ]
          ]
        ]
      ]
    ]

  let private selectedSummary (state: State) =
    let selectedCount = state.Game.Players.Length
    let difference = state.Game.FieldSlots - selectedCount

    Html.div [
      prop.className "grid grid-cols-2 gap-4"
      prop.children [
        Html.div [
          prop.className "bg-purple-50 rounded-lg p-4"
          prop.children [
            Html.p [
              prop.className "text-sm text-purple-700"
              prop.text "Players selected"
            ]
            Html.p [
              prop.className "text-2xl font-semibold text-purple-900"
              prop.text (string selectedCount)
            ]
          ]
        ]
        Html.div [
          prop.className "bg-purple-50 rounded-lg p-4"
          prop.children [
            Html.p [
              prop.className "text-sm text-purple-700"
              prop.text "Spots remaining"
            ]
            Html.p [
              prop.className "text-2xl font-semibold text-purple-900"
              prop.text (
                if difference > 0 then string difference else "0"
              )
            ]
          ]
        ]
      ]
    ]

  let render (state: State) (dispatch: Msg -> unit) =
    let selectedPlayers = state.Game.Players |> List.map _.Id |> Set.ofList
    let selectedCount = selectedPlayers |> Set.count
    let missingPlayers = max 0 (state.Game.FieldSlots - selectedCount)
    let canStart = missingPlayers = 0 && selectedCount > 0

    Html.div [
      prop.className "flex-1 bg-gradient-to-b from-purple-50 to-white py-10"
      prop.children [
        Html.div [
          prop.className "max-w-5xl mx-auto px-4 space-y-8"
          prop.children [
            Html.div [
              prop.className "space-y-3 text-center md:text-left"
              prop.children [
                Html.h1 [
                  prop.className "text-4xl font-extrabold text-purple-900"
                  prop.text "Set up your match"
                ]
                Html.p [
                  prop.className "text-lg text-purple-700"
                  prop.text "Name your game, pick who is playing, and choose how many spots you need on the field."
                ]
              ]
            ]

            Html.div [
              prop.className "grid gap-8 md:grid-cols-2"
              prop.children [
                Html.div [
                  prop.className "bg-white rounded-xl shadow-lg p-6 space-y-6"
                  prop.children [
                    Html.div [
                      prop.className "space-y-2"
                      prop.children [
                        Html.label [
                          prop.className "text-lg font-semibold text-gray-800"
                          prop.text "Game name"
                        ]
                        Html.input [
                          prop.type' "text"
                          prop.className "w-full border border-purple-200 rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-purple-400"
                          prop.placeholder "Saturday scrimmage"
                          prop.value state.Game.Name
                          prop.onChange (fun value -> dispatch (UpdateGameName value))
                        ]
                      ]
                    ]

                    fieldSlotSelector state dispatch

                    selectedSummary state

                    if missingPlayers > 0 then
                      Html.p [
                        prop.className "text-sm text-amber-600 bg-amber-50 border border-amber-200 rounded-lg px-3 py-2"
                        prop.text $"Add {missingPlayers} more player(s) to fill every position."
                      ]
                    else
                      Html.p [
                        prop.className "text-sm text-green-700 bg-green-50 border border-green-200 rounded-lg px-3 py-2"
                        prop.text "You're ready to take the field!"
                      ]

                    Html.button [
                      prop.className "w-full px-6 py-3 bg-purple-600 text-white rounded-lg font-semibold hover:bg-purple-700 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
                      prop.disabled (not canStart)
                      prop.onClick (fun _ -> dispatch StartGame)
                      prop.text "Start game"
                    ]
                  ]
                ]

                Html.div [
                  prop.className "bg-white rounded-xl shadow-lg p-6 space-y-5"
                  prop.children [
                    Html.div [
                      prop.className "flex items-center justify-between"
                      prop.children [
                        Html.h2 [
                          prop.className "text-2xl font-semibold text-gray-900"
                          prop.text "Choose your players"
                        ]
                        Html.span [
                          prop.className "text-sm text-gray-500"
                          prop.text $"{selectedCount} selected"
                        ]
                      ]
                    ]

                    AddPlayerForm dispatch

                    if state.TeamPlayers.IsEmpty then
                      Html.p [
                        prop.className "text-sm text-gray-500"
                        prop.text "Add your first player above to start building the roster."
                      ]
                    else
                      Html.div [
                        prop.className "space-y-2 max-h-[26rem] overflow-y-auto pr-1"
                        prop.children (
                          state.TeamPlayers
                          |> List.sortBy (fun p -> p.Name)
                          |> List.map (playerCheckbox selectedPlayers dispatch)
                        )
                      ]

                    Html.button [
                      prop.className "w-full px-4 py-2 text-sm font-semibold text-purple-600 border border-purple-200 rounded-lg hover:bg-purple-50 transition-colors"
                      prop.onClick (fun _ -> dispatch (NavigateToPage ManageTeamPage))
                      prop.text "Open team management"
                    ]
                  ]
                ]
              ]
            ]
          ]
        ]
      ]
    ]
