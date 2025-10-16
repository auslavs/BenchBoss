---
applyTo: '**'
---

# BenchBoss Codebase Instructions

## Project Overview

BenchBoss is a **client-side soccer game management application** built with:
- **F# + Fable** (compiles F# to JavaScript)
- **Feliz** (F# React bindings using JSX-like syntax)
- **TailwindCSS** for styling
- **Vite** as build tool and dev server
- **Browser Local Storage** for persistence (no backend)

The app helps manage players during kids' soccer matches by tracking:
- Team roster and player availability
- Game timer (two halves)
- Player field time vs bench time
- Score tracking with goal events

## Key Architecture Patterns

### 1. **Elmish/MVU Pattern**
- **State**: Single immutable state record in `Types.fs`
- **Messages**: Union type `Msg` for all possible actions
- **Update**: Pure functions that transform `State` based on `Msg`
- **View**: React components that render UI and dispatch messages

## Development Workflow

### Starting Development
```powershell
npm install             # Install JavaScript dependencies
npm run start           # Start Fable compiler + Vite dev server
```
- Fable watches F# files and compiles to JavaScript
- Vite provides hot reload at http://localhost:8080
- Changes to F# trigger automatic recompilation

### Build for Production
```powershell
npm run build       # Compile and build to /docs directory
```

### Build for testing
When testing if the application builds, prefer to use `npm run start` over `npm run build`.
This is so that I (the user that you are assisting) can see the application in action and catch any issues early.

## **Key F# Concepts Used**

### Indentation
- Use 2 spaces per indentation level (no tabs)

### **React Component Attribute**
```fsharp
[<ReactComponent>]
let MyComponent props = 
    Html.div [ /* JSX-like syntax */ ]
```

## State Management Best Practices

### **Pure Update Functions**
- All `State.update` functions are pure (no side effects)
- Return new state + command effects
- Use helper functions to keep updates readable

### **Validation & Consistency**
- `validateState` ensures data integrity after updates
- Removes invalid player references
- Called on load and after major changes

### **Persistence**
- State automatically saved to localStorage after updates
- `Store.fs` handles serialization/deserialization
- Graceful fallback to empty state if storage fails

## Styling Approach

### TailwindCSS Classes
- Utility-first approach with class composition
- Responsive design with breakpoint prefixes
- Dark mode not currently implemented
- Custom soccer field visual using CSS Grid

### Common Patterns
```fsharp
prop.className "flex flex-col space-y-4 p-4"
prop.className "bg-green-500 hover:bg-green-600 text-white px-4 py-2 rounded"
```

If you need to apply styles conditionally, you can pass a list of strings to `className` like this:
```fsharp
let isActive = true
prop.className [
  "base-class"
  if isActive then "active-class" else "inactive-class"
]
```

## Debugging Tips

### 1. **F# Compilation Errors**
- Check terminal output from `npm run start`
- Fable errors are usually clear about type mismatches
- Ensure all `open` statements are correct

### 2. **Runtime JavaScript Errors**
- Use browser dev tools console
- F# line numbers may not match JS source maps
- Check for `null`/`undefined` issues in interop

### 3. **State Debugging**
- Add temporary `console.log` in update functions
- Prefer to use Browser.Dom.console.log rather than printfn
- Use React Dev Tools to inspect component state
- Check localStorage for persisted state issues

## Common Modification Patterns

### Adding New Message Type
1. Add to `Msg` union in `Types.fs`
2. Handle in `State.update` function
3. Dispatch from appropriate component

### Adding New UI Component
1. Create new `.fs` file in appropriate folder
2. Add to `.fsproj` compilation order
3. Use `[<ReactComponent>]` attribute
4. Import and use in parent component

### Modifying Game Rules
1. Update domain types in `Types.fs` if needed
2. Modify validation logic in `State.fs`
3. Update UI components to reflect changes
4. Test drag/drop behavior thoroughly

## Testing Strategy

- Currently no automated tests (template includes test setup)
- Manual testing focus on:
  - Drag/drop edge cases
  - Timer accuracy across browser tabs
  - State persistence across page reloads
  - Mobile responsiveness

## Browser Compatibility

- Modern browsers with ES6+ support
- Uses Web Storage API for persistence
- Responsive design for mobile devices
- No server-side rendering (SPA only)

## Deployment

The app builds to static files in `/docs` directory, suitable for:
- GitHub Pages (current setup with `base: "/BenchBoss/"`)
- Any static file hosting
- CDN deployment

Current configuration assumes GitHub Pages deployment at `/BenchBoss/` path.