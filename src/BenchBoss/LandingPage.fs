namespace BenchBossApp.Components.BenchBoss

open Feliz
open BenchBossApp.Components.BenchBoss.Types

module LandingPage =

  // Utility to take first N items of a list (non-alloc tail recursion)
  let private takeUpTo count items =
    let rec loop remaining acc source =
      match remaining, source with
      | n, _ when n <= 0 -> List.rev acc
      | _, [] -> List.rev acc
      | n, x :: xs -> loop (n - 1) (x :: acc) xs
    loop count [] items

  // Compact chip list of selected lineup players
  let private playerChips (players: GamePlayer list) =
    match players with
    | [] ->
        Html.p [
          prop.className "text-sm text-gray-500 dark:text-gray-300"
          prop.text "No players selected yet."
        ]
    | _ ->
        Html.div [
          prop.className "flex flex-wrap gap-2"
          prop.children (
            let shown = players |> List.sortBy (fun p -> p.Name) |> takeUpTo 6
            let extra = players.Length - shown.Length
            [ yield!
                shown
                |> List.map (fun p ->
                  Html.span [
                    prop.key (string p.Id)
                    prop.className "rounded-full bg-indigo-600/10 px-3 py-1 text-xs font-medium text-indigo-700 dark:bg-indigo-500/20 dark:text-indigo-200"
                    prop.text p.Name
                  ])
              if extra > 0 then
                Html.span [
                  prop.className "rounded-full bg-gray-200 px-3 py-1 text-xs font-medium text-gray-600 dark:bg-gray-700 dark:text-gray-200"
                  prop.text $"+{extra}"
                ] ]
          )
        ]

  let render (state: State) (dispatch: Msg -> unit) =
    let lineup = state.Game.Players
    let selected = lineup.Length
    let totalTeam = state.TeamPlayers.Length
    let fieldSlots = state.Game.FieldSlots
    let fullLineup = selected = fieldSlots && selected > 0
    let needsPlayers = fieldSlots - selected |> max 0
    let noTeam = totalTeam = 0
    let canStart = fullLineup

    let statusText =
      if totalTeam = 0 then "Add team players to get started."
      elif selected = 0 then "Select players for today's game."
      elif not fullLineup then $"Select {needsPlayers} more."
      else "Ready to start the match."

    Html.section [
      prop.className "w-full py-8"
      prop.children [
        Html.div [
          prop.className "mx-auto w-full max-w-md px-4 flex flex-col gap-6"
          prop.children [
            // Header
            Html.div [
              prop.className "space-y-2"
              prop.children [
                Html.h1 [
                  prop.className "text-2xl font-bold text-gray-900 dark:text-white"
                  prop.text "BenchBoss"
                ]
                Html.p [
                  prop.className "text-sm text-gray-600 dark:text-gray-300"
                  prop.text "Quick match control."
                ]
              ]
            ]

            // Status card
            Html.div [
              prop.className "rounded-xl border border-gray-200 bg-white p-4 shadow-sm dark:border-white/10 dark:bg-gray-900/40 space-y-3"
              prop.children [
                Html.p [
                  prop.className "text-sm font-medium text-indigo-600 dark:text-indigo-300"
                  prop.text statusText
                ]
                Html.div [
                  prop.className "flex flex-wrap gap-2 text-xs"
                  prop.children [
                    Html.span [
                      prop.className "rounded-md bg-gray-100 px-2 py-1 font-medium text-gray-700 dark:bg-gray-800 dark:text-gray-200"
                      prop.text $"Team {totalTeam}"
                    ]
                    Html.span [
                      prop.className "rounded-md bg-gray-100 px-2 py-1 font-medium text-gray-700 dark:bg-gray-800 dark:text-gray-200"
                      prop.text $"Lineup {selected}/{fieldSlots}"
                    ]
                    Html.span [
                      prop.className "rounded-md bg-gray-100 px-2 py-1 font-medium text-gray-700 dark:bg-gray-800 dark:text-gray-200"
                      prop.text (if fullLineup then "Full lineup" else "Incomplete")
                    ]
                  ]
                ]
                playerChips lineup
              ]
            ]

            // Primary actions
            Html.div [
              prop.className "flex flex-col gap-3"
              prop.children [
                Html.button [
                  prop.className [
                    "w-full rounded-lg px-4 py-3 text-sm font-semibold shadow-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2"
                    if noTeam then
                      "bg-gray-300 text-gray-600 cursor-not-allowed dark:bg-gray-700 dark:text-gray-400"
                    else
                      "bg-indigo-600 text-white hover:bg-indigo-500 focus-visible:outline-indigo-500"
                  ]
                  prop.disabled noTeam
                  prop.onClick (fun _ -> dispatch (NavigateToPage StartGamePage))
                  prop.text (if selected = 0 then "Select lineup" else "Adjust lineup")
                ]
                Html.button [
                  prop.className [
                    "w-full rounded-lg px-4 py-3 text-sm font-semibold focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2"
                    if canStart then
                      "bg-emerald-600 text-white shadow-sm hover:bg-emerald-500 focus-visible:outline-emerald-500"
                    else
                      "bg-gray-200 text-gray-500 cursor-not-allowed dark:bg-gray-800 dark:text-gray-400"
                  ]
                  prop.disabled (not canStart)
                  prop.onClick (fun _ -> dispatch StartGame)
                  prop.text "Start game"
                ]
                Html.button [
                  prop.className "w-full rounded-lg border border-gray-300 bg-white px-4 py-3 text-sm font-semibold text-gray-700 hover:border-indigo-300 hover:text-indigo-600 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500 dark:border-white/15 dark:bg-gray-900/40 dark:text-gray-200 dark:hover:border-indigo-400/50 dark:hover:text-white"
                  prop.onClick (fun _ -> dispatch (NavigateToPage ManageTeamPage))
                  prop.text (if noTeam then "Add team players" else "Manage team")
                ]
              ]
            ]
          ]
        ]
      ]
    ]
