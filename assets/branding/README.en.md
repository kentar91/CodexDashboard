# Codex Dashboard logo

[Italiano](LEGGIMI.md) · English

The original **CD** monogram follows the project's diagonal cuts and palette: yellow C `#F3E600`, turquoise D `#55EAD4`. It uses no proprietary Cyberpunk 2077 logos or typefaces.

- `codex-dashboard-symbol.svg` and `.png`: transparent symbol for materials and interfaces.
- `codex-dashboard-logo.svg` and `.png`: full wordmark on a transparent background. SVG text uses Bahnschrift/Arial; PNG preserves the typography on other devices.
- `codex-dashboard-logo-dark.png`: full wordmark on black, ready to share.
- `codex-dashboard-icon.svg` and `.png`: black panel with HUD frame, transparent outside the panel.
- `codex-dashboard-icon-N.png`: icons from 16 to 512 pixels, including Stream Deck sizes 72 and 144.
- `CodexDashboard.ico`: Windows icon with 16, 24, 32, 48, 64, 128 and 256-pixel images.
- `codex-dashboard-symbol-white` and `-black`: monochrome SVG/PNG variants for different backgrounds or printing.

Use the symbol without text below 128 pixels; preserve proportions and clear space around the logo. Prefer dark backgrounds for the color version. Do not add glow, shadows or stretching.

SVG files are editable masters. PNG exports are 1024 pixels for symbol and icon, and 1440 × 480 for the full wordmark. `scripts/export-brand.cjs` regenerates exports with Sharp; `scripts/build.ps1` uses existing exports and does not require Node.js to compile the app.
