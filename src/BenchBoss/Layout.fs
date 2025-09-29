namespace BenchBossApp.Components.BenchBoss

module Layout =
  open System
  open Browser.Types
  open Feliz
  open BenchBossApp.Components.BenchBoss.Types

  let private iconClass isActive =
    if isActive then
      "size-6 shrink-0 text-indigo-600 dark:text-white"
    else
      "size-6 shrink-0 text-gray-400 group-hover:text-indigo-600 dark:group-hover:text-white"

  let private homeIcon isActive =
    Svg.svg [
      svg.viewBox (0, 0, 24, 24)
      svg.fill "none"
      svg.stroke "currentColor"
      svg.strokeWidth 1.5
      svg.className (iconClass isActive)
      svg.children [
        Svg.path [
          svg.d "m2.25 12 8.954-8.955c.44-.439 1.152-.439 1.591 0L21.75 12"
          svg.strokeLineCap "round"
          svg.strokeLineJoin "round"
        ]
        Svg.path [
          svg.d "M4.5 9.75v10.125c0 .621.504 1.125 1.125 1.125H9.75v-4.875c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125V21h4.125c.621 0 1.125-.504 1.125-1.125V9.75M8.25 21h8.25"
          svg.strokeLineCap "round"
          svg.strokeLineJoin "round"
        ]
      ]
    ]

  let private teamIcon isActive =
    Svg.svg [
      svg.viewBox (0, 0, 24, 24)
      svg.fill "none"
      svg.stroke "currentColor"
      svg.strokeWidth 1.5
      svg.className (iconClass isActive)
      svg.children [
        Svg.path [
          svg.d "M15 19.128a9.38 9.38 0 0 0 2.625.372 9.337 9.337 0 0 0 4.121-.952 4.125 4.125 0 0 0-7.533-2.493"
          svg.strokeLineCap "round"
          svg.strokeLineJoin "round"
        ]
        Svg.path [
          svg.d "M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07"
          svg.strokeLineCap "round"
          svg.strokeLineJoin "round"
        ]
        Svg.path [
          svg.d "M15 19.128v.106A12.318 12.318 0 0 1 8.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0 1 11.964-3.07"
          svg.strokeLineCap "round"
          svg.strokeLineJoin "round"
        ]
        Svg.path [
          svg.d "M12 6.375a3.375 3.375 0 1 1-6.75 0 3.375 3.375 0 0 1 6.75 0Z"
          svg.strokeLineCap "round"
          svg.strokeLineJoin "round"
        ]
        Svg.path [
          svg.d "M20.25 8.625a2.625 2.625 0 1 1-5.25 0 2.625 2.625 0 0 1 5.25 0Z"
          svg.strokeLineCap "round"
          svg.strokeLineJoin "round"
        ]
      ]
    ]

  let private clipboardIcon isActive =
    Svg.svg [
      svg.viewBox (0, 0, 24, 24)
      svg.fill "none"
      svg.stroke "currentColor"
      svg.strokeWidth 1.5
      svg.className (iconClass isActive)
      svg.children [
        Svg.path [
          svg.d "M2.25 12.75V12A2.25 2.25 0 0 1 4.5 9.75h15A2.25 2.25 0 0 1 21.75 12v.75"
          svg.strokeLineCap "round"
          svg.strokeLineJoin "round"
        ]
        Svg.path [
          svg.d "m13.06 6.31-2.12-2.12a1.5 1.5 0 0 0-1.061-.44H4.5A2.25 2.25 0 0 0 2.25 6v12a2.25 2.25 0 0 0 2.25 2.25h15A2.25 2.25 0 0 0 21.75 18V9a2.25 2.25 0 0 0-2.25-2.25h-5.379a1.5 1.5 0 0 1-1.06-.44Z"
          svg.strokeLineCap "round"
          svg.strokeLineJoin "round"
        ]
      ]
    ]

  let private whistleIcon isActive =
    Svg.svg [
      svg.viewBox (0, 0, 24, 24)
      svg.fill "none"
      svg.stroke "currentColor"
      svg.strokeWidth 1.5
      svg.className (iconClass isActive)
      svg.children [
        Svg.path [
          svg.d "M6.75 3v2.25"
          svg.strokeLineCap "round"
          svg.strokeLineJoin "round"
        ]
        Svg.path [
          svg.d "M17.25 3v2.25"
          svg.strokeLineCap "round"
          svg.strokeLineJoin "round"
        ]
        Svg.path [
          svg.d "M3 18.75V7.5a2.25 2.25 0 0 1 2.25-2.25h13.5A2.25 2.25 0 0 1 21 7.5v11.25"
          svg.strokeLineCap "round"
          svg.strokeLineJoin "round"
        ]
        Svg.path [
          svg.d "M3 18.75A2.25 2.25 0 0 0 5.25 21h13.5A2.25 2.25 0 0 0 21 18.75"
          svg.strokeLineCap "round"
          svg.strokeLineJoin "round"
        ]
        Svg.path [
          svg.d "M3 18.75v-7.5A2.25 2.25 0 0 1 5.25 9h13.5A2.25 2.25 0 0 1 21 11.25v7.5"
          svg.strokeLineCap "round"
          svg.strokeLineJoin "round"
        ]
      ]
    ]

  let private badge text =
    Html.span [
      prop.className "ml-auto w-9 min-w-max rounded-full bg-white px-2.5 py-0.5 text-center text-xs font-medium whitespace-nowrap text-gray-600 outline-1 -outline-offset-1 outline-gray-200 dark:bg-gray-900 dark:text-white dark:outline-white/15"
      prop.text text
    ]

  let private navLink dispatch state page label icon badgeText =
    let isActive = state.CurrentPage = page
    let classes =
      if isActive then
        "group flex gap-x-3 rounded-md bg-gray-50 p-2 text-sm font-semibold text-indigo-600 dark:bg-white/5 dark:text-white"
      else
        "group flex gap-x-3 rounded-md p-2 text-sm font-semibold text-gray-700 hover:bg-gray-50 hover:text-indigo-600 dark:text-gray-400 dark:hover:bg-white/5 dark:hover:text-white"

    Html.li [
      Html.a [
        prop.href "#"
        prop.className classes
        prop.onClick (fun (ev: MouseEvent) ->
          ev.preventDefault()
          dispatch (NavigateToPage page))
        prop.children [
          icon isActive
          Html.span label
          match badgeText with
          | Some text -> badge text
          | None -> Html.none
        ]
      ]
    ]

  let private playerInitial (name: string) =
    name.Trim()
    |> Seq.tryFind Char.IsLetter
    |> Option.defaultValue '#'
    |> string

  let private rosterEntry dispatch (player: TeamPlayer) =
    Html.li [
      Html.a [
        prop.key (string player.Id)
        prop.href "#"
        prop.className "group flex gap-x-3 rounded-md p-2 text-sm font-semibold text-gray-700 hover:bg-gray-50 hover:text-indigo-600 dark:text-gray-400 dark:hover:bg-white/5 dark:hover:text-white"
        prop.onClick (fun (ev: MouseEvent) ->
          ev.preventDefault()
          dispatch (NavigateToPage ManageTeamPage))
        prop.children [
          Html.span [
            prop.className "flex size-6 shrink-0 items-center justify-center rounded-lg border border-gray-200 bg-white text-[0.625rem] font-medium text-gray-400 group-hover:border-indigo-600 group-hover:text-indigo-600 dark:border-white/15 dark:bg-white/5 dark:group-hover:border-white/20 dark:group-hover:text-white"
            prop.text (playerInitial player.Name)
          ]
          Html.span [
            prop.className "truncate"
            prop.text player.Name
          ]
        ]
      ]
    ]

  let private headerTitle state =
    match state.CurrentPage with
    | LandingPage
    | StartGamePage -> "Game Setup"
    | ManageTeamPage -> "Team Management"
    | GamePage -> "Match Center"

  let private headerSubtitle state =
    match state.CurrentPage with
    | LandingPage
    | StartGamePage ->
        if String.IsNullOrWhiteSpace state.Game.Name then
          "Name your match and lock in today's lineup."
        else
          state.Game.Name
    | ManageTeamPage ->
        if state.TeamPlayers.IsEmpty then "Add players to build your roster." else $"{state.TeamPlayers.Length} players available"
    | GamePage -> state.Game.Name

  let private headerActions dispatch state =
    let actions =
      [
        if state.CurrentPage <> LandingPage then
          Html.div [
            prop.className "flex items-center gap-2"
            prop.children [
              Html.button [
                prop.className "rounded-full bg-indigo-600 px-4 py-2 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500"
                prop.text (string state.OurScore)
                prop.title "Click to add score for our team"
                prop.onClick (fun _ -> dispatch (ShowModal OurTeamScoreModal))
              ]
              Html.span [
                prop.className "text-sm font-semibold text-gray-500 dark:text-gray-300"
                prop.text "vs"
              ]
              Html.button [
                prop.className "rounded-full bg-slate-200 px-4 py-2 text-sm font-semibold text-slate-900 transition hover:bg-slate-300 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500 dark:bg-white/10 dark:text-white dark:hover:bg-white/20"
                prop.text (string state.OppScore)
                prop.title "Click to add score for the opposing team"
                prop.onClick (fun _ -> dispatch (ShowModal OpposingTeamScoreModal))
              ]
            ]
          ]

        if state.CurrentPage = GamePage || state.CurrentPage = ManageTeamPage then
          Html.button [
            prop.className "inline-flex items-center gap-2 rounded-full border border-indigo-200 px-4 py-2 text-sm font-semibold text-indigo-700 shadow-sm transition hover:border-indigo-400 hover:text-indigo-900 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-500 dark:border-white/15 dark:text-white dark:hover:border-white/30"
            prop.text (Time.formatMMSS (State.getElapsedSecondsInHalf state))
            prop.title "Open the time manager"
            prop.onClick (fun _ -> dispatch (ShowModal TimeManagerModal))
          ]
      ]
      |> List.filter (fun el -> not (obj.ReferenceEquals(el, Html.none)))

    match actions with
    | [] -> Html.none
    | xs ->
        Html.div [
          prop.className "flex items-center gap-4"
          prop.children xs
        ]

  let render (dispatch: Msg -> unit) (state: State) (pageContent: ReactElement) =
    let rosterItems =
      if state.TeamPlayers.IsEmpty then
        [
          Html.li [
            Html.div [
              prop.className "rounded-md border border-dashed border-gray-300 p-3 text-sm text-gray-500 dark:border-white/20 dark:text-gray-400"
              prop.text "No players yet. Add your squad from Team Management."
            ]
          ]
        ]
      else
        state.TeamPlayers
        |> List.sortBy (fun p -> p.Name)
        |> List.map (rosterEntry dispatch)

    let onFieldCount =
      state.Game.Players
      |> List.filter (fun p -> p.InGameStatus = OnField)
      |> List.length

    let navItems =
      [
        LandingPage, "Overview", homeIcon, None
        StartGamePage, "Start Game", clipboardIcon, Some (string state.Game.FieldSlots)
        ManageTeamPage, "Manage Team", teamIcon, (if state.TeamPlayers.IsEmpty then None else Some (string state.TeamPlayers.Length))
        GamePage, "Live Game", whistleIcon, (if onFieldCount > 0 then Some (string onFieldCount) else None)
      ]
      |> List.map (fun (page, label, icon, badgeText) -> navLink dispatch state page label icon badgeText)

    Html.div [
      prop.className "flex min-h-screen bg-white text-gray-900 dark:bg-gray-950 dark:text-white"
      prop.children [
        Html.aside [
          prop.className "relative flex w-72 shrink-0 flex-col gap-y-5 overflow-y-auto border-r border-gray-200 bg-white px-6 py-6 dark:border-white/10 dark:bg-gray-900"
          prop.children [
            Html.div [
              prop.className "relative flex h-16 shrink-0 items-center"
              prop.children [
                Html.img [
                  prop.className "h-8 w-auto dark:hidden"
                  prop.src "https://tailwindcss.com/plus-assets/img/logos/mark.svg?color=indigo&shade=600"
                  prop.alt "BenchBoss"
                ]
                Html.img [
                  prop.className "hidden h-8 w-auto dark:block"
                  prop.src "https://tailwindcss.com/plus-assets/img/logos/mark.svg?color=indigo&shade=500"
                  prop.alt "BenchBoss"
                ]
              ]
            ]
            Html.nav [
              prop.className "relative flex flex-1 flex-col"
              prop.children [
                Html.ul [
                  prop.className "flex flex-1 flex-col gap-y-7"
                  prop.children [
                    Html.li [
                      Html.ul [
                        prop.className "-mx-2 space-y-1"
                        prop.children navItems
                      ]
                    ]
                    Html.li [
                      Html.div [
                        prop.className "text-xs font-semibold uppercase tracking-wider text-gray-400"
                        prop.text "Your roster"
                      ]
                      Html.ul [
                        prop.className "-mx-2 mt-2 space-y-1"
                        prop.children rosterItems
                      ]
                    ]
                    Html.li [
                      prop.className "mt-auto"
                      prop.children [
                        Html.a [
                          prop.href "#"
                          prop.className "flex items-center gap-x-4 rounded-md px-4 py-3 text-sm font-semibold text-gray-900 hover:bg-gray-50 dark:text-white dark:hover:bg-white/5"
                          prop.onClick (fun (ev: MouseEvent) ->
                            ev.preventDefault()
                            dispatch (NavigateToPage ManageTeamPage))
                          prop.children [
                            Html.div [
                              prop.className "size-8 rounded-full bg-indigo-600 text-sm font-semibold text-white shadow-sm flex items-center justify-center"
                              prop.text "BB"
                            ]
                            Html.span [
                              prop.className "sr-only"
                              prop.text "Your profile"
                            ]
                            Html.span [
                              prop.ariaHidden true
                              prop.text "Coach Desk"
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
          prop.className "flex flex-1 flex-col"
          prop.children [
            Html.header [
              prop.className "flex items-center justify-between border-b border-gray-200 bg-white px-8 py-6 shadow-sm dark:border-white/10 dark:bg-gray-900"
              prop.children [
                Html.div [
                  prop.className "flex flex-col"
                  prop.children [
                    Html.h1 [
                      prop.className "text-lg font-semibold text-gray-900 dark:text-white"
                      prop.text (headerTitle state)
                    ]
                    Html.span [
                      prop.className "text-sm text-gray-500 dark:text-gray-300"
                      prop.text (headerSubtitle state)
                    ]
                  ]
                ]
                headerActions dispatch state
              ]
            ]
            Html.main [
              prop.className "flex-1 overflow-y-auto"
              prop.children [ pageContent ]
            ]
          ]
        ]
      ]
    ]
