namespace BenchBossApp.Components.BenchBoss

open Feliz
open Browser.Types
open BenchBossApp.Components.BenchBoss.Types
open BenchBossApp.Components.BenchBoss.DragDropUtils

module GamePage =
  // DragDropInit registers global drag/drop listeners once and cleans up on unmount.

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
    Html.div ([
      prop.className "cursor-move w-full h-16 select-none touch-none p-2 bg-white rounded-lg border border-gray-200 hover:border-purple-300 shadow-sm"
    ] @ (draggableProps (p.Id.ToString()) p.Name) @ [
      prop.children [ playerLabel p "" ]
    ])

  let private fieldSlot (slotIndex: int) (playerOpt: GamePlayer option) (dispatch: Msg -> unit) =
    Html.div ([
      prop.className [
        "w-full h-16 rounded-lg flex items-center justify-center bg-green-50 hover:bg-green-100 transition-colors touch-none"
        match playerOpt with
        | Some _ -> "cursor-move"
        | None -> "cursor-pointer border-2 border-gray-300 border-dashed"
      ]
    ] @ fieldSlotProps slotIndex @ [
      prop.children [
        match playerOpt with
        | Some player -> draggablePlayer player
        | None -> Html.span [ prop.className "text-gray-400 text-sm pointer-events-none"; prop.text "Drop player here" ]
      ]
    ])

  [<ReactComponent>]
  let private BenchArea (allPlayers: GamePlayer list) (dispatch: Msg -> unit) =

    let playerIsHovering, setPlayerIsHovering = React.useState false

    // Hover styling will be managed by pointer events implicitly when preview overlaps; simple state retained for future enhancement

    let benchPlayerObjs = 
      allPlayers 
      |> List.filter (fun p -> p.InGameStatus = OnBench)
      |> List.sortByDescending (fun p -> p.BenchedSeconds)

    React.fragment [
      Html.h3 [
        prop.className "text-lg font-semibold mb-3 text-yellow-800"
        prop.text "Bench"
      ]
      Html.div ([
        prop.className [
          "bg-yellow-50 border-2 border-yellow-300 rounded-lg p-4 min-h-32 touch-none"
          if playerIsHovering then "border-yellow-500 bg-yellow-100" else "border-dashed"
        ]
      ] @ benchProps() @ [
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
      ])
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

  [<ReactComponent>]
  let DragDropInit (state: State) (dispatch: Msg -> unit) =
    React.useEffect((fun () ->
      let dispose = DragDropUtils.registerGlobal
                      (fun slotIdx playerIdStr ->
                        match System.Guid.TryParse playerIdStr with
                        | true, gid -> dispatch (DropOnField(slotIdx, gid))
                        | _ -> () )
                      (fun playerIdStr ->
                        match System.Guid.TryParse playerIdStr with
                        | true, gid -> dispatch (DropOnBench gid)
                        | _ -> () )
      Some { new System.IDisposable with member _.Dispose() = dispose() }
    ), [||])
    Html.none

  // Convert previous render function into a React component so we can safely place components/hook usage.
  [<ReactComponent>]
  let RenderGamePage (state: State) (dispatch: Msg -> unit) =
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
        // inject global drag-drop initializer (no visible output)
        DragDropInit state dispatch
        // Soccer Field
        Html.div [
          prop.className "max-w-4xl mx-auto"
          prop.children [

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
                    for rowIdx, row in rows |> List.indexed do
                      Html.div [
                        prop.key $"row-{rowIdx}"
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
                              prop.key $"slot-{slotGlobalIndex}"
                              prop.className "w-full sm:w-44 max-w-full"
                              prop.children [
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
            Html.div [ prop.className "mt-8"; prop.children [ BenchArea state.Game.Players dispatch ] ]
          ]
        ]
      ]
    ]

  // Provide original render function name expected elsewhere
  let render (state: State) (dispatch: Msg -> unit) = RenderGamePage state dispatch
