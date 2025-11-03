namespace BenchBossApp.Components.BenchBoss.GameSetupPage

[<AutoOpen>]
module Common =
  open Feliz

  let PrimaryBadge (text: string)=
    Html.span [
      prop.className "inline-flex items-center rounded-md bg-purple-500 px-2 py-1 text-xs font-medium text-white"
      prop.text text
    ]

  let SecondaryBadge (text: string)=
    Html.span [
      prop.className "inline-flex items-center rounded-md bg-gray-200 px-2 py-1 text-xs font-medium text-gray-600"
      prop.text text
    ]
  
  let UserCheck props =
    Svg.svg [
      svg.xmlns "http://www.w3.org/2000/svg"
      svg.fill "none"
      svg.viewBox (0, 0, 24, 24)
      svg.strokeWidth 1.5
      svg.stroke "currentColor"
      yield! props
      svg.children [
          Svg.path [
              svg.strokeLineCap "round"
              svg.strokeLineJoin "round"
              svg.d "M12.75 6.375a3.375 3.375 0 1 1-6.75 0 3.375 3.375 0 0 1 6.75 0ZM3 19.235v-.11a6.375 6.375 0 0 1 12.75 0v.109A12.318 12.318 0 0 1 9.374 21c-2.331 0-4.512-.645-6.374-1.766Z"
          ]
          Svg.path [
              svg.strokeLineCap "round"
              svg.strokeLineJoin "round"
              svg.d "m16 11 2 2 4-4"
          ]
      ]
    ]

  let UserMinus props =
    Svg.svg [
      svg.xmlns "http://www.w3.org/2000/svg"
      svg.fill "none"
      svg.viewBox (0, 0, 24, 24)
      svg.strokeWidth 1.5
      svg.stroke "currentColor"
      yield! props
      svg.children [
        Svg.path [
          svg.strokeLineCap "round"
          svg.strokeLineJoin "round"
          svg.d "M22 10.5h-6m-2.25-4.125a3.375 3.375 0 1 1-6.75 0 3.375 3.375 0 0 1 6.75 0ZM4 19.235v-.11a6.375 6.375 0 0 1 12.75 0v.109A12.318 12.318 0 0 1 10.374 21c-2.331 0-4.512-.645-6.374-1.766Z"
        ]
      ]
    ]

  let Star props =
    Svg.svg [
      svg.custom ("xmlns", "http://www.w3.org/2000/svg")
      svg.width 24
      svg.height 24
      svg.viewBox (0, 0, 24, 24)
      svg.fill "none"
      svg.stroke "currentColor"
      yield! props
      svg.strokeWidth 2
      svg.strokeLineCap "round"
      svg.strokeLineJoin "round"
      svg.children [
        Svg.path [
          svg.d "M11.525 2.295a.53.53 0 0 1 .95 0l2.31 4.679a2.123 2.123 0 0 0 1.595 1.16l5.166.756a.53.53 0 0 1 .294.904l-3.736 3.638a2.123 2.123 0 0 0-.611 1.878l.882 5.14a.53.53 0 0 1-.771.56l-4.618-2.428a2.122 2.122 0 0 0-1.973 0L6.396 21.01a.53.53 0 0 1-.77-.56l.881-5.139a2.122 2.122 0 0 0-.611-1.879L2.16 9.795a.53.53 0 0 1 .294-.906l5.165-.755a2.122 2.122 0 0 0 1.597-1.16z"
        ]
      ]
    ]


  let Select (props:{| defaultValue: string; options: (string * string) list |} ) =
    Html.div [
      Html.div [
        prop.className "mt-2 grid grid-cols-1"
        prop.children [
          Html.select [
            prop.className "col-start-1 row-start-1 w-full appearance-none rounded-md bg-white py-1.5 pr-8 pl-3 text-base text-gray-900 outline-1 -outline-offset-1 outline-gray-300 focus-visible:outline-2 focus-visible:-outline-offset-2 focus-visible:outline-purple-600 sm:text-sm/6 dark:bg-white/5 dark:text-white dark:outline-white/10 dark:*:bg-gray-800 dark:focus-visible:outline-purple-500"
            prop.defaultValue props.defaultValue
            prop.children [
              for value, label in props.options do
                Html.option [
                  prop.value value
                  prop.text label
                ]
            ]
          ]
          Svg.svg [
            svg.viewBox (0, 0, 16, 16)
            svg.fill "currentColor"
            svg.custom ("data-slot", "icon")
            svg.custom ("aria-hidden", "true")
            svg.className "pointer-events-none col-start-1 row-start-1 mr-2 size-5 self-center justify-self-end text-gray-500 sm:size-4 dark:text-gray-400"
            svg.children [
              Svg.path [
                svg.d "M4.22 6.22a.75.75 0 0 1 1.06 0L8 8.94l2.72-2.72a.75.75 0 1 1 1.06 1.06l-3.25 3.25a.75.75 0 0 1-1.06 0L4.22 7.28a.75.75 0 0 1 0-1.06Z"
                svg.custom ("clipRule", "evenodd")
                svg.custom ("fillRule", "evenodd")
              ]
            ]
          ]
        ]
      ]
    ]