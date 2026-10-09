# Sviluppo e compilazione

[Italiano](SVILUPPO.md) · [English](DEVELOPMENT.en.md)

Kentar — k3ntarlab ([kentar91](https://github.com/kentar91)) · [MIT](../LICENSE)

Questa guida riguarda i sorgenti del progetto. Per installare e usare il programma consulta la [guida utente](GUIDA.md).

Per preparare tutti i download verificati usa `./scripts/package-release.ps1 -StreamDeckCli <percorso CLI Elgato>`. Richiede Windows, .NET Framework 4.8, Node.js e la CLI ufficiale Elgato. I pacchetti completi e i checksum vengono generati in `dist/release`. La pubblicazione su GitHub resta un passaggio separato.

Dopo la compilazione puoi avviare `dist/app/CodexDashboard.exe` oppure il launcher `Avvia-CodexDashboard.vbs` nella cartella principale. Il launcher dei sorgenti richiede una build locale; per il download pronto usa la release.

## Cartelle

| Cartella | Contenuto |
| --- | --- |
| `src` | Programma, configuratore, layout e manifest Stream Deck |
| `assets` | Tema e logo ufficiale SVG/PNG/ICO |
| `docs` | Documentazione e regole visive |
| `scripts` | Compilazione, export logo e modelli dei lanciatori |
| `tests` | Verifiche automatiche |
| `build` | Plugin compilato, sorgenti generati, anteprime e risultati dei test |
| `dist` | Programma, plugin installabile e kit logo pronti all'uso |

Dettagli in [docs/STRUTTURA.md](STRUTTURA.md), stile in [docs/DESIGN.md](DESIGN.md), palette in [assets/theme.json](../assets/theme.json).

## Compilazione e pacchetti

Chiudi il widget e l'eventuale configuratore. Esegui **build.ps1** dalla cartella principale: richiama `scripts/build.ps1` e ricrea programma, plugin compilato e ZIP del logo. Il compilatore .NET è quello di Windows; gli asset grafici esportati sono già inclusi nel progetto.

Per rigenerare anche l'installatore Stream Deck usa `build.ps1 -Package`, con la CLI ufficiale Elgato disponibile, oppure `build.ps1 -Package -StreamDeckCli <percorso CLI>`. Il manifest sorgente è `src/streamdeck/manifest.json`; non modificare la copia generata in `build/streamdeck`.

`build.ps1 -Installer -StreamDeckCli <percorso CLI>` genera anche l'installatore Windows bilingue che include programma, icona e pacchetto Stream Deck della stessa compilazione.

I sorgenti del logo sono in `assets/branding`; `scripts/export-brand.cjs` rigenera PNG e ICO con Sharp. Il kit contiene simbolo trasparente, firma completa, icona HUD, varianti monocromatiche e icone Windows da 16 a 256 pixel.

## Verifiche

Per salvare le verifiche dentro il progetto imposta `CODEXDASHBOARD_ARTIFACT_DIR` a `build/reports` o `build/previews` usando un percorso assoluto. Altrimenti vengono salvate in `%LOCALAPPDATA%/CodexDashboard/diagnostics`.

Il programma espone `--self-test`, `--settings-test`, `--verify`, `--preview`, `--config-preview` e `--deck-preview`. Verificano conversione quote, impostazioni, ridimensionamento, avvisi, collegamento reale e anteprime. Il test delle impostazioni usa una cartella separata senza cambiare il tuo avvio Windows.

`tests/streamdeck-smoke.cjs` simula il collegamento Stream Deck, controlla registrazione, tre immagini PNG 144 × 144 e comando di aggiornamento. I risultati sono in `build/reports`.

`tests/release-smoke.ps1` controlla anche Applica/Salva/Annulla, lingua, modalità di avvio, ingrandimenti, storico, impostazioni per tasto e corrispondenza dei file inclusi nell'installatore. I controlli che richiedono dispositivi e postazioni reali sono in [docs/VERIFICHE.md](VERIFICHE.md). Le etichette della lista azioni inglese seguono il formato di [localizzazione Elgato](https://docs.elgato.com/streamdeck/sdk/guides/i18n/); la lingua dei tasti e del loro configuratore segue le impostazioni dell'app o la scelta per tasto.

## Dati

Si usa `codex app-server --listen stdio://`, handshake initialize/initialized, lettura `account/rateLimits/read` e notifiche `account/rateLimits/updated`. Il bucket `codex` ha precedenza sul risultato legacy. Il programma non invia prompt, non legge o copia i file di autenticazione e non consuma crediti di reset. Il plugin comunica con Stream Deck sul WebSocket locale.

Questa versione mostra le quote dell'abbonamento e il loro storico; non include costi o storico token. I dati provengono dalla lettura periodica e dagli aggiornamenti Codex.

Documentazione: [Codex App Server](https://learn.chatgpt.com/docs/app-server), [manifest Stream Deck](https://docs.elgato.com/streamdeck/sdk/references/manifest/), [WebSocket Stream Deck](https://docs.elgato.com/streamdeck/sdk/references/websocket/plugin/).
