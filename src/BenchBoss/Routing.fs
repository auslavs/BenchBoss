namespace BenchBossApp.Components.BenchBoss

module Routing =
  open Feliz.Router
  open BenchBossApp.Components.BenchBoss.Types

  // Base path segment for GitHub Pages project deployment
  let [<Literal>] BaseSegment = "benchboss"

  let private pageSegments = function
    | HomePage -> [ BaseSegment ]
    | GamePage -> [ BaseSegment; "game" ]
    | ManageTeamPage -> [ BaseSegment; "manage-team" ]
    | GameSetupPage -> [ BaseSegment; "game-setup" ]
    | NotFoundPage -> [ BaseSegment; "404" ]

  let href page =
    pageSegments page |> Router.format

  let parseUrl (segments:string list) : Page =
    // Normalize by removing any empty segments and ensuring lowercase
    let cleaned = segments |> List.filter (fun s -> s <> "") |> List.map (fun s -> s.ToLower())
    // Drop leading base segment if present
    let withoutBase =
      match cleaned with
      | b :: rest when b = BaseSegment -> rest
      | other -> other
    match withoutBase with
    | [] -> HomePage
    | [ "game" ] -> GamePage
    | [ "manage-team" ] -> ManageTeamPage
    | [ "game-setup" ] -> GameSetupPage
    | [ "404" ] -> NotFoundPage
    | _ -> NotFoundPage

  let currentPageFromUrl () : Page =
    let segments = Router.currentPath() |> List.map (fun s -> s.Trim().ToLower())
    parseUrl segments
