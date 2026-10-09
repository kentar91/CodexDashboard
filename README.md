# Codex Dashboard

[Italiano](README.md) · [English](README.en.md)

![Codex Dashboard](assets/branding/codex-dashboard-logo-dark.png)

**Le quote Codex sempre a portata di mano, sul desktop e su Stream Deck.**

Codex Dashboard è un widget per Windows e un plugin Stream Deck che mostrano la disponibilità delle quote Codex su cinque ore e settimana. Un colpo d'occhio permette di controllare quanto utilizzo rimane e quando si rinnova, senza interrompere il lavoro per cercare queste informazioni.

[Scarica la versione corrente](https://github.com/kentar91/CodexDashboard/releases/latest) · [Guida completa](docs/GUIDA.md) · [Segnala un problema o proponi una modifica](https://github.com/kentar91/CodexDashboard/issues)

## Perché ho creato questo strumento

Ho creato Codex Dashboard per un'esigenza pratica: mentre lavoro con Codex, voglio avere sempre sott'occhio lo stato dell'utilizzo e sapere quanto margine mi rimane. Cercare queste informazioni ogni volta spezza il ritmo, soprattutto durante sessioni lunghe o quando passo fra più finestre.

L'idea è portare questo stato direttamente sulla scrivania: un piccolo widget visibile mentre lavoro e, per chi usa Stream Deck, percentuali grandi sui tasti fisici. Le soglie e lo storico aiutano a seguire l'andamento delle quote e a organizzare il lavoro in vista del prossimo rinnovo.

— **Marco (kentar91), creatore del progetto**

> **Quote e token:** la versione 2.0.7 visualizza le percentuali delle quote dell'abbonamento riportate da Codex. Non mostra il numero esatto di token consumati, i token di una singola conversazione o i costi API. Le percentuali non sono convertite in un conteggio di token.

## Cosa offre

| Funzione | Utilità |
| --- | --- |
| Widget compatto | Quote su cinque ore e settimana sempre visibili, con dimensione, opacità e posizione configurabili |
| Tasti Stream Deck | Percentuali grandi, aggiornamento manuale e apertura del widget |
| Disponibile o consumato | Scegli come leggere le percentuali |
| Soglie e notifiche | Imposta avvisi separati per le due quote |
| Storico locale | Grafici delle ultime 24 ore o 7 giorni, conservazione fino a 30 giorni ed esportazione CSV |
| Avvio e chiusura | Avvio manuale, con Windows o con Codex; chiusura facoltativa quando Codex si chiude |
| Italiano e inglese | Interfaccia, installer e documentazione in entrambe le lingue |
| Diagnostica | Stato della connessione, ultimo aggiornamento ed esportazione JSON |

Lo stile HUD è ispirato alla palette di Cyberpunk 2077, con grafica originale e percentuali leggibili. Il progetto è indipendente e non è affiliato a OpenAI, Elgato o CD PROJEKT RED.

## Installazione

**Requisiti:** Windows 10/11 x64, .NET Framework 4.8 e Codex installato con accesso ChatGPT già effettuato. Per il plugin serve Stream Deck 6.6 o superiore. Non servono chiavi API, Node.js o Python per usare l'app.

1. Apri la [pagina delle release](https://github.com/kentar91/CodexDashboard/releases/latest).
2. Scarica `CodexDashboard-Setup-2.0.7.exe` e scegli la lingua.
3. Seleziona **Solo programma**, **Solo plugin Stream Deck** oppure entrambi.
4. Avvia il programma e apri **Impostazioni** dal tasto destro sul widget o dal menu dell'icona vicino all'orologio.

Per l'uso portatile, estrai `CodexDashboard-2.0.7-windows-x64.zip` e apri `CodexDashboard.exe`. Per installare soltanto il plugin, apri `com.codexdashboard.monitor.streamDeckPlugin` e conferma l'importazione nell'app Elgato. Il kit logo è scaricabile separatamente.

Gli eseguibili della milestone non sono firmati digitalmente. La release include `SHA256SUMS.txt` per verificare l'integrità dei file scaricati. Le cartelle `build` e `dist` sono generate localmente e non sono incluse nei sorgenti su GitHub.

## Primo utilizzo

Il widget legge le quote dall'accesso Codex già presente sul computer. Il giallo identifica la quota su cinque ore, il turchese quella settimanale. Passa sulle quote con il mouse per vedere il rinnovo in Europe/Rome. Se mancano dati, compare `—`; `!` e `OFFLINE` segnalano dati precedenti o un collegamento non disponibile.

Nelle impostazioni puoi scegliere lingua, percentuali disponibili o consumate, monitor, posizione, soglie e frequenza di aggiornamento. **Applica** salva senza chiudere; **Salva** applica e chiude; **Annulla** scarta le modifiche non applicate. Avvio automatico e notifiche sono disattivati inizialmente.

In Stream Deck aggiungi **Quota 5 ore**, **Quota settimana** e **Apri widget**. Premi un tasto quota per aggiornare. Ogni tasto può seguire le impostazioni dell'app oppure avere lingua, modalità e soglia personalizzate. Il plugin funziona anche con il widget chiuso.

## Dati e riservatezza

Il programma legge le quote attraverso `codex app-server`. Non invia prompt, non legge o copia i file di autenticazione e non consuma crediti di reset. Impostazioni e storico sono locali, in `%LOCALAPPDATA%/CodexDashboard`; la disinstallazione li conserva.

Lo storico non registra dati di autenticazione. L'esportazione diagnostica contiene impostazioni, quote e ultimo errore di connessione, senza credenziali. Prima di allegarla a una issue pubblica, controlla comunque le informazioni che desideri condividere.

## Documentazione

| Argomento | Italiano | English |
| --- | --- | --- |
| Uso, configurazione e compilazione | [Guida utente](docs/GUIDA.md) | [User guide](docs/USER_GUIDE.en.md) |
| Organizzazione dei sorgenti | [Struttura](docs/STRUTTURA.md) | [Project structure](docs/STRUCTURE.en.md) |
| Palette e regole visive | [Design](docs/DESIGN.md) | [Design guidelines](docs/DESIGN.en.md) |
| Test e controlli su hardware reale | [Verifiche](docs/VERIFICHE.md) | [Verification](docs/VERIFICATION.en.md) |
| Uso e sorgenti del logo | [Kit logo](assets/branding/LEGGIMI.md) | [Logo kit](assets/branding/README.en.md) |

## Domande frequenti

**Serve Stream Deck?** No. Il widget funziona da solo; il plugin è un componente facoltativo.

**Perché vedo OFFLINE?** Controlla che Codex sia installato e autenticato, che la connessione sia disponibile e prova ad aggiornare. Per segnalare il problema, indica versione dell'app, versione di Windows e messaggio nella diagnostica.

**Come aggiorno?** Scarica l'installer dalla pagina delle release. App e plugin vengono forniti nella stessa versione; l'importazione del plugin richiede conferma in Elgato. Gli aggiornamenti automatici online non sono ancora implementati.

**Posso vedere costi e token esatti?** Questa milestone mostra quote percentuali e storico delle quote. Costi API e conteggi dei token non sono inclusi.

## Sviluppo e richieste

Per compilare, chiudi widget e configuratore ed esegui `./build.ps1` in PowerShell dalla cartella del progetto. Per pacchetto Stream Deck e installer usa `./build.ps1 -Installer -StreamDeckCli <percorso CLI Elgato>`. Consulta la guida per prerequisiti e verifiche.

Per chiedere una modifica, apri una [issue](https://github.com/kentar91/CodexDashboard/issues) descrivendo l'esigenza, il comportamento desiderato e un esempio d'uso. Per i problemi, aggiungi i passaggi per riprodurli e il risultato atteso. Puoi scrivere in italiano o inglese. Le richieste vengono valutate dall'autore; ogni aggiornamento deve mantenere allineate entrambe le lingue.

## Autore e licenza

Creato e mantenuto da **Marco ([kentar91](https://github.com/kentar91))**. Codex Dashboard è distribuito con [licenza MIT](LICENSE), con avviso di copyright da conservare nelle copie e nelle distribuzioni.