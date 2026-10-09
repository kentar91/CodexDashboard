# Struttura del progetto

[Italiano](STRUTTURA.md) · [English](STRUCTURE.en.md)

| Cartella | Contenuto |
| --- | --- |
| `src/` | Programma Windows, configuratore e layout WPF |
| `src/streamdeck/` | Manifest sorgente del plugin |
| `assets/` | Tema condiviso e kit grafico del logo |
| `docs/` | Regole visive e documentazione del progetto |
| `scripts/` | Compilazione, esportazione logo e modelli dei file di avvio |
| `tests/` | Verifiche automatiche |
| `build/generated/` | C# e XAML generati dalla compilazione |
| `build/streamdeck/` | Plugin compilato, pronto da confezionare |
| `build/reports/` | Risultati e dati dei test |
| `build/previews/` | Anteprime visive |
| `build/tools/` | Dipendenze locali della CLI Elgato per validare e confezionare il plugin |
| `dist/app/` | Programma Windows e avvio/configuratore |
| `dist/streamdeck/` | Pacchetto installabile Stream Deck |
| `dist/branding/` | Kit logo ZIP pronto da condividere |
| `dist/installer/` | Installatore Windows bilingue con app e plugin |
| `dist/release/` | Tutti gli asset pubblicabili e checksum SHA-256 |
| `docs/screenshots/` | Anteprime del prodotto in italiano e inglese |

Nella cartella principale rimangono README, indicazioni di progetto, comando di compilazione e collegamento di avvio. Non modificare manualmente `build/generated` o il plugin compilato: lavorare sui sorgenti in `src` e `assets`.

`build` e `dist` sono esclusi dal controllo versione perché riproducibili. La compilazione ricrea programma, plugin compilato e kit logo. Per ricreare anche il pacchetto installabile usare `build.ps1 -Package`, con la CLI Elgato disponibile, oppure passare il percorso della CLI con `-StreamDeckCli`.

In `dist/installer` mantenere l'installatore della versione corrente. Le vecchie versioni e i backup `CodexDashboard-before-*.exe` nei report non sono necessari per la compilazione. La CLI locale può essere conservata in `build/tools` (qui in `build/tools/node_modules/.bin`); gli archivi di download e la cache npm usati per installarla possono essere rimossi.

Le impostazioni personali rimangono in `%LOCALAPPDATA%/CodexDashboard` e non vengono spostate. In esecuzione normale, i log e gli artefatti diagnostici sono salvati nella sua sottocartella `diagnostics`. Per i test di sviluppo, `CODEXDASHBOARD_ARTIFACT_DIR` permette di scegliere `build/reports` o `build/previews`.

Per installer Windows e set completo dei download consulta la [guida di sviluppo](SVILUPPO.md). `-Package` crea il solo pacchetto Stream Deck; `-Installer` crea anche il setup Windows.
