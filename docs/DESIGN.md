# Night City HUD

Tutto il progetto usa un linguaggio visivo ispirato a Cyberpunk 2077: widget, tasti Stream Deck, icona del programma, area di notifica, menu e suggerimenti. Anche le nuove schermate devono seguire queste scelte.

Il logo ufficiale è il monogramma CD originale in `assets/branding`: C gialla, D turchese, forme angolari. Usare la versione con pannello HUD per icone Windows e Stream Deck, il simbolo trasparente per altre interfacce e la firma completa per documenti e materiali. I master SVG e le varianti PNG/ICO sono nella stessa cartella; non rigenerare icone testuali CD alternative durante la compilazione.

La palette condivisa è in `assets/theme.json` e riprende esattamente il riferimento inviato dall'utente: nero `#000000`, rosso `#C5003C`, bordeaux `#880425`, giallo `#F3E600` e turchese `#55EAD4`. Il giallo identifica la quota breve e l'apertura; il turchese identifica la settimana. Il rosso indica connessione mancante o quota bassa; il bordeaux definisce tracce delle barre, bordi e sfondi di selezione. Questa è la palette di riferimento per tutte le nuove interfacce del progetto.

Usare pannelli scuri, angoli tagliati, linee sottili e asimmetriche, barre nette e piccoli accenti da HUD. Evitare bordi arrotondati, gradienti e bagliori che sfocano il testo. Le percentuali sono sempre l'elemento più grande, soprattutto sui tasti fisici.

I tasti Stream Deck riprendono direttamente il widget: cornice superiore sempre gialla anche per settimana e quota bassa, dettaglio inferiore sinistro e linee laterali turchesi, barre sottili. Le etichette mantengono il colore della finestra; solo percentuale e barra diventano rosse alla soglia. Il tasto di apertura mostra CODEX DASHBOARD e APRI, senza una barra di quota fittizia.

La famiglia tipografica principale è Bahnschrift. I tasti Stream Deck sono immagini PNG da 144 × 144 pixel: il testo viene misurato, adattato ai margini e centrato prima di essere inviato. Le etichette sono brevi e maiuscole; il normale testo esplicativo rimane in italiano e leggibile. Mantenere le dimensioni ingrandite dei tasti Stream Deck.

`build.ps1` genera i colori C# e il layout WPF dalla palette condivisa, e incorpora le esportazioni del logo ufficiale. Le modifiche al tema si applicano alla successiva compilazione.
