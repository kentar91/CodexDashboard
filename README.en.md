# Codex Dashboard

[Italiano](README.md) · [English](README.en.md)

![Codex Dashboard](assets/branding/codex-dashboard-logo-dark.png)

**Codex quotas within reach, on your desktop and Stream Deck.**

Codex Dashboard is a Windows widget and Stream Deck plugin displaying available five-hour and weekly Codex quotas. See how much usage remains and when it renews at a glance, without interrupting your work to look up these details.

[Download the current version](https://github.com/kentar91/CodexDashboard/releases/latest) · [Complete guide](docs/USER_GUIDE.en.md) · [Report a problem or request a change](https://github.com/kentar91/CodexDashboard/issues)

## Why I built this tool

I created Codex Dashboard for a practical reason: while working with Codex, I want to keep an eye on usage and know how much capacity I have left. Looking up that information repeatedly breaks my flow, especially during long sessions or when switching between several windows.

The idea is to bring this status directly to my workspace: a small widget visible while I work and, for Stream Deck users, large percentages on physical keys. Thresholds and history help track quota changes and plan work around the next renewal.

— **Kentar — k3ntarlab (GitHub: kentar91), project creator**

> **Quotas and tokens:** version 2.0.7 displays subscription quota percentages reported by Codex. It does not show exact consumed token counts, tokens for an individual conversation or API costs. Percentages are not converted into token counts.

## Features

| Feature | Purpose |
| --- | --- |
| Compact widget | Five-hour and weekly quotas always visible, with configurable size, opacity and position |
| Stream Deck keys | Large percentages, manual refresh and widget launch |
| Available or used | Choose how percentages are displayed |
| Thresholds and notifications | Set separate alerts for both quotas |
| Local history | Charts for the last 24 hours or 7 days, up to 30 days of retention and CSV export |
| Startup and closing | Manual, Windows or Codex startup; optional closing when Codex closes |
| Italian and English | Interface, installer and documentation in both languages |
| Diagnostics | Connection status, last update and JSON export |

The HUD style is inspired by the Cyberpunk 2077 palette, with original artwork and readable percentages. This is an independent project, not affiliated with OpenAI, Elgato or CD PROJEKT RED.

## Installation

**[Download the 2.0.7 installer directly](https://github.com/kentar91/CodexDashboard/releases/download/v2.0.7/CodexDashboard-Setup-2.0.7.exe)** · [All milestone downloads](https://github.com/kentar91/CodexDashboard/releases/tag/v2.0.7)

**Requirements:** Windows 10/11 x64, .NET Framework 4.8 and Codex installed and signed in with ChatGPT. The plugin requires Stream Deck 6.6 or later. API keys, Node.js and Python are not required to use the app.

1. Open the [releases page](https://github.com/kentar91/CodexDashboard/releases/latest).
2. Download `CodexDashboard-Setup-2.0.7.exe` and choose your language.
3. Select the app only, Stream Deck plugin only, or both.
4. Launch the app and open **Settings** by right-clicking the widget or using the tray icon menu.

For portable use, extract `CodexDashboard-2.0.7-windows-x64.zip` and open `CodexDashboard.exe`. To install only the plugin, open `com.codexdashboard.monitor.streamDeckPlugin` and confirm import in the Elgato app. The logo kit is available as a separate download.

The milestone executables are not digitally signed. The release includes `SHA256SUMS.txt` to verify downloaded file integrity. The `build` and `dist` folders are generated locally and are not included in the GitHub source repository.

## Getting started

The widget reads quotas through the Codex sign-in already present on your computer. Yellow identifies the five-hour quota, turquoise the weekly quota. Hover over quotas to see renewal times in Europe/Rome. Missing data appears as `—`; `!` and `OFFLINE` indicate previous data or an unavailable connection.

Settings let you select language, available or used percentages, monitor, position, thresholds and refresh frequency. **Apply** saves without closing; **Save** applies and closes; **Cancel** discards changes that have not been applied. Automatic startup and notifications are initially disabled.

In Stream Deck, add the five-hour quota, weekly quota and open-widget actions. Press a quota key to refresh. Each key can follow app settings or use its own language, display mode and threshold. The plugin also works with the widget closed.

## Data and privacy

The app reads quotas through `codex app-server`. It sends no prompts, does not read or copy authentication files, and does not spend reset credits. Settings and history stay locally in `%LOCALAPPDATA%/CodexDashboard`; uninstalling preserves them.

History records no authentication data. Diagnostic export contains settings, quotas and the last connection error, without credentials. Before attaching it to a public issue, review the information you want to share.

## Documentation

| Topic | Italiano | English |
| --- | --- | --- |
| Usage, configuration and building | [Guida utente](docs/GUIDA.md) | [User guide](docs/USER_GUIDE.en.md) |
| Source organization | [Struttura](docs/STRUTTURA.md) | [Project structure](docs/STRUCTURE.en.md) |
| Palette and visual rules | [Design](docs/DESIGN.md) | [Design guidelines](docs/DESIGN.en.md) |
| Tests and real hardware checks | [Verifiche](docs/VERIFICHE.md) | [Verification](docs/VERIFICATION.en.md) |
| Logo usage and sources | [Kit logo](assets/branding/LEGGIMI.md) | [Logo kit](assets/branding/README.en.md) |

## Frequently asked questions

**Do I need Stream Deck?** No. The widget works on its own; the plugin is optional.

**Why do I see OFFLINE?** Check that Codex is installed and signed in, that the connection is available, and try refreshing. When reporting the issue, include the app version, Windows version and diagnostic message.

**How do I update?** Download the installer from the releases page. App and plugin are supplied at the same version; plugin import requires confirmation in Elgato. Automatic online updates are not implemented yet.

**Can I see costs and exact token counts?** This milestone displays percentage quotas and quota history. API costs and token counts are not included.

## Development and requests

To build, close the widget and Settings, then run `./build.ps1` in PowerShell from the project folder. For the Stream Deck package and installer, use `./build.ps1 -Installer -StreamDeckCli <Elgato CLI path>`. See the guide for prerequisites and checks.

To request a change, open an [issue](https://github.com/kentar91/CodexDashboard/issues) describing your need, desired behavior and an example use case. For problems, include reproduction steps and the expected result. Italian and English are both welcome. Requests are reviewed by the author; every update must keep both languages aligned.

## Author and license

Created and maintained by **Kentar — k3ntarlab ([kentar91](https://github.com/kentar91))**. Codex Dashboard is distributed under the [MIT license](LICENSE), whose copyright notice must be retained in copies and distributions.
