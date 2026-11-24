# ANALISI_INFRASTRUCTURE_SHOWCASE

Composizione delle pagine pubbliche **Home** (`/home`), **Funzionalità** (`/features`) e **Prezzi** (`/pricing`)
in funzione degli applicativi disponibili. Stato rilevato il 2026-11-24 su `AllFunction` (5be916a5) e confrontato con
tutti i branch di rilascio.

---

## 1. Architettura

Le pagine dello showcase sono **template di Infrastructure** (`pages/infrastructure/showcase-*-page`): non contengono
testi propri, leggono tutto da `ShowcaseService.Contenuti` (modello `ShowcaseContenuti` in `Utility/infrastructure/showcase.model.ts`).

```
ShowcaseService  (Utility/infrastructure/showcase.service.ts)
 ├─ ShowcaseInfrastructureService      base: menu, rotte, testi generici (chiavi showcase.*)
 └─ rami, in ordine di precedenza crescente (l'ultimo vince):
     1. ShowcaseAttendanceTrackingService   attendance.showcase.*
     2. ShowcaseCrmService                  crm.showcase.*
     3. ShowcaseDocumentManagementService   documents.showcase.*
     4. ShowcaseAllFunctionService          allfunction.showcase.*
```

Regole di fusione (`ShowcaseService`):

| Elemento | Regola |
|---|---|
| Voci di menu (`Pages`) | voce con lo stesso `url` (o header con lo stesso `group`) sostituita da quella del ramo, le altre si aggiungono |
| Rotte (`getRoutes`) | rotta con lo stesso `path` sostituita; a tutte si aggiunge `showcaseGuard` (dopo il login → `/dashboard`) |
| Contenuti | `Object.assign` **per sezione intera** (`home`, `features`, `pricing`, `about`, `register`, …): una sezione ridefinita sostituisce completamente quella di Infrastructure, nessuna fusione campo per campo |

### 1.1 Meccanismo stub / riempimento per branch

In `Infrastructure` i servizi di ramo esistono come **stub vuoti** (~20 righe, `Contenuti` = `{}`); ogni branch
applicativo riempie **solo il proprio** stub. Così `ShowcaseService` non cambia mai fuori da Infrastructure e i merge
verso AllFunction non vanno in conflitto.

| Branch | presenze | CRM | documentale | AllFunction | Risultato a video |
|---|---|---|---|---|---|
| Infrastructure | stub | stub | stub | stub | testi generici Infrastructure |
| AttendanceTracking | **pieno** | stub | stub | stub | showcase presenze |
| CRM | stub | **pieno** | stub | stub | showcase CRM |
| DocumentManagement | stub | stub | **pieno** | stub | showcase documentale |
| HumanResources | stub | stub | stub | stub | **testi generici Infrastructure** |
| Compliance | stub | stub | stub | stub | **testi generici Infrastructure** |
| Assets | stub | stub | stub | stub | **testi generici Infrastructure** |
| AllFunction | pieno | pieno | pieno | **pieno** | showcase completo (vince AllFunction) |

> Importante: lo showcase **non dipende dagli applicativi attivi a runtime** (non c'è ancora un utente/licenza).
> Dipende solo dal **branch compilato**: è una scelta “di build”, non di configurazione.

---

## 2. Home (`/home`)

Componente: `ShowcaseHomePageComponent`. Struttura fissa, dall'alto:

1. **Hero** – `home.etichetta / titolo / sottotitolo`, CTA primaria → `/register`, CTA secondaria → `/features`,
   badge AI (loghi `ASSISTENTI_AI`) che scorre alla sezione AI.
2. **Punti di forza** – griglia di card da `home.punti[]` (variabile per ramo).
3. **Integrazione AI** – *uguale in tutti i rami*: non è in `Contenuti.home` ma hard-coded nel componente
   (chiavi `showcase.ai.*`, 4 punti, esempi, richiamo ai piani Pro → `/pricing`).
4. **Come funziona** – `home.passiTitolo` + `home.passi[]` (3 passi numerati).
5. **Banda di chiusura** – `home.chiusuraTitolo / chiusuraTesto` + CTA primaria.

Cosa cambia per applicativo:

| Branch | `home.punti` | `passi` |
|---|---|---|
| Infrastructure (e HR/Compliance/Assets) | 4 generici: disponibile, sicuro, team, assistente | registrati, configura, lavora |
| AttendanceTracking | 4: timbratura, richieste, cartellino, note spese | registra, organizza, chiudi |
| CRM / Documentale | 4 propri del ramo | propri |
| AllFunction | **8**: uno per ognuno dei 6 applicativi + funzioni comuni + “insieme” | registra, **attiva** (moduli), lavora |

## 3. Funzionalità (`/features`)

Componente: `ShowcaseFeaturesPageComponent`. Due modalità decise dai dati:

- **`features.gruppi` presente** (solo AllFunction): indice di navigazione in cima (un bottone per gruppo, scroll
  all'ancora `nvx-sc-gruppo-i`) e un blocco per gruppo con intestazione (icona, titolo, sottotitolo) e griglia di card.
  Gruppi attuali: Presenze (8), CRM (7), Documentale (7), Risorse Umane (6), Compliance (7), Risorse aziendali (6),
  Funzioni comuni (7).
- **altrimenti `features.sezioni`**: una sola griglia piatta (rami singoli e Infrastructure).

Ogni card con `dettaglio` (`<chiave>.dettaglio = { intro, punti[] }` letto da `voceShowcase`) diventa cliccabile e
apre un `ion-modal` di approfondimento; senza dettaglio resta una card statica.

| Branch | Modalità | Card | Dettaglio |
|---|---|---|---|
| Infrastructure (e HR/Compliance/Assets) | sezioni | 6 generiche (aziende, ruoli, struttura, impersonate, notifiche, dispositivi) | no |
| AttendanceTracking | sezioni | 13 | **sì** (usa `v()`) |
| CRM | sezioni | 13 | **no** (oggetti scritti a mano, niente `voceShowcase`) |
| DocumentManagement | sezioni | 12 | **no** |
| AllFunction | gruppi | 48 | sì (tutte via `v()`) |

In fondo: banda di chiusura che riusa i testi di `home` + CTA `/register` e `/pricing`.

## 4. Prezzi (`/pricing`)

Componente: `ShowcasePricingPageComponent`. È l'unica pagina con dati **dal server**:

1. All'avvio chiama `License/LicensePublicPriceList` (anonima) passando `pricing.ambiti` del ramo.
   Il server restituisce i **listini globali del SuperUser** (`IdDealer == null`, attivi, con piani attivi) per quegli
   ambiti; se `ambiti` è vuoto usa `AmbitiOfferta` (tutti e 7).
2. Selettori, mostrati solo se servono:
   - **funzione** (`ambiti.length > 1`, cioè solo AllFunction): Presenze, CRM, Documentale, HR, Compliance, Assets, Cumulativo
     — filtrati a quelli che hanno davvero un listino;
   - **Aziende / Studi** (solo se il listino esiste per entrambi i `TipoSoggetto`);
   - **Mensile / Annuale** con risparmio massimo.
3. Schede piano (Standard = consigliato): prezzo al mese, “comprende <funzioni>”, **limiti**, riga AI (solo `Pro`), CTA →
   `/register?tipo=azienda|studio`.
4. `pricing.inclusi`, `pricing.nota`, FAQ `pricing.domande` (accordion) dal ramo.

| Branch | `pricing.ambiti` | Effetto |
|---|---|---|
| Infrastructure (e HR/Compliance/Assets) | `[Cumulativo]` | mostra il listino Cumulativo |
| AttendanceTracking | `[AttendanceTracking]` | listino Presenze |
| CRM | `[Crm]` | listino CRM |
| DocumentManagement | `[DocumentManagement]` | listino Documentale |
| AllFunction | tutti i 6 + Cumulativo | selettore funzione |

Limiti per scheda (`limiti()`): Presenze → utenti, CRM → clienti, Documentale → spazio, Cumulativo → i tre insieme.
HR, Compliance e Assets **non hanno limiti** mostrati (non esistono `maxXxx` per loro in `LicensePlanModel`).

---

## 5. Criticità rilevate

1. **HR, Compliance, Assets senza showcase.** Nessuno stub in Infrastructure (lo conferma `INF-APP.4` in
   `ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md`: “Showcase: nessuno stub, lo ridefiniscono i rami e AllFunction”), nessuna chiave
   `*.showcase` negli i18n `humanresources/`, `compliance/`, `assets/`. Sui rispettivi branch si vede la vetrina generica.
   Per aggiungerla dal ramo bisognerebbe modificare `ShowcaseService` sul branch applicativo, con conflitti garantiti in
   AllFunction: il pattern richiede gli stub **in Infrastructure**.
2. **Vetrina generica che parla di presenze.** `ShowcaseInfrastructureService.brand.icon = 'calendar-clear-outline'`
   (retaggio presenze) e `pricing.ambiti = [Cumulativo]`: su HR/Compliance/Assets la pagina prezzi mostra il listino
   Cumulativo con “comprende Presenze, CRM, Documentale, …”, cioè funzioni che quel branch non contiene.
3. **“Comprende” del Cumulativo hard-coded.** `funzioniIncluse` elenca sempre i 6 applicativi, indipendentemente dal
   branch: corretto solo in AllFunction.
4. **Limiti incompleti.** `limiti()` gestisce solo Presenze/CRM/Documentale; le schede di HR, Compliance, Assets mostrano
   solo “comprende X” + riga AI. Da decidere se questi moduli hanno un limite (es. dipendenti, veicoli/beni) o nessuno.
5. **Dettaglio non uniforme.** CRM e Documentale non usano `voceShowcase`: le loro card non aprono il dialog, a
   differenza di Presenze e AllFunction (che però hanno già i testi di dettaglio per CRM/Documentale nelle chiavi `allfunction.*`).
6. **Ordine di precedenza fisso nel codice.** Con 6 rami + AllFunction l'elenco `rami` va aggiornato a mano in
   Infrastructure; la precedenza tra rami singoli è irrilevante (un solo ramo pieno per branch) ma AllFunction deve restare ultimo.
7. **Duplicazione testi.** I testi di ogni applicativo esistono due volte (`<ramo>.showcase.*` e `allfunction.showcase.features.gruppi.<ramo>.*`):
   ogni modifica commerciale va fatta in due posti su due branch diversi.
8. **Sezione AI fissa.** È uguale ovunque e nel testo promette l'integrazione nei piani Pro, ma oggi è “solo presentazione”
   (`conAi()` non verifica nulla): coerente su tutti i branch, da tenere allineata quando l'integrazione arriverà.

## 6. Proposta

- **Infrastructure_Dev**: stub `ShowcaseHumanResourcesService`, `ShowcaseComplianceService`, `ShowcaseAssetsService`
  (stessa forma degli altri: `Pages=[]`, `getRoutes()=[]`, `Contenuti={}`), aggiunti a `ShowcaseService.rami`
  **prima** di AllFunction. Icona brand neutra (es. `apps-outline`). `funzioniIncluse` del Cumulativo spiegato come
  “tutti gli applicativi” senza elenco, oppure elenco preso da `pricing.ambiti` quando contiene più funzioni.
- **HumanResources_Dev / Compliance_Dev / Assets_Dev**: riempire il proprio stub (home, features con `v()` e dettaglio,
  pricing con `ambiti: [<proprio>]`, about, register) e le chiavi `<ramo>.showcase.*` in `it.json`/`en.json`.
  Si possono riusare i testi già scritti in `allfunction.showcase.*.risorseUmane/compliance/risorseAziendali`.
- **CRM_Dev / DocumentManagement_Dev**: passare le `features.sezioni` a `v()` e aggiungere i `dettaglio`.
- **Limiti** HR/Compliance/Assets: decisione commerciale (vedi §5.4) prima di toccare `LicensePlanModel` / `Lic_Piano`.
- Catena di merge standard verso AllFunction (nessun conflitto atteso grazie agli stub).

---

## Checklist

- [x] Analisi della composizione di Home, Funzionalità, Prezzi e del meccanismo stub/ramo (questo documento)
- [x] Decidere se HR, Compliance, Assets hanno limiti di piano da mostrare in `/pricing` (§5.4) — deciso 2026-11-24: **nessun limite** (niente nuovi campi licenza/migrazioni); la scheda mostra “comprende X” e la riga AI
- [x] Infrastructure_Dev: stub showcase HR / Compliance / Assets + registrazione in `ShowcaseService.rami` (prima di AllFunction) e in `RouteService`
- [x] Infrastructure_Dev: icona brand neutra nella vetrina generica (`apps-outline`)
- [x] Infrastructure_Dev: testo “comprende” del Cumulativo non legato ai 6 applicativi fissi (funzioni di `pricing.ambiti`, altrimenti `showcase.pricingPage.allFunctions`)
- [x] Infrastructure_Dev: `limiti()` per HR/Compliance/Assets — non necessario (nessun limite, vedi sopra)
- [x] HumanResources_Dev: `ShowcaseHumanResourcesService` pieno + chiavi `humanresources.showcase.*` (it/en)
- [ ] Compliance_Dev: `ShowcaseComplianceService` pieno + chiavi `compliance.showcase.*` (it/en)
- [ ] Assets_Dev: `ShowcaseAssetsService` pieno + chiavi `assets.showcase.*` (it/en)
- [x] CRM_Dev: card delle funzionalità con `voceShowcase` e testi `dettaglio`
- [x] DocumentManagement_Dev: card delle funzionalità con `voceShowcase` e testi `dettaglio`
- [ ] Catena di merge Infrastructure → rami → AllFunction_Dev → AllFunction e verifica visiva delle tre pagine su ogni branch
