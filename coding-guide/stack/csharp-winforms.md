# Stack rules: C# / .NET 10 / WinForms

## Tool config

- `Directory.Build.props`: `Nullable=enable`, `TreatWarningsAsErrors=true`, `AnalysisLevel=latest-recommended`, `EnforceCodeStyleInBuild=true`, `CodeMetricsConfig.txt` as an additional file.
- `.editorconfig`: formatting and style severities; `dotnet format --verify-no-changes` is a gate.
- Tests: xUnit, run with `dotnet test`.

## Rules

- S-01 File-scoped namespaces, one top-level type per file, file named after the type. Small private helper types may nest inside their only user. [IDE0161, A3]
- S-02 Nullable reference types stay enabled; no `!` except right after a check the compiler cannot see, with the check on the same or previous line. [build, A3]
- S-03 Forms are built in code. No designer files, no `.resx`.
- S-04 Windows that must not take focus derive from `OverlayForm`. Never set `TopMost` on them directly (it activates the window on show).
- S-05 A hotkey is a WinForms `Keys` value (key code plus modifier flags). Text form comes only from `Settings.HotkeyText`.
- S-06 P/Invoke: `DllImport` declarations are `private static extern` inside the module that uses them, with `SetLastError = true` when the error is read. [D-04]
- S-07 No public settable properties on `Control`-derived types (WFO1000); pass data through constructors or methods.
- S-08 Mouse and key injection goes through `Input`. Cursor position goes through `Cursor.Position`.
- S-09 No new NuGet packages in the app project without a line in the report explaining why the framework is not enough.
