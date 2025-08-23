namespace BenchBossApp.Components.BenchBoss.Modals

module TimeManager =
  open Feliz
  open Browser.Types
  open BenchBossApp.Components.BenchBoss.Types


  [<ReactComponent>]
  let View (isOpen: bool, onClose: MouseEvent -> unit, onStartTimer: unit -> unit, onPauseTimer: unit -> unit, onStopTimer: unit -> unit, onEndHalf: unit -> unit, onStartNewHalf: unit -> unit, gameTimer: Timer) =

    if not isOpen then Html.none
    else
      Html.div [
        prop.className "relative z-10"
        prop.children [
          Html.div [
            prop.className "fixed inset-0 bg-gray-500/75 dark:bg-gray-900/50"
          ]
          // Modal container
          Html.div [
            prop.className "fixed inset-0 z-10 w-screen overflow-y-auto"
            prop.children [
              Html.div [
                prop.className "flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0"
                prop.onClick (fun e -> if e.target = e.currentTarget then onClose e)
                prop.children [
                  // Panel
                  Html.div [
                    prop.className "relative transform overflow-hidden rounded-lg bg-white px-4 pt-5 pb-4 text-left shadow-xl transition-all md:max-w-md w-full sm:p-6 dark:bg-gray-800 dark:outline dark:-outline-offset-1 dark:outline-white/10"
                    prop.onClick (fun e -> e.stopPropagation()) // Prevent clicks from bubbling to backdrop
                    prop.children [
                      Html.div [
                        prop.children [
                          Html.div [
                            prop.className "mx-auto flex size-12 items-center justify-center rounded-full bg-purple-100 dark:bg-purple-500/10"
                            prop.children [ 
                              Svg.svg [
                                svg.className "size-6 text-purple-600 dark:text-purple-400"
                                svg.viewBox (0, 0, 24, 24)
                                svg.fill "currentColor"
                                svg.children [
                                  Svg.path [
                                    svg.d "M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4M12.5,7V12.25L17,14.92L16.25,16.15L11,13V7H12.5Z"
                                  ]
                                ]
                              ]
                            ]
                          ]
                          Html.div [
                            prop.className "mt-3 text-center sm:mt-5"
                            prop.children [
                              Html.h3 [
                                prop.className "text-base font-semibold text-gray-900 dark:text-white"
                                prop.text "Game Timer Control"
                              ]
                              Html.div [
                                prop.className "mt-2"
                                prop.children [
                                  Html.p [
                                    prop.className "text-sm text-gray-500 dark:text-gray-400"
                                    prop.text "Control the game timer with the buttons below:"
                                  ]
                                ]
                              ]
                            ]
                          ]
                        ]
                      ]
                      Html.div [
                        prop.className "mt-5 sm:mt-6"
                        prop.children [
                          Html.div [
                            prop.className "flex gap-2 flex-col"
                            prop.children [
                              // Show Start Timer button when game is stopped or paused
                              match gameTimer with
                              | Stopped | Paused _ | Break _ ->
                                Html.button [
                                  prop.type' "button"
                                  prop.className [
                                    "inline-flex w-full justify-center items-center gap-2 rounded-md bg-purple-600 px-3 py-2 text-sm font-semibold text-white shadow-xs"
                                    "hover:bg-purple-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600"
                                    "dark:bg-purple-500 dark:shadow-none dark:hover:bg-purple-400 dark:focus-visible:outline-purple-500"
                                  ]
                                  prop.onClick (fun _ -> onStartTimer())
                                  prop.children [
                                    Svg.svg [
                                      svg.className "size-5"
                                      svg.viewBox (0, 0, 24, 24)
                                      svg.fill "currentColor"
                                      svg.children [
                                        Svg.path [
                                          svg.d "M8 5v14l11-7z"
                                        ]
                                      ]
                                    ]
                                    match gameTimer with
                                    | Stopped -> Html.text "Start Timer"
                                    | Paused _ -> Html.text "Resume Timer" 
                                    | Break _ -> Html.text "Start Second Half"
                                    | _ -> Html.text "Start Timer"
                                  ]
                                ]
                              | Running _ -> Html.none
                              
                              // Show Pause Timer button when game is running
                              match gameTimer with
                              | Running _ ->
                                Html.button [
                                  prop.type' "button"
                                  prop.className [
                                    "inline-flex w-full justify-center items-center gap-2 rounded-md bg-purple-600 px-3 py-2 text-sm font-semibold text-white shadow-xs"
                                    "hover:bg-purple-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600"
                                    "dark:bg-purple-500 dark:shadow-none dark:hover:bg-purple-400 dark:focus-visible:outline-purple-500"
                                  ]
                                  prop.onClick (fun _ -> onPauseTimer())
                                  prop.children [
                                    Svg.svg [
                                      svg.className "size-5"
                                      svg.viewBox (0, 0, 24, 24)
                                      svg.fill "currentColor"
                                      svg.children [
                                        Svg.path [
                                          svg.d "M6 19h4V5H6v14zm8-14v14h4V5h-4z"
                                        ]
                                      ]
                                    ]
                                    Html.text "Pause Timer"
                                  ]
                                ]
                              | _ -> Html.none

                              // Show End Half button when game is running
                              match gameTimer with  
                              | Running r ->
                                Html.button [
                                  prop.type' "button"
                                  prop.className [
                                    "inline-flex w-full justify-center items-center gap-2 rounded-md bg-purple-600 px-3 py-2 text-sm font-semibold text-white shadow-xs"
                                    "hover:bg-purple-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600"
                                    "dark:bg-purple-500 dark:shadow-none dark:hover:bg-purple-400 dark:focus-visible:outline-purple-500"
                                  ]
                                  prop.onClick (fun _ -> onEndHalf())
                                  prop.children [
                                    Svg.svg [
                                      svg.className "size-5"
                                      svg.viewBox (0, 0, 24, 24)
                                      svg.fill "currentColor"
                                      svg.children [
                                        Svg.path [
                                          svg.d "M13 3h-2v10h2V3zm4.83 2.17l-1.42 1.42C17.99 7.86 19 9.81 19 12c0 3.87-3.13 7-7 7s-7-3.13-7-7c0-2.19 1.01-4.14 2.58-5.42L6.17 5.17C4.23 6.82 3 9.26 3 12c0 4.97 4.03 9 9 9s9-4.03 9-9c0-2.74-1.23-5.18-3.17-6.83z"
                                        ]
                                      ]
                                    ]
                                    match r.Half with
                                    | First -> Html.text "End First Half"
                                    | Second -> Html.text "End Game"
                                  ]
                                ]
                              | _ -> Html.none

                              // Show Stop Timer button when game is running or paused (but not during break)
                              match gameTimer with
                              | Running _ | Paused _ ->
                                Html.button [
                                  prop.type' "button"
                                  prop.className [
                                    "inline-flex w-full justify-center items-center gap-2 rounded-md bg-purple-600 px-3 py-2 text-sm font-semibold text-white shadow-xs"
                                    "hover:bg-purple-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600"
                                    "dark:bg-purple-500 dark:shadow-none dark:hover:bg-purple-400 dark:focus-visible:outline-purple-500"
                                  ]
                                  prop.onClick (fun _ -> onStopTimer())
                                  prop.children [
                                    Svg.svg [
                                      svg.className "size-5"
                                      svg.viewBox (0, 0, 24, 24)
                                      svg.fill "currentColor"
                                      svg.children [
                                        Svg.path [
                                          svg.d "M6 6h12v12H6z"
                                        ]
                                      ]
                                    ]
                                    Html.text "Stop Timer"
                                  ]
                                ]
                              | _ -> Html.none

                              // Show Start New Half button when game is stopped (but not at the very beginning)
                              match gameTimer with
                              | Stopped ->
                                Html.button [
                                  prop.type' "button"
                                  prop.className [
                                    "inline-flex w-full justify-center items-center gap-2 rounded-md bg-purple-600 px-3 py-2 text-sm font-semibold text-white shadow-xs"
                                    "hover:bg-purple-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600"
                                    "dark:bg-purple-500 dark:shadow-none dark:hover:bg-purple-400 dark:focus-visible:outline-purple-500"
                                  ]
                                  prop.onClick (fun _ -> onStartNewHalf())
                                  prop.children [
                                    Svg.svg [
                                      svg.className "size-5"
                                      svg.viewBox (0, 0, 24, 24)
                                      svg.fill "currentColor"
                                      svg.children [
                                        Svg.path [
                                          svg.d "M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"
                                        ]
                                      ]
                                    ]
                                    Html.text "Reset for New Half"
                                  ]
                                ]
                              | _ -> Html.none

                              // Cancel Button (always visible)
                              Html.button [
                                prop.type' "button"
                                prop.className [
                                  "inline-flex w-full justify-center rounded-md px-3 py-2 text-sm font-semibold text-purple-700 shadow-xs"
                                  "border border-purple-200 hover:border-purple-300"
                                  "hover:bg-purple-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-300"
                                  "dark:text-purple-300 dark:border-purple-600 dark:hover:bg-purple-600 dark:focus-visible:outline-purple-500"
                                ]
                                prop.text "Cancel"
                                prop.onClick onClose
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
          ]
        ]
      ]
