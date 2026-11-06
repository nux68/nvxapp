# Manuale della gestione presenze
Timbrature, richieste, calendario, note spese e chiusura del mese

## Introduzione
La gestione presenze raccoglie in un'unica applicazione timbrature, ferie e permessi, straordinari, note spese e cartellini mensili. I dipendenti timbrano e fanno le richieste dal browser dello smartphone o del computer, i responsabili approvano, l'amministrazione chiude il mese e prepara l'export per le paghe.

Le funzioni si trovano nella sezione **Presenze** del menu. Le voci dipendono dal ruolo:
- **Dipendente (User)**: timbratura, richieste, calendario, note spese;
- **Responsabile / CompanyAdmin**: approvazioni, calendario dei dipendenti, personale presente, piano ferie, esame delle note spese;
- **CompanyPowerAdmin**: tutto quanto sopra, più la chiusura del mese, gli export e la configurazione.

> Puoi anche usare l'assistente della Dashboard: scrivi o detta "timbra l'entrata", "chiedi ferie dal 10 al 14" oppure "apri il calendario" e l'assistente esegue o apre la pagina giusta.

Le funzioni comuni (accesso, utenti, clienti, assistenza) sono descritte nel **Manuale delle funzioni di base**.

## Dipendente {User}
### Timbratura
1. Apri **Presenze > Timbratura**.
2. Se l'azienda lo richiede, scegli l'**attività** su cui lavori.
3. Premi il pulsante di entrata o di uscita: la timbratura si registra con data e ora correnti.

In alto vedi l'ultima timbratura registrata. Se hai dimenticato una timbratura o ne hai registrata una sbagliata non puoi modificarla direttamente: usa **Richiesta timbratura**.

### Richiesta timbratura
Serve a chiedere l'inserimento o la correzione di una timbratura dimenticata o errata.
1. Apri **Presenze > Richiesta timbratura**.
2. Indica giorno, ora, tipo (entrata o uscita) ed eventualmente l'attività.
3. Conferma: la richiesta va al responsabile del tuo reparto.

### Richiesta giustificativo
Ferie, permessi, malattia, straordinari e le altre causali previste dall'azienda si chiedono con un giustificativo.
1. Apri **Presenze > Richiesta giustificativo**.
2. Scegli il tipo di giustificativo e il periodo (giorni interi oppure ore).
3. Per un'autorizzazione allo straordinario indica le ore in più che chiedi di farti riconoscere in ciascun giorno: le ore lavorate oltre quelle autorizzate non vengono riconosciute.
4. Aggiungi una nota se serve e conferma.

### Le mie richieste
In **Presenze > Richieste** trovi tutte le tue richieste con lo stato (in attesa, approvata, rifiutata). Una richiesta ancora in attesa si può cancellare. Il contatore accanto alla voce di menu indica le richieste in attesa.

### Calendario
**Presenze > Calendario** mostra il tuo mese: timbrature, giustificativi, note spese e le ore calcolate per ogni giorno secondo il tuo profilo orario. Cliccando un giorno vedi il dettaglio.

### Note spese
1. Apri **Nota spese > Richiesta rimborso**.
2. Scrivi la descrizione della nota (es. "Trasferta cliente Milano").
3. Aggiungi una o più spese: data, tipo di spesa, importo e descrizione. Per i rimborsi chilometrici indica i km e l'autovettura.
4. Allega scontrini e fatture: sono ammessi PDF e immagini, fino a 5 MB ciascuno e 15 MB in tutto.
5. Conferma: la nota va in approvazione.

In **Nota spese > Le mie note spese** segui lo stato delle note. Finché una nota non è approvata la puoi modificare (torna in attesa di approvazione) o annullare; se è già approvata puoi chiederne la revoca. L'approvazione della nota approva tutte le sue spese.

## Responsabile e CompanyAdmin {CompanyPowerAdmin, CompanyAdmin}
### Approvare le richieste
1. Apri **Presenze > Generale > Richieste**: trovi le richieste dei dipendenti dei reparti che approvi.
2. Apri una richiesta per vederne il dettaglio.
3. Usa **Approva** o **Rifiuta**; ti viene chiesta conferma.

Le richieste arrivano agli approvatori del reparto del dipendente; l'amministratore può approvare per tutti. Il contatore accanto alla voce indica le richieste da esaminare.

### Calendario dei dipendenti
In **Presenze > Generale > Calendario** scegli un dipendente e vedi il suo mese con timbrature, giustificativi e note spese; da qui puoi anche approvare o rifiutare le richieste. Nei giorni fuori dal rapporto di lavoro una richiesta si può solo rifiutare.

### Personale presente
**Personale presente** mostra in tempo reale chi è al lavoro, chi è assente e per quale motivo.

### Piano ferie
**Piano ferie** mostra le assenze programmate di tutti i dipendenti, utile per organizzare turni e periodi di chiusura.

### Esame delle note spese
1. Apri **Nota spese > Esame spese**.
2. Scegli il periodo ed eventualmente i dipendenti.
3. Apri il dettaglio di una nota per vedere spese e allegati, poi approva o rifiuta.

### Utenti del reparto (CompanyAdmin)
**Utenti** elenca i dipendenti dei reparti che amministri, con la possibilità di aprirne la scheda.

## CompanyPowerAdmin: gestione del mese {CompanyPowerAdmin}
### Calendario HR
**Calendario HR** è lo strumento per correggere i dati di qualsiasi dipendente:
1. scegli il dipendente e il mese;
2. clicca su un giorno per aggiungere una timbratura, un giustificativo o una causale, oppure per modificarli o cancellarli;
3. usa **Calcola** per ricalcolare il cartellino del periodo dopo le correzioni.

> Nei giorni fuori dal rapporto di lavoro del dipendente non sono ammessi movimenti: si possono solo cancellare quelli esistenti.

### Quadratura mensile
1. Apri **Quadratura mensile** e scegli il mese.
2. Controlla i cartellini: le anomalie sono evidenziate.
3. Se servono correzioni, sistemale dal Calendario HR e ricalcola il mese.
4. Seleziona i cartellini e approvali in blocco.

> Si approvano solo i cartellini chiusi: un mese in corso resta provvisorio. Dopo l'approvazione i giorni del mese non sono più modificabili. Le ore in più dei cartellini approvati vengono accreditate in banca ore.

### Export causali
**Export causali** produce il file per il programma paghe a partire dai cartellini del mese, secondo il **modello di export** scelto. Il file si prepara in background e compare nella barra dei lavori in alto, con il pulsante per scaricarlo.

### Statistiche attività e report
**Statistiche attività** mostra le ore lavorate per cliente, commessa e attività nel periodo. I report in PDF (attività, piano ferie, quadratura, note spese) si generano anch'essi in background e si scaricano dalla barra dei lavori.

## CompanyPowerAdmin: configurazione {CompanyPowerAdmin}
### Prima configurazione
Al primo accesso il **wizard** prepara l'azienda con causali, giustificativi, orari e parametri già pronti. Dopo il wizard si completano sedi, reparti e dipendenti.

### Configurazione
In **Parametri > Configurazione** si impostano le regole generali dell'azienda: modalità di approvazione delle richieste, cliente e commessa predefiniti e gli altri parametri della gestione presenze.

### Sedi e reparti
- **Sedi**: le sedi dell'azienda, con le loro festività.
- **Reparti**: per ogni reparto si indicano chi lo amministra e chi approva le richieste dei suoi dipendenti.

### Dipendenti
I dipendenti sono gli utenti dell'azienda con ruolo User. Nella loro scheda si indicano il rapporto di lavoro (date di inizio e fine, profilo orario), il reparto e le competenze. È disponibile anche l'**import dei dipendenti** da file, con la mappatura delle colonne assistita dall'AI.

### Commesse, attività e competenze
- **Commesse**: si collegano a un cliente (dall'anagrafica comune **Amministrazione > Anagrafiche > Clienti**) e si dividono in sub commesse.
- **Attività**: le lavorazioni su cui i dipendenti timbrano.
- **Competenze**: le capacità richieste dalle attività. A una sub commessa si possono collegare solo i reparti in cui si svolge almeno una delle sue attività e gli utenti con almeno una delle competenze richieste.

### Orari, profili orari e festività
- **Orari**: le fasce orarie giornaliere.
- **Profili orari**: il ciclo di orari di un dipendente (es. settimana di 5 giorni) e le regole per le ore in più: lavoro supplementare o straordinario, franchigia giornaliera, ore riconosciute solo se autorizzate.
- **Festività**: i giorni festivi, comuni o per sede.

### Giustificativi e causali
- **Causali**: i codici che finiscono nel cartellino e nell'export (ore o importi). Il tipo di una causale già usata non si può cambiare.
- **Giustificativi**: ciò che il dipendente chiede (ferie, permessi, autorizzazione allo straordinario...), collegati alla causale e agli eventuali contatori. Un giustificativo a importo registra solo un importo in euro e non incide su ore, contatori e piano ferie.

### Modelli di export
In **Modelli Export** si definisce il formato del file per il programma paghe. Puoi allegare il tracciato, il manuale o un file di esempio del programma paghe e aggiungere indicazioni (es. "separatore punto e virgola"): l'AI prepara il formato. Con la prova sui dati di un dipendente controlli il risultato prima di usarlo.

### Tipi di spesa
**Tipi spesa** elenca le voci delle note spese (vitto, alloggio, treno, rimborso chilometrico...) con le loro regole.

## Domande frequenti
- **I dipendenti devono installare qualcosa?** No: usano il browser dello smartphone o del computer.
- **Chi approva le richieste?** Gli approvatori del reparto del dipendente; l'amministratore può approvare per tutti.
- **Posso usarlo con il mio programma paghe?** Sì: l'export delle causali si configura con modelli adatti al programma paghe che usi.
- **Una timbratura è sbagliata, come la correggo?** Il dipendente fa una richiesta timbratura; l'amministratore può correggerla direttamente dal Calendario HR.
