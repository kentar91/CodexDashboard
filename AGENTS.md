# Indicazioni del progetto

L'utente ha richiesto che tutto CodexDashboard si basi sui colori e sullo stile di Cyberpunk 2077. Per le modifiche visive seguire `docs/DESIGN.md` e la palette condivisa in `assets/theme.json`, comprese nuove finestre, icone, menu e azioni Stream Deck.

Preservare le percentuali grandi sui tasti fisici. Lo stile HUD non deve ridurre la leggibilità o coprire i valori. Non usare loghi o font proprietari del gioco: il progetto usa grafica originale e Bahnschrift con fallback di sistema.

Le interfacce correnti sono in `src`; il manifest originale Stream Deck è `src/streamdeck/manifest.json`. I prototipi storici sono in `archive/prototypes`, non sono destinazioni per nuove funzionalità. Compilare con `build.ps1`, che richiama `scripts/build.ps1`; validare il pacchetto Stream Deck con la CLI Elgato. Sorgenti generati e plugin compilato vanno in `build`, i prodotti pronti in `dist/app`, `dist/streamdeck` e `dist/branding`. Dopo modifiche visive verificare le anteprime del widget e dei tasti. Per i controlli impostare CODEXDASHBOARD_ARTIFACT_DIR alla cartella build/reports o build/previews.
