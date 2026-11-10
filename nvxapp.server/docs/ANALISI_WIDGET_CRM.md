# Widget della dashboard — CRM

Dettaglio del ramo **CRM**. Il funzionamento generale (catalogo per ramo, configurazione per utente, tabella
`UserDashboard`, stili comuni) è in `ANALISI_WIDGET.md` (Infrastructure).

## 1. Stato

Il CRM **non espone ancora widget**. Il punto di estensione è pronto:

- il catalogo del ramo è `Utility/infrastructure/widget-crm.service.ts` (`WidgetCrmService.Widgets`), oggi vuoto
  come in Infrastructure;
- un widget dichiarato lì compare da solo nella configurazione della dashboard (gruppo "CRM") e nella dashboard,
  solo per le aziende con il CRM attivo (applicativo attivo e licenza) e per i ruoli indicati;
- nel ramo AllFunction arriva con la catena dei merge, insieme ai widget degli altri applicativi.

Nella pagina di configurazione il gruppo "CRM" non compare finché il ramo non dichiara almeno un widget.

## 2. Come aggiungere un widget del CRM

1. Componente **standalone** in `shared/components/Crm/widgets/<nome>-widget/`: carica da solo i dati, gestisce
   caricamento ed errori, include gli stili comuni:
   ```scss
   @use '../../../infrastructure/widgets/widget-common' as wc;
   @include wc.widget-common;
   ```
2. Dichiarazione in `widget-crm.service.ts`, con chiave `crm.<nome>` (stabile: si salva nelle dashboard degli utenti),
   testi, icona, dimensione (`small`, `medium`, `large`), ruoli, predefinito e `zorder` (fascia 300000, dopo le
   Presenze e prima del Documentale, come il menu).
3. Testi in `assets/i18n/crm/{it,en}.json`, sezione `crm.widgets`.
4. Dati: un endpoint del servizio CRM (es. `Crm_WidgetService` / `Crm_WidgetController`) con gli stessi controlli dei
   permessi delle pagine del CRM.
5. Questo documento aggiornato con la tabella dei widget.

## 3. Proposte

| Chiave proposta | Titolo | Ruoli | Contenuto |
|---|---|---|---|
| `crm.clients` | Clienti | CompanyPowerAdmin, CompanyAdmin | numero di clienti e, con licenza a limite, clienti usati / massimo del piano (ANALISI_LICENZE.md §5.1) |
| `crm.activities` | Attività di oggi | tutti | attività e scadenze del giorno assegnate all'utente |
| `crm.opportunities` | Opportunità | CompanyPowerAdmin, CompanyAdmin | opportunità aperte per fase |

Le proposte vanno confermate prima dell'implementazione.
