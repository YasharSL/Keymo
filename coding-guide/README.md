# Keymo coding guide

Single source of truth for how code is written here. Read before and after every change (the `ystack-code` skill does this).

## Read order

1. `rules.md` - core rules and the limits table
2. `stack/csharp-winforms.md` - stack rules and tool config
3. `architecture.md` - glossary, module map, layering, seams
4. `checklists.md` - before/after checklists per task type
5. `enforcement.md` - gates and how to run them
6. `refactor-playbook.md`, `deepening-log.md` - when refactoring

## Stack

C# on .NET 10, WinForms (`net10.0-windows`), one app project (`src/Keymo`) and one xUnit project (`tests/Keymo.Tests`). No third-party runtime dependencies. Windows only.

## Commands

| Command | What it does |
|---|---|
| `./check.ps1` | Every gate: limits, build (warnings are errors), format, tests |
| `./check.ps1 -Fast` | Limits and build only |
| `dotnet format Keymo.slnx` | Apply formatting |
| `dotnet run --project src/Keymo` | Run the app |

## Precedence

User instruction > `docs/adr/` > this guide > tool defaults. If a rule is wrong for a case, waive it in the change report with the reason and propose the guide edit; never skip it silently.
