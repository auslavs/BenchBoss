namespace BenchBossApp

module Main =

  open Feliz
  open BenchBossApp.Components
  open Browser.Dom

  let root = ReactDOM.createRoot(document.getElementById "bench-boss")
  root.render(BenchBoss.Component.Render())