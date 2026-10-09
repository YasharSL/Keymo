# Keymo

Control the mouse cursor with your keyboard. Keymo is a tiny Windows tray app: press a hotkey and the arrow keys move the cursor, or bring up a lettered grid and jump anywhere on the screen in three key presses.

- One small `.exe`, no installer, no background services
- Lives in the tray and stays out of the way until you press a hotkey
- Never steals focus from the app you are working in

## Install

1. Install the [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) (x64) if you do not have it.
2. Download `Keymo.exe` from the [latest release](https://github.com/YasharSL/Keymo/releases/latest).
3. Run it. A keyboard icon appears in the tray and Keymo is ready.

To start Keymo with Windows, put a shortcut to `Keymo.exe` in the folder that opens with `Win+R`, `shell:startup`.

## Cursor mode

Press **Ctrl+Alt+K**. A tiny keyboard badge appears next to the cursor to show the mode is on.

| Keys | Action |
|---|---|
| Arrows | Move the cursor in small steps (10 px) |
| Shift+Arrows | Move in big steps (100 px) |
| Alt+Arrows | Scroll up, down, left, right |
| Enter | Left click |
| Shift+Enter | Right click |
| Hold Enter + Arrows | Click and drag: the button stays down until you let go of Enter |
| Alt+Enter | Drag lock: the left button goes down and stays down, so you can move without holding anything. Enter or Alt+Enter drops it |
| Esc or Ctrl+Alt+K | Turn cursor mode off |

Drag lock is for compact keyboards where the arrows need Fn and cannot be reached while Enter is held.

While the mode is on, only these keys are taken by Keymo. Everything else you type goes to the app as usual.

## Grid

Press **Ctrl+Alt+G**. A see-through 20 x 20 grid covers the screen the cursor is on. Every cell shows two letters: its column, then its row.

1. Type the **column** letter. That column lights up and the rest dims.
2. Type the **row** letter. The cell is highlighted.
3. Press **Enter**. The cursor jumps to the middle of the cell and the grid closes.

After the jump Keymo turns on cursor mode, so you can fine-tune with the arrows and click with Enter straight away. In Settings, "After a grid jump" changes this to **Click** (Enter jumps and left-clicks in one go) or **Do nothing**.

**Esc** goes one step back: it clears the row, then the column, then closes the grid. **Ctrl+Alt+G** closes it at any point.

## Tray menu

Right-click the tray icon:

- **Settings** changes both hotkeys, the two step sizes, the scroll speed and what happens after a grid jump. Click a hotkey box and press the new combination (it must include Ctrl or Alt).
- **Help** lists the keys with your current hotkeys.
- **Check for updates** compares your version with the latest GitHub release.
- **About** and **Exit**.

Settings are stored in `%AppData%\Keymo\settings.json`. Delete the file to go back to the defaults.

## Build from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) on Windows.

```powershell
git clone https://github.com/YasharSL/Keymo.git
cd Keymo
dotnet run --project src/Keymo        # run it
./check.ps1                           # build, format check and tests
```

To produce the single-file `dist\Keymo.exe`:

```powershell
dotnet publish src/Keymo -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o dist
```

## How it works

Keymo is C# and WinForms with no third-party dependencies, about 700 lines.

- A low-level keyboard hook sees key presses system-wide and swallows only the ones Keymo handles.
- The badge and the grid are topmost, click-through windows that never activate, so the focused app keeps its focus.
- Clicks and scrolling are injected with `SendInput`.

The source is in `src/Keymo`, one small file per module. `coding-guide/architecture.md` has the module map.

## Contributing

Issues and pull requests are welcome. Before opening a pull request, read `coding-guide/README.md` and make sure `./check.ps1` passes. CI runs the same script.

Releases are cut by pushing a `v*` tag: CI builds `Keymo.exe` and attaches it to a new GitHub release.

## Limitations

- Windows only.
- Keymo cannot send input to windows running as administrator unless Keymo itself is run as administrator (a Windows rule).
- Mixed-DPI multi-monitor setups have not been tested.

## License

[MIT](LICENSE)
