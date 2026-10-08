# Core rules

Each rule is checked by a tool (named in brackets) or by a checklist item in `checklists.md`.

## Limits (hard for new code)

| Limit | Value | Checked by |
|---|---|---|
| File length | 300 lines | `scripts/limits.ps1` |
| Line length | 140 chars | `scripts/limits.ps1` |
| Cyclomatic complexity per member | 10 | CA1502 + `CodeMetricsConfig.txt` |
| Method length | 40 lines | A3 |
| Parameters per method | 4 | A3 |
| Compiler/analyzer warnings | 0 | build (`TreatWarningsAsErrors`) |

## D - Design (deep modules)

- D-01 A new type must pass the deletion test: deleting it would spread its complexity over callers, not just move it. [B5]
- D-02 No pass-through methods and no interface with a single implementation. [A4]
- D-03 A module's interface is smaller than its implementation: callers never need to know call order or internal state. [A4]
- D-04 Win32 calls stay private to the module that needs them. No shared `Native` grab-bag. [A3]
- D-05 Decision logic that can be pure is pure (no window, no cursor, no file) so it is testable without a desktop. [B6]
- D-06 Tests exercise a module through its interface, never private members. [A3]

## C - Code

- C-01 Names say what, not how: no abbreviations beyond common ones (`dpi`, `json`).
- C-02 No magic values: a number or string with meaning gets a named constant or a `Settings` property. [A3]
- C-03 Comments explain why, never restate the code. Each type has a one-line summary comment.
- C-04 Everything is `internal sealed` unless there is a reason otherwise. [CA1852]
- C-05 No dead code, commented-out code or leftover debug output. [A3]

## E - Errors

- E-01 Catch only specific exception types that the code can actually handle. No bare `catch`. [A3]
- E-02 A failed Win32 call that leaves the app unusable throws `Win32Exception`; one that is cosmetic is ignored on purpose with a comment.
- E-03 Bad user data (settings file) falls back to defaults; it never crashes startup. [test]

## T - Tests

- T-01 Every pure module has tests for its behavior, including edge cases. [A2]
- T-02 A bugfix starts with a failing test when the bug is in pure logic. [B7]
- T-03 Test names read as behavior: `Escape_with_nothing_selected_dismisses`.
- T-04 UI and input injection are verified by running the app; say what was run in the report. [A2]

## P - Performance and safety

- P-01 The keyboard hook callback does no I/O and nothing slow; Windows drops hooks that stall.
- P-02 Never swallow a key the user did not ask Keymo to handle. Keymo's own injected mask key always passes through.
- P-03 Dispose what you create (`Form`, `Timer`, `NotifyIcon`, hooks).

## M - Method

- M-01 One concern per change.
- M-02 Bugs noticed outside the task are reported, not fixed in the same change.
- M-03 Search for an existing helper before writing a new one.
- M-09 Update `architecture.md` when modules or seams change.
- M-10 A suppression (`#pragma`, `SuppressMessage`, baseline entry) carries an inline reason.
