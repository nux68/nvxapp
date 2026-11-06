# Manuale del documentale
Archivio dei documenti dell'azienda e dei documenti del personale

## Introduzione
Il documentale raccoglie in un unico archivio i documenti dell'azienda (DURC, visure, F24, contratti con i clienti...) e i documenti del personale (cedolini, CU, contratti di lavoro, comunicazioni). Ogni documento ha una **tipologia**, può stare in una **cartella** e può avere più **versioni**.

Le funzioni dipendono dal ruolo:
- **Dipendente** (ruolo User): vede i propri documenti e conferma la presa visione;
- **CompanyAdmin**: gestisce l'archivio aziendale; vede e carica i documenti del personale solo se abilitato;
- **CompanyPowerAdmin**: tutto quanto sopra, sempre abilitato ai documenti del personale, più la configurazione (tipologie, cartelle, abilitazioni, impostazioni, registro accessi).

Le pagine si trovano nella sezione **Documentale** del menu, sottosezione **Documenti**.

> I documenti del personale sono riservati: li vedono solo il dipendente interessato e gli amministratori abilitati. Ogni apertura, download e modifica resta nel registro degli accessi.

## Per il dipendente: i miei documenti {User}
### Consultare i documenti
1. Apri **Documentale > Documenti > I miei documenti**.
2. L'elenco mostra i documenti che ti riguardano, dal più recente; un pallino accanto al titolo indica quelli che non hai ancora aperto.
3. Con **Scarica** salvi il file; con **Apri** vedi i dati del documento e le sue versioni.

Nell'elenco trovi anche i documenti delle cartelle che l'azienda rende visibili a tutti i dipendenti (per esempio le comunicazioni).

### Presa visione
Alcune tipologie (per esempio cedolini, contratti, comunicazioni) richiedono di confermare la lettura. Nella colonna Presa visione compare **Da confermare**.
1. Leggi il documento (scaricalo o aprilo).
2. Premi **Presa visione** e conferma.
3. La data e l'ora della conferma restano registrate e l'azienda le vede nei destinatari del documento.

> Se il documento cambia (nuova versione), la conferma resta legata a quello che hai letto: in caso di dubbi chiedi all'amministrazione.

## Archivio dei documenti {CompanyPowerAdmin, CompanyAdmin}
### Cercare i documenti
Apri **Documentale > Documenti > Archivio**. I filtri permettono di cercare per cartella, tipologia, categoria, dipendente, cliente, anno e per **testo**: la ricerca guarda il titolo, la descrizione e il testo contenuto nei file (PDF, documenti di testo...).

Accanto ai documenti compaiono gli avvisi utili: documenti **in scadenza** o scaduti (per le tipologie con scadenza) e documenti **oltre il periodo di conservazione** della tipologia.

> I documenti oltre la conservazione vengono solo segnalati: non si eliminano mai da soli. Decidi tu se eliminarli.

### Caricare un documento
1. Nell'Archivio premi **Carica documento**.
2. Scegli la **tipologia**: in base alla categoria compaiono i campi giusti (il dipendente per i documenti del personale, il cliente per quelli del cliente).
3. Indica titolo, eventuale descrizione, data, competenza (mese e anno) se la tipologia la prevede, scadenza se la prevede, cartella.
4. Scegli lo **stato**: in **Bozza** il documento non è ancora visibile al dipendente; **Pubblicato** lo rende visibile; **Archiviato** lo conserva senza metterlo in evidenza.
5. Scegli il file e conferma.

Se lo stesso file è già presente in un altro documento ricevi un avviso (il caricamento avviene comunque). Se l'azienda lo ha previsto, il dipendente riceve un'email quando gli viene pubblicato un documento.

**Ruoli:** i documenti aziendali e dei clienti li caricano CompanyAdmin e CompanyPowerAdmin; i documenti del personale solo chi è abilitato a caricarli.

### Il dettaglio del documento
Dall'Archivio apri un documento per:
- modificare i dati (tipologia, titolo, cartella, competenza, scadenza, stato);
- **Scarica** l'ultima versione o una versione precedente;
- caricare una **Nuova versione** (con una nota): le versioni precedenti restano disponibili, salvo il limite impostato nelle impostazioni;
- vedere i **Destinatari**: quando il dipendente ha aperto e scaricato il documento e quando ha confermato la presa visione.

### Cestino
Con **Cestino** il documento esce dall'archivio ma si può ancora recuperare.
1. Nell'Archivio passa alla vista **Cestino**.
2. Usa **Ripristina** per riportare un documento nell'archivio, oppure **Elimina** per cancellarlo definitivamente con i suoi file.
3. **Svuota il cestino** elimina definitivamente tutti i documenti del cestino.

I documenti nel cestino da più giorni di quelli indicati nelle impostazioni si eliminano automaticamente di notte.

## Dipendenti {CompanyPowerAdmin, CompanyAdmin}
La pagina **Documentale > Documenti > Dipendenti** elenca i dipendenti a cui consegnare i documenti del personale. È visibile solo a chi è abilitato ai documenti del personale.
- I dipendenti con l'**icona della chiave** (evidenziati) hanno un account e accedono all'app: sono gli utenti dell'azienda con ruolo User e compaiono qui automaticamente.
- Gli altri sono **solo in anagrafica**: i loro documenti si archiviano, ma non possono consultarli in app finché non ricevono un accesso.

Con **Nuovo dipendente** aggiungi un dipendente senza account (cognome, nome, codice fiscale, matricola, email per gli avvisi); con **Modifica** aggiorni i dati o indichi la data di cessazione. Le opzioni dell'elenco permettono di mostrare anche i dipendenti disattivati.

### Invio a più dipendenti
Per consegnare lo stesso documento a più dipendenti (per esempio una comunicazione o un regolamento):
1. Nella pagina Dipendenti premi **Invia a più dipendenti**.
2. Scegli i dipendenti: puoi cercarli per nominativo, codice fiscale, matricola o utente, usare **Seleziona tutti** (vale per quelli mostrati) e limitare l'elenco a quelli **con account**.
3. Scegli una tipologia **del personale**, indica titolo, descrizione, data, competenza o scadenza se previste, stato.
4. Scegli il file e premi **Invia**.

Per ogni dipendente scelto si crea un documento del personale con la sua copia del file, la sua presa visione (se la tipologia la richiede) e la sua voce nel registro. Al termine compare quanti documenti sono stati creati; se per qualche dipendente non è stato possibile, l'elenco ne spiega il motivo.

> Lo spazio viene controllato prima dell'invio per tutte le copie: se non basta, non si carica nulla.

## Configurazione (CompanyPowerAdmin) {CompanyPowerAdmin}
### Tipologie
**Documentale > Documenti > Tipologie**. Ogni tipologia ha un codice, una descrizione e una **categoria**:
- **Personale**: documenti di un dipendente, riservati;
- **Azienda**: archivio aziendale, per gli amministratori;
- **Cliente**: fascicolo di un cliente dell'anagrafica comune.

Per ogni tipologia scegli gli **anni di conservazione** (vuoto = senza limite), se ha una **scadenza**, se richiede la **presa visione** e se ha una **competenza** (mese/anno, per esempio i cedolini). Alla prima attivazione sono già pronte le tipologie più comuni (cedolino, CU, contratto, certificato medico, formazione, comunicazione, F24, DURC, visura, contratto cliente, generico). Una tipologia usata da documenti non si elimina: si disattiva.

### Cartelle
**Documentale > Documenti > Cartelle**. Le cartelle ordinano l'archivio e possono essere annidate. Una cartella **visibile ai dipendenti** rende i documenti aziendali che contiene consultabili, in sola lettura, da tutti i dipendenti (utile per comunicazioni e regolamenti).

### Abilitazioni
**Documentale > Documenti > Abilitazioni** decide chi tra i CompanyAdmin **vede** e chi **carica** i documenti del personale (chi carica vede anche). Il CompanyPowerAdmin è sempre abilitato e compare come riga fissa.

### Impostazioni
**Documentale > Documenti > Impostazioni**:
- **Giorni nel cestino**: dopo quanti giorni i documenti del cestino si eliminano da soli (0 = mai);
- **Versioni da conservare**: quante versioni tenere per ogni documento (0 = tutte); i documenti con presa visione richiesta le conservano sempre;
- **Avvisi email ai dipendenti**: email al dipendente quando riceve un nuovo documento.

### Registro accessi
**Documentale > Documenti > Registro accessi** mostra chi ha caricato, aperto, scaricato, modificato, spostato nel cestino, ripristinato o eliminato ogni documento, con data e ora.

## Spazio {CompanyPowerAdmin, CompanyAdmin}
**Documentale > Documenti > Spazio** mostra lo spazio usato e il pacchetto dell'azienda, il numero di file, la dimensione massima di un file e la soglia di avviso. Quando lo spazio supera la soglia gli amministratori ricevono un avviso; oltre il 100% non si caricano nuovi documenti. Per un pacchetto più grande rivolgiti al centro.
