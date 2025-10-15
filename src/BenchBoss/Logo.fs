namespace BenchBossApp

open Feliz

module Logo =
  [<ReactComponent>]
  let BenchBossLogo () =
    // Simple inline SVG logo styled with Tailwind utility classes.
    Html.div [
      prop.className "flex items-center select-none"
      prop.children [
        Svg.svg [
          svg.viewBox (0, 0, 48, 48)
          svg.className "h-8 w-8 rounded-md bg-gradient-to-br from-purple-600 to-indigo-600 p-1 shadow-sm"
          svg.children [
            // Shield / badge shape
            Svg.path [
              svg.d "M24 4c-.6 0-1.2.17-1.7.5L10 12.2a3 3 0 0 0-1.3 2.5v9.2c0 5.6 3.4 10.9 8.8 14.5 2.3 1.5 4.8 2.6 6.5 3.2.6.2 1.2.2 1.8 0 1.7-.6 4.2-1.7 6.5-3.2 5.4-3.6 8.7-8.9 8.7-14.5v-9.2a3 3 0 0 0-1.3-2.5L25.7 4.5c-.5-.33-1.1-.5-1.7-.5Z"
              svg.fill "#fff"
              //svg.opacity 0.2
            ]
            // Stylized BB monogram: two vertical bars + arcs (soccer goal vibe)
            Svg.path [
              svg.d "M18 16c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h2.5c3.6 0 5.5-1.8 5.5-4.2 0-1.6-.9-2.9-2.4-3.5 1.2-.6 1.9-1.8 1.9-3.3 0-2.5-1.8-5-5.3-5H18Zm2.4 3c1.5 0 2.4.9 2.4 2.1 0 1.2-.9 2.1-2.4 2.1H20v-4.2h.4Zm.2 7.2c1.6 0 2.6.8 2.6 2 0 1.1-.9 2-2.6 2H20v-4h.6ZM28 16c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h2.5c3.6 0 5.5-1.8 5.5-4.2 0-1.6-.9-2.9-2.4-3.5 1.2-.6 1.9-1.8 1.9-3.3 0-2.5-1.8-5-5.3-5H28Zm2.4 3c1.5 0 2.4.9 2.4 2.1 0 1.2-.9 2.1-2.4 2.1H30v-4.2h.4Zm.2 7.2c1.6 0 2.6.8 2.6 2 0 1.1-.9 2-2.6 2H30v-4h.6Z"
              svg.fill "#fff"
            ]
          ]
        ]
        Html.span [
          prop.className "ml-2 font-bold tracking-tight text-lg bg-gradient-to-r from-purple-600 to-indigo-600 bg-clip-text text-transparent"
          prop.text "BenchBoss"
        ]
      ]
    ]
