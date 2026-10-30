# Analisi – Cancellazione fisica di Dealer, FinancialAdvisor, Company e User

Branch di sviluppo: **Infrastructure_Dev**.
Documento di sola analisi: nessun codice è stato scritto.

> La richiesta si interrompe a "se elimino un …". Si assume il significato naturale: eliminando un nodo si elimina tutto ciò che sta sotto
> (Dealer → i suoi FinancialAdvisor → le loro Company → i loro utenti e dati).

---

## 1. Situazione attuale

- Gerarchia: `Dealer` → `FinancialAdvisor.IdDealer` → `Company.IdFinancialAdvisor`. Gli utenti sono legati ai nodi da `UserDealer`, `UserFinancialAdvisor`, `UserCompany` (con `MainUser` e `Disabled`).
- **Nessuna funzione di cancellazione** per questi elementi: `AccountService` ha solo List/Get/Put.
- Nel database però le FK sono già in **cascade** (`PublicDbContext_Infrastructure.cs`): Dealer → FinancialAdvisor → Company, nodo → `User*`, AspNetUsers → `User*`. Il commento nel codice dice già "MODIFICARE SENNO RASA VIA TUTTO": un `DELETE` su un Dealer oggi cancella a catena studi e aziende, ma **non** gli utenti (AspNetUsers resta), né gli schemi tenant.
- Altre tabelle legate all'azienda: `CompanyApplication`, `CompanyMenuRestriction` (cascade), `CompanyRegistrationRequest` (`IdCompany` facoltativo), ticket (`IdCompany`/`IdFinancialAdvisor`/`IdDealer` senza FK, per scelta: "la cronologia resta"), e i nuovi `Document` / `TicketTypeAiSetting` (vedi `ANALISI_DOCUMENTAZIONE_AI.md`).
- Dati degli applicativi:
  - **multi-tenant:** uno schema per azienda e applicativo, `tenant_<IdCompany>_<App>` (`TenantSchemaName`);
  - **modalità singola:** tabelle degli applicativi in `public`, legate all'azienda da `Az_Anagrafica.IdCompany` (FK cascade verso `Company`) e agli utenti da `Dip_Anagrafica`, `Az_SediRepartoUser`, `Az_SubCommessaUser` (FK cascade verso AspNetUsers); `Dip_GG_Richiesta.IdAspNetUsers` è da verificare (campo senza navigation).

---

## 2. Principio: cancellazione esplicita, non affidata alle cascade

**Raccomandazione:** portare a `Restrict` le cascade Dealer → FinancialAdvisor → Company, e fare la cancellazione con un **servizio dedicato** che elimina in ordine, dal basso verso l'alto.

Motivi:
- la cascade del database non arriva agli **utenti** (AspNetUsers è il "figlio" di `User*`, non il contrario) né agli **schemi tenant**, quindi da sola lascia residui;
- un `DELETE` sbagliato (anche da SQL a mano) oggi rade al suolo un intero ramo senza controlli;
- serve un'**anteprima** con i conteggi prima di confermare, che si ottiene solo percorrendo l'albero nel servizio.

Le cascade "di proprietà" restano (nodo → `User*`, `Company` → `CompanyApplication` / `CompanyMenuRestriction`, nodo → `Document`): sono figli che non hanno senso senza il padre.

---

## 3. Cosa si elimina per ogni elemento

### 3.1 User (utente)

Un utente può essere collegato a più nodi (più aziende, o azienda + studio) e avere più ruoli. Due operazioni distinte:

| Operazione | Effetto |
|---|---|
| **Togli dal nodo** | elimina solo il collegamento `User*` di quel nodo (e i ruoli che dipendono solo da quel collegamento). Se resta collegato ad altri nodi, l'utente resta |
| **Elimina utente** | possibile solo se non ha più collegamenti (oppure, per il SuperUser, eliminando tutti i collegamenti). Cancella AspNetUsers |

Con l'eliminazione dell'utente:
- tabelle Identity (ruoli, claims, login, token): cascade standard di Identity;
- dati presenze del dipendente **in ogni azienda** in cui compare: `Dip_Anagrafica` e figli, `Az_SediRepartoUser`, `Az_SubCommessaUser`, richieste (`Dip_GG_Richiesta`). In multi-tenant le FK puntano a `public.AspNetUsers` da schemi diversi: verificare che le FK cross-schema esistano davvero con `ON DELETE CASCADE`, altrimenti vanno cancellati esplicitamente schema per schema;
- ticket **aperti** dall'utente: eliminati (con cronologia e allegati), salvo la scelta del §5;
- ticket di altri in cui l'utente compare come **autore** o **assegnatario** passato: restano, l'id diventa "utente eliminato" in visualizzazione;
- ticket **in carico** all'utente: tornano al loro livello senza assegnatario (stessa regola già prevista per gli utenti disabilitati);
- richieste di registrazione (`CompanyRegistrationRequest`) collegate: eliminate;
- sessioni: il token JWT dell'utente resta valido fino alla scadenza. Verificare che `ServiceBase` rifiuti un utente non più esistente; se si usa un refresh token salvato, va eliminato. Se qualcuno lo sta **impersonando**, la sessione di impersonificazione va chiusa.

Blocchi:
- non si può eliminare sé stessi né il SuperUser;
- non si può togliere/eliminare l'**ultimo PowerAdmin** di un nodo che resta in vita (regola dell'analisi ticket: "ogni nodo ha sempre un PowerAdmin"). Il blocco non vale se si sta eliminando il nodo stesso.

### 3.2 Company (azienda)

In ordine:
1. **Dati degli applicativi:**
   - multi-tenant: `DROP SCHEMA tenant_<Id>_<App> CASCADE` per ogni applicativo che ha uno schema (attivo o disattivato: lo schema resta anche dopo la disattivazione);
   - modalità singola: cancellazione delle righe dell'azienda nelle tabelle degli applicativi, tramite il loro DbContext (oggi la cascade da `Az_Anagrafica` copre gran parte, ma va verificata tabella per tabella: alcune relazioni sono `Restrict`).
2. **Utenti dell'azienda:** per ognuno si toglie il collegamento `UserCompany`; gli utenti che restano **senza collegamenti** si eliminano (§3.1). Un consulente collegato anche ad altre aziende resta.
3. **Ticket** con `IdCompany` = azienda: eliminati (salvo §5). I ticket aperti da altri e assegnati agli utenti eliminati: vedi §3.1.
4. `Document` e `TicketTypeAiSetting` dell'azienda (cascade).
5. `CompanyApplication`, `CompanyMenuRestriction` (cascade), `CompanyRegistrationRequest` collegate.
6. Job Hangfire in coda o ricorrenti legati all'azienda (export, import, ricalcoli): annullati prima di cancellare; un job che parte su un'azienda inesistente deve chiudersi senza errori.
7. Eventuali file su disco o cache per azienda (sessioni della chat AI in `ChatSessionStore`, file temporanei di export): da censire.
8. `Company`.

### 3.3 FinancialAdvisor (studio)

1. Ogni **Company** dello studio (§3.2).
2. Utenti dello studio: tolto `UserFinancialAdvisor`, eliminati quelli rimasti senza collegamenti.
3. Ticket con `IdFinancialAdvisor` = studio (quelli delle aziende sono già andati al punto 1; restano quelli aperti dallo studio).
4. `Document` / `TicketTypeAiSetting` dello studio.
5. `FinancialAdvisor`.

### 3.4 Dealer (centro)

1. Ogni **FinancialAdvisor** del centro (§3.3, che a sua volta elimina le aziende).
2. Utenti del centro (`UserDealer`), come sopra.
3. Ticket con `IdDealer` = centro.
4. `Document` / `TicketTypeAiSetting` del centro.
5. `Dealer`.

---

## 4. Transazione e ordine

- PostgreSQL ha il **DDL transazionale**: anche `DROP SCHEMA` si annulla con un rollback. Tutta la cancellazione può stare in **una sola transazione** se gli statement passano dalla stessa connessione (es. `PublicDbContext` con SQL esplicito per gli schemi, oppure i DbContext degli applicativi che condividono connessione e transazione).
- Se non si riesce a condividere la transazione: prima i dati degli applicativi, poi `public`; un errore a metà lascia un'azienda "senza dati applicativi" ma ancora presente, che si può ricancellare (operazione **ripetibile**: ogni passo tollera di trovare già vuoto).
- Un Dealer grande (molti studi e aziende) può richiedere tempo: eseguire la cancellazione come **job in background** (Hangfire, con il `LongJobNotifier` già presente) con avanzamento, invece che nella richiesta HTTP. Un'azienda singola può restare sincrona.
- Durante la cancellazione il nodo va **bloccato** (es. flag "in eliminazione" o controllo all'ingresso), per evitare che qualcuno ci lavori o ci sposti aziende (`CompanyMove`) a metà.

---

## 5. Ticket: conservare o eliminare?

L'analisi ticket dice "nessuna cascade, la cronologia va conservata". Con la cancellazione **fisica** le due regole vanno conciliate:

- **Proposta:** eliminare i ticket **nati** nel ramo eliminato (`IdCompany` / `IdFinancialAdvisor` / `IdDealer` del ramo), con cronologia e allegati. Sono dati del cliente che se ne va.
- I ticket nati altrove restano; i riferimenti agli utenti eliminati diventano "utente eliminato".
- Alternativa, se la cronologia serve per statistiche: anonimizzare invece di eliminare (testo e allegati cancellati, righe tenute). Più complessa, da fare solo se c'è un'esigenza concreta.

---

## 6. Chi può eliminare

| Elemento | Proposta |
|---|---|
| Dealer | solo SuperUser |
| FinancialAdvisor | SuperUser; DealerPowerAdmin del centro padre (da decidere) |
| Company | SuperUser; FinancialAdvisorPowerAdmin dello studio padre (da decidere) |
| User | PowerAdmin del nodo per "togli dal nodo"; eliminazione definitiva solo se l'utente non ha altri collegamenti, altrimenti solo SuperUser |

Tutti i controlli nel servizio, con il nodo preso dal token (come in `piano-security.md`), mai dal client.

---

## 7. Interfaccia

1. Comando **Elimina** nelle liste/pagine di modifica di Dealer, FinancialAdvisor, Company e utenti, visibile solo a chi può.
2. **Anteprima** prima della conferma, calcolata dal server: numero di studi, aziende, utenti eliminati, utenti che restano perché collegati altrove, dipendenti, ticket, documenti, applicativi con dati.
3. **Conferma forte:** digitare il nome del nodo; per Dealer e studi con aziende, avviso esplicito che l'operazione non è reversibile.
4. Esito con riepilogo; per le cancellazioni in background, avanzamento e notifica a fine job.

---

## 8. Aspetti legali e di sicurezza

- **Conservazione obbligatoria:** i dati delle presenze alimentano le paghe e il LUL, che hanno obblighi di conservazione pluriennali. Prima della cancellazione proporre (o rendere obbligatorio) un **export** completo dei dati dell'azienda, e chiarire contrattualmente chi conserva cosa.
- **Diritto alla cancellazione (GDPR):** la cancellazione fisica di un utente lo soddisfa, ma i dati restano nei **backup** del database fino alla loro scadenza: va scritto nell'informativa.
- **Registro:** salvare chi ha eliminato cosa e quando (log Serilog strutturato e, meglio, una tabella di registro con descrizione e conteggi, senza dati personali), perché dopo la cancellazione non resta altra traccia.
- **Alternativa morbida:** la disattivazione esiste già per gli utenti (`Disabled`). Valutare un passaggio intermedio "disattivato → eliminabile dopo N giorni", per proteggersi dagli errori.

---

## 9. Domande aperte

1. Conferma del significato della frase interrotta ("se elimino un …"): eliminazione a cascata di tutto il ramo sotto il nodo?
2. Chi può eliminare studi e aziende oltre al SuperUser (§6)?
3. Ticket del ramo eliminato: eliminati o anonimizzati (§5)?
4. Export obbligatorio prima di eliminare un'azienda (§8)?
5. Periodo di "disattivazione" prima della cancellazione definitiva?

---

## 10. Dove andranno le modifiche (tutto in Infrastructure)

- `nvxapp.server.data/Infrastructure/PublicDbContext_Infrastructure.cs`: cascade Dealer → FA → Company portate a `Restrict` (migration)
- `nvxapp.server.data/Infrastructure/Tenancy/TenantProvisioningService`: metodo per eliminare gli schemi di un'azienda
- `nvxapp.server.service/ClientServer-Service/infrastructure/Deletion` (nuovo): `DeletionService` con anteprima ed esecuzione per Dealer, FinancialAdvisor, Company, User; ogni applicativo espone un "cancellatore" dei propri dati per la modalità singola (interfaccia registrata per `ApplicationType`, come i DbContext in `ApplicationDbContextRegistry`)
- `SupportTicket`: rilascio dei ticket assegnati a utenti eliminati
- `Controllers/Infrastructure/DeletionController.cs` (solo POST): anteprima, esecuzione
- client: comando Elimina e dialog di anteprima/conferma nelle pagine di Dealer, studi, aziende e utenti; traduzioni
