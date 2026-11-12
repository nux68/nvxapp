using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.Assets;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Request;

namespace nvxapp.server.service.ClientServer_Service.Assets._utility
{
    /*
     Prenotazioni da approvare nelle richieste comuni di Infrastructure (ANALISI_INFRASTRUCTURE_RICHIESTE.md, RICH-AST.3).
     Richiesta = prenotazione passata dal ciclo di approvazione (Ast_Prenotazione.RichiestaApprovazione: nata InAttesa su
     una risorsa con RichiedeApprovazione).
        - Mine: le proprie richieste (DaApprovare = in attesa, Tutte = tutte); annulla il richiedente (in attesa o approvata, non passata)
        - Admin (amministratori dell'azienda): DaApprovare = in attesa, Tutte = tutte, Approvate = confermate; approva / rifiuta
        - prima di ogni lettura le richieste scadute (inizio passato senza decisione) si annullano (AstApprovazioni.ScadiAsync)
    */
    public class AstRequestProvider : IRequestProvider
    {
        public const string PrefixAst = "ast";
        private const int MaxRighe = 500;

        private readonly IAst_PrenotazioneRepository _Ast_Prenotazione;
        private readonly IAstUtility _utility;
        private readonly IAstApprovazioni _approvazioni;

        public AstRequestProvider(IAst_PrenotazioneRepository ast_PrenotazioneRepository, IAstUtility utility, IAstApprovazioni approvazioni)
        {
            _Ast_Prenotazione = ast_PrenotazioneRepository;
            _utility = utility;
            _approvazioni = approvazioni;
        }

        public ApplicationType Application => ApplicationType.Assets;

        public string Prefix => PrefixAst;

        public async Task<RequestCounts> CountAsync(bool admin)
        {
            var ctx = await _utility.ContestoAsync();
            await _approvazioni.ScadiAsync(ctx);
            var inAttesa = _Ast_Prenotazione.FindAll(x => x.Ast_RisorsaNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Stato == AstStatoPrenotazione.InAttesa);
            return new RequestCounts
            {
                InAttesa = await inAttesa.CountAsync(x => x.IdAspNetUsers == ctx.IdUtente),
                DaApprovare = admin && ctx.Amministratore ? await inAttesa.CountAsync() : 0,
            };
        }

        public async Task<List<RequestItem>> ListAsync(RequestScope scope, RequestFilter filtro)
        {
            var ctx = await _utility.ContestoAsync();
            bool admin = scope == RequestScope.Admin;
            if (admin && !ctx.Amministratore)
                throw new UserMessageException(AssetsMessages.Get("NotAllowed"));
            await _approvazioni.ScadiAsync(ctx);

            var q = _Ast_Prenotazione.FindAll(x => x.Ast_RisorsaNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.RichiestaApprovazione);
            if (!admin)
                q = q.Where(x => x.IdAspNetUsers == ctx.IdUtente);
            q = filtro switch
            {
                RequestFilter.DaApprovare => q.Where(x => x.Stato == AstStatoPrenotazione.InAttesa),
                RequestFilter.Approvate => q.Where(x => x.Stato == AstStatoPrenotazione.Confermata),
                _ => q,
            };
            var righe = await q.Include(x => x.Ast_RisorsaNavigation).ThenInclude(x => x!.Ast_VeicoloNavigation)
                               .OrderByDescending(x => x.Inizio).Take(MaxRighe).ToListAsync();
            var nomi = admin ? await _approvazioni.NomiAsync(ctx, righe.Select(x => x.IdAspNetUsers)) : new Dictionary<string, string>();
            var ora = DateTime.Now;

            return righe.Select(p => new RequestItem
            {
                Id = p.Id.ToString(),
                Tipo = "prenotazione",
                TipoTesto = AssetsMessages.Get("RequestTypeBooking"),
                Richiedente = nomi.TryGetValue(p.IdAspNetUsers, out var n) ? n : null,
                Dal = p.Inizio,
                Al = p.Fine,
                Dettaglio = Dettaglio(p),
                Stato = Stato(p.Stato),
                StatoTesto = StatoTesto(p),
                CanApprove = admin && p.Stato == AstStatoPrenotazione.InAttesa && p.Inizio > ora,
                CanReject = admin && p.Stato == AstStatoPrenotazione.InAttesa,
                CanCancel = !admin && p.IdAspNetUsers == ctx.IdUtente && p.Fine > ora
                            && (p.Stato == AstStatoPrenotazione.InAttesa || p.Stato == AstStatoPrenotazione.Confermata),
                Url = Ast_PrenotazioniService.UrlPrenotazioni,
                DataRichiesta = p.CreationDate,
            }).ToList();
        }

        public async Task ApproveAsync(string id)
        {
            var (ctx, p) = await InAttesaAsync(id);
            if (p.Inizio <= DateTime.Now)
                throw new UserMessageException(AssetsMessages.Get("BookingNotPending"));
            p.Stato = AstStatoPrenotazione.Confermata;
            Decisione(ctx, p);
            // ricontrollo dei conflitti (con il lock della risorsa): la fascia era gia' occupata dalla richiesta stessa
            if (!await _Ast_Prenotazione.SalvaSeLiberaAsync(p))
                throw new UserMessageException(AssetsMessages.Get("BookingConflict", p.Ast_RisorsaNavigation!.Descrizione));
            await _approvazioni.EsitoAsync(ctx, p, p.Ast_RisorsaNavigation!.Descrizione);
        }

        public async Task RejectAsync(string id, string? motivo)
        {
            var (ctx, p) = await InAttesaAsync(id);
            p.Stato = AstStatoPrenotazione.Rifiutata;
            p.MotivoRifiuto = AstUtility.Testo(motivo, 500);
            Decisione(ctx, p);
            await _Ast_Prenotazione.SaveChange();
            await _approvazioni.EsitoAsync(ctx, p, p.Ast_RisorsaNavigation!.Descrizione);
        }

        public async Task CancelAsync(string id)
        {
            var ctx = await _utility.ContestoAsync();
            var p = await CaricaAsync(ctx, id);
            if (p.IdAspNetUsers != ctx.IdUtente)
                throw new UserMessageException(AssetsMessages.Get("NotAllowed"));
            if ((p.Stato != AstStatoPrenotazione.InAttesa && p.Stato != AstStatoPrenotazione.Confermata) || p.Fine <= DateTime.Now)
                throw new UserMessageException(AssetsMessages.Get("BookingNotEditable"));
            var ora = DateTime.Now;
            p.Stato = AstStatoPrenotazione.Annullata;
            p.DataAnnullamento = ora;
            p.ModifiedDate = ora;
            p.ChangeUser = ctx.IdUtente;
            await _Ast_Prenotazione.SaveChange();
            await _approvazioni.AvvisaAsync(ctx, new[] { p.IdAspNetUsers }, p.Id);
        }

        // ---------------------------------------------------------------------------

        private async Task<(AstContesto, Ast_Prenotazione)> InAttesaAsync(string id)
        {
            var ctx = await _utility.ContestoAsync();
            _utility.RichiediAmministratore(ctx);
            await _approvazioni.ScadiAsync(ctx);
            var p = await CaricaAsync(ctx, id);
            if (p.Stato != AstStatoPrenotazione.InAttesa)
                throw new UserMessageException(AssetsMessages.Get("BookingNotPending"));
            return (ctx, p);
        }

        private async Task<Ast_Prenotazione> CaricaAsync(AstContesto ctx, string id)
        {
            if (!int.TryParse(id, out var idPrenotazione))
                throw new UserMessageException(AssetsMessages.Get("NotFound"));
            return await _Ast_Prenotazione.FindAll(x => x.Id == idPrenotazione && x.Ast_RisorsaNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                          .Include(x => x.Ast_RisorsaNavigation).AsTracking().FirstOrDefaultAsync()
                   ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
        }

        private static void Decisione(AstContesto ctx, Ast_Prenotazione p)
        {
            var ora = DateTime.Now;
            p.IdAspNetUsersDecisione = ctx.IdUtente;
            p.DataDecisione = ora;
            p.ModifiedDate = ora;
            p.ChangeUser = ctx.IdUtente;
        }

        private static string Dettaglio(Ast_Prenotazione p)
        {
            var r = p.Ast_RisorsaNavigation;
            var v = r?.Ast_VeicoloNavigation;
            var risorsa = r == null ? string.Empty
                        : v == null ? r.Descrizione
                        : r.Descrizione + " (" + Ast_FlottaService.DescrizioneVeicolo(v.Marca, v.Modello, v.Targa) + ")";
            return string.Join(" - ", new[] { risorsa, p.Titolo, p.Note }.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        public static RequestState Stato(AstStatoPrenotazione s) => s switch
        {
            AstStatoPrenotazione.Confermata => RequestState.Approvata,
            AstStatoPrenotazione.Rifiutata => RequestState.Rifiutata,
            AstStatoPrenotazione.Annullata => RequestState.Annullata,
            _ => RequestState.InAttesa,
        };

        private static string StatoTesto(Ast_Prenotazione p) => p.Stato switch
        {
            AstStatoPrenotazione.Confermata => AssetsMessages.Get("RequestStateApproved"),
            AstStatoPrenotazione.Rifiutata => string.IsNullOrEmpty(p.MotivoRifiuto) ? AssetsMessages.Get("RequestStateRejected")
                                                                                   : AssetsMessages.Get("RequestStateRejectedReason", p.MotivoRifiuto),
            AstStatoPrenotazione.Annullata => AssetsMessages.Get("RequestStateCancelled"),
            _ => AssetsMessages.Get("RequestStatePending"),
        };
    }
}
