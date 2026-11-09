# Manuale del CRM
Clienti, lead, trattative, attività e offerte

## Introduzione
Il CRM serve a seguire il rapporto con i clienti e i potenziali clienti dell'azienda: anagrafiche e persone di riferimento, trattative (opportunità), attività commerciali (chiamate, email, appuntamenti), offerte, note e allegati, con i report sui risultati.

Questo manuale è costruito sul **tuo menu**: trovi un capitolo per ogni voce della sezione **CRM** che vedi, con la descrizione di cosa puoi fare in quella pagina. Il CRM è disponibile quando è attivo per la tua azienda: lo attiva il centro.

### Chi fa cosa
- **Commerciale (ruolo User)**: lavora sui clienti che vede (vedi "Visibilità"), registra attività, segue lead e opportunità, prepara e invia offerte.
- **CompanyAdmin**: vede tutti i clienti e tutte le trattative, assegna i responsabili, consulta i report, riassegna i dati di un commerciale.
- **CompanyPowerAdmin**: tutto quello che fa il CompanyAdmin, più la configurazione del CRM (tabelle e impostazioni).

> Un utente può avere più ruoli: per esempio un CompanyAdmin che è anche commerciale vede sia le voci di amministrazione sia quelle del commerciale.

### Visibilità
L'azienda sceglie cosa vedono i commerciali (dalle **Impostazioni CRM**):
- **Solo i propri clienti** (predefinito): il commerciale vede i clienti di cui è responsabile, quelli senza responsabile ("condivisi") e quelli in cui è coinvolto, cioè con un'opportunità di cui è responsabile o un'attività assegnata a lui. Vede i lead suoi e quelli senza responsabile.
- **Tutti i clienti**: ogni commerciale vede tutto.

Gli amministratori vedono sempre tutto. Le regole valgono ovunque: elenchi, schede, pipeline, agenda, offerte e report.

### Note e allegati
In molte schede (clienti, opportunità, lead, offerte) trovi le sezioni **Note** e **Allegati**.
- **Note**: scrivi il testo e premi **Aggiungi**; una nota si può **fissare in alto**. Le può modificare o cancellare l'autore o un amministratore.
- **Allegati**: premi **Carica un file** (massimo 20 MB); da ogni riga scarichi o cancelli il file. Lo cancella chi l'ha caricato o un amministratore.

@@ /crmcustomerlist
I clienti del CRM sono l'anagrafica clienti comune dell'azienda, la stessa usata dalle presenze e dal documentale: il CRM aggiunge i suoi dati commerciali.

Nell'elenco vedi i clienti visibili con stato, responsabile, città, opportunità aperte e loro valore, prossima attività. In alto puoi filtrare per **responsabile**, **stato** ed **etichetta**; il campo di ricerca cerca nel testo. Nelle **Opzioni** trovi **Mostra disabilitati**. Dal pulsante **+** trovi **Nuovo cliente** ed **Esporta in Excel**.

### Creare un cliente
1. Premi **+** e scegli **Nuovo cliente**.
2. Compila la ragione sociale e gli altri dati: tipo di anagrafica, stato commerciale, responsabile, fonte, etichette, dati fiscali e recapiti.
3. Conferma: il cliente resta aperto e compaiono le sezioni Contatti, Opportunità, Attività, Offerte, Note e Allegati.

> Il commerciale che crea un cliente ne diventa il responsabile. Un commerciale può prendere un cliente senza responsabile o lasciarne uno suo; assegnare un cliente a un altro utente è compito degli amministratori.

### Lo stato commerciale
- **Prospect**: potenziale cliente (predefinito).
- **Cliente**: cliente acquisito.
- **Ex cliente**.
- **Non interessato**.

La casella **Consenso alle comunicazioni commerciali** registra il consenso del cliente con la data in cui è stato dato; togliendola la data si cancella.

### La scheda cliente
- **Dati**: anagrafica e dati commerciali (si salvano con **Conferma**).
- **Contatti**: le persone di riferimento; un nuovo contatto si aggiunge con **+** › **Contatto**: si apre la finestra con cognome, nome, ruolo, telefono, cellulare, email, note e la casella "Contatto principale" (uno solo per cliente).
- **Opportunità**, **Attività**, **Offerte**: le voci del cliente.

Per aggiungere qualcosa a un cliente già salvato premi **+** e scegli dal menu che si apre dal basso: **Contatto**, **Opportunità**, **Attività** o **Offerta**; la voce nasce già collegata al cliente.
- **Note** e **Allegati**: come descritto nell'introduzione.

> I clienti non si cancellano dal CRM: si disattivano (campo **Attivo**). La cancellazione definitiva si fa da Amministrazione ed è bloccata, con un messaggio, se il cliente è usato da opportunità, offerte, attività o allegati.

### Esportare in Excel
Dal pulsante **+** scegli **Esporta in Excel**: scarichi l'elenco con i filtri scelti, comprensivo di stato, responsabile, città, email, telefono, etichette, opportunità aperte e valore, ultima e prossima attività.

@@ /crmleadlist
Un lead è un contatto non ancora qualificato, per esempio arrivato dal sito o da una fiera. L'elenco mostra i lead aperti; nelle **Opzioni** trovi **Mostra anche convertiti e scartati**.

### Gestire un lead
1. Premi **+** e scegli **Nuovo lead**.
2. Indica cognome, nome, ruolo, azienda, email, telefono, fonte e stato (Nuovo, Contattato, Qualificato, Scartato).
3. Imposta responsabile, consenso commerciale e note, poi salva.
4. Dopo il salvataggio aggiungi **attività** e **note** al lead.

### Convertire un lead in cliente
1. Apri il lead e premi **Converti**.
2. Scegli se creare un **nuovo cliente** con i dati del lead o collegarlo a un **cliente esistente**.
3. Se vuoi, lascia selezionato **Crea anche un'opportunità** con titolo e valore.
4. Conferma: si apre la scheda del cliente.

> Con la conversione la persona del lead diventa una persona di riferimento del cliente, le attività e le note del lead passano al cliente e il lead diventa **Convertito**.

@@ /crmpipeline
Un'opportunità è una trattativa con un cliente: titolo, valore, probabilità, fase e data di chiusura prevista. La pipeline mostra le opportunità in colonne, una per fase: prima le fasi aperte, poi quelle vinte e perse (degli ultimi 30 giorni). In ogni colonna vedi il numero e il valore totale. Puoi filtrare per **responsabile** e passare alla vista **Elenco**.

### Spostare un'opportunità
1. Trascina una scheda in un'altra colonna per cambiarne la fase.
2. Su smartphone usa il pulsante con le frecce sulla scheda.
3. Spostandola in una fase **persa** ti viene chiesto il motivo della perdita.
4. Clicca una scheda per aprire l'opportunità.

### La scheda dell'opportunità
- **Fase**: la probabilità viene proposta da quella della fase (puoi cambiarla).
- **Chiusura prevista** e, per le fasi vinte o perse, la **data di chiusura** (proposta oggi).
- **Motivo della perdita**: nelle fasi perse.
- **Responsabile**, **fonte**, **persona di riferimento** del cliente e **note**.

Le sezioni **Attività**, **Offerte**, **Note**, **Allegati** e **Storico** completano la scheda. Lo **storico** registra ogni cambio di fase e di valore, con chi l'ha fatto e quando: è la base dei report.

> Cancellando un'opportunità si cancellano anche storico, attività e note; le offerte restano senza collegamento. Se ha allegati vanno cancellati prima.

@@ /crmactivitylist
L'agenda raccoglie le attività da fare e quelle concluse. È la prima voce del menu dei commerciali ed è divisa in sezioni:
- **Oggi**: le attività di oggi e quelle da fare già iniziate.
- **Scadute**: le attività da fare con l'inizio già passato; il numero in rosso indica quante sono.
- **Prossime**: le attività da fare dei giorni successivi.
- **Fatte / annullate**: lo storico.

Gli amministratori trovano l'interruttore **Solo le mie** per vedere anche le attività degli altri.

### Creare un'attività
1. Premi **+** e scegli **Nuova attività**.
2. Scegli il **tipo** (chiamata, email, appuntamento…): la fine viene proposta con la durata del tipo.
3. Indica **oggetto**, **inizio**, **fine** e a chi è **assegnata** (di norma a te).
4. Collega l'attività a un **cliente** (e, se vuoi, a una persona di riferimento e a un'opportunità aperta) oppure a un **lead**.
5. Conferma.

> Un'attività deve essere collegata a un cliente o a un lead. Puoi anche aggiungerla dalla scheda di un cliente, di un'opportunità o di un lead. Un commerciale può assegnare a sé stesso o passare ad altri le proprie attività; gli altri cambi di assegnatario li fanno gli amministratori.

### Segnare un'attività come fatta
1. Nella riga dell'attività premi **Fatta**.
2. Scrivi l'esito (per esempio "richiamare a fine mese") e conferma.

### Promemoria
Prima dell'inizio di un'attività da fare, l'assegnatario riceve un avviso a comparsa con il pulsante **Apri**. Il preavviso (predefinito 30 minuti) si imposta nelle Impostazioni CRM. L'avviso arriva solo se sei collegato all'applicazione; l'attività resta comunque nell'agenda.

@@ /crmofferlist
L'elenco mostra le offerte visibili con numero, cliente, oggetto, data, stato e totale; da ogni riga scarichi il **PDF**.

### Preparare un'offerta
1. Premi **+** e scegli **Nuova offerta**.
2. Scegli **cliente**, **persona di riferimento** e, se c'è, l'**opportunità**.
3. Controlla **data** e **valida fino al** e scrivi l'**oggetto**.
4. Aggiungi le **righe**: scegliendo un **prodotto** si compilano descrizione, prezzo, unità di misura e IVA; puoi modificare quantità, prezzo, sconto % e IVA %.
5. Premi **Conferma**: l'offerta riceve il numero (per anno, con l'eventuale prefisso) e resta aperta.

I totali si aggiornano mentre scrivi. Puoi anche aggiungere l'offerta dalla scheda del cliente o dell'opportunità.

### PDF e invio
- **PDF** scarica l'offerta con i dati dell'azienda e del cliente, le righe, i totali, le note e le condizioni impostate.
- **Invia** apre la finestra di invio: l'email è proposta da quella della persona di riferimento o del cliente; il messaggio è facoltativo (vuoto = testo predefinito). Il PDF parte in allegato e l'offerta diventa **Inviata**, con la data di invio.

### Gli stati dell'offerta
Bozza, Inviata, Accettata, Rifiutata: lo stato si cambia nella testata dell'offerta. Si cancellano solo le offerte in **bozza** senza allegati.

> Se l'invio delle email non è configurato compare un messaggio: in quel caso scarica il PDF e invialo con la tua posta.

@@ /crmreport
I report mostrano, per il periodo scelto (predefinito dall'inizio dell'anno), l'andamento commerciale:
- opportunità **vinte** (con il valore) e **perse** nel periodo, con il tasso di successo;
- **lead** creati e convertiti, con la percentuale di conversione;
- **pipeline aperta per fase** e **per responsabile**, con il valore e il valore pesato (valore × probabilità);
- **vinte per responsabile** e **perse per motivo**;
- **attività fatte per utente**.

> I report contano solo i dati che l'utente vede: un commerciale vede i propri numeri, gli amministratori vedono tutto.

@@ /crmparlist
Qui raccogli le tabelle del CRM, create all'attivazione con valori predefiniti e riutilizzate in tutte le schede:
- **Fasi**: descrizione, tipo (aperta, vinta, persa), probabilità proposta e ordine nella pipeline;
- **Tipi di attività**: icona, colore e durata proposta;
- **Motivi di perdita** e **Fonti**;
- **Prodotti**: codice, descrizione, prezzo, unità di misura e IVA, proposti nelle righe delle offerte;
- **Etichette**: per raggruppare i clienti, con un colore.

> Un valore già usato non si può cancellare: disattivalo, resta nello storico ma non viene più proposto. Deve restare almeno una fase attiva per ogni tipo e almeno un tipo di attività.

@@ /crmsettings
Qui imposti il comportamento generale del CRM per l'azienda:
- **visibilità dei commerciali** (solo i propri clienti / tutti);
- **valuta** (codice di 3 lettere, per esempio EUR);
- **preavviso dei promemoria** in minuti (0 = nessun promemoria);
- per le offerte: **prefisso del numero**, **validità predefinita** in giorni e **condizioni** stampate in fondo al PDF.

@@ /crmreassign
Questa funzione passa in blocco i dati di un commerciale a un altro utente, per esempio quando lascia l'azienda.
1. Scegli l'utente **Da**.
2. Scegli l'utente **A** (oppure "Nessuno": i dati restano senza responsabile).
3. Scegli cosa riassegnare: clienti, opportunità, lead, attività da fare.
4. Premi **Riassegna**: compare il numero di dati riassegnati.

> Le attività da fare si riassegnano solo indicando un destinatario. Quando un utente viene tolto dall'azienda i suoi clienti, opportunità e lead restano senza responsabile e le sue attività da fare vengono annullate.

## Domande frequenti
### Perché non vedo un cliente o un'opportunità?
Dipende dalla **visibilità** impostata dall'azienda. Con "Solo i propri clienti" vedi quelli di cui sei responsabile, quelli senza responsabile e quelli in cui sei coinvolto. Gli amministratori vedono sempre tutto.

### Come cambio il responsabile di un cliente?
Un commerciale può prendere un cliente senza responsabile o lasciarne uno suo. Assegnare un cliente a un altro utente, o spostare in blocco i dati di un commerciale, è compito degli amministratori (Riassegnazione).

### Non riesco a cancellare un cliente: perché?
I clienti non si cancellano dal CRM, si disattivano. La cancellazione definitiva è bloccata se il cliente è usato da opportunità, offerte, attività o allegati.

### Non parte l'invio dell'offerta via email.
Se l'invio delle email non è configurato compare un messaggio: scarica il PDF dell'offerta e invialo con la tua posta.
