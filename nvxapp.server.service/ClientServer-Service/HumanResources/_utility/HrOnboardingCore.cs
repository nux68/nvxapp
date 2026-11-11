using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.HumanResources;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.HumanResources;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline;

namespace nvxapp.server.service.ClientServer_Service.HumanResources._utility
{
    /*
     Checklist di assunzione e cessazione (HR-ONB.1, ANALISI_HUMANRESOURCES_APPLICATIVO.md §4.4) basate sulle date di
     Dip_Anagrafica (Common, D6): per ogni dipendente con la data di assunzione (o cessazione) da oggi meno FinestraGiorni
     in avanti, il modello predefinito del tipo genera la checklist se manca. Se la data cambia e la checklist aperta non
     ha ancora voci completate, si sposta sulla nuova data (voci e scadenze comprese).
     Si esegue all'apertura dell'elenco delle checklist e al salvataggio del dipendente da Risorse Umane: le date le
     possono cambiare anche le Presenze, quindi non c'e' un evento da ascoltare (scelta provvisoria).
     Ogni voce ha una scadenza nello scadenzario comune (fonte hr.checklist, riferimento la voce), chiusa al completamento.
    */
    public interface IHrOnboardingCore
    {
        Task<int> SincronizzaAsync(HrContesto ctx, int? idDip_Anagrafica = null);
        Task<Hr_Checklist> CreaAsync(HrContesto ctx, Dip_Anagrafica dip, Hr_ChecklistModello modello, DateTime dataRiferimento);
        Task ScadenzaVoceAsync(HrContesto ctx, Hr_Cfg cfg, Hr_Checklist checklist, Hr_ChecklistVoce voce, Dip_Anagrafica dip);
        Task CompletaVoceAsync(HrContesto ctx, int idVoce);
        Task AggiornaStatoAsync(int idChecklist);
    }

    public class HrOnboardingCore : IHrOnboardingCore
    {
        public const string FonteScadenza = "hr.checklist";
        // le date piu' vecchie di cosi' non generano checklist (dipendenti gia' assunti o usciti prima dell'uso del modulo)
        public const int FinestraGiorni = 30;

        private readonly IHr_ChecklistModelloRepository _Hr_ChecklistModello;
        private readonly IHr_ChecklistModelloVoceRepository _Hr_ChecklistModelloVoce;
        private readonly IHr_ChecklistRepository _Hr_Checklist;
        private readonly IHr_ChecklistVoceRepository _Hr_ChecklistVoce;
        private readonly IHr_Dip_AnagraficaReadRepository _Dip_Anagrafica;
        private readonly IHrUtility _utility;
        private readonly IDeadlineRegistry _deadlineRegistry;

        public HrOnboardingCore(IHr_ChecklistModelloRepository hr_ChecklistModello, IHr_ChecklistModelloVoceRepository hr_ChecklistModelloVoce,
                                IHr_ChecklistRepository hr_Checklist, IHr_ChecklistVoceRepository hr_ChecklistVoce,
                                IHr_Dip_AnagraficaReadRepository dip_Anagrafica, IHrUtility utility, IDeadlineRegistry deadlineRegistry)
        {
            _Hr_ChecklistModello = hr_ChecklistModello;
            _Hr_ChecklistModelloVoce = hr_ChecklistModelloVoce;
            _Hr_Checklist = hr_Checklist;
            _Hr_ChecklistVoce = hr_ChecklistVoce;
            _Dip_Anagrafica = dip_Anagrafica;
            _utility = utility;
            _deadlineRegistry = deadlineRegistry;
        }

        public async Task<int> SincronizzaAsync(HrContesto ctx, int? idDip_Anagrafica = null)
        {
            var modelli = await _Hr_ChecklistModello.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Predefinito && x.Attivo).ToListAsync();
            if (modelli.Count == 0)
                return 0;
            var dal = DateTime.Today.AddDays(-FinestraGiorni);
            int create = 0;
            foreach (var modello in modelli)
            {
                bool assunzione = modello.Tipo == Hr_TipoChecklist.Assunzione;
                var dipendenti = await _Dip_Anagrafica.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && (idDip_Anagrafica == null || x.Id == idDip_Anagrafica)
                                                                    && (assunzione ? x.DataAssunzione >= dal : x.DataCessazione >= dal))
                                                      .ToListAsync();
                foreach (var dip in dipendenti)
                {
                    var data = (assunzione ? dip.DataAssunzione : dip.DataCessazione)!.Value.Date;
                    var esistenti = await _Hr_Checklist.FindAll(x => x.IdDip_Anagrafica == dip.Id && x.Tipo == modello.Tipo && x.Stato != Hr_StatoChecklist.Annullata)
                                                       .AsTracking().ToListAsync();
                    if (esistenti.Any(x => x.DataRiferimento.Date == data))
                        continue;
                    // data cambiata: la checklist aperta e non iniziata si sposta sulla nuova data
                    var daSpostare = esistenti.FirstOrDefault(x => x.Stato == Hr_StatoChecklist.Aperta);
                    if (daSpostare != null && !await _Hr_ChecklistVoce.FindAll(x => x.IdHr_Checklist == daSpostare.Id && x.Completata).AnyAsync())
                    {
                        await SpostaAsync(ctx, daSpostare, data, dip);
                        continue;
                    }
                    if (daSpostare != null)
                        continue;
                    await CreaAsync(ctx, dip, modello, data);
                    create++;
                }
            }
            return create;
        }

        public async Task<Hr_Checklist> CreaAsync(HrContesto ctx, Dip_Anagrafica dip, Hr_ChecklistModello modello, DateTime dataRiferimento)
        {
            var cfg = await _utility.CfgAsync(ctx);
            var voci = await _Hr_ChecklistModelloVoce.FindAll(x => x.IdHr_ChecklistModello == modello.Id).OrderBy(x => x.Ordine).ThenBy(x => x.Id).ToListAsync();
            var c = new Hr_Checklist
            {
                IdAz_Anagrafica = ctx.IdAz_Anagrafica, IdDip_Anagrafica = dip.Id, IdHr_ChecklistModello = modello.Id, Tipo = modello.Tipo,
                Descrizione = modello.Descrizione, DataRiferimento = dataRiferimento.Date, Stato = Hr_StatoChecklist.Aperta
            };
            await _Hr_Checklist.CreateAsync(c, false);
            var nuove = voci.Select(v => new Hr_ChecklistVoce
            {
                Hr_ChecklistNavigation = c, Ordine = v.Ordine, Descrizione = v.Descrizione, IdAspNetUsersResponsabile = v.IdAspNetUsersResponsabile,
                Giorni = v.Giorni, DataPrevista = dataRiferimento.Date.AddDays(v.Giorni), Documento = v.Documento, Azione = v.Azione
            }).ToList();
            foreach (var v in nuove)
                await _Hr_ChecklistVoce.CreateAsync(v, false);
            await _Hr_Checklist.SaveChange();
            foreach (var v in nuove)
                await ScadenzaVoceAsync(ctx, cfg, c, v, dip);
            return c;
        }

        private async Task SpostaAsync(HrContesto ctx, Hr_Checklist c, DateTime data, Dip_Anagrafica dip)
        {
            var cfg = await _utility.CfgAsync(ctx);
            c.DataRiferimento = data;
            c.ModifiedDate = DateTime.Now;
            var voci = await _Hr_ChecklistVoce.FindAll(x => x.IdHr_Checklist == c.Id).AsTracking().ToListAsync();
            foreach (var v in voci)
                v.DataPrevista = data.AddDays(v.Giorni);
            await _Hr_Checklist.SaveChange();
            foreach (var v in voci)
                await ScadenzaVoceAsync(ctx, cfg, c, v, dip);
        }

        public async Task ScadenzaVoceAsync(HrContesto ctx, Hr_Cfg cfg, Hr_Checklist checklist, Hr_ChecklistVoce voce, Dip_Anagrafica dip)
        {
            if (checklist.Stato == Hr_StatoChecklist.Annullata)
            {
                await _deadlineRegistry.RemoveAsync(ctx.IdCompany, ApplicationType.HumanResources, FonteScadenza, voce.Id);
                return;
            }
            await _deadlineRegistry.UpsertAsync(new DeadlineRegistration
            {
                IdCompany = ctx.IdCompany, Application = ApplicationType.HumanResources, Fonte = FonteScadenza, IdRiferimento = voce.Id,
                Descrizione = HumanResourcesMessages.Get("ChecklistDeadline", voce.Descrizione, HrUtility.NomeDipendente(dip)),
                DataScadenza = voce.DataPrevista, GiorniPreavviso = cfg.GiorniPreavviso, IdAspNetUsersResponsabile = voce.IdAspNetUsersResponsabile,
                Url = "/hrchecklistedit", UrlState = new { id = checklist.Id }
            });
            if (voce.Completata)
                await _deadlineRegistry.CloseAsync(ctx.IdCompany, ApplicationType.HumanResources, FonteScadenza, voce.Id);
        }

        public async Task CompletaVoceAsync(HrContesto ctx, int idVoce)
        {
            var v = await _Hr_ChecklistVoce.FindAll(x => x.Id == idVoce && x.Hr_ChecklistNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                    ?? throw new UserMessageException(HumanResourcesMessages.Get("ChecklistItemNotFound"));
            if (v.Completata)
                return;
            v.Completata = true;
            v.DataCompletamento = DateTime.Now;
            v.IdAspNetUsersCompletamento = ctx.IdUtente;
            v.ModifiedDate = DateTime.Now;
            await _Hr_ChecklistVoce.SaveChange();
            await _deadlineRegistry.CloseAsync(ctx.IdCompany, ApplicationType.HumanResources, FonteScadenza, v.Id);
            await AggiornaStatoAsync(v.IdHr_Checklist);
        }

        // tutte le voci completate: checklist completata (e riaperta se una voce torna da fare)
        public async Task AggiornaStatoAsync(int idChecklist)
        {
            bool tutte = !await _Hr_ChecklistVoce.FindAll(x => x.IdHr_Checklist == idChecklist && !x.Completata).AnyAsync();
            await _Hr_Checklist.FindAll(x => x.Id == idChecklist && x.Stato != Hr_StatoChecklist.Annullata)
                               .ExecuteUpdateAsync(u => u.SetProperty(x => x.Stato, tutte ? Hr_StatoChecklist.Completata : Hr_StatoChecklist.Aperta));
        }
    }
}
