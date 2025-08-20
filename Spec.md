Specification: Client-Side Game Tracker App
Overview

This is a client-side only application, written in F# with Feliz for the UI, styled using TailwindCSS.
The app manages a single game at a time by tracking:

Score (our team and opposition)

Game timer

Player participation (time on field vs bench)
All data is stored locally in the browser using the Web Storage API.

Functional Requirements
1. Player Management

The user can create and store a list of team players.

Player data persists across sessions using browser storage.

Players are organized into two lists:

Field list (≤ 4 players at once).

Bench list (remaining players, unlimited).

Player movement rules:

Players can be dragged from bench → empty field slot.

Players can be dragged from field → bench.

If the field is full, dragging a bench player onto a field player triggers a replacement (the existing player goes to bench, new player goes to field).

Each player shows:

Name

Minutes played

Minutes benched

2. Game Lifecycle

One game is tracked at a time.

Games are divided into two halves:

User can start a new half.

Timer resets each half, but scores and player minutes accumulate.

Timer controls:

Start / Pause

Stop

Elapsed game time is visible in the UI.

3. Timer & Tracking

While the timer is running:

Minutes played for field players increment.

Minutes benched for bench players increment.

Pausing or stopping halts tracking.

4. Scorekeeping

The user can record goals for:

Our team (with optional scoring player).

Opposition.

Score is displayed alongside elapsed time.

Display Specification

The display is divided into rows (top to bottom):

Score & Elapsed Time Row

Displays:

Elapsed time (large text, centered).

Scoreboard: Our Team vs Opposition, each with [+] button.

Field Players Row

List of up to 4 player boxes, one on top of the other (vertical stack).

Each box shows:

Player name

Minutes played

[Drag handle / interaction area]

Bench players can be dragged here:

If there’s an empty box → player fills it.

If dragging onto an occupied box → replacement occurs.

Bench Players Row

Vertical list of player boxes, one on top of the other.

Each shows:

Player name

Minutes benched

[Drag handle / interaction area]

Players can be dragged from bench → field.

Players can be dragged from field → bench.

Non-Functional Requirements

Client-only: No backend.

Persistence: Use localStorage or IndexedDB.

Framework: F# + Feliz.

Styling: TailwindCSS.

Responsiveness: Rows collapse naturally on mobile (still vertical stacking).

Performance: Support ~20 players smoothly.

Drag & Drop: Should feel natural (HTML5 drag-and-drop API or a React drag library).

Possible Extensions

Undo last move (swap or replacement).

Export/import team data.

History of past games.

Dark mode theme.