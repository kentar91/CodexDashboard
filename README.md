# Codex Dashboard

Widget Windows e plugin Stream Deck per leggere le quote Codex. Tema Cyberpunk 2077, logo CD, colori condivisi, configuratore integrato.

## Avvio rapido

- Doppio clic su **Avvia-CodexDashboard.vbs** nella cartella principale, oppure su **dist/app/CodexDashboard.exe**.
- Per configurare: tasto destro sul widget → **Impostazioni**, menu dell'icona vicino all'orologio oppure **dist/app/Impostazioni-CodexDashboard.vbs**.
- Per Stream Deck: installa **dist/streamdeck/com.codexdashboard.monitor.streamDeckPlugin** con doppio clic.
- Per il logo: **dist/branding/CodexDashboard-logo-kit.zip**. I file modificabili sono in **assets/branding**.
- Per installare o aggiornare tutto: **dist/installer/CodexDashboard-Setup-2.0.1.exe**. Scegli Italiano o English; l'installazione è per il tuo utente, con collegamenti e voce di disinstallazione. Il pacchetto Stream Deck viene aperto nell'app Elgato per confermare l'importazione.

Serve Windows 10/11 x64 con .NET Framework 4.8 e Codex installato con accesso ChatGPT già effettuato. Per usare il programma non servono Node.js, Python o chiavi API. Il plugin richiede Stream Deck 6.6 o superiore e funziona anche con il widget chiuso.

## Configuratore

Puoi impostare dimensione dal 80% al 200%, opacità a riposo dal 40% al 100%, primo piano, posizione, avvio manuale/con Windows/quando apri Codex, aggiornamento ogni 30/60/120/300 secondi, soglia di quota bassa e notifiche facoltative. La scelta quota disponibile/consumata e la frequenza si applicano anche al plugin aggiornato.

**Applica** salva e applica subito senza chiudere. **Salva** applica e chiude. **Annulla** scarta solo le modifiche successive all'ultimo Applica. **Lingua** sceglie Italiano o English per widget, menu, impostazioni e tasti. **Ripristina** carica i valori predefiniti da confermare con Applica o Salva. **Centra widget** seleziona il centro dello schermo da confermare con Applica o Salva. Avvio automatico e notifiche sono disattivati inizialmente. L'avvio automatico si applica al prossimo accesso a Windows; nella modalità Codex un monitor nell'area di notifica mostra/nasconde il widget seguendo l'app desktop.

**Schermo del widget** sceglie il monitor. **Posizione del widget** offre nove posizioni (angoli, centro dei bordi e centro schermo) oppure **Personalizzata (trascina)**. **Distanza dai bordi** imposta il margine da 0 a 120 pixel; la barra delle applicazioni viene esclusa dall'area disponibile. La scelta resta salvata e viene applicata anche al riavvio o al cambio di dimensione del widget. Se il monitor non è disponibile, il widget usa quello principale. Trascinando il widget passi automaticamente alla posizione personalizzata. La finestra delle impostazioni scorre sui display più piccoli.

Le impostazioni e la posizione sono salvate in `%LOCALAPPDATA%/CodexDashboard`, condivise con il plugin. La riorganizzazione del progetto non modifica questi dati personali.

## Widget e tasti

Il widget è trascinabile, largo 220 × 64 pixel alla dimensione predefinita. Mostra titolo completo e percentuali; i pulsanti Aggiorna e Chiudi compaiono al passaggio del mouse. Una seconda apertura mostra l'istanza esistente. I suggerimenti sulle quote mostrano il rinnovo in Europe/Rome.

In Stream Deck trascina sui tasti le azioni **Quota 5 ore**, **Quota settimana** e **Apri widget**. Premi le quote per aggiornare, oppure il tasto **CODEX DASHBOARD / APRI** per aprire il programma. Le quote sono identificate per durata, 300 e 10080 minuti; durate mancanti sono segnalate. Sono supportati i pulsanti, non le manopole Stream Deck+.

Seleziona un tasto nell'app Stream Deck per personalizzare lingua, percentuale disponibile/consumata e soglia rossa. **Impostazioni app** segue la configurazione globale. Le modifiche per tasto sono salvate da Stream Deck.

Il menu del widget e dell'icona vicino all'orologio offre **Storico quote** e **Informazioni e diagnostica**: grafico delle ultime 24 ore/7 giorni, esportazione CSV, versione, piano dell'account, ultimo aggiornamento, stato offline ed esportazione JSON. Lo storico conserva fino a 30 giorni e non registra dati di autenticazione. L'esportazione diagnostica contiene impostazioni, quote e ultimo errore di connessione, senza credenziali.

**Installa aggiornamento** apre un installatore completo scaricato dall'utente. Il pacchetto porta app e plugin alla stessa versione; l'importazione in Stream Deck richiede la conferma nell'app Elgato. Non è ancora disponibile un canale pubblico per gli aggiornamenti automatici online. La disinstallazione conserva impostazioni e storico; il plugin installato in Elgato si rimuove dall'app Stream Deck.

Il giallo identifica la quota breve; il turchese la settimana. Percentuali e barre diventano rosse alla soglia impostata. `!` e `OFFLINE` indicano dati precedenti o collegamento mancante; `—` indica un dato non disponibile. Le notifiche non si ripetono finché la quota non risale e poi scende di nuovo. Riconnessione dopo 15 secondi e timeout di lettura dopo 25 secondi.

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
| `archive` | Prototipi e layout storici conservati |

Dettagli in [docs/STRUTTURA.md](docs/STRUTTURA.md), stile in [docs/DESIGN.md](docs/DESIGN.md), palette in [assets/theme.json](assets/theme.json).

## Compilazione e pacchetti

Chiudi il widget e l'eventuale configuratore. Esegui **build.ps1** dalla cartella principale: richiama `scripts/build.ps1` e ricrea programma, plugin compilato e ZIP del logo. Il compilatore .NET è quello di Windows; gli asset grafici esportati sono già inclusi nel progetto.

Per rigenerare anche l'installatore Stream Deck usa `build.ps1 -Package`, con la CLI ufficiale Elgato disponibile, oppure `build.ps1 -Package -StreamDeckCli <percorso CLI>`. Il manifest sorgente è `src/streamdeck/manifest.json`; non modificare la copia generata in `build/streamdeck`.

`build.ps1 -Installer -StreamDeckCli <percorso CLI>` genera anche l'installatore Windows bilingue che include programma, icona e pacchetto Stream Deck della stessa compilazione.

I sorgenti del logo sono in `assets/branding`; `scripts/export-brand.cjs` rigenera PNG e ICO con Sharp. Il kit contiene simbolo trasparente, firma completa, icona HUD, varianti monocromatiche e icone Windows da 16 a 256 pixel.

## Verifiche

Per salvare le verifiche dentro il progetto imposta `CODEXDASHBOARD_ARTIFACT_DIR` a `build/reports` o `build/previews` usando un percorso assoluto. Altrimenti vengono salvate in `%LOCALAPPDATA%/CodexDashboard/diagnostics`.

Il programma espone `--self-test`, `--settings-test`, `--verify`, `--preview`, `--config-preview` e `--deck-preview`. Verificano conversione quote, impostazioni, ridimensionamento, avvisi, collegamento reale e anteprime. Il test delle impostazioni usa una cartella separata senza cambiare il tuo avvio Windows.

`tests/streamdeck-smoke.cjs` simula il collegamento Stream Deck, controlla registrazione, tre immagini PNG 144 × 144 e comando di aggiornamento. I risultati sono in `build/reports`.

`tests/release-smoke.ps1` controlla anche Applica/Salva/Annulla, lingua, modalità di avvio, ingrandimenti, storico, impostazioni per tasto e corrispondenza dei file inclusi nell'installatore. I controlli che richiedono dispositivi e postazioni reali sono in [docs/VERIFICHE.md](docs/VERIFICHE.md). Le etichette della lista azioni inglese seguono il formato di [localizzazione Elgato](https://docs.elgato.com/streamdeck/sdk/guides/i18n/); la lingua dei tasti e del loro configuratore segue le impostazioni dell'app o la scelta per tasto.

## Dati

Si usa `codex app-server --listen stdio://`, handshake initialize/initialized, lettura `account/rateLimits/read` e notifiche `account/rateLimits/updated`. Il bucket `codex` ha precedenza sul risultato legacy. Il programma non invia prompt, non legge o copia i file di autenticazione e non consuma crediti di reset. Il plugin comunica con Stream Deck sul WebSocket locale.

Questa versione mostra le quote dell'abbonamento e il loro storico; non include costi o storico token. I dati provengono dalla lettura periodica e dagli aggiornamenti Codex.

Documentazione: [Codex App Server](https://learn.chatgpt.com/docs/app-server), [manifest Stream Deck](https://docs.elgato.com/streamdeck/sdk/references/manifest/), [WebSocket Stream Deck](https://docs.elgato.com/streamdeck/sdk/references/websocket/plugin/).
