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
      let circle stepNumber label state =
        let baseClasses = "flex h-10 w-10 items-center justify-center rounded-full text-base font-semibold transition-all duration-200"
        let variantClasses =
          match state with
          | "active" -> "bg-gradient-to-br from-purple-500 via-purple-600 to-purple-700 text-white shadow-lg shadow-purple-300"
          | "completed" -> "bg-white text-purple-600 border border-purple-200 shadow-inner shadow-purple-100"
          | _ -> "bg-purple-100 text-purple-500 border border-transparent"
        Html.div [
          prop.className $"{baseClasses} {variantClasses}"
          prop.children [ Html.span [ prop.text (string stepNumber) ] ]
          prop.title label
        ]

      let connector variant =
        let connectorClasses =
          match variant with
          | "active" -> "bg-gradient-to-r from-purple-400 to-purple-200"
          | "completed" -> "bg-purple-200"
          | _ -> "bg-purple-100"
        Html.div [
          prop.className $"h-1 flex-1 rounded-full transition-all duration-200 {connectorClasses}"
        ]

      let stepState step =
        match currentStep, step with
        | SelectPlayers, SelectPlayers -> "active"
        | ChooseLineup, SelectPlayers -> "completed"
        | ChooseLineup, ChooseLineup -> "active"
        | _ -> "upcoming"

      Html.div [
        prop.className "flex items-center gap-4"
        prop.children [
          circle 1 "Select Players" (stepState SelectPlayers)
          connector (if currentStep = ChooseLineup then "active" else "upcoming")
          circle 2 "Organize Lineup" (stepState ChooseLineup)
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
        prop.className "border border-purple-100 rounded-2xl bg-gradient-to-b from-purple-50 to-white p-4 sm:p-5 space-y-3.5"
        prop.children [
          Html.div [
            prop.className "flex items-start justify-between gap-3"
            prop.children [
              Html.div [
                prop.className "space-y-0.5"
                prop.children [
                  Html.h3 [ prop.className "text-sm font-semibold text-purple-900"; prop.text title ]
                  Html.p [ prop.className "text-xs text-purple-500"; prop.text description ]
                ]
              ]
              Html.div [
                prop.className "px-3 py-1 rounded-full bg-white border border-purple-200 text-xs font-semibold text-purple-700"
                prop.text (string count)
              ]
            ]
          ]
          if List.isEmpty items then
            Html.div [
              prop.className "text-xs text-purple-500 bg-white border border-dashed border-purple-200 rounded-xl p-3.5 text-center"
              prop.text emptyMessage
            ]
          else
            Html.div [
              prop.className contentClasses
              prop.children (items |> List.map renderItem)
            ]
        ]
      ]

    let personIcon  (iconClass: string) =
      Svg.svg [
        svg.className iconClass
        svg.viewBox (0, 0, 24, 24)
        svg.fill "none"
        svg.stroke "currentColor"
        svg.strokeWidth 1.6
        svg.strokeLineCap "round"
        svg.strokeLineJoin "round"
        svg.children [
          Svg.circle [
            svg.cx 12
            svg.cy 8
            svg.r 4
          ]
          Svg.path [
            svg.d "M6 20c0-3.3 2.7-6 6-6s6 2.7 6 6"
          ]
        ]
      ]

    let checkIcon  (iconClass: string) =
      Svg.svg [
        svg.className iconClass
        svg.viewBox (0, 0, 24, 24)
        svg.fill "none"
        svg.stroke "currentColor"
        svg.strokeWidth 2
        svg.strokeLineCap "round"
        svg.strokeLineJoin "round"
        svg.children [
          Svg.path [ svg.d "M5 12.5l3 3L19 7" ]
        ]
      ]

    let closeIcon  (iconClass: string) =
      Svg.svg [
        svg.className iconClass
        svg.viewBox (0, 0, 24, 24)
        svg.fill "none"
        svg.stroke "currentColor"
        svg.strokeWidth 2
        svg.strokeLineCap "round"
        svg.strokeLineJoin "round"
        svg.children [
          Svg.path [ svg.d "M6 6l12 12" ]
          Svg.path [ svg.d "M6 18L18 6" ]
        ]
      ]

    let plusIcon  (iconClass: string) =
      Svg.svg [
        svg.className iconClass
        svg.viewBox (0, 0, 24, 24)
        svg.fill "none"
        svg.stroke "currentColor"
        svg.strokeWidth 2
        svg.strokeLineCap "round"
        svg.strokeLineJoin "round"
        svg.children [
          Svg.path [ svg.d "M12 5v14" ]
          Svg.path [ svg.d "M5 12h14" ]
        ]
      ]

    let starIconFilled  (iconClass: string) =
      Svg.svg [
        svg.className iconClass
        svg.viewBox (0, 0, 24, 24)
        svg.fill "currentColor"
        svg.stroke "none"
        svg.children [
          Svg.path [ svg.d "M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.86L12 17.77l-6.18 3.23L7 14.14 2 9.27l6.91-1.01L12 2z" ]
        ]
      ]

    let starIconOutline  (iconClass: string) =
      Svg.svg [
        svg.className iconClass
        svg.viewBox (0, 0, 24, 24)
        svg.fill "none"
        svg.stroke "currentColor"
        svg.strokeWidth 1.5
        svg.strokeLineCap "round"
        svg.strokeLineJoin "round"
        svg.children [
          Svg.path [ svg.d "M12 3l2.3 4.66 5.14.75-3.72 3.63.88 5.09L12 14.9l-4.6 2.4.88-5.09-3.72-3.63 5.14-.75L12 3z" ]
        ]
      ]

    let benchIcon  (iconClass: string) =
      Svg.svg [
        svg.className iconClass
        svg.viewBox (0, 0, 24, 24)
        svg.fill "none"
        svg.stroke "currentColor"
        svg.strokeWidth 1.6
        svg.strokeLineCap "round"
        svg.strokeLineJoin "round"
        svg.children [
          Svg.path [
            svg.d "M4 9h16a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1h-2l1.2 3H18l-1.1-3H7.1L6 17H4.8L6 14H4a1 1 0 0 1-1-1v-3a1 1 0 0 1 1-1z"
          ]
          Svg.path [
            svg.d "M8 9V7h8v2"
          ]
        ]
      ]

    let arrowRightIcon  (iconClass: string) =
      Svg.svg [
        svg.className iconClass
        svg.viewBox (0, 0, 24, 24)
        svg.fill "none"
        svg.stroke "currentColor"
        svg.strokeWidth 2
        svg.strokeLineCap "round"
        svg.strokeLineJoin "round"
        svg.children [
          Svg.path [ svg.d "M5 12h14" ]
          Svg.path [ svg.d "M13 6l6 6-6 6" ]
        ]
      ]


    Html.div [
      prop.className "min-h-screen bg-gradient-to-br from-purple-50 via-white to-blue-50 py-8"
      prop.children [
        Html.div [
          prop.className "w-full max-w-2xl mx-auto mb-6 px-4"
          prop.children [
            Html.div [
              prop.className "flex items-center justify-between"
              prop.children [
                Html.h1 [ prop.className "text-2xl font-semibold text-purple-900"; prop.text "Game Setup" ]
                stepIndicator
              ]
            ]
            match currentStep with
            | SelectPlayers ->
              Html.div [
                prop.className "max-w-2xl mx-auto rounded-3xl border border-purple-100 bg-white/95 p-8 shadow-xl shadow-purple-100/60 backdrop-blur space-y-6"
                prop.children [
                  Html.div [
                    prop.className "space-y-2"
                    prop.children [
                      Html.h2 [ prop.className "text-lg font-semibold text-purple-900"; prop.text "Select Players" ]
                      Html.p [ prop.className "text-sm text-gray-500"; prop.text "Choose which players are attending this game." ]
                    ]
                  ]
                  Html.div [
                    prop.className "rounded-2xl border border-purple-100 bg-gradient-to-br from-purple-50 via-white to-purple-50 p-6 text-center shadow-inner"
                    prop.children [
                      Html.div [ prop.className "text-4xl font-semibold text-purple-600"; prop.text (string selectedPlayers.Count) ]
                      Html.div [ prop.className "mt-2 text-sm font-medium text-purple-500"; prop.text "Players Selected" ]
                    ]
                  ]
                  Html.div [
                    prop.className "space-y-6"
                    prop.children [
                      Html.div [
                        prop.className "space-y-3"
                        prop.children [
                          Html.div [
                            prop.className "flex items-center justify-between gap-3"
                            prop.children [
                              Html.div [
                                prop.className "flex items-center gap-3"
                                prop.children [
                                  Html.div [
                                    prop.className "flex h-10 w-10 items-center justify-center rounded-2xl bg-purple-100 text-purple-600"
                                    prop.children [ checkIcon "h-5 w-5" ]
                                  ]
                                  Html.div [
                                    prop.className "space-y-0.5"
                                    prop.children [
                                      Html.h3 [ prop.className "text-xs font-semibold uppercase tracking-wide text-purple-800"; prop.text "Playing Today" ]
                                      Html.p [ prop.className "text-xs text-purple-400"; prop.text "Tap a player to move them back to the roster." ]
                                    ]
                                  ]
                                ]
                              ]
                              Html.div [
                                prop.className "rounded-full bg-purple-100 px-3 py-1 text-xs font-semibold text-purple-700"
                                prop.text (string selectedRoster.Length)
                              ]
                            ]
                          ]
                          Html.div [
                            prop.className "space-y-2"
                            prop.children (
                              if List.isEmpty selectedRoster then
                                [ Html.div [
                                    prop.className "rounded-2xl border border-dashed border-purple-200 bg-white py-6 text-center text-xs text-purple-400"
                                    prop.text "No players selected yet"
                                  ] ]
                              else
                                selectedRoster
                                |> List.map (fun player ->
                                  Html.div [
                                    prop.key (string player.Id)
                                    prop.role "button"
                                    prop.tabIndex 0
                                    prop.className "group flex items-center justify-between rounded-2xl border border-transparent bg-white px-4 py-3 shadow-sm transition hover:-translate-y-0.5 hover:border-purple-200 hover:shadow-lg focus:outline-none focus:ring-2 focus:ring-purple-300"
                                    prop.onClick (fun _ -> toggleSelect player.Id)
                                    prop.children [
                                      Html.span [ prop.className "text-sm font-medium text-purple-900"; prop.text player.Name ]
                                      Html.button [
                                        prop.type' "button"
                                        prop.tabIndex -1
                                        prop.className "inline-flex h-9 w-9 items-center justify-center rounded-xl border border-red-100 text-red-500 transition hover:bg-red-50 hover:text-red-600 group-hover:border-red-200"
                                        prop.ariaLabel ($"Remove {player.Name}")
                                        prop.onClick (fun e ->
                                          e.stopPropagation()
                                          toggleSelect player.Id)
                                        prop.children [ closeIcon "h-4 w-4" ]
                                      ]
                                    ]
                                  ])
                            )
                          ]
                        ]
                      ]
                      Html.div [
                        prop.className "space-y-3"
                        prop.children [
                          Html.div [
                            prop.className "flex items-center justify-between gap-3"
                            prop.children [
                              Html.div [
                                prop.className "flex items-center gap-3"
                                prop.children [
                                  Html.div [
                                    prop.className "flex h-10 w-10 items-center justify-center rounded-2xl bg-gray-100 text-gray-500"
                                    prop.children [ personIcon "h-5 w-5" ]
                                  ]
                                  Html.div [
                                    prop.className "space-y-0.5"
                                    prop.children [
                                      Html.h3 [ prop.className "text-xs font-semibold uppercase tracking-wide text-gray-700"; prop.text "Roster" ]
                                      Html.p [ prop.className "text-xs text-gray-400"; prop.text "Add players who are available for today." ]
                                    ]
                                  ]
                                ]
                              ]
                              Html.div [
                                prop.className "rounded-full bg-gray-100 px-3 py-1 text-xs font-semibold text-gray-600"
                                prop.text (string availableRoster.Length)
                              ]
                            ]
                          ]
                          Html.div [
                            prop.className "space-y-2"
                            prop.children (
                              if List.isEmpty availableRoster then
                                [ Html.div [
                                    prop.className "rounded-2xl border border-dashed border-gray-200 bg-gray-50 py-6 text-center text-xs text-gray-400"
                                    prop.text "All players selected"
                                  ] ]
                              else
                                availableRoster
                                |> List.map (fun player ->
                                  Html.div [
                                    prop.key (string player.Id)
                                    prop.role "button"
                                    prop.tabIndex 0
                                    prop.className "group flex items-center justify-between rounded-2xl border border-transparent bg-white px-4 py-3 shadow-sm transition hover:-translate-y-0.5 hover:border-purple-200 hover:shadow-lg focus:outline-none focus:ring-2 focus:ring-purple-300"
                                    prop.onClick (fun _ -> toggleSelect player.Id)
                                    prop.children [
                                      Html.span [ prop.className "text-sm font-medium text-gray-700"; prop.text player.Name ]
                                      Html.button [
                                        prop.type' "button"
                                        prop.tabIndex -1
                                        prop.className "inline-flex h-9 w-9 items-center justify-center rounded-xl border border-purple-100 text-purple-500 transition hover:bg-purple-50 hover:text-purple-600 group-hover:border-purple-200"
                                        prop.ariaLabel ($"Add {player.Name}")
                                        prop.onClick (fun e ->
                                          e.stopPropagation()
                                          toggleSelect player.Id)
                                        prop.children [ plusIcon "h-4 w-4" ]
                                      ]
                                    ]
                                  ])
                            )
                          ]
                        ]
                      ]
                    ]
                  ]
                  Html.button [
                    prop.type' "button"
                    prop.disabled (not canAdvance)
                    prop.className (
                      if canAdvance then
                        "group relative flex w-full items-center justify-center gap-3 rounded-2xl bg-gradient-to-r from-purple-500 via-purple-600 to-purple-700 px-6 py-4 text-base font-semibold text-white shadow-lg shadow-purple-200 transition hover:from-purple-500 hover:via-purple-500 hover:to-purple-600 focus:outline-none focus:ring-2 focus:ring-purple-400"
                      else
                        "group relative flex w-full items-center justify-center gap-3 rounded-2xl bg-gray-200 px-6 py-4 text-base font-semibold text-gray-500 shadow-none cursor-not-allowed"
                    )
                    prop.onClick (fun _ ->
                      let orderedSelection = selectedRoster |> List.map _.Id
                      setupDispatch (AdvanceToLineup orderedSelection))
                    prop.children [
                      Html.span [ prop.text "Continue to Lineup" ]
                      arrowRightIcon (
                        if canAdvance then
                          "h-5 w-5 transition-transform group-hover:translate-x-1"
                        else
                          "h-5 w-5 text-gray-400"
                      )
                    ]
                  ]
                ]
              ]
            | ChooseLineup ->
              Html.div [
                prop.className "max-w-2xl mx-auto rounded-3xl border border-purple-100 bg-white/95 p-8 shadow-xl shadow-purple-100 backdrop-blur space-y-6"
                prop.children [
                  Html.div [
                    prop.className "space-y-2"
                    prop.children [
                      Html.h2 [ prop.className "text-lg font-semibold text-purple-900"; prop.text "Organize your lineup for the game." ]
                      Html.p [ prop.className "text-sm text-gray-500"; prop.text "Choose who starts on the field and who opens on the bench." ]
                    ]
                  ]
                  Html.div [
                    prop.className "grid gap-4 sm:grid-cols-2"
                    prop.children [
                      Html.div [
                        prop.className "rounded-2xl border border-blue-100 bg-gradient-to-br from-blue-50 via-white to-blue-50 p-5 shadow-inner"
                        prop.children [
                          Html.label [ prop.className "block text-xs font-semibold uppercase tracking-wide text-blue-700"; prop.text "Players on Field" ]
                          Html.select [
                            prop.className "mt-3 h-12 w-full rounded-xl border border-blue-200 bg-white px-3 text-sm text-gray-700 shadow-sm focus:outline-none focus:ring-2 focus:ring-purple-400"
                            prop.value p.FieldPlayerTarget
                            prop.onChange (fun (value: string) ->
                              match System.Int32.TryParse value with
                              | true, v -> p.SetFieldPlayerTarget v
                              | _ -> ())
                            prop.children [
                              for n in 4..11 ->
                                Html.option [
                                  prop.value n
                                  prop.text (if n = 1 then "1 player at a time" else $"{n} players at a time")
                                ]
                            ]
                          ]
                        ]
                      ]
                      Html.div [
                        prop.className "flex flex-col items-center justify-center rounded-2xl border border-purple-100 bg-gradient-to-br from-purple-50 via-white to-purple-50 p-5 text-center shadow-inner"
                        prop.children [
                          Html.div [ prop.className "text-4xl font-semibold text-purple-600"; prop.text (string selectedPlayers.Count) ]
                          Html.div [ prop.className "mt-2 text-sm font-medium text-purple-500"; prop.text "Players Selected" ]
                        ]
                      ]
                    ]
                  ]
                  Html.div [
                    prop.className "space-y-3"
                    prop.children [
                      Html.div [
                        prop.className "flex items-center justify-between gap-3"
                        prop.children [
                          Html.div [
                            prop.className "flex items-center gap-3"
                            prop.children [
                              Html.div [
                                prop.className "flex h-10 w-10 items-center justify-center rounded-2xl bg-purple-100 text-purple-600"
                                prop.children [ starIconFilled "h-5 w-5" ]
                              ]
                              Html.div [
                                prop.className "space-y-0.5"
                                prop.children [
                                  Html.h3 [ prop.className "text-xs font-semibold uppercase tracking-wide text-purple-800"; prop.text "Starting Lineup" ]
                                  Html.p [ prop.className "text-xs text-purple-400"; prop.text "Tap a player to move them back to the bench." ]
                                ]
                              ]
                            ]
                          ]
                          Html.div [
                            prop.className "rounded-full bg-purple-100 px-3 py-1 text-xs font-semibold text-purple-700"
                            prop.text (string starterPlayers.Length)
                          ]
                        ]
                      ]
                      Html.div [
                        prop.className "space-y-3 rounded-2xl border border-purple-200 bg-gradient-to-br from-white via-purple-50 to-white p-4 shadow-sm"
                        prop.children (
                          if List.isEmpty starterPlayers then
                            [ Html.div [
                                prop.className "rounded-xl border border-dashed border-purple-200 bg-white py-6 text-center text-xs text-purple-400"
                                prop.text "No starters selected yet"
                              ] ]
                          else
                            starterPlayers
                            |> List.map (fun player ->
                              Html.div [
                                prop.key (string player.Id)
                                prop.role "button"
                                prop.tabIndex 0
                                prop.className "group flex items-center justify-between rounded-2xl border border-transparent bg-white/95 px-4 py-3 shadow-sm transition hover:border-purple-200 hover:shadow-md focus:outline-none focus:ring-2 focus:ring-purple-300 cursor-pointer"
                                prop.onClick (fun _ -> toggleStarter player.Id)
                                prop.children [
                                  Html.div [
                                    prop.className "flex items-center gap-3"
                                    prop.children [
                                      Html.div [
                                        prop.className "flex h-9 w-9 items-center justify-center rounded-xl bg-gray-900 text-white shadow-inner shadow-gray-700/40"
                                        prop.children [ checkIcon "h-4 w-4" ]
                                      ]
                                      Html.span [ prop.className "text-sm font-medium text-purple-900"; prop.text player.Name ]
                                    ]
                                  ]
                                  Html.button [
                                    prop.type' "button"
                                    prop.tabIndex -1
                                    prop.className "inline-flex h-9 w-9 items-center justify-center rounded-xl border border-purple-100 bg-white text-purple-500 transition hover:bg-purple-50 group-hover:border-purple-200 group-hover:text-purple-600"
                                    prop.title "Move to bench"
                                    prop.ariaLabel ($"Move {player.Name} to bench")
                                    prop.onClick (fun e ->
                                      e.stopPropagation()
                                      toggleStarter player.Id)
                                    prop.children [ benchIcon "h-4 w-4" ]
                                  ]
                                ]
                              ])
                        )
                      ]
                    ]
                  ]
                  Html.div [
                    prop.className "space-y-3"
                    prop.children [
                      Html.div [
                        prop.className "flex items-center justify-between gap-3"
                        prop.children [
                          Html.div [
                            prop.className "flex items-center gap-3"
                            prop.children [
                              Html.div [
                                prop.className "flex h-10 w-10 items-center justify-center rounded-2xl bg-purple-50 text-purple-500"
                                prop.children [ benchIcon "h-5 w-5" ]
                              ]
                              Html.div [
                                prop.className "space-y-0.5"
                                prop.children [
                                  Html.h3 [ prop.className "text-xs font-semibold uppercase tracking-wide text-purple-800"; prop.text "On Bench" ]
                                  Html.p [ prop.className "text-xs text-purple-400"; prop.text "Promote players to start on the field." ]
                                ]
                              ]
                            ]
                          ]
                          Html.div [
                            prop.className "rounded-full bg-purple-100 px-3 py-1 text-xs font-semibold text-purple-700"
                            prop.text (string benchPlayers.Length)
                          ]
                        ]
                      ]
                      Html.div [
                        prop.className "space-y-3 rounded-2xl border border-purple-100 bg-white/95 p-4 shadow-sm"
                        prop.children (
                          if List.isEmpty benchPlayers then
                            [ Html.div [
                                prop.className "rounded-xl border border-dashed border-purple-200 bg-white py-6 text-center text-xs text-purple-300"
                                prop.text "All players are in the starting lineup"
                              ] ]
                          else
                            benchPlayers
                            |> List.map (fun player ->
                              let disabled = starters.Count >= maxStarters
                              Html.div [
                                prop.key (string player.Id)
                                prop.role "button"
                                prop.tabIndex (if disabled then -1 else 0)
                                prop.className (
                                  if disabled then
                                    "group flex items-center justify-between rounded-2xl border border-transparent bg-gray-50 px-4 py-3 text-gray-400 opacity-70 cursor-not-allowed"
                                  else
                                    "group flex items-center justify-between rounded-2xl border border-transparent bg-white/95 px-4 py-3 shadow-sm transition hover:border-purple-200 hover:shadow-md focus:outline-none focus:ring-2 focus:ring-purple-300 cursor-pointer"
                                )
                                prop.onClick (fun _ ->
                                  if not disabled then toggleStarter player.Id)
                                prop.children [
                                  Html.div [
                                    prop.className "flex items-center gap-3"
                                    prop.children [
                                      Html.div [
                                        prop.className (
                                          if disabled then
                                            "flex h-9 w-9 items-center justify-center rounded-xl bg-gray-200 text-gray-400"
                                          else
                                            "flex h-9 w-9 items-center justify-center rounded-xl bg-gray-900 text-white"
                                        )
                                        prop.children [ checkIcon "h-4 w-4" ]
                                      ]
                                      Html.span [
                                        prop.className (
                                          if disabled then "text-sm font-medium text-gray-400"
                                          else "text-sm font-medium text-purple-900"
                                        )
                                        prop.text player.Name
                                      ]
                                    ]
                                  ]
                                  Html.div [
                                    prop.className "flex items-center gap-2"
                                    prop.children [
                                      if disabled then
                                        Html.span [
                                          prop.className "rounded-full border border-gray-200 px-2 py-0.5 text-[11px] font-medium text-gray-400"
                                          prop.text ($"Max {maxStarters}")
                                        ]
                                      Html.button [
                                        prop.type' "button"
                                        prop.tabIndex (if disabled then -1 else 0)
                                        prop.className (
                                          if disabled then
                                            "inline-flex h-9 w-9 items-center justify-center rounded-xl border border-gray-200 bg-gray-100 text-gray-300"
                                          else
                                            "inline-flex h-9 w-9 items-center justify-center rounded-xl border border-purple-100 bg-white text-purple-500 transition hover:bg-purple-50 group-hover:border-purple-200 group-hover:text-purple-600"
                                        )
                                        prop.disabled disabled
                                        prop.ariaLabel (
                                          if disabled then
                                            $"Starters are capped at {maxStarters}"
                                          else
                                            $"Set {player.Name} as starter"
                                        )
                                        prop.title (
                                          if disabled then $"Starters are capped at {maxStarters}" else "Set as starter"
                                        )
                                        prop.onClick (fun e ->
                                          e.stopPropagation()
                                          if not disabled then toggleStarter player.Id)
                                        prop.children [
                                          starIconOutline "h-4 w-4"
                                        ]
                                      ]
                                    ]
                                  ]
                                ]
                              ])
                        )
                      ]
                    ]
                  ]
                  Html.div [
                    prop.className "flex flex-col-reverse gap-3 sm:flex-row"
                    prop.children [
                      Html.button [
                        prop.type' "button"
                        prop.className "h-14 rounded-2xl border border-purple-200 bg-white text-purple-600 transition hover:bg-purple-50 focus:outline-none focus:ring-2 focus:ring-purple-300 sm:flex-1"
                        prop.text "Back"
                        prop.onClick (fun _ -> setupDispatch BackToSelection)
                      ]
                      Html.button [
                        prop.type' "button"
                        prop.disabled (not canStart)
                        prop.className (
                          if canStart then
                            "group relative flex h-14 items-center justify-center gap-3 rounded-2xl bg-gradient-to-r from-purple-500 via-purple-600 to-purple-700 px-6 text-base font-semibold text-white shadow-lg shadow-purple-200 transition hover:from-purple-500 hover:via-purple-500 hover:to-purple-600 focus:outline-none focus:ring-2 focus:ring-purple-400 sm:flex-1"
                          else
                            "group relative flex h-14 items-center justify-center gap-3 rounded-2xl bg-gray-200 px-6 text-base font-semibold text-gray-500 shadow-none cursor-not-allowed sm:flex-1"
                        )
                        prop.onClick startGame
                        prop.children [
                          Html.span [ prop.text "Start Game" ]
                          arrowRightIcon (
                            if canStart then
                              "h-5 w-5 transition-transform group-hover:translate-x-1"
                            else
                              "h-5 w-5 text-gray-400"
                          )
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
