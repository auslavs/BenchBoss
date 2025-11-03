namespace BenchBossApp.Components.BenchBoss

module GameSetupPage =

  open Feliz
  open BenchBossApp.Components.BenchBoss.Types
  open Elmish
  open Feliz.UseElmish
  open BenchBossApp.Components.BenchBoss.GameSetupPage

  type SetupStep =
    | SelectPlayers of {| SelectedPlayers: TeamPlayer list;  |}
    | ChooseLineup of {| Starters: TeamPlayer list; Bench: TeamPlayer list |}

  type SetupModel = {
    TeamPlayers: TeamPlayer list
    Step: SetupStep
  }

  type SetupViewProps = {|
    TeamPlayers: TeamPlayer list
    FieldPlayerTarget: int
    Cancel: unit -> unit
    SetFieldPlayerTarget: int -> unit
    StartNewGame: PlayerId list * PlayerId list -> unit
  |}

  type SetupMsg =
    | ToggleSelected of PlayerId
    | ToggleStarter of PlayerId
    | AdvanceToLineup of TeamPlayer list
    | BackToSelection

  let setupInit (teamPlayers: TeamPlayer list) : SetupModel * Cmd<SetupMsg> =
    {
      TeamPlayers = teamPlayers
      Step = SelectPlayers {| SelectedPlayers = [] |}
    },
    Cmd.none

  let setupUpdate (maxPlayersOnField: int) (msg: SetupMsg) (model: SetupModel) : SetupModel * Cmd<SetupMsg> =
    match msg with
    | ToggleSelected pid ->

      match model.Step with
      | SelectPlayers state ->
          let isSelected = state.SelectedPlayers |> List.exists (fun p -> p.Id = pid)
          let newSelected =
            if isSelected then
              state.SelectedPlayers |> List.filter (fun p -> p.Id <> pid)
            else
              match model.TeamPlayers |> List.tryFind (fun p -> p.Id = pid) with
              | Some player -> player :: state.SelectedPlayers
              | None -> state.SelectedPlayers

          { model with
              Step = SelectPlayers {| SelectedPlayers = newSelected |}
          }, Cmd.none
      | ChooseLineup _  ->
          // Ignore toggles in ChooseLineup step
          model, Cmd.none

      
    | ToggleStarter pid ->

      match model.Step with
      | ChooseLineup state ->
          let isStarter = state.Starters |> List.exists (fun p -> p.Id = pid)
          let isBench = state.Bench |> List.exists (fun p -> p.Id = pid)

          match isStarter, isBench with
          | true, _ ->
              // Remove from starters
              let newStarters = state.Starters |> List.filter (fun p -> p.Id <> pid)
              let newBench = state.Bench @ (state.Starters |> List.filter (fun p -> p.Id = pid))
              { model with
                  Step = ChooseLineup {| Starters = newStarters; Bench = newBench |}
              }, Cmd.none
          | false, true ->
              // Add to starters if we have room
              if state.Starters.Length < maxPlayersOnField then
                  let playerToAdd = state.Bench |> List.find (fun p -> p.Id = pid)
                  let newStarters = playerToAdd :: state.Starters
                  let newBench = state.Bench |> List.filter (fun p -> p.Id <> pid)
                  { model with
                      Step = ChooseLineup {| Starters = newStarters; Bench = newBench |}
                  }, Cmd.none
              else
                  // Reached max, do not add
                  model, Cmd.none
            | _ ->
              // Should not happen
              model, Cmd.none

      | SelectPlayers _  ->
          // Ignore toggles in SelectPlayers step
          model, Cmd.none

    | AdvanceToLineup orderedSelection ->

      let newStep = ChooseLineup {| Starters = []; Bench = orderedSelection |}
      { model with Step = newStep }, Cmd.none


    | BackToSelection ->

        match model.Step with
        | ChooseLineup s ->
            let newStep = SelectPlayers {| SelectedPlayers = s.Starters @ s.Bench |}
            { model with Step = newStep }, Cmd.none
        | SelectPlayers _ ->
            // Already in selection step, no change
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
        | SelectPlayers detail ->
            SelectPlayersPage.Component
              {| allPlayers = p.TeamPlayers
                 selectedPlayers = detail.SelectedPlayers
                 onContinue = fun selected -> selected |> AdvanceToLineup |> setupDispatch
                 togglePlayer = fun gp -> gp.Id |> ToggleSelected |> setupDispatch |}

        | ChooseLineup detail ->
            OrganizeLineupPage.Component
              {| startingPlayers = detail.Starters
                 benchedPlayers = detail.Bench
                 toggleStarter = fun pid -> pid |> ToggleStarter |> setupDispatch
                 CurrentFieldPlayerTarget = p.FieldPlayerTarget
                 SetFieldPlayerTarget = p.SetFieldPlayerTarget
                 onBack = fun () -> BackToSelection |> setupDispatch
                 onStartGame = fun () ->
                  let starterIds = detail.Starters |> List.map (fun p -> p.Id)
                  let benchIds = detail.Bench |> List.map (fun p -> p.Id)
                  p.StartNewGame (starterIds, benchIds) |}

      ]     
  ]
