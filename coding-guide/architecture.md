# Architecture

## Glossary

- **Module**: a unit with an interface and an implementation (here: usually one type).
- **Interface**: everything a caller must know to use a module: members, ordering, error behavior.
- **Implementation**: what the module hides.
- **Depth**: how much behavior sits behind how small an interface. Deep is good; shallow means the interface is nearly as complex as the implementation.
- **Seam**: a place where behavior can be swapped without editing callers.
- **Adapter**: a concrete thing plugged into a seam.
- **Leverage**: what callers gain from a module's depth.
- **Locality**: a change, bug or piece of knowledge lives in one place.
- **Deletion test**: imagine deleting the module. If its complexity reappears across callers, it earns its place. If it just moves, it is shallow.

## Module map (`src/Keymo`)

| Module | Interface | Hides |
|---|---|---|
| `Program` | `Main` | single-instance mutex, app bootstrap |
| `TrayApp` | constructed and run | tray icon, menu, dialogs, routing a key press to hotkeys and modes |
| `KeyboardHook` | `new(handler)`, `Paused`, `Dispose` | low-level hook, modifier state (incl. an Alt held but lifted by a scroll), Alt-menu suppression |
| `Input` | `MoveTo`, `Scroll`, `Click`, `SetButton`, `TapKey`, `AltMenuMaskKey` | `SendInput` structs and flags, lifting a held Alt so the wheel arrives plain, exact absolute moves |
| `CursorMode` | `Toggle`, `TurnOn`, `HandleKey`, `HandleKeyUp`, `IgnoreHeldEnter` | badge window, cursor follow timer, arrow/scroll mapping, holding a button while Enter is down, drag lock |
| `GridOverlay` | `Toggle`, `HandleKey`, `Jumped` event | overlay placement, painting, cursor jump |
| `GridSelection` (pure) | `Press`, `Target`, `CellBounds`, `Reset` | column/row state machine and cell geometry |
| `OverlayForm` | base class | non-activating, click-through, topmost window styles |
| `KeyboardGlyph` | `Draw`, `ToIcon` | the keyboard picture used by badge and tray |
| `Settings` (pure + file) | properties, `Load`, `Save`, `HotkeyText` | JSON format, defaults, bad-file fallback |
| `SettingsForm` | `new(settings)`, `ShowDialog` | layout, hotkey capture, validation |
| `UpdateCheck` | `DescribeAsync`, `Current` | GitHub release lookup, version parsing |

## Layering

`Program` -> `TrayApp` -> modes (`CursorMode`, `GridOverlay`) and dialogs -> leaf modules (`Input`, `GridSelection`, `Settings`, `OverlayForm`, `KeyboardGlyph`, `UpdateCheck`).

- Leaf modules never reference `TrayApp` or the modes.
- The modes do not know each other. `TrayApp` decides what follows a grid jump (`GridOverlay.Jumped` + `Settings.AfterGridJump`).
- Only `TrayApp` knows about `KeyboardHook`. Modes receive key presses through `HandleKey(Keys)` (and `CursorMode` releases through `HandleKeyUp`) and return whether they consumed the key.
- Pure modules (`GridSelection`, `Settings` parsing, `UpdateCheck.ParseLatest`) touch no window, cursor or network.

## Seam register

| Seam | Adapters | Why it exists |
|---|---|---|
| `KeyboardHook` handler (`Func<Keys, bool, bool>`) | `TrayApp.OnKey` | keeps hook plumbing apart from what keys mean |
| `Settings.Load/Save(path)` | real AppData path, temp path in tests | file format testable without touching the user's profile |

No other seams. Add one only when a second adapter actually exists.
