# Version 2.0 verification

[Italiano](VERIFICHE.md) · English · [Main documentation](../README.en.md)

Version 2.0.7 closes the window, tray icon and process when closing is tied to Codex. `tests/full-close-smoke.ps1` starts two real processes with simulated Codex state and checks complete exit and a new instance. Combined with Codex startup, the per-user `CodexDashboard-CodexStartup` task runs `--launch-with-codex` every 10 seconds: the command exits immediately if Codex has no open windows and does not reopen the widget if an instance already exists. No CodexDashboard monitor remains active between sessions. Choosing another mode or uninstalling removes the task.

Checks introduced in version 2.0.2 also cover Settings buttons in a 450-pixel-high window, the next available reset expiry and the full monitor lifecycle (`--lifecycle-test`). Desktop processes without a window do not count as an open Codex app.

After a real reboot, sign into Windows, open Codex and run `tests/windows-startup-audit.ps1`. It saves `build/reports/windows-startup-audit.json` with the last system boot, installed version, startup-entry match, running monitor and Codex window presence. This check does not restart Windows or change the registry. The post-reboot check still needs to be performed; simulated tests do not replace it.

`tests/release-smoke.ps1` checks quota conversion, boundary values, saving, notifications, language, Apply/Save/Cancel, startup modes with paths containing spaces, widget sizes from 80% to 200%, history and reading damaged rows. Tests use separate folders and check that the Windows startup entry remains unchanged.

The Stream Deck simulator checks registration, 144 × 144 PNG images for all three keys, refresh and language changes for one key. The installer check extracts resources without installing and compares app, icon and plugin hashes. Settings and installer previews are available in both languages.

`--placement-test` checks all nine positions on a monitor with negative coordinates, margins on small screens, Apply and Cancel for positioning, actual window movement and fallback to the primary monitor when the saved monitor is missing. Different DPI values and physically disconnecting monitors remain in the checklist below.

Check on real workstations:

- Per-user installation, update and uninstall, with personal data retained.
- Plugin import confirmation in the Elgato app and readability on physical hardware.
- Windows sign-in with manual, automatic and Codex-triggered startup.
- Single and multiple monitors, 100/125/150/200% DPI, dragging between monitors and removing the monitor hosting the widget.
- Expired authentication and reconnection after a network interruption.

These checks require hardware, Windows sign-ins or connection changes that simulation cannot replace. Updates currently use the complete package selected by the user; automatic online updates require a distribution channel that has yet to be published.
