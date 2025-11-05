namespace BenchBossApp.Components.BenchBoss

module GameSetupPage =

  open Feliz
  open BenchBossApp.Components.BenchBoss.Types
  open Elmish
  open Feliz.UseElmish
  open BenchBossApp.Components.BenchBoss.GameSetupPage
  open BenchBossApp.Components.BenchBoss.Types

  type SetupStep =
    | SelectPlayers of Map<PlayerId,TeamPlayer>
    | ChooseLineup of TeamSetupData

  type SetupModel = {
    TeamPlayers: Map<PlayerId,TeamPlayer>
    Step: SetupStep
  }

  type SetupViewProps = {|
    TeamPlayers: TeamPlayer list
    FieldPlayerTarget: int
    Cancel: unit -> unit
    StartNewGame: TeamSetupData -> unit
  |}

  type SetupMsg =
    | ToggleSelected of PlayerId
    | SelectStarter of PlayerId
    | DeselectStarter of PlayerId
    | AdvanceToLineup of Map<PlayerId,TeamPlayer>
    | SetFieldPlayerTarget of int
    | BackToSelection

  let setupInit (teamPlayers: TeamPlayer list) : SetupModel * Cmd<SetupMsg> =
    {
      TeamPlayers = teamPlayers |> List.map (fun p -> p.Id, p) |> Map.ofList
      Step = SelectPlayers Map.empty
    },
    Cmd.none

  let setupUpdate (initialMaxPlayersOnField: int) (msg: SetupMsg) (model: SetupModel) : SetupModel * Cmd<SetupMsg> =
    match msg with
    | ToggleSelected pid ->
      match model.Step with
      | SelectPlayers state ->
          let newSelected =
            if Map.containsKey pid state then
              Map.remove pid state
            else
              match Map.tryFind pid model.TeamPlayers with
              | Some player -> Map.add pid player state
              | None -> state

          { model with Step = SelectPlayers newSelected }, Cmd.none
      | ChooseLineup _  ->
          model, Cmd.none

    | SelectStarter pId ->
        match model.Step with
        | ChooseLineup state ->
            match Map.tryFind pId state.OnBench with
            | Some player ->
                match TeamSetupData.addToField player state with
                | Ok updatedTeam ->
                    { model with Step = ChooseLineup updatedTeam }, Cmd.none
                | Error err ->
                    printfn "Error adding to field: %A" err
                    model, Cmd.none
            | None -> model, Cmd.none
        | SelectPlayers _  -> model, Cmd.none

    | DeselectStarter pId ->
        match model.Step with
        | ChooseLineup state ->
            match Map.tryFind pId state.OnField with
            | Some player ->
                let updatedTeam = TeamSetupData.addToBench player state
                { model with Step = ChooseLineup updatedTeam }, Cmd.none
            | None -> model, Cmd.none
        | SelectPlayers _  -> model, Cmd.none

    | AdvanceToLineup orderedSelection ->

        let team = TeamSetupData.create initialMaxPlayersOnField orderedSelection
        { model with Step = ChooseLineup team }, Cmd.none

    | BackToSelection ->

        match model.Step with
        | ChooseLineup s ->
            let allSelected =
              Map.fold (fun acc key value -> Map.add key value acc) s.OnField s.OnBench
            { model with Step = SelectPlayers allSelected }, Cmd.none
        | SelectPlayers _ ->
            // Already in selection step, no change
            model, Cmd.none
    | SetFieldPlayerTarget num ->
        match model.Step with
        | ChooseLineup s ->
            let updatedTeam = TeamSetupData.setMaxOnField num s
            { model with Step = ChooseLineup updatedTeam }, Cmd.none
        | SelectPlayers _ ->
            // No change in selection step
            model, Cmd.none

  [<ReactComponent>]
  let View (p: SetupViewProps) =

    let setupModel, setupDispatch = React.useElmish (setupInit p.TeamPlayers, setupUpdate p.FieldPlayerTarget)

    let isSelectPlayers =
      match setupModel.Step with
      | SelectPlayers _ -> true
      | _ -> false

    let isChooseLineup =
      match setupModel.Step with
      | ChooseLineup _ -> true
      | _ -> false

    Html.div [
      prop.className "min-h-screen bg-gradient-to-br from-purple-50 via-white to-blue-50 py-8"
      prop.children [
        // Header
        Html.div [
          prop.className "max-w-lg mx-auto mb-6 px-4"
          prop.children [
            Html.div [
              prop.className "flex items-center justify-between"
              prop.children [
                Html.h1 [ prop.className "text-purple-900"; prop.text "Game Setup" ]
                Html.div [
                  prop.className "flex items-center gap-2"
                  prop.children [
                    Html.div [
                      prop.className [
                        "w-8 h-8 rounded-full flex items-center justify-center"
                        if isSelectPlayers
                        then "bg-purple-600 text-white"
                        else "bg-purple-200 text-purple-600"
                      ]
                      prop.text "1"
                    ]
                    Html.div [ prop.className "w-8 h-px bg-purple-300" ]
                    Html.div [
                      prop.className [
                        "w-8 h-8 rounded-full flex items-center justify-center"
                        if isChooseLineup
                        then "bg-purple-600 text-white"
                        else "bg-purple-200 text-purple-600"
                      ]
                      prop.text "2"
                    ]
                  ]
                ]
              ]
            ]
          ]
        ]

        //Content
        match setupModel.Step with
        | SelectPlayers selectedPlayers ->
            SelectPlayersPage.Component
              {| allPlayers = p.TeamPlayers
                 selectedPlayers = selectedPlayers
                 onContinue = fun selected -> selected |> AdvanceToLineup |> setupDispatch
                 togglePlayer = fun gp -> gp.Id |> ToggleSelected |> setupDispatch |}

        | ChooseLineup detail ->
            OrganizeLineupPage.Component
              {| TeamSetupData = detail
                 SelectStarter = fun pid -> pid |> SelectStarter |> setupDispatch
                 DeselectStarter = fun pid -> pid |> DeselectStarter |> setupDispatch
                 SetFieldPlayerTarget = fun num -> num |> SetFieldPlayerTarget |> setupDispatch
                 onBack = fun () -> BackToSelection |> setupDispatch
                 onStartGame = p.StartNewGame |}
      ]     
  ]
