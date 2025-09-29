namespace BenchBossApp.Components.BenchBoss

open Feliz
open BenchBossApp.Components.BenchBoss.Types

module LandingPage =

  let private takeUpTo count items =
    let rec loop remaining acc source =
      match remaining, source with
      | n, _ when n <= 0 -> List.rev acc
      | _, [] -> List.rev acc
      | n, x :: xs -> loop (n - 1) (x :: acc) xs
    loop count [] items

  let private statTile label value description =
    Html.div [
      prop.className "rounded-xl border border-gray-200 bg-white p-4 shadow-sm dark:border-white/10 dark:bg-gray-900/40"
      prop.children [
        Html.span [
          prop.className "text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-300"
          prop.text label
        ]
        Html.p [
          prop.className "mt-2 text-2xl font-bold text-gray-900 dark:text-white"
          prop.text value
        ]
        Html.p [
          prop.className "mt-1 text-sm text-gray-500 dark:text-gray-300"
          prop.text description
        ]
      ]
    ]

  let private lineupPreview (players: GamePlayer list) =
    match players with
    | [] ->
        Html.p [
          prop.className "rounded-lg border border-dashed border-gray-300 px-4 py-3 text-sm text-gray-500 dark:border-white/15 dark:text-gray-300"
          prop.text "No players have been added to today's lineup yet."
        ]
    | _ ->
        let preview = takeUpTo 3 players
        let extra = players.Length - preview.Length

        Html.div [
          prop.className "space-y-2"
          prop.children [
            Html.ul [
              prop.className "space-y-1 text-sm text-gray-700 dark:text-gray-200"
              prop.children (
                preview
                |> List.map (fun player ->
                    Html.li [
                      prop.key (string player.Id)
                      prop.className "flex items-center justify-between rounded-md bg-gray-50 px-3 py-2 dark:bg-gray-900/60"
                      prop.children [
                        Html.span [
                          prop.className "font-medium"
                          prop.text player.Name
                        ]
                        Html.span [
                          prop.className "text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400"
                          prop.text (
                            match player.InGameStatus with
                            | OnField -> "On field"
                            | OnBench -> "On bench"
                          )
                        ]
                      ]
                    ])
              )
            ]
            if extra > 0 then
              Html.p [
                prop.className "text-xs font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400"
                prop.text $"and {extra} more player{if extra = 1 then "" else "s"}..."
              ]
            else
              Html.none
          ]
        ]

  let private teamPreview (players: TeamPlayer list) =
    match players with
    | [] ->
        Html.p [
          prop.className "rounded-lg border border-dashed border-gray-300 px-4 py-3 text-sm text-gray-500 dark:border-white/15 dark:text-gray-300"
          prop.text "Build your roster to get started with BenchBoss."
        ]
    | _ ->
        let preview =
          players
          |> List.sortBy (fun p -> p.Name)
          |> takeUpTo 6

        Html.ul [
          prop.className "grid gap-2 sm:grid-cols-2"
          prop.children (
            preview
            |> List.map (fun player ->
                Html.li [
                  prop.key (string player.Id)
                  prop.className "flex items-center gap-3 rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm font-medium text-gray-700 shadow-sm dark:border-white/15 dark:bg-gray-900/40 dark:text-gray-200"
                  prop.children [
                    Html.span [
                      prop.className "flex size-8 items-center justify-center rounded-full bg-indigo-600/10 text-sm font-semibold text-indigo-700 dark:bg-indigo-500/20 dark:text-indigo-200"
                      prop.text (
                        player.Name.Trim()
                        |> Seq.tryFind System.Char.IsLetter
                        |> Option.defaultValue '#'
                        |> string
                      )
                    ]
                    Html.span [ prop.text player.Name ]
                  ]
                ])
          )
        ]

  let render (state: State) (dispatch: Msg -> unit) =
    let sortedLineup = state.Game.Players |> List.sortBy (fun p -> p.Name)
    let totalTeamPlayers = state.TeamPlayers.Length
    let selectedCount = sortedLineup.Length
    let fieldSlots = state.Game.FieldSlots
    let missingPlayers = max 0 (fieldSlots - selectedCount)
    let canStart = selectedCount > 0 && missingPlayers = 0

    let lineupStatus =
      if totalTeamPlayers = 0 then
        "Add players to your squad to unlock game setup."
      elif selectedCount = 0 then
        "No one has been added to today's game yet."
      elif missingPlayers > 0 then
        $"Choose {missingPlayers} more player{if missingPlayers = 1 then "" else "s"} to fill every position."
      else
        "Lineup locked in! You're ready to kick off."

    Html.section [
      prop.className "w-full py-10"
      prop.children [
        Html.div [
          prop.className "mx-auto flex w-full max-w-6xl flex-col gap-10 px-4 sm:px-6 lg:px-8"
          prop.children [
            Html.div [
              prop.className "space-y-4 text-center sm:text-left"
              prop.children [
                Html.span [
                  prop.className "inline-flex items-center rounded-full border border-indigo-200 bg-indigo-50 px-3 py-1 text-xs font-semibold uppercase tracking-wide text-indigo-700 dark:border-indigo-400/40 dark:bg-indigo-500/10 dark:text-indigo-200"
                  prop.text "Game day control center"
                ]
                Html.h1 [
                  prop.className "text-3xl font-bold text-gray-900 dark:text-white sm:text-4xl"
                  prop.text "Everything you need to run today's match"
                ]
                Html.p [
                  prop.className "text-base text-gray-600 dark:text-gray-300 sm:text-lg"
                  prop.text "Choose your lineup, set how many players take the field, and jump straight into the live game when you're ready."
                ]
              ]
            ]

            Html.div [
              prop.className "grid gap-4 sm:grid-cols-3"
              prop.children [
                statTile "Team players" (string totalTeamPlayers) (if totalTeamPlayers = 0 then "No roster yet" else "Players available for selection")
                statTile "Game lineup" (string selectedCount) (if selectedCount = 0 then "No players assigned" else $"Tracking {state.Game.Name}")
                statTile "Field positions" (string fieldSlots) "Number of starters you'll send out"
              ]
            ]

            Html.div [
              prop.className "grid gap-6 lg:grid-cols-2"
              prop.children [
                Html.div [
                  prop.className "flex h-full flex-col justify-between rounded-2xl border border-indigo-100 bg-white p-6 shadow-sm dark:border-indigo-400/40 dark:bg-gray-950/40"
                  prop.children [
                    Html.div [
                      prop.className "space-y-5"
                      prop.children [
                        Html.div [
                          prop.className "space-y-1"
                          prop.children [
                            Html.h2 [
                              prop.className "text-2xl font-semibold text-gray-900 dark:text-white"
                              prop.text "Game setup"
                            ]
                            Html.p [
                              prop.className "text-sm text-gray-600 dark:text-gray-300"
                              prop.text "Review who's playing today and adjust your starting lineup before kickoff."
                            ]
                          ]
                        ]

                        lineupPreview sortedLineup

                        Html.p [
                          prop.className "text-sm font-medium text-indigo-600 dark:text-indigo-300"
                          prop.text lineupStatus
                        ]
                      ]
                    ]

                    Html.div [
                      prop.className "mt-6 flex flex-col gap-3 sm:flex-row"
                      prop.children [
                        Html.button [
                          prop.className "inline-flex w-full items-center justify-center rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500"
                          prop.onClick (fun _ -> dispatch (NavigateToPage StartGamePage))
                          prop.text "Open game setup"
                        ]
                        Html.button [
                          prop.className "inline-flex w-full items-center justify-center rounded-lg border border-indigo-200 px-4 py-2 text-sm font-semibold text-indigo-700 transition hover:border-indigo-400 hover:text-indigo-900 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500 disabled:cursor-not-allowed disabled:border-indigo-200 disabled:text-indigo-300 dark:border-white/15 dark:text-indigo-200 dark:hover:border-white/30 dark:hover:text-white dark:disabled:border-white/10 dark:disabled:text-white/30"
                          prop.disabled (not canStart)
                          prop.onClick (fun _ -> dispatch StartGame)
                          prop.text "Start game now"
                        ]
                      ]
                    ]
                  ]
                ]

                Html.div [
                  prop.className "flex h-full flex-col justify-between rounded-2xl border border-gray-200 bg-white p-6 shadow-sm dark:border-white/10 dark:bg-gray-950/40"
                  prop.children [
                    Html.div [
                      prop.className "space-y-5"
                      prop.children [
                        Html.div [
                          prop.className "space-y-1"
                          prop.children [
                            Html.h2 [
                              prop.className "text-2xl font-semibold text-gray-900 dark:text-white"
                              prop.text "Team management"
                            ]
                            Html.p [
                              prop.className "text-sm text-gray-600 dark:text-gray-300"
                              prop.text "Update player details and build out your squad ahead of the match."
                            ]
                          ]
                        ]

                        teamPreview state.TeamPlayers
                      ]
                    ]

                    Html.button [
                      prop.className "mt-6 inline-flex w-full items-center justify-center rounded-lg border border-gray-200 px-4 py-2 text-sm font-semibold text-gray-700 transition hover:border-indigo-300 hover:text-indigo-600 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500 dark:border-white/15 dark:text-gray-200 dark:hover:border-indigo-400/50 dark:hover:text-white"
                      prop.onClick (fun _ -> dispatch (NavigateToPage ManageTeamPage))
                      prop.text "Manage team"
                    ]
                  ]
                ]
              ]
            ]
          ]
        ]
      ]
    ]
