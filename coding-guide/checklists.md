# Checklists

## Before (all tasks)

- B1 State the scope and what is out of scope.
- B2 Name the modules touched (see the module map) and whether each is deep or shallow.
- B3 Search for existing helpers and patterns to reuse (M-03).
- B4 Decide which module's implementation absorbs the change.
- B5 A new type passes the deletion test (D-01), or do not add it.
- B6 Split pure decision logic from window/input code (D-05).
- B7 Plan tests and the interface they go through.
- B8 Run `./check.ps1 -Fast`; note pre-existing failures.

Extras:
- Feature: list new settings and their defaults.
- Bugfix: reproduce first; failing test first when the bug is in pure logic (T-02).
- Refactor: follow `refactor-playbook.md`; no behavior change.

## After (all tasks)

- A1 `dotnet format Keymo.slnx`, then `./check.ps1` is green.
- A2 Tests cover new pure logic; UI/input changes were run by hand and the report says what was run (T-04).
- A3 Read the real diff against `rules.md`: limits table, names, magic values, comments, error handling, dead code, P/Invoke placement.
- A4 Depth check: nothing got shallower (no pass-through, no single-implementation interface, no interface growing faster than its implementation).
- A5 Hook safety: nothing slow in the hook path; no key swallowed that Keymo does not handle (P-01, P-02).
- A6 Disposal: every new `Form`, `Timer`, icon or handle is disposed (P-03).
- A7 Baseline in `baseline/` did not grow.
- A8 `architecture.md`, `deepening-log.md` and the Help text match the new behavior.
