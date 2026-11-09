# Manuale delle funzioni di base
Accesso, menu, utenti, aziende, anagrafiche e assistenza

## Introduzione
Questo manuale descrive le funzioni comuni a tutta l'applicazione. È costruito sul **tuo menu**: trovi un capitolo per ogni sezione che vedi e un paragrafo per ogni voce, con la schermata della pagina. Gli applicativi attivi per la tua azienda (presenze, CRM, documentale) hanno ciascuno il proprio manuale, scaricabile dalla sezione **Assistenza**.

L'applicazione è organizzata su più livelli:
- **Centro**: gestisce gli studi e attiva gli applicativi;
- **Studio**: segue le aziende;
- **Azienda**: i suoi amministratori e i suoi dipendenti usano gli applicativi attivi.

### Il menu
Il menu laterale è diviso in sezioni espandibili: **Accesso**, **Amministrazione**, una sezione per ogni applicativo attivo e **Assistenza**. Dopo il login si apre il primo applicativo disponibile. Le sezioni e le sottosezioni si aprono e si chiudono una alla volta; accanto alle sezioni compaiono i contatori delle cose da fare (per esempio i ticket in carico a te).

### Lavori in background
Le operazioni lunghe (export, report, manuali) girano in background: l'avanzamento compare nella **barra dei lavori** in alto nella pagina. Quando un file è pronto premi il suo pulsante per scaricarlo; puoi chiudere la notifica quando non ti serve più.

### Impersonare un utente {SuperUser, PowerAdmin, Admin, DealerPowerAdmin, DealerAdmin, FinancialAdvisorPowerAdmin, FinancialAdvisorAdmin, CompanyPowerAdmin, CompanyAdmin}
Dagli elenchi degli utenti, dei centri, degli studi e delle aziende il pulsante **Impersona** ti fa entrare nell'applicazione "come" un altro utente, per vedere quello che vede o per aiutarlo. In alto compare il percorso degli utenti impersonati: cliccando su un nome torni a quell'utente.
> Le operazioni fatte impersonando un utente restano tracciate con l'utente che ha fatto il login.

### I ruoli dell'azienda {CompanyPowerAdmin, CompanyAdmin}
- **CompanyPowerAdmin**: amministratore principale; configura gli applicativi e il menu degli utenti.
- **CompanyAdmin**: amministratore dell'azienda.
- **User**: dipendente o collaboratore.

Un utente può avere più ruoli nella stessa azienda (per esempio CompanyAdmin e User).

@@ /dashboard
La pagina iniziale dopo il login. Dalla casella **Chiedi all'assistente** puoi scrivere (o dettare) quello che vuoi fare: l'assistente apre la pagina giusta o esegue l'operazione. In alternativa scegli una voce dal menu.

@@ /user
I tuoi dati: nome utente, email e lingua. Da qui cambi anche la **password**.
1. Apri la voce e controlla i dati.
2. Per cambiare la password inserisci quella attuale e quella nuova, poi conferma.
> Se hai dimenticato la password usa **Password dimenticata** nella pagina di login: ricevi un'email con il collegamento per impostarne una nuova.

@@ /companypoweradmin
La pagina riservata al CompanyPowerAdmin dell'azienda, con le informazioni sull'azienda e sugli applicativi attivi.

@@ /companyadmin
La pagina del CompanyAdmin, con le informazioni sull'azienda che amministri.

@@ /usercompanylist
Gli utenti dell'azienda, con i loro ruoli.
1. Con **+** aggiungi un utente: email, nome utente e ruoli.
2. Con **Modifica** cambi ruoli e dati; con **Impersona** entri nell'applicazione come quell'utente.
3. Nelle **Opzioni** dell'elenco attivi **Mostra disabilitati** per vedere anche gli utenti disattivati e, se puoi, **Abilita cancellazioni**.
> La cancellazione mostra prima un riepilogo di cosa verrà eliminato e chiede di scrivere il nome dell'utente per confermare.

@@ /customerlist
I clienti dell'azienda: un'anagrafica unica usata da tutti gli applicativi (commesse delle presenze, opportunità del CRM, fascicolo del cliente nel documentale).
1. Con **+** crei un cliente: ragione sociale, tipo (cliente, fornitore o entrambi), dati fiscali, sede e recapiti.
2. Dopo il primo salvataggio aggiungi le **persone di riferimento**; una può essere il contatto principale.
3. Un cliente **disattivato** resta nello storico ma non viene più proposto nelle scelte: lo vedi con **Mostra disabilitati**.
> Un cliente usato da un applicativo non si può cancellare: il messaggio indica dove è usato. In questo caso disattivalo.

@@ /companymenurestriction
Qui il CompanyPowerAdmin nasconde ai CompanyAdmin e agli User le voci di menu che non devono usare. Le voci di sistema (login, dashboard, dati utente) non si possono nascondere. Le voci nascoste spariscono anche dai manuali di chi non le vede.

@@ /superuser
La pagina del SuperUser, con le informazioni generali dell'installazione.

@@ /dealerlist
I centri. Con **+** ne crei uno nuovo con il suo primo amministratore; con **Modifica** cambi i dati; con **Impersona** entri nel centro come suo amministratore.

@@ /userlist
Tutti gli utenti dell'applicazione, con i collegamenti a centri, studi e aziende. Da qui il SuperUser può eliminare un utente con tutti i collegamenti.

@@ /companymove
Sposta un'azienda da uno studio a un altro, con i suoi utenti e i suoi dati.

@@ /storageplanlist
I pacchetti di spazio per il documentale (per esempio 1, 5 e 20 GB), con il pacchetto predefinito assegnato alle aziende nuove.

@@ /companystoragelist
Lo spazio del documentale usato da ogni azienda: pacchetto, spazio occupato e avvisi. Da qui assegni un pacchetto diverso a un'azienda.

@@ /poweradmin
La pagina del PowerAdmin dell'installazione.

@@ /admin
La pagina degli amministratori dell'installazione.

@@ /dealerpoweradmin
La pagina del DealerPowerAdmin, con le informazioni sul centro.

@@ /dealeradmin
La pagina del DealerAdmin, con le informazioni sul centro.

@@ /userdealerlist
Gli amministratori del centro: aggiungi, modifica, disattiva o impersona.

@@ /financialadvisorlist
Gli studi seguiti dal centro.
1. Con **+** crei uno studio con il suo primo amministratore.
2. Nella scheda dello studio abiliti gli **applicativi** che lo studio può usare.
3. Con **Impersona** entri nello studio per lavorare sulle sue aziende.
> Gli applicativi delle aziende (presenze, CRM, documentale) li attivano solo il SuperUser e il DealerPowerAdmin del centro, dalla scheda dell'azienda.

@@ /financialadvisorpoweradmin
La pagina del FinancialAdvisorPowerAdmin, con le informazioni sullo studio.

@@ /financialadvisoradmin
La pagina del FinancialAdvisorAdmin, con le informazioni sullo studio.

@@ /userfinancialadvisorlist
Gli amministratori dello studio: aggiungi, modifica, disattiva o impersona.

@@ /companylist
Le aziende seguite dallo studio.
1. Con **+** crei un'azienda con il suo primo amministratore: parte la procedura guidata con i dati di base.
2. Con **Modifica** apri la scheda dell'azienda (dati e applicativi attivi).
3. Con **Impersona** entri nell'azienda per lavorare al suo posto.

@@ /ticketlist
Le richieste di assistenza (ticket). Con **+** ne apri una nuova scegliendo il tipo di richiesta, descrivi il problema e allega file se servono. Nel dettaglio segui le risposte, rispondi o chiudi il ticket. Il contatore accanto alla voce indica i ticket in carico a te.

@@ /tickettypelist
I tipi di richiesta dei ticket, con i livelli di assistenza che li gestiscono.

@@ /licensemanagement
Le licenze della tua azienda (o del tuo studio) per Presenze, CRM e Documentale: per ogni funzione il livello (Free, Standard o Pro), se è tua o dello studio, la scadenza e il numero massimo di utenti. Il PowerAdmin sottoscrive, cambia piano, rinnova e attiva il rinnovo automatico; trovi anche lo storico dei pagamenti. Lo studio vede le aziende coperte dalle sue licenze.

@@ /licensecataloglist
I listini delle licenze: per ogni tipo di soggetto (azienda o studio) e funzione, i tre piani Free, Standard e Pro con prezzi mensile e annuale e utenti massimi. Il SuperUser gestisce i listini globali e le impostazioni; un centro abilitato può definire i propri listini.

@@ /aidocumentlist
La documentazione che l'assistente AI usa per rispondere ai ticket: documenti caricati e collegati ai tipi di richiesta.

@@ /tickettypeaisetting
Per ogni tipo di richiesta scegli se e come l'assistente AI propone una risposta ai ticket.
