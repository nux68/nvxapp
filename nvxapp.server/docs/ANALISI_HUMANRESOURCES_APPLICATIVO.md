# Analisi – Applicativo Risorse Umane (HumanResources)

Branch: **HumanResources_Dev** (riceve i merge da Infrastructure, si mergia in HumanResources).
Documento generale: `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` (§4.1, aree `HR-*` in §10.2), punto `ANA.8`.

Stato (11/11/2026): **analisi scritta, scheletro del modulo fatto** (`HR-BASE`, ramo di lavoro `HumanResources_Dev_hr-base`); **aree funzionali fatte** (`HR-CED`, `HR-FOR`, `HR-ONB`, ramo di lavoro `HumanResources_Dev_f3`, §7.1), da mergiare. Le decisioni aperte sono in §8 (con le scelte provvisorie fatte nelle aree).

---

## 1. Obiettivo

Il fascicolo del dipendente **al di là delle presenze**. Le Presenze restano il modulo delle timbrature, delle giornate e dell'export paghe; Risorse Umane raccoglie quello che l'ufficio del personale (o lo studio paghe) gestisce sul dipendente nel corso del rapporto:
- **Portale del dipendente con cedolini e CU** (`HR-CED`): lo studio o l'azienda carica il PDF unico, il sistema lo divide per dipendente, lo pubblica nell'area del dipendente e chiede la presa visione.
- **Formazione e competenze** (`HR-FOR`): corsi, attestati, piani di formazione sulle competenze (in Common dopo `INF-COM.2`), scadenze nello scadenzario comune.
- **Onboarding e offboarding** (`HR-ONB`): checklist di assunzione e cessazione legate alle date in Common (D6), documenti da consegnare o firmare, creazione e disattivazione dell'account.

Il modulo è indipendente: funziona anche senza Presenze, Documentale e Compliance. Le integrazioni con gli altri moduli sono facoltative (§6).

## 2. Applicativo e schema

| Voce | Valore |
|---|---|
| `ApplicationType` | `HumanResources = 5` |
| Schema | `tenant_<IdAzienda>_5` (in modalità singola `public`) |
| Prefisso tabelle | `Hr_` |
| Contesto | `HumanResourcesDbContext` (migration in `Migrations/HumanResources`, storico `__EFMigrationsHistory_HumanResources`) |
| Licenza | `AmbitoLicenza.HumanResources = 5`, nessun limite (`INF-APP.3`); listini con prezzi segnaposto (§8, D8) |
| Codice del manuale | `humanresources` (`Manuals/humanresources.<lingua>.md`) |
| Cartella i18n del client | `assets/i18n/humanresources/`, chiavi sotto `hr.` |
| Cartelle | server `Controllers/HumanResources`, `ClientServer-Service/HumanResources`, `Entities/Tenant/HumanResources`, `Repositories/Tenant/HumanResources`; client `ClientServer-Service/HumanResources`, `pages/HumanResources` |

## 3. Ruoli e visibilità

| Ruolo | Cosa fa |
|---|---|
| **CompanyPowerAdmin** | Configurazione del modulo; tutto il resto |
| **CompanyAdmin** (ufficio del personale) | Carica cedolini e CU, gestisce corsi, checklist e fascicoli di tutti i dipendenti; vede le conferme di presa visione |
| **User** (dipendente) | **Area del dipendente**: i propri cedolini e CU, i propri corsi e attestati, i documenti da leggere o firmare. Vede solo i propri dati |
| **FinancialAdvisorPowerAdmin/Admin** (studio paghe) | Carica il PDF unico dei cedolini per le aziende dello studio (lato studio, `getStudioRoutes`), vede gli scarti della divisione. Non vede i fascicoli |

Regole:
- i permessi li controlla il server in ogni servizio (`HrUtility.ContestoAsync`, come `CrmUtility`); le pagine li rispecchiano soltanto;
- il dipendente è collegato al proprio utente tramite `Dip_Anagrafica.IdAspNetUsers` (Common): senza utente non ha l'area del dipendente, ma i suoi documenti restano consultabili dall'ufficio del personale;
- i cedolini sono **dati personali** (retribuzione): accesso solo al dipendente, all'ufficio del personale e allo studio che li carica; esclusi dalla chat AI salvo le sole funzioni dell'utente sui propri dati (da decidere, §8).

## 4. Modello dati proposto (schema `tenant_<id>_5`, prefisso `Hr_`)

Tutte le tabelle hanno `IdAz_Anagrafica` (FK verso `Az_Anagrafica` di Common) e i campi di `BaseEntity`. I riferimenti al dipendente sono verso `Dip_Anagrafica` (Common, sola lettura nel contesto, `ExcludeFromMigrations`).

### 4.1 Configurazione (fatta in `HR-BASE`)
- `Hr_Cfg` (una riga per azienda, creata all'attivazione): `GiorniPreavviso` (preavviso predefinito delle scadenze registrate dal modulo), `NotificheAttive`, `Note`. Le prossime aree aggiungono qui i propri parametri (es. criterio di divisione dei cedolini, giorni per la presa visione).

### 4.2 Cedolini e CU (`HR-CED`)
- `Hr_DocCaricamento`: un caricamento del PDF unico (tipo: cedolini/CU, periodo mese/anno, chi ha caricato, studio o azienda, stato: in elaborazione / da verificare / pubblicato, numero pagine, chiave del file originale in `IFileStorage`).
- `Hr_DocDipendente`: documento del dipendente estratto dal caricamento (`IdHr_DocCaricamento`, `IdDip_Anagrafica`, tipo, periodo, pagine, chiave del file, data di pubblicazione, metodo di abbinamento: codice fiscale / matricola / AI / manuale).
- `Hr_DocScarto`: pagine non abbinate da verificare (pagine, testo riconosciuto, proposta dell'AI, esito).
- I file stanno fuori dal database (`IFileStorage`), come gli allegati del CRM.

### 4.3 Formazione e competenze (`HR-FOR`)
- `Hr_Corso`: catalogo dei corsi (titolo, ente, durata ore, validità in mesi, competenza collegata `Par_Competenza` in Common, obbligatorio sì/no).
- `Hr_CorsoEdizione`: edizione di un corso (date, sede `Az_Sedi` in Common, docente, note).
- `Hr_CorsoPartecipante`: dipendente iscritto a un'edizione (esito, data di conseguimento, data di scadenza calcolata, attestato in `IFileStorage`). Alla chiusura con esito positivo aggiorna `Dip_Competenza` (Common) tramite i servizi comuni.
- `Hr_PianoFormazione`, `Hr_PianoFormazioneRiga`: piano per anno (dipendente o reparto, corso, entro quando).

### 4.4 Onboarding e offboarding (`HR-ONB`)
- `Hr_ChecklistModello`, `Hr_ChecklistModelloVoce`: modelli di checklist (tipo: assunzione / cessazione; voci con responsabile, giorni rispetto alla data, documento da consegnare o firmare sì/no).
- `Hr_Checklist`, `Hr_ChecklistVoce`: checklist del dipendente generata dal modello quando cambia la data di assunzione o di cessazione in Common (D6), con stato di ogni voce.

## 5. Uso dei servizi comuni (Infrastructure)

| Servizio | Dove | Uso in HR |
|---|---|---|
| Scadenzario `IDeadlineRegistry` | `ClientServer-Service/infrastructure/Deadline` | Scadenze dei corsi (`hr.corso`, riferimento `Hr_CorsoPartecipante.Id`), voci delle checklist (`hr.checklist`), presa visione non confermata entro n giorni. Preavviso predefinito da `Hr_Cfg.GiorniPreavviso`. La chiusura (`CloseAsync`) quando il corso è rinnovato o la voce completata |
| Notifiche `INotificationSender` | `ClientServer-Service/infrastructure/Notification` | "Nuovo cedolino disponibile", promemoria delle voci di checklist, corsi in scadenza. Una categoria propria (`INotificationCategoryProvider`, es. `hr`) se serve separare le preferenze. Rispettare `Hr_Cfg.NotificheAttive` |
| Presa visione `IAcknowledgementService` | `ClientServer-Service/infrastructure/Acknowledgement` | Cedolini e CU (`hr.cedolino`), documenti dell'onboarding (`hr.checklist.voce`). Con il Documentale attivo (`DM-FIRMA`) la stessa chiamata passa alla firma FES |
| Dati Common | `Dip_Anagrafica`, date di assunzione/cessazione (`INF-COM.4`), `Az_Sedi`/`Az_SediReparto` (`INF-COM.1`), `Par_Competenza`/`Dip_Competenza` (`INF-COM.2`) | Lettura nel contesto HR con repository `Hr_<Entità>ReadRepository`, scrittura con i repository di `Repositories/Tenant/Common` |
| Cancellazione `IApplicationDataDeleter` | `HrDataDeleter` | Oggi solo `Hr_Cfg` in modalità singola. Con i dati: conteggi per l'anteprima, cancellazione per azienda in modalità singola, dati dell'utente rimosso (i documenti del dipendente restano all'azienda per gli obblighi di conservazione: da decidere, §8) |
| Licenze, job, AI | `ILicenseGuard`, job per export/PDF, provider AI configurato | La divisione dei cedolini è un **job** (come gli export); l'AI aiuta l'abbinamento delle pagine non riconosciute |

## 6. Integrazioni facoltative

- **Presenze**: la cessazione chiude il rapporto nelle Presenze (data in Common, D6); le Presenze leggono le stesse date.
- **Documentale**: archiviazione dei cedolini e firma dei documenti dell'onboarding tramite `IAcknowledgementService` (`DM-FIRMA`).
- **Compliance**: i corsi obbligatori della sicurezza (`CMP-81.1`) possono usare il catalogo dei corsi di HR. Da decidere chi è il proprietario del catalogo (§8).
- **Risorse aziendali**: all'offboarding l'elenco dei beni da restituire (`AST-BENI.3`).

## 7. Fatto in `HR-BASE` (11/11/2026)

- Server: `HumanResourcesDbContext` (whitelist dei tipi, `Az_Anagrafica`/`Company`/`AspNetUsers` in sola lettura con `ExcludeFromMigrations`), registrato in `Installers4HumanResources`; design-time factory in un file proprio (`HumanResourcesDbContextDesignTimeFactory.cs`, come il CRM); migration `InitHumanResources` con `Hr_Cfg`; repository `Hr_CfgRepository`, `Hr_Az_AnagraficaReadRepository`; `HrUtility` (contesto e ruoli), `HrCompanyInitializer` (priorità 50, crea `Hr_Cfg`), `HrDataDeleter`; `Hr_ConfigService` + `Hr_ConfigController` (`api/Hr_Config/Hr_CfgGet` per gli amministratori, `Hr_CfgPut` solo CompanyPowerAdmin); testi `HumanResourcesMessages` (it/en); manuale `humanresources` (it/en); listini `HrLicenseCatalogSeed`.
- Client: pagina `/hrsettings` (`HrSettingsPageComponent`, da `BasePageConfirmCancelComponent`), voce di menu "Risorse Umane › Impostazioni" (CompanyPowerAdmin, gruppo 50), manuale nella sezione Assistenza, rotta, cartella i18n `humanresources` (`i18n-human-resources.ts` in `i18n-folders.ts`).
- Widget: nessuno (catalogo vuoto, arrivano con le aree). Comandi vocali: nessuno.
- Prova su DB usa e getta: creazione azienda, attivazione come SuperUser (non bloccata dalle licenze), schema `tenant_<id>_5` con `Hr_Cfg` creata dalla migration e riga creata dall'inizializzatore, Get/Put con CompanyPowerAdmin, Get in sola lettura e Put rifiutata per il CompanyAdmin (12 controlli, tutti ok).
- L'area del dipendente (`HR-BASE.2`) è prevista nei ruoli (§3) ma non ha ancora pagine: nasce con `HR-CED`.

### 7.1 Fatto in `HR-CED`, `HR-FOR`, `HR-ONB` (11/11/2026, ramo `HumanResources_Dev_f3`)

**Dati** (migration incrementale `HrCedoliniFormazioneOnboarding`): le tabelle del §4.2–4.4 (`Hr_DocCaricamento`, `Hr_DocDipendente`, `Hr_DocScarto`, `Hr_Corso`, `Hr_CorsoEdizione`, `Hr_CorsoPartecipante`, `Hr_PianoFormazione`, `Hr_PianoFormazioneRiga`, `Hr_ChecklistModello`, `Hr_ChecklistModelloVoce`, `Hr_Checklist`, `Hr_ChecklistVoce`), un repository per tabella; `Dip_Anagrafica`, `Par_Competenza`, `Az_Sedi` in sola lettura nel contesto (`Hr_<Entità>ReadRepository`). I documenti del dipendente hanno la chiave esterna `Restrict` verso `Dip_Anagrafica` (restano dopo l'uscita, HR-D4). File in `IFileStorage` (PDF unico, documenti divisi, attestati, documenti delle checklist). `HrDataDeleter`: conteggi e cancellazione per azienda in modalità singola; utente rimosso: solo i responsabili delle voci tornano vuoti.

**Servizi** (`ClientServer-Service/HumanResources`, logica in `_utility/HrCedoliniCore`, `HrOnboardingCore`, `HrPdf`):
- `Hr_CedolinoService` (`api/Hr_Cedolino/*`): caricamento multipart del PDF unico (fino a 100 MB), divisione in un job (`RunInBackground` + `ILongJobNotifier`, avanzamento nella barra dei lavori): testo per pagina con PdfPig, codice fiscale dei dipendenti, poi "Matricola …", nuovo PDF per dipendente con PDFsharp; pagine senza un solo dipendente come scarti, con la proposta del dipendente con cognome e nome nella pagina. Assegnazione (ricompone il PDF del dipendente; dopo la pubblicazione lo pubblica subito) e scarto; pubblicazione in un job con la presa visione (`hr.cedolino`, con il PDF per il canale del documentale) e la notifica "nuovo documento" (se attive); report delle conferme nel dettaglio; "I miei cedolini" con download e conferma (servizio comune delle prese visione). **Studio**: `HrUtility.ContestoCedoliniAsync` accetta l'azienda indicata dallo studio (dello studio, Risorse Umane attivo, abilitato e con licenza per lo studio) e il lavoro gira in un nuovo scope con `TenantScope` (`HrAzienda.RunAsync`), come i job.
- `Hr_DipendenteService` (`api/Hr_Dipendente/*`): elenco e scheda dei dipendenti (anagrafica comune, scritture con `IDip_AnagraficaCommonRepository`: codice fiscale e matricola unici, date), creazione e disattivazione dell'account con `IAccountService.UserCompanyPut/UserCompanyGet` (ruolo User, limite utenti della licenza, inizializzatori).
- `Hr_FormazioneService` (`api/Hr_Formazione/*`): corsi, edizioni con partecipanti (esito, conseguimento, scadenza = conseguimento + validità), attestati, piani per anno con l'avanzamento calcolato; corso superato → `Dip_Competenza` in Common e scadenza `hr.corso` (riferimento il partecipante, preavviso `Hr_Cfg.GiorniPreavviso`, chiusa al rinnovo, tolta se l'esito cambia).
- `Hr_OnboardingService` (`api/Hr_Onboarding/*`): modelli (solo CompanyPowerAdmin, uno predefinito per tipo), checklist generate dai modelli predefiniti sulle date di `Dip_Anagrafica` dagli ultimi 30 giorni in avanti (all'apertura dell'elenco e al salvataggio del dipendente da HR) e spostate se la data cambia prima di iniziarle, voci con scadenza `hr.checklist` (responsabile della voce o amministratori), documento con presa visione `hr.checklist.voce` (la conferma completa la voce), azioni sull'account dalle voci, annullamento.

**Client** (`pages/HumanResources`): dipendenti (elenco e scheda con account), cedolini e CU (elenco, caricamento, dettaglio con documenti, report delle conferme, pagine da verificare e pubblicazione) anche lato studio (`/hrstudiopayslips`, `/hrstudiopayslipupload`, `/hrstudiopayslipdetail` in `getStudioRoutes`, voce in `Pages4FinancialAdvisorAdmin`), "I miei cedolini" (`Pages4User`), corsi ed edizioni, piani di formazione, modelli e checklist. Pagine di modifica da `BasePageConfirmCancelComponent`, "aggiungi" con il **+** del FAB, bottoni predefiniti, dialog comune dei dipendenti, i18n it/en. Manuale `humanresources` aggiornato (it/en).

**Prova su DB usa e getta** (`nvx_test_hrf3`, 80 controlli, tutti ok): azienda e attivazione HumanResources = 5, abilitazione dello studio; dipendenti con codice fiscale e matricola (duplicato rifiutato), account creato senza anagrafiche doppie; PDF di 7 pagine generato al volo: Rossi pagine 1–2 e Bianchi 3 per codice fiscale, Verdi 4 per matricola, scarti 5–7 con la proposta su Neri e il motivo "più dipendenti", assegnazione e scarto, PDF diviso di 2 pagine; pubblicazione (job), notifica, presa visione, "I miei cedolini", download proprio sì / altrui no, conferma e report (1 su 4, 2 senza account); studio: rifiutato senza licenza (licenze attive), aziende dello studio, caricamento delle CU per l'azienda, rifiuto senza azienda o su azienda non sua, cancellazione; corso con competenza, edizione con scadenza +12 mesi nello scadenzario, competenza in Common, attestato, rinnovo che chiude la scadenza precedente, piano 1 su 2; modelli (rifiutati al CompanyAdmin), checklist di assunzione generata dalla data e spostata al cambio di data, scadenze delle voci, documento con presa visione confermata che completa la voce, cessazione con disattivazione dell'account (ruoli conservati) e checklist completata.; manuale del modulo generato (job).

**Problemi aperti**
- ~~Creazione dell'account con anagrafica doppia~~ risolto con `INF-SVC.8`/`HR-ONB.4`: `Hr_AccountCrea` passa `IdDip_Anagrafica` ad `Account/UserCompanyPut`, che collega l'utente al dipendente esistente prima degli inizializzatori (nessuna anagrafica nuova, niente più scollegamento e cancellazione).
- ~~Categoria di notifica propria~~ risolto con `INF-SVC.7`/`HR-SEG.2`: categoria `hr.cedolini` (`HrNotificationCategoryProvider`, titolo `NotificationCategory_Cedolini` nei testi di HR, email predefinita), visibile nelle preferenze solo con Risorse Umane attivo.
- Area del dipendente per corsi e attestati ("I miei corsi"): l'attestato si scarica già anche dal dipendente (`Hr_AttestatoDownload`), la pagina non c'è.
- Pagine scansionate (senza testo): restano tutte scarti; niente OCR né AI.
- Scadenza della presa visione non confermata entro n giorni: non registrata.
- Spazio dei file: i file di HR non contano nello spazio dell'azienda (come gli allegati del CRM, nessun `IStorageUsageProvider`).
- Modalità singola: l'ordine dei `IApplicationDataDeleter` deve cancellare i dati di HR prima di `Dip_Anagrafica` (chiavi `Restrict`).

## 8. Decisioni aperte

| # | Decisione | Note |
|---|---|---|
| HR-D1 | Livelli Free/Standard/Pro e prezzi del modulo (D8, `ANA.10`) | Oggi listini con la struttura del CRM e **prezzi segnaposto**, nessun limite |
| HR-D2 | Chi carica i cedolini: solo lo studio, solo l'azienda o entrambi | Proposta: entrambi, con l'origine registrata nel caricamento. **Scelta provvisoria (HR-CED)**: entrambi; lo studio con licenza e applicativo abilitato, sulle aziende dello studio con HR attivo; l'origine (azienda/studio) e lo studio sono nel caricamento |
| HR-D3 | Criterio di abbinamento delle pagine (codice fiscale, matricola, AI) e cosa fare degli scarti | Proposta: codice fiscale, poi matricola, poi proposta AI da confermare a mano. **Scelta provvisoria (HR-CED)**: codice fiscale, poi matricola ("Matricola …"); al posto dell'AI una proposta per cognome e nome trovati nella pagina, sempre da confermare a mano; le pagine di seguito senza codice sono scarti |
| HR-D4 | Conservazione dei documenti del dipendente cessato o dell'utente rimosso | Obblighi di legge: i documenti restano all'azienda; l'utente perde solo l'accesso. **Scelta provvisoria**: così (cancellazione dell'utente: i documenti restano, chiavi `Restrict` verso il dipendente) |
| HR-D5 | Catalogo dei corsi: in HR, in Compliance o in Common | Se lo usano entrambi i moduli va in Common (regola del §2.2 del documento generale). **Scelta provvisoria (HR-FOR)**: in HR (`Hr_Corso`); se la Compliance lo usa (`CMP-81.1`) si sposta in Common |
| HR-D6 | Chat AI sui dati del modulo | Proposta: esclusi cedolini e CU; corsi e checklist ammessi per i ruoli che li vedono. **Scelta provvisoria**: per ora nessuna funzione della chat AI sul modulo |
| HR-D7 | Cartella i18n e `i18n-folders.ts` | Il ramo aggiunge `i18n-human-resources.ts` e una riga in `i18n-folders.ts` (file di Infrastructure): nel merge di AllFunction con gli altri moduli nuovi c'è un conflitto banale (tenere tutte le righe). In alternativa Infrastructure aggiunge gli stub `i18n-<app>.ts` a `null` come fece per il CRM |

## 9. Checklist

Legenda: ✅ svolto · ⏳ da fare · 🔄 in corso · 🔍 da decidere

#### `HR-BASE` – Avvio del modulo
- ✅ `HR-BASE.1` `HumanResourcesDbContext` registrato, migration `InitHumanResources` con `Hr_Cfg`, repository, inizializzatore e `HrDataDeleter`
- ✅ `HR-BASE.2` Cartelle del modulo, servizio/controller della configurazione, pagina `/hrsettings`, ruoli (§3); area del dipendente prevista, pagine con `HR-CED`
- ✅ `HR-BASE.3` Menu, rotta, manuale e i18n; widget e comandi vocali: nessuno per ora

#### `HR-CED` – Portale con cedolini e CU
- ✅ `HR-CED.1` Caricamento del PDF unico da parte dello studio o dell'azienda (`Hr_DocCaricamento`, job) — pagine lato studio con la scelta dell'azienda (§7.1)
- ✅ `HR-CED.2` Divisione per dipendente (codice fiscale o matricola, con l'AI come aiuto) e verifica degli scarti (`Hr_DocScarto`) — proposta per cognome e nome al posto dell'AI (HR-D3)
- ✅ `HR-CED.3` Pubblicazione nell'area del dipendente e notifica (`INF-SVC.4`) — "I miei cedolini", notifica in categoria `hr.cedolini` (`HR-SEG.2`)
- ✅ `HR-CED.4` Presa visione (`INF-SVC.6`) e report delle conferme

#### `HR-FOR` – Formazione e competenze
- ✅ `HR-FOR.1` Corsi, attestati e piani di formazione sulle competenze in Common (dopo `INF-COM.2` e `AT-COM`) — "I miei corsi" del dipendente ancora da fare (§7.1)
- ✅ `HR-FOR.2` Scadenze dei corsi nello scadenzario (`INF-SVC.2`)

#### `HR-ONB` – Onboarding e offboarding
- ✅ `HR-ONB.1` Checklist di assunzione e cessazione, basate sulle date in Common (D6, dopo `INF-COM.4`)
- ✅ `HR-ONB.2` Documenti da consegnare o firmare (`INF-SVC.6`)
- ✅ `HR-ONB.3` Creazione e disattivazione dell'account — l'utente si collega al dipendente esistente con `HR-ONB.4`
- ✅ `HR-ONB.4` Creazione dell'utente su un'anagrafica esistente senza passare dall'anagrafica nuova dell'inizializzatore — fatta, da mergiare (`INF-SVC.8`): `Hr_AccountCrea` passa `IdDip_Anagrafica` a `Account/UserCompanyPut`, tolta la correzione (scollegamento e cancellazione dell'anagrafica doppia). Provata su DB usa e getta (`nvx_test_svc78`, 42 controlli con `INF-SVC.7`): nessuna anagrafica nuova, anche dopo il login; rifiutate anagrafica già collegata, inesistente o di un'altra azienda, senza creare l'utente

#### Seguiti delle aree (da fare)
- ⏳ `HR-SEG.1` Pagina "I miei corsi" del dipendente (corsi e attestati)
- ✅ `HR-SEG.2` Categoria di notifica propria del modulo — fatta, da mergiare (`INF-SVC.7`): `hr.cedolini` per "nuovo cedolino/CU" (email predefinita); formazione e checklist restano nello scadenzario (`scadenze`), i documenti in `presevisione`. Provata: visibile nelle preferenze solo con HR attivo (sparisce disattivandolo, non salvabile da un'azienda senza HR), titolo it/en, preferenza rispettata (App ed Email disattivate: notifica conservata, nessuna email né avviso a comparsa; predefinita: email e avviso)
- ⏳ `HR-SEG.3` Scadenza delle prese visione non confermate entro n giorni

#### Decisioni
- 🔍 `HR-D1`…`HR-D7` (§8) — scelte provvisorie per HR-D2…HR-D6 nelle aree (§8)
