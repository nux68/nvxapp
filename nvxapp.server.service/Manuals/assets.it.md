# Manuale di Risorse aziendali
Beni, flotta e prenotazioni

## Introduzione
Il modulo Risorse aziendali raccoglie cio' che l'azienda possiede e a chi lo assegna: flotta, beni assegnati, prenotazioni di postazioni, sale e auto. È disponibile quando è attivo per la tua azienda: lo attiva il centro (o lo studio, con la licenza).

Questo manuale è costruito sul **tuo menu**: trovi un capitolo per ogni voce della sezione **Risorse aziendali** che vedi. Le voci che vedi dipendono dal tuo ruolo.

### Chi fa cosa
- **CompanyPowerAdmin**: configura il modulo e fa tutto quello che fa il CompanyAdmin.
- **CompanyAdmin** (acquisti, logistica, IT): gestisce veicoli, assegnazioni, scadenze, costi e multe.
- **Dipendente**: nella pagina **Le mie risorse** vede il veicolo assegnato e le multe che lo riguardano.

@@ /astsettings
Nella pagina **Impostazioni** il CompanyPowerAdmin imposta i parametri generali del modulo:
- **Giorni di preavviso delle scadenze**: con quanti giorni di anticipo il modulo segnala le scadenze che registra nello scadenzario comune (predefinito 30).
- **Notifiche attive**: se togli la spunta il modulo non invia notifiche né promemoria. Ogni utente sceglie comunque come riceverle (nell'app o per email) dalle preferenze delle notifiche.
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
La pagina **Le mie risorse** mostra il veicolo che ti è assegnato, con il fringe benefit dell'anno a titolo informativo, e le multe che ti sono state notificate.
