namespace BenchBossApp.Components.BenchBoss.Modals

module TimeManager =
  open Feliz
  open Browser.Types
  open BenchBossApp.Components.BenchBoss.Types


  [<ReactComponent>]
  let View (isOpen: bool, onClose: MouseEvent -> unit, onStartGame: unit -> unit, onStopGame: unit -> unit) =

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
            prop.onClick onClose
            prop.children [
              Html.div [
                prop.className "flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0"
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
                                    svg.d "M12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22C6.47,22 2,17.5 2,12A10,10 0 0,1 12,2Z"
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
                              // Start Game Button
                              Html.button [
                                prop.type' "button"
                                prop.className [
                                  "inline-flex w-full justify-center items-center gap-2 rounded-md bg-purple-600 px-3 py-2 text-sm font-semibold text-white shadow-xs"
                                  "hover:bg-purple-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600"
                                  "dark:bg-purple-500 dark:shadow-none dark:hover:bg-purple-400 dark:focus-visible:outline-purple-500"
                                ]
                                prop.onClick (fun _ -> onStartGame())
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
                                  Html.text "Start Game"
                                ]
                              ]
                              // Stop Game Button
                              Html.button [
                                prop.type' "button"
                                prop.className [
                                  "inline-flex w-full justify-center items-center gap-2 rounded-md bg-purple-600 px-3 py-2 text-sm font-semibold text-white shadow-xs"
                                  "hover:bg-purple-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600"
                                  "dark:bg-purple-500 dark:shadow-none dark:hover:bg-purple-400 dark:focus-visible:outline-purple-500"
                                ]
                                prop.onClick (fun _ -> onStopGame())
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
                                  Html.text "Stop Game"
                                ]
                              ]
                              // Cancel Button
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
