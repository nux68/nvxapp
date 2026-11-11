## Primi passi
Questo capitolo ti accompagna nelle prime operazioni, in base al tuo ruolo. Trovi solo le parti che riguardano i ruoli che hai.

### Il primo accesso
1. Apri la pagina di login e inserisci **User Name** e **Password** (almeno 3 caratteri), poi premi **Login**.
2. Se il primo accesso è con una password provvisoria, usa il collegamento **Cambia password** della pagina di login per sceglierne una tua (almeno 8 caratteri).
3. Dopo il login si apre la **Dashboard**: scrivi all'assistente cosa vuoi fare oppure scegli una voce dal menu.
4. Dal pulsante rotondo in basso puoi passare dal tema chiaro a quello scuro e dall'italiano all'inglese.

> Il nome utente si scrive esattamente come ti è stato comunicato; più utenti possono avere la stessa email, per questo l'accesso e il recupero della password chiedono sempre il nome utente.

### Se amministri un'azienda {CompanyPowerAdmin}
1. Apri **Utenti** e crea gli utenti dei tuoi collaboratori, scegliendo per ciascuno il ruolo User o CompanyAdmin.
2. Se usi presenze, CRM o documentale, apri **Clienti** e inserisci l'anagrafica dei clienti, che sarà condivisa da tutti gli applicativi.
3. Con **Menu utenti** nascondi ai CompanyAdmin e agli User le voci che non devono usare.
4. Per qualsiasi problema apri un ticket da **Ticket**: arriva al tuo studio.

### Se sei amministratore dell'azienda {CompanyAdmin}
Lavori sugli applicativi attivi per la tua azienda con le voci che il CompanyPowerAdmin ti ha lasciato nel menu. Per le richieste di assistenza usa **Ticket**: puoi aprire una richiesta come amministratore aziendale, che arriva allo studio.

### Se sei un dipendente {User}
Nel menu trovi le funzioni degli applicativi che usi ogni giorno. Per un dubbio o un problema apri un ticket da **Ticket** scegliendo "Dipendente": la richiesta arriva al responsabile della tua azienda.

### Se lavori in uno studio {FinancialAdvisorPowerAdmin, FinancialAdvisorAdmin}
1. Apri **Aziende** e crea le aziende che segui: per ognuna nascono due amministratori (CompanyPowerAdmin e CompanyAdmin).
2. Chiedi al tuo centro di attivare gli applicativi che servono a ciascuna azienda: lo studio non li attiva da solo.
3. Con **Impersona** entri nell'azienda come suo CompanyPowerAdmin per configurarla o aiutarla.
4. In **Utenti Studio** gestisci i colleghi che lavorano con te nello studio.

### Se lavori in un centro {DealerPowerAdmin, DealerAdmin}
1. Apri **Studi** e crea gli studi del centro, ciascuno con i suoi amministratori.
2. Il DealerPowerAdmin abilita gli applicativi degli studi e, impersonando lo studio, attiva gli applicativi delle aziende dalla loro scheda.
3. In **Spazio documenti** controlli lo spazio usato dal documentale delle aziende del centro.
4. In **Utenti Centro** gestisci i colleghi del centro.

### Se sei il SuperUser {SuperUser}
1. Crea i centri da **Centri**; da lì, impersonando, scendi a studi e aziende.
2. Prepara i **Tipi di richiesta** dei ticket e, se usi le risposte automatiche, la **Documentazione AI** e i valori di **Configura risposte AI**.
3. Se è attivo il documentale, controlla le **Impostazioni spazio** (soglia di avviso); lo spazio delle aziende viene dalla licenza Documentale.
4. Con **Sposta azienda** porti un'azienda sotto un altro studio.

## Concetti chiave

### La gerarchia
L'applicazione è un albero di nodi: ogni nodo ha i suoi utenti e vede solo quello che sta sotto di lui.
| Livello | Chi è | Ruoli |
|---|---|---|
| SuperUser | Chi gestisce l'intera installazione | SuperUser |
| DEV | Il gruppo tecnico che riceve i ticket dal SuperUser | PowerAdmin, Admin |
| Centro | Il rivenditore: crea gli studi e attiva gli applicativi | DealerPowerAdmin, DealerAdmin |
| Studio | Lo studio professionale che segue le aziende | FinancialAdvisorPowerAdmin, FinancialAdvisorAdmin |
| Azienda | L'azienda cliente | CompanyPowerAdmin, CompanyAdmin |
| Utente | Il dipendente o collaboratore dell'azienda | User |

### Power e Admin
In ogni nodo ci sono due tipi di amministratore. Il **Power** (DealerPowerAdmin, FinancialAdvisorPowerAdmin, CompanyPowerAdmin) è l'amministratore principale: configura, decide e riceve i ticket non ancora assegnati. L'**Admin** lavora con lui, ma alcune impostazioni le vede in sola lettura. Creando un centro, uno studio o un'azienda nascono sempre due utenti principali, uno per tipo, con nomi ricavati dal nome del nodo (per esempio Rossi_PowerAdmin e Rossi_Admin).
> Gli utenti principali Power non si eliminano singolarmente: escono solo eliminando il loro nodo.

### Impersonare
Impersonando entri nell'applicazione come un altro utente e vedi esattamente il suo menu.
- Da **Centri**, **Studi** e **Aziende** entri come amministratore principale (Power) di quel nodo.
- Da **Utenti** dell'azienda puoi impersonare solo gli utenti con ruolo User, non gli amministratori.
- Puoi scendere più livelli di seguito (centro, poi studio, poi azienda): il percorso in alto mostra la catena e con un clic torni a un livello precedente.
- Le operazioni riservate al SuperUser restano permesse anche mentre impersona, perché si controlla l'utente che ha fatto il login.

### Applicativi
Presenze, CRM e documentale si attivano azienda per azienda dalla scheda dell'azienda. Possono attivarli o disattivarli solo il SuperUser e il DealerPowerAdmin del centro a cui appartiene l'azienda. Disattivando un applicativo l'accesso viene bloccato ma i dati restano: riattivandolo si ritrova tutto. Gli **Applicativi dello studio** sono un'altra cosa: abilitano le funzioni lato studio e non attivano nulla sulle aziende.

### Il pulsante rotondo
In basso nella pagina c'è un pulsante rotondo che si apre in due direzioni.
- Verso l'alto: la chat dell'assistente, il microfono per i comandi vocali, il cambio di tema chiaro/scuro e il cambio di lingua (il pulsante mostra la lingua verso cui passi; la pagina si ricarica).
- Verso sinistra: le azioni della pagina in cui ti trovi, per esempio il **+** per aggiungere un elemento.

### Elenchi
Gli elenchi hanno la casella **Cerca** in alto e, dove serve, le **Opzioni**: **Mostra disabilitati** fa comparire anche le righe disabilitate, **Abilita cancellazioni** fa comparire il pulsante **Cancella** su ogni riga (solo per chi ha il permesso di eliminare). Una riga **Disabilitata** non è cancellata: esce dagli elenchi e chi è disabilitato non riceve più ticket né avvisi.

### Notifiche
- Accanto alla voce **Ticket** il contatore indica i ticket in carico a te; a ogni passaggio di un ticket che ti riguarda compare un avviso con il pulsante per aprirlo.
- I lavori lunghi (export, report, manuali) avvisano nella barra dei lavori quando il file è pronto.
- Quando lo spazio del documentale di un'azienda supera la soglia di avviso e quando arriva al 100%, ricevono un avviso il CompanyPowerAdmin dell'azienda e il DealerPowerAdmin del centro.

@@ /dashboard
### Come fare
1. Premi **Chiedi all'assistente…**: si apre la chat.
2. Scrivi una richiesta in parole tue, per esempio "apri i clienti" oppure "passa al tema scuro".
3. Se l'operazione modifica dei dati, l'assistente mostra un riepilogo e aspetta la tua conferma; per aprire una pagina non chiede conferma.

### Cose che puoi chiedere
- **aiuto** o **cosa sai fare**: l'elenco dei comandi disponibili per i tuoi ruoli, prima quelli della pagina in cui sei.
- **apri** o **vai a** seguito dal nome di una pagina, oppure "apri la scheda" di un elemento indicandone il nome.
- **tema scuro** o **tema chiaro**, **lingua inglese** o **lingua italiana**.
- **annulla** o **ricomincia** per lasciare a metà la richiesta in corso.

### Comandi vocali
Con il microfono del pulsante rotondo l'assistente ascolta in continuo; in alternativa tieni premuto il pulsante **Tieni premuto per parlare**. Dopo un periodo di inattività il microfono si spegne da solo: premi **Riattiva** per riaccenderlo. Le risposte vocali si accendono e spengono dall'intestazione della chat.
> Le operazioni che modificano dei dati non si confermano mai da sole, neppure a voce.

=> /ticketlist /user

@@ /user
### Cambiare la password
La pagina **Cambia password** si apre dal collegamento omonimo della pagina di login e non richiede di essere collegati.
| Campo | Descrizione |
|---|---|
| Nome utente | Il tuo nome utente, obbligatorio. |
| Password attuale | La password che usi oggi, obbligatoria. |
| Nuova password | Obbligatoria, almeno 8 caratteri, diversa da quella attuale. |
| Conferma nuova password | Deve essere uguale alla nuova password. |
Premi **Salva password** e poi accedi con la password nuova.

### Recuperare una password dimenticata
1. Nella pagina di login premi **Password dimenticata?**.
2. Indica il tuo **Nome utente** e premi **Invia link**.
3. Apri l'email ricevuta (guarda anche nella posta indesiderata) e premi **Reimposta la password**.
4. Scegli la nuova password (almeno 8 caratteri), confermala e premi **Salva password**.

### Attenzione
- Il link di recupero vale 24 ore e si usa una volta sola; se è scaduto, chiedine uno nuovo con **Nuovo link**.
- Per non rivelare quali nomi utente esistono, la conferma dell'invio è sempre la stessa: se l'email non arriva, controlla di aver scritto bene il nome utente.
- Nel cambio password, un nome utente sbagliato e una password attuale sbagliata danno lo stesso messaggio.

=> /dashboard

@@ /companypoweradmin
### Cosa puoi fare
Come CompanyPowerAdmin sei l'amministratore principale dell'azienda:
- crei gli utenti e decidi i loro ruoli;
- curi l'anagrafica dei clienti condivisa dagli applicativi;
- scegli quali voci di menu vedono i CompanyAdmin e gli User;
- ricevi i ticket aperti dai dipendenti e quelli non ancora assegnati, e apri ticket verso lo studio;
- gestisci la documentazione AI e le risposte automatiche dell'assistenza per la tua azienda.
> Gli applicativi dell'azienda non li attivi tu: li attivano il SuperUser o il DealerPowerAdmin del centro.

=> /usercompanylist /customerlist /companymenurestriction /ticketlist

@@ /companyadmin
### Cosa puoi fare
Come CompanyAdmin lavori sugli applicativi attivi con le voci che il CompanyPowerAdmin ti ha lasciato. Gestisci i ticket che ti vengono assegnati, apri richieste verso lo studio e consulti in sola lettura la documentazione AI e le impostazioni delle risposte automatiche dell'azienda.

=> /ticketlist /aidocumentlist

@@ /usercompanylist
### Campi
Nella finestra **Inserimento nuovo utente** e nella scheda dell'utente:
| Campo | Descrizione |
|---|---|
| UserName | Nome con cui l'utente accede, obbligatorio, fino a 50 caratteri. Deve essere unico in tutta l'applicazione e non si cambia dopo la creazione. |
| Email | Obbligatoria per un utente nuovo, deve essere un indirizzo valido. Più utenti possono avere la stessa email. |
| Password | Solo per un utente nuovo, obbligatoria. |
| Conferma Password | Deve coincidere con la password. |
| Ruolo | Uno o più ruoli tra User e CompanyAdmin; proposto User. Il CompanyPowerAdmin principale mantiene il suo ruolo. |
| Disabilitato | Toglie l'utente dagli elenchi: non riceve più ticket né avvisi. Lo ritrovi con **Mostra disabilitati**. |

### Come fare
1. Premi **+**; se un applicativo permette l'importazione, scegli **Nuovo utente** oppure **Importa da file**.
2. Compila i campi e premi **Conferma**: si apre la scheda dell'utente appena creato.
3. Comunica all'utente il nome utente e la password provvisoria, e invitalo a cambiarla con **Cambia password** dalla pagina di login.

### Esempio
Entra in azienda Laura Bianchi, che dovrà anche gestire le richieste dei colleghi. Crei l'utente l.bianchi con la sua email, una password provvisoria e i ruoli User e CompanyAdmin: come User usa gli applicativi, come CompanyAdmin può ricevere i ticket che le assegni.

### Attenzione
- Gli amministratori principali dell'azienda non si eliminano: non hanno il pulsante **Cancella**.
- Eliminando un utente lo togli dall'azienda e vengono eliminati i suoi dati negli applicativi dell'azienda; l'utente viene eliminato del tutto solo se non è collegato ad altri nodi, altrimenti resta collegato altrove.
- I ticket aperti dall'utente eliminato vengono eliminati; quelli che aveva in carico tornano al loro livello senza assegnatario.
- Non puoi eliminare il tuo utente, né l'ultimo CompanyPowerAdmin attivo dell'azienda.
- La password scelta qui per un utente nuovo può essere corta; per cambiarla in seguito ne servono almeno 8 caratteri.

=> /companymenurestriction /companylist /ticketlist

@@ /customerlist
### Campi
| Campo | Descrizione |
|---|---|
| Ragione sociale | Obbligatoria, fino a 100 caratteri. |
| Referente | Nome del referente, fino a 100 caratteri. |
| Tipo | Cliente, Fornitore oppure Cliente e fornitore; proposto Cliente. |
| Attivo | Acceso per i clienti nuovi; spento, il cliente resta nello storico ma non si propone nelle scelte. |
| Partita IVA | Facoltativa, esattamente 11 cifre. |
| Codice fiscale | Facoltativo, 16 caratteri (persona) oppure 11 cifre (società). |
| Indirizzo, CAP, Città | Fino a 100, 10 e 50 caratteri. |
| Provincia | Sigla di 2 lettere, per esempio MI. |
| Nazione | Codice di 2 lettere, per esempio IT. |
| Telefono | Fino a 30 caratteri. |
| Email e PEC | Indirizzi validi, fino a 100 caratteri. |
| Codice destinatario (SDI) | Per la fatturazione elettronica: 7 caratteri, 6 per la Pubblica Amministrazione. |
| Note | Testo libero, fino a 500 caratteri. |

### Persone di riferimento
Nella finestra **Persona di riferimento** indichi Cognome, Nome, Ruolo, Telefono, Cellulare, Email, Note e l'interruttore **Contatto principale**. Serve almeno il cognome o il nome. Un cliente ha un solo contatto principale: segnandone uno, gli altri smettono di esserlo.

### Esempio
Inserisci il cliente Ferramenta Verdi Srl, tipo Cliente, con partita IVA, sede a Bergamo (provincia BG, nazione IT) e codice destinatario SDI. Dopo il primo salvataggio aggiungi Mario Verdi come contatto principale e l'ufficio acquisti come seconda persona di riferimento.

### Attenzione
- Modificano l'anagrafica dei clienti solo il CompanyPowerAdmin e il CompanyAdmin; gli applicativi (per esempio il CRM) possono avere pagine proprie con regole proprie.
- Un cliente usato da un applicativo non si cancella: il messaggio indica dove è usato e quante volte (per esempio le commesse). Disattivalo togliendo **Attivo**.
- La voce compare solo se l'azienda ha attivo almeno un applicativo che usa i clienti.

=> /usercompanylist /companymenurestriction

@@ /companymenurestriction
### Come fare
1. Apri **Menu utenti**: per ogni voce trovi un interruttore nella colonna **Admin** e uno nella colonna **Utente**.
2. Spegni l'interruttore delle voci da nascondere a quel ruolo; dove la voce non è prevista per il ruolo l'interruttore non c'è.
3. Premi **Conferma** per salvare.

### Esempio
Un applicativo prevede per i dipendenti una voce che nella tua azienda non si usa: nella colonna **Utente** spegni quella voce e premi **Conferma**. I dipendenti non la vedono più, i CompanyAdmin continuano a vederla.

### Attenzione
- Le voci si possono solo togliere: un ruolo non vedrà mai una voce che il programma non prevede per lui.
- Le pagine tolte non si aprono neppure scrivendo il loro indirizzo nel browser.
- Le voci aggiunte in futuro dagli applicativi sono visibili finché non le togli.
- Solo il CompanyPowerAdmin può cambiare questa configurazione.

=> /usercompanylist /companypoweradmin

@@ /superuser
### Cosa puoi fare
Il SuperUser gestisce l'installazione:
- crea e gestisce i centri e, impersonandoli, studi e aziende;
- gestisce gli utenti PowerAdmin e Admin del livello DEV;
- sposta un'azienda sotto un altro studio;
- imposta la soglia di avviso dello spazio del documentale;
- prepara i tipi di richiesta dei ticket, la documentazione AI e i valori predefiniti delle risposte automatiche;
- apre ticket verso il livello DEV assegnandoli direttamente a un PowerAdmin o Admin.

=> /dealerlist /userlist /companymove /tickettypelist

@@ /dealerlist
### Campi
| Campo | Descrizione |
|---|---|
| Descrizione | Nome del centro, obbligatorio, fino a 20 caratteri. I caratteri speciali vengono tolti alla creazione. |
| Email | Solo alla creazione, obbligatoria: email dei due amministratori principali. |
| Password | Solo alla creazione, obbligatoria: password iniziale dei due amministratori principali. |
| Conferma Password | Deve coincidere con la password. |
| Disabilitato | Toglie il centro dagli elenchi (vale per i suoi amministratori principali). |
Mentre scrivi il nome, un avviso mostra i nomi utente che verranno creati, per esempio Nord_Admin e Nord_PowerAdmin.

### Come fare
1. Premi **+**, compila i campi e premi **Conferma**.
2. Comunica al centro i due nomi utente e la password iniziale.
3. Con **Impersona** entri nel centro come DealerPowerAdmin per creare i suoi studi.

### Attenzione
- I nomi utente sono unici in tutta l'applicazione: scegli un nome di centro che non generi nomi già usati.
- Eliminare un centro elimina tutto il suo ramo: studi, aziende con i dati degli applicativi, documenti, ticket nati nel ramo e gli utenti che non sono collegati altrove. Prima di confermare la finestra mostra il riepilogo e chiede di digitare il nome del centro.
- Solo il SuperUser elimina un centro.

=> /financialadvisorlist /companymove /userlist

@@ /userlist
### Campi
| Campo | Descrizione |
|---|---|
| UserName | Nome utente, obbligatorio, fino a 50 caratteri, unico in tutta l'applicazione. |
| Email | Obbligatoria per un utente nuovo, indirizzo valido. |
| Password e Conferma Password | Solo per un utente nuovo, obbligatorie e uguali. |
| Ruolo | Obbligatorio: PowerAdmin oppure Admin. |

### Come fare
Nell'elenco compaiono gli utenti con ruolo PowerAdmin o Admin, cioè il livello DEV che riceve i ticket dal SuperUser. Premi **+** per crearne uno; gli utenti di centri, studi e aziende si creano dai loro elenchi.

### Attenzione
- Elimina solo il SuperUser, dopo aver attivato **Abilita cancellazioni** nelle **Opzioni**.
- Un utente PowerAdmin non si elimina mai; il SuperUser e il tuo stesso utente non si possono eliminare.
- L'eliminazione è definitiva: i ticket aperti dall'utente vengono eliminati, quelli in carico a lui tornano al livello senza assegnatario.

=> /dealerlist /superuser

@@ /companymove
### Campi
| Campo | Descrizione |
|---|---|
| Azienda da spostare | Obbligatoria. I filtri **Centro** e **Studio** aiutano a trovarla; sotto compare la posizione attuale. |
| Sposta sotto | Il **Centro** e lo **Studio** di destinazione; lo studio è obbligatorio e deve essere diverso da quello attuale. |

### Come fare
1. Scegli l'azienda: la pagina mostra la **Posizione attuale**.
2. Scegli centro e studio di destinazione: la pagina mostra la **Nuova posizione**.
3. Premi **Conferma** e rispondi alla domanda di conferma.

### Cosa succede
- Utenti, applicativi attivi, dati e voci di menu nascoste restano all'azienda.
- Gli utenti del vecchio studio non vedono più l'azienda; quelli del nuovo studio la trovano in **Aziende**.
- I ticket non chiusi dell'azienda passano al nuovo studio (e al nuovo centro se cambia); quelli che erano allo studio o al centro tornano al livello senza assegnatario.

### Attenzione
L'operazione è riservata al SuperUser, anche quando sta impersonando un altro utente.

=> /companylist /financialadvisorlist /ticketlist

@@ /storagesettings
### Campi
Scheda **Impostazioni dello spazio documenti**, in alto lo spazio usato da tutte le aziende:
| Campo | Descrizione |
|---|---|
| Soglia di avviso (%) | Obbligatoria, da 1 a 99; proposta 80. Vale per le aziende che non hanno una soglia propria. |

### Attenzione
- Lo spazio di ogni azienda è quello della sua licenza Documentale (Free, Standard, Pro); senza licenza, o con il sistema licenze spento, lo spazio è illimitato.
- Non c'è un limite sulla dimensione del singolo file: conta solo lo spazio totale.

=> /companystoragelist /licensecataloglist

@@ /companystoragelist
### Campi
Scheda **Spazio documenti dell'azienda**, in alto lo spazio usato, il numero di file e la licenza da cui viene lo spazio:
| Campo | Descrizione |
|---|---|
| Soglia di avviso (%) | Facoltativa, da 1 a 99; vuota vale il valore predefinito. |

### Chi può fare cosa
| Ruolo | Cosa vede | Cosa cambia |
|---|---|---|
| SuperUser | Tutte le aziende | Soglia di avviso |
| DealerPowerAdmin | Le aziende del centro | Soglia di avviso |
| DealerAdmin | Le aziende del centro | Niente |
| Studio | Le aziende dello studio | Niente |

### Esempio
L'azienda Rossi Spa ha la licenza Documentale Free (1 GB), è all'85% e ha ricevuto l'avviso. Passando alla licenza Standard (5 GB) dalla pagina Gestione licenze, l'azienda può continuare a caricare documenti.

### Attenzione
- Oltre il 100% l'azienda non può più caricare documenti finché non libera spazio o non passa a una licenza con più spazio.
- Lo spazio non si cambia da questa pagina: dipende dalla licenza Documentale dell'azienda (o dello studio che la copre).

=> /storagesettings /companylist

@@ /poweradmin
### Cosa puoi fare
Il PowerAdmin fa parte del livello DEV: riceve i ticket che il SuperUser gli assegna o che salgono fino al DEV, li assegna ai colleghi Admin, chiede informazioni e li risolve.

=> /ticketlist /userlist

@@ /admin
### Cosa puoi fare
L'Admin fa parte del livello DEV: lavora i ticket che gli vengono assegnati, aggiunge note, chiede informazioni e li risolve.

=> /ticketlist

@@ /dealerpoweradmin
### Cosa puoi fare
Come DealerPowerAdmin sei l'amministratore principale del centro:
- crei gli studi e abiliti i loro applicativi;
- attivi e disattivi gli applicativi delle aziende dalla loro scheda (entrando nello studio con **Impersona**);
- controlli lo spazio del documentale delle aziende del centro e ne cambi la soglia di avviso;
- gestisci i colleghi del centro, la documentazione AI e le risposte automatiche del centro;
- ricevi i ticket che salgono dagli studi.

=> /financialadvisorlist /userdealerlist /companystoragelist /ticketlist

@@ /dealeradmin
### Cosa puoi fare
Come DealerAdmin lavori con il DealerPowerAdmin: vedi gli studi e lo spazio documenti delle aziende del centro, lavori i ticket che ti vengono assegnati e consulti in sola lettura la documentazione AI del centro. Gli applicativi e le soglie di avviso dello spazio li cambia il DealerPowerAdmin.

=> /financialadvisorlist /ticketlist

@@ /userdealerlist
### Campi
| Campo | Descrizione |
|---|---|
| UserName | Obbligatorio, fino a 50 caratteri, unico in tutta l'applicazione; non si cambia dopo la creazione. |
| Email | Obbligatoria per un utente nuovo, indirizzo valido. |
| Password e Conferma Password | Solo per un utente nuovo, obbligatorie e uguali. |
| Ruolo | Obbligatorio: DealerPowerAdmin oppure DealerAdmin. |
| Disabilitato | Toglie l'utente dagli elenchi del centro: non riceve più ticket né avvisi. |

### Attenzione
- Eliminano gli utenti del centro il SuperUser e il DealerPowerAdmin; gli amministratori principali del centro non si eliminano.
- Il centro deve avere sempre almeno un DealerPowerAdmin attivo.

=> /dealerpoweradmin /financialadvisorlist

@@ /financialadvisorlist
### Campi
| Campo | Descrizione |
|---|---|
| Descrizione | Nome dello studio, obbligatorio, fino a 20 caratteri; i caratteri speciali vengono tolti alla creazione. |
| Email, Password, Conferma Password | Solo alla creazione: email e password iniziale dei due amministratori principali dello studio. |
| Disabilitato | Toglie lo studio dagli elenchi (vale per i suoi amministratori principali). |
| Applicativi dello studio | Abilitano le funzioni lato studio, per esempio l'area studio del documentale. |

### Attenzione
- Gli applicativi dello studio li abilitano solo il SuperUser e il DealerPowerAdmin del centro; il DealerAdmin li vede in sola lettura.
- Abilitare un applicativo per lo studio non lo attiva sulle sue aziende.
- Eliminare uno studio elimina le sue aziende con i loro dati, i documenti, i ticket nati nello studio e gli utenti che non sono collegati altrove. Possono farlo il SuperUser e il DealerPowerAdmin del centro.

### Esempio
Il centro acquisisce lo Studio Neri: il DealerPowerAdmin crea lo studio "StudioNeri" con l'email del titolare, abilita il documentale tra gli applicativi dello studio e comunica i nomi utente StudioNeri_PowerAdmin e StudioNeri_Admin.

=> /companylist /userfinancialadvisorlist /dealerpoweradmin

@@ /financialadvisorpoweradmin
### Cosa puoi fare
Come FinancialAdvisorPowerAdmin sei l'amministratore principale dello studio:
- crei le aziende che segui e, se serve, le elimini;
- entri nelle aziende con **Impersona** per configurarle o assisterle;
- gestisci i colleghi dello studio;
- ricevi i ticket che salgono dalle aziende e li assegni;
- curi la documentazione AI e le risposte automatiche dello studio.

=> /companylist /userfinancialadvisorlist /ticketlist /aidocumentlist

@@ /financialadvisoradmin
### Cosa puoi fare
Come FinancialAdvisorAdmin lavori sulle aziende dello studio, gestisci i ticket che ti vengono assegnati e consulti in sola lettura la documentazione AI e le risposte automatiche dello studio.

=> /companylist /ticketlist

@@ /userfinancialadvisorlist
### Campi
| Campo | Descrizione |
|---|---|
| UserName | Obbligatorio, fino a 50 caratteri, unico in tutta l'applicazione; non si cambia dopo la creazione. |
| Email | Obbligatoria per un utente nuovo, indirizzo valido. |
| Password e Conferma Password | Solo per un utente nuovo, obbligatorie e uguali. |
| Ruolo | Obbligatorio: FinancialAdvisorPowerAdmin oppure FinancialAdvisorAdmin. |
| Disabilitato | Toglie l'utente dagli elenchi dello studio: non riceve più ticket né avvisi. |

### Attenzione
- Eliminano gli utenti dello studio il SuperUser, il FinancialAdvisorPowerAdmin e il DealerPowerAdmin del centro; gli amministratori principali dello studio non si eliminano.
- Lo studio deve avere sempre almeno un FinancialAdvisorPowerAdmin attivo.

=> /financialadvisorpoweradmin /companylist

@@ /companylist
### Campi
| Campo | Descrizione |
|---|---|
| Descrizione | Nome dell'azienda, obbligatorio, fino a 20 caratteri; i caratteri speciali vengono tolti alla creazione. |
| Email | Solo alla creazione, obbligatoria: email del CompanyPowerAdmin e del CompanyAdmin. |
| Password e Conferma Password | Solo alla creazione: password iniziale dei due amministratori. |
| Disabilitato | Toglie l'azienda dagli elenchi (vale per i suoi amministratori principali). |
| Applicativi | Presenze, CRM, Documentale: spuntati e salvati vengono attivati; togliendo la spunta l'accesso si blocca ma i dati restano. |

### Esempio
Lo studio prende in carico la Rossi Spa. Il FinancialAdvisorPowerAdmin crea l'azienda "RossiSpa" con l'email dell'amministratrice: nascono gli utenti RossiSpa_PowerAdmin e RossiSpa_Admin. Poi chiede al centro di attivare le presenze; il DealerPowerAdmin apre la scheda dell'azienda, spunta Presenze e conferma.

### Attenzione
- Gli applicativi li attivano solo il SuperUser e il DealerPowerAdmin del centro: per gli altri la sezione è in sola lettura e un'azienda creata dallo studio nasce senza applicativi.
- I nomi utente generati devono essere unici in tutta l'applicazione: evita nomi di azienda già usati.
- Eliminare un'azienda è definitivo: si eliminano i dati di tutti i suoi applicativi, i documenti, i ticket e gli utenti non collegati altrove. Esporta prima i dati che devi conservare (presenze e paghe hanno obblighi di conservazione). Possono farlo il SuperUser e il FinancialAdvisorPowerAdmin dello studio.

=> /usercompanylist /companymove /companystoragelist

@@ /ticketlist
### Le viste
| Vista | Cosa contiene |
|---|---|
| In carico a me | Ticket assegnati a te o arrivati al tuo livello quando sei il Power del nodo. |
| Aperti da me | Le richieste che hai aperto tu. |
| Altri | I ticket dei livelli sotto di te e quelli che hai gestito in passato. |
Con **Stato** filtri Aperto, In carico, In attesa di informazioni, Risolto o Chiuso. Le righe Aperto e Risolto sono evidenziate.

### Campi del nuovo ticket
| Campo | Descrizione |
|---|---|
| Apri come | Il ruolo con cui apri, se ne hai più di uno: decide a chi arriva la richiesta. |
| Tipo di richiesta | Obbligatorio; propone solo i tipi previsti per il ruolo scelto e per gli applicativi attivi. |
| Assegna a | Solo per il SuperUser: il PowerAdmin o Admin che riceve il ticket. |
| Oggetto | Obbligatorio, fino a 200 caratteri. |
| Descrizione | Obbligatoria, fino a 4000 caratteri. |
| Allegati | Fino a 5 file per volta, ciascuno fino a 10 MB e in totale fino a 20 MB. |
Sono ammessi file PDF, immagini, testo, CSV, JSON, XML, documenti Office e OpenDocument, archivi ZIP e 7Z, email EML e MSG.

### Le azioni sul ticket
| Pulsante | A cosa serve |
|---|---|
| Assegna | Il Power del livello affida il ticket a un collega. |
| Inoltra | Passa il ticket al livello superiore previsto dal tipo di richiesta. |
| Chiedi informazioni | Chiede chiarimenti a chi ha aperto o a un livello intermedio da cui il ticket è passato. |
| Rispondi | Risponde a una richiesta di informazioni. |
| Nota | Aggiunge un testo o un allegato senza cambiare lo stato. |
| Cambia tipo | Cambia il tipo di richiesta. |
| Risolvi | Manda la soluzione a chi ha aperto. |
| Chiudi ticket | Chi ha aperto chiude definitivamente un ticket risolto. |
| Riapri | Chi ha aperto riapre un ticket risolto che non lo soddisfa. |

### Come fare
1. Apri il ticket dall'elenco: in alto vedi stato, livello e utente che lo ha in carico.
2. Nella **Cronologia** leggi ogni passaggio, con testi e allegati da scaricare.
3. Premi il pulsante dell'azione, scrivi il testo e conferma.

### Esempio
Un dipendente apre un ticket "Non vedo le ferie di agosto" come Dipendente: arriva al CompanyPowerAdmin, che lo assegna al CompanyAdmin. Questo chiede informazioni al dipendente, riceve la risposta e risolve. Il dipendente legge la soluzione e chiude il ticket.

### Attenzione
- Un ticket chiuso non si può più riaprire: per tornare all'elenco senza chiuderlo usa **Torna all'elenco**.
- Se un altro utente ha appena lavorato lo stesso ticket, la tua azione viene rifiutata: ricarica la pagina e riprova.
- Se non c'è un livello superiore previsto dal tipo di richiesta, il ticket non si può inoltrare.
- L'unico CompanyPowerAdmin dell'azienda non può aprire ticket come dipendente, perché nessuno potrebbe gestirli.
- Quando il ticket mostra **Risposta automatica in corso**, l'assistente AI sta preparando una risposta; se un operatore agisce prima, la risposta automatica si ferma.

=> /tickettypelist /aidocumentlist /tickettypeaisetting

@@ /tickettypelist
### Campi
| Campo | Descrizione |
|---|---|
| Codice | Obbligatorio, fino a 20 caratteri, diverso da quello degli altri tipi. |
| Descrizione | Obbligatoria, fino a 100 caratteri: è il testo proposto a chi apre il ticket. |
| Applicativo | Generale oppure un applicativo: i tipi di un applicativo si propongono alle aziende e ai dipendenti solo se l'applicativo è attivo. |
| Livelli abilitati | Almeno due, tra SuperUser, DEV, Centro, Studio, Azienda e Utente. |
| Ordine | Posizione nell'elenco dei tipi. |
| Attivo | Un tipo disattivato non si può più scegliere, ma resta sui ticket esistenti. |

### Come funzionano i livelli
Il ticket si apre da un livello abilitato e sale sempre al primo livello abilitato sopra quello corrente; i livelli non abilitati vengono saltati. Un tipo si propone a chi apre da un livello solo se sopra quel livello c'è almeno un altro livello abilitato; per i ticket aperti dal SuperUser servono i livelli SuperUser e DEV.

### Esempio
Il tipo PRES01 "Problemi con le timbrature", applicativo Presenze, con i livelli Utente, Azienda e Studio: il dipendente apre verso l'azienda, l'azienda può inoltrare allo studio, e il ticket non sale oltre.

### Attenzione
I tipi di richiesta li gestisce solo il SuperUser; non si eliminano, si disattivano.

=> /ticketlist /aidocumentlist /tickettypeaisetting

@@ /aidocumentlist
### Campi
Scheda **Documento AI**:
| Campo | Descrizione |
|---|---|
| Titolo | Obbligatorio, fino a 200 caratteri. |
| Descrizione | Fino a 1000 caratteri: a cosa serve il documento, aiuta l'assistente a capire quando usarlo. |
| Tipi di richiesta | Il documento si usa solo per i ticket di questi tipi; nessun tipo vale per tutti. |
| Applicativo | Generale oppure l'applicativo a cui si riferisce il documento. |
| Vale anche per i livelli sotto | Il documento si usa anche per i ticket dei livelli sottostanti. |
| File originale | PDF, Word, OpenDocument, Excel o testo, fino a 10 MB; il suo testo sostituisce il contenuto. |
| Contenuto | Obbligatorio, fino a 100.000 caratteri: è il testo che l'assistente legge. |
| Ordine | Se i documenti sono troppi, l'assistente legge prima quelli con ordine più basso. |
| Attivo | Un documento disattivato non viene usato. |

### Come fare
1. Se gestisci più livelli, scegli il **Livello** in alto.
2. Premi **+** (**Nuovo documento**) e compila titolo e descrizione.
3. Premi **Carica un file** per estrarre il testo da un file, oppure scrivi il contenuto; correggi il testo se serve.
4. Scegli i tipi di richiesta e premi **Conferma**.

### Esempio
Lo studio carica una volta il proprio prontuario sulle buste paga, con **Vale anche per i livelli sotto** acceso e il tipo "Domande sulle buste paga": l'assistente lo usa per rispondere ai ticket di quel tipo che arrivano allo studio e alle sue aziende, se lì l'uso della documentazione dei livelli superiori è attivo.

### Attenzione
- Modificano i documenti il SuperUser e i Power di centro, studio e azienda; gli Admin li vedono in sola lettura.
- Il contenuto dei documenti viene inviato al servizio AI: non caricare dati che non devono uscire dall'azienda.
- I file .doc e .xls del vecchio formato non sono leggibili: salvali come .docx o .xlsx. Un PDF scansionato non contiene testo: scrivi o incolla il testo nel contenuto. Un file protetto da password non è leggibile.
- Oltre 100.000 caratteri dividi il testo in più documenti.
- Eliminando un documento, le risposte già date lo citano ancora per titolo.

=> /tickettypeaisetting /tickettypelist /ticketlist

@@ /tickettypeaisetting
### Campi
| Campo | Descrizione |
|---|---|
| Livello | Se gestisci più livelli, quello da configurare. |
| Personalizza | Solo per un nodo: acceso usi valori tuoi, spento valgono i valori predefiniti del SuperUser. |
| Risposta AI | Se l'assistente risponde ai ticket di quel tipo quando arrivano al livello. |
| Tentativi | Da 1 a 10: quante risposte automatiche prova prima di lasciare il ticket all'operatore. |
| Usa la documentazione dei livelli superiori | Se l'assistente usa anche i documenti dei livelli sopra pubblicati per i livelli sotto; spento usa solo quelli del nodo. |

### Come funziona
Quando un ticket arriva al livello, l'assistente risponde se la risposta AI è attiva per quel tipo e se ci sono documenti utilizzabili. Se trova la risposta, il ticket va a chi l'ha aperto come risolto; se la risposta non soddisfa e il ticket viene riaperto, l'assistente riprova finché restano tentativi, poi il ticket passa all'operatore. Gli operatori vengono avvisati solo quando l'assistente ha finito.

### Esempio
Il SuperUser imposta come predefinito, per il livello Azienda, la risposta AI attiva con 2 tentativi. Un'azienda che preferisce rispondere sempre di persona apre la pagina, accende **Personalizza** sul tipo e spegne **Risposta AI**.

### Attenzione
- I valori predefiniti di ogni livello li decide il SuperUser; per il livello DEV valgono solo quelli.
- Nel nodo modifica solo il Power; gli Admin vedono la pagina in sola lettura.
- L'assistente non risponde mai ai ticket aperti dal SuperUser né a quelli che arrivano al SuperUser.

=> /aidocumentlist /tickettypelist /ticketlist

## Glossario
- **Centro**: il rivenditore che crea gli studi e attiva gli applicativi delle aziende (in inglese Dealer).
- **Studio**: lo studio professionale che segue un gruppo di aziende (in inglese Financial advisor).
- **Azienda**: l'azienda cliente, con i suoi amministratori e dipendenti.
- **Nodo**: un singolo centro, studio o azienda nella gerarchia.
- **DEV**: il livello tecnico degli utenti PowerAdmin e Admin, che riceve i ticket dal SuperUser.
- **Power**: l'amministratore principale di un nodo (DealerPowerAdmin, FinancialAdvisorPowerAdmin, CompanyPowerAdmin).
- **Admin**: l'amministratore che affianca il Power, con alcune impostazioni in sola lettura.
- **Utenti principali**: i due amministratori creati insieme al nodo; non si eliminano singolarmente.
- **Impersonare**: entrare nell'applicazione come un altro utente, vedendo il suo menu.
- **Applicativo**: un modulo attivabile per azienda: presenze, CRM, documentale.
- **Disabilitato**: stato di una riga tolta dagli elenchi senza cancellarla.
- **Ticket**: richiesta di assistenza che passa da un livello all'altro.
- **Tipo di richiesta**: la categoria di un ticket, con i livelli che lo possono aprire e ricevere.
- **Livello abilitato**: un livello che un tipo di richiesta coinvolge.
- **Assegnatario**: l'utente che ha in carico un ticket.
- **Documentazione AI**: i testi che l'assistente usa per rispondere da solo ai ticket.
- **Spazio documenti**: lo spazio massimo del documentale di un'azienda, dato dalla sua licenza Documentale (illimitato senza licenza).
- **Soglia di avviso**: la percentuale di spazio usato oltre la quale arriva un avviso.
- **Barra dei lavori**: la barra in alto con l'avanzamento delle operazioni lunghe e i file da scaricare.
- **Persona di riferimento**: un contatto del cliente; il contatto principale è uno solo.

## Risoluzione dei problemi

### Al login compare "Password errata" o "Nome utente non trovato"
Controlla di scrivere il nome utente esattamente come ti è stato dato (non l'email). Se non ricordi la password usa **Password dimenticata?**; se il problema continua chiedi all'amministratore della tua azienda.

### Il link per reimpostare la password non funziona
Il link vale 24 ore e una sola volta. Se l'hai già usato o è scaduto, torna su **Password dimenticata?** e chiedine uno nuovo.

### Non trovo una voce nel menu
Le voci dipendono dai tuoi ruoli, dagli applicativi attivi per la tua azienda e dalle voci che il CompanyPowerAdmin ha nascosto in **Menu utenti**. Una pagina nascosta non si apre neppure scrivendo l'indirizzo. Chiedi al tuo CompanyPowerAdmin.

### Compare "L'applicativo non è attivo per l'azienda"
L'applicativo è stato disattivato o non è mai stato attivato. Solo il SuperUser o il DealerPowerAdmin del centro possono attivarlo; i dati di un applicativo disattivato restano e tornano visibili alla riattivazione.

### Nel nuovo ticket non ci sono tipi di richiesta
Con il ruolo scelto non ci sono tipi di richiesta attivi che salgono a un livello superiore, oppure i tipi riguardano applicativi non attivi. Prova un altro ruolo in **Apri come** o chiedi al SuperUser di rivedere i tipi.

### "Il ticket è stato modificato da un altro utente"
Qualcuno ha lavorato il ticket mentre lo avevi aperto. Ricarica la pagina, leggi l'ultimo passaggio nella cronologia e ripeti l'azione se serve ancora.

### Un allegato viene rifiutato
Ogni passaggio accetta fino a 5 file, ciascuno fino a 10 MB e in totale 20 MB; i file vuoti e i tipi non ammessi (per esempio i programmi) vengono rifiutati. Comprimi i file in uno ZIP o dividili su più passaggi.

### Non riesco a cancellare un cliente
Il cliente è usato da un applicativo: il messaggio indica dove. Togli **Attivo** nella sua scheda: resta nello storico ma non viene più proposto.

### L'eliminazione viene rifiutata
- Il nome digitato deve coincidere esattamente con quello mostrato nella finestra.
- Gli amministratori principali (Power) non si eliminano da soli: escono solo eliminando il loro nodo.
- Ogni nodo deve conservare almeno un Power attivo: prima assegna il ruolo a un altro utente.
- Non puoi eliminare il tuo utente né il SuperUser.

### Non si caricano documenti per mancanza di spazio
Lo spazio della licenza Documentale dell'azienda è esaurito. Elimina documenti che non servono o passa a una licenza con più spazio dalla pagina Gestione licenze.

### Compare "Si è verificato un errore imprevisto" con un codice
Annota il codice indicato nel messaggio e comunicalo all'assistenza aprendo un ticket: serve a ritrovare l'errore.

### Il manuale non si scarica
Il PDF si prepara in background e compare nella barra dei lavori in alto con il pulsante per scaricarlo. Il manuale di un applicativo non attivo per la tua azienda non è disponibile.
