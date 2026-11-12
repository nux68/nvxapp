using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.Assets;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Request;

namespace nvxapp.server.service.ClientServer_Service.Assets._utility
{
    /*
     Prenotazioni da approvare nelle richieste comuni di Infrastructure (ANALISI_INFRASTRUCTURE_RICHIESTE.md §3bis, AST-RICH):
     tipo PrenotazioneRisorsa di Dip_GG_Richiesta, approvato da un amministratore dell'azienda (strategia Singola, motore
     RichiestaCommonService). Qui: testi delle righe e stato della prenotazione allineato all'esito.
        - la prenotazione e' la fonte dello stato mostrato: se un amministratore l'ha annullata dalla pagina prenotazioni,
          la riga dice "Annullata" anche se la richiesta era approvata
        - StrategiaAsync applica prima la scadenza pigra (AstApprovazioni.ScadiAsync): RequestService la chiama a ogni conteggio
          ed elenco delle richieste, cosi' le richieste non decise all'ora di inizio spariscono anche dai contatori
    */
    public class AstRichiestaTipoHandler : IRichiestaTipoHandler
    {
        private static readonly IReadOnlyCollection<TipoRichiesta> TipiGestiti = new[] { TipoRichiesta.PrenotazioneRisorsa };

        private readonly IAst_PrenotazioneRepository _Ast_Prenotazione;
        private readonly IAstUtility _utility;
        private readonly IAstApprovazioni _approvazioni;
        private readonly IRichiestaCommonService _richieste;

        public AstRichiestaTipoHandler(IAst_PrenotazioneRepository ast_PrenotazioneRepository, IAstUtility utility,
                                       IAstApprovazioni approvazioni, IRichiestaCommonService richieste)
        {
            _Ast_Prenotazione = ast_PrenotazioneRepository;
            _utility = utility;
            _approvazioni = approvazioni;
            _richieste = richieste;
        }

        public ApplicationType Application => ApplicationType.Assets;

        public IReadOnlyCollection<TipoRichiesta> Tipi => TipiGestiti;

        public async Task<StrategiaApprovazione> StrategiaAsync()
        {
            await _approvazioni.ScadiAsync(await _utility.ContestoAsync());
            return StrategiaApprovazione.Singola;
        }

        public async Task CompletaAsync(List<(Dip_GG_Richiesta Richiesta, RequestItem Item)> righe, RequestScope scope)
        {
            var ctx = await _utility.ContestoAsync();
            var ids = righe.Select(x => AstApprovazioni.IdPrenotazione(x.Richiesta)).Where(x => x != null).Select(x => x!.Value).Distinct().ToList();
            var prenotazioni = await _Ast_Prenotazione.FindAll(x => ids.Contains(x.Id) && x.Ast_RisorsaNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                                      .Include(x => x.Ast_RisorsaNavigation).ThenInclude(x => x!.Ast_VeicoloNavigation)
                                                      .ToDictionaryAsync(x => x.Id);
            var ora = DateTime.Now;

            foreach (var (r, item) in righe)
            {
                item.TipoTesto = AssetsMessages.Get("RequestTypeBooking");
                item.Url = Ast_PrenotazioniService.UrlPrenotazioni;
                var id = AstApprovazioni.IdPrenotazione(r);
                if (id == null || !prenotazioni.TryGetValue(id.Value, out var p))
                {
                    // prenotazione cancellata (es. risorsa eliminata): la richiesta resta solo in lettura
                    item.StatoTesto = AssetsMessages.Get("RequestStateCancelled");
                    item.Stato = RequestState.Annullata;
                    item.CanApprove = item.CanReject = item.CanCancel = false;
                    continue;
                }

                item.Dal = p.Inizio;
                item.Al = p.Fine;
                item.Dettaglio = Dettaglio(p);
                item.Stato = Stato(p.Stato);
                item.StatoTesto = p.Stato switch
                {
                    AstStatoPrenotazione.Confermata => AssetsMessages.Get("RequestStateApproved"),
                    AstStatoPrenotazione.Rifiutata => AstApprovazioni.Motivo(r) is string m ? AssetsMessages.Get("RequestStateRejectedReason", m)
                                                                                              : AssetsMessages.Get("RequestStateRejected"),
                    AstStatoPrenotazione.Annullata => AssetsMessages.Get("RequestStateCancelled"),
                    _ => AssetsMessages.Get("RequestStatePending"),
                };
                bool inAttesa = p.Stato == AstStatoPrenotazione.InAttesa;
                item.CanApprove = item.CanApprove && inAttesa && p.Inizio > ora;
                item.CanReject = item.CanReject && inAttesa;
                // il richiedente annulla la prenotazione in attesa o confermata finche' non e' finita
                item.CanCancel = item.CanCancel && p.Fine > ora && (inAttesa || p.Stato == AstStatoPrenotazione.Confermata);
            }
        }

        public async Task ApproveAsync(Dip_GG_Richiesta richiesta)
        {
            var (ctx, p) = await PrenotazioneAsync(richiesta);
            if (p.Stato != AstStatoPrenotazione.InAttesa || p.Inizio <= DateTime.Now)
                throw new UserMessageException(AssetsMessages.Get("BookingNotPending"));
            _utility.RichiediAmministratore(ctx);

            // prima la prenotazione (ricontrollo dei conflitti con il lock della risorsa), poi la richiesta; se il motore
            // rifiuta (es. richiesta decisa nel frattempo) la prenotazione torna in attesa
            p.Stato = AstStatoPrenotazione.Confermata;
            p.ModifiedDate = DateTime.Now;
            p.ChangeUser = ctx.IdUtente;
            if (!await _Ast_Prenotazione.SalvaSeLiberaAsync(p))
                throw new UserMessageException(AssetsMessages.Get("BookingConflict", p.Ast_RisorsaNavigation!.Descrizione));
            try
            {
                await _richieste.ApprovaAsync(richiesta, await _approvazioni.AvvisoAsync(ctx, p, p.Ast_RisorsaNavigation!.Descrizione));
            }
            catch
            {
                await _Ast_Prenotazione.FindAll(x => x.Id == p.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.Stato, AstStatoPrenotazione.InAttesa));
                throw;
            }
        }

        public async Task RejectAsync(Dip_GG_Richiesta richiesta, string? motivo)
        {
            var (ctx, p) = await PrenotazioneAsync(richiesta);
            if (p.Stato != AstStatoPrenotazione.InAttesa)
                throw new UserMessageException(AssetsMessages.Get("BookingNotPending"));
            await _richieste.RifiutaAsync(richiesta, AstUtility.Testo(motivo, 500), await _approvazioni.AvvisoAsync(ctx, p, p.Ast_RisorsaNavigation!.Descrizione));
            await ImpostaAsync(ctx, p, AstStatoPrenotazione.Rifiutata);   // fascia libera
        }

        public async Task CancelAsync(Dip_GG_Richiesta richiesta)
        {
            var (ctx, p) = await PrenotazioneAsync(richiesta);
            if ((p.Stato != AstStatoPrenotazione.InAttesa && p.Stato != AstStatoPrenotazione.Confermata) || p.Fine <= DateTime.Now)
                throw new UserMessageException(AssetsMessages.Get("BookingNotEditable"));
            await _richieste.AnnullaAsync(richiesta, await _approvazioni.AvvisoAsync(ctx, p, p.Ast_RisorsaNavigation!.Descrizione));
            await ImpostaAsync(ctx, p, AstStatoPrenotazione.Annullata);
        }

        // ---------------------------------------------------------------------------

        private async Task<(AstContesto, Ast_Prenotazione)> PrenotazioneAsync(Dip_GG_Richiesta richiesta)
        {
            var ctx = await _utility.ContestoAsync();
            var id = AstApprovazioni.IdPrenotazione(richiesta) ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
            var p = await _Ast_Prenotazione.FindAll(x => x.Id == id && x.Ast_RisorsaNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                           .Include(x => x.Ast_RisorsaNavigation).AsTracking().FirstOrDefaultAsync()
                    ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
            return (ctx, p);
        }

        private async Task ImpostaAsync(AstContesto ctx, Ast_Prenotazione p, AstStatoPrenotazione stato)
        {
            var ora = DateTime.Now;
            p.Stato = stato;
            if (stato == AstStatoPrenotazione.Annullata)
                p.DataAnnullamento = ora;
            p.ModifiedDate = ora;
            p.ChangeUser = ctx.IdUtente;
            await _Ast_Prenotazione.SaveChange();
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

        private static RequestState Stato(AstStatoPrenotazione s) => s switch
        {
            AstStatoPrenotazione.Confermata => RequestState.Approvata,
            AstStatoPrenotazione.Rifiutata => RequestState.Rifiutata,
            AstStatoPrenotazione.Annullata => RequestState.Annullata,
            _ => RequestState.InAttesa,
        };
    }
}
