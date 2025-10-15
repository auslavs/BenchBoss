namespace BenchBossApp.Components.BenchBoss

module Routing =
  open Feliz.Router
  open BenchBossApp.Components.BenchBoss.Types

  let private pageSegments = function
    | HomePage -> [ "" ]
    | GamePage -> [ "game" ]
    | ManageTeamPage -> [ "manage-team" ]

  let href page =
    pageSegments page |> Router.format

  let parseUrl (segments:string list) : Page =
    match segments with
    | []
    | ["" ] -> HomePage
    | [ "game" ] -> GamePage
    | [ "manage-team" ] -> ManageTeamPage
    | _ -> HomePage

  let currentPageFromUrl () : Page =
    let segments = Router.currentPath() |> List.map (fun s -> s.Trim().ToLower())
    parseUrl segments
