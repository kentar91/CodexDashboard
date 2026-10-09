# Night City HUD

[Italiano](DESIGN.md) · English · [Main documentation](../README.en.md)

The entire project uses a visual language inspired by Cyberpunk 2077: widget, Stream Deck keys, app icon, tray, menus and tooltips. New screens must follow these choices too.

The official logo is the original CD monogram in `assets/branding`: yellow C, turquoise D and angular shapes. Use the HUD panel version for Windows and Stream Deck icons, the transparent symbol for other interfaces and the full wordmark for documents and materials. SVG masters and PNG/ICO variants are in the same folder; do not generate alternative text-based CD icons during building.

The shared palette in `assets/theme.json` matches the user's reference: black `#000000`, red `#C5003C`, burgundy `#880425`, yellow `#F3E600` and turquoise `#55EAD4`. Yellow identifies the short quota and opening action; turquoise identifies the weekly quota. Red indicates missing connection or low quota; burgundy defines bar tracks, borders and selection backgrounds. Use this palette for every new project interface.

Use dark panels, cut corners, thin asymmetric lines, crisp bars and small HUD accents. Avoid rounded corners, gradients and glows that blur text. Percentages must remain the largest element, especially on physical keys.

Stream Deck keys directly follow the widget: the top border is always yellow, including weekly and low-quota states; the lower-left detail and side lines are turquoise, with thin bars. Labels retain the window colors; only the percentage and bar turn red at the threshold. The opening key shows CODEX DASHBOARD and APRI (OPEN in English), without a dummy quota bar.

The main font is Bahnschrift. Stream Deck keys are 144 × 144 PNG images: text is measured, fitted to margins and centered before sending. Labels are short and uppercase; explanatory text must remain readable in the selected language. Preserve the enlarged Stream Deck key sizes. Do not use proprietary game logos or fonts; use system font fallbacks.

`build.ps1` generates C# colors and the WPF layout from the shared palette and embeds official logo exports. Theme changes apply on the next build.
