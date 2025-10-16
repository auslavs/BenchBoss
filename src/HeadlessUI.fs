namespace BenchBossApp

open Fable.Core
open Fable.Core.JsInterop
open Feliz

// Headless UI bindings
module HeadlessUI =
    let private headlessUI: obj = importAll "@headlessui/react"
    
    type DisclosureProps = {|
        As: string
        ClassName: string
        Children: ReactElement list
    |}
    
    type DisclosureButtonProps = {|
        As: string option
        Key: string
        Href: string option
        ClassName: string
        Children: ReactElement list
        OnClick: (unit -> unit) option
    |}
    
    type DisclosurePanelProps = {|
        ClassName: string
        Children: ReactElement list
        Key: string option
    |}
    
    type MenuProps = {|
        As: string
        ClassName: string
        Children: ReactElement list
    |}
    
    type MenuButtonProps = {|
        ClassName: string
        Children: ReactElement list
    |}
    
    // NOTE: Using a named record type for component props triggers a Fable warning
    // related to React Fast Refresh / HMR. Replace the named record with an
    // anonymous record usage in the component declaration.
    // (Other components can be migrated similarly if desired.)
    
    type MenuItemProps = {|
        Children: ReactElement
    |}

    [<ReactComponent>]
    let Disclosure (props: DisclosureProps) =
        Feliz.Interop.reactApi.createElement(headlessUI?Disclosure, createObj [
            "as" ==> props.As
            "className" ==> props.ClassName
            "children" ==> props.Children
        ])
    
    [<ReactComponent>]
    let DisclosureButton (props: DisclosureButtonProps) =
        // Build property list conditionally. Previous version misused `jsOptions` and list literals
        // leading to a type error (expected function but got obj) and ignored values.
        let propPairs =
            [ "className" ==> props.ClassName
              "children" ==> props.Children ]
            |> fun pairs ->
                let pairs =
                    match props.As with
                    | Some asValue -> ("as" ==> asValue)::pairs
                    | None -> pairs
                match props.Href with
                | Some href -> ("href" ==> href)::pairs
                | None -> pairs
                |> fun pairs ->
                    match props.OnClick with
                    | Some cb -> ("onClick" ==> (fun (_:obj) -> cb()))::pairs
                    | None -> pairs
            |> List.rev // reverse to preserve original ordering when optional props are present
        let propsObj = createObj propPairs
        Feliz.Interop.reactApi.createElement(headlessUI?DisclosureButton, propsObj)
    
    [<ReactComponent>]
    let DisclosurePanel (props: DisclosurePanelProps) =
        let pairs = ["className" ==> props.ClassName; "children" ==> props.Children]
        let pairs =
            match props.Key with
            | Some k -> ("key" ==> k)::pairs
            | None -> pairs
        Feliz.Interop.reactApi.createElement(headlessUI?DisclosurePanel, createObj pairs)
    
    [<ReactComponent>]
    let Menu (props: MenuProps) =
        Feliz.Interop.reactApi.createElement(headlessUI?Menu, createObj [
            "as" ==> props.As
            "className" ==> props.ClassName
            "children" ==> props.Children
        ])
    
    [<ReactComponent>]
    let MenuButton (props: MenuButtonProps) =
        Feliz.Interop.reactApi.createElement(headlessUI?MenuButton, createObj [
            "className" ==> props.ClassName
            "children" ==> props.Children
        ])
    
    [<ReactComponent>]
    let MenuItems (props: {| Transition: bool; ClassName: string; Children: ReactElement list |}) =
        Feliz.Interop.reactApi.createElement(headlessUI?MenuItems, createObj [
            "transition" ==> props.Transition
            "className" ==> props.ClassName
            "children" ==> props.Children
        ])
    
    [<ReactComponent>]
    let MenuItem (props: MenuItemProps) =
        Feliz.Interop.reactApi.createElement(headlessUI?MenuItem, createObj [
            "children" ==> props.Children
        ])

// Heroicons bindings
module Heroicons =
    let private heroicons: obj = importAll "@heroicons/react/24/outline"
    
    [<ReactComponent>]
    let Bars3Icon props = 
        Feliz.Interop.reactApi.createElement(heroicons?Bars3Icon, createObj props)
    
    [<ReactComponent>]
    let BellIcon props = 
        Feliz.Interop.reactApi.createElement(heroicons?BellIcon, createObj props)
    
    [<ReactComponent>]
    let XMarkIcon props = 
        Feliz.Interop.reactApi.createElement(heroicons?XMarkIcon, createObj props)