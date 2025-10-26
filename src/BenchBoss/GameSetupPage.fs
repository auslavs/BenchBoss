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
    let maxStarters = state.FieldPlayerTarget // cap equals configured on-field target

    // Ensure starters never exceed maxStarters if target reduced mid-setup
    React.useEffect((fun () ->
      if starters.Count > maxStarters then
        let trimmed = starters |> Seq.take maxStarters |> Set.ofSeq
        setStarters trimmed
    ), [| box starters; box maxStarters |])

    let toggleSelect (pid:PlayerId) =
      if selectedPlayers.Contains pid then
        // Deselect also removes starter designation if present
        setSelectedPlayers (selectedPlayers.Remove pid)
        if starters.Contains pid then setStarters (starters.Remove pid)
      else
        setSelectedPlayers (selectedPlayers.Add pid)

    let toggleStarter (pid:PlayerId) =
      if starters.Contains pid then
        // Demote to bench (still selected if checkbox checked)
        setStarters (starters.Remove pid)
      else if starters.Count < maxStarters then
        // Promote to starter only if capacity available
        setStarters (starters.Add pid)
      else
        // Ignore clicks when at capacity (button will appear disabled visually)
        ()

    let roster = state.TeamPlayers

    // Removed mandatory 4 starters requirement. A game can start with any selected players (must have at least one).
    let canStart = not (Set.isEmpty selectedPlayers)

    let startGame _ =
      if canStart then
        // Determine starting players respecting FieldPlayerTarget.
        // If user marked more starters than allowed, overflow becomes bench.
        // We preserve roster order for deterministic selection.
        let rosterStarters =
          if starters.Count = 0 then []
          else roster |> List.filter (fun tp -> starters.Contains tp.Id)
        let maxStarting = state.FieldPlayerTarget
        let startingPlayers =
          if List.isEmpty rosterStarters then []
          else rosterStarters |> List.take (min maxStarting rosterStarters.Length) |> List.map _.Id
        let startingSet = startingPlayers |> Set.ofList
        // Bench = all selected players not in (trimmed) starting list (includes any overflow starters)
        let benchPlayers = selectedPlayers |> Set.filter (fun id -> not (startingSet.Contains id)) |> Set.toList
        dispatch (StartNewGame (startingPlayers, benchPlayers))

    Html.div [
      prop.className "flex-1 p-6 bg-gradient-to-b from-purple-50 to-indigo-50"
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
                    Html.div [ prop.className "px-3 py-2 rounded bg-purple-50 text-purple-700 font-medium"; prop.text $"Selected: {selectedPlayers.Count}" ]
                    Html.div [ prop.className "px-3 py-2 rounded bg-green-50 text-green-700 font-medium"; prop.text $"Starters: {starters.Count}" ]
                    Html.div [
                      prop.className "flex items-center gap-2 px-3 py-2 rounded bg-blue-50 text-blue-700"
                      prop.children [
                        Html.span [ prop.className "font-medium"; prop.text "Players on field" ]
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
                      // Unified purple outlined style per user request (white background always)
                      let starterBtnClasses =
                        if isStarter then "text-xs px-2 py-1 rounded border border-purple-600 text-purple-700 bg-white hover:bg-purple-50"
                        elif starters.Count >= maxStarters then "text-xs px-2 py-1 rounded border border-purple-300 text-purple-300 bg-white cursor-not-allowed"
                        else "text-xs px-2 py-1 rounded border border-purple-600 text-purple-700 bg-white hover:bg-purple-50"
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
                              if isSelected then
                                Html.button [
                                  prop.className starterBtnClasses
                                  prop.text (
                                    if isStarter then "Bench"
                                    elif starters.Count >= maxStarters then $"Max {maxStarters} Starters"
                                    else "Set Starter"
                                  )
                                  prop.disabled (not isStarter && starters.Count >= maxStarters)
                                  prop.onClick (fun _ -> toggleStarter p.Id)
                                  prop.title (
                                    if isStarter then "Click to move this player to bench at game start"
                                    elif starters.Count >= maxStarters then "Starter capacity reached. Demote a starter to free a spot."
                                    else "Mark player to begin on field"
                                  )
                                ]
                              else Html.none
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
