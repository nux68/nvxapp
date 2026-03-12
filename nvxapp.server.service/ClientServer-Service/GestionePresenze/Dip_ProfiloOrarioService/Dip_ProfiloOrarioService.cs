using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService
{

    public class Dip_ProfiloOrarioService : ServiceBase, IDip_ProfiloOrarioService
    {
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IDip_ProfiloOrarioRepository _dip_ProfiloOrarioRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IPar_ProfiloOrarioRepository _par_ProfiloOrarioRepository;
        private readonly IPar_ProfiloOrarioGGRepository _par_ProfiloOrarioGGRepository;
        private readonly IPar_OrarioService             _par_OrarioService;
        

        public Dip_ProfiloOrarioService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IDip_AnagraficaRepository dip_AnagraficaRepository,
                                  IPar_ProfiloOrarioRepository par_ProfiloOrarioRepository,
                                  IPar_ProfiloOrarioGGRepository par_ProfiloOrarioGGRepository,
                                  IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                  IPar_OrarioService par_OrarioService,
                                  IDip_ProfiloOrarioRepository dip_ProfiloOrarioRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_ProfiloOrarioRepository = dip_ProfiloOrarioRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _par_ProfiloOrarioRepository = par_ProfiloOrarioRepository;
            _par_ProfiloOrarioGGRepository = par_ProfiloOrarioGGRepository;
            _par_OrarioService            = par_OrarioService;
        }

        public virtual async Task<GenericResult<Dip_ProfiloOrario_Get_OutModel>> Dip_ProfiloOrarioGet(GenericRequest<Dip_ProfiloOrario_Get_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_ProfiloOrario_Get_OutModel();
                
                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var entity = _dip_ProfiloOrarioRepository.GetAll().Where( x=> x.IdDip_RapportoLavoro == model.Data.Id).ToList();
                    retVal.Dip_ProfiloOrario = _mapper.Map<List<Dip_ProfiloOrarioModel>>(entity);
                }
                await Task.Delay(DelayAsyncMethod);
                
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_ProfiloOrario_Put_OutModel>> Dip_ProfiloOrarioPut(GenericRequest<Dip_ProfiloOrario_Put_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_ProfiloOrario_Put_OutModel();
                //var entity = _mapper.Map<Dip_ProfiloOrario>(model.Data.Dip_ProfiloOrario);

                
                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {

                    // sostituisce il valore negativo
                    var newRow = model.Data.Dip_ProfiloOrario.Where(x=> x.Id<0).ToList();
                    foreach(var item in newRow)
                        item.Id = 0;
                        

                    
                    //rileggo i dati originali
                    var reqOrig_Data = new GenericRequest<Dip_ProfiloOrario_Get_InModel>();
                    reqOrig_Data.Data.Id = model.Data.Id;

                    var Orig_Data = await Dip_ProfiloOrarioGet(reqOrig_Data, true);
                    if (Orig_Data.Success && Orig_Data.Data != null)
                    {
                        //cancellazione record eliminati
                        foreach (var item in Orig_Data.Data.Dip_ProfiloOrario)
                        {   

                            //ottengo il record orig del db
                            var origRec = _dip_ProfiloOrarioRepository.FindAll(x => x.Id == item.Id).FirstOrDefault();

                            if (origRec != null)
                            {
                                 //cerco la Par_ProfiloOrarioGG nei dati tornati dal client
                                 var orig_TMP = model.Data.Dip_ProfiloOrario.Where(x => x.Id == item.Id).FirstOrDefault();

                                //se non trovo la corrispondenza nei dati del client, vul dire che è stata eliminata
                                if (orig_TMP == null)
                                {
                                    //procedo alla cancelazione
                                    await _dip_ProfiloOrarioRepository.DeleteAsync(origRec);
                                }
                            }
                        }
                        
                        //upsert 
                        foreach (var item in model.Data.Dip_ProfiloOrario)
                        {
                            //ottengo il record orig del db
                            var par_ProfiloOrarioGG = _dip_ProfiloOrarioRepository.FindAll(x => x.Id == item.Id).FirstOrDefault();
                            if (par_ProfiloOrarioGG == null)
                            {
                                par_ProfiloOrarioGG = _mapper.Map<Dip_ProfiloOrario>(item);
                                par_ProfiloOrarioGG.IdDip_RapportoLavoro = model.Data.Id;
                                par_ProfiloOrarioGG.Id = 0;
                            }
                            else
                            {
                                par_ProfiloOrarioGG = _mapper.Map<Dip_ProfiloOrario>(item);
                            }
                            par_ProfiloOrarioGG = await _dip_ProfiloOrarioRepository.UpsertAsync(par_ProfiloOrarioGG);
                        }


                        //rileggo i dati dopo le varizioni per ritornare il valore corrente
                        Orig_Data = await Dip_ProfiloOrarioGet(reqOrig_Data, true);    
                        if (Orig_Data.Success && Orig_Data.Data != null)
                        {
                            retVal.Dip_ProfiloOrario = Orig_Data.Data.Dip_ProfiloOrario;
                        }


                    }

                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;

            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_ProfiloOrario_Get_Profile_4Calculation_OutModel>> Dip_ProfiloOrario_Get_Profile_4Calculation(GenericRequest<Dip_ProfiloOrario_Get_Profile_4Calculation_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_ProfiloOrario_Get_Profile_4Calculation_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var Dal = model.Data.Dal;
                    var Al  = model.Data.Al;

                    // ── 1. Dip_Anagrafica ────────────────────────────────────────────
                    // filtra le anagrafiche il cui IdAspNetUsers è nella lista richiesta
                    var anagrafiche = _dip_AnagraficaRepository
                        .FindAll(a => model.Data.UsersId.Contains(a.IdAspNetUsers))
                        .ToList();

                    var anagraficaIds = anagrafiche.Select(a => a.Id).ToList();

                    // ── 2. Dip_RapportoLavoro ────────────────────────────────────────
                    // Il rapporto è compatibile se il suo periodo di lavoro si sovrappone
                    // anche parzialmente al periodo richiesto [Dal, Al].
                    //
                    // Casi gestiti (DataAss e DataLic possono cadere a metà mese):
                    //
                    //   [Dal ............. Al]        periodo richiesto
                    //         [Ass .......]           assunto a metà: DataAss <= Al  ✓
                    //   [..... Lic]                   licenziato a metà: DataLic >= Dal  ✓
                    //         [Ass .. Lic]            entrambi a metà: entrambe le condizioni  ✓
                    //   DataLic == null               ancora in servizio: sempre compatibile  ✓
                    //
                    // Condizione: DataAss <= Al  &&  (DataLic == null || DataLic >= Dal)
                    var rapporti = _dip_RapportoLavoroRepository
                        .FindAll(r =>
                            anagraficaIds.Contains(r.IdDip_Anagrafica) &&
                            r.DataAss != null &&
                            r.DataAss.Value <= Al &&
                            (r.DataLic == null || r.DataLic.Value >= Dal))
                        .ToList();

                    var rapportoIds = rapporti.Select(r => r.Id).ToList();

                    // ── 3. Dip_ProfiloOrario ─────────────────────────────────────────
                    // Stesso criterio di sovrapposizione: la riga di profilo orario è
                    // compatibile se il suo intervallo [profilo.Dal, profilo.Al] si
                    // sovrappone anche parzialmente al periodo richiesto [Dal, Al].
                    //
                    // Condizione: profilo.Dal <= Al  &&  profilo.Al >= Dal
                    var profili = _dip_ProfiloOrarioRepository
                        .FindAll(p =>
                            rapportoIds.Contains(p.IdDip_RapportoLavoro) &&
                            p.Dal <= Al &&
                            p.Al  >= Dal)
                        .ToList();

                    // ── 4. Par_ProfiloOrario ─────────────────────────────────────────
                    // carica le testate dei profili orario referenziati
                    // (serve TipoProfilo e NumGiorniCiclo per la risoluzione del giorno)
                    var parProfiloIds = profili
                        .Where(p => p.IdPar_ProfiloOrario.HasValue)
                        .Select(p => p.IdPar_ProfiloOrario!.Value)
                        .Distinct()
                        .ToList();

                    var parProfili = _par_ProfiloOrarioRepository
                        .FindAll(pp => parProfiloIds.Contains(pp.Id))
                        .ToList();

                    // ── 5. Par_ProfiloOrarioGG ───────────────────────────────────────
                    // carica TUTTE le righe giornaliere senza filtrare su ZOrder:
                    // per ogni (IdPar_ProfiloOrario, NumGiorno) ci possono essere
                    // più orari ordinati per ZOrder (1=base, 2+=override condizionale)
                    var parProfiliGG = _par_ProfiloOrarioGGRepository
                        .FindAll(gg => parProfiloIds.Contains(gg.IdPar_ProfiloOrario))
                        .ToList();

                    // ── 6. Composizione DaySlots ─────────────────────────────────────
                    // per ogni dipendente × ogni giorno del periodo richiesto, risolve
                    // quale profilo è attivo e tutte le sue righe orario ordinate per ZOrder
                    foreach (var rapporto in rapporti)
                    {
                        var anagrafica = anagrafiche.First(a => a.Id == rapporto.IdDip_Anagrafica);

                        // limite effettivo del rapporto intersecato con il periodo richiesto
                        var giornoInizio = Dal > rapporto.DataAss!.Value ? Dal : rapporto.DataAss!.Value;
                        var giornoFine   = (rapporto.DataLic == null || rapporto.DataLic.Value > Al)
                                            ? Al
                                            : rapporto.DataLic.Value;

                        for (var giorno = giornoInizio; giorno <= giornoFine; giorno = giorno.AddDays(1))
                        {
                            // trova il profilo Dip attivo esattamente quel giorno
                            var profilo = profili
                                .Where(p => p.IdDip_RapportoLavoro == rapporto.Id
                                         && p.Dal <= giorno
                                         && p.Al  >= giorno
                                         && p.IdPar_ProfiloOrario.HasValue)
                                .FirstOrDefault();

                            if (profilo == null) continue;

                            var parProfilo = parProfili
                                .FirstOrDefault(pp => pp.Id == profilo.IdPar_ProfiloOrario!.Value);

                            if (parProfilo == null) continue;

                            // calcola il NumGiorno in base al TipoProfilo
                            int numGiorno;
                            if (parProfilo.TipoProfilo == TipoProfilo.Settimanale)
                            {
                                // Convenzione DB: 1=Lun, 2=Mar, 3=Mer, 4=Gio, 5=Ven, 6=Sab, 7=Dom
                                // DayOfWeek C#:   1=Lun, 2=Mar, 3=Mer, 4=Gio, 5=Ven, 6=Sab, 0=Dom
                                numGiorno = giorno.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)giorno.DayOfWeek;
                            }
                            else // Ciclico
                            {
                                numGiorno = ((giorno - profilo.Dal).Days + profilo.NumGiornoPartenzaCiclo)
                                            % parProfilo.NumGiorniCiclo;
                            }

                            // recupera TUTTE le righe del giorno ordinate per ZOrder
                            var orariDelGiorno = parProfiliGG
                                .Where(gg => gg.IdPar_ProfiloOrario == profilo.IdPar_ProfiloOrario!.Value
                                          && gg.NumGiorno           == numGiorno)
                                .OrderBy(gg => gg.ZOrder)
                                .Select(gg => new Dip_ProfiloOrario_DaySlot_GG
                                {
                                    ZOrder       = gg.ZOrder,
                                    IdPar_Orario = gg.IdPar_Orario
                                })
                                .ToList();

                            retVal.DaySlots.Add(new Dip_ProfiloOrario_DaySlot
                            {
                                IdAspNetUsers        = anagrafica.IdAspNetUsers,
                                IdDip_RapportoLavoro = rapporto.Id,
                                Data                 = giorno,
                                IdPar_ProfiloOrario  = profilo.IdPar_ProfiloOrario!.Value,
                                Orari                = orariDelGiorno
                            });
                        }
                    }

                     // lista univoca di tutti gli IdPar_Orario presenti nei DaySlots
                    var parOrarioIds = retVal.DaySlots
                        .SelectMany(ds => ds.Orari)
                        .Select(gg => gg.IdPar_Orario)
                        .Distinct()
                        .ToList();

                    // ── 7. Par_Orario + Par_OrarioIntervalloHH ───────────────────────
                    // Par_OrarioGet ritorna già sia la testata che gli intervalli orari,
                    // quindi una sola chiamata per id copre entrambe le liste
                    retVal.ParOrario            = new List<Par_OrarioModel>();
                    retVal.Par_OrarioIntervalloHH = new List<Par_OrarioIntervalloHHModel>();

                    foreach (var idOrario in parOrarioIds)
                    {
                        var req = new GenericRequest<Par_Orario_GetInModel>();
                        req.Data.Id = idOrario;

                        var res = await _par_OrarioService.Par_OrarioGet(req, true);
                        if (res.Success && res.Data != null)
                        {
                            if (res.Data.Par_Orario != null)
                                retVal.ParOrario.Add(res.Data.Par_Orario);

                            retVal.Par_OrarioIntervalloHH.AddRange(res.Data.Par_OrarioIntervalloHH);
                        }
                    }
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;

            }, isSubProcess);
        }
    }

    public interface IDip_ProfiloOrarioService : IServiceBase
    {
        Task<GenericResult<Dip_ProfiloOrario_Get_OutModel>> Dip_ProfiloOrarioGet(GenericRequest<Dip_ProfiloOrario_Get_InModel> model, bool isSubProcess);
        Task<GenericResult<Dip_ProfiloOrario_Put_OutModel>> Dip_ProfiloOrarioPut(GenericRequest<Dip_ProfiloOrario_Put_InModel> model, bool isSubProcess);
        Task<GenericResult<Dip_ProfiloOrario_Get_Profile_4Calculation_OutModel>> Dip_ProfiloOrario_Get_Profile_4Calculation(GenericRequest<Dip_ProfiloOrario_Get_Profile_4Calculation_InModel> model, bool isSubProcess);
    }
}
// Convenzione DB: 1=Lun, 2=Mar, 3=Mer, 4=Gio, 5=Ven, 6=Sab, 7=Dom
// DayOfWeek C#:   0=Dom, 1=Lun, 2=Mar, 3=Mer, 4=Gio, 5=Ven, 6=Sab
//numGiorno = giorno.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)giorno.DayOfWeek;
