namespace BenchBossApp.Components.BenchBoss

module Routing =
  open Feliz.Router
  open BenchBossApp.Components.BenchBoss.Types

  // Don't include base segment in the routes - GitHub Pages handles that
  let private pageSegments = function
    | HomePage -> [ ]  // Just #/
    | GamePage -> [ "game" ]  // #/game
    | ManageTeamPage -> [ "manage-team" ]  // #/manage-team
    | GameSetupPage -> [ "game-setup" ]  // #/game-setup
    | NotFoundPage -> [ "404" ]  // #/404

  let href page =
    pageSegments page |> Router.format

  let parseUrl (segments:string list) : Page =
    // Clean up segments
    let cleaned = segments |> List.filter (fun s -> s <> "") |> List.map (_.ToLower())

    match cleaned with
    | [] -> HomePage
    | [ "game" ] -> GamePage
    | [ "manage-team" ] -> ManageTeamPage
    | [ "game-setup" ] -> GameSetupPage
    | [ "404" ] -> NotFoundPage
    | _ -> NotFoundPage

  let currentPageFromUrl () : Page =
    let segments = Router.currentPath()
    parseUrl segments
