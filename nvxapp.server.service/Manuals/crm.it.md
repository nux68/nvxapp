# Manuale del CRM
Clienti, lead, trattative, attività e offerte

## Introduzione
Il CRM serve a seguire il rapporto con i clienti e i potenziali clienti dell'azienda: anagrafiche e persone di riferimento, trattative (opportunità), attività commerciali (chiamate, email, appuntamenti), offerte, note e allegati, con i report sui risultati.

Le funzioni del CRM si trovano nella sezione **CRM** del menu. Il CRM è disponibile quando è attivo per la tua azienda: lo attiva il centro.

### Chi fa cosa
- **Commerciale (ruolo User)**: lavora sui clienti che vede (vedi "Visibilità"), registra attività, segue lead e opportunità, prepara e invia offerte.
- **CompanyAdmin**: vede tutti i clienti e tutte le trattative, assegna i responsabili, consulta i report, riassegna i dati di un commerciale.
- **CompanyPowerAdmin**: tutto quello che fa il CompanyAdmin, più la configurazione del CRM (tabelle e impostazioni).

> Un utente può avere più ruoli: per esempio un CompanyAdmin che è anche commerciale vede sia le voci di amministrazione sia quelle del commerciale.

### Visibilità
L'azienda sceglie cosa vedono i commerciali (Configurazione CRM > Impostazioni):
- **Solo i propri clienti** (predefinito): il commerciale vede i clienti di cui è responsabile, quelli senza responsabile ("condivisi") e quelli in cui è coinvolto, cioè con un'opportunità di cui è responsabile o un'attività assegnata a lui. Vede i lead suoi e quelli senza responsabile.
- **Tutti i clienti**: ogni commerciale vede tutto.

Gli amministratori vedono sempre tutto. Le regole valgono ovunque: elenchi, schede, pipeline, agenda, offerte e report.

## L'agenda
L'agenda (**CRM > Agenda**) raccoglie le attività da fare e quelle concluse. È la prima voce del menu dei commerciali.

### Le sezioni dell'agenda
- **Oggi**: le attività di oggi e quelle da fare già iniziate.
- **Scadute**: le attività da fare con l'inizio già passato; il numero in rosso indica quante sono.
- **Prossime**: le attività da fare dei giorni successivi.
- **Fatte / annullate**: lo storico.

Gli amministratori trovano l'interruttore **Solo le mie** per vedere anche le attività degli altri.

### Segnare un'attività come fatta
1. Nella riga dell'attività premi **Fatta**.
2. Scrivi l'esito (per esempio "richiamare a fine mese") e conferma.

### Creare un'attività
1. Dall'agenda premi **+** e scegli **Nuova attività**, oppure aggiungila dalla scheda di un cliente, di un'opportunità o di un lead.
2. Scegli il **tipo** (chiamata, email, appuntamento, attività…): la fine viene proposta con la durata del tipo.
3. Indica **oggetto**, **inizio**, **fine** e a chi è **assegnata** (di norma a te).
4. Collega l'attività a un **cliente** (e, se vuoi, a una persona di riferimento e a un'opportunità aperta del cliente) oppure a un **lead**.
5. Conferma.

> Un'attività deve essere collegata a un cliente o a un lead. Un commerciale può assegnare a sé stesso o passare ad altri le proprie attività; gli altri cambi di assegnatario li fanno gli amministratori.

### Promemoria
Prima dell'inizio di un'attività da fare, l'assegnatario riceve un avviso a comparsa con il pulsante **Apri**. Il preavviso (predefinito 30 minuti) si imposta in Configurazione CRM > Impostazioni. L'avviso arriva solo se sei collegato all'applicazione; l'attività resta comunque nell'agenda.

## I clienti
I clienti del CRM sono l'anagrafica clienti comune dell'azienda, la stessa usata dalle presenze e dal documentale: il CRM aggiunge i suoi dati commerciali.

### L'elenco
Da **CRM > Clienti** vedi i clienti visibili con stato, responsabile, città, opportunità aperte e loro valore, prossima attività. In alto puoi filtrare per **responsabile**, **stato** ed **etichetta**; il campo di ricerca cerca nel testo. Nelle **Opzioni** trovi **Mostra disabilitati**.

Dal pulsante **+** trovi **Nuovo cliente** ed **Esporta in Excel** (l'elenco con i filtri scelti).

### Creare un cliente
1. Premi **+** e scegli **Nuovo cliente**.
2. Compila la ragione sociale e gli altri dati: tipo di anagrafica (cliente, fornitore o entrambi), stato commerciale, responsabile, fonte, etichette, settore, sito, fatturato stimato, numero di dipendenti, dati fiscali e recapiti.
3. Conferma: il cliente resta aperto e compaiono le sezioni Contatti, Opportunità, Attività, Offerte, Note e Allegati.

> Il commerciale che crea un cliente ne diventa il responsabile. Un commerciale può prendere un cliente senza responsabile o lasciarne uno suo; assegnare un cliente a un altro utente è compito degli amministratori.

### Stato commerciale
- **Prospect**: potenziale cliente (predefinito).
- **Cliente**: cliente acquisito.
- **Ex cliente**.
- **Non interessato**.

### Consenso alle comunicazioni commerciali
La casella **Consenso alle comunicazioni commerciali** registra il consenso del cliente con la data in cui è stato dato; togliendola la data si cancella.

### La scheda cliente
- **Dati**: anagrafica e dati commerciali (si salvano con **Conferma**).
- **Contatti**: le persone di riferimento; **Aggiungi** apre la finestra con cognome, nome, ruolo, telefono, cellulare, email, note e la casella "Contatto principale" (uno solo per cliente).
- **Opportunità**, **Attività**, **Offerte**: le voci del cliente, con **Aggiungi** per crearne una già collegata al cliente.
- **Note** e **Allegati**: vedi il capitolo dedicato.

> I clienti non si cancellano dal CRM: si disattivano (campo **Attivo**). La cancellazione si fa da Amministrazione > Anagrafiche > Clienti ed è bloccata, con un messaggio, se il cliente è usato da opportunità, offerte, attività o allegati.

## I lead
Un lead è un contatto non ancora qualificato (per esempio arrivato dal sito o da una fiera): **CRM > Lead**.

### Gestire un lead
1. Premi **+** e scegli **Nuovo lead**.
2. Indica cognome, nome, ruolo, azienda, email, telefono, fonte, stato (Nuovo, Contattato, Qualificato, Scartato), responsabile, consenso commerciale e note.
3. Dopo il salvataggio puoi aggiungere **attività** e **note** al lead.

L'elenco mostra i lead aperti; nelle **Opzioni** trovi **Mostra anche convertiti e scartati**.

### Convertire un lead in cliente
1. Apri il lead e premi **Converti**.
2. Scegli se creare un **nuovo cliente** con i dati del lead (ragione sociale = azienda del lead, oppure il nominativo) o collegarlo a un **cliente esistente**.
3. Se vuoi, lascia selezionato **Crea anche un'opportunità** con titolo e valore.
4. Conferma: si apre la scheda del cliente.

Con la conversione la persona del lead diventa una persona di riferimento del cliente, le attività e le note del lead passano al cliente e il lead diventa **Convertito**.

## Opportunità e pipeline
Un'opportunità è una trattativa con un cliente: titolo, valore, probabilità, fase e data di chiusura prevista.

### La pipeline
**CRM > Pipeline** mostra le opportunità in colonne, una per fase: prima le fasi aperte, poi quelle vinte e perse (degli ultimi 30 giorni). In ogni colonna vedi il numero e il valore totale. Puoi filtrare per **responsabile** e passare alla vista **Elenco**.

1. Trascina una scheda in un'altra colonna per cambiarne la fase; su smartphone usa il pulsante con le frecce sulla scheda.
2. Spostando un'opportunità in una fase **persa** ti viene chiesto il motivo della perdita.
3. Clicca una scheda per aprire l'opportunità.

### L'opportunità
- **Fase**: la probabilità viene proposta da quella della fase (puoi cambiarla).
- **Chiusura prevista** e, per le fasi vinte o perse, **data di chiusura** (proposta oggi).
- **Motivo della perdita**: nelle fasi perse.
- **Responsabile**, **fonte**, **persona di riferimento** del cliente e **note**.

Le sezioni **Attività**, **Offerte**, **Note**, **Allegati** e **Storico** completano la scheda. Lo **storico** registra ogni cambio di fase e di valore, con chi l'ha fatto e quando: è la base dei report.

> Cancellando un'opportunità si cancellano anche storico, attività e note; le offerte restano senza collegamento. Se ha allegati vanno cancellati prima.

## Le offerte
**CRM > Offerte** elenca le offerte visibili con numero, cliente, oggetto, data, stato e totale; da ogni riga scarichi il **PDF**.

### Preparare un'offerta
1. Premi **+** e scegli **Nuova offerta**, oppure aggiungila dalla scheda del cliente o dell'opportunità.
2. Scegli **cliente**, **persona di riferimento** e, se c'è, l'**opportunità**.
3. Controlla **data** e **valida fino al** (proposta con la validità predefinita) e scrivi l'**oggetto**.
4. Aggiungi le **righe**: scegliendo un **prodotto** si compilano descrizione, prezzo, unità di misura e IVA; puoi modificare quantità, prezzo, sconto % e IVA %. I totali si aggiornano mentre scrivi.
5. Premi **Conferma**: l'offerta riceve il numero (per anno, con il prefisso eventualmente impostato) e resta aperta.

### PDF e invio
- **PDF** scarica l'offerta con i dati dell'azienda e del cliente, le righe, i totali, le note e le condizioni impostate.
- **Invia** apre la finestra di invio: l'email è proposta da quella della persona di riferimento o del cliente; il messaggio è facoltativo (vuoto = testo predefinito). Il PDF parte in allegato e l'offerta diventa **Inviata**, con la data di invio.

### Stati
Bozza, Inviata, Accettata, Rifiutata: lo stato si cambia nella testata dell'offerta. Si cancellano solo le offerte in **bozza** senza allegati.

> Se l'invio delle email non è configurato compare un messaggio: in quel caso scarica il PDF e invialo con la tua posta.

## Note e allegati
### Note
Nelle sezioni **Note** di clienti, opportunità e lead scrivi una nota e premi **Aggiungi**. Una nota si può **fissare in alto**. Modifica e cancellazione: l'autore o un amministratore.

### Allegati
Nelle sezioni **Allegati** di clienti, opportunità e offerte premi **Carica un file** (massimo 20 MB). Da ogni riga scarichi o cancelli il file; lo cancella chi l'ha caricato o un amministratore.

## I report
**CRM > Report** mostra, per il periodo scelto (predefinito dall'inizio dell'anno):
- opportunità **vinte** (con il valore) e **perse** nel periodo, con il tasso di successo;
- **lead** creati e convertiti, con la percentuale di conversione;
- **pipeline aperta per fase** e **per responsabile**, con il valore e il valore pesato (valore × probabilità);
- **vinte per responsabile** e **perse per motivo**;
- **attività fatte per utente**.

I report contano solo i dati che l'utente vede.

## Configurazione (amministratori) {CompanyPowerAdmin, CompanyAdmin}
### Tabelle {CompanyPowerAdmin}
**Configurazione CRM > Tabelle** (CompanyPowerAdmin) raccoglie le tabelle del CRM, create all'attivazione con valori predefiniti:
- **Fasi**: descrizione, tipo (aperta, vinta, persa), probabilità proposta e ordine nella pipeline;
- **Tipi di attività**: icona, colore e durata proposta;
- **Motivi di perdita** e **Fonti**;
- **Prodotti**: codice, descrizione, prezzo, unità di misura e IVA, proposti nelle righe delle offerte;
- **Etichette**: per raggruppare i clienti, con un colore.

> Un valore già usato non si può cancellare: disattivalo, resta nello storico ma non viene più proposto. Deve restare almeno una fase attiva per ogni tipo e almeno un tipo di attività.

### Impostazioni {CompanyPowerAdmin}
**Configurazione CRM > Impostazioni** (CompanyPowerAdmin):
- **visibilità dei commerciali** (solo i propri clienti / tutti);
- **valuta** (codice di 3 lettere, per esempio EUR);
- **preavviso dei promemoria** in minuti (0 = nessun promemoria);
- per le offerte: **prefisso del numero**, **validità predefinita** in giorni e **condizioni** stampate in fondo al PDF.

### Riassegnazione
**Configurazione CRM > Riassegnazione** (CompanyPowerAdmin e CompanyAdmin) passa in blocco i dati di un commerciale a un altro utente, per esempio quando lascia l'azienda.
1. Scegli l'utente **Da** e l'utente **A** (oppure "Nessuno": i dati restano senza responsabile).
2. Scegli cosa riassegnare: clienti, opportunità, lead, attività da fare.
3. Premi **Riassegna**: compare il numero di dati riassegnati.

> Le attività da fare si riassegnano solo indicando un destinatario. Quando un utente viene tolto dall'azienda i suoi clienti, opportunità e lead restano senza responsabile e le sue attività da fare vengono annullate.

### Export Excel
Dall'elenco dei clienti, **+ > Esporta in Excel** scarica l'elenco con i filtri scelti: stato, responsabile, città, email, telefono, etichette, opportunità aperte e valore, ultima e prossima attività.
