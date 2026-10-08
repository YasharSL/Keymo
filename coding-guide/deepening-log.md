# Deepening log

Backlog of places where a module could get deeper. Strength: `Strong`, `Worth exploring`, `Speculative`.

| ID | Module | Files | Strength | Friction | Deletion test | Win | Status |
|---|---|---|---|---|---|---|---|
| DL-01 | `CursorMode` | `src/Keymo/CursorMode.cs` | Speculative | The key-to-action mapping (move, scroll, click) is only verifiable by running the app because it calls `Input` and `Cursor` directly. | n/a (no shallow module; the mapping is a few lines) | Pure mapping would give test leverage if the mode grows more keys. | open |
