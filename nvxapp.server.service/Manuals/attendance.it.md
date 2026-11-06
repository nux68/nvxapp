# Manuale della gestione presenze
Timbrature, richieste, calendario, note spese e chiusura del mese

## Introduzione
La gestione presenze raccoglie in un'unica applicazione timbrature, ferie e permessi, straordinari, note spese e cartellini mensili. I dipendenti timbrano e fanno le richieste dal browser dello smartphone o del computer, i responsabili approvano, l'amministrazione chiude il mese e prepara l'export per le paghe.

Questo manuale è costruito sul **tuo menu**: trovi un capitolo per ogni voce che vedi nella sezione **Presenze**, con la schermata della pagina. Le voci disponibili dipendono dal tuo ruolo.

Le funzioni comuni (accesso, utenti, clienti, assistenza) sono descritte nel **Manuale delle funzioni di base**.

### I ruoli {CompanyPowerAdmin, CompanyAdmin}
- **Dipendente (User)**: timbra, invia richieste, consulta il proprio calendario e gestisce le note spese.
- **Responsabile / CompanyAdmin**: approva le richieste, consulta il calendario dei dipendenti, vede il personale presente e il piano ferie, esamina le note spese.
- **CompanyPowerAdmin**: fa tutto quanto sopra, più la chiusura del mese, gli export, i report e la configurazione dell'applicativo.

Un utente può avere più ruoli nella stessa azienda (per esempio CompanyAdmin e User): in questo caso vede sia le voci del dipendente sia quelle dell'amministrazione.

### L'assistente
Dalla casella **Chiedi all'assistente** della Dashboard puoi scrivere o dettare quello che vuoi fare: "timbra l'entrata", "chiedi ferie dal 10 al 14" oppure "apri il calendario". L'assistente esegue l'operazione o apre la pagina giusta.

### Ore in più e banca ore
Le ore lavorate oltre l'orario previsto diventano lavoro supplementare o straordinario secondo il profilo orario del dipendente e le autorizzazioni. Le ore in più dei cartellini approvati vengono accreditate in **banca ore**. Le regole di calcolo (franchigia giornaliera, soglie, ore riconosciute solo se autorizzate) si impostano nei profili orari.

@@ /timeclockuser
Da qui registri l'entrata e l'uscita. In alto vedi sempre l'ultima timbratura registrata.
1. Scegli l'attività, se l'azienda lo richiede.
2. Premi **Entrata** o **Uscita**.
3. Controlla l'orario registrato.

La timbratura si registra con data e ora correnti. Se hai dimenticato una timbratura o ne hai registrata una sbagliata non puoi modificarla direttamente: usa la **Richiesta timbratura**.

@@ /requestlistuser
L'elenco di tutte le tue richieste (giustificativi, timbrature, correzioni) con il loro stato: **in attesa**, **approvata** o **rifiutata**. Apri una richiesta per vederne il dettaglio.

Una richiesta ancora in attesa si può cancellare. Il contatore accanto alla voce di menu indica quante richieste sono in attesa.

@@ /usertimesheet
Il tuo cartellino mensile. Mostra, giorno per giorno, timbrature, giustificativi, note spese e le ore calcolate secondo il tuo profilo orario.

Scorri i mesi con le frecce e clicca un giorno per aprirne il dettaglio. È lo strumento con cui controlli che le tue presenze e le tue assenze siano corrette prima della chiusura del mese.

@@ /requestjustificationuser
Qui chiedi ferie, permessi, malattia, straordinari e le altre causali previste dall'azienda.
1. Scegli il tipo di giustificativo.
2. Indica il periodo, a giorni interi o a ore.
3. Aggiungi una nota, se serve.
4. Conferma l'invio.

Per un'autorizzazione allo straordinario indica le ore in più che chiedi di farti riconoscere in ciascun giorno: le ore lavorate oltre quelle autorizzate non vengono riconosciute. La richiesta va agli approvatori del tuo reparto.

@@ /requestclockinguser
Serve a chiedere l'inserimento o la correzione di una timbratura dimenticata o errata.
1. Indica il giorno.
2. Indica l'ora e il tipo, entrata o uscita.
3. Scegli l'attività, se serve.
4. Conferma l'invio.

La richiesta va al responsabile del tuo reparto, che la approva o la rifiuta. Una volta approvata, la timbratura entra nel cartellino.

@@ /notaspesarequestuser
Da qui prepari e invii una nuova nota spese per il rimborso.
1. Scrivi la descrizione della nota, ad esempio "Trasferta cliente Milano".
2. Aggiungi le spese: data, tipo, importo e descrizione.
3. Allega scontrini e fatture.
4. Conferma per mandare la nota in approvazione.

Per i rimborsi chilometrici indica i km e l'autovettura. Gli allegati ammessi sono PDF e immagini, fino a 5 MB ciascuno e 15 MB in tutto.

@@ /notaspeseuser
L'elenco delle tue note spese con lo stato di ciascuna. Apri una nota per vedere spese e allegati.

Finché una nota non è approvata la puoi modificare (torna in attesa di approvazione) o annullare; se è già approvata puoi chiederne la revoca. L'approvazione della nota approva tutte le sue spese.

@@ /requestlistadmin
Le richieste dei dipendenti dei reparti che approvi, in attesa del tuo esame.
1. Apri una richiesta per vederne il dettaglio.
2. Premi **Approva** oppure **Rifiuta**.
3. Conferma la scelta.

Le richieste arrivano agli approvatori del reparto del dipendente; l'amministratore può approvare per tutti. Il contatore accanto alla voce indica le richieste da esaminare.

@@ /poweradmintimesheet
Il Calendario HR è lo strumento per correggere i dati di qualsiasi dipendente.
1. Scegli il dipendente e il mese.
2. Clicca un giorno per aggiungere o modificare timbrature, giustificativi e causali.
3. Premi **Calcola** per ricalcolare il cartellino del periodo.

Nei giorni fuori dal rapporto di lavoro del dipendente non sono ammessi nuovi movimenti: si possono solo cancellare quelli esistenti.

@@ /quadraturamensile
La chiusura del mese: qui controlli i cartellini e li approvi in blocco.
1. Scegli il mese.
2. Controlla i cartellini: le anomalie sono evidenziate.
3. Correggi dal Calendario HR e ricalcola, se serve.
4. Seleziona i cartellini e approvali.

Si approvano solo i cartellini chiusi: un mese in corso resta provvisorio. Dopo l'approvazione i giorni del mese non sono più modificabili e le ore in più vengono accreditate in banca ore.

@@ /exportcausali
Produce il file per il programma paghe a partire dai cartellini del mese, secondo il **modello di export** scelto.

Scegli il mese e il modello, poi avvia l'export. Il file si prepara in background e compare nella barra dei lavori in alto, con il pulsante per scaricarlo.

@@ /activitystatistics
Mostra le ore lavorate per cliente, commessa e attività nel periodo scelto. È utile per analizzare l'impegno sulle commesse e per la rendicontazione.

I dati si possono filtrare per periodo e per dipendente; i report in PDF si generano in background e si scaricano dalla barra dei lavori.

@@ /presentstaff
Mostra in tempo reale chi è al lavoro, chi è assente e per quale motivo. È il quadro immediato della situazione del personale nel momento in cui apri la pagina.

@@ /vacationplan
Mostra le assenze programmate di tutti i dipendenti su un calendario unico. È utile per organizzare turni, coperture e periodi di chiusura evitando sovrapposizioni.

@@ /admintimesheet
Il calendario dei dipendenti dei reparti che amministri. Scegli un dipendente e vedi il suo mese con timbrature, giustificativi e note spese.

Da qui puoi anche approvare o rifiutare le sue richieste. Nei giorni fuori dal rapporto di lavoro una richiesta si può solo rifiutare.

@@ /userdepartmentlist
L'elenco dei dipendenti dei reparti che amministri. Da qui apri la scheda di un dipendente per consultarne i dati.

@@ /notaspeseadmin
L'esame delle note spese dei dipendenti.
1. Scegli il periodo e, se serve, i dipendenti.
2. Apri il dettaglio di una nota per vedere spese e allegati.
3. Approva oppure rifiuta la nota.

Approvando una nota approvi tutte le spese che contiene.

@@ /justificationlist
I giustificativi sono ciò che il dipendente chiede: ferie, permessi, autorizzazione allo straordinario e simili. Ogni giustificativo è collegato a una causale e agli eventuali contatori.

Un giustificativo a importo registra solo un importo in euro e non incide su ore, contatori e piano ferie. Da qui crei, modifichi e disattivi i giustificativi disponibili in azienda.

@@ /causalilist
Le causali sono i codici che finiscono nel cartellino e nell'export, a ore o a importo. Sono la base su cui si appoggiano i giustificativi e l'export per le paghe.

Il tipo di una causale già usata non si può cambiare. Da qui gestisci l'elenco delle causali dell'azienda.

@@ /companycfgedit
Qui imposti le regole generali della gestione presenze: modalità di approvazione delle richieste, cliente e commessa predefiniti e gli altri parametri dell'applicativo.

Sono le impostazioni che valgono per tutta l'azienda e che determinano il comportamento di timbrature, richieste e cartellini.

@@ /orarilist
Gli orari sono le fasce orarie giornaliere: definiscono le ore previste in una giornata di lavoro. Sono i mattoni con cui si costruiscono i profili orari.

Da qui crei e modifichi gli orari usati in azienda.

@@ /profiliorarilist
I profili orari descrivono il ciclo di orari di un dipendente, ad esempio la settimana di 5 giorni, e le regole per le ore in più: lavoro supplementare o straordinario, franchigia giornaliera, ore riconosciute solo se autorizzate.

Il profilo orario si assegna al dipendente nella sua scheda e guida il calcolo del cartellino.

@@ /festivitalist
Le festività sono i giorni festivi, comuni a tutta l'azienda oppure specifici di una sede. Entrano nel calcolo del cartellino.

Da qui gestisci il calendario delle festività e lo colleghi, dove serve, alle singole sedi.

@@ /exportcaulist
I modelli di export definiscono il formato del file per il programma paghe.

Puoi allegare il tracciato, il manuale o un file di esempio del programma paghe e aggiungere indicazioni (ad esempio "separatore punto e virgola"): l'AI prepara il formato. Con la prova sui dati di un dipendente controlli il risultato prima di usarlo nell'export mensile.

@@ /tipospesalist
I tipi di spesa sono le voci delle note spese: vitto, alloggio, treno, rimborso chilometrico e simili, ciascuno con le proprie regole.

Da qui definisci quali tipi di spesa i dipendenti possono usare quando compilano una nota.

@@ /commessalist
Le commesse rappresentano i lavori dell'azienda e si collegano a un cliente dell'anagrafica comune (**Amministrazione > Anagrafiche > Clienti**). Si dividono in sub commesse.

A una sub commessa si possono collegare solo i reparti in cui si svolge almeno una delle sue attività e gli utenti con almeno una delle competenze richieste. Da qui gestisci commesse e sub commesse su cui i dipendenti timbrano.

@@ /departmentlist
I reparti organizzano i dipendenti dell'azienda. Per ogni reparto si indica chi lo amministra e chi approva le richieste dei suoi dipendenti.

I reparti determinano a chi arrivano le richieste e chi può vedere e correggere i cartellini dei dipendenti.

@@ /azsedilist
Le sedi sono le sedi di lavoro dell'azienda, ciascuna con le proprie festività. Da qui crei e modifichi le sedi e colleghi loro il calendario delle festività.

@@ /activitylist
Le attività sono le lavorazioni su cui i dipendenti timbrano. Si collegano alle sub commesse e richiedono determinate competenze.

Da qui gestisci l'elenco delle attività disponibili per la timbratura.

@@ /competencelist
Le competenze sono le capacità richieste dalle attività. Servono a collegare le persone giuste alle lavorazioni giuste: a una sub commessa si possono collegare solo gli utenti con almeno una delle competenze richieste.

Da qui gestisci l'elenco delle competenze dell'azienda.

## Prima configurazione {CompanyPowerAdmin}
Al primo accesso il **wizard** prepara l'azienda con causali, giustificativi, orari e parametri già pronti. Dopo il wizard si completano sedi, reparti e dipendenti.

I dipendenti sono gli utenti dell'azienda con ruolo User. Nella loro scheda si indicano il rapporto di lavoro (date di inizio e fine, profilo orario), il reparto e le competenze. È disponibile anche l'**import dei dipendenti** da file, con la mappatura delle colonne assistita dall'AI.

## Domande frequenti
- **I dipendenti devono installare qualcosa?** No: usano il browser dello smartphone o del computer.
- **Chi approva le richieste?** Gli approvatori del reparto del dipendente; l'amministratore può approvare per tutti.
- **Posso usarlo con il mio programma paghe?** Sì: l'export delle causali si configura con modelli adatti al programma paghe che usi.
- **Una timbratura è sbagliata, come la correggo?** Il dipendente fa una richiesta timbratura; l'amministratore può correggerla direttamente dal Calendario HR.
- **Perché non riesco ad approvare un mese?** Si approvano solo i cartellini chiusi: un mese ancora in corso resta provvisorio.
