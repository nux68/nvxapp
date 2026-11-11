# Manuale di Risorse aziendali
Beni, flotta e prenotazioni

## Introduzione
Il modulo Risorse aziendali raccoglie cio' che l'azienda possiede e a chi lo assegna: flotta, beni assegnati, prenotazioni di postazioni, sale e auto. È disponibile quando è attivo per la tua azienda: lo attiva il centro (o lo studio, con la licenza).

Questo manuale è costruito sul **tuo menu**: trovi un capitolo per ogni voce della sezione **Risorse aziendali** che vedi. Le voci che vedi dipendono dal tuo ruolo.

### Chi fa cosa
- **CompanyPowerAdmin**: configura il modulo e le risorse prenotabili e fa tutto quello che fa il CompanyAdmin.
- **CompanyAdmin** (acquisti, logistica, IT): gestisce veicoli, assegnazioni, scadenze, costi e multe, beni con consegne e restituzioni; vede e annulla tutte le prenotazioni.
- **Dipendente**: prenota postazioni, sale e auto in pool; nella pagina **Le mie risorse** vede i beni che ha in consegna (con i verbali da confermare), il veicolo assegnato e le multe che lo riguardano.

@@ /astsettings
Nella pagina **Impostazioni** il CompanyPowerAdmin imposta i parametri generali del modulo:
- **Giorni di preavviso delle scadenze**: con quanti giorni di anticipo il modulo segnala le scadenze che registra nello scadenzario comune (predefinito 30).
- **Notifiche attive**: se togli la spunta il modulo non invia notifiche né promemoria. Ogni utente sceglie comunque come riceverle (nell'app o per email) dalle preferenze delle notifiche.
- **Prenotazioni**: durata massima di una prenotazione (ore) e anticipo massimo (giorni); 0 = nessun limite. Le prenotazioni non richiedono approvazione.
- **Note**: appunti interni sulla configurazione.

Premi **Conferma** per salvare.

@@ /astvehicles
La pagina **Veicoli** elenca i veicoli dell'azienda con l'assegnatario attuale e la prossima scadenza. Con il **+** in basso aggiungi un veicolo; dalla riga lo modifichi o lo elimini (con assegnazioni, scadenze, costi e multe). La casella **Mostra i veicoli dismessi** nelle opzioni mostra anche quelli non più in uso.

### Il veicolo
Targa (unica nell'azienda), marca, modello, alimentazione, immatricolazione, sede e chilometri. **Uso**: *Assegnato* (a un dipendente) o *Pool* (si prenota dalle prenotazioni). **Contratto**: proprietà, noleggio o leasing, con fornitore, numero, scadenza, km inclusi e canone; la scadenza del contratto va nello **scadenzario**. **Fringe benefit**: costo chilometrico ACI ed emissioni di CO2.

Salvato il veicolo, sotto compaiono le sezioni **Assegnazioni**, **Scadenze**, **Costi** e **Multe**; il **+** apre il menu per scegliere cosa aggiungere.

### Assegnazioni e fringe benefit
L'assegnazione indica il dipendente e il periodo (un veicolo è assegnato a una persona per volta). Con l'**uso promiscuo** il fringe benefit annuo è 15.000 km per il costo ACI per la percentuale (lasciando 0 si usa quella delle emissioni: fino a 60 g/km 25%, fino a 160 30%, fino a 190 50%, oltre 60%); si vede anche la quota dell'anno in corso. Il valore è informativo: l'export per le paghe resta nelle Presenze.

Ogni assegnazione crea o aggiorna l'**autovettura del dipendente** (con il rimborso al km) che le note spese delle Presenze usano per i rimborsi chilometrici.

### Scadenze
Bollo, assicurazione, revisione, tagliando (anche a chilometri) o altro: ogni scadenza va nello **scadenzario** comune con il preavviso delle impostazioni e i promemoria agli amministratori. Indicando la data di esecuzione la scadenza si chiude.

### Costi e multe
I **costi** (carburante, manutenzione, pedaggi...) mostrano il totale dell'anno; i chilometri indicati aggiornano quelli del veicolo. Nella **multa** il conducente, se non lo indichi, è chi aveva il veicolo il giorno dell'infrazione; con il pulsante **Invia** la multa si notifica al conducente (serve che abbia un utente) e passa a *Notificata*.

@@ /astmine
La pagina **Le mie risorse** mostra i beni che hai in consegna (e quelli restituiti nell'ultimo anno) con lo stato dei verbali: quelli da confermare si aprono con **Apri** nella pagina delle prese visione. Sotto trovi il veicolo che ti è assegnato, con il fringe benefit dell'anno a titolo informativo, e le multe che ti sono state notificate.

@@ /astgoods
La pagina **Beni** ha tre sezioni. Con il **+** in basso aggiungi un bene o una categoria.

### Beni
L'elenco dei beni (PC, telefoni, badge, attrezzature...) con lo stato e il dipendente che li ha in consegna. Il bene ha un codice di inventario unico, la categoria, descrizione, marca e modello, numero di serie, data di acquisto, valore, sede e fine della garanzia (che va nello **scadenzario**). Lo stato *Consegnato* lo gestiscono consegna e restituzione; con il bene in magazzino puoi sceglierlo *Disponibile*, *In riparazione* o *Dismesso*.

### Consegna e restituzione
Nella scheda di un bene disponibile il **+** registra la **consegna** a un dipendente, con la data e le condizioni. Il dipendente riceve il **verbale di consegna** da confermare nella pagina delle prese visione; con il Documentale attivo il verbale diventa un PDF da firmare. Dalla riga della consegna aperta registri la **restituzione** (data, condizioni e stato del bene al rientro): il dipendente riceve il verbale di restituzione. Lo storico mostra per ogni consegna lo stato dei verbali. Un dipendente senza utente non riceve verbali: si registra solo la consegna.

### Da restituire
I beni e i veicoli ancora in mano a dipendenti che hanno una **data di cessazione** (la registrano le Presenze o le Risorse Umane). Ogni notte gli amministratori ricevono una notifica con le nuove cessazioni entro i giorni di preavviso e ogni restituzione va nello **scadenzario** alla data di cessazione; la scadenza si chiude con la restituzione del bene o con la fine dell'assegnazione del veicolo. Il pulsante **Verifica ora** lancia subito il controllo.

### Categorie
Le categorie dei beni: all'attivazione trovi quelle predefinite. Una categoria usata dai beni non si elimina: la disattivi.

@@ /astbookings
La pagina **Prenotazioni** mostra una settimana alla volta (con le frecce cambi settimana) le prenotazioni di postazioni, sale e auto in pool. Puoi filtrare per risorsa o vedere solo le tue. Delle prenotazioni degli altri vedi solo che la risorsa è *Occupata*; gli amministratori vedono anche chi ha prenotato.

### Prenotare
Con il **+** in basso scegli la risorsa, l'inizio e la fine (e se vuoi un titolo e delle note). Se la risorsa è già prenotata in quell'orario la prenotazione viene rifiutata, anche quando due persone prenotano nello stesso momento. Valgono i limiti delle impostazioni (durata massima e anticipo massimo).

Dalla riga modifichi o annulli le tue prenotazioni future; gli amministratori possono annullare quelle di tutti (l'interessato riceve una notifica).

### Calendario personale
Le tue prenotazioni compaiono nel tuo calendario personale (Google, Outlook, Apple): il pulsante **Calendario** apre la pagina con l'indirizzo da aggiungere.

@@ /astresources
La pagina **Risorse prenotabili** (CompanyPowerAdmin) elenca postazioni, sale e auto in pool. Con il **+** aggiungi una risorsa: tipo, descrizione, sede, i posti per le sale e, per le auto, il veicolo della flotta con uso *Pool*. Una risorsa con prenotazioni future non si elimina: disattivala.
