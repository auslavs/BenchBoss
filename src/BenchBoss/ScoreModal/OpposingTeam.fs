namespace BenchBossApp.Components.BenchBoss.ScoreModal

module OpposingTeam =
  open Feliz
  open Browser.Types

  [<ReactComponent>]
  let View (isOpen: bool, onClose: MouseEvent -> unit, onConfirmScore: unit -> unit) =

    if not isOpen then Html.none
    else
      Html.div [
        prop.className "relative z-10"
        prop.children [
          // Backdrop
          Html.div [
            prop.className "fixed inset-0 bg-gray-500/75 dark:bg-gray-900/50"
            prop.onClick onClose
          ]
          // Modal container
          Html.div [
            prop.className "fixed inset-0 z-10 w-screen overflow-y-auto"
            prop.children [
              Html.div [
                prop.className "flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0"
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
                                prop.text "Score for Opposing Team"
                              ]
                              Html.div [
                                prop.className "mt-2"
                                prop.children [
                                  Html.p [
                                    prop.className "text-sm text-gray-500 dark:text-gray-400"
                                    prop.text "Record a score for the opposing team."
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
                            prop.className "flex gap-1 flex-col"
                            prop.children [
                              Common.confirmButton false (fun _ -> onConfirmScore ())
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
