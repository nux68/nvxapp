# Analisi – Applicativo Risorse Umane (HumanResources)

Branch: **HumanResources_Dev** (riceve i merge da Infrastructure, si mergia in HumanResources).
Documento generale: `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` (§4.1, aree `HR-*` in §10.2), punto `ANA.8`.

Stato (11/11/2026): **analisi scritta, scheletro del modulo fatto** (`HR-BASE`, ramo di lavoro `HumanResources_Dev_hr-base`). Le aree funzionali (`HR-CED`, `HR-FOR`, `HR-ONB`) sono da fare; le decisioni aperte sono in §8.

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

## 8. Decisioni aperte

| # | Decisione | Note |
|---|---|---|
| HR-D1 | Livelli Free/Standard/Pro e prezzi del modulo (D8, `ANA.10`) | Oggi listini con la struttura del CRM e **prezzi segnaposto**, nessun limite |
| HR-D2 | Chi carica i cedolini: solo lo studio, solo l'azienda o entrambi | Proposta: entrambi, con l'origine registrata nel caricamento |
| HR-D3 | Criterio di abbinamento delle pagine (codice fiscale, matricola, AI) e cosa fare degli scarti | Proposta: codice fiscale, poi matricola, poi proposta AI da confermare a mano |
| HR-D4 | Conservazione dei documenti del dipendente cessato o dell'utente rimosso | Obblighi di legge: i documenti restano all'azienda; l'utente perde solo l'accesso |
| HR-D5 | Catalogo dei corsi: in HR, in Compliance o in Common | Se lo usano entrambi i moduli va in Common (regola del §2.2 del documento generale) |
| HR-D6 | Chat AI sui dati del modulo | Proposta: esclusi cedolini e CU; corsi e checklist ammessi per i ruoli che li vedono |
| HR-D7 | Cartella i18n e `i18n-folders.ts` | Il ramo aggiunge `i18n-human-resources.ts` e una riga in `i18n-folders.ts` (file di Infrastructure): nel merge di AllFunction con gli altri moduli nuovi c'è un conflitto banale (tenere tutte le righe). In alternativa Infrastructure aggiunge gli stub `i18n-<app>.ts` a `null` come fece per il CRM |

## 9. Checklist

Legenda: ✅ svolto · ⏳ da fare · 🔄 in corso · 🔍 da decidere

#### `HR-BASE` – Avvio del modulo
- ✅ `HR-BASE.1` `HumanResourcesDbContext` registrato, migration `InitHumanResources` con `Hr_Cfg`, repository, inizializzatore e `HrDataDeleter`
- ✅ `HR-BASE.2` Cartelle del modulo, servizio/controller della configurazione, pagina `/hrsettings`, ruoli (§3); area del dipendente prevista, pagine con `HR-CED`
- ✅ `HR-BASE.3` Menu, rotta, manuale e i18n; widget e comandi vocali: nessuno per ora

#### `HR-CED` – Portale con cedolini e CU
- ⏳ `HR-CED.1` Caricamento del PDF unico da parte dello studio o dell'azienda (`Hr_DocCaricamento`, job)
- ⏳ `HR-CED.2` Divisione per dipendente (codice fiscale o matricola, con l'AI come aiuto) e verifica degli scarti (`Hr_DocScarto`)
- ⏳ `HR-CED.3` Pubblicazione nell'area del dipendente e notifica (`INF-SVC.4`)
- ⏳ `HR-CED.4` Presa visione (`INF-SVC.6`) e report delle conferme

#### `HR-FOR` – Formazione e competenze
- ⏳ `HR-FOR.1` Corsi, attestati e piani di formazione sulle competenze in Common (dopo `INF-COM.2` e `AT-COM`)
- ⏳ `HR-FOR.2` Scadenze dei corsi nello scadenzario (`INF-SVC.2`)

#### `HR-ONB` – Onboarding e offboarding
- ⏳ `HR-ONB.1` Checklist di assunzione e cessazione, basate sulle date in Common (D6, dopo `INF-COM.4`)
- ⏳ `HR-ONB.2` Documenti da consegnare o firmare (`INF-SVC.6`)
- ⏳ `HR-ONB.3` Creazione e disattivazione dell'account

#### Decisioni
- 🔍 `HR-D1`…`HR-D7` (§8)
