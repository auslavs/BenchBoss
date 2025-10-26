namespace BenchBossApp.Components.BenchBoss

module GameSetupPage =

  open Feliz
  open BenchBossApp.Components.BenchBoss.Types
  open Elmish
  open Feliz.UseElmish

  type SetupStep =
    | SelectPlayers
    | ChooseLineup

  type SetupModel =
    { Step: SetupStep
      Selected: Set<PlayerId>
      Starters: Set<PlayerId> }

  type SetupViewProps =
    {| TeamPlayers: TeamPlayer list
       GamePlayers: GamePlayer list
       FieldPlayerTarget: int
       Cancel: unit -> unit
       SetFieldPlayerTarget: int -> unit
       StartNewGame: PlayerId list * PlayerId list -> unit |}

  type SetupMsg =
    | ToggleSelected of PlayerId
    | ToggleStarter of PlayerId
    | AdvanceToLineup of PlayerId list
    | BackToSelection

  let private enforceCap (cap: int) (m: SetupModel) =
    if m.Starters.Count > cap then
      let trimmed =
        m.Starters
        |> Seq.truncate cap
        |> Set.ofSeq
      { m with Starters = trimmed }
    else
      m

  let setupInit (gamePlayers: GamePlayer list, cap: int) : SetupModel * Cmd<SetupMsg> =
    let initialSelected = gamePlayers |> List.map _.Id |> Set.ofList
    let initialStarters =
      gamePlayers
      |> List.filter (fun gp -> gp.InGameStatus = OnField)
      |> List.map _.Id
      |> Set.ofList

    { Step = SelectPlayers
      Selected = initialSelected
      Starters = initialStarters }
    |> enforceCap cap,
    Cmd.none

  let setupUpdate (cap: int) (msg: SetupMsg) (model: SetupModel) : SetupModel * Cmd<SetupMsg> =
    match msg with
    | ToggleSelected pid ->
      if model.Selected.Contains pid then
        { model with
            Selected = model.Selected.Remove pid
            Starters = model.Starters.Remove pid }
        |> enforceCap cap,
        Cmd.none
      else
        { model with Selected = model.Selected.Add pid }
        |> enforceCap cap,
        Cmd.none
    | ToggleStarter pid ->
      if model.Starters.Contains pid then
        { model with Starters = model.Starters.Remove pid } |> enforceCap cap, Cmd.none
      elif model.Starters.Count < cap && model.Selected.Contains pid then
        { model with Starters = model.Starters.Add pid } |> enforceCap cap, Cmd.none
      else
        model, Cmd.none
    | AdvanceToLineup orderedSelection ->
      let selectedSet = model.Selected
      let sanitizedStarters = model.Starters |> Set.filter (fun id -> selectedSet.Contains id)
      let modelWithSanitized = { model with Starters = sanitizedStarters }
      let finalModel =
        if modelWithSanitized.Starters.Count > 0 then
          modelWithSanitized
        else
          let orderedCandidates = orderedSelection |> List.filter (fun pid -> selectedSet.Contains pid)
          let rec takeUpTo n items acc =
            match n, items with
            | 0, _ | _, [] -> List.rev acc
            | _, x :: xs -> takeUpTo (n - 1) xs (x :: acc)
          let defaults = takeUpTo cap orderedCandidates [] |> Set.ofList
          { modelWithSanitized with Starters = defaults }
      { finalModel with Step = ChooseLineup } |> enforceCap cap, Cmd.none
    | BackToSelection ->
      { model with Step = SelectPlayers } |> enforceCap cap, Cmd.none

  [<ReactComponent>]
  let View (p: SetupViewProps) =
    let setupModel, setupDispatch = React.useElmish(setupInit (p.GamePlayers, p.FieldPlayerTarget), setupUpdate p.FieldPlayerTarget)
    let selectedPlayers = setupModel.Selected
    let starters = setupModel.Starters
    let currentStep = setupModel.Step
    let maxStarters = p.FieldPlayerTarget

    let toggleSelect pid = setupDispatch (ToggleSelected pid)
    let toggleStarter pid = setupDispatch (ToggleStarter pid)

    let roster = p.TeamPlayers |> List.sortBy _.Name
    let selectedRoster = roster |> List.filter (fun tp -> selectedPlayers.Contains tp.Id)
    let availableRoster = roster |> List.filter (fun tp -> not (selectedPlayers.Contains tp.Id))
    let starterPlayers = selectedRoster |> List.filter (fun tp -> starters.Contains tp.Id)
    let benchPlayers = selectedRoster |> List.filter (fun tp -> not (starters.Contains tp.Id))

    let canStart = not (Set.isEmpty selectedPlayers)
    let canAdvance = canStart

    let startGame _ =
      if canStart then
        let orderedSelected = selectedRoster |> List.map _.Id
        let orderedStarters = orderedSelected |> List.filter (fun pid -> starters.Contains pid)
        let rec takeUpTo n items acc =
          match n, items with
          | 0, _ | _, [] -> List.rev acc
          | _, x :: xs -> takeUpTo (n - 1) xs (x :: acc)
        let startingPlayers =
          if List.isEmpty orderedStarters then
            takeUpTo p.FieldPlayerTarget orderedSelected []
          else
            takeUpTo p.FieldPlayerTarget orderedStarters []
        let startingSet = startingPlayers |> Set.ofList
        let benchPlayersOrdered = orderedSelected |> List.filter (fun id -> not (startingSet.Contains id))
        p.StartNewGame (startingPlayers, benchPlayersOrdered)

    let stepIndicator =
      let circle stepNumber label isActive isCompleted =
        let baseClasses = "flex items-center justify-center w-9 h-9 rounded-full text-sm font-semibold border"
        let activeClasses =
          if isActive then "bg-purple-600 text-white border-purple-600"
          elif isCompleted then "bg-purple-100 text-purple-600 border-purple-200"
          else "bg-gray-200 text-gray-500 border-gray-200"
        Html.div [
          prop.className $"{baseClasses} {activeClasses}"
          prop.children [ Html.span [ prop.text (string stepNumber) ] ]
          prop.title label
        ]

      let connector isActive =
        Html.div [
          prop.className $"h-0.5 flex-1 rounded-full {(if isActive then "bg-purple-400" else "bg-gray-200")}"
        ]

      let isStepActive step =
        match currentStep, step with
        | SelectPlayers, SelectPlayers
        | ChooseLineup, ChooseLineup -> true
        | _ -> false

      let isStepCompleted step = currentStep = ChooseLineup && step = SelectPlayers

      Html.div [
        prop.className "flex items-center gap-3 w-full max-w-xs"
        prop.children [
          circle 1 "Select Players" (isStepActive SelectPlayers) (isStepCompleted SelectPlayers)
          connector (currentStep = ChooseLineup)
          circle 2 "Choose Lineup" (isStepActive ChooseLineup) false
        ]
      ]

    let rosterListSection
      (title: string)
      (description: string)
      (count: int)
      (contentClasses: string)
      items
      renderItem
      (emptyMessage: string)
      =
      Html.div [
        prop.className "border border-purple-100 rounded-2xl bg-gradient-to-b from-purple-50 to-white p-5 space-y-4"
        prop.children [
          Html.div [
            prop.className "flex items-start justify-between gap-4"
            prop.children [
              Html.div [
                prop.className "space-y-1"
                prop.children [
                  Html.h3 [ prop.className "font-semibold text-purple-900"; prop.text title ]
                  Html.p [ prop.className "text-sm text-purple-500"; prop.text description ]
                ]
              ]
              Html.div [
                prop.className "px-3 py-1 rounded-full bg-white border border-purple-200 text-sm font-medium text-purple-700"
                prop.text (string count)
              ]
            ]
          ]
          if List.isEmpty items then
            Html.div [
              prop.className "text-sm text-purple-500 bg-white border border-dashed border-purple-200 rounded-xl p-4 text-center"
              prop.text emptyMessage
            ]
          else
            Html.div [
              prop.className contentClasses
              prop.children (items |> List.map renderItem)
            ]
        ]
      ]

    Html.div [
      prop.className "flex-1 p-6 bg-gradient-to-b from-purple-50 to-indigo-50"
      prop.children [
        Html.div [
          prop.className "max-w-4xl mx-auto space-y-8"
          prop.children [
            Html.div [
              prop.className "flex flex-col gap-4 md:flex-row md:items-center md:justify-between"
              prop.children [
                Html.div [
                  prop.className "space-y-2"
                  prop.children [
                    Html.h1 [ prop.className "text-2xl font-bold text-purple-900"; prop.text "Game Setup" ]
                    Html.p [
                      prop.className "text-sm text-purple-500"
                      prop.text (
                        match currentStep with
                        | SelectPlayers -> "Choose which players are attending this game."
                        | ChooseLineup -> "Organize your lineup for the start of the game."
                      )
                    ]
                  ]
                ]
                Html.div [ stepIndicator ]
                Html.button [
                  prop.className "text-sm text-gray-600 hover:underline"
                  prop.text "Cancel"
                  prop.onClick (fun _ -> p.Cancel ())
                ]
              ]
            ]
            match currentStep with
            | SelectPlayers ->
              Html.div [
                prop.className "bg-white rounded-2xl shadow-xl p-8 space-y-8"
                prop.children [
                  Html.div [
                    prop.className "flex items-center justify-between"
                    prop.children [
                      Html.div [
                        prop.className "space-y-1"
                        prop.children [
                          Html.h2 [ prop.className "text-lg font-semibold text-purple-900"; prop.text "Select Players" ]
                          Html.p [ prop.className "text-sm text-purple-500"; prop.text "Choose which players are attending this game." ]
                        ]
                      ]
                      Html.div [
                        prop.className "px-4 py-2 rounded-full bg-purple-50 text-purple-600 font-medium"
                        prop.text $"{selectedPlayers.Count} Players Selected"
                      ]
                    ]
                  ]
                  Html.div [
                    prop.className "grid gap-6 lg:grid-cols-2"
                    prop.children [
                      rosterListSection
                        "Playing Today"
                        "Remove players if they won't be at the game."
                        selectedRoster.Length
                        "space-y-3"
                        selectedRoster
                        (fun player ->
                          Html.div [
                            prop.key (string player.Id)
                            prop.className "flex items-center justify-between bg-white border border-purple-100 rounded-xl px-4 py-3 shadow-sm"
                            prop.children [
                              Html.span [ prop.className "font-medium text-purple-900"; prop.text player.Name ]
                              Html.button [
                                prop.className "text-sm text-red-500 hover:text-red-600"
                                prop.text "Remove"
                                prop.onClick (fun _ -> toggleSelect player.Id)
                              ]
                            ]
                          ])
                        "Select players from your roster to add them here."
                      rosterListSection
                        "Roster"
                        "Add available players to today's game."
                        availableRoster.Length
                        "space-y-3"
                        availableRoster
                        (fun player ->
                          Html.div [
                            prop.key (string player.Id)
                            prop.className "flex items-center justify-between bg-white border border-purple-100 rounded-xl px-4 py-3 shadow-sm"
                            prop.children [
                              Html.span [ prop.className "font-medium text-purple-900"; prop.text player.Name ]
                              Html.button [
                                prop.className "text-sm text-purple-600 hover:text-purple-700"
                                prop.text "Add"
                                prop.onClick (fun _ -> toggleSelect player.Id)
                              ]
                            ]
                          ])
                        "Everyone on your roster is already marked as playing."
                    ]
                  ]
                  Html.div [
                    prop.className "flex flex-col gap-3 sm:flex-row sm:justify-end"
                    prop.children [
                      Html.button [
                        prop.className (
                          if canAdvance then
                            "inline-flex items-center justify-center gap-2 px-6 py-3 rounded-xl bg-purple-600 text-white font-medium shadow hover:bg-purple-700 transition-colors"
                          else
                            "inline-flex items-center justify-center gap-2 px-6 py-3 rounded-xl bg-gray-300 text-gray-500 font-medium cursor-not-allowed"
                        )
                        prop.disabled (not canAdvance)
                        prop.text "Continue to Lineup"
                        prop.onClick (fun _ ->
                          let orderedSelection = selectedRoster |> List.map _.Id
                          setupDispatch (AdvanceToLineup orderedSelection))
                      ]
                    ]
                  ]
                ]
              ]
            | ChooseLineup ->
              Html.div [
                prop.className "bg-white rounded-2xl shadow-xl p-8 space-y-8"
                prop.children [
                  Html.div [
                    prop.className "space-y-2"
                    prop.children [
                      Html.h2 [ prop.className "text-lg font-semibold text-purple-900"; prop.text "Organize Lineup" ]
                      Html.p [ prop.className "text-sm text-purple-500"; prop.text "Select starters and confirm who begins on the bench." ]
                    ]
                  ]
                  Html.div [
                    prop.className "flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between"
                    prop.children [
                      Html.div [
                        prop.className "flex items-center gap-3"
                        prop.children [
                          Html.div [
                            prop.className "px-4 py-2 rounded-xl bg-purple-50 text-purple-600 font-medium"
                            prop.text $"{selectedPlayers.Count} Playing"
                          ]
                          Html.div [
                            prop.className "px-4 py-2 rounded-xl bg-green-50 text-green-600 font-medium"
                            prop.text $"{starters.Count} Starters"
                          ]
                        ]
                      ]
                      Html.div [
                        prop.className "flex items-center gap-3"
                        prop.children [
                          Html.label [ prop.className "text-sm font-medium text-purple-700"; prop.text "Players on Field" ]
                          Html.select [
                            prop.className "border border-purple-200 rounded-lg px-3 py-2 text-sm bg-white focus:outline-none focus:ring-2 focus:ring-purple-400"
                            prop.value p.FieldPlayerTarget
                            prop.onChange (fun (value: string) ->
                              match System.Int32.TryParse value with
                              | true, v -> p.SetFieldPlayerTarget v
                              | _ -> ())
                            prop.children [ for n in 4..11 -> Html.option [ prop.value n; prop.text (string n) ] ]
                          ]
                        ]
                      ]
                    ]
                  ]
                  Html.div [
                    prop.className "grid gap-6 lg:grid-cols-2"
                    prop.children [
                      rosterListSection
                        "Starting Lineup"
                        "Players who will begin on the field."
                        starterPlayers.Length
                        "space-y-3"
                        starterPlayers
                        (fun player ->
                          Html.div [
                            prop.key (string player.Id)
                            prop.className "flex items-center justify-between bg-purple-50 border border-purple-100 rounded-xl px-4 py-3 shadow-sm"
                            prop.children [
                              Html.div [
                                prop.className "flex items-center gap-3"
                                prop.children [
                                  Html.input [ prop.type' "checkbox"; prop.isChecked true; prop.disabled true ]
                                  Html.span [ prop.className "font-medium text-purple-900"; prop.text player.Name ]
                                ]
                              ]
                              Html.button [
                                prop.className "text-sm text-purple-600 hover:text-purple-700"
                                prop.text "Move to Bench"
                                prop.onClick (fun _ -> toggleStarter player.Id)
                              ]
                            ]
                          ])
                        "Choose starters from the bench list."
                      rosterListSection
                        "On Bench"
                        "Select players to promote to the starting lineup."
                        benchPlayers.Length
                        "space-y-3"
                        benchPlayers
                        (fun player ->
                          let disabled = starters.Count >= maxStarters
                          Html.div [
                            prop.key (string player.Id)
                            prop.className "flex items-center justify-between bg-white border border-purple-100 rounded-xl px-4 py-3 shadow-sm"
                            prop.children [
                              Html.span [ prop.className "font-medium text-purple-900"; prop.text player.Name ]
                              Html.button [
                                prop.className (
                                  if disabled then
                                    "text-sm text-gray-400 cursor-not-allowed"
                                  else
                                    "text-sm text-purple-600 hover:text-purple-700"
                                )
                                prop.text (if disabled then $"Max {maxStarters} starters" else "Set as Starter")
                                prop.disabled disabled
                                prop.onClick (fun _ -> toggleStarter player.Id)
                              ]
                            ]
                          ])
                        "Everyone selected is currently in the starting lineup."
                    ]
                  ]
                  Html.div [
                    prop.className "flex flex-col gap-3 sm:flex-row sm:justify-between"
                    prop.children [
                      Html.button [
                        prop.className "inline-flex items-center justify-center gap-2 px-6 py-3 rounded-xl border border-purple-200 text-purple-700 font-medium hover:bg-purple-50"
                        prop.text "Back to Selection"
                        prop.onClick (fun _ -> setupDispatch BackToSelection)
                      ]
                      Html.button [
                        prop.className (
                          if canStart then
                            "inline-flex items-center justify-center gap-2 px-6 py-3 rounded-xl bg-purple-600 text-white font-medium shadow hover:bg-purple-700 transition-colors"
                          else
                            "inline-flex items-center justify-center gap-2 px-6 py-3 rounded-xl bg-gray-300 text-gray-500 font-medium cursor-not-allowed"
                        )
                        prop.disabled (not canStart)
                        prop.text "Start Game"
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
