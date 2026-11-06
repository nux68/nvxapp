# Manuale del documentale
Archivio dei documenti dell'azienda e dei documenti del personale

## Introduzione
Il documentale raccoglie in un unico archivio i documenti dell'azienda (DURC, visure, F24, contratti con i clienti...) e i documenti del personale (cedolini, CU, contratti di lavoro, comunicazioni). Questo manuale è costruito sul **tuo menu**: trovi un capitolo per ogni voce che vedi nella sezione **Documentale**, con la schermata della pagina.

Ogni documento ha una **tipologia**, può stare in una **cartella** e può avere più **versioni**. La tipologia decide come si comporta il documento: quanti anni va conservato, se ha una scadenza, se richiede la presa visione del dipendente e se ha una competenza (mese e anno, come i cedolini).

### I ruoli del documentale {CompanyPowerAdmin, CompanyAdmin}
Le funzioni dipendono dal ruolo:
- **Dipendente** (ruolo User): vede i propri documenti e conferma la presa visione.
- **CompanyAdmin**: gestisce l'archivio aziendale; vede e carica i documenti del personale solo se abilitato.
- **CompanyPowerAdmin**: tutto quanto sopra, sempre abilitato ai documenti del personale, più la configurazione (tipologie, cartelle, permessi, impostazioni, registro accessi).

### Riservatezza e tracciamento {CompanyPowerAdmin, CompanyAdmin}
I documenti del personale sono riservati: li vedono solo il dipendente interessato e gli amministratori abilitati. Ogni apertura, download e modifica resta nel **registro degli accessi**, con data e ora.

> I documenti dei clienti usano l'anagrafica comune (clienti e fornitori): un documento di tipologia "cliente" si collega alla scheda del cliente ed entra nel suo fascicolo.

@@ /doclist
L'archivio è il punto da cui cerchi, carichi e gestisci i documenti dell'azienda e, se sei abilitato, quelli del personale e dei clienti.

I filtri permettono di cercare per cartella, tipologia, categoria, dipendente, cliente e anno, oppure per **testo**: la ricerca guarda il titolo, la descrizione e il testo contenuto nei file (PDF, documenti di testo...). Accanto ai documenti compaiono gli avvisi utili: documenti **in scadenza** o scaduti (per le tipologie con scadenza) e documenti **oltre il periodo di conservazione** della tipologia.

> I documenti oltre la conservazione vengono solo segnalati: non si eliminano mai da soli. Decidi tu se eliminarli.

Per caricare un nuovo documento:
1. Premi **Carica documento**.
2. Scegli la tipologia: compaiono i campi giusti per la categoria (il dipendente per il personale, il cliente per i documenti del cliente).
3. Compila i dati: titolo, descrizione, data, competenza e scadenza se previste, cartella.
4. Scegli lo stato: **Bozza** non è ancora visibile al dipendente, **Pubblicato** lo rende visibile, **Archiviato** lo conserva senza metterlo in evidenza.
5. Carica il file e conferma.

Se lo stesso file è già presente in un altro documento ricevi un avviso, ma il caricamento avviene comunque. Se l'azienda ha attivato gli avvisi email, il dipendente riceve un messaggio quando gli viene pubblicato un documento. I documenti aziendali e dei clienti li caricano CompanyAdmin e CompanyPowerAdmin; i documenti del personale solo chi è abilitato.

Aprendo un documento dall'archivio puoi modificarne i dati, scaricare l'ultima versione o una precedente, caricare una **Nuova versione** con una nota (le versioni precedenti restano disponibili, salvo il limite nelle impostazioni) e vedere i **Destinatari**: quando il dipendente ha aperto e scaricato il documento e quando ha confermato la presa visione.

Con **Cestino** un documento esce dall'archivio ma si può ancora recuperare:
1. Passa alla vista **Cestino**.
2. Usa **Ripristina** per riportarlo nell'archivio.
3. Usa **Elimina** per cancellarlo definitivamente con i suoi file.

Con **Svuota il cestino** elimini definitivamente tutti i documenti del cestino. I documenti rimasti nel cestino oltre i giorni indicati nelle impostazioni si eliminano automaticamente di notte.

@@ /docemployeelist
Qui trovi l'elenco dei dipendenti a cui consegnare i documenti del personale e il loro fascicolo. La pagina è visibile solo a chi è abilitato ai documenti del personale.

- I dipendenti con l'**icona della chiave** (evidenziati) hanno un account e accedono all'app: sono gli utenti dell'azienda con ruolo User e compaiono qui automaticamente.
- Gli altri sono **solo in anagrafica**: i loro documenti si archiviano, ma non possono consultarli in app finché non ricevono un accesso.

Con **Nuovo dipendente** aggiungi un dipendente senza account (cognome, nome, codice fiscale, matricola, email per gli avvisi); con **Modifica** aggiorni i dati o indichi la data di cessazione. Le opzioni dell'elenco permettono di mostrare anche i dipendenti disattivati.

Per consegnare lo stesso documento a più dipendenti (per esempio una comunicazione o un regolamento):
1. Premi **Invia a più dipendenti**.
2. Scegli i dipendenti: cercali per nominativo, codice fiscale, matricola o utente; **Seleziona tutti** vale per quelli mostrati; puoi limitare l'elenco a quelli con account.
3. Scegli una tipologia del personale e compila titolo, descrizione, data, competenza o scadenza se previste, stato.
4. Carica il file e premi **Invia**.

Per ogni dipendente scelto si crea un documento del personale con la sua copia del file, la sua presa visione (se la tipologia la richiede) e la sua voce nel registro. Al termine vedi quanti documenti sono stati creati; se per qualcuno non è stato possibile, l'elenco ne spiega il motivo.

> Lo spazio viene controllato prima dell'invio per tutte le copie: se non basta, non si carica nulla.

@@ /doctypelist
Le tipologie definiscono come si comportano i documenti dell'archivio. Ogni tipologia ha un codice, una descrizione e una **categoria**:
- **Personale**: documenti di un dipendente, riservati;
- **Azienda**: archivio aziendale, per gli amministratori;
- **Cliente**: fascicolo di un cliente dell'anagrafica comune.

Per ogni tipologia scegli gli **anni di conservazione** (vuoto = senza limite), se ha una **scadenza**, se richiede la **presa visione** e se ha una **competenza** (mese/anno, per esempio i cedolini).

Alla prima attivazione sono già pronte le tipologie più comuni (cedolino, CU, contratto, certificato medico, formazione, comunicazione, F24, DURC, visura, contratto cliente, generico). Una tipologia usata da documenti non si elimina: si disattiva.

@@ /docfolderlist
Le cartelle ordinano l'archivio e possono essere annidate, per dare una struttura chiara ai documenti (per esempio per anno, per area o per argomento).

Una cartella **visibile ai dipendenti** rende i documenti aziendali che contiene consultabili, in sola lettura, da tutti i dipendenti: è utile per comunicazioni, regolamenti e modulistica che devono raggiungere tutti.

@@ /docpermissions
Qui decidi quali tra i CompanyAdmin possono lavorare sui documenti del personale, che sono riservati.

Per ogni amministratore scegli chi **vede** e chi **carica** i documenti del personale; chi è abilitato al caricamento vede anche. Il CompanyPowerAdmin è sempre abilitato e compare come riga fissa, non modificabile.

@@ /docsettings
Le impostazioni regolano il comportamento generale dell'archivio:
- **Giorni nel cestino**: dopo quanti giorni i documenti del cestino si eliminano da soli (0 = mai);
- **Versioni da conservare**: quante versioni tenere per ogni documento (0 = tutte); i documenti con presa visione richiesta le conservano sempre;
- **Avvisi email ai dipendenti**: invia un'email al dipendente quando riceve un nuovo documento.

@@ /doclog
Il registro accessi tiene traccia di tutto ciò che succede ai documenti: chi ha caricato, aperto, scaricato, modificato, spostato nel cestino, ripristinato o eliminato ogni documento, con data e ora.

È lo strumento per verificare la riservatezza dei documenti del personale e per ricostruire la storia di un documento in caso di dubbi.

@@ /docstorage
Questa pagina mostra lo spazio usato e il pacchetto dell'azienda, il numero di file, la dimensione massima consentita per un singolo file e la soglia di avviso.

Quando l'occupazione supera la soglia gli amministratori ricevono un avviso; oltre il 100% non si possono caricare nuovi documenti. Per un pacchetto di spazio più grande rivolgiti al centro che segue la tua azienda.

@@ /docmine
Qui trovi i documenti che ti riguardano come dipendente: cedolini, contratti, comunicazioni e gli altri documenti che l'azienda archivia a tuo nome.

L'elenco mostra i documenti dal più recente; un pallino accanto al titolo indica quelli che non hai ancora aperto. Con **Scarica** salvi il file, con **Apri** vedi i dati del documento e le sue versioni. Nell'elenco compaiono anche i documenti delle cartelle che l'azienda rende visibili a tutti i dipendenti (per esempio le comunicazioni).

Alcune tipologie (per esempio cedolini, contratti, comunicazioni) richiedono di confermare la lettura: nella colonna **Presa visione** compare "Da confermare".
1. Leggi il documento, scaricandolo o aprendolo.
2. Premi **Presa visione**.
3. Conferma: la data e l'ora restano registrate e l'azienda le vede tra i destinatari.

> Se il documento cambia (nuova versione), la conferma resta legata a quello che hai letto: in caso di dubbi chiedi all'amministrazione.

## Domande frequenti
### Ho eliminato un documento per sbaglio, posso recuperarlo?
Se è ancora nel cestino sì: apri l'archivio, passa alla vista **Cestino** e usa **Ripristina**. Una volta svuotato il cestino, o passati i giorni impostati, l'eliminazione è definitiva.

### Perché non riesco a caricare un nuovo documento? {CompanyPowerAdmin, CompanyAdmin}
Probabilmente lo spazio dell'azienda ha superato il 100%: controlla la pagina dello spazio di archiviazione. Oltre il limite i caricamenti sono bloccati finché non liberi spazio o non passi a un pacchetto più grande.

### Un dipendente non vede i suoi documenti, perché? {CompanyPowerAdmin, CompanyAdmin}
Controlla che il dipendente abbia un account (icona della chiave) e che i documenti siano in stato **Pubblicato**: in stato Bozza non sono visibili. I dipendenti solo in anagrafica non possono consultare i documenti in app finché non ricevono un accesso.
