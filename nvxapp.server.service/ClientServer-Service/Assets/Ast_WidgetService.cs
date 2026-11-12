using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Assets;
using nvxapp.server.service.ClientServer_Service.Assets._utility;
using nvxapp.server.service.ClientServer_Service.Assets.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Assets
{
    /*
     Dati dei widget della dashboard di Risorse aziendali (ANALISI_ASSETS_WIDGET.md). Gli altri widget usano servizi gia' esistenti
     (prenotazioni, beni e veicoli dell'utente, restituzioni alla cessazione); qui c'e' solo il riepilogo della flotta.
     Scadenze della flotta: come lo scadenzario comune (fonti "ast.veicolo.scadenza" e "ast.veicolo.contratto"), veicoli attivi,
     scadenze non eseguite; "in arrivo" = da oggi entro Ast_Cfg.GiorniPreavviso. Dei contratti di noleggio/leasing (senza data di
     esecuzione) contano solo quelli in arrivo: uno gia' scaduto resterebbe in elenco per sempre.
     Riservato agli amministratori dell'azienda (CompanyPowerAdmin, CompanyAdmin), come la pagina dei veicoli.
    */
    public class Ast_WidgetService : ServiceBase, IAst_WidgetService
    {
        private const int MaxScadenze = 6;

        private readonly IAst_VeicoloRepository _Ast_Veicolo;
        private readonly IAst_VeicoloScadenzaRepository _Ast_VeicoloScadenza;
        private readonly IAst_MultaRepository _Ast_Multa;
        private readonly IAstUtility _utility;

        public Ast_WidgetService(IMapper mapper,
                                 UserManager<ApplicationUser> userManager,
                                 IAspNetUsersRepository aspNetUsersRepository,
                                 IOptions<JwtParameter> jwtParameter,
                                 IHttpContextAccessor httpContextAccessor,
                                 IConfiguration configuration,
                                 IAst_VeicoloRepository ast_VeicoloRepository,
                                 IAst_VeicoloScadenzaRepository ast_VeicoloScadenzaRepository,
                                 IAst_MultaRepository ast_MultaRepository,
                                 IAstUtility utility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Ast_Veicolo = ast_VeicoloRepository;
            _Ast_VeicoloScadenza = ast_VeicoloScadenzaRepository;
            _Ast_Multa = ast_MultaRepository;
            _utility = utility;
        }

        public virtual async Task<GenericResult<Ast_WidgetFlottaOutModel>> Ast_WidgetFlottaGet(GenericRequest<Ast_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var cfg = await _utility.CfgAsync(ctx);
                var oggi = DateTime.Today;
                var limite = oggi.AddDays(Math.Max(cfg.GiorniPreavviso, 0));

                var veicoliAttivi = await _Ast_Veicolo.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Stato == AstStatoVeicolo.Attivo).CountAsync();

                // scadenze dei veicoli non eseguite: scadute ed entro il preavviso
                var scadenze = await _Ast_VeicoloScadenza.FindAll(s => s.Ast_VeicoloNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica
                                                                      && s.Ast_VeicoloNavigation.Stato == AstStatoVeicolo.Attivo
                                                                      && s.DataEsecuzione == null && s.DataScadenza <= limite)
                                                         .Select(s => new
                                                         {
                                                             s.IdAst_Veicolo, s.Tipo, s.Descrizione, s.DataScadenza,
                                                             s.Ast_VeicoloNavigation!.Targa, s.Ast_VeicoloNavigation.Marca, s.Ast_VeicoloNavigation.Modello,
                                                         })
                                                         .ToListAsync();

                // contratti di noleggio/leasing in scadenza entro il preavviso
                var contratti = await _Ast_Veicolo.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Stato == AstStatoVeicolo.Attivo
                                                               && x.ScadenzaContratto != null && x.ScadenzaContratto >= oggi && x.ScadenzaContratto <= limite)
                                                  .Select(x => new { x.Id, x.Targa, x.Marca, x.Modello, x.Possesso, x.ScadenzaContratto })
                                                  .ToListAsync();

                var multe = await _Ast_Multa.FindAll(m => m.Ast_VeicoloNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica && m.Stato == AstStatoMulta.DaNotificare).CountAsync();

                var elenco = scadenze.Select(s => new Ast_WidgetScadenzaModel
                                     {
                                         IdAst_Veicolo = s.IdAst_Veicolo,
                                         Targa = s.Targa,
                                         Veicolo = Ast_FlottaService.DescrizioneVeicolo(s.Marca, s.Modello, s.Targa),
                                         Descrizione = Ast_FlottaService.DescrizioneScadenza(s.Tipo, s.Descrizione),
                                         DataScadenza = s.DataScadenza,
                                         Scaduta = s.DataScadenza < oggi,
                                     })
                                     .Concat(contratti.Select(c => new Ast_WidgetScadenzaModel
                                     {
                                         IdAst_Veicolo = c.Id,
                                         Targa = c.Targa,
                                         Veicolo = Ast_FlottaService.DescrizioneVeicolo(c.Marca, c.Modello, c.Targa),
                                         Descrizione = AssetsMessages.Get("WidgetContractDeadline", AssetsMessages.Get("VehicleOwnership_" + (int)c.Possesso)),
                                         DataScadenza = c.ScadenzaContratto!.Value,
                                         Contratto = true,
                                     }))
                                     .OrderBy(x => x.DataScadenza)
                                     .ToList();

                return new Ast_WidgetFlottaOutModel
                {
                    GiorniPreavviso = cfg.GiorniPreavviso,
                    VeicoliAttivi = veicoliAttivi,
                    Scadute = elenco.Count(x => x.Scaduta),
                    InArrivo = elenco.Count(x => !x.Scaduta),
                    MulteDaNotificare = multe,
                    Scadenze = elenco.Take(MaxScadenze).ToList(),
                };
            }, isSubProcess);
        }
    }

    public interface IAst_WidgetService : IServiceBase
    {
        Task<GenericResult<Ast_WidgetFlottaOutModel>> Ast_WidgetFlottaGet(GenericRequest<Ast_EmptyInModel> model, Boolean isSubProcess);
    }
}
