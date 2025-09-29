namespace BenchBossApp.Components.BenchBoss

open System
open Feliz
open BenchBossApp.Components.BenchBoss.Types

module GameSetupView =

  let private maxSlots = 11

  let private playerToggle (selected: Set<PlayerId>) (dispatch: Msg -> unit) (player: TeamPlayer) =
    let isSelected = selected.Contains player.Id
    let baseClasses = "flex items-center justify-between gap-3 rounded-xl border px-4 py-3 transition"
    let classes =
      if isSelected then
        $"{baseClasses} border-indigo-500 bg-indigo-50/80 text-indigo-700 shadow-sm dark:border-indigo-400/60 dark:bg-indigo-500/10 dark:text-indigo-100"
      else
        $"{baseClasses} border-gray-200 hover:border-indigo-300 hover:bg-indigo-50/40 dark:border-white/15 dark:hover:border-indigo-400/50 dark:hover:bg-indigo-500/5"

    Html.label [
      prop.key (string player.Id)
      prop.className classes
      prop.children [
        Html.div [
          prop.className "flex items-center gap-3"
          prop.children [
            Html.input [
              prop.type' "checkbox"
              prop.isChecked isSelected
              prop.className "h-5 w-5 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
              prop.onChange (fun (_: bool) -> dispatch (TogglePlayerGameAvailability player.Id))
            ]
            Html.span [
              prop.className "text-base font-medium"
              prop.text player.Name
            ]
          ]
        ]
        Html.span [
          prop.className "text-xs font-semibold uppercase tracking-wide text-indigo-500"
          prop.text (if isSelected then "Selected" else "Available")
        ]
      ]
    ]

  let private fieldSlotControls (state: State) (dispatch: Msg -> unit) =
    let slots = state.Game.FieldSlots

    let adjust delta =
      dispatch (SetFieldSlots (slots + delta))

    let handleChange (value: string) =
      match Int32.TryParse value with
      | true, parsed -> dispatch (SetFieldSlots parsed)
      | _ -> ()

    Html.div [
      prop.className "space-y-4"
      prop.children [
        Html.div [
          prop.className "flex items-center justify-between"
          prop.children [
            Html.span [
              prop.className "text-sm font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300"
              prop.text "Field positions"
            ]
          ]
        ]
        Html.div [
          prop.className "flex items-center gap-3"
          prop.children [
            Html.button [
              prop.className "inline-flex h-10 w-10 items-center justify-center rounded-lg border border-gray-300 text-lg font-semibold text-gray-600 transition hover:border-indigo-300 hover:text-indigo-600 disabled:cursor-not-allowed disabled:opacity-40 dark:border-white/15 dark:text-gray-200 dark:hover:border-indigo-400/40 dark:hover:text-white"
              prop.disabled (slots <= 1)
              prop.onClick (fun _ -> adjust -1)
              prop.text "-"
            ]
            Html.span [
              prop.className "text-3xl font-semibold text-gray-900 dark:text-white"
              prop.text (string slots)
            ]
            Html.button [
              prop.className "inline-flex h-10 w-10 items-center justify-center rounded-lg border border-gray-300 text-lg font-semibold text-gray-600 transition hover:border-indigo-300 hover:text-indigo-600 disabled:cursor-not-allowed disabled:opacity-40 dark:border-white/15 dark:text-gray-200 dark:hover:border-indigo-400/40 dark:hover:text-white"
              prop.disabled (slots >= maxSlots)
              prop.onClick (fun _ -> adjust 1)
              prop.text "+"
            ]
          ]
        ]
      ]
    ]

  let render (state: State) (dispatch: Msg -> unit) =
    let selectedPlayers =
      state.Game.Players
      |> List.map _.Id
      |> Set.ofList

    let selectedCount = Set.count selectedPlayers
    let totalPlayers = state.TeamPlayers.Length
    let canStart = selectedCount > 0

    Html.section [
      prop.className "w-full py-10"
      prop.children [
        Html.div [
          prop.className "mx-auto flex w-full max-w-6xl flex-col gap-10 px-4 sm:px-6 lg:px-8"
          prop.children [
            Html.div [
              prop.className "space-y-3 text-center sm:text-left"
              prop.children [
                Html.h1 [
                  prop.className "text-3xl font-bold text-gray-900 dark:text-white sm:text-4xl"
                  prop.text "Set up today's match"
                ]
                Html.p [
                  prop.className "text-base text-gray-600 dark:text-gray-300 sm:text-lg"
                  prop.text "Name the game, pick who is available, and decide how many players will start on the field."
                ]
              ]
            ]

            Html.div [
              prop.className "grid gap-6 lg:grid-cols-2"
              prop.children [
                Html.div [
                  prop.className "rounded-2xl border border-gray-200 bg-white p-6 shadow-sm dark:border-white/10 dark:bg-gray-950/40"
                  prop.children [
                    Html.div [
                      prop.className "space-y-6"
                      prop.children [
                        Html.div [
                          prop.className "space-y-2"
                          prop.children [
                            Html.label [
                              prop.className "text-sm font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300"
                              prop.text "Opponent"
                            ]
                            Html.input [
                              prop.type' "text"
                              prop.className "w-full rounded-lg border border-gray-300 px-4 py-2 text-base shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:border-white/15 dark:bg-gray-900 dark:text-white"
                              prop.placeholder "Opposing team name"
                              prop.value state.Game.Name
                              prop.onChange (fun value -> dispatch (UpdateGameName value))
                            ]
                          ]
                        ]

                        fieldSlotControls state dispatch

                        Html.div [
                          if selectedCount = 0 then
                            Html.p [
                              prop.className "rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-700 dark:border-amber-400/40 dark:bg-amber-500/10 dark:text-amber-100"
                              prop.text "Select players from your roster to build the starting lineup."
                            ]
                          else
                            Html.p [
                              prop.className "rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700 dark:border-green-400/40 dark:bg-green-500/10 dark:text-green-100"
                              prop.text "Lineup locked in! You're ready to start the match."
                            ]
                        ]
                      ]
                    ]

                    Html.button [
                      prop.className "mt-6 w-full rounded-lg bg-indigo-600 px-6 py-3 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500 disabled:cursor-not-allowed disabled:bg-indigo-300 dark:disabled:bg-indigo-700/40"
                      prop.disabled (not canStart)
                      prop.onClick (fun _ -> dispatch StartGame)
                      prop.text "Start game"
                    ]
                  ]
                ]

                Html.div [
                  prop.className "rounded-2xl border border-gray-200 bg-white p-6 shadow-sm dark:border-white/10 dark:bg-gray-950/40"
                  prop.children [
                    Html.div [
                      prop.className "flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between"
                      prop.children [
                        Html.h2 [
                          prop.className "text-xl font-semibold text-gray-900 dark:text-white"
                          prop.text "Choose your players"
                        ]
                        Html.span [
                          prop.className "text-sm text-gray-500 dark:text-gray-300"
                          prop.text (
                            if totalPlayers = 0 then "No players yet"
                            else $"{selectedCount} of {totalPlayers} selected"
                          )
                        ]
                      ]
                    ]

                    if state.TeamPlayers.IsEmpty then
                      Html.div [
                        prop.className "mt-4 rounded-xl border border-dashed border-gray-300 px-4 py-6 text-center text-sm text-gray-500 dark:border-white/15 dark:text-gray-300"
                        prop.text "Add your first player above to start building the roster."
                      ]
                    else
                      Html.div [
                        prop.className "mt-4 max-h-[24rem] space-y-2 overflow-y-auto pr-1"
                        prop.children (
                          state.TeamPlayers
                          |> List.sortBy (fun p -> p.Name)
                          |> List.map (playerToggle selectedPlayers dispatch)
                        )
                      ]

                    Html.button [
                      prop.className "mt-6 inline-flex w-full items-center justify-center rounded-lg border border-gray-200 px-4 py-2 text-sm font-semibold text-gray-700 transition hover:border-indigo-300 hover:text-indigo-600 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500 dark:border-white/15 dark:text-gray-200 dark:hover:border-indigo-400/50 dark:hover:text-white"
                      prop.onClick (fun _ -> dispatch (NavigateToPage ManageTeamPage))
                      prop.text "Open full team manager"
                    ]
                  ]
                ]
              ]
            ]
          ]
        ]
      ]
    ]
