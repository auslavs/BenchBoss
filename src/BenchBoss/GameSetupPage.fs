namespace BenchBossApp.Components.BenchBoss

module GameSetupPage =

  open Feliz
  open BenchBossApp.Components.BenchBoss.Types
  open Elmish
  open Feliz.UseElmish

  type SetupModel = { Selected: Set<PlayerId>; Starters: Set<PlayerId> }

  type SetupMsg = | ToggleSelected of PlayerId | ToggleStarter of PlayerId

  type SetupViewProps =
    {| TeamPlayers: TeamPlayer list
       GamePlayers: GamePlayer list
       FieldPlayerTarget: int
       Cancel: unit -> unit
       SetFieldPlayerTarget: int -> unit
       StartNewGame: PlayerId list * PlayerId list -> unit |}

  let setupInit (gamePlayers: GamePlayer list) : SetupModel * Cmd<SetupMsg> =
    let initialSelected = gamePlayers |> List.map _.Id |> Set.ofList
    { Selected = initialSelected; Starters = Set.empty }, Cmd.none

  let private enforceCap (cap:int) (m:SetupModel) =
    if m.Starters.Count > cap then
      let trimmed = m.Starters |> Seq.take cap |> Set.ofSeq
      { m with Starters = trimmed }
    else m

  let setupUpdate (cap:int) (msg: SetupMsg) (model: SetupModel) : SetupModel * Cmd<SetupMsg> =
    match msg with
    | ToggleSelected pid ->
      if model.Selected.Contains pid then
        { model with Selected = model.Selected.Remove pid; Starters = model.Starters.Remove pid } |> enforceCap cap, Cmd.none
      else
        { model with Selected = model.Selected.Add pid } |> enforceCap cap, Cmd.none
    | ToggleStarter pid ->
      if model.Starters.Contains pid then
        { model with Starters = model.Starters.Remove pid } |> enforceCap cap, Cmd.none
      elif model.Starters.Count < cap && model.Selected.Contains pid then
        { model with Starters = model.Starters.Add pid } |> enforceCap cap, Cmd.none
      else model, Cmd.none

  [<ReactComponent>]
  let View (p: SetupViewProps) =
    let setupModel, setupDispatch = React.useElmish(setupInit p.GamePlayers, setupUpdate p.FieldPlayerTarget)
    let selectedPlayers = setupModel.Selected
    let starters = setupModel.Starters
    let maxStarters = p.FieldPlayerTarget
    let toggleSelect pid = setupDispatch (ToggleSelected pid)
    let toggleStarter pid = setupDispatch (ToggleStarter pid)

    let roster = p.TeamPlayers
    let canStart = not (Set.isEmpty selectedPlayers)

    let startGame _ =
      if canStart then
        let rosterStarters =
          if starters.Count = 0 then [] else roster |> List.filter (fun tp -> starters.Contains tp.Id)
        let maxStarting = p.FieldPlayerTarget
        let startingPlayers =
          if List.isEmpty rosterStarters then []
          else rosterStarters |> List.take (min maxStarting rosterStarters.Length) |> List.map _.Id
        let startingSet = startingPlayers |> Set.ofList
        let benchPlayers = selectedPlayers |> Set.filter (fun id -> not (startingSet.Contains id)) |> Set.toList
        p.StartNewGame (startingPlayers, benchPlayers)

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
                  prop.onClick (fun _ -> p.Cancel ())
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
                    Html.div [ prop.className "px-3 py-2 rounded bg-green-50 text-green-700 font-medium"; prop.text $"Starters: {starters.Count} / {maxStarters}" ]
                    Html.div [
                      prop.className "flex items-center gap-2 px-3 py-2 rounded bg-blue-50 text-blue-700"
                      prop.children [
                        Html.span [ prop.className "font-medium"; prop.text "Players on field" ]
                        Html.select [
                          prop.className "border border-blue-300 rounded px-2 py-1 bg-white text-sm"
                          prop.value p.FieldPlayerTarget
                          prop.onChange (fun (value:string) ->
                            match System.Int32.TryParse value with
                            | true, v -> p.SetFieldPlayerTarget v
                            | _ -> () )
                          prop.children [ for n in 4..11 -> Html.option [ prop.value n; prop.text (string n) ] ]
                        ]
                      ]
                    ]
                  ]
                ]
                Html.div [
                  prop.className "divide-y border rounded-lg overflow-hidden"
                  prop.children [
                    for player in roster ->
                      let isSelected = selectedPlayers.Contains player.Id
                      let isStarter = starters.Contains player.Id
                      let faded = if isSelected then "" else "opacity-60"
                      let rowClasses = $"flex items-center justify-between p-3 hover:bg-gray-50 transition-colors {faded}"
                      let starterBtnClasses =
                        if isStarter then "text-xs px-2 py-1 rounded border border-purple-600 text-purple-700 bg-white hover:bg-purple-50"
                        elif starters.Count >= maxStarters then "text-xs px-2 py-1 rounded border border-purple-300 text-purple-300 bg-white cursor-not-allowed"
                        else "text-xs px-2 py-1 rounded border border-purple-600 text-purple-700 bg-white hover:bg-purple-50"
                      Html.div [
                        prop.key (string player.Id)
                        prop.className rowClasses
                        prop.children [
                          Html.div [
                            prop.className "flex items-center gap-3"
                            prop.children [
                              Html.input [
                                prop.type' "checkbox"
                                prop.isChecked isSelected
                                prop.onChange (fun (_: bool) -> toggleSelect player.Id)
                              ]
                              Html.span [ prop.className "font-medium"; prop.text player.Name ]
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
                                  prop.onClick (fun _ -> toggleStarter player.Id)
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