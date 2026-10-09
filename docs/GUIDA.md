# Guida utente

[Italiano](GUIDA.md) · [English](USER_GUIDE.en.md) · [Pagina del progetto](../README.md)

Widget Windows e plugin Stream Deck per leggere le quote Codex. Tema Cyberpunk 2077, logo CD, colori condivisi, configuratore integrato.

Creato da **Kentar — k3ntarlab (GitHub: kentar91)**. Distribuito con [licenza MIT](../LICENSE). Per proporre una modifica o segnalare un problema, apri una [issue su GitHub](https://github.com/kentar91/CodexDashboard/issues). Progetto indipendente, non affiliato a OpenAI, Elgato o CD PROJEKT RED.

## Installazione e primo avvio

Serve Windows 10/11 x64 con .NET Framework 4.8 e Codex installato con accesso ChatGPT già effettuato. Non servono chiavi API, Node.js o Python per usare il programma. Stream Deck è facoltativo; il plugin richiede l'app Elgato 6.6 o superiore.

1. Scarica [CodexDashboard-Setup-2.0.7.exe](https://github.com/kentar91/CodexDashboard/releases/download/v2.0.7/CodexDashboard-Setup-2.0.7.exe).
2. Scegli Italiano o English, poi **Solo programma**, **Solo plugin Stream Deck** oppure entrambi.
3. Per il programma, avvia **Codex Dashboard** dal collegamento creato dall'installer. Per il plugin, conferma l'importazione nell'app Elgato.
4. Apri **Impostazioni** dal tasto destro sul widget o dal menu dell'icona vicino all'orologio.

L'installazione è per il tuo utente. Un componente già presente e non selezionato viene conservato. Se installi solo il plugin, non viene creato un collegamento per il programma.

### Uso portatile e solo plugin

Per l'uso portatile, scarica lo [ZIP Windows x64](https://github.com/kentar91/CodexDashboard/releases/download/v2.0.7/CodexDashboard-2.0.7-windows-x64.zip), estrailo completamente in una cartella e apri `CodexDashboard.exe`. I lanciatori `.vbs` nella cartella estratta sono facoltativi. Lo ZIP non installa automaticamente il plugin Stream Deck.

Per usare soltanto Stream Deck, scarica il [pacchetto plugin](https://github.com/kentar91/CodexDashboard/releases/download/v2.0.7/com.codexdashboard.monitor.streamDeckPlugin), aprilo con doppio clic e conferma l'importazione in Elgato. Il plugin funziona anche senza il widget aperto.

### Aggiornamento e rimozione

Per aggiornare, scarica ed esegui il setup della release corrente; per Stream Deck conferma la nuova importazione. Gli aggiornamenti online automatici non sono implementati. Per rimuovere il programma usa la disinstallazione di Windows; per il plugin usa l'app Stream Deck. Impostazioni e storico personali vengono conservati.

Gli eseguibili non sono firmati digitalmente. I [checksum SHA-256](https://github.com/kentar91/CodexDashboard/releases/download/v2.0.7/SHA256SUMS.txt) consentono di verificare l'integrità dei download. **Download ZIP** e **Download source code** sono diversi: gli archivi dei sorgenti generati da GitHub richiedono compilazione. I percorsi `build` e `dist` riguardano gli sviluppatori, descritti nella [guida di sviluppo](SVILUPPO.md).

## Configuratore

Puoi impostare dimensione dal 80% al 200%, opacità a riposo dal 40% al 100%, primo piano, posizione, avvio manuale/con Windows/quando apri Codex, aggiornamento ogni 30/60/120/300 secondi, soglie separate per quota 5 ore e settimana e notifiche facoltative. La scelta quota disponibile/consumata e la frequenza si applicano anche al plugin aggiornato.

**Applica** salva e applica subito senza chiudere. **Salva** applica e chiude. **Annulla** scarta solo le modifiche successive all'ultimo Applica. **Lingua** sceglie Italiano o English per widget, menu, impostazioni e tasti. **Ripristina** carica i valori predefiniti da confermare con Applica o Salva. **Centra widget** seleziona il centro dello schermo da confermare con Applica o Salva. Avvio automatico e notifiche sono disattivati inizialmente. La scelta di avvio si applica subito al programma aperto e viene registrata per i successivi accessi a Windows; nella modalità Codex un monitor nell'area di notifica apre il widget quando avvii l'app desktop. **Chiusura widget** sceglie **Solo manualmente** (predefinito) oppure **Quando chiudi Codex**, indipendentemente dalla modalità di avvio. La chiusura automatica interviene dopo aver rilevato una finestra di Codex aperta e poi chiusa (i processi residui in background non contano), entro circa tre secondi. Con avvio quando apri Codex e chiusura quando chiudi Codex, il programma termina completamente (widget, icona e processo). Un’attività di Windows per il tuo utente controlla l’apertura ogni 10 secondi e avvia una nuova istanza soltanto quando Codex è aperto; la comparsa può richiedere circa 10 secondi. L’attività viene creata con Applica/Salva, aggiornata dall’installer e rimossa scegliendo un’altra modalità o disinstallando. Con chiusura manuale il monitor può invece restare attivo in background. La X nasconde il widget nella modalità di avvio Codex; Esci termina il programma, che potrà ripartire alla successiva verifica se Codex resta aperto e l’avvio automatico è abilitato.

**Schermo del widget** sceglie il monitor. **Posizione del widget** offre nove posizioni (angoli, centro dei bordi e centro schermo) oppure **Personalizzata (trascina)**. **Distanza dai bordi** imposta il margine da 0 a 120 pixel; la barra delle applicazioni viene esclusa dall'area disponibile. La scelta resta salvata e viene applicata anche al riavvio o al cambio di dimensione del widget. Se il monitor non è disponibile, il widget usa quello principale. Trascinando il widget passi automaticamente alla posizione personalizzata. La finestra delle impostazioni scorre sui display più piccoli; Ripristina, Annulla, Applica e Salva restano sempre visibili in basso. Lo stato dell’avvio Windows e il comportamento della X sono indicati nelle impostazioni.

Le impostazioni e la posizione sono salvate in `%LOCALAPPDATA%/CodexDashboard`, condivise con il plugin. La riorganizzazione del progetto non modifica questi dati personali.

## Widget e tasti

Il widget è trascinabile, largo 220 × 64 pixel alla dimensione predefinita. Mostra titolo completo e percentuali; i pulsanti Aggiorna e Chiudi compaiono al passaggio del mouse. Una seconda apertura mostra l'istanza esistente. I suggerimenti sul titolo e sulle quote mostrano anche il numero di reset disponibili, letto da Codex a ogni aggiornamento. Se il dato manca viene indicato come non disponibile; se la connessione è persa, il numero precedente è contrassegnato come ultimo dato ricevuto. Il suggerimento mostra anche la prossima scadenza fra i reset disponibili. I suggerimenti sulle quote mostrano il rinnovo in Europe/Rome.

In Stream Deck trascina sui tasti le azioni **Quota 5 ore**, **Quota settimana** e **Apri widget**. Premi le quote per aggiornare, oppure il tasto **CODEX DASHBOARD / APRI** per aprire il programma. Le quote sono identificate per durata, 300 e 10080 minuti; durate mancanti sono segnalate. Sono supportati i pulsanti, non le manopole Stream Deck+.

Seleziona un tasto nell'app Stream Deck per personalizzare lingua, percentuale disponibile/consumata e soglia rossa. **Impostazioni app** segue la configurazione globale. Le modifiche per tasto sono salvate da Stream Deck.

Il menu del widget e dell'icona vicino all'orologio offre **Storico quote** e **Informazioni e diagnostica**: due grafici separati per quota 5 ore e settimana, filtrati sulle ultime 24 ore/7 giorni. Gli assi mostrano percentuale disponibile e orario Europe/Rome; il periodo disegnato va dal primo all’ultimo dato raccolto, con valori leggibili al passaggio del mouse. Include, esportazione CSV, versione, piano dell'account, ultimo aggiornamento, stato offline ed esportazione JSON. Lo storico conserva fino a 30 giorni e non registra dati di autenticazione. L'esportazione diagnostica contiene impostazioni, quote e ultimo errore di connessione, senza credenziali.

**Installa aggiornamento** apre un installatore completo scaricato dall'utente. Il pacchetto porta app e plugin alla stessa versione; l'importazione in Stream Deck richiede la conferma nell'app Elgato. Gli aggiornamenti automatici online non sono implementati. La disinstallazione conserva impostazioni e storico; il plugin installato in Elgato si rimuove dall'app Stream Deck.

Il giallo identifica la quota breve; il turchese la settimana. Percentuali e barre diventano rosse alla soglia impostata per ciascuna quota. Nella sezione Soglie e notifiche puoi impostare separatamente Quota 5 ore e Quota settimana (1–50% disponibile), anche quando visualizzi la quota consumata. Le impostazioni esistenti inizializzano entrambe le soglie con il vecchio valore. I tasti Stream Deck seguono la soglia della rispettiva quota, salvo una personalizzazione per tasto. `!` e `OFFLINE` indicano dati precedenti o collegamento mancante; `—` indica un dato non disponibile. Le notifiche non si ripetono finché la quota non risale e poi scende di nuovo. Riconnessione dopo 15 secondi e timeout di lettura dopo 25 secondi.

## Riservatezza e supporto

Il programma legge le quote attraverso Codex, non invia prompt, non legge o copia i file di autenticazione e non consuma crediti di reset. Visualizza percentuali delle quote dell’abbonamento, non conteggi di token o costi API. Prima di condividere una diagnostica, controlla i dettagli che desideri rendere pubblici.

Per problemi o richieste usa i [modelli di segnalazione](https://github.com/kentar91/CodexDashboard/issues/new/choose). Per compilazione e verifiche consulta [Sviluppo](SVILUPPO.md) e [Verifiche](VERIFICHE.md).
