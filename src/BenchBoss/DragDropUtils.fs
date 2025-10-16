namespace BenchBossApp.Components.BenchBoss

open Feliz
open Fable.Core.JsInterop
open Browser.Types
open Browser.Dom

module DragDropUtils =
  type DragContext = { ItemId: string; PreviewEl: HTMLElement option }
  let mutable private currentDrag : DragContext option = None

  let rec private ascendForAttr (attr: string) (el: HTMLElement) : HTMLElement option =
    if isNull el then None
    elif el.hasAttribute attr then Some el
    else ascendForAttr attr (unbox el.parentElement)

  let private findTarget (attr: string) (ev: PointerEvent) : HTMLElement option =
    let el = unbox<HTMLElement> ev.target
    if isNull el then None else ascendForAttr attr el

  let beginDrag (itemId: string) (playerName: string) (origin: HTMLElement) (ev: PointerEvent) =
    if currentDrag.IsNone then
      ev.preventDefault()
      let preview = unbox<HTMLElement> (document.createElement("div"))
      let style: obj = preview?style
      style?position <- "fixed"
      style?pointerEvents <- "none"
      style?touchAction <- "none"
      style?top <- ev.clientY.ToString() + "px"
      style?left <- ev.clientX.ToString() + "px"
      style?opacity <- "0.85"
      style?zIndex <- "9999"
      preview.className <- "px-3 py-2 rounded bg-purple-600 text-white text-sm shadow-lg select-none"
      // Preview should only show player name, not timing stats
      preview.textContent <- playerName
      document.body.appendChild(preview) |> ignore
      currentDrag <- Some { ItemId = itemId; PreviewEl = Some preview }
      // Disable body scroll while dragging (mobile)
      document.body?style?overscrollBehavior <- "contain"
      document.body?style?touchAction <- "none"
  // Attempt pointer capture for reliable move tracking
  // TODO: Pointer capture skipped (Fable HTMLElement missing member); implement via JS emit if needed

  let moveDrag (ev: PointerEvent) =
    match currentDrag with
    | Some ctx ->
      ev.preventDefault()
      match ctx.PreviewEl with
      | Some p ->
        let style: obj = p?style
        let y = int (ev.clientY - 10.0)
        let x = int (ev.clientX - 10.0)
        style?top <- y.ToString() + "px"
        style?left <- x.ToString() + "px"
      | None -> ()
    | None -> ()

  // Track current hover target for styling
  let mutable private currentHoverFieldSlot : int option = None
  let mutable private hoveringBench = false

  // Apply hover classes dynamically
  let private applyHoverEffects () =
    // Clear all field slot hover classes first
    let slots = document.querySelectorAll("[data-drop-slot]")
    for i in 0 .. int slots.length - 1 do
      let el = unbox<HTMLElement> slots.[i]
      el.classList.remove("ring", "ring-purple-400", "ring-offset-2") |> ignore
    // Clear bench hover
    let bench = document.querySelector("[data-drop-bench]")
    if not (isNull bench) then
      let benchEl = unbox<HTMLElement> bench
      benchEl.classList.remove("ring", "ring-yellow-500", "ring-offset-2") |> ignore
    // Add to active field slot
    match currentHoverFieldSlot with
    | Some idx ->
      let selector = sprintf "[data-drop-slot='%d']" idx
      let target = document.querySelector(selector)
      if not (isNull target) then
        let tEl = unbox<HTMLElement> target
        tEl.classList.add("ring", "ring-purple-400", "ring-offset-2") |> ignore
    | None -> ()
    // Add to bench if hovering
    if hoveringBench then
      let bench2 = document.querySelector("[data-drop-bench]")
      if not (isNull bench2) then
        let bEl = unbox<HTMLElement> bench2
        bEl.classList.add("ring", "ring-yellow-500", "ring-offset-2") |> ignore

  let private updateHover (ev: PointerEvent) =
    match currentDrag with
    | Some _ ->
      // Use elementFromPoint for more reliable detection (event.target may be origin)
      let elAtPoint = document.elementFromPoint(ev.clientX, ev.clientY)
      let fieldEl =
        if isNull elAtPoint then None
        else
          let htmlEl = unbox<HTMLElement> elAtPoint
          ascendForAttr "data-drop-slot" htmlEl
      let benchEl =
        if isNull elAtPoint then None
        else
          let htmlEl = unbox<HTMLElement> elAtPoint
          ascendForAttr "data-drop-bench" htmlEl
      currentHoverFieldSlot <-
        match fieldEl with
        | Some f ->
          match System.Int32.TryParse (f.getAttribute("data-drop-slot")) with
          | true, idx -> Some idx
          | _ -> None
        | None -> None
      hoveringBench <- benchEl.IsSome && fieldEl.IsNone
      applyHoverEffects()
    | None -> ()

  let endDrag (ev: PointerEvent) (onDropField: int -> string -> unit) (onDropBench: string -> unit) =
    match currentDrag with
    | Some ctx ->
      ev.preventDefault()
      ctx.PreviewEl |> Option.iter (fun el -> if not (isNull el) then document.body.removeChild(el) |> ignore)
      // Refresh hover for final coordinates
      updateHover ev
      match currentHoverFieldSlot, hoveringBench with
      | Some slotIdx, _ -> onDropField slotIdx ctx.ItemId
      | None, true -> onDropBench ctx.ItemId
      | None, false -> ()
      currentDrag <- None
      // reset hover visuals
      currentHoverFieldSlot <- None
      hoveringBench <- false
      applyHoverEffects()
      // Restore body scroll
      document.body?style?overscrollBehavior <- null
      document.body?style?touchAction <- null
    | None -> ()

  let mutable private globalHooksRegistered = false
  let registerGlobal (onDropField: int -> string -> unit) (onDropBench: string -> unit) =
    if not globalHooksRegistered then
      globalHooksRegistered <- true
      let pointerMoveHandler = fun (e: Event) ->
        let pe = unbox<PointerEvent> e
        moveDrag pe
        updateHover pe
      let pointerUpHandler = fun (e: Event) -> endDrag (unbox e) onDropField onDropBench
      let touchEndHandler = fun (_: Event) ->
        match currentDrag with
        | Some _ ->
          let fake = jsOptions<PointerEvent>(fun _ -> ())
          endDrag fake onDropField onDropBench
        | None -> ()
      document.addEventListener("pointermove", pointerMoveHandler)
      document.addEventListener("pointerup", pointerUpHandler)
      document.addEventListener("touchend", touchEndHandler)
      // Return cleanup disposer
      (fun () ->
        document.removeEventListener("pointermove", pointerMoveHandler)
        document.removeEventListener("pointerup", pointerUpHandler)
        document.removeEventListener("touchend", touchEndHandler)
        globalHooksRegistered <- false)
    else (fun () -> ())

  let draggableProps (itemId: string) (playerName: string) =
    [ prop.custom("data-player-name", playerName)
      prop.onPointerDown (fun ev ->
        let origin = unbox<HTMLElement> ev.target
        if not (isNull origin) then
          ev.preventDefault()
          beginDrag itemId playerName origin (unbox ev))
      prop.onTouchStart (fun ev ->
        if currentDrag.IsNone then
          let origin = unbox<HTMLElement> ev.target
          beginDrag itemId playerName origin (unbox ev))
    ]

  let fieldSlotProps (slotIndex: int) = [ prop.custom("data-drop-slot", slotIndex.ToString()) ]
  let benchProps () = [ prop.custom("data-drop-bench", "true") ]
