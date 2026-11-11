# Manuale di Risorse Umane
Il fascicolo del dipendente

## Introduzione
Il modulo Risorse Umane raccoglie il fascicolo del dipendente oltre le presenze: cedolini e CU, formazione e competenze, onboarding e offboarding. È disponibile quando è attivo per la tua azienda: lo attiva il centro (o lo studio, con la licenza). Funziona anche senza Presenze e Documentale.

Questo manuale è costruito sul **tuo menu**: trovi un capitolo per ogni voce della sezione **Risorse Umane** che vedi.

### Chi fa cosa
- **CompanyPowerAdmin**: configura il modulo e i modelli delle checklist; tutto il resto.
- **CompanyAdmin** (ufficio del personale): dipendenti, cedolini e CU, corsi, piani di formazione e checklist di tutti i dipendenti.
- **Dipendente** (ruolo User): vede i propri cedolini e CU e conferma la presa visione.
- **Studio paghe**: carica e verifica i cedolini delle aziende dello studio con Risorse Umane attivo (non vede il resto del fascicolo).

### Scadenze, notifiche e prese visione
Il modulo usa i servizi comuni: le scadenze dei corsi e delle voci delle checklist finiscono nello **scadenzario** (con il preavviso delle Impostazioni), i nuovi documenti arrivano con una **notifica** e la conferma di lettura passa dalle **prese visione** (con il Documentale attivo diventa una firma del documento).

@@ /hremployeelist
L'elenco dei **Dipendenti** mostra l'anagrafica comune a tutti gli applicativi: codice fiscale, matricola, date di assunzione e cessazione e account. Dal pulsante **+** crei un nuovo dipendente; con **Modifica** apri la scheda.

- **Codice fiscale** e **matricola** servono a dividere il PDF unico dei cedolini: sono unici tra i dipendenti dell'azienda.
- Le **date di assunzione e cessazione** generano le checklist dai modelli predefiniti.
- **Account**: dalla scheda puoi **creare l'account** del dipendente (ruolo User, conta nel limite utenti della licenza) o **disattivarlo** quando esce. Con l'account il dipendente vede i propri cedolini.

@@ /hrpayslips
Nella pagina **Cedolini e CU** l'ufficio del personale carica il **PDF unico** prodotto dal programma paghe.

1. Premi **+** e scegli **Carica il PDF unico**: indica il tipo (cedolino o CU), l'anno e, per i cedolini, il mese (anche la tredicesima), poi scegli il file e premi **Carica e dividi**.
2. La **divisione per dipendente** prosegue in background (l'avanzamento è nella barra dei lavori): ogni pagina va al dipendente il cui **codice fiscale** compare nel testo; se non c'è, si cerca la **matricola** (scritta come "Matricola 123").
3. Le pagine non riconosciute (nessun codice, più dipendenti nella stessa pagina, scansioni senza testo) restano **da verificare**: nel dettaglio le **assegni** al dipendente giusto (ti viene proposto quello con cognome e nome nella pagina) o le **ignori**.
4. Con **Pubblica** i documenti compaiono nell'area dei dipendenti: chi ha l'account riceve la notifica e la richiesta di presa visione.

Nel dettaglio trovi anche il **report delle conferme**: confermati, in attesa e dipendenti senza account. Il PDF di ogni dipendente si scarica con **PDF**. Un caricamento si può cancellare finché non è pubblicato; le pagine assegnate dopo la pubblicazione si pubblicano subito.

@@ /hrmypayslips
In **I miei cedolini** trovi i cedolini e le CU pubblicati per te. Con **PDF** scarichi il documento; con **Confermo di aver letto** confermi la presa visione (con il Documentale attivo la conferma è la firma del documento, dalla pagina **Prese visione**).

@@ /hrcourselist
Il catalogo dei **Corsi** dell'azienda: titolo, ente, durata, **validità** dell'attestato in mesi (vuota = non scade), **competenza** acquisita (le competenze comuni dell'azienda) e corso obbligatorio. Dal pulsante **+** crei un corso; nella scheda di un corso salvato trovi le **edizioni** e con **+** ne aggiungi una.

Nell'edizione aggiungi i **partecipanti** con **+** e ne indichi l'esito: con **Superato** il dipendente acquisisce la competenza del corso e la **scadenza** (conseguimento più la validità) entra nello scadenzario; quando il corso si rinnova, la scadenza precedente si chiude. Dopo il salvataggio carichi l'**attestato** di ogni partecipante.

Un corso con edizioni non si cancella: si disattiva.

@@ /hrplanlist
I **Piani di formazione** dicono, per un anno, quali corsi deve seguire ogni dipendente ed entro quando. L'avanzamento conta le righe con il corso **superato** dal dipendente nell'anno del piano. Dal pulsante **+** crei un piano; nella scheda aggiungi i dipendenti con **+** e scegli il corso di ogni riga.

@@ /hrchecklistlist
Le **Checklist di assunzione e cessazione** si generano da sole dal **modello predefinito** quando un dipendente ha la data di assunzione (o di cessazione) da 30 giorni fa in avanti; se la data cambia e la checklist non è ancora iniziata, si sposta sulla nuova data. Con **+** ne crei una a mano scegliendo il dipendente e il modello.

Nella checklist ogni voce ha la data prevista (con la scadenza nello scadenzario, avvisato il responsabile o gli amministratori):
- spunta la voce quando è fatta;
- per le voci con **documento**: carica il file e premi **Chiedi presa visione**; quando il dipendente conferma, la voce si completa da sola;
- per le voci con un'**azione sull'account**: **Crea account** o **Disattiva account** eseguono l'azione e completano la voce.

Con tutte le voci completate la checklist è completata; **Annullata** la chiude senza scadenze né prese visione in attesa.

@@ /hrchecklisttemplatelist
I **Modelli di checklist** (solo CompanyPowerAdmin) definiscono le voci delle checklist di assunzione e cessazione: descrizione, giorni rispetto alla data (negativi = prima), responsabile, documento da consegnare o firmare, azione sull'account. Il modello **predefinito** di ogni tipo genera le checklist dalle date dei dipendenti. Cancellare un modello non tocca le checklist già generate.

@@ /hrstudiopayslips
Lo **studio paghe** vede in **Cedolini delle aziende** le aziende dello studio con Risorse Umane attivo: scelta l'azienda, carica il PDF unico, verifica le pagine e pubblica come l'ufficio del personale. Lo studio non vede gli altri dati del fascicolo.

@@ /hrsettings
Nella pagina **Impostazioni** il CompanyPowerAdmin imposta i parametri generali del modulo:
- **Giorni di preavviso delle scadenze**: con quanti giorni di anticipo il modulo segnala le scadenze che registra nello scadenzario comune (corsi e voci delle checklist, predefinito 30).
- **Notifiche attive**: se togli la spunta il modulo non invia notifiche (es. "nuovo cedolino"). Ogni utente sceglie comunque come riceverle (nell'app o per email) dalle preferenze delle notifiche.
- **Note**: appunti interni sulla configurazione.

Premi **Conferma** per salvare.
