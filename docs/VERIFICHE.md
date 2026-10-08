# Verifiche della versione 2.0

`tests/release-smoke.ps1` verifica conversione quote, valori limite, salvataggio, notifiche, lingua, Applica/Salva/Annulla, modalità di avvio con percorsi contenenti spazi, dimensioni del widget dal 80% al 200%, storico e lettura con righe danneggiate. I test usano cartelle separate e controllano che la voce di avvio Windows rimanga invariata.

Il simulatore Stream Deck verifica registrazione, immagini PNG 144×144 dei tre tasti, aggiornamento e modifica della lingua per un singolo tasto. Il controllo dell'installatore estrae le risorse senza installare e confronta gli hash di programma, icona e plugin. Anteprime del configuratore e dell'installatore sono disponibili in entrambe le lingue.

`--placement-test` verifica le nove posizioni su un monitor con coordinate negative, i margini su schermi piccoli, Applica e Annulla per la posizione, lo spostamento reale della finestra e il ritorno al monitor principale quando quello salvato non esiste. Le prove con DPI differenti e scollegamento fisico dei monitor restano nella lista seguente.

Da verificare sulle postazioni reali:

- installazione, aggiornamento e disinstallazione per utente; dati personali conservati;
- conferma importazione plugin nell'app Elgato e leggibilità sul dispositivo fisico;
- accesso a Windows con avvio manuale, automatico e quando Codex si apre;
- monitor singolo e multipli, DPI 100/125/150/200%, trascinamento fra monitor e rimozione del monitor che ospita il widget;
- autenticazione scaduta e riconnessione dopo interruzione della rete.

Questi controlli richiedono hardware, accessi a Windows o modifiche della connessione che la simulazione non sostituisce. Il sistema di aggiornamento attuale usa il pacchetto completo scelto dall'utente; gli aggiornamenti automatici online richiedono un canale di distribuzione ancora da pubblicare.
