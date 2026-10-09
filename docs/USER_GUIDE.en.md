# User guide

[Italiano](GUIDA.md) · English · [Project overview](../README.en.md)

Windows widget and Stream Deck plugin for monitoring Codex quotas. Cyberpunk 2077 theme, original CD logo, shared colors and built-in settings.

Created by **Kentar — k3ntarlab (GitHub: kentar91)**. Distributed under the [MIT license](../LICENSE). To request a change or report a problem, open a [GitHub issue](https://github.com/kentar91/CodexDashboard/issues). Independent project, not affiliated with OpenAI, Elgato or CD PROJEKT RED.

## Installation and first launch

Requires Windows 10/11 x64, .NET Framework 4.8 and Codex installed and signed in with ChatGPT. API keys, Node.js and Python are not required to use the app. Stream Deck is optional; the plugin requires Elgato's app version 6.6 or later.

1. Download [CodexDashboard-Setup-2.0.7.exe](https://github.com/kentar91/CodexDashboard/releases/download/v2.0.7/CodexDashboard-Setup-2.0.7.exe).
2. Choose Italiano or English, then the app only, Stream Deck plugin only, or both.
3. For the app, launch **Codex Dashboard** using the shortcut created by the installer. For the plugin, confirm the import in Elgato's app.
4. Open **Settings** by right-clicking the widget or using the tray icon menu.

Installation is per user. An existing component that is not selected is retained. Installing only the plugin does not create an app shortcut.

### Portable use and plugin only

For portable use, download the [Windows x64 ZIP](https://github.com/kentar91/CodexDashboard/releases/download/v2.0.7/CodexDashboard-2.0.7-windows-x64.zip), extract it completely into a folder and open `CodexDashboard.exe`. The `.vbs` launchers in the extracted folder are optional. The ZIP does not automatically install the Stream Deck plugin.

For Stream Deck only, download the [plugin package](https://github.com/kentar91/CodexDashboard/releases/download/v2.0.7/com.codexdashboard.monitor.streamDeckPlugin), double-click it and confirm the import in Elgato. The plugin works with the widget closed.

### Updating and uninstalling

To update, download and run the current release's setup; confirm the new import in Stream Deck. Automatic online updates are not implemented. Remove the app using Windows uninstall; remove the plugin using the Stream Deck app. Personal settings and history are retained.

Executables are not digitally signed. [SHA-256 checksums](https://github.com/kentar91/CodexDashboard/releases/download/v2.0.7/SHA256SUMS.txt) let you verify download integrity. **Download ZIP** and **Download source code** are different: GitHub's generated source archives require building. The `build` and `dist` paths are for developers, covered in the [development guide](DEVELOPMENT.en.md).

## Settings

Configure size from 80% to 200%, idle opacity from 40% to 100%, always on top, position, manual/Windows/Codex startup, refresh every 30/60/120/300 seconds, separate thresholds for the 5-hour and weekly quotas, and optional notifications. Available/used quota display and refresh frequency also apply to the updated plugin.

**Apply** saves and applies immediately without closing. **Save** applies and closes. **Cancel** discards only changes made since the last Apply. **Language** selects Italiano or English for the widget, menus, settings and keys. **Reset** loads defaults, to be confirmed with Apply or Save. **Center widget** selects the screen center, also requiring Apply or Save. Automatic startup and notifications are initially disabled.

The startup choice applies immediately to the running app and is registered for future Windows sign-ins. In Codex startup mode, a tray monitor opens the widget when you open the desktop app. Widget closing can be manual only (default) or when Codex closes, independently of startup mode. Automatic closing occurs within about three seconds after detecting a Codex window open and then closed; remaining background processes do not count.

With startup when Codex opens and closing when Codex closes, the program exits completely: widget, tray icon and process. A per-user Windows task checks every 10 seconds and starts a new instance only while Codex is open; the widget can take about 10 seconds to appear. Apply/Save creates the task, the installer updates it, and choosing another mode or uninstalling removes it. With manual closing, the monitor can remain in the background. The X hides the widget in Codex startup mode. Exit terminates the app, which can start again on the next check if Codex remains open and automatic startup is enabled.

The widget screen setting selects the monitor. Widget position offers nine positions (corners, edge centers and screen center), or a custom dragged position. Distance from edges sets a margin of 0–120 pixels; the taskbar is excluded from the available area. The selection is saved and applied on restart or resizing. If the saved monitor is unavailable, the primary monitor is used. Dragging switches to custom positioning. Settings scroll on smaller displays; Reset, Cancel, Apply and Save remain visible at the bottom. Windows startup status and X behavior are shown in Settings.

Settings and position are saved in `%LOCALAPPDATA%/CodexDashboard` and shared with the plugin. Reorganizing the project does not change these personal files.

## Widget and keys

The draggable widget is 220 × 64 pixels at its default size. It shows the full title and percentages; Refresh and Close appear on hover. Opening the app again shows the existing instance. Title and quota tooltips also show available reset credits, read from Codex on every refresh. Missing data is marked unavailable; after a lost connection, the previous value is identified as the last received value. The tooltip also shows the next expiry among available reset credits. Quota tooltips show renewal times in Europe/Rome.

In Stream Deck, drag the 5-hour quota, weekly quota and open-widget actions onto keys. Press quota keys to refresh, or **CODEX DASHBOARD / OPEN** to open the app. Quotas are identified by duration: 300 and 10080 minutes; missing durations are reported. Buttons are supported; Stream Deck+ dials are not.

Select a key in the Stream Deck app to customize language, available/used percentage and the red threshold. The app-settings choice follows global configuration. Per-key changes are saved by Stream Deck.

Widget and tray menus provide quota history and about/diagnostics. History has separate 5-hour and weekly charts, filtered to the last 24 hours or 7 days. Axes show available percentage and Europe/Rome time; the plotted period runs from the first to the last collected sample, with values on hover. Features include CSV export, version, account plan, last update, offline status and JSON export. History retains up to 30 days and records no authentication data. Diagnostic export contains settings, quotas and the last connection error, without credentials.

Install update opens a complete installer downloaded by the user. The package updates app and plugin to the same version; Stream Deck import requires confirmation in the Elgato app. Automatic online updates are not implemented. Uninstalling retains settings and history; remove the Elgato plugin through the Stream Deck app.

Yellow identifies the short quota; turquoise identifies the weekly quota. Percentages and bars turn red at the configured threshold for each quota. Thresholds and notifications allows separate 5-hour and weekly thresholds (1–50% available), even when displaying used quota. Existing settings initialize both thresholds from the old value. Stream Deck keys follow their quota's threshold unless overridden per key. `!` and `OFFLINE` indicate previous data or a missing connection; `—` indicates unavailable data. Notifications do not repeat until quota rises and then falls again. Reconnection occurs after 15 seconds; reads time out after 25 seconds.

## Privacy and support

The app reads quotas through Codex, sends no prompts, does not read or copy authentication files, and does not spend reset credits. It displays subscription quota percentages, not token counts or API costs. Review diagnostic details before sharing them publicly.

For problems or requests, use the [issue templates](https://github.com/kentar91/CodexDashboard/issues/new/choose). For building and checks, see [Development](DEVELOPMENT.en.md) and [Verification](VERIFICATION.en.md).
