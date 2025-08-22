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
        Html.span [ prop.className "font-medium"; prop.text p.Name ]
        Html.span [ 
          prop.className "text-sm text-slate-500"
          prop.text $"P{nbsp}%s{Time.formatMMSS p.PlayedSeconds}"
        ]
        Html.span [ 
          prop.className "text-sm text-slate-500"
          prop.text $"B{nbsp}%s{Time.formatMMSS p.BenchedSeconds}"
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
      //printfn "Field slot %d drop event" slotIndex
      //printfn "DataTransfer items: %A" ev.dataTransfer.items
      ev.preventDefault()
      let playerIdStr = ev.dataTransfer.getData "text"
      match System.Guid.TryParse(playerIdStr) with
      | true, playerId -> dispatch (DropOnField(slotIndex, playerId))
      | false, _ -> ()

    let onDragOver (ev: DragEvent) =
      //printfn "Field slot %d drag over event" slotIndex
      //printfn "DataTransfer items: %A" ev.dataTransfer.items
      ev.preventDefault()
      ev.dataTransfer.dropEffect <- "move"

    let onDragEnter (ev: DragEvent) =
      //printfn "Field slot %d drag enter event" slotIndex
      ev.preventDefault()

    let onDragLeave (ev: DragEvent) =
      //printfn "Field slot %d drag leave event" slotIndex
      ev.preventDefault()

    Html.div [
      prop.className [
        "w-full h-16 border-2 border-gray-300 rounded-lg flex items-center justify-center bg-green-50 hover:bg-green-100 transition-colors"
        if playerOpt.IsSome then "cursor-move"
        else "cursor-pointer border-dashed"
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
  let private BenchArea (benchPlayers: PlayerId list) (allPlayers: GamePlayer list) (dispatch: Msg -> unit) =

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

    let playerById = allPlayers |> List.map (fun p -> p.Id, p) |> Map.ofList
    let benchPlayerObjs = benchPlayers |> List.choose (fun playerId -> playerById |> Map.tryFind playerId)

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
    let playerById = state.GamePlayers |> List.map (fun p -> p.Id, p) |> Map.ofList
    let fieldPlayers = state.FieldSlots |> Array.map (Option.bind (fun id -> playerById |> Map.tryFind id))
    
    Html.div [
      prop.className "flex-1 p-4 bg-gradient-to-b from-green-50 to-blue-50"
      prop.children [
        // Soccer Field
        Html.div [
          prop.className "max-w-4xl mx-auto"
          // prop.onDragEnter (fun ev ->
          //   Browser.Dom.console.log("Soccer Field drag enter event", ev)
          // )
          prop.children [

            // Field layout
            Html.div [
              prop.className "bg-green-200 border-4 border-green-800 rounded-lg p-8 relative"
              prop.children [
                
                fieldMarkings

                Html.div [
                  prop.className "flex flex-col sm:flex-row sm:flex-wrap gap-4 justify-center items-start min-h-64 z-10 relative"
                  prop.children ( fieldPlayers |> Array.mapi (fun i playerOpt ->
                      Html.div [
                        prop.className "w-full sm:w-auto sm:min-w-48 sm:max-w-xs sm:flex-[0_1_48%] relative z-10"
                        prop.children [
                          fieldSlot i playerOpt dispatch
                        ]
                      ]
                    )
                  )  
                ]
              ]
            ]

            // Bench section
            Html.div [
              prop.className "mt-8"
              prop.children [
                BenchArea state.Bench state.GamePlayers dispatch
              ]
            ]
          ]
        ]
      ]
    ]
