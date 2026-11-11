using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.Crm;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline;

namespace nvxapp.server.service.ClientServer_Service.Crm._utility
{
    /*
     Ciclo di vita delle offerte (preventivi) e scadenzario (ANALISI_CRM_PREVENTIVI.md §3, CRM-PREV.1):
        - l'offerta Inviata con una validita' ha la sua scadenza (fonte "crm.preventivo", riferimento = Id dell'offerta)
        - all'esito (Accettata, Rifiutata, Scaduta) la scadenza si chiude; tornando in bozza o cancellando l'offerta si toglie
        - un'offerta Inviata con la validita' passata diventa Scaduta: lo fanno il servizio in background del CRM
          (CrmReminderService, una volta all'ora) e l'apertura di elenco e scheda delle offerte (azienda corrente)
    */
    public static class CrmOfferteCiclo
    {
        public const string FonteScadenza = "crm.preventivo";
        // preavviso fisso (scelta provvisoria P3)
        public const int GiorniPreavviso = 7;

        public static bool Esito(Crm_StatoOfferta stato) => stato is Crm_StatoOfferta.Accettata or Crm_StatoOfferta.Rifiutata or Crm_StatoOfferta.Scaduta;

        /// <summary>Allinea la scadenza dello scadenzario allo stato dell'offerta.</summary>
        public static async Task ScadenzaAsync(IDeadlineRegistry registry, int idCompany, Crm_Offerta o, string numero, string cliente, string? responsabile)
        {
            if (o.Stato == Crm_StatoOfferta.Inviata && o.ValidaFinoAl is DateTime validita)
                await registry.UpsertAsync(new DeadlineRegistration
                {
                    IdCompany = idCompany,
                    Application = ApplicationType.Crm,
                    Fonte = FonteScadenza,
                    IdRiferimento = o.Id,
                    Descrizione = CrmMessages.Get("OfferDeadlineDescription", numero, cliente),
                    Note = o.Oggetto,
                    DataScadenza = validita,
                    GiorniPreavviso = GiorniPreavviso,
                    IdAspNetUsersResponsabile = responsabile,
                    Url = "/crmofferedit",
                    UrlState = new { id = o.Id },
                });
            else if (Esito(o.Stato))
                await registry.CloseAsync(idCompany, ApplicationType.Crm, FonteScadenza, o.Id);
            else
                await registry.RemoveAsync(idCompany, ApplicationType.Crm, FonteScadenza, o.Id);
        }

        /// <summary>Offerte inviate con la validita' passata: diventano Scadute e la loro scadenza si chiude.</summary>
        public static async Task<int> ScadiAsync(ICrm_OffertaRepository offerte, IDeadlineRegistry registry, int idCompany, int idAz_Anagrafica)
        {
            var oggi = DateTime.Today;
            var ids = await offerte.FindAll(o => o.IdAz_Anagrafica == idAz_Anagrafica && o.Stato == Crm_StatoOfferta.Inviata
                                                 && o.ValidaFinoAl != null && o.ValidaFinoAl < oggi)
                                   .Select(o => o.Id).ToListAsync();
            if (ids.Count == 0)
                return 0;
            var ora = DateTime.Now;
            await offerte.FindAll(o => ids.Contains(o.Id) && o.Stato == Crm_StatoOfferta.Inviata)
                         .ExecuteUpdateAsync(u => u.SetProperty(x => x.Stato, Crm_StatoOfferta.Scaduta)
                                                   .SetProperty(x => x.DataEsito, ora)
                                                   .SetProperty(x => x.ModifiedDate, ora));
            foreach (var id in ids)
                await registry.CloseAsync(idCompany, ApplicationType.Crm, FonteScadenza, id);
            return ids.Count;
        }
    }
}
