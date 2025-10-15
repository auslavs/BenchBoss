namespace BenchBossApp.Components.BenchBoss

open Feliz
open BenchBossApp.Components.BenchBoss.Types

module GameSetupPage =

  [<ReactComponent>]
  let View (state: State) (dispatch: Msg -> unit) =
    // Local selection state (do not mutate global until Start)
    let initialSelected = state.Game.Players |> List.map _.Id |> Set.ofList // if coming from existing game
    let selectedPlayers, setSelectedPlayers = React.useState initialSelected
    let starters, setStarters = React.useState (Set.empty<PlayerId>)

    let toggleSelect (pid:PlayerId) =
      if selectedPlayers.Contains pid then setSelectedPlayers (selectedPlayers.Remove pid)
      else setSelectedPlayers (selectedPlayers.Add pid)

    let toggleStarter (pid:PlayerId) =
      if starters.Contains pid then setStarters (starters.Remove pid)
      else setStarters (starters.Add pid)

    let roster = state.TeamPlayers

    // Removed mandatory 4 starters requirement. A game can start with any selected players (must have at least one).
    let canStart = not (Set.isEmpty selectedPlayers)

    let startGame _ =
      if canStart then
        let starting = if starters.Count = 0 then [] else starters |> Set.toList
        let bench = (selectedPlayers - (Set.ofList starting)) |> Set.toList
        dispatch (StartNewGame (starting, bench))

    Html.div [
      prop.className "flex-1 p-6 bg-gradient-to-b from-green-50 to-blue-50"
      prop.children [
        Html.div [
          prop.className "max-w-4xl mx-auto space-y-8"
          prop.children [
            Html.div [
              prop.className "flex items-center justify-between"
              prop.children [
                Html.h1 [ prop.className "text-2xl font-bold"; prop.text "Game Setup" ]
                Html.button [
                  prop.className "text-sm text-gray-600 hover:underline"
                  prop.text "Cancel"
                  prop.onClick (fun _ -> dispatch (NavigateToPage HomePage))
                ]
              ]
            ]
            Html.div [
              prop.className "bg-white rounded-xl shadow p-6 space-y-4"
              prop.children [
                Html.p [ prop.className "text-gray-600"; prop.text "Select players for this game and optionally mark starters." ]
                Html.div [
                  prop.className "flex flex-wrap gap-4 text-sm"
                  prop.children [
                    Html.div [ prop.className "px-3 py-2 rounded bg-purple-50 text-purple-700 font-medium"; prop.text ($"Selected: {selectedPlayers.Count}") ]
                    Html.div [ prop.className "px-3 py-2 rounded bg-green-50 text-green-700 font-medium"; prop.text ($"Starters: {starters.Count}") ]
                    Html.div [ prop.className "px-3 py-2 rounded bg-gray-100 text-gray-600"; prop.text "Starters optional" ]
                    Html.div [
                      prop.className "flex items-center gap-2 px-3 py-2 rounded bg-blue-50 text-blue-700"
                      prop.children [
                        Html.span [ prop.className "font-medium"; prop.text "On Field" ]
                        Html.select [
                          prop.className "border border-blue-300 rounded px-2 py-1 bg-white text-sm"
                          prop.value state.FieldPlayerTarget
                          prop.onChange (fun (value:string) ->
                            match System.Int32.TryParse value with
                            | true, v -> dispatch (SetFieldPlayerTarget v)
                            | _ -> () )
                          prop.children [ for n in 4..11 -> Html.option [ prop.value n; prop.text (string n) ] ]
                        ]
                      ]
                    ]
                  ]
                ]
                Html.div [
                  prop.className "divide-y border rounded-lg"
                  prop.children [
                    for p in roster do
                      let isSelected = selectedPlayers.Contains p.Id
                      let isStarter = starters.Contains p.Id
                      let faded = if isSelected then "" else "opacity-60"
                      let rowClasses = $"flex items-center justify-between p-3 hover:bg-gray-50 {faded}"
                      let starterBtnClasses =
                        if isStarter then "text-xs px-2 py-1 rounded border bg-green-600 text-white border-green-600"
                        else "text-xs px-2 py-1 rounded border border-green-600 text-green-700 hover:bg-green-50"
                      Html.div [
                        prop.key (string p.Id)
                        prop.className rowClasses
                        prop.children [
                          Html.div [
                            prop.className "flex items-center gap-3"
                            prop.children [
                              Html.input [
                                prop.type' "checkbox"
                                prop.isChecked isSelected
                                prop.onChange (fun (_: bool) -> toggleSelect p.Id)
                              ]
                              Html.span [ prop.className "font-medium"; prop.text p.Name ]
                            ]
                          ]
                          Html.div [
                            prop.className "flex items-center gap-3"
                            prop.children [
                              Html.button [
                                prop.className starterBtnClasses
                                prop.text (if isStarter then "Starter" else "Make Starter")
                                prop.disabled (not isSelected)
                                prop.onClick (fun _ -> toggleStarter p.Id)
                              ]
                            ]
                          ]
                        ]
                      ]
                  ]
                ]
                Html.div [
                  prop.className "pt-4 flex justify-end"
                  prop.children [
                    let startBtnClasses =
                      if canStart then "px-6 py-3 rounded-lg font-medium transition-colors bg-purple-600 hover:bg-purple-700 text-white"
                      else "px-6 py-3 rounded-lg font-medium transition-colors bg-gray-300 text-gray-500 cursor-not-allowed"
                    Html.button [
                      prop.className startBtnClasses
                      prop.text "Start Game"
                      prop.disabled (not canStart)
                      prop.onClick startGame
                    ]
                  ]
                ]
              ]
            ]
          ]
        ]
      ]
    ]
