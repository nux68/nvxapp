using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant; // <-- corretto per Az_SubCommessaAttivita
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService
{
    public class Az_SubCommessaAttivitaService : ServiceBase, IAz_SubCommessaAttivitaService
    {
        private readonly IAz_SubCommessaAttivitaRepository _az_SubCommessaAttivitaRepository;
        private readonly IAz_CommessaRepository _az_CommessaRepository;
        private readonly IAz_SubCommessaRepository _az_SubCommessaRepository;
        private readonly IPar_AttivitaRepository _par_AttivitaRepository;
        private readonly IAz_ClienteRepository _az_ClienteRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Az_SubCommessaAttivitaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_SubCommessaAttivitaRepository az_SubCommessaAttivitaRepository,
                                  IAz_CommessaRepository az_CommessaRepository,
                                  IAz_SubCommessaRepository az_SubCommessaRepository,
                                  IPar_AttivitaRepository par_AttivitaRepository,
                                  IAz_ClienteRepository az_ClienteRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SubCommessaAttivitaRepository = az_SubCommessaAttivitaRepository;
            _az_CommessaRepository = az_CommessaRepository;
            _az_SubCommessaRepository = az_SubCommessaRepository;
            _par_AttivitaRepository = par_AttivitaRepository;
            _az_ClienteRepository = az_ClienteRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Az_SubCommessaAttivita_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaAttivita_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessaAttivita_GetAll_OutModel retVal = new Az_SubCommessaAttivita_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_SubCommessaAttivita = await _az_SubCommessaAttivitaRepository.FindAll();
                    retVal.Az_SubCommessaAttivita = _mapper.Map<List<Az_SubCommessaAttivitaModel>>(az_SubCommessaAttivita);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_SubCommessaAttivita_Get4SubCommessa_OutModel>> Get4SubCommessa(GenericRequest<Az_SubCommessaAttivita_Get4SubCommessa_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessaAttivita_Get4SubCommessa_OutModel retVal = new Az_SubCommessaAttivita_Get4SubCommessa_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var entities = _az_SubCommessaAttivitaRepository.FindAll(x => x.IdAz_SubCommessa == model.Data.IdAz_SubCommessa).ToList();
                    retVal.Az_SubCommessaAttivita = _mapper.Map<List<Az_SubCommessaAttivita4EditModel>>(entities);
                }

                foreach (var item in retVal.Az_SubCommessaAttivita)
                    item.Checked = true;

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_SubCommessaAttivita_Put4SubCommessa_OutModel>> Put4SubCommessa(GenericRequest<Az_SubCommessaAttivita_Put4SubCommessa_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessaAttivita_Put4SubCommessa_OutModel retVal = new Az_SubCommessaAttivita_Put4SubCommessa_OutModel();

                //rileggo i dati originali
                var req = new GenericRequest<Az_SubCommessaAttivita_Get4SubCommessa_InModel>();
                req.Data.IdAz_SubCommessa = model.Data.IdAz_SubCommessa;

                var res = await Get4SubCommessa(req, true);
                if (res.Success && res.Data != null)
                {
                    //cancellazione attività eliminate
                    foreach (var item in res.Data.Az_SubCommessaAttivita)
                    {
                        var orig = _az_SubCommessaAttivitaRepository.FindAll(x => x.Id == item.Id).FirstOrDefault();
                        if (orig != null)
                        {
                            var orig_TMP = model.Data.Az_SubCommessaAttivita.Where(x => x.Id == item.Id && x.Checked == true).FirstOrDefault();
                            if (orig_TMP == null)
                            {
                                await _az_SubCommessaAttivitaRepository.DeleteAsync(orig);
                            }
                        }
                    }
                    //upsert
                    foreach (var item in model.Data.Az_SubCommessaAttivita.Where(x => x.Checked == true).ToList())
                    {
                        var orig = _az_SubCommessaAttivitaRepository.FindAll(x => x.IdAz_SubCommessa == item.IdAz_SubCommessa && x.IdPar_Attivita == item.IdPar_Attivita).FirstOrDefault();
                        if (orig == null)
                        {
                            orig = _mapper.Map<Az_SubCommessaAttivita>(item);
                            orig.IdAz_SubCommessa = model.Data.IdAz_SubCommessa;
                            orig.IdPar_Attivita = item.IdPar_Attivita;
                        }
                        else
                        {
                            orig = _mapper.Map<Az_SubCommessaAttivita>(item);
                        }
                        orig = await _az_SubCommessaAttivitaRepository.UpsertAsync(orig);
                    }
                }

                return retVal;
            }, isSubProcess);
        }
    
        public virtual async Task<GenericResult<Az_SubCommessaAttivita_GetAll_4FullList_OutModel>> GetAll_4FullList(GenericRequest<Az_SubCommessaAttivita_GetAll_4FullList_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessaAttivita_GetAll_4FullList_OutModel retVal = new Az_SubCommessaAttivita_GetAll_4FullList_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var idAzAnagrafica  = company_DATA.az_Anagrafica.Id;

                    var commesse        = await _az_CommessaRepository.FindAll();
                    commesse            = commesse.Where(c => c.IdAz_Anagrafica == idAzAnagrafica).ToList();

                    var commesseIds     = commesse.Select(c => c.Id).ToHashSet();

                    var subCommesse     = await _az_SubCommessaRepository.FindAll();
                    subCommesse         = subCommesse.Where(s => commesseIds.Contains(s.IdAz_Commessa)).ToList();

                    var subCommesseIds  = subCommesse.Select(s => s.Id).ToHashSet();

                    var attivitaLinks   = await _az_SubCommessaAttivitaRepository.FindAll();
                    attivitaLinks       = attivitaLinks.Where(a => subCommesseIds.Contains(a.IdAz_SubCommessa)).ToList();

                    var parAttivita     = await _par_AttivitaRepository.FindAll();
                    parAttivita         = parAttivita.Where(a => a.IdAz_Anagrafica == idAzAnagrafica).ToList();

                    var clienti         = await _az_ClienteRepository.FindAll();
                    clienti             = clienti.Where(c => c.IdAz_Anagrafica == idAzAnagrafica).ToList();

                    retVal.Az_SubCommessaAttivita = (
                        from link in attivitaLinks
                        join sub  in subCommesse  on link.IdAz_SubCommessa equals sub.Id
                        join com  in commesse     on sub.IdAz_Commessa     equals com.Id
                        join att  in parAttivita  on link.IdPar_Attivita   equals att.Id
                        join cli  in clienti      on com.IdAz_Cliente      equals cli.Id
                        select new Az_SubCommessaAttivita_4FullListModel
                        {
                            Commessa_Id              = com.Id,
                            Commessa_IdAz_Cliente    = com.IdAz_Cliente,
                            Commessa_Decrizione      = com.Descrizione,
                            Commessa_Default         = com.Default,

                            Cliente_Descrizione      = cli.Descrizione,
                            Cliente_Default          = cli.Default,

                            SubCommessa_Id           = sub.Id,
                            SubCommessa_Decrizione   = sub.Descrizione ?? string.Empty,
                            SubCommessa_Default      = sub.Default,

                            SubCommessaAttivita_Id              = link.Id,
                            SubCommessaAttivita_Decrizione      = att.Descrizione,
                            SubCommessaAttivita_Default         = link.Default,
                            SubCommessaAttivita_IdPar_Attivita  = link.IdPar_Attivita,
                            SubCommessaAttivita_Par_Attivita_Descrizione = att.Descrizione
                        }
                    )
                    .OrderBy(x => x.Cliente_Default  ? 0 : 1)
                    .ThenBy(x => x.Cliente_Descrizione)
                    .ThenBy(x => x.Commessa_Default  ? 0 : 1)
                    .ThenBy(x => x.Commessa_Decrizione)
                    .ThenBy(x => x.SubCommessa_Default ? 0 : 1)
                    .ThenBy(x => x.SubCommessa_Decrizione)
                    .ToList();
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        //public virtual async Task<GenericResult<Get_Az_SubCommessaAttivita_Default_OutModel>> Get_Az_SubCommessaAttivita_Default(GenericRequest<Get_Az_SubCommessaAttivita_Default_InModel> model, bool isSubProcess)
        //{
        //    return await ExecuteAction(model, async () =>
        //    {
        //        Get_Az_SubCommessaAttivita_Default_OutModel retVal = new Get_Az_SubCommessaAttivita_Default_OutModel();

        //        int IdCompany;
        //        int.TryParse(this.CurrentCompany, out IdCompany);

        //        Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
        //        if (company_DATA != null && company_DATA.az_Anagrafica != null)
        //        {
        //            int idAz = company_DATA.az_Anagrafica.Id;

        //            // ?? 1. Cliente default ???????????????????????????????????????????
        //            var cliente = (await _az_ClienteRepository.FindAll())
        //                .FirstOrDefault(c => c.IdAz_Anagrafica == idAz && c.Default);

        //            if (cliente == null)
        //            {
        //                cliente = await _az_ClienteRepository.UpsertAsync(new Az_Cliente
        //                {
        //                    IdAz_Anagrafica = idAz,
        //                    Descrizione     = "Default",
        //                    Default         = true
        //                });
        //            }

        //            // ?? 2. Commessa default ??????????????????????????????????????????
        //            var commessa = (await _az_CommessaRepository.FindAll())
        //                .FirstOrDefault(c => c.IdAz_Anagrafica == idAz && c.IdAz_Cliente == cliente.Id && c.Default);

        //            if (commessa == null)
        //            {
        //                commessa = await _az_CommessaRepository.UpsertAsync(new Az_Commessa
        //                {
        //                    IdAz_Anagrafica = idAz,
        //                    IdAz_Cliente    = cliente.Id,
        //                    Descrizione     = "Default",
        //                    Default         = true
        //                });
        //            }

        //            // ?? 3. SubCommessa default ???????????????????????????????????????
        //            var subCommessa = (await _az_SubCommessaRepository.FindAll())
        //                .FirstOrDefault(s => s.IdAz_Commessa == commessa.Id && s.Default);

        //            if (subCommessa == null)
        //            {
        //                subCommessa = await _az_SubCommessaRepository.UpsertAsync(new Az_SubCommessa
        //                {
        //                    IdAz_Commessa = commessa.Id,
        //                    Descrizione   = "Default",
        //                    Default       = true,
        //                    Data          = DateTime.Today,
        //                    DataA         = DateTime.Today.AddYears(10)
        //                });
        //            }

        //            // ?? 4. Par_Attivita default ??????????????????????????????????????
        //            var attivita = (await _par_AttivitaRepository.FindAll())
        //                .FirstOrDefault(a => a.IdAz_Anagrafica == idAz && a.Default);

        //            if (attivita == null)
        //            {
        //                attivita = await _par_AttivitaRepository.UpsertAsync(new Par_Attivita
        //                {
        //                    IdAz_Anagrafica = idAz,
        //                    Descrizione     = "Default",
        //                    Default         = true
        //                });
        //            }

        //            // ?? 5. Az_SubCommessaAttivita default ????????????????????????????
        //            var subCommessaAttivita = (await _az_SubCommessaAttivitaRepository.FindAll())
        //                .FirstOrDefault(a => a.IdAz_SubCommessa == subCommessa.Id && a.IdPar_Attivita == attivita.Id && a.Default);

        //            if (subCommessaAttivita == null)
        //            {
        //                subCommessaAttivita = await _az_SubCommessaAttivitaRepository.UpsertAsync(new Az_SubCommessaAttivita
        //                {
        //                    IdAz_SubCommessa = subCommessa.Id,
        //                    IdPar_Attivita   = attivita.Id,
        //                    Default          = true
        //                });
        //            }

        //            retVal.Az_SubCommessaAttivita = _mapper.Map<Az_SubCommessaAttivitaModel>(subCommessaAttivita);
        //        }

        //        await Task.Delay(DelayAsyncMethod);
        //        return retVal;
        //    }, isSubProcess);
        //}

    }

    public interface IAz_SubCommessaAttivitaService : IServiceBase
    {
        Task<GenericResult<Az_SubCommessaAttivita_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaAttivita_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_SubCommessaAttivita_Get4SubCommessa_OutModel>> Get4SubCommessa(GenericRequest<Az_SubCommessaAttivita_Get4SubCommessa_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_SubCommessaAttivita_Put4SubCommessa_OutModel>> Put4SubCommessa(GenericRequest<Az_SubCommessaAttivita_Put4SubCommessa_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_SubCommessaAttivita_GetAll_4FullList_OutModel>> GetAll_4FullList(GenericRequest<Az_SubCommessaAttivita_GetAll_4FullList_InModel> model, bool isSubProcess);

        //Task<GenericResult<Get_Az_SubCommessaAttivita_Default_OutModel>> Get_Az_SubCommessaAttivita_Default(GenericRequest<Get_Az_SubCommessaAttivita_Default_InModel> model, bool isSubProcess);
    }
}
