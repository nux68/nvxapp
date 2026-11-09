## Primi passi
Questa parte ti indica da dove cominciare in base al tuo ruolo. Le voci di menu citate sono quelle della sezione **Presenze** e delle sezioni collegate.

### Se sei un dipendente {User}
1. All'inizio e alla fine del lavoro apri **Timbratura** e premi **Conferma**.
2. Per ferie, permessi o ore in più da autorizzare usa **Richiesta giustificativo**.
3. Se hai dimenticato una timbratura usa **Richiesta timbratura**.
4. Segui lo stato delle tue richieste in **Richieste** e controlla il mese in **Calendario**.
5. Per i rimborsi usa **Richiesta rimborso** e segui le note in **Le mie note spese**.

> Finché una richiesta non è approvata il calendario la mostra come in attesa e le ore di quel giorno possono risultare incomplete: è normale.

### Se sei responsabile di reparto {CompanyAdmin}
- In **Richieste** trovi le richieste dei dipendenti da esaminare; il numero accanto alla voce indica quante ne aspettano una tua decisione.
- In **Calendario** vedi il mese di un dipendente dei reparti che amministri e approvi le richieste direttamente dal giorno.
- **Personale presente** e **Piano ferie** ti danno il quadro di presenze e assenze.
- In **Esame spese** approvi o rifiuti le note spese.
- In **Utenti** apri la scheda dei dipendenti.

Ricevi le richieste solo se nel reparto sei indicato come amministratore con la casella **Approva richieste** attiva: l'impostazione si fa in **Reparti**, scheda **Administratori**.

### Se gestisci il personale {CompanyPowerAdmin}
Ordine consigliato per la messa in funzione:
1. Controlla causali e giustificativi proposti dal wizard (**Causali**, **Giustificativi**).
2. Definisci **Orari**, **Profili Orari** e il calendario delle **Festività**.
3. Prepara **Competenze**, **Attività**, **Sedi**, **Reparti** (con amministratori e approvatori) e **Commesse**.
4. Inserisci i dipendenti con rapporto di lavoro, profilo orario e competenze.
5. Scegli in **Configurazione** la modalità di approvazione delle richieste.
6. Ogni mese: correzioni nel **Calendario HR**, approvazione in **Quadratura mensile**, file per le paghe con **Export causali**.

Il wizard **Nuovo dipendente** guida l'inserimento in cinque passi: Utente, Rapporto di lavoro, Profilo orario, Competenze e Maturazione contatori. Con **Importa dipendenti da file** puoi caricare un foglio di calcolo, un documento, un testo o un'immagine: l'assistente legge cognome, nome, email, username e data di assunzione e ti fa controllare i dati prima dell'inserimento (fino a 500 dipendenti per importazione, file fino a 5 MB).

## Concetti chiave
Questo capitolo spiega come il programma costruisce il cartellino. Conoscere questi meccanismi aiuta a capire perché un giorno risulta in errore o perché le ore in più non sono ancora qualificate.

### Ore teoriche, ore lavorate, ore lavorabili
- **Ore teoriche**: le ore previste dall'orario del giorno, cioè la somma delle coppie DALLE-ALLE dell'orario che il profilo orario del dipendente assegna a quel giorno. Un orario senza coppie è un giorno di riposo (0 ore). Nei giorni festivi vale l'**Orario festivo** del profilo.
- **Ore lavorate**: la somma delle coppie entrata-uscita, calcolate sulle timbrature arrotondate.
- **Ore lavorabili**: le ore teoriche corrette dai giustificativi del giorno. Un giustificativo con segno "-" (ferie, permessi) toglie ore dal dovuto e conta come assenza; con segno "+" le aggiunge; con "Non agisce" non cambia il dovuto.
- **Ore in più** o **ore mancanti**: la differenza tra ore lavorate e ore lavorabili.

### Timbrature, verso e arrotondamento
La timbratura si registra senza verso. Il calcolo ordina le timbrature del giorno e le abbina alle coppie dell'orario: la prima è un'entrata, la seconda un'uscita e così via. Ogni orario di inizio e fine coppia ha una finestra di tolleranza (**Limite SX** e **Limite DX**): la timbratura che cade nella finestra viene agganciata a quell'orario e arrotondata secondo il **Tipo arrotondamento** (1, 5, 15, 30 o 60 minuti) e il **Segno arrotondamento** ("+" in avanti, "-" indietro).

Ogni timbratura conserva l'orario originale (**Timbratura Originale**), quello eventualmente corretto dall'HR (**Timbratura**) e quello usato nei conteggi (**Timbratura Arrotondata**). Il **Giorno Competenza** è il giorno a cui la timbratura appartiene: di norma coincide con la data, ma per un turno che finisce dopo la mezzanotte l'HR può attribuire l'uscita al giorno precedente.

### Stato della giornata
| Stato | Significato |
|---|---|
| Non ancora elaborata | Il giorno non è stato ancora calcolato. |
| Giornata corretta | Timbrature e ore quadrano con l'orario. |
| Presenza di avvertimenti | Il giorno è calcolato ma c'è qualcosa da verificare, per esempio ore lavorate superiori alle previste. |
| Presenza di errori | Mancano timbrature rispetto all'orario atteso o le ore lavorate sono inferiori alle previste. |
| Giornata bloccata | Il giorno appartiene a un cartellino approvato e non si ricalcola. |

### Quando avviene il calcolo
Il giorno si ricalcola da solo quando si registra una timbratura, quando si invia, approva o cancella una richiesta e quando l'HR modifica un movimento. Il **Calendario HR** permette anche il ricalcolo manuale di un giorno, di una parte del mese o del mese intero; dopo una modifica a orari o profili orari serve un ricalcolo manuale dei mesi interessati.

### Giustificativi e causali
- Il **giustificativo** è ciò che il dipendente chiede o che l'HR inserisce: ferie, permesso, malattia, autorizzazione alle ore in più. Modifica le ore dovute e può scalare un contatore.
- La **causale** è il codice che il calcolo scrive sul giorno e che finisce nell'export per le paghe: le ore lavorate (tramite la causale dell'intervallo orario), le ore del giustificativo (tramite la causale collegata), straordinario, supplementare, banca ore.
- Sia giustificativi sia causali possono essere **a ore** o **a importo**. Quelli a importo registrano solo euro e non toccano le ore.

### Ore in più: supplementare, straordinario, banca ore
Se nel profilo orario è attivo **Calcola straordinario e supplementare**, le ore in più vengono qualificate sul **Periodo di conteggio** del profilo (giornaliero, settimanale lunedì-domenica o mensile):
- le ore mancanti di un giorno compensano quelle in più di un altro giorno dello stesso periodo (**compensate**);
- per un full-time le ore oltre il teorico del periodo diventano **straordinario** se superano la soglia, altrimenti **ore eccedenti non straordinarie**;
- per un part-time le ore in più sono **lavoro supplementare** fino alla soglia full-time e straordinario oltre; il supplementare che supera la percentuale delle ore settimanali contrattuali è **supplementare oltre il limite**;
- lo straordinario è **notturno** per la parte svolta nella fascia notturna del profilo e **festivo** nei giorni festivi;
- la **franchigia giornaliera** esclude i primi minuti in più e le ore si riconoscono a blocchi (per esempio di 15 minuti);
- con **Ore in più solo se autorizzate** si riconoscono al massimo le ore di un giustificativo di autorizzazione approvato per quel giorno; il resto risulta "non autorizzato".

Finché il periodo di conteggio non è finito le ore in più restano **provvisorie**. Se nel rapporto di lavoro il dipendente ha **Accredito in banca ore**, le ore qualificate dei cartellini approvati vanno in banca ore invece che in pagamento.

### Il cartellino del mese
| Stato | Significato |
|---|---|
| Non calcolato | Per il mese non esiste ancora un calcolo. |
| Provvisorio | Il periodo è in corso, ci sono ore in più ancora provvisorie o autorizzazioni da approvare. |
| Da approvare | Il mese è concluso e calcolato: chiuso, in attesa di approvazione. |
| Approvato | I giorni sono bloccati: niente nuove timbrature, richieste o correzioni finché l'HR non lo riapre. |

### Richieste e approvazioni
Ogni richiesta (giustificativo, timbratura, nota spese) segue gli stessi stati: **Immessa** appena inviata, **Approvazione In Corso** o **Parzialmente Approvata** quando servono più approvatori, poi **Approvata**, **Rifiutata** o **Cancellata**. I movimenti inseriti direttamente, come la timbratura dal terminale, sono **Diretta**. Una richiesta già approvata non si cancella: si chiede la **revoca**, che segue a sua volta l'approvazione.

### Contatori
I giustificativi con contatore (ferie, permessi) hanno un **maturato** mensile, un **goduto** e un **saldo**. Il saldo tiene conto del riporto dell'anno precedente e si vede per il mese precedente, il mese corrente e i mesi successivi. Il tipo di contatore decide cosa succede a una richiesta che supera il saldo (vedi **Giustificativi**).

@@ /timeclockuser
### Campi
| Campo | Descrizione |
|---|---|
| Orario | Mostra la data e l'ora correnti, aggiornate ogni minuto. |
| Ultima timbratura | L'ora e la data dell'ultima timbratura registrata da questa pagina. |
| Commessa Attivita | Obbligatorio. Elenca cliente, commessa, sub commessa e attività su cui puoi timbrare. Viene proposta l'attività dell'ultima timbratura. |

### Come fare
1. Controlla l'attività proposta in **Commessa Attivita** e cambiala se oggi lavori su un'altra.
2. Premi **Conferma**.
3. Verifica l'orario in **Ultima timbratura**.

Non devi indicare se è un'entrata o un'uscita: il calcolo assegna il verso in base all'ordine delle timbrature del giorno. Per questo è importante timbrare ogni coppia prevista dal tuo orario: una timbratura in meno lascia il giorno in errore.

### Esempio
Il tuo orario è 08:30-12:30 e 13:30-17:30. Timbri alle 08:27, 12:31, 13:29 e 17:34: il calcolo riconosce entrata, uscita, entrata, uscita, applica gli arrotondamenti dell'orario e il giorno risulta corretto. Se dimentichi la timbratura delle 17:34, il giorno resta con tre timbrature e risulta in errore finché non invii una **Richiesta timbratura**.

### Attenzione
- Non si timbra in un giorno fuori dal rapporto di lavoro (prima dell'assunzione o dopo la cessazione).
- Non si timbra in un mese il cui cartellino è già approvato: l'HR deve prima riaprirlo.
- Si può usare solo un'attività delle sub commesse a cui sei collegato; se non sei collegato a nessuna vale quella proposta dall'azienda.

=> /requestclockinguser /usertimesheet

@@ /requestlistuser
### Stati della richiesta
| Stato | Significato |
|---|---|
| Immessa | Inviata, nessun approvatore ha ancora deciso. |
| Approvazione In Corso | Alcuni approvatori hanno approvato, altri devono ancora farlo. |
| Parzialmente Approvata | Approvazione avviata ma non completata. |
| Approvata | La richiesta è valida ed entra nel cartellino. |
| Rifiutata | Almeno un approvatore l'ha rifiutata: non ha effetto. |
| Cancellata | L'hai annullata prima dell'approvazione. |
| Diretta | Movimento registrato senza approvazione, per esempio dal terminale o dall'HR. |

Nelle colonne trovi **Dal**, **Al**, **Tipo** (timbratura, giustificativo, nota spese), **Dettaglio** e **Stato**.

### Come fare
1. Individua la richiesta nell'elenco.
2. Usa il comando di cancellazione della riga.
3. Conferma alla domanda "Confermi la cancellazione della richiesta?".

Se la richiesta è ancora in attesa viene annullata subito. Se è già approvata parte una **richiesta di revoca**: gli approvatori la esaminano e, se la approvano, il movimento sparisce dal cartellino. Finché la revoca è in attesa puoi ritirarla dal **Calendario** con **Cancella revoca**.

=> /usertimesheet /requestjustificationuser /requestclockinguser

@@ /usertimesheet
### Cosa vedi
| Elemento | Significato |
|---|---|
| Timbrature | Ogni timbratura con il verso (E entrata, U uscita) e l'ora; se è legata a una richiesta ne mostra lo stato. |
| Giustificativi | Ferie, permessi e altri giustificativi del giorno, con ore o importo. |
| Ore teoriche e lavorate | Le ore previste dall'orario e quelle risultanti dalle timbrature. |
| In più | Ore oltre il previsto (o mancanti), con la loro qualifica: supplementare, straordinario, banca ore. |
| Nota spese | Le note spese del giorno con il totale; il dettaglio mostra spese e allegati. |
| Stato giornata | L'icona del giorno: corretta, avvertimenti, errori, bloccata o non elaborata. |
| Riepilogo del mese | Il riassunto del cartellino: ore teoriche previste, ore lavorate effettive, permessi e assenze, ore in più e loro destinazione, saldo del periodo. |

### Come fare
1. Clicca un elemento del giorno (timbratura, giustificativo o nota spese).
2. Scegli **Dettaglio** per vederne i dati, oppure **Cancella richiesta** se è una tua richiesta.
3. Per ritirare una revoca ancora in attesa scegli **Cancella revoca**.

Con il pulsante **+** in basso apri al volo la richiesta di giustificativo o di timbratura e, con **PDF**, prepari il cartellino del mese visualizzato: il file si scarica dalla barra dei lavori in alto quando è pronto.

### Esempio
A metà mese il riepilogo mostra "in attesa di quadratura" accanto alle ore in più: il tuo profilo conta lo straordinario a settimana o a mese e le ore si qualificano solo a periodo concluso. Non è un errore: a fine periodo diventano supplementare, straordinario o ore compensate.

### Attenzione
- Le ore in più restano provvisorie finché non finisce il periodo di conteggio del tuo profilo.
- Le ore in più oltre un'autorizzazione approvata risultano "non autorizzate" e non vengono riconosciute, se il tuo profilo lo prevede.
- I giorni di un mese approvato sono bloccati: per correzioni rivolgiti all'HR.

=> /requestjustificationuser /requestclockinguser /requestlistuser

@@ /requestjustificationuser
### Campi
| Campo | Descrizione |
|---|---|
| Giustificativo | Obbligatorio. I giustificativi disponibili in azienda (ferie, permessi, autorizzazione alle ore in più...). |
| Data inizio | Obbligatoria. Primo giorno del periodo. |
| Data fine | Obbligatoria. Ultimo giorno del periodo (uguale alla data inizio per un solo giorno). |
| Giorno intero | Se attivo, per ogni giorno vengono richieste le ore teoriche del tuo orario. Non è disponibile per l'autorizzazione alle ore in più. |
| Ore e Min | Le ore richieste per ciascun giorno quando non chiedi il giorno intero: al massimo 8 ore, 16 per un'autorizzazione alle ore in più. |
| Importo per giorno (€) | Solo per i giustificativi a importo, al posto delle ore: maggiore di zero. |
| Note | Facoltative, visibili agli approvatori. |

### Come fare
1. Scegli **Permesso** (o il giustificativo usato in azienda) in **Giustificativo**.
2. Imposta lo stesso giorno in **Data inizio** e **Data fine**.
3. Lascia spento **Giorno intero** e indica 2 in **Ore** e 0 in **Min**.
4. Premi **Invia**.

### Esempio
Chiedi ferie da lunedì 4 a domenica 10 con **Giorno intero**. Il programma conta solo i giorni con ore previste dal tuo orario: sabato e domenica di riposo vengono saltati e la richiesta vale 5 giorni di ferie per le ore teoriche di ciascun giorno. Se il saldo ferie attuale non basta ma basta quello dei mesi successivi, ricevi un avviso e la richiesta parte comunque.

### Attenzione
- Un periodo che non contiene giorni lavorativi viene rifiutato.
- Se il giustificativo ha un **contatore con blocco** e il saldo, anche contando la maturazione futura, non copre le ore richieste, la richiesta viene rifiutata con il saldo disponibile.
- Se il tuo reparto non ha nessun approvatore abilitato la richiesta non può partire: avvisa l'amministrazione.
- Non sono ammessi giorni fuori dal rapporto di lavoro né giorni di un mese già approvato.
- Un'autorizzazione alle ore in più vale per ogni giorno del periodo, anche sabato, domenica e festivi, e viene sempre decisa da un approvatore: il calcolo non la approva mai in automatico.

=> /requestlistuser /usertimesheet

@@ /requestclockinguser
### Campi
| Campo | Descrizione |
|---|---|
| Data e ora | Obbligatorie. Il giorno e l'orario esatti della timbratura dimenticata o da correggere. |
| Commessa Attivita | Obbligatorio. L'attività su cui hai lavorato, scelta tra quelle consentite. |

### Come fare
1. Imposta la data e l'ora della timbratura mancante.
2. Scegli l'attività in **Commessa Attivita**.
3. Premi **Conferma**.

Ogni richiesta inserisce una sola timbratura: se ne hai dimenticate due, invia due richieste. Come per il terminale, non si indica il verso: il calcolo lo assegna in base all'ordine delle timbrature del giorno.

### Esempio
Martedì sei uscito alle 17:30 senza timbrare. Il giorno risulta in errore per timbrature mancanti. Invii una richiesta per martedì alle 17:30: finché è in attesa il calendario la mostra come richiesta, dopo l'approvazione la timbratura entra nel cartellino e il giorno viene ricalcolato.

### Attenzione
- L'attività deve essere tra quelle delle sub commesse a cui sei collegato.
- Non si possono chiedere timbrature in giorni fuori dal rapporto di lavoro o in un mese già approvato.

=> /timeclockuser /requestlistuser /usertimesheet

@@ /notaspesarequestuser
### Campi
| Campo | Descrizione |
|---|---|
| Descrizione della nota spese | Facoltativa, fino a 200 caratteri. Es. "Trasferta cliente Milano". |
| Data | Obbligatoria, per ogni spesa. |
| Tipo di spesa | Obbligatorio, per ogni spesa. I tipi sono definiti dall'amministrazione. |
| Importo (€) | Obbligatorio, maggiore di zero. Per il rimborso chilometrico è calcolato. |
| Descrizione | Facoltativa, fino a 200 caratteri. Es. "pranzo con il cliente". |
| Allega scontrino / fattura | PDF o immagini (JPEG, PNG, WEBP, HEIC): fino a 5 MB ciascuno, al massimo 10 per spesa e 15 MB per l'intera nota. |
| Autovettura e Km percorsi | Solo per il rimborso chilometrico: l'auto del tuo rapporto di lavoro e i km (maggiori di zero). |

### Come fare
1. Premi **Aggiungi spesa** e scegli un tipo di spesa a rimborso chilometrico.
2. Premi **Indica km e autovettura**.
3. Scegli l'**Autovettura** e scrivi i **Km percorsi**: l'importo è km per rimborso al km dell'auto.
4. Conferma la finestra e completa le altre spese.
5. Premi **Invia**.

### Esempio
Trasferta a Milano: treno 45,00 euro con il biglietto in PDF, pranzo 18,50 euro con la foto dello scontrino, 120 km con l'auto aziendale a 0,30 euro al km (36,00 euro calcolati). La nota ha tre spese per 99,50 euro e va in approvazione come un'unica richiesta.

### Attenzione
- Serve almeno una spesa.
- Se nel tuo rapporto di lavoro non ci sono autovetture non puoi inserire rimborsi chilometrici: chiedi all'amministratore di aggiungerle.
- La nota spese non modifica il cartellino delle presenze.

=> /notaspeseuser

@@ /notaspeseuser
### Stati e azioni
| Situazione | Cosa puoi fare |
|---|---|
| In attesa di approvazione | Modificarla (salvando torna in attesa) oppure **Annulla nota**. |
| Approvata | **Chiedi revoca**: la revoca va agli approvatori. |
| Revoca in attesa | Attendere la decisione sulla revoca. |
| Revocata | Nessuna: la nota non è più valida. |
| Rifiutata o annullata | Nessuna: se serve, crea una nuova nota. |

Con il pulsante **Nuova nota spese** apri direttamente la compilazione.

### Come fare
1. Apri la nota in attesa.
2. Correggi spese o allegati.
3. Premi **Salva**: la nota torna in attesa di approvazione.

### Attenzione
- Una nota già approvata, rifiutata o annullata non si può più modificare.
- Puoi modificare solo le tue note spese.

=> /notaspesarequestuser

@@ /requestlistadmin
### Filtri
| Filtro | Cosa mostra |
|---|---|
| Da approvare | Le richieste che aspettano la tua decisione. |
| Tutte | Tutte le richieste dei dipendenti che gestisci. |
| Approvate | Solo quelle già approvate. |

Le colonne sono **Cognome**, **Nome**, **Dal**, **Al**, **Tipo**, **Dettaglio** e **Stato**.

### Come funziona l'approvazione
Gli approvatori di una richiesta sono gli amministratori del reparto del dipendente con **Approva richieste** attivo, nell'ordine indicato in **Ordine**. Cosa succede dipende dalla modalità scelta in **Configurazione**:
| Modalità | Effetto |
|---|---|
| Singolo amministratore | Basta la decisione di un approvatore. |
| Tutti gli amministratori | Devono approvare tutti; nel frattempo la richiesta è "Approvazione In Corso". Basta un rifiuto per rifiutarla. |
| Tutti gli amministratori gerarchicamente | Come sopra, ma in ordine: vedi la richiesta da approvare solo dopo che l'approvatore precedente l'ha approvata. |

### Esempio
Con la modalità gerarchica il reparto Produzione ha come approvatori il capo reparto (ordine 1) e il direttore (ordine 2). Il direttore vede le ferie di un operaio tra quelle da approvare solo dopo l'approvazione del capo reparto; se il capo reparto le rifiuta, la richiesta è rifiutata e al direttore non arriva.

### Attenzione
- Una richiesta con giorni fuori dal rapporto di lavoro del dipendente si può solo rifiutare.
- Tra le richieste ci sono anche le **revoche** di richieste già approvate: approvando la revoca il movimento viene tolto dal cartellino.
- Le richieste di autorizzazione alle ore in più vanno sempre decise qui o dal calendario: il calcolo non le approva mai in automatico.

=> /admintimesheet /vacationplan /companycfgedit /departmentlist

@@ /poweradmintimesheet
### Comandi del giorno
Clicca il numero del giorno per i comandi di calcolo e inserimento, clicca un movimento per modificarlo o cancellarlo.
| Comando | Effetto |
|---|---|
| Calcola (giorno) | Ricalcola solo quel giorno. |
| Calcola dal 01 al (giorno) | Ricalcola dall'inizio del mese fino al giorno. |
| Calcola dal (giorno) al (fine mese) | Ricalcola dal giorno a fine mese. |
| Calcola tutto il mese | Ricalcola l'intero mese. |
| Aggiungi timbratura al giorno | Inserisce una timbratura. |
| Aggiungi giustificativo al giorno | Inserisce un giustificativo, a ore o a importo. |
| Aggiungi causale al giorno | Inserisce una causale direttamente, con le ore o l'importo. |
| Approva richiesta / Rifiuta richiesta | Decide una richiesta in attesa del dipendente. |

### Finestra Calcola
| Campo | Descrizione |
|---|---|
| Dal, Al | Il periodo da ricalcolare, proposto dal comando scelto. |
| Selezione | I dipendenti da ricalcolare (proposto quello visualizzato). |
| Approva Richieste Timbrature | Approva le richieste di timbratura in attesa del periodo. Attivo di default. |
| Approva Richieste Giustificativo | Approva le richieste di giustificativo in attesa, escluse le autorizzazioni alle ore in più. Attivo di default. |
| Genera Timbrature Mancanti | Completa le coppie mancanti secondo l'orario. Attivo di default. |
| Genera Giustificativi Assenza | Copre le ore mancanti con il giustificativo di assenza ingiustificata del profilo. Attivo di default. |

Le ultime due opzioni agiscono solo sui giorni già passati, mai su oggi o sul futuro.

### Campi della timbratura
| Campo | Descrizione |
|---|---|
| Giorno Competenza | Obbligatorio. Il giorno a cui appartiene la timbratura (es. uscita dopo mezzanotte attribuita al giorno prima). |
| Timbratura | Obbligatoria. L'orario valido. |
| Timbratura Originale | Obbligatoria. L'orario registrato in origine, conservato come riferimento. |
| Timbratura Arrotondata | Obbligatoria. L'orario usato nei conteggi; il calcolo lo riscrive applicando gli arrotondamenti. |
| Commessa Attivita | Obbligatorio. L'HR può usare qualsiasi attività. |

### Riepilogo del mese
Sotto il calendario il **Riepilogo del mese** mostra il cartellino e le azioni dell'HR:
- **Ore in più riconosciute (h:mm, vuoto = tutte)** con **Applica e ricalcola**: limita le ore in più qualificate del mese; quelle tolte risultano "non riconosciute" (si tolgono partendo dagli ultimi giorni).
- **Riconosci tutte** e **Rifiuta tutte**: riconoscono tutte le ore in più oppure nessuna.
- **Approva**: approva il cartellino quando è chiuso, in attesa di approvazione.
- **Riapri**: riporta un cartellino approvato a "da approvare" e rende di nuovo modificabili i giorni.

Il pulsante **+** apre la richiesta di giustificativo o di timbratura per il dipendente visualizzato, inserita dall'HR, e **PDF** prepara i cartellini del mese dei dipendenti che scegli.

### Come fare
1. Clicca la timbratura sbagliata e scegli **Modifica timbratura**.
2. Correggi **Timbratura** (l'originale resta conservata) e, per un turno notturno, il **Giorno Competenza**.
3. Conferma e scegli **Calcola** sul giorno.
4. Controlla che lo stato giornata sia corretto.

### Esempio
Un dipendente ha dimenticato l'uscita di ieri e non ha fatto richiesta. Dal giorno scegli **Calcola** lasciando attiva **Genera Timbrature Mancanti**: il programma completa la coppia secondo l'orario. Se preferisci un orario preciso, usa invece **Aggiungi timbratura al giorno**.

### Attenzione
- Con le opzioni di default il calcolo approva le richieste in attesa del periodo: se vuoi lasciarle all'esame dei responsabili, disattiva le opzioni di approvazione prima di confermare.
- Nei giorni fuori dal rapporto di lavoro non si inseriscono movimenti, si possono solo cancellare.
- Un giorno di un mese approvato non si modifica e non si ricalcola: usa **Riapri** nel riepilogo.
- Le ore riconosciute non si cambiano su un cartellino approvato: va prima riaperto.

=> /quadraturamensile /exportcausali /requestlistadmin

@@ /quadraturamensile
### Colonne
| Colonna | Significato |
|---|---|
| Dipendente | Cognome e nome, con contratto (full-time o part-time) ed eventuale banca ore. |
| Stato | Non calcolato, Provvisorio, Da approvare o Approvato. |
| Lavorate | Ore lavorate rispetto alle teoriche. |
| In più | Ore oltre il dovuto; "provv." indica la parte ancora provvisoria. |
| Suppl. | Lavoro supplementare (part-time). |
| Strao | Straordinario. |
| Eccedenti | Ore in più non straordinarie dei full-time. |
| Non ric. | Ore non riconosciute dall'HR o "non autoriz."; "da autorizzare" indica autorizzazioni ancora in attesa. |

Dal pulsante accanto a ogni dipendente apri il suo mese nel **Calendario HR**.

### Come fare
1. Scegli anno e mese (ed eventualmente il reparto).
2. Premi **Seleziona da approvare**: vengono selezionati i cartellini chiusi.
3. Premi **Approva (n)** e conferma.

### Esempio
A inizio novembre approvi ottobre. Un dipendente risulta ancora **Provvisorio**: ha una richiesta di autorizzazione allo straordinario non ancora decisa. Dopo l'approvazione della richiesta e il ricalcolo del mese, il cartellino passa a **Da approvare** e puoi approvarlo.

### Attenzione
- Un cartellino resta provvisorio finché il mese non è finito; con il conteggio settimanale serve che sia passata anche la domenica dell'ultima settimana del mese.
- Resta provvisorio anche con ore in più non ancora qualificate o autorizzazioni da approvare.
- Se compare "Ricalcolare il mese dal Calendario HR." i dati sono da aggiornare: ricalcola il mese prima di approvare.
- Dal pulsante **+** scegli **Report** per la stampa PDF della quadratura mensile.

=> /poweradmintimesheet /exportcausali

@@ /exportcausali
### Campi
| Campo | Descrizione |
|---|---|
| Anno e mese | Il mese da esportare: imposta automaticamente Dal e Al al primo e all'ultimo giorno. |
| Reparto e Dipendenti | Facoltativi, per limitare l'export ad alcuni dipendenti. |
| Dal, Al | Obbligatori. Puoi restringere il periodo all'interno del mese. |
| Tipo export | Obbligatorio. Il modello di export da applicare. |

### Come fare
1. Scegli anno e mese.
2. Se serve, limita reparti e dipendenti.
3. Scegli il modello in **Tipo export**.
4. Premi **Esegui** e conferma.
5. Scarica il file dalla barra dei lavori quando compare "Export completato, clicca per scaricare".

### Esempio
Il consulente paghe vuole entro il 5 il file di ottobre. Approvi i cartellini in **Quadratura mensile**, poi esporti ottobre con il modello del suo programma paghe. Il messaggio finale segnala due dipendenti senza matricola: completi la matricola nella loro scheda e ripeti l'export.

### Attenzione
- L'export legge le causali calcolate: se hai corretto qualcosa, ricalcola il mese prima di esportare.
- Un modello senza causali associate non produce nulla: aggiungile nel modello.
- Se nessun dipendente ha causali del modello nel periodo, non viene generato alcun file.
- Se il formato del programma paghe non è disponibile viene prodotto il file JSON dei dati.
- Al termine possono comparire avvisi: dipendenti senza matricola o valori negativi dovuti a righe con verso "Sottrai".

=> /exportcaulist /quadraturamensile /causalilist

@@ /activitystatistics
### Viste
| Vista | Cosa mostra |
|---|---|
| Totali attività | Le ore per attività, con commessa, sub commessa e cliente, e il totale complessivo. |
| Per dipendente | Le stesse ore suddivise per dipendente. |
| Giornate escluse | I giorni non conteggiati, con dipendente, data e stato (non elaborata, errore, warning). |

### Come fare
1. Scegli il periodo, i reparti e i dipendenti.
2. Controlla la vista **Giornate escluse** e, se ci sono giorni, correggili e ricalcolali dal **Calendario HR**.
3. Dal pulsante **+** scegli **Report** e poi **Totali attività** o **Per dipendente** per il PDF.

### Attenzione
- Le ore per attività derivano dalle timbrature: i giorni non ancora elaborati o con errori non vengono conteggiati.

=> /commessalist /activitylist /poweradmintimesheet

@@ /presentstaff
### Come leggere la pagina
Filtra per **Reparto** e **Dipendenti**. Per ogni dipendente vedi un'icona di stato, cognome, nome e l'ultima timbratura del giorno con verso (E o U) e ora; il trattino indica che non ci sono timbrature.

### Attenzione
- La pagina si basa sulle timbrature registrate: chi ha dimenticato di timbrare risulta senza timbrature anche se è al lavoro.

=> /vacationplan /admintimesheet

@@ /vacationplan
### Come fare
1. Scegli anno e mese, reparti e dipendenti.
2. Clicca un'assenza in attesa per approvarla o rifiutarla con **Approva richiesta** o **Rifiuta richiesta**.
3. Per la stampa usa il pulsante **+**, **Report** e poi **Piano ferie**.

### Attenzione
- Compaiono solo i giustificativi con **Visualizza nel piano ferie** attivo; quelli a importo non compaiono mai.

=> /justificationlist /requestlistadmin

@@ /admintimesheet
### Cosa puoi fare
- Cliccare una richiesta in attesa e scegliere **Approva richiesta** o **Rifiuta richiesta**.
- Aprire il **Dettaglio nota spese (approva / rifiuta)** di una nota del giorno.
- Con il pulsante **+**, inserire una richiesta di giustificativo o di timbratura per il dipendente e preparare con **PDF** i cartellini del mese dei dipendenti scelti.

### Attenzione
- Vedi solo i dipendenti dei reparti che amministri.
- Da qui non si modificano direttamente timbrature e giustificativi: le correzioni dirette si fanno nel **Calendario HR**.

=> /requestlistadmin /notaspeseadmin /vacationplan

@@ /userdepartmentlist
### Scheda del dipendente
| Sezione | Campi |
|---|---|
| Anagrafica | Cognome e Nome (obbligatori, fino a 50 caratteri), Matricola (fino a 20, unica in azienda), Codice fiscale (verificato e unico), Disabilitato. |
| Competenze | Le competenze del dipendente: determinano le attività che può svolgere. Senza competenze viene assegnata quella di default. |
| Rapporto Lavoro | Data assunzione, Data licenziamento, Tipo contratto (Full-time o Part-time), Ore settimanali contrattuali (0-168), CCNL (fino a 50), Straordinario e supplementare (Pagamento in busta paga o Accredito in banca ore). |
| Profili orari | I profili orari del rapporto, ciascuno con Dal, Al e Num. Giorno partenza ciclo. |
| Contatori | Per anno: Riporto anno precedente (anche con Leggi dal), Maturazione mensile per giustificativo (formato HHH:MM) e dettaglio mensile di maturato, goduto e saldo. |
| Spesa | Autovetture per i rimborsi chilometrici: Descrizione (obbligatoria, fino a 50), Targa (fino a 15), Rimborso per km (€). |

Le **Ore settimanali contrattuali** sono la base del limite del lavoro supplementare; se le lasci vuote si usano le ore teoriche della settimana. **Mostra disabilitati** fa comparire anche i dipendenti disabilitati.

### Come fare
1. Apri la scheda del dipendente e la sezione **Profili orari**.
2. Chiudi il profilo attuale impostando **Al** al giorno prima del cambio.
3. Aggiungi il nuovo profilo con **Dal** dalla data del cambio.
4. Per un profilo a turni indica in **Num. Giorno partenza ciclo** da quale giorno del ciclo si parte.
5. Ricalcola i mesi interessati dal **Calendario HR**.

### Esempio
Dal 1 marzo una dipendente passa a part-time 20 ore. Nel rapporto imposti **Part-time** e 20 nelle **Ore settimanali contrattuali**, chiudi il profilo a tempo pieno al 28 febbraio e aggiungi il profilo part-time dal 1 marzo: da marzo le ore in più diventano lavoro supplementare fino alla soglia full-time.

### Attenzione
- La data di licenziamento non può precedere quella di assunzione.
- Matricola e codice fiscale non possono essere già usati da un altro dipendente.
- Un'autovettura usata in note spese non si può cancellare.

=> /departmentlist /profiliorarilist /competencelist

@@ /notaspeseadmin
### Filtri e viste
| Elemento | Descrizione |
|---|---|
| Periodo, Reparto, Dipendenti | Selezionano le note da esaminare. |
| Tipo di spesa | Tutti oppure un solo tipo. |
| Stato | In attesa e approvate, In attesa di approvazione, Approvate, Rifiutate o annullate, Tutte. |
| Per dipendente / Per tipo di spesa | Raggruppano le spese. |
| Dati / Grafico | Elenco dettagliato o grafico dei totali. |
| Espandi tutte / Comprimi tutte | Aprono o chiudono tutti i gruppi. |

### Come fare
1. Filtra lo stato **In attesa di approvazione**.
2. Apri il dettaglio della nota spese: vedi spese, allegati e totale.
3. Premi **Approva** o **Rifiuta** e conferma.
4. Per una nota con **Revoca richiesta** decidi allo stesso modo la revoca.

Dal pulsante **+** scegli **Report** e poi **Per tipo di spesa** o **Per dipendente** per il PDF.

### Attenzione
- L'esame delle note spese è riservato agli amministratori dell'azienda.
- La decisione vale per la nota intera: non si approva una singola spesa.

=> /tipospesalist /admintimesheet

@@ /justificationlist
### Campi
| Campo | Descrizione |
|---|---|
| Codice | Obbligatorio, fino a 5 caratteri (es. FER, ROL, MAL). |
| Descrizione | Obbligatoria, fino a 50 caratteri. |
| Colore sfondo, Colore testo | I colori con cui il giustificativo compare in calendario e piano ferie. |
| Tipo | Ore oppure Importo. Non si cambia più dopo il primo utilizzo. |
| Segno | "-" toglie ore dal dovuto (ferie, permessi), "+" le aggiunge, "Non agisce" non cambia il dovuto. Obbligatorio. |
| Causale | La causale scritta nel cartellino e usata dall'export. Deve essere dello stesso tipo del giustificativo. |
| Visualizza nel piano ferie | Fa comparire il giustificativo nel piano ferie. |
| Tipo contatore | Nessun contatore, Contatore, Contatore con avviso, Contatore con blocco. Obbligatorio. |
| Recupero banca ore | Il contatore matura le ore in più accreditate in banca ore nei cartellini approvati. |
| Autorizza ore in più | Fa del giustificativo un'autorizzazione allo straordinario. |

### Tipi di contatore
| Tipo | Comportamento sulle richieste |
|---|---|
| Nessun contatore | Nessun controllo. |
| Contatore | Tiene maturato, goduto e saldo, senza controlli. |
| Contatore con avviso | Avvisa se il saldo attuale non copre la richiesta ma la invia. |
| Contatore con blocco | Rifiuta la richiesta se nemmeno il saldo comprensivo della maturazione futura la copre. |

### Esempio
Per un'autorizzazione allo straordinario crei il giustificativo AUTS con **Autorizza ore in più** attivo, segno "Non agisce", causale vuota e nessun contatore. Nei profili orari con **Ore in più solo se autorizzate**, un AUTS approvato di 2 ore permette di riconoscere al massimo 2 ore in più in quel giorno.

### Attenzione
- Un giustificativo già usato non può cambiare tipo (ore o importo).
- Un giustificativo a importo vuole una causale a importo e uno a ore una causale a ore.
- I giustificativi a importo non incidono su ore, contatori, banca ore e piano ferie.

=> /causalilist /profiliorarilist /vacationplan

@@ /causalilist
### Campi
| Campo | Descrizione |
|---|---|
| Codice | Obbligatorio, fino a 5 caratteri. |
| Descrizione | Obbligatoria, fino a 50 caratteri. |
| Tipo | Ore oppure Importo. |

### Dove si usano
- Negli intervalli degli orari (**Causale HH Lav**, per le ore lavorate) e nel monte ore.
- Nei giustificativi, per tradurre l'assenza in un codice paghe.
- Nella scheda **Straordinario** dei profili orari (straordinario, supplementare, banca ore).
- Nei modelli di export, dove ogni causale riceve il codice del programma paghe.

### Attenzione
- Il tipo di una causale già usata non si può cambiare.
- Le causali a importo non si usano in profili, orari e fasce: lì servono causali a ore.

=> /justificationlist /exportcaulist /orarilist

@@ /companycfgedit
### Campi
| Campo | Descrizione |
|---|---|
| Tipo Approvazione | Obbligatorio. Singolo amministratore, Tutti gli amministratori oppure Tutti gli amministratori gerarchicamente. |

### Esempio
Un'azienda piccola sceglie **Singolo amministratore**: basta il sì del titolare o del responsabile. Un'azienda con più livelli sceglie la modalità gerarchica e imposta in ogni reparto l'**Ordine** degli approvatori.

=> /departmentlist /requestlistadmin

@@ /orarilist
### Campi
| Campo | Descrizione |
|---|---|
| Codice | Obbligatorio, fino a 10 caratteri. |
| Descrizione | Obbligatoria, fino a 50 caratteri. |
| Tipo timbrature | Intervallo Orario (coppie di orari) oppure Monte Ore / Monte Ore Valore (solo un totale di ore teoriche). |
| Numero Coppie | Le coppie entrata-uscita previste; 0 = riposo (0 ore teoriche). |
| Causale monte ore, Ore Teoriche | Solo per il monte ore: la causale e le ore previste nel giorno. |

Per ogni coppia, nella scheda **Intervalli orari**:
| Campo | Descrizione |
|---|---|
| Dalle, Alle | Gli orari previsti di inizio e fine. |
| Limite SX, Limite DX | La finestra entro cui una timbratura viene agganciata a quell'orario. |
| Tipo arrotondamento | 1, 5, 15, 30 o 60 minuti. |
| Segno arrotondamento | "+" arrotonda in avanti, "-" indietro. |
| Usa x Match | Usa quell'orario per riconoscere le timbrature. |
| Causale HH Lav | Obbligatoria. La causale a ore su cui vanno le ore lavorate della coppia. |

### Esempio
Orario 08:30-12:30 e 13:30-17:30. Per l'entrata delle 08:30 imposti limiti 08:00 e 09:00, arrotondamento 15 minuti con segno "+": chi timbra alle 08:34 risulta entrato alle 08:45. Per l'uscita delle 17:30 imposti arrotondamento 15 minuti con segno "-": un'uscita alle 17:41 vale 17:30.

### Attenzione
- Le modifiche a un orario valgono per i calcoli successivi: ricalcola i mesi interessati.
- Le causali a importo non si possono usare negli orari.

=> /profiliorarilist /causalilist

@@ /profiliorarilist
### Campi
| Campo | Descrizione |
|---|---|
| Descrizione, Codice | Obbligatori, fino a 50 e 10 caratteri. |
| Tipo Profilo | Settimanale (ciclo fisso di 7 giorni) oppure Turno (ciclo da 1 a 15 giorni). |
| Num. Giorni Ciclo | Solo per i turni: la durata del ciclo. |
| Orario festivo | Obbligatorio. L'orario applicato nei giorni festivi. |
| Giustificativo assenza ingiustificata | Obbligatorio. Quello che il calcolo usa per coprire le ore mancanti quando si generano i giustificativi di assenza. |
| Giorni | Per ogni giorno del ciclo l'orario da applicare; senza orario il giorno è di riposo. |

Nella scheda **Straordinario**:
| Campo | Descrizione |
|---|---|
| Calcola straordinario e supplementare | Se spento le ore in più restano solo segnalate nel cartellino. |
| Periodo di conteggio | Giornaliero, Settimanale (lun-dom) o Mensile. |
| Soglia giornaliera full-time | Usata per i part-time con conteggio giornaliero. |
| Soglia settimanale full-time (ore) | 0-168, proposta 40. |
| Limite supplementare (% ore settimanali contrattuali) | 0-100, proposta 25. |
| Ore in più riconosciute a blocchi di | 1, 5, 15, 30 o 60 minuti, proposta 15. |
| Franchigia giornaliera (minuti non riconosciuti) | Sotto questa soglia le ore in più del giorno non si riconoscono. |
| Ferie e permessi contano per la soglia | Le assenze si sommano al lavorato per decidere lo straordinario. |
| Domenica festiva | La domenica è trattata come giorno festivo. |
| Ore in più solo se autorizzate | Si riconoscono al massimo le ore autorizzate nel giorno. |
| Notturno dalle, alle | La fascia notturna, proposta 22:00-06:00. |
| Causali | Causale base dello straordinario e del supplementare (obbligatorie) e causali facoltative per notturno, festivo, oltre il limite, eccedenti e banca ore. |

### Come fare
1. Dal pulsante **+** scegli **Duplica un profilo**.
2. Scegli il profilo di partenza in **Profilo da duplicare**.
3. Cambia descrizione, codice e i giorni che differiscono.
4. Salva e assegnalo ai dipendenti dalla loro scheda.

### Esempio
Part-time orizzontale di 20 ore: profilo **Settimanale** con orario 08:00-12:00 dal lunedì al venerdì e riposo il sabato e la domenica; conteggio settimanale, limite supplementare 25%. Con 20 ore contrattuali il supplementare entro il limite arriva a 5 ore a settimana; le ore oltre diventano supplementare oltre il limite, fino alla soglia full-time.

### Attenzione
- Le modifiche al profilo non ricalcolano da sole i mesi passati: ricalcola dal **Calendario HR**.
- Le causali dei profili devono essere a ore.

=> /orarilist /userdepartmentlist /quadraturamensile

@@ /festivitalist
### Campi
| Campo | Descrizione |
|---|---|
| Descrizione | Obbligatoria, fino a 50 caratteri. |
| Tipo | Ogni anno, stesso giorno; Solo nella data indicata; Lunedì dell'Angelo (calcolato). |
| Giorno / Data | Obbligatorio per i primi due tipi; per le ricorrenti l'anno non conta. |

### Esempio
Per il santo patrono del 7 dicembre inserisci "Sant'Ambrogio" con tipo **Ogni anno, stesso giorno**. Per un ponte aziendale di un solo anno usa **Solo nella data indicata**.

### Attenzione
- Nei giorni festivi vale l'orario festivo del profilo orario e le ore lavorate oltre il teorico sono straordinario festivo.
- Le festività aggiunte dopo il calcolo entrano nel cartellino solo dopo il ricalcolo del mese.

=> /profiliorarilist /poweradmintimesheet

@@ /exportcaulist
### Campi
| Campo | Descrizione |
|---|---|
| Codice | Obbligatorio, fino a 10 caratteri. |
| Descrizione | Obbligatoria, fino a 50 caratteri. |
| Indicazioni per l'AI | Facoltative, fino a 1000 caratteri. |
| Documentazione | Tracciato, manuale o file di esempio del programma paghe: PDF, DOCX, ODT, XLSX, TXT, CSV, XML, JSON o MD, fino a 5 MB e al massimo 10 documenti. |

Nella scheda **Causali**, per ogni riga:
| Campo | Descrizione |
|---|---|
| Codice | Obbligatorio. Il codice della voce nel programma paghe. |
| Causale | Obbligatoria. La causale delle presenze da esportare. |
| Tipo elaborazione | Giornaliera o Mensile. |
| Unità | Ore, Giorni o Importo. |
| Verso | Somma o Sottrai: le righe con lo stesso codice si sommano, con Sottrai il valore viene tolto. |

### Come fare
1. Salva prima il modello, poi premi **Carica documenti**.
2. Nella **Prova del formato** scegli **Mese** e **Dipendente**.
3. Premi **Prova il formato** e controlla le prime righe del file.
4. Se il risultato non va bene, correggi le indicazioni e premi **Rigenera con l'AI**.

Con **Duplica un export** copi un modello esistente con i suoi documenti e il suo formato.

### Attenzione
- Le righe con lo stesso codice devono avere la stessa elaborazione e la stessa unità.
- L'unità Importo è solo per le causali a importo, e una causale a importo vuole l'unità Importo.
- Un PDF scansionato non contiene testo e non può essere usato dall'AI.
- Una documentazione molto lunga viene letta solo in parte.
- Senza documentazione l'export produce il file JSON dei dati.

=> /exportcausali /causalilist

@@ /tipospesalist
### Campi
| Campo | Descrizione |
|---|---|
| Codice | Obbligatorio, fino a 10 caratteri. |
| Descrizione | Obbligatoria, fino a 50 caratteri. |
| Tipo | Generica oppure Rimborso KM: con Rimborso KM nella nota si indicano km e autovettura e l'importo è calcolato. |

### Attenzione
- Un tipo di spesa usato in qualche spesa non si può cancellare.
- Il rimborso chilometrico funziona solo se nella scheda del dipendente ci sono autovetture con il rimborso per km.

=> /notaspesarequestuser /notaspeseadmin /userdepartmentlist

@@ /commessalist
### Campi
| Campo | Descrizione |
|---|---|
| Descrizione | Obbligatoria, fino a 50 caratteri. |
| Cliente | Obbligatorio, dall'anagrafica clienti comune. |
| Descrizione subcommessa | La descrizione della sub commessa. |
| Data inizio, Data fine | Il periodo della sub commessa. |
| Attività, Reparti, Utenti | Le attività della sub commessa e i reparti e gli utenti collegati. |

### Come fare
1. Crea la commessa con **Nuova Commessa** e scegli il cliente.
2. Aggiungi una sub commessa e collega le attività.
3. Collega i reparti dove si svolgono quelle attività.
4. Collega gli utenti che hanno le competenze richieste.
5. Da questo momento i dipendenti collegati vedono le attività in **Timbratura**.

### Attenzione
- Scollegando un'attività, i reparti e gli utenti non più compatibili vengono scollegati automaticamente.
- Un cliente usato in una commessa non può essere cancellato dall'anagrafica clienti.

=> /activitylist /departmentlist /activitystatistics

@@ /departmentlist
### Campi
| Campo | Descrizione |
|---|---|
| Sede | Scelta nell'elenco: il reparto appartiene a una sede. |
| Descrizione reparto | Obbligatoria, fino a 20 caratteri. |
| Admin | L'utente amministra il reparto e ne vede i dipendenti. |
| Approva richieste | L'amministratore riceve le richieste dei dipendenti del reparto. |
| Ordine | La posizione dell'approvatore nella catena, usata dall'approvazione gerarchica. |
| Attività di reparto | Le attività svolte nel reparto (Abil). |
| Utenti che lavorano nel reparto | I dipendenti del reparto, scelti tra quelli con le competenze richieste. |

### Come fare
1. Apri il reparto e la scheda **Administratori**.
2. Spunta **Admin** e **Approva richieste** per il responsabile.
3. Indica l'**Ordine** se usi l'approvazione gerarchica.
4. Salva.

### Attenzione
- Se nel reparto nessun amministratore ha **Approva richieste**, le richieste dei suoi dipendenti non possono essere inviate.
- Togliendo un'attività, gli utenti che non hanno più attività compatibili vengono scollegati dal reparto.

=> /azsedilist /companycfgedit /requestlistadmin

@@ /azsedilist
### Campi
| Campo | Descrizione |
|---|---|
| Descrizione | Obbligatoria, fino a 20 caratteri. |
| Attività svolte nella sede | Le attività abilitate nella sede (Abil). |

=> /departmentlist /activitylist

@@ /activitylist
### Campi
| Campo | Descrizione |
|---|---|
| Descrizione | Obbligatoria, fino a 50 caratteri. |
| Colore sfondo, Colore testo | Come l'attività compare nelle timbrature e nelle statistiche. |
| Competenze necessarie | Le competenze richieste per svolgere l'attività. |

### Attenzione
- L'attività di default deve avere almeno una competenza.
- Le competenze dell'attività decidono quali utenti si possono collegare a reparti e sub commesse che la usano.

=> /competencelist /commessalist /azsedilist

@@ /competencelist
### Campi
| Campo | Descrizione |
|---|---|
| Descrizione | Obbligatoria, fino a 50 caratteri. |

### Esempio
Crei le competenze "Saldatura" e "Collaudo". L'attività "Assemblaggio telai" richiede "Saldatura": solo i dipendenti con quella competenza si possono collegare ai reparti e alle sub commesse con quell'attività e timbrarci sopra.

=> /activitylist /userdepartmentlist

## Glossario
- **Approvatore**: amministratore di reparto con "Approva richieste" attivo, che decide le richieste dei dipendenti del reparto.
- **Banca ore**: destinazione delle ore in più qualificate quando il rapporto di lavoro prevede l'accredito in banca ore invece del pagamento.
- **Cartellino**: il riepilogo mensile delle presenze di un dipendente; può essere provvisorio, da approvare o approvato.
- **Causale**: codice scritto sul giorno dal calcolo (ore lavorate, assenze, straordinario) e trasmesso al programma paghe.
- **Ciclo**: la sequenza di giorni di un profilo orario; 7 per il settimanale, da 1 a 15 per i turni.
- **Competenza**: capacità di un dipendente, richiesta dalle attività.
- **Contatore**: saldo di un giustificativo (maturato, goduto, saldo), per esempio ferie o permessi.
- **Coppia**: un'entrata e un'uscita previste dall'orario.
- **Franchigia**: minuti in più al giorno che non vengono riconosciuti.
- **Giorno Competenza**: il giorno a cui una timbratura viene attribuita.
- **Giustificativo**: assenza o autorizzazione chiesta dal dipendente o inserita dall'HR, a ore o a importo.
- **Ore eccedenti**: ore in più di un full-time che non superano la soglia dello straordinario.
- **Ore lavorabili**: ore teoriche più o meno i giustificativi del giorno.
- **Ore teoriche**: ore previste dall'orario del giorno.
- **Periodo di conteggio**: giorno, settimana o mese su cui si qualificano le ore in più.
- **Profilo orario**: assegna a ogni giorno del ciclo un orario e contiene le regole delle ore in più.
- **Revoca**: richiesta di annullare una richiesta già approvata, soggetta ad approvazione.
- **Sub commessa**: parte di una commessa con attività, reparti e utenti collegati.
- **Supplementare**: ore in più di un part-time fino alla soglia del tempo pieno.

## Risoluzione dei problemi

### La richiesta non parte: nessun amministratore abilitato all'approvazione
Il reparto del dipendente non ha nessun amministratore con **Approva richieste** attivo. Chi gestisce il personale deve abilitarne almeno uno in **Reparti**, scheda **Administratori**.

### Saldo insufficiente
Il giustificativo ha un contatore con blocco e il saldo, compresa la maturazione futura, non copre le ore richieste. Riduci il periodo o le ore, oppure chiedi all'amministrazione di verificare maturazione e riporto nella scheda del dipendente.

### Il periodo richiesto non contiene giorni lavorativi
Nel periodo scelto il tuo orario non prevede ore (riposi o festivi). Controlla le date: le assenze si chiedono sui giorni lavorativi.

### Movimento rifiutato: giorno prima dell'assunzione o dopo la cessazione
Timbrature e richieste sono ammesse solo nel periodo del rapporto di lavoro. Se le date del rapporto sono sbagliate vanno corrette nella scheda del dipendente.

### Il cartellino del mese è approvato
In un mese approvato non si inseriscono né si modificano movimenti. Chi gestisce il personale può usare **Riapri** nel riepilogo del mese del **Calendario HR**, correggere, ricalcolare e approvare di nuovo.

### L'attività selezionata non è tra quelle consentite
Puoi timbrare solo sulle attività delle sub commesse a cui sei collegato. Chiedi all'amministrazione di collegarti alla sub commessa giusta.

### Il giorno risulta in errore
Di solito manca una timbratura (numero dispari) o le ore lavorate sono inferiori a quelle previste. Il dipendente invia una **Richiesta timbratura** o un giustificativo; l'HR può correggere dal **Calendario HR** e ricalcolare.

### Il cartellino è ancora provvisorio {CompanyPowerAdmin}
Il mese non è concluso, ci sono ore in più ancora da qualificare o autorizzazioni in attesa. Attendi la fine del periodo, fai decidere le richieste, ricalcola il mese e riprova.

### L'export non produce il file {CompanyPowerAdmin}
Il modello non ha causali associate, oppure nessun dipendente ha causali del modello nel periodo. Controlla il modello in **Modelli Export** e che il mese sia stato calcolato.

### L'export produce un file JSON {CompanyPowerAdmin}
Il formato del programma paghe non era disponibile (manca la documentazione o l'AI non ha prodotto un formato valido). Carica la documentazione nel modello e usa **Prova il formato** prima di ripetere l'export.

### La nota spese non si può più modificare
È già approvata, rifiutata o annullata. Se è approvata puoi solo chiederne la revoca; negli altri casi crea una nuova nota.

### Non riesco a inserire un rimborso chilometrico
Nel tuo rapporto di lavoro non ci sono autovetture. L'amministratore deve aggiungerle nella sezione **Spesa** della tua scheda, con il rimborso per km.

### Non riesco a cancellare un tipo di spesa o un'autovettura {CompanyPowerAdmin, CompanyAdmin}
Sono già usati in qualche spesa e vanno conservati per lo storico delle note spese.

### Non riesco a cambiare il tipo di una causale o di un giustificativo {CompanyPowerAdmin}
Dopo il primo utilizzo il tipo (ore o importo) resta fisso. Crea una nuova causale o un nuovo giustificativo con il tipo desiderato.
