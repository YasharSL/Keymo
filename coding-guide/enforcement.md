# Enforcement

## Gates

| Gate | Tool | In `-Fast` |
|---|---|---|
| File and line length | `coding-guide/scripts/limits.ps1` (ratchet against `baseline/limits.txt`) | yes |
| Compiler warnings, nullability, analyzers, complexity (CA1502 at 10), code style | `dotnet build` with `TreatWarningsAsErrors` | yes |
| Formatting | `dotnet format --verify-no-changes` | no |
| Tests | `dotnet test` | no |

Run all: `./check.ps1`. Fast: `./check.ps1 -Fast`.

## Ratchet

`baseline/limits.txt` lists `path` entries that are allowed to exceed the length limits. It starts empty. `limits.ps1` fails on any violation not listed, and also fails on a listed file that no longer violates, so the list can only shrink. Never add an entry to get a change through; shorten the file.

## Exceptions

A suppression needs an inline reason (M-10) and a line in the change report. Analyzer severities are changed only in `.editorconfig`, with a comment.

## Hooks and CI

`.github/workflows/ci.yml` runs `./check.ps1` on `windows-latest` for every push to `main` and every pull request. Pushing a `v*` tag also publishes `Keymo.exe` and creates the GitHub release for that tag. No local git hooks.
