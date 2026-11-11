# Analisi – Applicativo Compliance

Branch: **Compliance_Dev** (riceve i merge da Infrastructure, si mergia in Compliance).
Documento generale: `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md` (§4.2, aree `CMP-*` in §10.2), punto `ANA.7`.

Stato (11/11/2026): **analisi scritta, scheletro del modulo fatto** (`CMP-BASE`, ramo di lavoro `Compliance_Dev_cmp-base`). Le aree funzionali (`CMP-81`, `CMP-WB`, `CMP-GDPR`) sono da fare. **Il whistleblowing non si rilascia senza la verifica legale (`ANA.5`) e l'analisi della sicurezza (`ANA.6`)**. Decisioni aperte in §8.

---

## 1. Obiettivo

Gli **adempimenti normativi** dell'azienda, in un modulo che lo studio può proporre a tutte le sue aziende:
- **Sicurezza sul lavoro, D.Lgs 81/08** (`CMP-81`): corsi obbligatori per mansione e loro scadenze, visite mediche e idoneità, consegna dei DPI con verbale, nomine (RSPP, RLS, preposti, addetti alle emergenze e al primo soccorso), documenti DVR.
- **Whistleblowing, D.Lgs 24/2023** (`CMP-WB`), compreso nel modulo senza licenza separata (D7): canale di segnalazione anche anonimo, codice per seguire la propria segnalazione, scadenze di legge (avviso di ricevimento entro 7 giorni, riscontro entro 3 mesi), accesso riservato al gestore.
- **Registro GDPR** (`CMP-GDPR`): registro dei trattamenti, nomine e responsabili esterni, registro delle violazioni; lo studio può gestirlo per conto delle aziende.

Il modulo funziona da solo; HR, Presenze, Documentale e Risorse aziendali sono integrazioni facoltative (§6).

## 2. Applicativo e schema

| Voce | Valore |
|---|---|
| `ApplicationType` | `Compliance = 6` |
| Schema | `tenant_<IdAzienda>_6` (in modalità singola `public`) |
| Prefisso tabelle | `Cmp_` |
| Contesto | `ComplianceDbContext` (migration in `Migrations/Compliance`, storico `__EFMigrationsHistory_Compliance`) |
| Licenza | `AmbitoLicenza.Compliance = 6`, nessun limite (`INF-APP.3`); listini con prezzi segnaposto (§8, D8) |
| Codice del manuale | `compliance` (`Manuals/compliance.<lingua>.md`) |
| Cartella i18n del client | `assets/i18n/compliance/`, chiavi sotto `cmp.` |
| Cartelle | server `Controllers/Compliance`, `ClientServer-Service/Compliance`, `Entities/Tenant/Compliance`, `Repositories/Tenant/Compliance`; client `ClientServer-Service/Compliance`, `pages/Compliance` |

## 3. Ruoli e visibilità

| Ruolo | Cosa fa |
|---|---|
| **CompanyPowerAdmin** | Configurazione del modulo, nomina del **gestore delle segnalazioni**; sicurezza e GDPR completi |
| **CompanyAdmin** (RSPP, ufficio del personale, DPO interno) | Sicurezza sul lavoro e registro GDPR. **Non** vede le segnalazioni se non è gestore |
| **Gestore delle segnalazioni** (utente designato, anche esterno all'organigramma) | Unico che vede e tratta le segnalazioni del whistleblowing. Ruolo applicativo del modulo, non un ruolo di Identity: elenco degli utenti in configurazione |
| **User** (dipendente) | I propri corsi, visite (solo esito di idoneità, non i dati sanitari), DPI ricevuti con presa visione; invio di una segnalazione e consultazione con il codice |
| **Segnalante esterno o anonimo** | Pagina pubblica di segnalazione (senza login) e consultazione con il codice; da verificare con `ANA.5` |
| **FinancialAdvisorPowerAdmin/Admin** (studio) | Registro GDPR e scadenzario della sicurezza per conto delle aziende dello studio (lato studio, `getStudioRoutes`). Mai le segnalazioni |

Regole:
- permessi controllati dal server in ogni servizio (`CmpUtility.ContestoAsync`), come nel CRM;
- **dati sanitari** delle visite (referti, prescrizioni) visibili solo al medico competente/RSPP designato: il modulo conserva per gli altri solo l'esito di idoneità e la scadenza (`ANA.6`);
- **segnalazioni**: cifrate a livello di campo, mai nei log, mai nella chat AI, mai negli export generici, accessi registrati (`ANA.6`).

## 4. Modello dati proposto (schema `tenant_<id>_6`, prefisso `Cmp_`)

Tutte le tabelle hanno `IdAz_Anagrafica` (FK verso `Az_Anagrafica` di Common) e i campi di `BaseEntity`. Dipendenti, sedi e competenze sono in Common (`Dip_Anagrafica`, `Az_Sedi`/`Az_SediReparto` dopo `INF-COM.1`, `Par_Competenza`/`Dip_Competenza` dopo `INF-COM.2`), letti in sola lettura.

### 4.1 Configurazione (fatta in `CMP-BASE`)
- `Cmp_Cfg` (una riga per azienda, creata all'attivazione): `GiorniPreavviso`, `NotificheAttive`, `Note`. Le aree aggiungono i propri parametri (es. testo informativo del canale di segnalazione, giorni di legge configurabili solo in aumento).

### 4.2 Sicurezza sul lavoro (`CMP-81`)
- `Cmp_Mansione`: mansioni con rischio (basso/medio/alto) e corsi obbligatori collegati.
- `Cmp_MansioneCorso`: corso richiesto per la mansione (tipo di corso, validità in mesi: formazione generale, specifica, aggiornamento quinquennale, antincendio, primo soccorso…).
- `Cmp_DipendenteMansione`: mansione del dipendente (`Dip_Anagrafica`) con date.
- `Cmp_Formazione`: corso svolto dal dipendente (data, ore, ente, scadenza, attestato in `IFileStorage`). Se HR è attivo può riferire il corso di HR (decisione `CMP-D4`).
- `Cmp_Visita`: visita medica (tipo: preventiva/periodica/su richiesta, data, esito di idoneità, limitazioni in sintesi, scadenza). Il referto, se caricato, è un file cifrato accessibile solo al ruolo sanitario.
- `Cmp_Dpi`, `Cmp_DpiConsegna`: catalogo dei DPI e consegne al dipendente (quantità, taglia, data, verbale con presa visione).
- `Cmp_Nomina`: nomine (tipo: RSPP, ASPP, RLS, preposto, addetto antincendio, primo soccorso, medico competente; persona interna o esterna, sede, dal/al, documento).
- `Cmp_Documento`: DVR, piani di emergenza e altri documenti con versione, data di revisione e scadenza.

### 4.3 Whistleblowing (`CMP-WB`) – vincolato a `ANA.5` e `ANA.6`
- `Cmp_Segnalazione`: codice pubblico (casuale, non deducibile), canale, data di ricezione, stato (ricevuta / avviso inviato / in istruttoria / riscontro dato / archiviata), scadenze di legge (7 giorni, 3 mesi), oggetto e testo **cifrati**, identità del segnalante **cifrata e facoltativa**.
- `Cmp_SegnalazioneMessaggio`: dialogo tra gestore e segnalante (cifrato), visibile al segnalante tramite il codice.
- `Cmp_SegnalazioneAllegato`: allegati cifrati in `IFileStorage`.
- `Cmp_SegnalazioneAccesso`: registro degli accessi del gestore (chi, quando, cosa).
- `Cmp_Gestore`: utenti designati come gestori.

### 4.4 Registro GDPR (`CMP-GDPR`)
- `Cmp_Trattamento`: registro dei trattamenti (art. 30: finalità, base giuridica, categorie di interessati e di dati, destinatari, trasferimenti, termini di cancellazione, misure di sicurezza).
- `Cmp_Responsabile`: responsabili esterni e nomine (art. 28), con contratto e scadenza; collegamento facoltativo ai fornitori/clienti comuni (`Az_Cliente` in Common).
- `Cmp_Violazione`: registro delle violazioni (data di scoperta, descrizione, dati coinvolti, valutazione del rischio, notifica al Garante entro 72 ore, comunicazione agli interessati).

## 5. Uso dei servizi comuni (Infrastructure)

| Servizio | Dove | Uso in Compliance |
|---|---|---|
| Scadenzario `IDeadlineRegistry` | `ClientServer-Service/infrastructure/Deadline` | Corsi (`cmp.formazione`), visite (`cmp.visita`), nomine e documenti (`cmp.nomina`, `cmp.documento`), contratti dei responsabili (`cmp.responsabile`), 72 ore di una violazione (`cmp.violazione`). **Non** per le segnalazioni: le loro scadenze restano dentro il modulo e le vede solo il gestore (lo scadenzario comune lo vedono gli amministratori) |
| Notifiche `INotificationSender` | `ClientServer-Service/infrastructure/Notification` | Scadenze, nuova segnalazione al gestore (testo generico, senza contenuto), DPI da confermare. Rispettare `Cmp_Cfg.NotificheAttive` |
| Presa visione `IAcknowledgementService` | `ClientServer-Service/infrastructure/Acknowledgement` | Verbale di consegna dei DPI (`cmp.dpi`), lettura del DVR o delle procedure (`cmp.documento`), nomine (`cmp.nomina`). Con il Documentale (`DM-FIRMA`) diventa firma FES |
| Dati Common | `Dip_Anagrafica`, `Az_Sedi`/`Az_SediReparto`, `Par_Competenza`/`Dip_Competenza`, `Az_Cliente` | Sola lettura con `Cmp_<Entità>ReadRepository`, scritture dai repository di `Repositories/Tenant/Common` |
| Cancellazione `IApplicationDataDeleter` | `CmpDataDeleter` | Oggi solo `Cmp_Cfg` in modalità singola. Le segnalazioni hanno termini di conservazione propri (5 anni dalla chiusura): da definire con `ANA.5` |
| Chat AI | `IRouteProvider`, handler | Solo sicurezza e GDPR; **segnalazioni escluse** in modo esplicito |

## 6. Integrazioni facoltative

- **Risorse Umane**: corsi obbligatori dal catalogo di HR (`HR-FOR`), assunzione/cessazione per assegnare e chiudere mansioni.
- **Presenze**: sedi e reparti (in Common dopo `AT-COM`).
- **Documentale**: firma dei verbali (`DM-FIRMA.2`) e archiviazione del DVR.
- **Risorse aziendali**: DPI come beni assegnati? Proposta: no, restano in Compliance (sono materiale di consumo con obbligo di verbale).

## 7. Fatto in `CMP-BASE` (11/11/2026)

- Server: `ComplianceDbContext` registrato in `Installers4Compliance`, design-time factory in un file proprio (`ComplianceDbContextDesignTimeFactory.cs`), migration `InitCompliance` con `Cmp_Cfg`, repository `Cmp_CfgRepository`, `Cmp_Az_AnagraficaReadRepository`, `CmpUtility`, `CmpCompanyInitializer` (priorità 60), `CmpDataDeleter`, `Cmp_ConfigService` + `Cmp_ConfigController` (`api/Cmp_Config/Cmp_CfgGet` amministratori, `Cmp_CfgPut` CompanyPowerAdmin), testi `ComplianceMessages` (it/en), manuale `compliance` (it/en), listini `CmpLicenseCatalogSeed`.
- Client: pagina `/cmpsettings` (`CmpSettingsPageComponent`), voce di menu "Compliance › Impostazioni" (CompanyPowerAdmin, gruppo 60), manuale nella sezione Assistenza, rotta, cartella i18n `compliance`.
- Widget e comandi vocali: nessuno per ora.
- Prova su DB usa e getta: azienda, attivazione come SuperUser (non bloccata dalle licenze), schema `tenant_<id>_6` con `Cmp_Cfg`, Get/Put del CompanyPowerAdmin, Put rifiutata al CompanyAdmin (12 controlli, tutti ok).

## 8. Decisioni aperte

| # | Decisione | Note |
|---|---|---|
| CMP-D1 | Livelli Free/Standard/Pro e prezzi (D8, `ANA.10`) | Oggi listini con la struttura del CRM e **prezzi segnaposto**, nessun limite |
| CMP-D2 | **Verifica legale del whistleblowing** (`ANA.5`) | Canale interno conforme al D.Lgs 24/2023, segnalazioni orali, anonimato, conservazione, informativa; prima del rilascio di `CMP-WB` |
| CMP-D3 | **Analisi della sicurezza** (`ANA.6`, `piano-security.md`) | Cifratura per campo e chiavi (per azienda?), registro degli accessi, esclusione da log/AI/export, dati sanitari delle visite |
| CMP-D4 | Catalogo dei corsi condiviso con HR | Se condiviso va in Common (vedi `HR-D5` in `ANALISI_HUMANRESOURCES_APPLICATIVO.md`) |
| CMP-D5 | Gestore delle segnalazioni esterno all'azienda (es. lo studio o un legale) | Serve un utente con accesso alla sola funzione: da decidere con `ANA.5`/`ANA.6` |
| CMP-D6 | Pagina pubblica di segnalazione senza login | Indirizzo per azienda, protezione da abusi, nessun dato di tracciamento |
| CMP-D7 | Cartella i18n e `i18n-folders.ts` | Come per HR: riga aggiunta in `i18n-folders.ts` (file di Infrastructure), conflitto banale nel merge di AllFunction, oppure stub `i18n-compliance.ts` in Infrastructure |

## 9. Checklist

Legenda: ✅ svolto · ⏳ da fare · 🔄 in corso · 🔍 da decidere

#### `CMP-BASE` – Avvio del modulo
- ✅ `CMP-BASE.1` `ComplianceDbContext` registrato, migration `InitCompliance` con `Cmp_Cfg`, repository, inizializzatore e `CmpDataDeleter`
- ✅ `CMP-BASE.2` Cartelle del modulo, servizio/controller della configurazione, pagina `/cmpsettings`, ruoli (§3)
- ✅ `CMP-BASE.3` Menu, rotta, manuale e i18n; widget e comandi vocali: nessuno per ora

#### `CMP-81` – Sicurezza sul lavoro (D.Lgs 81/08)
- ⏳ `CMP-81.1` Corsi obbligatori per mansione e scadenze (`Cmp_Mansione`, `Cmp_MansioneCorso`, `Cmp_Formazione`)
- ⏳ `CMP-81.2` Visite mediche e idoneità (dati sanitari protetti, `ANA.6`)
- ⏳ `CMP-81.3` Consegna dei DPI con verbale (`INF-SVC.6`)
- ⏳ `CMP-81.4` Nomine (RSPP, RLS, preposti, addetti alle emergenze) e documenti DVR
- ⏳ `CMP-81.5` Scadenze registrate nello scadenzario (`INF-SVC.2`)

#### `CMP-WB` – Whistleblowing (richiede `ANA.5` e `ANA.6`)
- ⏳ `CMP-WB.1` Canale di segnalazione con anonimato e codice per seguire la segnalazione
- ⏳ `CMP-WB.2` Gestore, accesso limitato, cifratura, esclusione dalla chat AI
- ⏳ `CMP-WB.3` Scadenze di legge (7 giorni, 3 mesi) e registro

#### `CMP-GDPR` – Registro GDPR
- ⏳ `CMP-GDPR.1` Registro dei trattamenti
- ⏳ `CMP-GDPR.2` Nomine e responsabili esterni
- ⏳ `CMP-GDPR.3` Registro delle violazioni
- ⏳ `CMP-GDPR.4` Gestione da parte dello studio per conto delle aziende

#### Decisioni
- 🔍 `CMP-D1`…`CMP-D7` (§8)
