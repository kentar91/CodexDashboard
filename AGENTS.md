# Indicazioni del progetto

## Italiano e inglese

Ogni aggiornamento del software deve mantenere il supporto completo a italiano e inglese. Quando cambiano funzionalità, comportamenti o testi, aggiornare insieme entrambe le lingue nell'interfaccia (widget, menu, configuratore, diagnostica, installer e Stream Deck), nella documentazione e nei materiali distribuiti. Conservare i collegamenti fra le versioni linguistiche e includere le guide inglesi nei prodotti generati. Per le parti interessate verificare entrambe le lingue, comprese leggibilità, layout e corrispondenza delle istruzioni al comportamento corrente. La doppia lingua è un requisito di ogni aggiornamento, non un'attività da rimandare a una versione successiva.

L'utente ha richiesto che tutto CodexDashboard si basi sui colori e sullo stile di Cyberpunk 2077. Per le modifiche visive seguire `docs/DESIGN.md` e la palette condivisa in `assets/theme.json`, comprese nuove finestre, icone, menu e azioni Stream Deck.

Preservare le percentuali grandi sui tasti fisici. Lo stile HUD non deve ridurre la leggibilità o coprire i valori. Non usare loghi o font proprietari del gioco: il progetto usa grafica originale e Bahnschrift con fallback di sistema.

Le interfacce correnti sono in `src`; il manifest originale Stream Deck è `src/streamdeck/manifest.json`. Compilare con `build.ps1`, che richiama `scripts/build.ps1`; validare il pacchetto Stream Deck con la CLI Elgato. Sorgenti generati e plugin compilato vanno in `build`, i prodotti pronti in `dist/app`, `dist/streamdeck` e `dist/branding`. Dopo modifiche visive verificare le anteprime del widget e dei tasti. Per i controlli impostare CODEXDASHBOARD_ARTIFACT_DIR alla cartella build/reports o build/previews.
