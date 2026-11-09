# Analisi – Gestione attivazione e pagamento delle licenze

Branch: **Infrastructure_Dev** (funzione base: entità, risoluzione listino, abbonamenti, gateway di pagamento e l'aggancio all'attivazione applicativi stanno nell'infrastruttura, poi si mergiano verso gli altri rami).
Le tabelle, il contesto `public` e il legame con `CompanyApplication`/`FinancialAdvisorApplication` sono lavoro di Infrastructure_Dev e vanno fatti *prima* che i singoli rami aggancino l'inibizione delle proprie funzioni (§8).

Stato: **analisi iniziale, niente implementato. Decisa (09/11/2026) la propagazione delle licenze dello studio alle aziende (§4.3); gli altri punti di §13 sono aperti.** Documento da rivedere con l'utente ai punti aperti (§13) prima di cominciare. Le scelte segnate "proposta" sono indicazioni di partenza, non decisioni.

> Data: 06/11/2026. Le funzioni da inibire a licenza scaduta (punto 9 della richiesta) **non sono ancora pianificate**: qui si predispone solo il "gancio", non il comportamento di ogni funzione.

---

## 1. Situazione di partenza (cosa c'è già)

Il sistema di licenze non parte da zero: si innesta su strutture esistenti. Capire queste è metà del lavoro.

### 1.1 Gerarchia dei soggetti

Quattro livelli annidati (`AccountService.UserLoad`, risale la catena e popola il token JWT):

```
Dealer (centro / rivenditore)
  └─ FinancialAdvisor (studio / consulente)
       └─ Company (azienda)
            └─ User (dipendente)
```

Entità in `nvxapp.server.data/Entities/Public/`: `Company.IdFinancialAdvisor`, `FinancialAdvisor.IdDealer`, `Dealer` radice. Ruoli in `ApplicationRole.RoleCode`:
`User=0`, `CompanyAdmin=10`, `CompanyPowerAdmin=11`, `FinancialAdvisorAdmin=100`, `FinancialAdvisorPowerAdmin=101`, `DealerAdmin=1000`, `DealerPowerAdmin=1001`, `Admin=10000`, `PowerAdmin=10001`, `SuperUser=100000`.

### 1.2 Attivazione degli applicativi (il cardine)

- Enum `ApplicationType`: `Common=0` (dati condivisi, **non** attivabile, schema sempre presente), `Moke=1`, `AttendanceTracking=2` (Presenze), `Crm=3`, `DocumentManagement=4` (Documentale).
- `CompanyApplication` (una riga per azienda/applicativo): `Active`, `ActivationDate`, `DeactivationDate`. In multi-tenant l'attivazione crea/migra lo schema `tenant_<IdCompany>_<IdApp>`; la disattivazione **blocca solo l'accesso**, schema e dati restano.
- `FinancialAdvisorApplication`: gemella per gli applicativi "lato studio" (non crea schemi, abilita le funzioni dello studio). Un'azienda può avere un applicativo attivo anche se il suo studio non lo ha.
- **Chi attiva/disattiva**: solo **SuperUser** (anche mentre impersona) o **DealerPowerAdmin del centro** del soggetto (`ApplicationActivationAuthorizer`). Né lo studio né l'azienda attivano i propri applicativi.
- **Verifica "funzione attiva"**:
  - backend: `ICompanyApplicationRepository.ActiveApplications(idCompany)`, caricata in `UserLoad` nel token;
  - frontend: `AuthService.hasApplication(app)` / `hasFinancialAdvisorApplication(app)` (`auth.service.ts`), alimentate da `UserLoad`; il menu mostra le voci di un ramo solo se l'app è attiva (`main-menu.service.ts`, `forApplication`).

> Oggi l'attivazione è un **interruttore manuale** di SuperUser/Dealer, senza alcun legame con un pagamento o una scadenza. La licenza è esattamente il pezzo mancante: **cosa** rende lecito accendere quell'interruttore e **per quanto tempo**.

### 1.3 Precedente utile: i piani dello storage

Esiste già un concetto di "piano a quota" per lo spazio documentale: `StoragePlan` + `CompanyStorage`/`CompanyStorageHistory`, pagine `/storageplanlist` (SuperUser) e `/companystoragelist`, servizio `StorageQuotaService` (riga `CompanyStorage` creata con il "pacchetto predefinito" se manca, `IdStoragePlan`, `QuotaBytes`, soglie di avviso). È il modello concettuale più vicino ai listini: **conviene ispezionarlo e riusarne lo stile** (definizione del piano a livello SuperUser, assegnazione per azienda, stato calcolato).

### 1.4 Registrazione attuale

`register-page.component.ts` → `RegistrationService.CompanyRegister`/`CompanyRegisterConfirm`:
- registrazione pubblica dallo showcase in due fasi (richiesta + conferma via email, token 24h in `CompanyRegistrationRequest`);
- crea **solo aziende (Company)**, dentro il ramo DEMO (studio DEMO del centro DEMO), riusando `AccountService.CompanyPut`;
- **nessuna scelta di tipologia soggetto**: oggi chi si registra è sempre un'azienda. Il punto 1 della richiesta introduce la scelta Company vs FinancialAdvisor.

### 1.5 Inibizione per ruolo già esistente

`CompanyMenuRestriction` (voce di menu nascosta a un ruolo dell'azienda, gestita dal CompanyPowerAdmin) mostra che esiste già un meccanismo di nascondimento voci. Non è legato alle licenze, ma è il precedente di "nascondere/limitare" lato menu (§8 lo tiene distinto: scaduto ≠ nascosto per scelta).

---

## 2. Obiettivo e mappa dei requisiti

Gestire **sottoscrizione, rinnovo e pagamento** delle licenze d'uso degli applicativi, con listini configurabili e un aggancio all'attivazione/inibizione delle funzioni. I 12 punti della richiesta, raggruppati:

**Chi e cosa si compra**
1. In registrazione chiedere se il soggetto è **Company** o **FinancialAdvisor**. (Oggi solo Company — §9)
2. **Listini differenziati per livello** di soggetto. Per ora solo Company e FinancialAdvisor. (§4–5)
5. Listini per **singola funzione** (Presenze, CRM, Documentale) **+ un cumulativo**. (§5)
3. Un listino ha **N livelli** di abbonamento modificabili (es. Free/Standard/Pro); per ora 3. (§5)
4. Ogni abbonamento è **mensile o annuale**. (§5–6)

**Sottoscrizione e pagamento**
6. L'utente può **sottoscrivere e rinnovare** i piani. (§6)
7. **Carta di credito** e **PayPal**. (§7)
8. **Modalità debug**: il giro di acquisto/rinnovo si completa **senza eseguire la transazione**. (§7)

**Effetti e gestione**
9. Metodi che **inibiscono funzioni** a licenza scaduta (quali, non ancora pianificato). (§8)
10. Nuova **voce di menu sotto Assistenza** per gestire le licenze. (§10)
11. Listini definiti a livello **SuperUser**, con possibilità (abilitabile) che un **Dealer** definisca un **proprio listino** che non tocca quello del SuperUser. (§4)
12. Parametro di listino: se le licenze sottoscritte dal **FinancialAdvisor** sono disponibili anche sulle **Company** del suo studio. (§4.3)

---

## 3. Modello dati proposto

Tutte le tabelle stanno nello schema `public` (`PublicDbContext`, come `Company`/`CompanyApplication`/`StoragePlan`): sono dati trasversali, non per-tenant. Prefisso proposto **`Lic_`**. Repository in `Repositories/Public`.

### 3.1 Listino e piani (la definizione, lato SuperUser/Dealer)

**`Lic_Listino`** – un listino = una combinazione (proprietario, tipo soggetto, ambito):
- `IdDealer` *(nullable)*: `null` = listino **globale del SuperUser**; valorizzato = **override del Dealer** (punto 11);
- `TipoSoggetto` (enum `Company` / `FinancialAdvisor`) (punto 2);
- `Ambito`: la funzione coperta. Proposta: riusare `ApplicationType` (Presenze=2, CRM=3, Documentale=4) + un valore **`Cumulativo`** (tutte le funzioni) (punto 5);
- `Valuta`, `Attivo`, `Descrizione`;
- `DisponibileSuCompany` *(bool, solo se `TipoSoggetto=FinancialAdvisor`)*: le licenze del listino valgono anche per le aziende dello studio (punto 12 – §4.3);
- unicità logica: una sola riga attiva per (`IdDealer`, `TipoSoggetto`, `Ambito`).

**`Lic_Piano`** – i livelli dentro un listino (Free/Standard/Pro…, punto 3):
- `IdListino`, `Codice` (libero), `Descrizione`, `Ordine`;
- `PrezzoMensile`, `PrezzoAnnuale` (punto 4; un prezzo a 0 = piano gratuito, es. Free);
- `Attivo`;
- *(predisposizione, non da compilare ora)* collegamento alle **capability/limiti** del piano (quali funzioni/limiti dà) → tabella figlia `Lic_PianoFunzione` o un `IdStoragePlan`/quote, da definire quando si pianifica il punto 9.

> **Perché prezzo come due colonne e non righe mensile/annuale:** l'abbonamento sceglie la periodicità (§3.2); tenere i due prezzi sul piano evita di duplicare il piano. Se in futuro servono più periodicità (trimestrale…) si passa a una figlia `Lic_PianoPrezzo (Periodicita, Prezzo)`. Da decidere (§13).

### 3.2 Abbonamento e pagamento (la sottoscrizione, lato cliente)

**`Lic_Abbonamento`** – una licenza sottoscritta da un soggetto:
- soggetto: **`IdCompany` *oppure* `IdFinancialAdvisor`** (uno dei due, coerente con `Lic_Piano→Lic_Listino.TipoSoggetto`);
- `IdPiano`, `Periodicita` (`Mensile`/`Annuale`), `PrezzoBloccato` (il prezzo al momento della sottoscrizione, per non cambiarlo se il listino cambia);
- `DataInizio`, `DataScadenza`, `Stato` (`Attivo`/`InScadenza`/`Scaduto`/`Sospeso`/`Annullato`);
- `RinnovoAutomatico` (bool);
- tracce: `IdListino` e snapshot di origine (per sapere da quale listino/Dealer è nato).

**`Lic_Pagamento`** – la transazione (storico, uno per tentativo/rinnovo):
- `IdAbbonamento`, `Importo`, `Valuta`, `Periodicita` coperta, `DataInizioCoperta`/`DataFineCoperta`;
- `Metodo` (`CartaCredito`/`PayPal`);
- `Stato` (`Creato`/`InAttesa`/`Completato`/`Fallito`/`Rimborsato`/`Debug`);
- `RiferimentoGateway` (id transazione del provider), `Debug` (bool, §7);
- date di creazione/aggiornamento.

### 3.3 Abilitazione del Dealer a gestire i listini (punto 11)

Flag **`PuoGestireListini`** sull'entità `Dealer` (predefinito `false`): se `true`, il DealerPowerAdmin può creare i propri `Lic_Listino` (con `IdDealer` valorizzato). Senza il flag vede solo quelli del SuperUser (sola lettura). L'abilitazione la dà il SuperUser.

### 3.4 Legame con l'attivazione applicativi

Il legame licenza ↔ attivazione **non duplica** `CompanyApplication`: un abbonamento attivo per un `Ambito` è ciò che **autorizza e mantiene** `Active=true` dell'applicativo corrispondente; la scadenza porta all'inibizione (§8). Da decidere (§13) se la sottoscrizione **attiva automaticamente** l'applicativo o se resta la doppia azione (SuperUser/Dealer attiva + licenza copre).

---

## 4. Risoluzione del listino (punti 11 e 12)

### 4.1 Quale listino vede un soggetto

Quando una Company (o uno Studio) apre la pagina "sottoscrivi", il sistema sceglie il listino così, per ogni `Ambito`:

1. risale al **Dealer** del soggetto (Company→FinancialAdvisor→Dealer);
2. se quel Dealer ha `PuoGestireListini=true` **ed** esiste un `Lic_Listino` attivo (`IdDealer`=quel dealer, `TipoSoggetto` del soggetto, `Ambito`) → **usa quello**;
3. altrimenti → usa il **listino globale del SuperUser** (`IdDealer=null`, stesso `TipoSoggetto`/`Ambito`).

Il listino del Dealer **ridefinisce** (non modifica) quello del SuperUser: sono righe distinte, la risoluzione sceglie la più specifica. Un servizio `ILicenseCatalogResolver.ResolveListino(soggetto, ambito)` incapsula questa regola (riuso obbligatorio, mai duplicata nei vari punti).

### 4.2 Chi definisce cosa

| Ruolo | Può |
|---|---|
| **SuperUser** | creare/modificare i listini globali (`IdDealer=null`); abilitare i Dealer (`PuoGestireListini`) |
| **DealerPowerAdmin** (se abilitato) | creare/modificare **solo** i propri listini (`IdDealer`=suo); non vede/tocca quelli di altri Dealer né quelli del SuperUser |
| altri | sola lettura del listino risolto, in fase di sottoscrizione |

Autorizzazione: riusare/estendere `IApplicationActivationAuthorizer` (SuperUser + DealerPowerAdmin del centro), che già modella esattamente questa coppia.

### 4.3 Licenze dello studio disponibili sulle aziende (punto 12)

Requisito: uno studio può acquistare una licenza che si **propaga alle sue aziende**. Un'azienda figlia di uno studio che **non** ha attivato la licenza può comunque attivarla **in autonomia**.

Parametro `Lic_Listino.DisponibileSuCompany` (valido per i listini con `TipoSoggetto=FinancialAdvisor`):
- se **attivo**, quando lo studio sottoscrive un piano, **tutte le Company dello studio** risultano coperte per la/e funzione/i del piano, senza dover sottoscrivere a loro volta;
- se **spento**, l'abbonamento dello studio vale solo per le funzioni "lato studio" (`FinancialAdvisorApplication`) e ogni azienda si sottoscrive da sé.

**Decisioni (09/11/2026):**
1. **Posti illimitati, copertura piena e automatica**: il piano dello studio copre **tutte** le sue aziende, comprese quelle create dopo la sottoscrizione. Non si assegnano posti alle singole aziende e non serve una tabella di assegnazione: la copertura si ricava da `Company.IdFinancialAdvisor`.
2. **Prezzo fisso**: il piano dello studio ha un prezzo unico (`PrezzoMensile`/`PrezzoAnnuale` del `Lic_Piano`), indipendente dal numero di aziende.
3. **Upgrade autonomo**: un'azienda coperta dallo studio può comprarsi da sola un piano **superiore** (listino Company). Vale il piano più alto tra i due (confronto per `Lic_Piano.Ordine`).

**Copertura di un'azienda** (regola del `ILicenseGuard`, §8.1), per ogni `Ambito`:
1. abbonamento **proprio** della Company attivo (anche `Cumulativo`);
2. abbonamento dello **studio** attivo, su un listino con `DisponibileSuCompany` (anche `Cumulativo`);
3. se valgono entrambi → si applica il **piano più alto**;
4. nessuno dei due → non coperta: l'azienda vede il listino **Company** e può sottoscrivere in autonomia.

**Casi limite:**

| Caso | Comportamento |
|---|---|
| Lo studio non ha la licenza (o ha `DisponibileSuCompany` spento) | Ogni azienda sottoscrive da sé dal listino Company, come un'azienda senza studio |
| L'azienda ha già una licenza propria e poi lo studio sottoscrive | L'azienda è coperta dallo studio da subito. Se il suo piano proprio è **uguale o inferiore**, le si propone di togliere il rinnovo automatico (per non pagare due volte) e la licenza propria resta valida fino alla scadenza. Se è **superiore**, la tiene (punto 3) |
| Lo studio non rinnova o la licenza scade | Le aziende ricevono un avviso. Dopo l'eventuale periodo di grace (§13) perdono la copertura dello studio e possono sottoscrivere in autonomia. Chi ha una licenza propria attiva resta coperto da quella |
| Un'azienda lascia lo studio (cambio di `IdFinancialAdvisor`, es. `CompanyMove`) | Perde la copertura del vecchio studio ed eredita quella del nuovo, se c'è |

**Cosa vede chi entra in "Gestione licenze":**
- **studio**: per ogni funzione piano, scadenza e stato. Con `DisponibileSuCompany` vede la nota "copre tutte le aziende dello studio" e l'elenco (in sola lettura) delle aziende coperte, con evidenza di quelle che hanno un piano proprio superiore;
- **azienda coperta dallo studio**: "Coperta dallo studio *nome dello studio* – piano *piano*, fino al *scadenza*" in sola lettura, con l'azione **Passa a un piano superiore** (a proprie spese, listino Company, mostrati solo i piani con `Ordine` più alto);
- **azienda non coperta**: stato "Non attiva" e **Sottoscrivi** dal listino Company.

---

## 5. Piani, prezzi, ambiti (punti 2, 3, 4, 5)

- **Livelli di soggetto** (punto 2): per ora `Company` e `FinancialAdvisor`. L'enum `TipoSoggetto` è aperto: aggiungere `Dealer` in futuro non rompe il modello.
- **Ambiti** (punto 5): un listino per **Presenze**, uno per **CRM**, uno per **Documentale**, uno **Cumulativo**. Da decidere (§13) se il Cumulativo è un `Ambito` a sé con prezzo proprio, oppure uno "sconto bundle" calcolato sui singoli: la proposta è **`Ambito` a sé** (più semplice, prezzo esplicito, nessuna regola di sconto da mantenere).
- **Livelli/piani** (punto 3): N per listino, per ora 3 (es. Free/Standard/Pro). Modificabili da chi gestisce il listino; il `Codice` è libero, non un enum, così si aggiungono/rinominano senza migration.
- **Periodicità** (punto 4): `Mensile`/`Annuale`, scelta alla sottoscrizione; prezzo annuale indipendente (consente lo sconto annuale senza calcoli).

Esempio (listino globale SuperUser, TipoSoggetto=Company, Ambito=Presenze):

| Piano | Mensile | Annuale |
|---|---|---|
| Free | 0 | 0 |
| Standard | 9,00 | 90,00 |
| Pro | 19,00 | 190,00 |

---

## 6. Sottoscrizione e rinnovo (punto 6)

Flusso dalla pagina "Gestione licenze" (§10), lato soggetto (CompanyPowerAdmin per l'azienda, FinancialAdvisorPowerAdmin per lo studio):

1. **Scelta**: ambito → piano → periodicità; il prezzo è quello del listino risolto (§4.1), bloccato in `Lic_Abbonamento.PrezzoBloccato`.
2. **Pagamento** (§7) se il prezzo > 0; i piani gratuiti (Free) saltano il pagamento e creano subito l'abbonamento.
3. **Attivazione**: creato/aggiornato `Lic_Abbonamento` (`DataInizio`=oggi, `DataScadenza`=+1 mese/+1 anno), e di conseguenza l'applicativo risulta coperto (§3.4).
4. **Rinnovo**:
   - **manuale**: stessa pagina, "Rinnova", estende `DataScadenza` dalla scadenza corrente (non da oggi, per non perdere giorni);
   - **automatico** (`RinnovoAutomatico`): un **hosted service** notturno (sul modello di `StorageMaintenanceService`/`CrmReminderService`) rinnova gli abbonamenti in scadenza, crea il `Lic_Pagamento`, e in caso di esito negativo porta lo stato a `InScadenza`→`Scaduto` con avviso email.
5. **Avvisi**: a N giorni dalla scadenza (`InScadenza`) email al PowerAdmin del soggetto (riuso del servizio email dell'infrastruttura, come gli avvisi quota storage).

Ogni operazione è un metodo di servizio (`ServiceBase`), accesso DB **solo via repository**.

---

## 7. Pagamenti: gateway e modalità debug (punti 7, 8)

### 7.1 Astrazione del gateway

Interfaccia unica `IPaymentGateway` (in Infrastructure), con implementazioni intercambiabili:
- `CreditCardPaymentGateway` (provider da scegliere, §13 – tipicamente Stripe per carte);
- `PayPalPaymentGateway`;
- `DebugPaymentGateway` (§7.2).

Contratto (proposta):
```
Task<PaymentResult> StartAsync(PaymentRequest req);     // importo, valuta, soggetto, metodo, ritorno
Task<PaymentResult> ConfirmAsync(string gatewayRef);    // callback/redirect del provider
Task<PaymentResult> RefundAsync(string gatewayRef);     // opzionale, v2
```
Il servizio licenze parla **solo** con `IPaymentGateway`: non conosce i dettagli del provider. La scelta carta/PayPal seleziona l'implementazione (factory/strategy su `Metodo`). I **secret** del provider stanno in configurazione (come `NVXAPP_Email__*`), mai nel codice.

### 7.2 Modalità debug (punto 8)

Flag di configurazione (es. `NVXAPP_Payments__Debug=true`) che fa risolvere l'`IPaymentGateway` al **`DebugPaymentGateway`**:
- esegue **tutto il giro** (crea `Lic_Pagamento`, aggiorna l'abbonamento, chiude il flusso) **senza chiamare alcun provider né muovere denaro**;
- marca il pagamento con `Debug=true` e `Stato=Debug`, così lo storico resta distinguibile;
- serve per demo, test end-to-end e sviluppo senza credenziali reali.

> Analogo alla regola "email nel server di prova con host vuoto": in debug il giro si completa ma **nessuna transazione reale** parte. Da tenere coerente nel server di prova.

### 7.3 Sicurezza e idempotenza

- Mai fidarsi dell'importo dal client: il server ricalcola il prezzo dal listino risolto.
- Callback del provider verificati (firma/webhook) e **idempotenti** (un rinnovo non deve contare due volte): chiave sul `RiferimentoGateway`.
- Rilevante per `piano-security.md` (auth/authz): gli endpoint di pagamento richiedono il ruolo PowerAdmin del soggetto e verificano l'appartenenza, mai l'id soggetto dal client.

---

## 8. Inibizione delle funzioni a licenza scaduta (punto 9)

Le funzioni da inibire **non sono pianificate**: qui si predispone solo il **gancio unico**, già pronto quando si deciderà cosa bloccare.

### 8.1 Il punto di verità

Servizio `ILicenseGuard` (Infrastructure):
```
Task<LicenseState> StateAsync(int idCompany, ApplicationType ambito);
// LicenseState: Licensed / Grace / Expired / NotSubscribed
```
Risolve con la regola del §4.3: abbonamento proprio della Company e abbonamento dello studio con `DisponibileSuCompany`. Se valgono entrambi vince il piano più alto. Se non ce n'è nessuno, la funzione non è coperta. Tiene conto di `DataScadenza` e di un eventuale periodo di **grace** (giorni dopo la scadenza in cui si avvisa ma non si blocca).

### 8.2 Dove si applica (tre livelli, da attivare quando si pianifica)

1. **Attivazione applicativi** (`CompanyApplicationService`): un abbonamento scaduto porta `CompanyApplication.Active` a bloccato (o un sotto-stato "sospeso per licenza"), riusando il meccanismo che **già** blocca l'accesso agli applicativi non attivi (`TenantSchemaAccessor`). È il blocco più "forte" e gratis: la funzione sparisce come un applicativo spento.
2. **Backend per-azione** (granularità fine, futura): attributo/filtro `[RequiresLicense(ApplicationType.Crm)]` sui controller che interroga `ILicenseGuard`; su `Expired` → messaggio utente dedicato (non un 500).
3. **Frontend**: estendere `AuthService.hasApplication` con lo stato licenza (es. `isLicensed(app)`), così il menu e i pulsanti mostrano lo stato "scaduto / rinnova" invece di sparire di colpo, con link alla pagina licenze.

### 8.3 Distinzione importante

"Scaduto" (licenza) è **diverso** da "nascosto per scelta" (`CompanyMenuRestriction`) e da "non attivato" (`CompanyApplication`). I tre stati coesistono: la UI deve dire *perché* una funzione non c'è (non attivata / non licenziata-scaduta / nascosta dall'admin).

---

## 9. Registrazione: scelta del tipo di soggetto (punto 1)

Oggi la registrazione pubblica crea solo aziende (§1.4). Serve introdurre la scelta **Company / FinancialAdvisor**:

- **Frontend** (`register-page.component.ts`): aggiungere un selettore "Mi registro come **Azienda** / **Studio (consulente)**" in cima al form; i campi comuni restano, cambiano etichette e (eventualmente) i nomi admin predefiniti.
- **Backend** (`RegistrationService`): `CompanyRegistrationRequest` acquisisce un campo `TipoSoggetto`; alla conferma, se `FinancialAdvisor`, creare uno **studio** (sotto il Dealer DEMO) invece di un'azienda, riusando il CRUD esistente (`AccountService.FinancialAdvisorPut` invece di `CompanyPut`).
- **Collegamento con le licenze**: dopo la conferma, il soggetto parte **senza licenza** (o con il piano **Free** dell'ambito, se previsto) e viene portato alla pagina "Gestione licenze" per sottoscrivere.

Da decidere (§13): se la registrazione di uno Studio dallo showcase pubblico sia davvero voluta (oggi gli studi li crea il Dealer/SuperUser) o se la scelta serva solo a instradare la licenza.

---

## 10. Menu e UI (punto 10)

### 10.1 Voce di menu

Aggiungere sotto **Assistenza** (`Pages4Support` in `main-menu-infrastructure.service.ts`, dove stanno già ticket, documenti AI, manuale) una voce **"Gestione licenze"**:
- una sola voce, contenuto diverso per ruolo (il server ricontrolla i permessi):
  - **SuperUser**: gestione listini globali + abilitazione Dealer;
  - **DealerPowerAdmin** (se `PuoGestireListini`): gestione dei propri listini;
  - **CompanyPowerAdmin / FinancialAdvisorPowerAdmin**: stato licenze del proprio soggetto, sottoscrivi/rinnova, storico pagamenti.
- `MainMenuItem` con `roles` adeguati; essendo in Assistenza è in fondo, visibile ai soggetti collegati.

### 10.2 Pagine (Angular, `BasePageConfirmCancelComponent`)

- **Listini** (SuperUser/Dealer): elenco listini per TipoSoggetto/Ambito, modifica piani e prezzi, flag `DisponibileSuCompany`. Sul modello di `/storageplanlist`.
- **Gestione licenze del soggetto**: stato per ambito (Attivo/In scadenza/Scaduto), pulsante Sottoscrivi/Rinnova, scelta piano+periodicità, avvio pagamento, storico `Lic_Pagamento`.
- Lingua attiva (transloco, chiavi `menu.*`/`pages.*`), pulsanti predefiniti del progetto.

---

## 11. Multi-tenant e impatti sui contesti

- Tutte le `Lic_*` stanno in `public` (trasversali ai tenant): nessuno schema per-azienda, nessuna FK dagli schemi applicativi verso di loro.
- I contesti applicativi **non** referenziano le `Lic_*`: l'inibizione (§8) passa dall'infrastruttura (`ILicenseGuard`, `CompanyApplication`), non da query cross-schema.
- Migration nel `PublicDbContext` (Infrastructure). Nessun impatto sugli schemi `tenant_*`.

---

## 12. Autorizzazioni (riepilogo)

| Azione | Chi |
|---|---|
| Definire listini globali, abilitare Dealer | SuperUser |
| Definire listini propri | DealerPowerAdmin del centro, se abilitato |
| Sottoscrivere/rinnovare per l'azienda | CompanyPowerAdmin dell'azienda |
| Sottoscrivere/rinnovare per lo studio | FinancialAdvisorPowerAdmin dello studio |
| Vedere stato licenze | PowerAdmin/Admin del soggetto |

Regola ferma: ruoli e appartenenza dal **token**, mai dal client (come `ApplicationActivationAuthorizer`). Riuso di `IApplicationActivationAuthorizer` dove possibile.

---

## 13. Punti aperti (da decidere con l'utente prima di implementare)

**Modello e listini**
1. Prezzo come due colonne (Mensile/Annuale) sul piano, o tabella figlia per più periodicità? (proposta: due colonne)
2. Cumulativo come `Ambito` con prezzo proprio o come bundle/sconto sui singoli? (proposta: `Ambito` a sé)
3. Valuta unica (EUR) o multi-valuta? (proposta: EUR)
4. I piani portano già **limiti/capability** (es. quote), o solo prezzo/accesso in v1? (proposta: solo accesso in v1, capability predisposte)

**Attivazione e licenze**
5. La sottoscrizione **attiva automaticamente** l'applicativo, o resta la doppia azione (SuperUser/Dealer attiva + licenza copre)?
6. Periodo di **grace** dopo la scadenza (giorni in cui si avvisa ma non si blocca)? Durata?
7. ~~Ereditarietà studio→aziende (punto 12): **piena** o **a tetto**?~~ **Deciso il 09/11/2026 (§4.3)**: copertura piena e automatica di tutte le aziende dello studio, prezzo fisso, upgrade autonomo dell'azienda a un piano superiore.

**Registrazione e pagamenti**
8. La registrazione pubblica di uno **Studio** è voluta, o la scelta serve solo a instradare la licenza? (punto 1)
9. Provider carte: **Stripe** o altro? PayPal con quale SDK/flow (redirect/REST)?
10. Rimborsi/upgrade-downgrade a metà periodo: in v1 o rimandati?

**Perimetro v1**
11. Fatturazione/documenti fiscali: fuori dalla v1? (proposta: sì, fuori)
12. Quali funzioni inibire a scadenza (punto 9): confermato che si predispone solo il gancio e si rimanda la scelta?

---

## 14. Fasi di implementazione (bozza, dopo le decisioni §13)

**Fase 0 – Modello (Infrastructure_Dev)**
- Entità `Lic_Listino`, `Lic_Piano`, `Lic_Abbonamento`, `Lic_Pagamento`; enum `TipoSoggetto`, `Periodicita`, stati; flag `Dealer.PuoGestireListini`; repository `Repositories/Public`; migration `PublicDbContext`.

**Fase 1 – Listini (SuperUser/Dealer)**
- Servizi + controller di gestione listini e piani; `ILicenseCatalogResolver` (§4.1); pagina Angular listini (sul modello `/storageplanlist`); autorizzazioni.

**Fase 2 – Sottoscrizione senza pagamento reale**
- `Lic_Abbonamento`, flusso sottoscrivi/rinnova con **`DebugPaymentGateway`** (§7.2); pagina "Gestione licenze" del soggetto; voce menu Assistenza; hosted service dei rinnovi/avvisi.

**Fase 3 – Pagamenti reali**
- `IPaymentGateway` + Stripe/PayPal; webhook idempotenti; configurazione secret; coerenza col server di prova (debug di default).

**Fase 4 – Inibizione (quando pianificata)**
- `ILicenseGuard`; aggancio a `CompanyApplicationService`/`TenantSchemaAccessor`; attributo `[RequiresLicense]`; stato licenza in `AuthService`/menu; messaggi utente dedicati.

**Fase 5 – Registrazione per tipo soggetto (punto 1)**
- Selettore in `register-page`; `TipoSoggetto` in `CompanyRegistrationRequest`; instradamento Studio/Azienda in `RegistrationService`.

---

### Riferimenti nel codice

- Attivazione: `nvxapp.server.service/.../CompanyApplication/CompanyApplicationService.cs`, `ApplicationActivationAuthorizer.cs`; `Entities/Public/CompanyApplication.cs`, `FinancialAdvisorApplication.cs`.
- Applicativi/tenant: `nvxapp.server.data/Infrastructure/Tenancy/ApplicationType.cs`, `TenantSchemaName.cs`, `TenantSchemaAccessor.cs`.
- Precedente piani: `StorageQuotaService.cs`, pagine `/storageplanlist`, `/companystoragelist`.
- Registrazione: `register-page.component.ts`, `RegistrationService.cs`, `Entities/Public/CompanyRegistrationRequest.cs`.
- Menu/auth: `main-menu-infrastructure.service.ts` (`Pages4Support`), `auth.service.ts` (`hasApplication`).
- Soggetti: `Entities/Public/{Company,FinancialAdvisor,Dealer}.cs`, `ApplicationRole.cs` (`RoleCode`).
