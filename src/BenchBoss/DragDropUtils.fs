namespace BenchBossApp.Components.BenchBoss

open Feliz
open Fable.Core.JsInterop
open Browser.Types
open Browser.Dom

module DragDropUtils =
  type DragContext = { ItemId: string; PreviewEl: HTMLElement option }
  let mutable private currentDrag : DragContext option = None

  [<Literal>]
  let private dragStartDelayMs = 120.

  type PendingDrag =
    { TimerId: float
      ItemId: string
      PlayerName: string
      Origin: HTMLElement
      PointerId: float option
      TouchId: float option
      StartX: float
      StartY: float
      LastX: float
      LastY: float }

  let mutable private pendingDrag : PendingDrag option = None

  let private clearPendingDrag () =
    match pendingDrag with
    | Some pending ->
        window.clearTimeout pending.TimerId |> ignore
        pendingDrag <- None
    | None -> ()

  let private updatePendingPointerMove (ev: PointerEvent) =
    match pendingDrag with
    | Some pending when pending.PointerId = Some ev.pointerId ->
        let dx = abs (ev.clientX - pending.StartX)
        let dy = abs (ev.clientY - pending.StartY)
        if dx > 6. || dy > 6. then
          clearPendingDrag ()
        else
          pendingDrag <- Some { pending with LastX = ev.clientX; LastY = ev.clientY }
    | _ -> ()

  let private updatePendingTouchMove (ev: TouchEvent) =
    match pendingDrag with
    | Some pending ->
        match pending.TouchId with
        | Some touchId ->
            let touches = ev.touches
            let mutable found = None
            for i in 0 .. int touches.length - 1 do
              let touch = touches.item(float i)
              if not (isNull touch) then
                let identifier: float = touch?identifier
                if identifier = touchId then
                  found <- Some touch
            match found with
            | Some touch ->
                let clientX: float = touch?clientX
                let clientY: float = touch?clientY
                let dx = abs (clientX - pending.StartX)
                let dy = abs (clientY - pending.StartY)
                if dx > 6. || dy > 6. then
                  clearPendingDrag ()
                else
                  pendingDrag <- Some { pending with LastX = clientX; LastY = clientY }
            | None -> ()
        | None -> ()
    | None -> ()

  let beginDrag (itemId: string) (playerName: string) (origin: HTMLElement) (clientX: float) (clientY: float) =
    if currentDrag.IsNone then
      let preview = unbox<HTMLElement> (document.createElement("div"))
      let style: obj = preview?style
      style?position <- "fixed"
      style?pointerEvents <- "none"
      style?touchAction <- "none"
      style?top <- clientY.ToString() + "px"
      style?left <- clientX.ToString() + "px"
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

  let private beginPendingDrag (pending: PendingDrag) =
    pendingDrag <- None
    beginDrag pending.ItemId pending.PlayerName pending.Origin pending.LastX pending.LastY

  let private schedulePendingDrag (pending: PendingDrag) =
    let mutable timerId = 0.
    timerId <-
      window.setTimeout(
        (fun _ ->
          match pendingDrag with
          | Some current when current.TimerId = timerId -> beginPendingDrag current
          | _ -> ()
        ),
        dragStartDelayMs
      )
    pendingDrag <- Some { pending with TimerId = timerId }

  let private schedulePointerDrag (itemId: string) (playerName: string) (origin: HTMLElement) (ev: PointerEvent) =
    clearPendingDrag ()
    let pending =
      { TimerId = 0.
        ItemId = itemId
        PlayerName = playerName
        Origin = origin
        PointerId = Some ev.pointerId
        TouchId = None
        StartX = ev.clientX
        StartY = ev.clientY
        LastX = ev.clientX
        LastY = ev.clientY }
    schedulePendingDrag pending

  let private scheduleTouchDrag (itemId: string) (playerName: string) (origin: HTMLElement) (ev: TouchEvent) =
    clearPendingDrag ()
    if ev.touches.length > 0. then
      let touch = ev.touches.item(0.)
      if not (isNull touch) then
        let clientX: float = touch?clientX
        let clientY: float = touch?clientY
        let identifier: float = touch?identifier
        let pending =
          { TimerId = 0.
            ItemId = itemId
            PlayerName = playerName
            Origin = origin
            PointerId = None
            TouchId = Some identifier
            StartX = clientX
            StartY = clientY
            LastX = clientX
            LastY = clientY }
        schedulePendingDrag pending

  let rec private ascendForAttr (attr: string) (el: HTMLElement) : HTMLElement option =
    if isNull el then None
    elif el.hasAttribute attr then Some el
    else ascendForAttr attr (unbox el.parentElement)

  let private findTarget (attr: string) (ev: PointerEvent) : HTMLElement option =
    let el = unbox<HTMLElement> ev.target
    if isNull el then None else ascendForAttr attr el

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
        updatePendingPointerMove pe
        moveDrag pe
        updateHover pe
      let pointerUpHandler = fun (e: Event) ->
        clearPendingDrag ()
        endDrag (unbox e) onDropField onDropBench
      let pointerCancelHandler = fun (_: Event) -> clearPendingDrag ()
      let touchEndHandler = fun (_: Event) ->
        clearPendingDrag ()
        match currentDrag with
        | Some _ ->
          let fake = jsOptions<PointerEvent>(fun _ -> ())
          endDrag fake onDropField onDropBench
        | None -> ()
      let touchMoveHandler = fun (e: Event) -> updatePendingTouchMove (unbox e)
      let touchCancelHandler = fun (_: Event) -> clearPendingDrag ()
      document.addEventListener("pointermove", pointerMoveHandler)
      document.addEventListener("pointerup", pointerUpHandler)
      document.addEventListener("pointercancel", pointerCancelHandler)
      document.addEventListener("touchend", touchEndHandler)
      document.addEventListener("touchmove", touchMoveHandler)
      document.addEventListener("touchcancel", touchCancelHandler)
      // Return cleanup disposer
      (fun () ->
        document.removeEventListener("pointermove", pointerMoveHandler)
        document.removeEventListener("pointerup", pointerUpHandler)
        document.removeEventListener("pointercancel", pointerCancelHandler)
        document.removeEventListener("touchend", touchEndHandler)
        document.removeEventListener("touchmove", touchMoveHandler)
        document.removeEventListener("touchcancel", touchCancelHandler)
        globalHooksRegistered <- false)
    else (fun () -> ())

  let draggableProps (itemId: string) (playerName: string) =
    [ prop.custom("data-player-name", playerName)
      prop.onPointerDown (fun ev ->
        let origin = unbox<HTMLElement> ev.target
        if not (isNull origin) then
          schedulePointerDrag itemId playerName origin (unbox ev))
      prop.onTouchStart (fun ev ->
        if currentDrag.IsNone then
          let origin = unbox<HTMLElement> ev.target
          if not (isNull origin) then
            scheduleTouchDrag itemId playerName origin (unbox ev))
    ]

  let fieldSlotProps (slotIndex: int) = [ prop.custom("data-drop-slot", slotIndex.ToString()) ]
  let benchProps () = [ prop.custom("data-drop-bench", "true") ]
