# Logo Codex Dashboard

Il monogramma originale **CD** riprende i tagli diagonali e la palette del progetto: C gialla `#F3E600`, D turchese `#55EAD4`. Il logo non usa loghi né caratteri proprietari del gioco Cyberpunk 2077.

- `codex-dashboard-symbol.svg` e `.png`: simbolo su fondo trasparente, per materiali e interfacce.
- `codex-dashboard-logo.svg` e `.png`: logo completo con nome, su fondo trasparente. Il testo SVG usa Bahnschrift/Arial; il PNG mantiene la tipografia anche su altri dispositivi.
- `codex-dashboard-logo-dark.png`: logo completo su fondo nero, pronto da condividere.
- `codex-dashboard-icon.svg` e `.png`: icona con pannello nero e cornice HUD, trasparente fuori dal pannello.
- `codex-dashboard-icon-N.png`: icone da 16 a 512 pixel, comprese le dimensioni Stream Deck 72 e 144.
- `CodexDashboard.ico`: icona Windows con immagini da 16, 24, 32, 48, 64, 128 e 256 pixel.
- `codex-dashboard-symbol-white` e `-black`: varianti monocromatiche SVG/PNG per sfondi o stampe diversi.

Usare il simbolo senza testo sotto 128 pixel; mantenere le proporzioni e uno spazio libero intorno al marchio. Preferire fondi scuri per la versione a colori. Non aggiungere effetti di bagliore, ombre o allungamenti.

I file SVG sono i master modificabili. I PNG sono esportati a 1024 pixel per simbolo e icona, a 1440 × 480 per il logo completo. `scripts/export-brand.cjs` rigenera gli export con Sharp; `scripts/build.ps1` usa gli asset già esportati e non richiede Node.js per compilare il programma.
