# Project structure

[Italiano](STRUTTURA.md) · English · [Main documentation](../README.en.md)

| Folder | Contents |
| --- | --- |
| `src/` | Windows app, settings and WPF layout |
| `src/streamdeck/` | Plugin source manifest |
| `assets/` | Shared theme and logo assets |
| `docs/` | Visual guidelines and project documentation |
| `scripts/` | Build, logo export and launcher templates |
| `tests/` | Automated checks |
| `build/generated/` | Generated C# and XAML |
| `build/streamdeck/` | Compiled plugin ready for packaging |
| `build/reports/` | Test results and data |
| `build/previews/` | Visual previews |
| `build/tools/` | Local Elgato CLI dependencies for validation and packaging |
| `dist/app/` | Windows app and launch/settings shortcuts |
| `dist/streamdeck/` | Installable Stream Deck package |
| `dist/branding/` | Logo ZIP ready to share |
| `dist/installer/` | Bilingual Windows installer with app and plugin |

The project root contains the README files, project instructions, build entry point and launcher. Do not manually edit `build/generated` or the compiled plugin: work on sources in `src` and `assets`.

`build` and `dist` are excluded from version control because they can be reproduced. Building recreates the app, compiled plugin and logo kit. To recreate the installation package, use `build.ps1 -Package` with the Elgato CLI available, or pass its path with `-StreamDeckCli`.

Keep the current installer in `dist/installer`. Previous versions and `CodexDashboard-before-*.exe` backups in reports are not required for building. Keep the local CLI in `build/tools/elgato`; download archives and npm cache used to install it can be removed.

Personal settings remain in `%LOCALAPPDATA%/CodexDashboard` and are not moved. During normal operation, logs and diagnostic artifacts go to its `diagnostics` subfolder. For development tests, `CODEXDASHBOARD_ARTIFACT_DIR` selects `build/reports` or `build/previews`.
