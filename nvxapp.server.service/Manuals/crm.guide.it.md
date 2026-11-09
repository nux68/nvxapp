## Primi passi
Questa guida completa la descrizione delle pagine con i campi, le procedure passo passo, gli esempi e i messaggi che puoi incontrare. Le parti che seguono sono mostrate in base ai tuoi ruoli.

### Se sei un commerciale {User}
La tua pagina di partenza è l'**Agenda**: lì trovi le attività di oggi, quelle scadute e le prossime.
1. Apri **Agenda** e controlla le **Scadute** (il numero in rosso).
2. Dopo ogni chiamata o appuntamento premi **Fatta** e scrivi l'esito.
3. Programma subito l'attività successiva con **+** e **Nuova attività**.
4. Quando entra un contatto nuovo crealo in **Lead**; quando è interessato premi **Converti**.
5. Segui le trattative nella **Pipeline** e prepara le offerte da **Offerte** o dalla scheda del cliente.

> Ciò che crei senza indicare un responsabile diventa tuo: clienti, lead e opportunità nuovi hanno te come responsabile, le attività nuove sono assegnate a te.

### Se sei un amministratore {CompanyAdmin, CompanyPowerAdmin}
Vedi sempre tutti i clienti, le trattative e le attività, a prescindere dalla visibilità scelta per i commerciali.
- Nell'**Agenda** togli **Solo le mie** per vedere anche le attività degli altri.
- Nella **Pipeline** filtra per **responsabile** per controllare il lavoro di ciascuno.
- Nei **Report** confronta vinte, perse e pipeline per responsabile.
- Con la **Riassegnazione** sposti in blocco i dati di un commerciale.

> Un cliente, un lead o un'opportunità creati da un amministratore senza indicare il responsabile restano **senza responsabile** (condivisi): assegnali, se serve.

### Configurazione iniziale {CompanyPowerAdmin}
Il CRM parte con fasi, tipi di attività, motivi di perdita e fonti già pronti. Prima di far lavorare i commerciali:
1. Apri **Impostazioni** e scegli la visibilità dei commerciali, la valuta e il preavviso dei promemoria.
2. Nella stessa pagina imposta prefisso, validità predefinita e condizioni delle offerte.
3. Apri **Tabelle** e adatta le **Fasi** della pipeline con la loro probabilità.
4. Inserisci i **Prodotti** con prezzo, unità di misura e IVA, se fai offerte a listino.
5. Crea le **Etichette** per raggruppare i clienti e controlla **Fonti** e **Motivi di perdita**.

## Concetti chiave
### Il percorso commerciale
Un contatto nuovo entra come **lead**; quando è qualificato si **converte** in **cliente**. Con il cliente si apre un'**opportunità** (la trattativa), si prepara un'**offerta** e, alla fine, l'opportunità si chiude come **vinta** o **persa**. Le **attività** (chiamate, email, appuntamenti) accompagnano tutto il percorso.

| Elemento | Cos'è | Esempio |
|---|---|---|
| Lead | Contatto non ancora qualificato | Mario Rossi, conosciuto in fiera |
| Cliente | Azienda dell'anagrafica comune, con i dati commerciali del CRM | Rossi Srl |
| Opportunità | Trattativa con valore, probabilità e fase | Fornitura software, 15.000 EUR, 50% |
| Attività | Cosa fare o cosa è stato fatto | Richiamare Rossi giovedì alle 10 |
| Offerta | Preventivo con righe, totali, PDF e invio | Offerta 14/2026 |

### Fasi e probabilità
Ogni fase ha un **tipo**: **Aperta**, **Vinta** o **Persa**. Le fasi predefinite sono:

| Fase | Tipo | Probabilità proposta |
|---|---|---|
| Nuova | Aperta | 10% |
| Qualificata | Aperta | 25% |
| Proposta | Aperta | 50% |
| Trattativa | Aperta | 75% |
| Vinta | Vinta | 100% |
| Persa | Persa | 0% |

Quando scegli una fase la probabilità viene proposta da quella della fase; puoi cambiarla. Portando l'opportunità in una fase vinta la probabilità diventa 100%, in una fase persa diventa 0%. Le fasi vinte e perse chiudono l'opportunità e ne fissano la data di chiusura.

### Valore pesato
Il **valore pesato** è il valore dell'opportunità moltiplicato per la sua probabilità: 10.000 EUR al 25% pesano 2.500 EUR. Sommato su tutte le opportunità aperte dà una stima realistica di quanto entrerà; lo trovi nei report della pipeline aperta.

### Responsabile e visibilità
Clienti, lead e opportunità hanno un **responsabile**; le attività hanno un **assegnatario**. Con la visibilità **Solo i propri clienti** un commerciale vede i clienti di cui è responsabile, quelli senza responsabile e quelli in cui è coinvolto; vede le opportunità, le attività e le offerte di quei clienti. Un elemento che non vedi produce il messaggio "non trovato o non visibile".
- Un commerciale può prendere un cliente, un lead o un'opportunità **senza responsabile** o lasciarne uno suo.
- Un commerciale può assegnare a sé un'attività o passare ad altri le proprie.
- Ogni altro cambio di responsabile è degli amministratori: altrimenti compare "Solo gli amministratori dell'azienda possono assegnare ad altri.".

### Promemoria
Prima dell'inizio di un'attività **Da fare** l'assegnatario riceve un avviso a comparsa, con il preavviso impostato nelle Impostazioni (predefinito 30 minuti). Se cambi l'inizio o l'assegnatario dell'attività il promemoria viene ripreparato.

### Disattivare invece di cancellare
Clienti e valori delle tabelle già usati non si cancellano: si disattivano. Un elemento disattivato resta nello storico e nei documenti che lo usano, ma non viene più proposto nelle scelte.

@@ /crmcustomerlist
### Campi
Sezione **Anagrafica e dati commerciali**:

| Campo | Descrizione |
|---|---|
| Ragione sociale | Obbligatoria, massimo 100 caratteri. |
| Tipo anagrafica | Cliente, Fornitore, Cliente e fornitore. I fornitori non sono proposti nelle opportunità e nelle offerte. |
| Stato | Stato commerciale: Prospect (predefinito), Cliente, Ex cliente, Non interessato. |
| Attivo | Togliendolo il cliente non viene più proposto nelle scelte ma resta nello storico. |
| Responsabile | Il commerciale che segue il cliente, oppure Nessuno (cliente condiviso). Per un commerciale è proposto il suo nome. |
| Fonte | Da dove arriva il cliente (tabella Fonti). |
| Etichette | Una o più etichette per raggruppare i clienti; servono anche come filtro nell'elenco. |
| Settore | Testo libero, massimo 100 caratteri. |
| Sito web | Massimo 200 caratteri. |
| Fatturato stimato | Numero, non negativo. |
| N. dipendenti | Numero, non negativo. |
| Consenso alle comunicazioni commerciali | Registra il consenso con la data (Consenso dato il). |

Sezione **Dati fiscali e recapiti**:

| Campo | Descrizione |
|---|---|
| Partita IVA | 11 cifre. |
| Codice fiscale | 16 caratteri alfanumerici oppure 11 cifre. |
| Codice destinatario (SDI) | 6 o 7 caratteri alfanumerici. |
| Indirizzo, CAP, Città | Massimo 100, 10 e 50 caratteri. |
| Provincia, Nazione | Sigla di 2 lettere (per esempio MI, IT). |
| Telefono | Massimo 30 caratteri. |
| Email, PEC | Indirizzi email validi, massimo 100 caratteri. L'email è proposta come destinatario delle offerte quando manca quella della persona di riferimento. |
| Referente | Testo libero, massimo 100 caratteri. |
| Note | Massimo 500 caratteri. |

Finestra **Persona di riferimento** (contatti del cliente):

| Campo | Descrizione |
|---|---|
| Cognome, Nome | Almeno uno dei due è obbligatorio; massimo 100 caratteri ciascuno. |
| Ruolo | Per esempio Ufficio acquisti; massimo 100 caratteri. |
| Telefono, Cellulare | Massimo 30 caratteri; nell'elenco si vede il cellulare, se c'è. |
| Email | Email valida, massimo 100 caratteri; è proposta come destinatario delle offerte. |
| Contatto principale | Uno solo per cliente: è evidenziato nell'elenco dei contatti. |
| Note | Massimo 500 caratteri. |

### Come fare
Aggiungere contatti, opportunità, attività e offerte dalla scheda del cliente:
1. Apri il cliente dall'elenco (il cliente deve essere già salvato).
2. Premi il pulsante **+** in basso e scegli **Aggiungi al cliente**.
3. Dal menu che si apre dal basso scegli **Contatto**, **Opportunità**, **Attività** oppure **Offerta**.
4. Compila la finestra o la pagina che si apre: il cliente è già indicato.
5. Conferma: torni alla scheda, con il nuovo elemento nella sezione corrispondente.

Modificare un contatto:
1. Apri la sezione **Contatti** della scheda.
2. Nella riga del contatto premi **Modifica** (oppure **Cancella** per toglierlo).
3. Correggi i dati e conferma.

### Esempio
La segreteria riceve una richiesta da Bianchi Arredi Srl. La commerciale Laura crea il cliente con stato Prospect, fonte Sito web ed etichetta Arredamento; dopo la conferma la scheda resta aperta. Con **+** aggiunge il contatto Paolo Bianchi (Ufficio acquisti, contatto principale), poi un'attività "Prima telefonata" per domani alle 10 e un'opportunità "Arredo uffici sede" da 8.000 EUR.

### Attenzione
- Contatti, opportunità, attività, offerte, note e allegati si aggiungono solo dopo aver salvato il cliente la prima volta.
- Su un cliente già esistente **Conferma** salva e torna all'elenco.
- "La ragione sociale è obbligatoria.": compila la ragione sociale.
- "Solo gli amministratori dell'azienda possono assegnare ad altri.": un commerciale può solo prendere un cliente senza responsabile o lasciarne uno suo.
- "Cliente non trovato o non visibile.": il cliente non rientra nella tua visibilità (per esempio è stato assegnato a un altro commerciale).
- L'esportazione in Excel contiene solo i clienti che vedi, con i filtri attivi.

=> /crmleadlist /crmpipeline /crmactivitylist /crmofferlist

@@ /crmleadlist
### Campi
| Campo | Descrizione |
|---|---|
| Cognome, Nome, Ruolo | Massimo 100 caratteri ciascuno. |
| Azienda | Massimo 100 caratteri; con la conversione diventa la ragione sociale del nuovo cliente. |
| Email | Email valida, massimo 100 caratteri. |
| Telefono | Massimo 30 caratteri. |
| Stato | Nuovo (predefinito), Contattato, Qualificato, Scartato; Convertito lo imposta la conversione. |
| Fonte | Da dove arriva il lead (tabella Fonti). |
| Responsabile | Per un commerciale è proposto il suo nome; Nessuno = lead condiviso. |
| Consenso alle comunicazioni commerciali | Passa al cliente con la conversione. |
| Note | Massimo 2000 caratteri. |

Finestra **Converti il lead in cliente**:

| Campo | Descrizione |
|---|---|
| Cliente | Nuovo cliente dai dati del lead (predefinito) oppure un cliente attivo esistente. |
| Crea anche un'opportunità | Selezionata per default; l'opportunità parte nella prima fase aperta. |
| Titolo | Proposto con l'azienda o il nominativo del lead; massimo 200 caratteri. |
| Valore | Valore della nuova opportunità, non negativo. |

### Come fare
Registrare una chiamata a un lead:
1. Apri il lead e passa alla sezione **Attività**.
2. Premi **Aggiungi**: l'attività è già collegata al lead.
3. Scegli il tipo Chiamata, scrivi l'oggetto e conferma.

### Esempio
In fiera Marco raccoglie il biglietto di Giulia Verdi, responsabile acquisti di Verdi Impianti. Crea il lead con fonte Fiera / evento e stato Nuovo, poi una chiamata per lunedì. Dopo la telefonata segna l'attività come fatta e porta il lead a Qualificato; quando Giulia chiede un preventivo preme **Converti**, sceglie **Nuovo cliente dai dati del lead** e crea l'opportunità "Impianto magazzino" da 12.000 EUR: si apre la scheda del nuovo cliente Verdi Impianti, con Giulia come contatto principale.

### Attenzione
- "Inserisci il nominativo o l'azienda del lead.": serve almeno cognome, nome o azienda.
- Il pulsante **Converti** compare solo su un lead salvato e non ancora convertito; "Il lead è già stato convertito in cliente." se lo era già.
- Con la conversione su un nuovo cliente: ragione sociale = azienda del lead (o il nominativo), stato Prospect, stesso responsabile, fonte e consenso del lead.
- La persona del lead diventa contatto principale solo se il cliente non aveva ancora contatti.
- Un lead convertito resta consultabile con **Mostra anche convertiti e scartati** e indica il cliente in cui è stato convertito.

=> /crmcustomerlist /crmpipeline /crmactivitylist

@@ /crmpipeline
### Campi
| Campo | Descrizione |
|---|---|
| Titolo | Obbligatorio, massimo 200 caratteri. |
| Cliente | Obbligatorio; sono proposti i clienti attivi che non sono solo fornitori. |
| Contatto | Persona di riferimento del cliente scelto (facoltativa). |
| Fase | Obbligatoria; una nuova opportunità parte dalla prima fase aperta. |
| Valore | Importo nella valuta del CRM, non negativo. |
| Probabilità % | Da 0 a 100; proposta dalla fase. |
| Chiusura prevista | Data in cui pensi di chiudere la trattativa. |
| Data di chiusura | Solo nelle fasi vinte o perse; se vuota viene messa la data di oggi. |
| Motivo della perdita | Solo nelle fasi perse (tabella Motivi di perdita). |
| Responsabile | Per un commerciale è proposto il suo nome. |
| Fonte | Da dove arriva la trattativa. |
| Note | Massimo 2000 caratteri. |

### Come fare
Creare un'opportunità:
1. Premi **+** e scegli **Nuova opportunità** (oppure usa il **+** della scheda cliente).
2. Scrivi il titolo, scegli il cliente e, se vuoi, il contatto.
3. Controlla fase, valore e probabilità e indica la chiusura prevista.
4. Conferma: la scheda compare nella colonna della fase.

Chiudere una trattativa vinta:
1. Trascina la scheda nella colonna della fase vinta (su smartphone usa il pulsante con le frecce e scegli **Cambia fase**).
2. Apri l'opportunità e controlla la **Data di chiusura**.
3. Aggiorna lo stato dell'offerta collegata ad Accettata.

### Esempio
L'opportunità "Arredo uffici sede" (8.000 EUR) è in Proposta al 50%: pesa 4.000 EUR. Il cliente chiede uno sconto e Laura la sposta in Trattativa: la probabilità proposta diventa 75% e lo storico registra il passaggio. Due settimane dopo il cliente sceglie un concorrente: Laura trascina la scheda in Persa e indica il motivo "Scelto un concorrente"; la perdita compare nel report Perse per motivo.

### Attenzione
- "Il titolo dell'opportunità è obbligatorio.": compila il titolo.
- "Fase non valida.": la fase scelta non esiste più; ricarica la pagina e scegline un'altra.
- "L'opportunità ha degli allegati: cancellali prima.": per cancellare l'opportunità togli prima i suoi allegati.
- Nella vista a colonne le opportunità vinte e perse sono solo quelle degli ultimi 30 giorni; le altre si trovano dalla scheda del cliente.
- Lo storico registra un passaggio solo quando cambiano la fase o il valore.

=> /crmcustomerlist /crmofferlist /crmreport /crmactivitylist

@@ /crmactivitylist
### Campi
| Campo | Descrizione |
|---|---|
| Tipo | Obbligatorio (tabella Tipi di attività); è proposto il primo tipo attivo. |
| Oggetto | Obbligatorio, massimo 200 caratteri. |
| Inizio | Obbligatorio, data e ora; proposto all'ora intera successiva. |
| Fine | Facoltativa; proposta con la durata del tipo; non può precedere l'inizio. |
| Assegnata a | Chi deve svolgerla; proposto l'utente collegato. |
| Cliente | Il cliente dell'attività (clienti attivi). |
| Contatto | Persona di riferimento del cliente. |
| Opportunità | Una delle opportunità aperte del cliente. |
| Descrizione | Massimo 4000 caratteri. |
| Stato | Da fare (predefinito), Fatta, Annullata. |
| Esito | Com'è andata; massimo 2000 caratteri. |

Tipi di attività predefiniti:

| Tipo | Durata proposta |
|---|---|
| Chiamata | 15 minuti |
| Email | 10 minuti |
| Appuntamento | 60 minuti |
| Attività | 30 minuti |

### Come fare
Annullare o spostare un'attività:
1. Apri l'attività dalla riga con **Modifica**.
2. Per spostarla cambia **Inizio** e **Fine**; per annullarla porta lo **Stato** ad Annullata.
3. Conferma: un'attività annullata passa in **Fatte / annullate** e non genera promemoria.

Passare un'attività a un collega:
1. Apri l'attività e cambia **Assegnata a**.
2. Conferma: l'attività compare nell'agenda del collega, che riceverà il promemoria.

### Esempio
Lunedì alle 9 Marco apre l'agenda: in **Scadute** c'è "Richiamare Verdi Impianti" di venerdì. Telefona, preme **Fatta** e scrive l'esito "Inviare offerta entro mercoledì". Subito dopo crea un'attività di tipo Attività "Preparare offerta Verdi" per martedì alle 15, collegata al cliente e all'opportunità "Impianto magazzino"; martedì alle 14:30 riceve il promemoria.

### Attenzione
- "Collega l'attività a un cliente o a un lead.": scegli il cliente (un'attività di un lead si crea dalla scheda del lead).
- "La fine è precedente all'inizio.": correggi le date.
- "L'oggetto dell'attività è obbligatorio." e "Tipo di attività non valido." si risolvono compilando oggetto e tipo.
- "Solo gli amministratori dell'azienda possono assegnare ad altri.": un commerciale non può riassegnare un'attività di un altro utente.
- L'agenda mostra le attività dagli ultimi 60 giorni ai prossimi 180: quelle più vecchie o più lontane si trovano dalla scheda del cliente, del lead o dell'opportunità.
- Un'attività di un lead passa al cliente con la conversione del lead.

=> /crmcustomerlist /crmleadlist /crmsettings

@@ /crmofferlist
### Campi
Testata:

| Campo | Descrizione |
|---|---|
| Cliente | Obbligatorio; clienti attivi che non sono solo fornitori. |
| Contatto | Persona di riferimento: la sua email è proposta per l'invio. |
| Opportunità | Facoltativa; deve essere del cliente scelto. |
| Data | Obbligatoria; proposta oggi. Il suo anno determina la numerazione. |
| Valida fino al | Se vuota viene calcolata con la validità predefinita delle Impostazioni. |
| Stato | Bozza, Inviata, Accettata, Rifiutata. |
| Oggetto | Massimo 200 caratteri. |
| Note | Massimo 4000 caratteri; stampate nel PDF. |

Righe:

| Campo | Descrizione |
|---|---|
| Prodotto | Facoltativo; compila descrizione, prezzo, UM e IVA dal listino. |
| Descrizione | Obbligatoria per ogni riga; massimo 500 caratteri. |
| Q.tà | Proposta 1. |
| Prezzo | Prezzo unitario. |
| Sconto % | Da 0 a 100. |
| IVA % | Proposta 22. |

Importo riga = quantità × prezzo × (1 - sconto/100), arrotondato ai centesimi. L'imponibile è la somma degli importi, l'IVA è calcolata riga per riga, il totale è imponibile più IVA.

Finestra **Invia l'offerta**:

| Campo | Descrizione |
|---|---|
| Email del destinatario | Obbligatoria e valida; proposta dal contatto o dal cliente. |
| Messaggio | Facoltativo, massimo 4000 caratteri; vuoto = testo predefinito. |

### Come fare
Aggiungere o togliere righe:
1. Premi **Aggiungi riga** sotto l'elenco delle righe.
2. Scegli un prodotto oppure scrivi direttamente la descrizione.
3. Per togliere una riga premi il cestino in fondo alla riga.
4. Premi **Conferma** per salvare: i totali vengono ricalcolati.

Registrare l'esito di un'offerta:
1. Apri l'offerta inviata.
2. Porta lo **Stato** ad Accettata o Rifiutata e premi **Conferma**.
3. Aggiorna anche la fase dell'opportunità collegata nella pipeline.

### Esempio
Laura prepara l'offerta per Bianchi Arredi: sceglie il cliente, il contatto Paolo Bianchi e l'opportunità "Arredo uffici sede". Aggiunge 10 scrivanie a 450 EUR con sconto 10% (4.050 EUR) e il montaggio a 300 EUR: imponibile 4.350 EUR, IVA 22% 957 EUR, totale 5.307 EUR. Con **Conferma** l'offerta riceve il numero (con prefisso OF diventa per esempio OF14/2026); con **Invia** parte all'indirizzo di Paolo e lo stato diventa Inviata.

### Attenzione
- "L'offerta deve avere almeno una riga." e "Ogni riga deve avere una descrizione.": completa le righe prima di confermare.
- **PDF** e **Invia** compaiono solo dopo il primo salvataggio.
- "L'invio delle email non è configurato.": scarica il PDF e invialo con la tua posta. "Indirizzo email non valido.": correggi il destinatario.
- "Si cancellano solo le offerte in bozza." e "L'offerta ha degli allegati: cancellali prima.": un'offerta inviata non si cancella, si porta a Rifiutata.
- La numerazione riparte da 1 a ogni nuovo anno.

=> /crmcustomerlist /crmpipeline /crmsettings /crmparlist

@@ /crmreport
### Come fare
Leggere i risultati di un periodo:
1. Imposta **Dal** e **Al**.
2. Premi **Aggiorna**.
3. Leggi in alto vinte, perse, tasso di successo e conversione dei lead; sotto trovi le tabelle per fase, responsabile, motivo e utente.

### Esempio
A fine trimestre l'amministratore imposta dal 1 luglio al 30 settembre: 12 vinte e 8 perse danno un tasso di successo del 60%. In **Perse per motivo** prevale "Prezzo": decide di rivedere i listini. In **Pipeline aperta per responsabile** vede che Marco ha 40.000 EUR aperti ma solo 12.000 EUR pesati: le sue trattative sono ancora nelle prime fasi.

### Attenzione
- "Il periodo non è valido.": la data Al non può precedere la data Dal.
- Vinte e perse sono le opportunità con la data di chiusura nel periodo; i lead convertiti sono quelli creati nel periodo e già convertiti.
- Il tasso di successo è il rapporto tra vinte e (vinte + perse) nel periodo.
- La pipeline aperta non dipende dal periodo: mostra le opportunità aperte in questo momento.
- Le opportunità senza responsabile compaiono come "(senza responsabile)", le perdite senza motivo come "(motivo non indicato)".

=> /crmpipeline /crmactivitylist /crmleadlist

@@ /crmparlist
### Campi
| Campo | Descrizione |
|---|---|
| Descrizione | Obbligatoria, massimo 100 caratteri (tutte le tabelle). |
| Tipo di fase | Solo Fasi: Aperta, Vinta, Persa. |
| Probabilità % | Solo Fasi: da 0 a 100, proposta nelle opportunità. |
| Icona (nome Ionicons) | Solo Tipi di attività: nome dell'icona, per esempio call, mail, people. |
| Durata (minuti) | Solo Tipi di attività: proposta per calcolare la fine (predefinito 30). |
| Colore | Tipi di attività ed Etichette: primary, secondary, tertiary, success, warning, danger, medium. |
| Codice | Solo Prodotti: massimo 30 caratteri. |
| Prezzo, UM, IVA % | Solo Prodotti: proposti nelle righe delle offerte; UM massimo 10 caratteri, IVA proposta 22. |
| Ordine | Posizione nelle scelte e, per le fasi, nella pipeline. |
| Attivo | Un valore disattivato non viene più proposto. |

### Come fare
Aggiungere un valore:
1. Scegli la tabella (Fasi, Tipi di attività, Motivi di perdita, Fonti, Prodotti, Etichette).
2. Premi **+** e **Nuovo valore**.
3. Compila i campi e conferma.

### Esempio
Un'azienda vuole una fase "Demo fatta" tra Qualificata e Proposta: crea la fase con tipo Aperta, probabilità 40% e ordine 25 (le fasi predefinite hanno ordine 10, 20, 30...). Da quel momento la pipeline mostra la nuova colonna.

### Attenzione
- "Il valore è usato: invece di cancellarlo, disattivalo.": disattivalo con **Attivo**.
- "Deve restare almeno una fase attiva di ogni tipo (aperta, vinta, persa).": vale anche cambiando il tipo di una fase.
- "Deve restare almeno un tipo di attività attivo.".
- "La descrizione è obbligatoria.": compila la descrizione.

=> /crmpipeline /crmofferlist /crmsettings

@@ /crmsettings
### Campi
| Campo | Descrizione |
|---|---|
| I commerciali (ruolo User) vedono | Solo i propri clienti (predefinito) oppure Tutti i clienti. |
| Valuta (codice di 3 lettere) | Obbligatoria; predefinita EUR. Si vede accanto ai valori di opportunità e offerte. |
| Preavviso dei promemoria (minuti) | Predefinito 30; 0 = nessun promemoria; massimo una settimana. |
| Prefisso del numero | Massimo 10 caratteri, messo davanti al numero dell'offerta (per esempio OF14/2026). |
| Validità predefinita (giorni) | Predefinita 30; usata quando Valida fino al è vuota; 0 = nessuna scadenza proposta. |
| Condizioni in fondo al PDF | Massimo 2000 caratteri: termini di pagamento, consegna, garanzia. |

### Esempio
Una piccola azienda con tre commerciali che si coprono a vicenda sceglie **Tutti i clienti**, preavviso 15 minuti, prefisso OF, validità 60 giorni e come condizioni "Pagamento 30 gg d.f. Consegna 4 settimane dall'ordine.".

### Attenzione
- Il cambio di visibilità vale subito per tutti i commerciali, in elenchi, pipeline, agenda e report.
- Il prefisso vale anche per la numerazione mostrata delle offerte già esistenti.

=> /crmparlist /crmreassign

@@ /crmreassign
### Come fare
Riassegnare solo una parte dei dati, per esempio i lead:
1. Scegli gli utenti **Da** e **A**.
2. Togli la selezione da **Clienti**, **Opportunità** e **Attività da fare**, lasciando solo **Lead**.
3. Premi **Riassegna**.

> Per il tipo scelto si spostano tutti i dati dell'utente, aperti e chiusi; per un singolo cliente o una singola opportunità cambia il responsabile direttamente nella scheda.

### Esempio
Marco lascia l'azienda. L'amministratore sceglie Da: Marco, A: Laura, lascia selezionati clienti, opportunità, lead e attività da fare e preme **Riassegna**: il messaggio indica per esempio "Riassegnati: clienti 34, opportunità 12, lead 9, attività 21.".

### Attenzione
- "Scegli l'utente da cui riassegnare e un destinatario diverso.": Da è obbligatorio e A deve essere diverso.
- Con A = Nessuno clienti, opportunità e lead restano senza responsabile, mentre le attività da fare restano a chi le aveva.
- Si spostano solo le attività **Da fare**; quelle fatte o annullate restano all'utente originale.
- La riassegnazione si applica a tutti i dati dell'utente, anche se non attivi.

=> /crmcustomerlist /crmsettings

## Glossario
- **Agenda**: elenco delle attività, diviso in Oggi, Scadute, Prossime, Fatte / annullate.
- **Assegnatario**: l'utente che deve svolgere un'attività (Assegnata a).
- **Cliente condiviso**: cliente senza responsabile, visibile a tutti i commerciali.
- **Contatto principale**: la persona di riferimento principale di un cliente; ce n'è una sola.
- **Fase**: passo della pipeline; può essere aperta, vinta o persa.
- **Lead**: contatto non ancora qualificato, da convertire in cliente.
- **Opportunità**: trattativa con un cliente, con valore, probabilità e fase.
- **Pipeline**: l'insieme delle opportunità disposte per fase.
- **Prospect**: potenziale cliente, stato commerciale predefinito.
- **Responsabile**: il commerciale che segue un cliente, un lead o un'opportunità.
- **Storico**: registro dei cambi di fase e di valore di un'opportunità.
- **Tasso di successo**: vinte diviso (vinte + perse) nel periodo.
- **Valore pesato**: valore × probabilità di un'opportunità.

## Risoluzione dei problemi
### Compare "non trovato o non visibile"
L'elemento è stato cancellato oppure non rientra più nella tua visibilità, per esempio perché il cliente è stato assegnato a un altro commerciale. Chiedi a un amministratore di verificarlo.

### Non riesco a cambiare il responsabile
Un commerciale può solo prendere un elemento senza responsabile o lasciarne uno suo. Per assegnarlo a un collega serve un amministratore, oppure la funzione Riassegnazione.

### Non trovo un cliente nella scelta di un'opportunità o di un'offerta
Sono proposti solo i clienti attivi che non sono soltanto fornitori. Controlla nella scheda del cliente i campi **Attivo** e **Tipo anagrafica**.

### Non vedo i pulsanti PDF e Invia
Compaiono dopo il primo salvataggio dell'offerta: premi **Conferma**.

### Non ricevo i promemoria
Il promemoria arriva solo se sei collegato all'applicazione, se l'attività è Da fare e assegnata a te e se il preavviso nelle Impostazioni non è 0.

### Compare "Il CRM si sta preparando per l'azienda: riprova tra un istante."
Al primo accesso il CRM crea le tabelle predefinite: attendi qualche secondo e ripeti l'operazione.

### Compare "Operazione non consentita nel CRM."
La funzione richiede un ruolo che non hai, per esempio la configurazione (solo CompanyPowerAdmin) o la riassegnazione (solo amministratori).
