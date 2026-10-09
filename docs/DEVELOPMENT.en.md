# Development and building

[Italiano](SVILUPPO.md) · [English](DEVELOPMENT.en.md)

Kentar — k3ntarlab ([kentar91](https://github.com/kentar91)) · [MIT](../LICENSE)

This guide covers the project sources. To install and use the app, see the [user guide](USER_GUIDE.en.md).

To prepare all verified downloads, run `./scripts/package-release.ps1 -StreamDeckCli <Elgato CLI path>`. Requires Windows, .NET Framework 4.8, Node.js and the official Elgato CLI. Complete packages and checksums are generated in `dist/release`. Publishing to GitHub remains a separate step.

After building, open `dist/app/CodexDashboard.exe` or the root `Avvia-CodexDashboard.vbs` launcher. The source launcher requires a local build; use the release for ready-to-run downloads.

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
