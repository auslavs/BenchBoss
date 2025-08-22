namespace BenchBossApp.Components.BenchBoss.Modals

module Common =

  open Feliz

  let confirmButton isDisabled onClick =
    Html.button [
      prop.type' "button"
      prop.disabled isDisabled
      prop.className [
        "inline-flex w-full justify-center rounded-md bg-purple-600 px-3 py-2 text-sm font-semibold text-white shadow-xs"
        "hover:bg-purple-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600"
        "disabled:bg-purple-300"
        "dark:bg-purple-500 dark:shadow-none dark:hover:bg-purple-400 dark:focus-visible:outline-purple-500 sm:w-auto"
      ]
      prop.text "Record Goal"
      prop.onClick onClick
    ]

  let cancelButton onClick =
    Html.button [
      prop.type' "button"
      prop.className [
        "inline-flex w-full justify-center rounded-md px-3 py-2 text-sm font-semibold text-purple-700 shadow-xs"
        "border border-purple-200 hover:border-purple-300"
        "hover:bg-purple-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-300"
        "dark:bg-purple-700 dark:shadow-none dark:hover:bg-purple-600 dark:focus-visible:outline-purple-700 sm:w-auto"
      ]
      prop.text "Cancel"
      prop.onClick onClick
    ]

  let goalIcon =
    Svg.svg [
      svg.className "size-6 text-purple-600 dark:text-purple-400"
      svg.viewBox (0, 0, 24, 24)
      svg.fill "currentColor"
      svg.stroke "currentColor"
      svg.strokeWidth 1
      svg.children [
        // Soccer net
        Svg.path [
          svg.d "M2 8 L22 8 L20 20 L4 20 Z"
          svg.fill "none"
          svg.stroke "currentColor"
          svg.strokeWidth 0.5
        ]
        // Net mesh lines
        Svg.path [
          svg.d "M4 10 L20 10 M6 12 L18 12 M8 14 L16 14 M10 16 L14 16"
          svg.fill "none"
          svg.stroke "currentColor"
          svg.strokeWidth 0.3
        ]
        // Vertical net lines
        Svg.path [
          svg.d "M6 8 L5 20 M9 8 L8 20 M12 8 L12 20 M15 8 L16 20 M18 8 L19 20"
          svg.fill "none"
          svg.stroke "currentColor"
          svg.strokeWidth 0.3
        ]
        // Soccer ball
        Svg.circle [
          svg.cx 12
          svg.cy 5
          svg.r 2.5
          svg.fill "currentColor"
        ]
        // Soccer ball pattern (pentagons)
        Svg.path [
          svg.d "M12 3 L13 4 L12.5 5 L11.5 5 L11 4 Z"
          svg.fill "none"
          svg.stroke "#ffffff"
          svg.strokeWidth 0.3
        ]
        Svg.path [
          svg.d "M10.5 4.5 L11.5 3.5 M13.5 4.5 L12.5 3.5 M10 5.5 L11 6.5 M14 5.5 L13 6.5"
          svg.fill "none"
          svg.stroke "#ffffff"
          svg.strokeWidth 0.3
        ]
        // Motion lines
        Svg.path [
          svg.d "M8 3 L9 2.5 M7 4 L8 3.5 M6 5 L7 4.5"
          svg.fill "none"
          svg.stroke "currentColor"
          svg.strokeWidth 0.5
        ]
      ]
    ]
