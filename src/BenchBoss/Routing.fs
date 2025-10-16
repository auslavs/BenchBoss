namespace BenchBossApp.Components.BenchBoss

module Routing =
  open Feliz.Router
  open BenchBossApp.Components.BenchBoss.Types

  [<Literal>]
  let BaseSegment = "benchboss"

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
    let cleaned = segments |> List.filter (fun s -> s <> "") |> List.map _.ToLower()

    match cleaned with
    | [ BaseSegment ] -> HomePage
    | [ BaseSegment; "game" ] -> GamePage
    | [ BaseSegment; "manage-team" ] -> ManageTeamPage
    | [ BaseSegment; "game-setup" ] -> GameSetupPage
    | [ BaseSegment; "404" ] -> NotFoundPage
    | _ -> NotFoundPage
    |> fun p ->
      Browser.Dom.console.log("Parsed URL segments:", cleaned |> List.toArray, "->", Page.toString p)
      p

  let currentPageFromUrl () : Page =
    Browser.Dom.console.log("Current URL segments:", Router.currentPath() |> List.toArray)
    let segments = Router.currentPath()
    parseUrl segments
