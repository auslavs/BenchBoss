namespace BenchBossApp.Components.BenchBoss

open Feliz
open BenchBossApp.Components.BenchBoss.Types

module NotFoundPage =
  [<ReactComponent>]
  let View (dispatch: Msg -> unit) =
    Html.div [
      prop.className "flex-1 p-8 flex flex-col items-center justify-center text-center bg-gradient-to-b from-red-50 to-orange-50"
      prop.children [
        Html.h1 [ prop.className "text-5xl font-extrabold text-red-600 mb-4"; prop.text "404" ]
        Html.p [ prop.className "text-lg text-gray-700 mb-8 max-w-md"; prop.text "We couldn't find that page. It may have moved or never existed." ]
        Html.button [
          prop.className "px-6 py-3 rounded-lg bg-purple-600 hover:bg-purple-700 text-white font-medium shadow"
          prop.text "Go Home"
          prop.onClick (fun _ -> dispatch (NavigateToPage HomePage))
        ]
      ]
    ]
