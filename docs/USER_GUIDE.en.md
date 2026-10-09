# User guide

[Italiano](GUIDA.md) · English · [Project overview](../README.en.md)

Windows widget and Stream Deck plugin for monitoring Codex quotas. Cyberpunk 2077 theme, original CD logo, shared colors and built-in settings.

Created by **Kentar — k3ntarlab (GitHub: kentar91)**. Distributed under the [MIT license](../LICENSE). To request a change or report a problem, open a [GitHub issue](https://github.com/kentar91/CodexDashboard/issues). Independent project, not affiliated with OpenAI, Elgato or CD PROJEKT RED.

## Quick start

Download ready-to-use packages from the [releases page](https://github.com/kentar91/CodexDashboard/releases/latest). To install, use `CodexDashboard-Setup-2.0.7.exe`; for portable use, extract `CodexDashboard-2.0.7-windows-x64.zip` and open `CodexDashboard.exe`. The `dist` folders described below are generated locally and are not included in the source repository.

- Double-click **Avvia-CodexDashboard.vbs** in the project root, or **dist/app/CodexDashboard.exe**.
- To configure: right-click the widget → **Settings**, use the tray icon menu, or open **dist/app/Impostazioni-CodexDashboard.vbs**.
- For Stream Deck: double-click **dist/streamdeck/com.codexdashboard.monitor.streamDeckPlugin** to install it.
- For the logo: **dist/branding/CodexDashboard-logo-kit.zip**. Editable files are in **assets/branding**.
- To install or update: **dist/installer/CodexDashboard-Setup-2.0.7.exe**. Choose Italiano or English, then app only, Stream Deck plugin only, or both. Installation is per user; shortcuts are created only for the app. An existing component that is not selected is retained. The plugin package opens in the Elgato app for import confirmation.

Requires Windows 10/11 x64, .NET Framework 4.8 and an installed Codex app already signed in with ChatGPT. Running the app does not require Node.js, Python or API keys. The plugin requires Stream Deck 6.6 or later and works with the widget closed.

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

Install update opens a complete installer downloaded by the user. The package updates app and plugin to the same version; Stream Deck import requires confirmation in the Elgato app. A public channel for automatic online updates is not yet available. Uninstalling retains settings and history; remove the Elgato plugin through the Stream Deck app.

Yellow identifies the short quota; turquoise identifies the weekly quota. Percentages and bars turn red at the configured threshold for each quota. Thresholds and notifications allows separate 5-hour and weekly thresholds (1–50% available), even when displaying used quota. Existing settings initialize both thresholds from the old value. Stream Deck keys follow their quota's threshold unless overridden per key. `!` and `OFFLINE` indicate previous data or a missing connection; `—` indicates unavailable data. Notifications do not repeat until quota rises and then falls again. Reconnection occurs after 15 seconds; reads time out after 25 seconds.

## Folders

| Folder | Contents |
| --- | --- |
| `src` | App, settings, layout and Stream Deck manifest |
| `assets` | Shared theme and official SVG/PNG/ICO logo |
| `docs` | Documentation and visual guidelines |
| `scripts` | Build, logo export and launcher templates |
| `tests` | Automated checks |
| `build` | Compiled plugin, generated sources, previews and test results |
| `dist` | Ready-to-use app, plugin, installer and logo kit |

See [project structure](STRUCTURE.en.md), [design guidelines](DESIGN.en.md) and [palette](../assets/theme.json).

## Building and packaging

Close the widget and Settings. Run **build.ps1** from the project root: it calls `scripts/build.ps1` and recreates the app, compiled plugin and logo ZIP. The .NET compiler is provided by Windows; exported graphics are included in the project.

To regenerate the Stream Deck installation package, use `build.ps1 -Package` with the official Elgato CLI available, or `build.ps1 -Package -StreamDeckCli <CLI path>`. The source manifest is `src/streamdeck/manifest.json`; do not edit the generated copy in `build/streamdeck`.

`build.ps1 -Installer -StreamDeckCli <CLI path>` also creates the bilingual Windows installer containing app, icon and Stream Deck package from the same build.

Logo sources are in `assets/branding`; `scripts/export-brand.cjs` regenerates PNG and ICO files with Sharp. The kit includes the transparent symbol, full wordmark, HUD icon, monochrome variants and Windows icons from 16 to 256 pixels.

## Verification

To save checks inside the project, set `CODEXDASHBOARD_ARTIFACT_DIR` to an absolute path to `build/reports` or `build/previews`. Otherwise, results go to `%LOCALAPPDATA%/CodexDashboard/diagnostics`.

The app exposes `--self-test`, `--settings-test`, `--verify`, `--preview`, `--config-preview` and `--deck-preview`. These check quota conversion, settings, resizing, alerts, the live connection and previews. Settings tests use a separate folder without changing Windows startup.

`tests/streamdeck-smoke.cjs` simulates the Stream Deck connection and checks registration, three 144 × 144 PNG images and the refresh command. Results go to `build/reports`.

`tests/release-smoke.ps1` also checks Apply/Save/Cancel, language, startup modes, scaling, history, per-key settings and matching installer resources. Checks requiring real devices and workstations are listed in [verification notes](VERIFICATION.en.md). English action-list labels follow [Elgato localization](https://docs.elgato.com/streamdeck/sdk/guides/i18n/); keys and their settings follow the app language or the per-key choice.

## Data

Uses `codex app-server --listen stdio://`, the initialize/initialized handshake, `account/rateLimits/read` and `account/rateLimits/updated` notifications. The `codex` bucket takes precedence over the legacy result. The app sends no prompts, does not read or copy authentication files, and does not spend reset credits. The plugin communicates with Stream Deck through a local WebSocket.

This version shows subscription quotas and their history; it does not include costs or token history. Data comes from periodic reads and Codex updates.

References: [Codex App Server](https://learn.chatgpt.com/docs/app-server), [Stream Deck manifest](https://docs.elgato.com/streamdeck/sdk/references/manifest/), [Stream Deck WebSocket](https://docs.elgato.com/streamdeck/sdk/references/websocket/plugin/).
