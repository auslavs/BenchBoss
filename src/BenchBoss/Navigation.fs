namespace BenchBossApp.Components

module Navigation =

  open Feliz
  open BenchBossApp
  open Browser.Dom
  open Fable.Core.JsInterop
  open BenchBossApp.Components.BenchBoss
  open Feliz.Router


  module private Mobile =

    let menuButton =
      Html.div [
        prop.className "-mr-2 flex items-center sm:hidden"
        prop.children [
          HeadlessUI.DisclosureButton {|
            As = None
            Href = None
            ClassName =
              "group relative inline-flex items-center justify-center rounded-md p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-500 focus:outline-2 focus:-outline-offset-1 focus:outline-purple-600 dark:hover:bg-white/5 dark:hover:text-white dark:focus:outline-purple-500"
            Children = [
              Html.span [ prop.className "absolute -inset-0.5" ]
              Html.span [ prop.className "sr-only"; prop.text "Open main menu" ]
              Heroicons.Bars3Icon [
                "aria-hidden" ==> true
                "className" ==> "block size-6 group-data-open:hidden"
              ]
              Heroicons.XMarkIcon [
                "aria-hidden" ==> true
                "className" ==> "hidden size-6 group-data-open:block"
              ]
            ]
            OnClick = None
          |}
        ]
      ]

    let pageHref page = Routing.href page

    let menuItem isCurrentPage (text: string) (page:Page) (onNavigate: unit -> unit) =
      let defaultClasses =
        "block border-l-4 border-transparent py-2 pr-4 pl-3 text-base font-medium text-gray-500 hover:border-gray-300 hover:bg-gray-50 hover:text-gray-700 dark:text-gray-300 dark:hover:border-white/20 dark:hover:bg-white/5 dark:hover:text-white"
      let currentPageClasses =
        "block border-l-4 border-purple-600 bg-purple-50 py-2 pr-4 pl-3 text-base font-medium text-purple-700 dark:border-purple-500 dark:bg-purple-600/10 dark:text-purple-400"

      HeadlessUI.DisclosureButton {|
        As = Some "a"
        Href = Some (pageHref page)
        ClassName = if isCurrentPage then currentPageClasses else defaultClasses
        Children = [ Html.text text ]
        OnClick = Some onNavigate
      |}

    let menuPanel (links: ReactElement list) =
      HeadlessUI.DisclosurePanel {|
        ClassName = "sm:hidden"
        Children = [
          Html.div [
            prop.className "space-y-1 pt-2 pb-3"
            prop.children links
          ]
        ]
      |}

  module private Desktop =
    let private pageHref page = Routing.href page

    let navLink isCurrentPage (text: string) (onClick: unit -> unit) (page:Page) =
      let defaultClasses =
        "inline-flex items-center cursor-pointer border-b-2 border-transparent px-1 pt-1 text-sm font-medium text-gray-500 hover:border-gray-300 hover:text-gray-700 dark:text-gray-300 dark:hover:border-white/20 dark:hover:text-white"
      let currentPageClasses =
        "inline-flex items-center cursor-pointer border-b-2 border-purple-600 px-1 pt-1 text-sm font-medium text-gray-900 dark:border-purple-500 dark:text-white"

      Html.a [
        prop.href (pageHref page)
        prop.className (if isCurrentPage then currentPageClasses else defaultClasses)
        prop.text text
        prop.onClick (fun ev -> ev.preventDefault(); onClick())
      ]

    let leftSideNav (navigate: Page -> unit) (links: ReactElement list) =
      Html.div [
        prop.className "flex"
        prop.children [
          Html.button [
            prop.className "flex shrink-0 items-center gap-2 group cursor-pointer"
            prop.onClick (fun _ -> navigate HomePage)
            prop.children [
              Logo.BenchBossLogo()
            ]
          ]
          Html.div [
            prop.className "hidden sm:ml-6 sm:flex sm:space-x-8"
            prop.children links
          ]
        ]
      ]

  [<ReactComponent>]
  let NavBar (navigate: Page -> unit, currentPage: Page) =
    HeadlessUI.Disclosure {|
      As = "nav"
      ClassName = "relative bg-white shadow-sm dark:bg-gray-800/50 dark:shadow-none dark:after:pointer-events-none dark:after:absolute dark:after:inset-x-0 dark:after:bottom-0 dark:after:h-px dark:after:bg-white/10"
      Children = [
        Html.div [
          prop.className "mx-auto max-w-7xl px-4 sm:px-6 lg:px-8"
          prop.children [
            Html.div [
              prop.className "flex h-16 justify-between"
              prop.children [
                // Left side - Logo and Nav Links
                Desktop.leftSideNav navigate [
                  Desktop.navLink (currentPage = HomePage) "Home" (fun () -> navigate HomePage) HomePage
                  Desktop.navLink (currentPage = ManageTeamPage) "Manage Team" (fun () -> navigate ManageTeamPage) ManageTeamPage
                  Desktop.navLink (currentPage = GamePage) "Current Game" (fun () -> navigate GamePage) GamePage
                ]
                // Html.div [
                //   prop.className "flex"
                //   prop.children [
                //     // Logo
                //     Html.div [
                //       prop.className "flex shrink-0 items-center"
                //       prop.children [ Logo.BenchBossLogo() ]
                //     ]
                //     // Desktop Nav Links
                //     Html.div [
                //       prop.className "hidden sm:ml-6 sm:flex sm:space-x-8"
                //       prop.children [
                //         Html.a [
                //           prop.href "#"
                //           prop.className "inline-flex items-center border-b-2 border-purple-600 px-1 pt-1 text-sm font-medium text-gray-900 dark:border-purple-500 dark:text-white"
                //           prop.text "Home"
                //           prop.onClick (fun _ -> navigate HomePage)
                //         ]
                //         Html.a [
                //           prop.href "#"
                //           prop.className "inline-flex items-center border-b-2 border-transparent px-1 pt-1 text-sm font-medium text-gray-500 hover:border-gray-300 hover:text-gray-700 dark:text-gray-300 dark:hover:border-white/20 dark:hover:text-white"
                //           prop.text "Manage Team"
                //           prop.onClick (fun _ -> navigate ManageTeamPage)
                //         ]
                //         Html.a [
                //           prop.href "#"
                //           prop.className "inline-flex items-center border-b-2 border-transparent px-1 pt-1 text-sm font-medium text-gray-500 hover:border-gray-300 hover:text-gray-700 dark:text-gray-300 dark:hover:border-white/20 dark:hover:text-white"
                //           prop.text "Current Game"
                //           prop.onClick (fun _ -> navigate GamePage)
                //         ]
                //       ]
                //     ]
                //   ]
                // ]

                // Right side - Notifications and Profile
                // Html.div [
                //   prop.className "hidden sm:ml-6 sm:flex sm:items-center"
                //   prop.children [
                //     // Notification button
                //     Html.button [
                //       prop.type' "button"
                //       prop.className
                //         "relative rounded-full p-1 text-gray-400 hover:text-gray-500 focus:outline-2 focus:outline-offset-2 focus:outline-purple-600 dark:hover:text-white dark:focus:outline-purple-500"
                //       prop.children [
                //         Html.span [ prop.className "absolute -inset-1.5" ]
                //         Html.span [ prop.className "sr-only"; prop.text "View notifications" ]
                //         Heroicons.BellIcon [ "aria-hidden" ==> true; "className" ==> "size-6" ]
                //       ]
                //     ]

                //     // Profile dropdown
                //     HeadlessUI.Menu {|
                //       As = "div"
                //       ClassName = "relative ml-3"
                //       Children = [
                //         HeadlessUI.MenuButton {|
                //           ClassName = "relative flex rounded-full focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-purple-600 dark:focus-visible:outline-purple-500"
                //           Children = [
                //             Html.span [ prop.className "absolute -inset-1.5" ]
                //             Html.span [ prop.className "sr-only"; prop.text "Open user menu" ]
                //             Html.img [
                //               prop.alt ""
                //               prop.src
                //                 "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=facearea&facepad=2&w=256&h=256&q=80"
                //               prop.className
                //                 "size-8 rounded-full bg-gray-100 outline -outline-offset-1 outline-black/5 dark:bg-gray-800 dark:outline-white/10"
                //             ]
                //           ]
                //         |}

                //         HeadlessUI.MenuItems {|
                //           Transition = true
                //           ClassName =
                //             "absolute right-0 z-10 mt-2 w-48 origin-top-right rounded-md bg-white py-1 shadow-lg outline outline-black/5 transition data-closed:scale-95 data-closed:transform data-closed:opacity-0 data-enter:duration-200 data-enter:ease-out data-leave:duration-75 data-leave:ease-in dark:bg-gray-800 dark:shadow-none dark:-outline-offset-1 dark:outline-white/10"
                //           Children = [
                //             HeadlessUI.MenuItem {|
                //               Children =
                //                 Html.a [
                //                   prop.href "#"
                //                   prop.className "block px-4 py-2 text-sm text-gray-700 data-focus:bg-gray-100 data-focus:outline-hidden dark:text-gray-300 dark:data-focus:bg-white/5"
                //                   prop.text "Your profile"
                //                 ]
                //             |}
                //             HeadlessUI.MenuItem {|
                //               Children =
                //                 Html.a [
                //                   prop.href "#"
                //                   prop.className "block px-4 py-2 text-sm text-gray-700 data-focus:bg-gray-100 data-focus:outline-hidden dark:text-gray-300 dark:data-focus:bg-white/5"
                //                   prop.text "Settings"
                //                 ]
                //             |}
                //             HeadlessUI.MenuItem {|
                //               Children =
                //                 Html.a [
                //                   prop.href "#"
                //                   prop.className "block px-4 py-2 text-sm text-gray-700 data-focus:bg-gray-100 data-focus:outline-hidden dark:text-gray-300 dark:data-focus:bg-white/5"
                //                   prop.text "Sign out"
                //                 ]
                //             |}
                //           ]
                //         |}
                //       ]
                //     |}
                //   ]
                // ]

                // Mobile menu button
                Mobile.menuButton
              ]
            ]
          ]
        ]

        // Mobile menu panel
        Mobile.menuPanel [
          Mobile.menuItem (currentPage = HomePage) "Home" HomePage (fun () -> navigate HomePage)
          Mobile.menuItem (currentPage = ManageTeamPage) "Manage Team" ManageTeamPage (fun () -> navigate ManageTeamPage)
          Mobile.menuItem (currentPage = GamePage) "Current Game" GamePage (fun () -> navigate GamePage)
        ]

        // HeadlessUI.DisclosurePanel {|
        //   ClassName = "sm:hidden"
        //   Children = [
        //     Html.div [
        //       prop.className "space-y-1 pt-2 pb-3"
        //       prop.children [
        //         HeadlessUI.DisclosureButton {|
        //           As = Some "a"
        //           Href = Some "#"
        //           ClassName = "block border-l-4 border-purple-600 bg-purple-50 py-2 pr-4 pl-3 text-base font-medium text-purple-700 dark:border-purple-500 dark:bg-purple-600/10 dark:text-purple-400"
        //           Children = [ Html.text "Dashboard" ]
        //         |}
        //         HeadlessUI.DisclosureButton {|
        //           As = Some "a"
        //           Href = Some "#"
        //           ClassName = "block border-l-4 border-transparent py-2 pr-4 pl-3 text-base font-medium text-gray-500 hover:border-gray-300 hover:bg-gray-50 hover:text-gray-700 dark:text-gray-300 dark:hover:border-white/20 dark:hover:bg-white/5 dark:hover:text-white"
        //           Children = [ Html.text "Team" ]
        //         |}
        //         HeadlessUI.DisclosureButton {|
        //           As = Some "a"
        //           Href = Some "#"
        //           ClassName = "block border-l-4 border-transparent py-2 pr-4 pl-3 text-base font-medium text-gray-500 hover:border-gray-300 hover:bg-gray-50 hover:text-gray-700 dark:text-gray-300 dark:hover:border-white/20 dark:hover:bg-white/5 dark:hover:text-white"
        //           Children = [ Html.text "Projects" ]
        //         |}
        //         HeadlessUI.DisclosureButton {|
        //           As = Some "a"
        //           Href = Some "#"
        //           ClassName = "block border-l-4 border-transparent py-2 pr-4 pl-3 text-base font-medium text-gray-500 hover:border-gray-300 hover:bg-gray-50 hover:text-gray-700 dark:text-gray-300 dark:hover:border-white/20 dark:hover:bg-white/5 dark:hover:text-white"
        //           Children = [ Html.text "Calendar" ]
        //         |}
        //       ]
        //     ]
        //   ]
        // |}
      ]
    |}

