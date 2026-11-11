# Manuale di Compliance
Adempimenti normativi

## Introduzione
Il modulo Compliance raccoglie gli adempimenti normativi: sicurezza sul lavoro (D.Lgs 81/08), whistleblowing (D.Lgs 24/2023), registro GDPR. È disponibile quando è attivo per la tua azienda: lo attiva il centro (o lo studio, con la licenza).

Questo manuale è costruito sul **tuo menu**: trovi un capitolo per ogni voce della sezione **Compliance** che vedi. Sono disponibili la sicurezza sul lavoro e il registro GDPR; il whistleblowing arriverà dopo la verifica legale.

### Chi fa cosa
- **CompanyPowerAdmin**: configura il modulo, gestisce la sicurezza sul lavoro e il registro GDPR.
- **CompanyAdmin** (RSPP, ufficio del personale, DPO interno): sicurezza sul lavoro e registro GDPR.
- **Utenti abilitati ai dati sanitari** (medico competente o RSPP designato, scelti nelle impostazioni): le visite mediche con i dati sanitari.
- **Dipendente**: vede i propri corsi, le visite (solo esito e scadenza), i DPI ricevuti e le nomine, e conferma consegne e nomine nelle prese visione.
- **Studio**: gestisce il registro GDPR e consulta le scadenze del modulo delle aziende che seguono.

@@ /cmpsettings
Nella pagina **Impostazioni** il CompanyPowerAdmin imposta i parametri generali del modulo:
- **Giorni di preavviso delle scadenze**: con quanti giorni di anticipo il modulo segnala le scadenze che registra nello scadenzario comune (predefinito 30).
- **Notifiche attive**: se togli la spunta il modulo non invia notifiche né promemoria. Ogni utente sceglie comunque come riceverle (nell'app o per email) dalle preferenze delle notifiche.
- **Utenti abilitati ai dati sanitari**: il medico competente o l'RSPP designato; solo loro vedono e scrivono i dati sanitari delle visite.
- **Note**: appunti interni sulla configurazione.

Premi **Conferma** per salvare.

@@ /cmp81
La pagina **Sicurezza sul lavoro** raccoglie gli adempimenti del D.Lgs 81/08, una sezione alla volta (in alto):
- **Corsi svolti**: i corsi di ogni dipendente con data, ore, ente e scadenza. Se non indichi la scadenza si calcola con la validità richiesta dalle mansioni. Il corso più recente di un tipo rinnova i precedenti.
- **Situazione formazione**: per ogni dipendente i corsi richiesti dalle sue mansioni e l'ultimo svolto; mancanti e scaduti sono in evidenza e con **Aggiungi** registri il corso.
- **Visite mediche**: tipo, esito di idoneità, prescrizioni in sintesi e scadenza della prossima visita. I dati sanitari li vedono e li scrivono solo gli utenti abilitati nelle impostazioni; sono conservati cifrati.
- **Consegne DPI**: a ogni consegna il dipendente (se ha un utente) riceve la richiesta di presa visione, che vale come verbale di consegna.
- **Nomine**: RSPP, ASPP, RLS, preposti, addetti alle emergenze e al primo soccorso, medico competente; interni (con presa visione del nominato) o esterni.
- **Documenti (DVR)**: DVR, piani di emergenza e procedure con versione, revisione e scadenza.
- **Mansioni**, **Corsi per mansione**, **Mansioni dei dipendenti** e **Catalogo DPI**: le tabelle che servono alle sezioni precedenti.

Con il **+** aggiungi un elemento alla sezione aperta; dalla riga lo modifichi o lo cancelli. Le scadenze (corsi, visite, nomine, documenti) finiscono nello **Scadenzario** e si aggiornano a ogni salvataggio.

@@ /cmpgdpr
La pagina **Registro GDPR** contiene:
- **Trattamenti**: il registro delle attività di trattamento (art. 30): finalità, base giuridica, interessati, dati, destinatari, trasferimenti, termini di cancellazione, misure di sicurezza.
- **Responsabili esterni**: responsabili del trattamento e nomine (art. 28), con la scadenza del contratto nello scadenzario.
- **Violazioni**: il registro delle violazioni (art. 33). Se la violazione comporta un rischio, la notifica al Garante va fatta entro 72 ore dalla scoperta: la scadenza resta aperta nello scadenzario finché non registri la data di notifica. Alla registrazione di una violazione gli amministratori ricevono una notifica.

Con **Excel** prepari il registro completo: il file si scarica dalla barra dei job quando è pronto.

@@ /cmpmine
In **I miei adempimenti** trovi i tuoi corsi con le scadenze, le visite mediche (tipo, data, esito e scadenza), i DPI ricevuti e le nomine. Le consegne dei DPI e le nomine si confermano nella pagina **Prese visione** (bottone in basso).

@@ /cmpstudio
In **Compliance aziende** lo studio sceglie una delle sue aziende con Compliance attivo (accanto al nome le scadenze aperte e scadute) e ne gestisce il registro GDPR (trattamenti, responsabili, violazioni, export in Excel); nella sezione **Scadenze** consulta le scadenze del modulo dell'azienda.
