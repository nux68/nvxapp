## Primi passi
Questo capitolo indica da dove cominciare in base al tuo ruolo. Le voci del documentale si trovano nella sezione **Documenti** del menu; la configurazione è nella sezione **Configurazione**, visibile solo al CompanyPowerAdmin.

### Se sei un dipendente {User}
1. Apri **I miei documenti** dal menu.
2. Cerca i documenti con il pallino accanto al titolo: sono quelli che non hai ancora aperto.
3. Premi **Scarica** per salvare il file oppure **Apri** per vederne i dati.
4. Se nella colonna **Presa visione** compare "Da confermare", leggi il documento e premi **Presa visione**.
5. Se nella colonna **Firma** compare "Da firmare", premi prima **Scarica** e leggi il documento, poi premi **Firma** e conferma.

> Vedi solo i documenti pubblicati a tuo nome: i documenti in bozza e quelli degli altri dipendenti non ti sono mai mostrati.

### Se sei un amministratore {CompanyAdmin, CompanyPowerAdmin}
1. Apri **Archivio** e prova i filtri: cartella, tipologia, anno e **Cerca nel testo**.
2. Usa **Carica documento** per archiviare il primo documento aziendale (per esempio un DURC o una visura).
3. Controlla in **Spazio** quanto spazio usa l'azienda e la dimensione massima di un file.
4. Se devi gestire anche cedolini e documenti del personale, verifica in **Dipendenti** di essere abilitato: senza abilitazione la pagina lo segnala.

### Configurazione iniziale {CompanyPowerAdmin}
Alla prima apertura il documentale prepara da solo tipologie e cartelle predefinite. Prima di iniziare a caricare conviene:
1. Controllare in **Tipologie** gli anni di conservazione, la presa visione e l'invio multiplo delle tipologie del personale.
2. Rivedere in **Cartelle** la struttura proposta (Personale, Amministrazione, Clienti, Comunicazioni) e aggiungere le cartelle che servono.
3. Decidere in **Abilitazioni** quali CompanyAdmin vedono e caricano i documenti del personale.
4. Impostare in **Impostazioni** i giorni del cestino, le versioni da conservare e gli avvisi email.

## Concetti chiave

### Categorie
La categoria della tipologia decide chi vede il documento e quali dati servono.
| Categoria | Per cosa | Chi lo vede | Dato obbligatorio |
|---|---|---|---|
| Personale | Cedolini, CU, contratti di lavoro, comunicazioni al singolo dipendente | Il dipendente (solo se Pubblicato) e gli amministratori abilitati | Dipendente |
| Azienda | DURC, F24, visure, documenti interni | CompanyAdmin e CompanyPowerAdmin | Nessuno oltre al titolo |
| Cliente | Contratti e documenti di un cliente dell'anagrafica comune | CompanyAdmin e CompanyPowerAdmin | Cliente |

### Stati del documento
| Stato | Significato |
|---|---|
| Bozza | Il documento è archiviato ma il dipendente non lo vede; nell'archivio il titolo riporta "(Bozza)". |
| Pubblicato | Il documento è visibile al dipendente interessato. È lo stato proposto per un nuovo documento. |
| Archiviato | Il documento resta consultabile dagli amministratori ma non è più visibile al dipendente. |

### Versioni
Ogni documento ha una versione corrente e può avere versioni precedenti. Il dipendente vede e scarica solo la versione corrente; le versioni precedenti sono visibili solo a chi può modificare il documento. Con l'impostazione **Versioni da conservare** le versioni più vecchie vengono eliminate, tranne nei documenti con presa visione richiesta.

### Presa visione
Se la tipologia **Richiede la presa visione**, il dipendente deve confermare la lettura. Data e ora della conferma restano registrate e l'amministratore le vede tra i **Destinatari** del documento, insieme alla data della prima apertura e dell'ultimo download. Quando si carica una **Nuova versione**, la presa visione e la prima apertura vengono azzerate: il dipendente dovrà confermare di nuovo.

### Firma
La tipologia può proporre **Richiede la firma** (solo per il personale). È solo un valore proposto: quando carichi o modifichi un documento puoi togliere o aggiungere la richiesta di firma per quel documento, anche dopo averlo inviato. Il dipendente firma con **Firma** dopo aver scaricato il documento: è una firma elettronica semplice, che registra data e ora, versione, impronta SHA-256 del file, indirizzo IP e browser. Firmare vale anche come presa visione. L'amministratore vede la firma tra i **Destinatari**. Una **Nuova versione** azzera la firma (va firmata di nuovo), ma le firme precedenti restano nel **Registro accessi** e le versioni dei documenti firmati non vengono mai eliminate.

> La firma elettronica semplice è valutata liberamente dal giudice: va bene per conferme e accettazioni (regolamenti, informative, consegne). Per contratti e atti che richiedono la forma scritta serve una firma avanzata o qualificata.

### Abilitazioni: vede e carica
I documenti del personale sono riservati. Il CompanyPowerAdmin vede e carica sempre; un CompanyAdmin solo se abilitato nella pagina **Abilitazioni**.
- **Vede**: consulta i documenti del personale in sola lettura (senza destinatari e senza versioni precedenti).
- **Carica**: carica, modifica, sposta nel cestino ed elimina i documenti del personale e gestisce l'anagrafica dei dipendenti. Chi carica vede anche.

### Conservazione
Ogni tipologia può avere gli **Anni di conservazione**. Un documento è oltre la conservazione quando sono passati quegli anni dalla data del documento (o dal caricamento, se la data manca): nell'archivio il titolo mostra il simbolo ⏳ e nella pagina del documento compare un avviso. Il documento non viene mai eliminato in automatico.

### Spazio
L'azienda ha un pacchetto di spazio. Ogni caricamento controlla la dimensione massima del singolo file e lo spazio residuo; ogni versione occupa spazio. Lo spazio si libera solo con l'eliminazione definitiva (dal cestino o dalla pulizia automatica delle versioni), non spostando un documento nel cestino.

@@ /doclist
### Campi
Campi della pagina **Documento** (nuovo documento o documento esistente).
| Campo | Descrizione |
|---|---|
| Scegli il file | Solo per un nuovo documento, obbligatorio. Il titolo viene proposto dal nome del file. Formati ammessi: PDF, P7M, immagini (PNG, JPG, GIF, WEBP, TIF, BMP), Word, Excel, PowerPoint, OpenDocument, RTF, TXT, CSV, XML, ZIP, EML, MSG. |
| Tipologia | Obbligatoria. Mostra solo le tipologie attive; quelle del personale solo a chi può caricarle. Determina i campi seguenti. |
| Dipendente / Dipendenti | Solo per la categoria Personale, obbligatorio. Apre il dialog di scelta dei dipendenti. |
| Cliente | Solo per la categoria Cliente, obbligatorio: cliente dell'anagrafica comune. |
| Cartella | Facoltativa; l'elenco mostra il percorso completo delle cartelle annidate. |
| Titolo | Obbligatorio, massimo 200 caratteri. |
| Descrizione | Facoltativa, massimo 1000 caratteri. |
| Data | Data del documento; usata per l'ordinamento, il filtro per anno e il calcolo della conservazione. |
| Mese e Anno | Solo se la tipologia ha una competenza: mese da 1 a 12, anno tra 1900 e 2200. |
| Scadenza | Solo se la tipologia ha una scadenza. |
| Stato | Bozza, Pubblicato (proposto) o Archiviato. |
| Note della versione | Solo per un nuovo documento, facoltative (massimo 500 caratteri): restano sulla prima versione. |

Nell'elenco dell'archivio la colonna **Riferimento** mostra il dipendente, il cliente o la cartella del documento; **Competenza** mostra mese/anno oppure la data del documento.

### Come fare
Caricare i cedolini del mese, uno per dipendente:
1. Premi **Carica documento** e scegli il file del cedolino.
2. Scegli la tipologia **Cedolino** e premi la riga **Dipendente**.
3. Cerca il dipendente per cognome, nome, codice fiscale o matricola e toccalo: il dialog si chiude da solo.
4. Indica **Mese** e **Anno** della competenza e lascia lo stato **Pubblicato**.
5. Premi **Conferma**: il dipendente trova il cedolino in **I miei documenti**.

Consegnare una comunicazione a più dipendenti:
1. Premi **Carica documento**, scegli il file e una tipologia del personale con **Personale invio multiplo** attivo.
2. Premi la riga **Dipendenti**: nel dialog spunta i dipendenti oppure usa **Seleziona tutti**, poi **Conferma**.
3. La riga mostra il nome del dipendente oppure "N dipendenti selezionati".
4. Compila titolo e stato e premi **Conferma**: viene creato un documento per ciascun dipendente con lo stesso file.
5. Se qualche documento non è stato creato, resti sulla pagina e un messaggio indica quanti sono riusciti e il motivo degli altri.

Caricare una nuova versione:
1. Apri il documento dall'archivio con **Apri**.
2. Premi **Nuova versione** e scegli il file aggiornato.
3. La nuova versione diventa quella corrente; le precedenti restano nell'elenco **Versioni** e si scaricano toccandole.

Recuperare un documento spostato nel cestino:
1. Dal menu della pagina passa a **Cestino**.
2. Trova il documento e premi **Ripristina**.
3. Con **Torna all'archivio** riprendi la vista normale.

### Esempio
L'ufficio del personale deve consegnare il nuovo regolamento aziendale a 25 dipendenti e avere la conferma di lettura. Usa la tipologia "Comunicazione al dipendente", che richiede la presa visione, dopo aver attivato **Personale invio multiplo** in **Tipologie**. Carica il PDF una volta, sceglie i 25 dipendenti con **Seleziona tutti** e conferma. Nei giorni seguenti apre i singoli documenti e controlla nei **Destinatari** chi ha già confermato la presa visione.

### Attenzione
- I documenti del personale si caricano e si modificano solo con l'abilitazione **Carica**; altrimenti compare "Per i documenti del personale serve l'abilitazione a caricarli.".
- Una tipologia disattivata non si può usare per nuovi documenti ("La tipologia non è più attiva").
- L'avviso email al dipendente parte solo al caricamento di un documento in stato **Pubblicato**. Se carichi in **Bozza** e pubblichi in seguito, l'email non viene inviata.
- Spostare un documento nel **Cestino** non libera spazio. **Cancella** nella vista Cestino e **Svuota il cestino** eliminano definitivamente documenti e file, senza possibilità di recupero.
- Il cestino è visibile solo a chi può modificare i documenti; **Svuota il cestino** elimina solo i documenti che puoi modificare.
- L'elenco mostra al massimo 500 documenti: se compare l'avviso, affina i filtri.
- La ricerca nel testo trova le parole contenute in PDF, Word (docx), Excel (xlsx), OpenDocument e file di testo. Le immagini, le scansioni senza testo e i vecchi formati .doc e .xls si trovano solo per titolo.
- Chi può solo vedere i documenti del personale li apre in sola lettura: non vede i destinatari né le versioni precedenti.

=> /docemployeelist /doctypelist /docstorage /doclog

@@ /docemployeelist
### Campi
Campi della pagina **Dipendente** (Nuovo dipendente o Modifica).
| Campo | Descrizione |
|---|---|
| Cognome e Nome | Massimo 100 caratteri ciascuno; serve almeno uno dei due. |
| Codice fiscale | Facoltativo; 16 caratteri (o 11 cifre). Viene salvato in maiuscolo e deve essere unico nell'azienda. |
| Matricola | Facoltativa, massimo 20 caratteri, unica nell'azienda. |
| Email personale | Facoltativa, deve essere un indirizzo valido. Usata per gli avvisi quando il dipendente non ha un account. |
| Data di cessazione | Data di fine del rapporto di lavoro. |
| Attivo | Togliendo il segno il dipendente risulta disattivato e si vede solo con **Mostra disabilitati**. |

Nell'elenco la colonna **Account** mostra il nome utente di chi accede all'app, oppure "Senza accesso (solo anagrafica)".

### Come fare
Aggiungere un dipendente senza account:
1. Premi **Nuovo dipendente**.
2. Indica cognome, nome e, se li conosci, codice fiscale e matricola: aiutano a trovarlo nel dialog di scelta.
3. Inserisci l'**Email personale** se vuoi che riceva gli avvisi dei nuovi documenti.
4. Premi **Conferma**.

Registrare la cessazione di un dipendente:
1. Premi **Modifica** sulla riga del dipendente.
2. Indica la **Data di cessazione** e, se non deve più comparire, togli **Attivo**.
3. Premi **Conferma**: i suoi documenti restano in archivio.

### Esempio
Un'azienda senza il modulo presenze inserisce qui i dieci operai che non usano l'app, con codice fiscale e email personale. Quando carica i cedolini li sceglie dal dialog cercandoli per matricola; ogni operaio riceve l'email di avviso, e i documenti saranno già pronti in app il giorno in cui gli verrà dato un accesso.

### Attenzione
- Aggiungere e modificare dipendenti richiede l'abilitazione **Carica**; chi ha solo **Vede** consulta l'elenco senza poterlo modificare.
- Codice fiscale e matricola già usati da un altro dipendente vengono rifiutati con un messaggio che indica il valore duplicato.
- I dipendenti con account sono gli utenti dell'azienda con ruolo User: compaiono da soli, non vanno inseriti a mano.

=> /doclist /docpermissions

@@ /doctypelist
### Campi
Campi della pagina **Tipologia di documento**.
| Campo | Descrizione |
|---|---|
| Codice | Obbligatorio, massimo 20 caratteri; viene salvato in maiuscolo e deve essere unico. |
| Descrizione | Obbligatoria, massimo 100 caratteri: è il nome che compare negli elenchi. |
| Categoria | Personale, Azienda (proposta) o Cliente. |
| Personale invio multiplo | Solo per la categoria Personale. Se attivo, al caricamento si possono scegliere più dipendenti insieme. |
| Anni di conservazione | Facoltativo, almeno 1. Vuoto significa senza limite. |
| Ha competenza (mese/anno) | Nel documento compaiono Mese e Anno (per esempio cedolini, CU, F24). |
| Ha una scadenza | Nel documento compare la data di Scadenza (per esempio DURC, certificati). |
| Richiede la presa visione | Il dipendente deve confermare la lettura dei documenti di questa tipologia. |
| Richiede la firma | Solo per la categoria Personale. Propone la richiesta di firma quando si carica un documento; chi lo invia può toglierla o aggiungerla. |
| Ordine | Posizione della tipologia negli elenchi. |
| Attivo | Una tipologia non attiva non si propone per i nuovi documenti. |

### Come fare
Creare una tipologia per le circolari interne con conferma di lettura:
1. Premi **Nuova tipologia**.
2. Indica codice (per esempio CIRCOLARE) e descrizione.
3. Scegli la categoria **Personale** e attiva **Personale invio multiplo**.
4. Attiva **Richiede la presa visione** e indica gli anni di conservazione.
5. Premi **Conferma**.

### Esempio
Le tipologie predefinite per il personale sono Cedolino (5 anni, competenza, presa visione), Certificazione Unica (10 anni), Contratto di lavoro (10 anni), Certificato medico (5 anni, scadenza), Attestato di formazione (10 anni, scadenza) e Comunicazione al dipendente (5 anni, presa visione). Un'azienda che invia spesso comunicazioni collettive attiva **Personale invio multiplo** solo su "Comunicazione al dipendente", lasciando i cedolini a scelta singola per evitare errori.

### Attenzione
- La categoria non si può cambiare se la tipologia è già usata da documenti, perché deciderebbe chi può vederli.
- **Cancella** elimina la tipologia solo se nessun documento la usa; altrimenti la disattiva. Le tipologie disattivate si rivedono con **Mostra anche i disattivati**.
- Cambiare la presa visione di una tipologia vale per i documenti caricati da quel momento in poi.
- Cambiare **Richiede la firma** di una tipologia non tocca i documenti già caricati: la richiesta si cambia nel singolo documento.

=> /doclist /docfolderlist /docsettings

@@ /docfolderlist
### Campi
Campi della pagina **Cartella**.
| Campo | Descrizione |
|---|---|
| Descrizione | Obbligatoria, massimo 100 caratteri. |
| Cartella superiore | Facoltativa: la cartella dentro cui si trova. Vuota = cartella principale. |
| Ordine | Posizione della cartella negli elenchi. |
| Visibile ai dipendenti | I documenti aziendali pubblicati nella cartella sono consultabili, in sola lettura, anche dai dipendenti. |

### Come fare
Creare una sottocartella per anno:
1. Premi **Nuova cartella**.
2. Scrivi la descrizione, per esempio "2026".
3. Scegli come **Cartella superiore** la cartella "Amministrazione".
4. Premi **Conferma**: nei filtri e nei documenti comparirà come percorso completo.

### Esempio
Per tenere ordinati i documenti contabili l'amministratore crea sotto "Amministrazione" le cartelle "F24" e "DURC". Nell'archivio filtra per cartella e trova subito tutti gli F24 dell'anno.

### Attenzione
- Le cartelle predefinite (Personale, Amministrazione, Clienti, Comunicazioni) non si eliminano.
- Una cartella si elimina solo se è vuota, cioè senza documenti e senza sottocartelle: con documenti **Cancella** non compare, con sottocartelle l'eliminazione viene rifiutata.
- Una cartella non può stare dentro sé stessa o dentro una sua sottocartella.
- **Visibile ai dipendenti** riguarda i documenti aziendali: i documenti del personale restano sempre riservati, in qualunque cartella si trovino.

=> /doclist /doctypelist

@@ /docpermissions
### Campi
| Campo | Descrizione |
|---|---|
| Utente | Nome utente e ruoli dell'amministratore. |
| Vede | Consulta i documenti del personale in sola lettura e vede l'elenco dei dipendenti. |
| Carica | Carica, modifica ed elimina i documenti del personale e gestisce i dipendenti. Attivandolo si attiva anche Vede. |

### Come fare
Abilitare un collega dell'ufficio paghe:
1. Apri **Abilitazioni**.
2. Trova il collega nell'elenco.
3. Attiva **Carica**: la modifica si salva subito, senza bisogno di confermare.

### Esempio
Due CompanyAdmin lavorano in amministrazione: uno prepara i cedolini, l'altro segue solo la contabilità. Al primo si attiva **Carica**, al secondo nessuna abilitazione: continuerà a gestire DURC, F24 e contratti con i clienti senza vedere alcun documento del personale.

### Attenzione
- Si abilitano solo gli amministratori dell'azienda; il CompanyPowerAdmin è sempre abilitato e la sua riga non si modifica.
- Finché **Carica** è attivo, **Vede** resta attivo anche se provi a spegnerlo: per togliere ogni accesso spegni prima Carica.
- Le abilitazioni valgono per tutti i documenti del personale, cedolini compresi: non esistono abilitazioni per singola tipologia o cartella.

=> /docemployeelist /doclog

@@ /docsettings
### Campi
| Campo | Descrizione |
|---|---|
| Giorni nel cestino | Obbligatorio, 0 o più; predefinito 30. 0 = il cestino non si svuota mai da solo. |
| Versioni da conservare | Obbligatorio, 0 o più; predefinito 0 (tutte). |
| Avvisi email ai dipendenti | Attivo in partenza. Funziona solo se l'invio delle email è configurato. |

### Come fare
1. Apri **Impostazioni**.
2. Modifica i valori.
3. Premi **Conferma**.

### Esempio
Con **Giorni nel cestino** a 60 e **Versioni da conservare** a 3, un documento spostato per errore nel cestino si recupera per due mesi, e un listino aggiornato ogni mese conserva solo le ultime tre versioni, liberando spazio.

### Attenzione
- La pulizia del cestino avviene di notte (dopo le 3): l'eliminazione è definitiva.
- La riduzione delle versioni si applica quando si carica una nuova versione del documento, non subito dopo aver cambiato l'impostazione.
- L'email arriva all'**Email personale** del dipendente oppure, se manca, all'email del suo account; senza nessuna delle due l'avviso non parte.

=> /doctypelist /docstorage

@@ /doclog
### Campi
| Campo | Descrizione |
|---|---|
| Data | Data e ora dell'operazione. |
| Utente | Nome utente di chi l'ha eseguita. |
| Azione | Caricamento, Nuova versione, Modifica, Visualizzazione, Download, Presa visione, Cestino, Ripristino, Eliminazione. |
| Dettaglio | Numero di versione e nome del file; per l'eliminazione il titolo del documento. |

### Come fare
Verificare chi ha scaricato un cedolino:
1. Apri **Registro accessi**.
2. Scrivi nel filtro della pagina il nome del file o il nome utente.
3. Leggi le righe con azione **Download** e **Visualizzazione**.

### Esempio
Un dipendente sostiene di non aver mai ricevuto il contratto. Il registro mostra il suo nome utente con un **Download** e una **Presa visione** di quel file, con data e ora.

### Attenzione
- Il registro è riservato al CompanyPowerAdmin e mostra le ultime 2000 operazioni dell'azienda, dalla più recente.
- Il registro mostra le operazioni dei documenti ancora presenti, compresi quelli nel cestino: dopo l'eliminazione definitiva le righe di quel documento non compaiono più. Prima di eliminare un documento importante verifica qui la sua storia.

=> /doclist /docpermissions

@@ /docstorage
### Campi
| Campo | Descrizione |
|---|---|
| Pacchetto | Nome del pacchetto di spazio dell'azienda. |
| Usati | MB usati, MB del pacchetto e percentuale. |
| File | Numero di file, dimensione massima di un singolo file e soglia di avviso. |
| Quasi pieno / Spazio esaurito | Segnale che compare oltre la soglia di avviso o oltre il 100%. |

### Esempio
L'azienda ha un pacchetto da 1 GB con avviso all'80%. Superato l'80% la pagina mostra **Quasi pieno** e arriva l'email di avviso; l'amministratore svuota il cestino e riduce le versioni da conservare prima di chiedere un pacchetto più grande.

### Attenzione
- Un file più grande della dimensione massima viene rifiutato con un messaggio che indica i MB del file e il massimo consentito.
- Con lo spazio esaurito il messaggio indica quanti MB restano: elimina documenti dal cestino o chiedi un pacchetto più grande.
- Con l'invio a più dipendenti lo spazio si conta per ogni copia: 20 dipendenti per un file da 2 MB occupano 40 MB.
- L'email di avviso arriva al CompanyPowerAdmin dell'azienda e al centro che la segue.

=> /docsettings /doclist

@@ /docmine
### Campi
| Campo | Descrizione |
|---|---|
| Titolo | Nome del documento; il pallino indica che non l'hai ancora aperto. |
| Tipologia | Tipo di documento, per esempio Cedolino o Contratto di lavoro. |
| Competenza | Mese/anno di riferimento, oppure la data del documento. |
| Presa visione | "Da confermare" se devi ancora confermare, oppure il segno di spunta con la data della conferma. |

### Esempio
A inizio mese Marco riceve l'email "Nuovo documento: Cedolino". Apre l'app, va in **I miei documenti**, scarica il cedolino di ottobre e preme **Presa visione**: la colonna mostra la spunta con la data di oggi.

### Attenzione
- Se l'azienda carica una nuova versione di un documento, il pallino ricompare e la presa visione torna "Da confermare": va confermata di nuovo.
- La presa visione si può confermare una sola volta per versione e solo per i documenti di cui sei destinatario.
- Vedi e scarichi solo la versione corrente del documento.

=> /doclist

## Glossario
| Termine | Significato |
|---|---|
| Tipologia | Modello che stabilisce categoria, conservazione, scadenza, competenza e presa visione di un documento. |
| Categoria | Personale, Azienda o Cliente: decide chi vede il documento. |
| Competenza | Mese e anno a cui si riferisce un documento, come per i cedolini. |
| Presa visione | Conferma di lettura del dipendente, registrata con data e ora. |
| Destinatario | Dipendente a cui è consegnato il documento; per lui si registrano apertura, download e presa visione. |
| Versione | Uno dei file caricati nel tempo per lo stesso documento. |
| Invio multiplo | Caricamento dello stesso file a più dipendenti, con un documento per ciascuno. |
| Conservazione | Numero di anni dopo i quali il documento viene segnalato come oltre la conservazione. |
| Cestino | Area dei documenti eliminati ma ancora recuperabili. |
| Abilitazione | Permesso di un CompanyAdmin a vedere o caricare i documenti del personale. |
| Registro accessi | Elenco di tutte le operazioni sui documenti. |

## Risoluzione dei problemi

### Nel caricamento non trovo la tipologia che mi serve {CompanyAdmin, CompanyPowerAdmin}
La tipologia potrebbe essere disattivata, oppure è del personale e non hai l'abilitazione **Carica**. Il CompanyPowerAdmin può riattivarla in **Tipologie** o abilitarti in **Abilitazioni**.

### Non riesco a scegliere più dipendenti {CompanyAdmin, CompanyPowerAdmin}
La scelta multipla è disponibile solo per un nuovo documento con una tipologia del personale che ha **Personale invio multiplo** attivo. Per un documento già salvato si può indicare un solo dipendente.

### Il file viene rifiutato {CompanyAdmin, CompanyPowerAdmin}
Controlla il messaggio: tipo di file non ammesso (per esempio un eseguibile), file vuoto, file più grande della dimensione massima o spazio esaurito. La pagina **Spazio** mostra la dimensione massima e lo spazio residuo.

### La ricerca nel testo non trova un documento {CompanyAdmin, CompanyPowerAdmin}
Il testo si legge solo da PDF con testo, Word (docx), Excel (xlsx), OpenDocument e file di testo. Per le scansioni e le immagini cerca per titolo o usa i filtri per tipologia, cartella e anno.

### Il dipendente non ha ricevuto l'email {CompanyAdmin, CompanyPowerAdmin}
L'email parte solo se gli **Avvisi email ai dipendenti** sono attivi, il documento è stato caricato già **Pubblicato** e il dipendente ha un'email personale o un account con email. Un documento pubblicato in un secondo momento non genera l'avviso.

### Non vedo il pulsante Presa visione {User}
Il pulsante compare solo per i documenti la cui tipologia richiede la presa visione e che non hai ancora confermato. Se la colonna mostra la spunta, la conferma è già registrata.
