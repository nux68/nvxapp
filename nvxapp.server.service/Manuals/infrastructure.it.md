# Manuale delle funzioni di base
Accesso, menu, assistenza, utenti, aziende e anagrafiche comuni

## Introduzione
Questo manuale descrive le funzioni comuni a tutta l'applicazione, disponibili qualunque siano gli applicativi attivati per l'azienda (presenze, CRM, documentale). Gli applicativi hanno ciascuno il proprio manuale, scaricabile dalla sezione **Assistenza** del menu.

L'applicazione è organizzata su più livelli:
- **Centro**: gestisce gli studi e attiva gli applicativi;
- **Studio**: gestisce le aziende che segue;
- **Azienda**: i suoi amministratori e i suoi dipendenti usano gli applicativi attivi.

> Le voci di menu che vedi dipendono dai tuoi ruoli e dagli applicativi attivi per la tua azienda: se una funzione descritta qui non compare, non è prevista per il tuo ruolo.

## Accesso all'applicazione
### Primo accesso e login
1. Apri l'indirizzo dell'applicazione: prima del login vedi le pagine pubbliche del sito.
2. Scegli **Login** e inserisci nome utente (o email) e password.
3. Dopo il login si apre la **Dashboard** con l'assistente e il menu laterale.

Se hai dimenticato la password usa **Password dimenticata** nella pagina di login: ricevi un'email con il collegamento per impostarne una nuova.

### Dati utente e cambio password
Dalla sezione **Accesso** del menu apri **Dati utente** per controllare i tuoi dati e cambiare la password.

### Uscita
Usa **Logout** nella sezione **Accesso**. Se stai impersonando un altro utente, al posto di Logout trovi il percorso degli utenti impersonati (vedi più avanti).

## Il menu
Il menu laterale è diviso in sezioni espandibili:
- **Accesso**: dashboard, dati utente, logout;
- **Amministrazione**: le funzioni comuni (utenti, aziende, anagrafiche, configurazione);
- una sezione per ogni **applicativo attivo** (Presenze, CRM, Documentale), con le sue sottosezioni;
- **Assistenza**: ticket, documentazione e manuali.

Dopo il login si apre il primo applicativo disponibile con la sua prima sottosezione. Le sezioni e le sottosezioni si aprono e si chiudono una alla volta. Accanto alle sezioni compaiono i contatori delle cose da fare (per esempio i ticket assegnati a te).

## Utenti e ruoli {SuperUser, PowerAdmin, Admin, DealerPowerAdmin, DealerAdmin, FinancialAdvisorPowerAdmin, FinancialAdvisorAdmin, CompanyPowerAdmin, CompanyAdmin}
### I ruoli
- **CompanyPowerAdmin**: amministratore principale dell'azienda; configura gli applicativi e il menu degli utenti.
- **CompanyAdmin**: amministratore dell'azienda.
- **User**: dipendente o collaboratore che usa gli applicativi.

Un utente può avere più ruoli nella stessa azienda (per esempio CompanyAdmin e User).

### Gestire gli utenti dell'azienda {CompanyPowerAdmin}
1. Apri **Amministrazione > Generale > Utenti**.
2. Usa il pulsante **+** per aggiungere un utente e indica email, nome utente e ruoli.
3. Con **Modifica** cambi ruoli e dati; con **Impersona** entri nell'applicazione come quell'utente (vedi sotto).

Nelle opzioni dell'elenco trovi **Mostra disabilitati** per vedere anche gli utenti disattivati.

### Impersonare un utente
Gli amministratori possono entrare nell'applicazione "come" un altro utente per verificare cosa vede o per aiutarlo. In alto compare il percorso degli utenti impersonati: cliccando su un nome torni a quell'utente.

> Le operazioni fatte impersonando un utente restano tracciate con l'utente che ha fatto il login.

## Menu degli utenti {CompanyPowerAdmin}
Il CompanyPowerAdmin può nascondere ai CompanyAdmin e agli User le voci di menu che non devono usare: **Amministrazione > Parametri > Menu utenti**. Le voci di sistema (login, dashboard, dati utente) non si possono nascondere.

## Anagrafiche comuni {CompanyPowerAdmin}
### Clienti
I clienti dell'azienda sono un'anagrafica unica, usata da tutti gli applicativi (commesse delle presenze, opportunità del CRM, fascicolo del cliente nel documentale). Si gestiscono in **Amministrazione > Anagrafiche > Clienti**.

1. Con **+** crei un cliente: ragione sociale, tipo (cliente, fornitore o entrambi), dati fiscali, sede e recapiti.
2. Dopo il primo salvataggio puoi aggiungere le **persone di riferimento** (una può essere il contatto principale).
3. Un cliente **disattivato** resta nello storico ma non viene più proposto nelle scelte; si vede con **Mostra disabilitati**.

> Un cliente usato da un applicativo (commesse, opportunità, offerte, documenti) non si può cancellare: il messaggio indica dove è usato. In questo caso disattivalo.

## Assistenza
### Ticket
Da **Assistenza > Ticket** apri una richiesta di assistenza, segui le risposte e allega file. Il contatore accanto alla voce indica i ticket in carico a te.

### Manuali
Da **Assistenza** scarichi i manuali in PDF: questo manuale e quelli degli applicativi attivi. Il PDF si prepara in pochi secondi e compare nella barra dei lavori in alto nella pagina, con il pulsante per scaricarlo.

## Lavori in background
Le operazioni lunghe (export, report, manuali) girano in background: l'avanzamento compare nella barra dei lavori in alto nella pagina. Quando un file è pronto, clicca sul pulsante di download; puoi chiudere la notifica quando non ti serve più.

## Gestione dei centri {SuperUser}
Dalla sezione **Amministrazione > SuperUser** il SuperUser gestisce l'intera installazione:
- **Centri**: crea e modifica i centri; da qui puoi entrare in un centro impersonandone un amministratore;
- **Utenti**: tutti gli utenti dell'applicazione;
- **Sposta azienda**: sposta un'azienda da uno studio a un altro;
- **Pacchetti di spazio** e **Spazio documenti**: pacchetti disponibili e spazio assegnato alle aziende.

## Centro: studi e applicativi {SuperUser, DealerPowerAdmin, DealerAdmin}
Dalla sezione **Amministrazione > Centro**:
- **Utenti Centro**: gli amministratori del centro;
- **Studi**: crea e modifica gli studi; dalla scheda dello studio abiliti gli applicativi allo studio e da qui entri negli studi e nelle loro aziende;
- **Spazio documenti**: spazio del documentale delle aziende del centro.

> Gli applicativi di un'azienda (presenze, CRM, documentale) li attivano solo il SuperUser e il DealerPowerAdmin del centro, dalla scheda dell'azienda.

## Studio: aziende {FinancialAdvisorPowerAdmin, FinancialAdvisorAdmin}
Dalla sezione **Amministrazione > Studio**:
- **Utenti Studio**: gli amministratori dello studio;
- **Aziende**: crea le aziende seguite dallo studio e i loro amministratori; con **Impersona** entri nell'azienda per lavorare al suo posto.

## Applicativi e spazio {SuperUser, DealerPowerAdmin, DealerAdmin, CompanyPowerAdmin, CompanyAdmin}
Gli applicativi di un'azienda li attiva il centro (o il SuperUser). Il documentale usa uno spazio su disco a pacchetti: l'uso e il pacchetto si vedono nella pagina **Spazio** del documentale; quando lo spazio sta per finire arriva un avviso agli amministratori.
