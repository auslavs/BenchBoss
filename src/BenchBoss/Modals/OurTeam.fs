namespace BenchBossApp.Components.BenchBoss.Modals

module OurTeam =
  open Feliz
  open Browser.Types
  open BenchBossApp.Components.BenchBoss.Types


  [<ReactComponent>]
  let View (isOpen: bool, players: GamePlayer list, onClose: MouseEvent -> unit, confirmOurGoal: PlayerId option -> unit) =

    let currentSelectedScorer, setCurrentSelectedScorer = React.useState<PlayerId option> None

    if not isOpen then Html.none
    else
      Html.div [
        prop.className "relative z-10"
        prop.children [
          // Backdrop
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
                    prop.children [
                      Html.div [
                        prop.children [
                          Html.div [
                            prop.className "mx-auto flex size-12 items-center justify-center rounded-full bg-purple-100 dark:bg-purple-500/10"
                            prop.children [ Common.goalIcon ]
                          ]
                          Html.div [
                            prop.className "mt-3 text-center sm:mt-5"
                            prop.children [
                              Html.h3 [
                                prop.className "text-base font-semibold text-gray-900 dark:text-white"
                                prop.text "Score for Our Team"
                              ]
                              Html.div [
                                prop.className "mt-2"
                                prop.children [
                                  Html.div [
                                    prop.className "space-y-3"
                                    prop.children [
                                      Html.p [
                                        prop.className "text-sm text-gray-500 dark:text-gray-400"
                                        prop.text "Select the player who scored:"
                                      ]
                                      Html.div [
                                        prop.className "space-y-2 min-h-36 max-h-96 overflow-y-auto"
                                        prop.children [
                                          for player in players do
                                            Html.button [
                                              prop.className [
                                                "w-full p-2 text-left rounded border"
                                                if Some player.Id = currentSelectedScorer
                                                then "bg-purple-50 border-purple-300 text-purple-900 dark:bg-purple-900/20 dark:border-purple-600 dark:text-purple-100"
                                                else "bg-gray-50 border-gray-200 text-gray-700 hover:bg-gray-100 dark:bg-gray-700 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-600"
                                              ]
                                              prop.text player.Name
                                              prop.onClick (fun _ -> player.Id |> Some |> setCurrentSelectedScorer)
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
                      Html.div [
                        prop.className "mt-5 sm:mt-6"
                        prop.children [
                          Html.div [
                            prop.className "flex gap-2 flex-col"
                            prop.children [
                              Common.confirmButton currentSelectedScorer.IsNone (fun _ -> confirmOurGoal currentSelectedScorer)
                              Common.cancelButton onClose
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
