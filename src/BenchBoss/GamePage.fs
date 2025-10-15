namespace BenchBossApp.Components.BenchBoss

open Feliz
open Browser.Types
open BenchBossApp.Components.BenchBoss.Types

module GamePage =

  let private playerLabel (p:GamePlayer) (extra:string) =
    let nbsp = "\u00A0"
    Html.div [
      prop.className ("flex items-center justify-between gap-2 " + extra)
      prop.children [
        Html.span [ prop.className "font-medium text-xl"; prop.text p.Name ]
        Html.div [
          prop.className "flex flex-col items-end text-base text-slate-500"
          prop.children [
            Html.span [ 
              prop.text $"P{nbsp}%s{Time.formatMMSS p.PlayedSeconds}"
            ]
            Html.span [ 
              prop.text $"B{nbsp}%s{Time.formatMMSS p.BenchedSeconds}"
            ]
          ]
        ]
      ]
    ]

  let private draggablePlayer (p:GamePlayer) =
    Html.div [
      prop.className "cursor-move w-full h-16 select-none p-2 bg-white rounded-lg border border-gray-200 hover:border-purple-300 shadow-sm"
      prop.draggable true
      prop.onDragStart (fun ev ->
        let playerId = p.Id.ToString()
        ev.dataTransfer.setData("text", playerId) |> ignore
        ev.dataTransfer.effectAllowed <- "move"
      )
      prop.children [
        playerLabel p ""
      ]
    ]

  let private fieldSlot (slotIndex: int) (playerOpt: GamePlayer option) (dispatch: Msg -> unit) =
    let onDrop (ev: DragEvent) =
      ev.preventDefault()
      let playerIdStr = ev.dataTransfer.getData "text"
      match System.Guid.TryParse(playerIdStr) with
      | true, playerId -> dispatch (DropOnField(slotIndex, playerId))
      | false, _ -> ()

    let onDragOver (ev: DragEvent) =
      ev.preventDefault()
      ev.dataTransfer.dropEffect <- "move"

    let onDragEnter (ev: DragEvent) =
      ev.preventDefault()

    let onDragLeave (ev: DragEvent) =
      ev.preventDefault()

    Html.div [
      prop.className [
        "w-full h-16 rounded-lg flex items-center justify-center bg-green-50 hover:bg-green-100 transition-colors"
        match playerOpt with
        | Some _ -> "cursor-move"
        | None -> "cursor-pointer border-2 border-gray-300 border-dashed"
      ]
      prop.onDrop onDrop
      prop.onDragOver onDragOver
      prop.onDragEnter onDragEnter
      prop.onDragLeave onDragLeave
      prop.children [
        match playerOpt with
        | Some player ->
            draggablePlayer player
        | None ->
            Html.span [
              prop.className "text-gray-400 text-sm pointer-events-none"
              prop.text "Drop player here"
            ]
      ]
    ]

  [<ReactComponent>]
  let private BenchArea (allPlayers: GamePlayer list) (dispatch: Msg -> unit) =

    let playerIsHovering, setPlayerIsHovering = React.useState false

    let onDrop (ev: DragEvent) =
      Browser.Dom.console.log("Bench drop event", ev)
      ev.preventDefault()
      let playerIdStr = ev.dataTransfer.getData "text"
      match System.Guid.TryParse playerIdStr with
      | true, playerId -> dispatch (DropOnBench playerId)
      | false, _ -> ()

    let onDragEnter (ev: DragEvent) =
      Browser.Dom.console.log("Bench drag enter event", ev)
      setPlayerIsHovering true
      ev.preventDefault()

    let onDragLeave (ev: DragEvent) =
      Browser.Dom.console.log("Bench drag leave event", ev)
      setPlayerIsHovering false
      ev.preventDefault()

    let onDragOver (ev: DragEvent) =
      Browser.Dom.console.log("Bench drag over event", ev)
      ev.preventDefault()
      ev.dataTransfer.dropEffect <- "move"

    let benchPlayerObjs = 
      allPlayers 
      |> List.filter (fun p -> p.InGameStatus = OnBench)
      |> List.sortByDescending (fun p -> p.BenchedSeconds)

    React.fragment [
      Html.h3 [
        prop.className "text-lg font-semibold mb-3 text-yellow-800"
        prop.text "Bench"
      ]
      Html.div [
        prop.className [
          "bg-yellow-50 border-2 border-yellow-300 rounded-lg p-4 min-h-32"
          if playerIsHovering then "border-yellow-500 bg-yellow-100" else "border-dashed"
        ]
        prop.onDrop onDrop
        prop.onDragEnter onDragEnter
        prop.onDragLeave onDragLeave
        prop.onDragOver onDragOver
        prop.children [
          // Remove the heading from here
          Html.div [
            prop.className "space-y-2"
            prop.children [
              for player in benchPlayerObjs do
                draggablePlayer player
            ]
          ]
          if benchPlayerObjs.IsEmpty then
            Html.div [
              prop.className "text-gray-400 text-center py-4"
              prop.text "Drag players here to put them on the bench"
            ]
        ]
      ]
    ]

  let private fieldMarkings =
    React.fragment [
      Html.div [
        prop.className "absolute inset-4 border-2 border-white opacity-50 rounded z-2"
      ]
      Html.div [
        prop.className "absolute top-1/2 left-4 right-4 h-0.5 bg-white opacity-50 z-2"
      ]
      Html.div [
        prop.className "absolute top-1/2 left-1/2 w-20 h-20 border-2 border-white opacity-50 rounded-full transform -translate-x-1/2 -translate-y-1/2 z-2"
      ]
    ]

  let render (state: State) (dispatch: Msg -> unit) =
    let onFieldPlayers = 
      state.Game.Players 
      |> List.filter (fun p -> p.InGameStatus = OnField)
      |> List.sortBy (fun p -> p.PlayedSeconds)
    // Dynamic field slots based on configured target (defensive clamp)
    let targetSlots = if state.FieldPlayerTarget < 4 then 4 elif state.FieldPlayerTarget > 11 then 11 else state.FieldPlayerTarget
    let fieldPlayers = 
      let playerArray = Array.create targetSlots None
      onFieldPlayers |> List.iteri (fun i p -> if i < targetSlots then playerArray[i] <- Some p)
      playerArray

    Html.div [
      prop.className "flex-1 p-4 bg-gradient-to-b from-green-50 to-blue-50"
      prop.children [
        // Soccer Field
        Html.div [
          prop.className "max-w-4xl mx-auto"
          prop.children [

            // Control bar
            Html.div [
              prop.className "flex items-center justify-end mb-4 gap-3 text-sm"
              prop.children [
                Html.label [
                  prop.className "font-medium"
                  prop.text "Players on Field"
                ]
                Html.select [
                  prop.className "border rounded px-2 py-1 bg-white"
                  prop.value state.FieldPlayerTarget
                  // Feliz onChange for select often yields obj/string; parse manually
                  prop.onChange (fun (value: string) ->
                    match System.Int32.TryParse value with
                    | true, v -> dispatch (SetFieldPlayerTarget v)
                    | _ -> () )
                  prop.children [ for n in 4..11 -> Html.option [ prop.value n; prop.text (string n) ] ]
                ]
              ]
            ]

            // Field layout
            Html.div [
              prop.className "bg-green-200 border-4 border-green-800 rounded-lg p-8 relative"
              prop.children [
                fieldMarkings
                let rows : GamePlayer option array list =
                  if targetSlots <= 4 then
                    [ fieldPlayers ]
                  elif targetSlots = 5 then
                    [ fieldPlayers.[0..1]; fieldPlayers.[2..4] ]
                  elif targetSlots = 6 then
                    [ fieldPlayers.[0..2]; fieldPlayers.[3..5] ]
                  elif targetSlots = 7 then
                    [ fieldPlayers.[0..2]; fieldPlayers.[3..6] ]
                  elif targetSlots = 8 then
                    [ fieldPlayers.[0..2]; fieldPlayers.[3..5]; fieldPlayers.[6..7] ]
                  elif targetSlots = 9 then
                    [ fieldPlayers.[0..2]; fieldPlayers.[3..5]; fieldPlayers.[6..8] ]
                  elif targetSlots = 10 then
                    [ fieldPlayers.[0..2]; fieldPlayers.[3..5]; fieldPlayers.[6..7]; fieldPlayers.[8..9] ]
                  else
                    [ fieldPlayers.[0..2]; fieldPlayers.[3..5]; fieldPlayers.[6..8]; fieldPlayers.[9..10] ]
                Html.div [
                  prop.className "flex flex-col gap-6 justify-center items-stretch min-h-64 z-10 relative"
                  prop.children [
                    for (rowIdx, row) in rows |> List.indexed do
                      Html.div [
                        prop.key ($"row-{rowIdx}")
                        prop.className "flex flex-col items-stretch gap-4 sm:flex-row sm:justify-center sm:gap-6"
                        prop.children [
                          for i in 0 .. row.Length - 1 do
                            let slotGlobalIndex =
                              let priorCount =
                                rows
                                |> List.take rowIdx
                                |> List.sumBy (fun (r: GamePlayer option array) -> r.Length)
                              priorCount + i
                            Html.div [
                              prop.key ($"slot-{slotGlobalIndex}")
                              prop.className "w-full sm:w-44 max-w-full"
                              prop.children [
                                Html.div [
                                  prop.className "text-center text-[10px] uppercase tracking-wide text-green-900 mb-1 opacity-60"
                                  prop.text ($"Pos {slotGlobalIndex + 1}")
                                ]
                                fieldSlot slotGlobalIndex row[i] dispatch
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
              prop.className "mt-8"
              prop.children [
                BenchArea state.Game.Players dispatch
              ]
            ]
          ]
        ]
      ]
    ]
