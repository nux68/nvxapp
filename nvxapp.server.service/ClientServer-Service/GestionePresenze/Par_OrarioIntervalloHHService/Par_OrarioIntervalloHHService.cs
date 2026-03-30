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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;


namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService
{

    public class Par_OrarioIntervalloHHService : ServiceBase, IPar_OrarioIntervalloHHService
    {
        private readonly IPar_OrarioRepository _par_OrarioRepository;
        private readonly IPar_OrarioIntervalloHHRepository _par_OrarioIntervalloHHRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IPar_CausaliRepository _par_CausaliRepository;

        public Par_OrarioIntervalloHHService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IPar_CausaliRepository par_CausaliRepository,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IPar_OrarioRepository par_OrarioRepository,
                                  IPar_OrarioIntervalloHHRepository par_OrarioIntervalloHHRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _par_OrarioIntervalloHHRepository = par_OrarioIntervalloHHRepository;
            _par_OrarioRepository = par_OrarioRepository;
            _par_CausaliRepository = par_CausaliRepository;
        }

        public virtual async Task<GenericResult<Par_OrarioIntervalloHHOutModel>> GetAll(GenericRequest<Par_OrarioIntervalloHHInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_OrarioIntervalloHHOutModel retVal = new Par_OrarioIntervalloHHOutModel();


                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var par_OrarioIntervalloHH = _par_OrarioIntervalloHHRepository.FindAll( x=> x.Id>0);
                    retVal.Par_OrarioIntervalloHH = _mapper.Map<List<Par_OrarioIntervalloHHModel>>(par_OrarioIntervalloHH);
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Par_OrarioIntervalloHH_Get_4Edit_OutModel>> Par_OrarioIntervalloHH_Get(GenericRequest<Par_OrarioIntervalloHH_Get_4Edit_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_OrarioIntervalloHH_Get_4Edit_OutModel retVal = new Par_OrarioIntervalloHH_Get_4Edit_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    int NumeroCoppie = 1;
                    var par_Orario = _par_OrarioRepository.FindById(model.Data.Id);
                    if (par_Orario != null)
                        NumeroCoppie = par_Orario.NumeroCoppie;

                    var par_OrarioIntervalloHH = _par_OrarioIntervalloHHRepository.FindAll(x => x.IdPar_Orario == model.Data.Id).ToList();
                    retVal.Par_OrarioIntervalloHH = _mapper.Map<List<Par_OrarioIntervalloHHModel>>(par_OrarioIntervalloHH);

                    int tmpCounter = retVal.Par_OrarioIntervalloHH.Any(g => g.Id < 0) ? retVal.Par_OrarioIntervalloHH.Where(g => g.Id < 0).Min(g => g.Id) : 0;
                    for (var i = 1; i <= NumeroCoppie; i++)
                    {
                        if (!retVal.Par_OrarioIntervalloHH.Any(x => x.NumCoppia == i))
                        {
                            var cop = init_Par_OrarioIntervalloHH(i, company_DATA.az_Anagrafica.Id);
                            cop.IdPar_Orario = model.Data.Id;
                            cop.Id = --tmpCounter;
                            retVal.Par_OrarioIntervalloHH.Add(cop);
                        }
                    }
                    retVal.Par_OrarioIntervalloHH = retVal.Par_OrarioIntervalloHH.OrderBy(x => x.NumCoppia).ToList();
                    
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Par_OrarioIntervalloHH_Put_4Edit_OutModel>> Par_OrarioIntervalloHH_Put(GenericRequest<Par_OrarioIntervalloHH_Put_4Edit_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_OrarioIntervalloHH_Put_4Edit_OutModel retVal = new Par_OrarioIntervalloHH_Put_4Edit_OutModel();


                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var par_Orario = _par_OrarioRepository.FindById(model.Data.Id);
                    if (par_Orario != null)
                    {
                        retVal.Id = model.Data.Id;

                        //elimino gli id <0
                        model.Data.Par_OrarioIntervalloHH.Where(x => x.Id < 0).ToList().ForEach(x => x.Id = 0);

                        var par_OrarioIntervalloHH_All = _par_OrarioIntervalloHHRepository.FindAll(x => x.IdPar_Orario == model.Data.Id).ToList();
                        //cancellazione sub commesse eliminate
                        foreach (var item in par_OrarioIntervalloHH_All)
                        {
                            //cerco il record nei dati tornati dal client
                            var orig_TMP = model.Data.Par_OrarioIntervalloHH.Where(x => x.Id == item.Id).FirstOrDefault();
                            //se non trovo la corrispondenza nei dati del client, vul dire che è stata eliminata
                            if (orig_TMP == null)
                            {
                                //procedo alla cancelazione
                                await _par_OrarioIntervalloHHRepository.DeleteAsync(item);
                            }
                        }

                        foreach (var item in model.Data.Par_OrarioIntervalloHH)
                        {
                            var par_OrarioIntervalloHH = _mapper.Map<Par_OrarioIntervalloHH>(item);
                            par_OrarioIntervalloHH.IdPar_Orario = model.Data.Id;
                            await _par_OrarioIntervalloHHRepository.UpsertAsync(par_OrarioIntervalloHH);
                        }

                        //rireggo gli i dati
                        var req_1 = new GenericRequest<Par_OrarioIntervalloHH_Get_4Edit_InModel>();
                        req_1.Data = new Par_OrarioIntervalloHH_Get_4Edit_InModel() { Id = model.Data.Id };
                        var res_1 = await Par_OrarioIntervalloHH_Get(req_1, true);
                        if (res_1.Success && res_1.Data != null)
                        {
                            retVal.Par_OrarioIntervalloHH = res_1.Data.Par_OrarioIntervalloHH;
                        }

                    }
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Par_OrarioIntervalloHH_Arrange_Coppie_OutModel>> Par_OrarioIntervalloHH_Arrange_NumCoppie(GenericRequest<Par_OrarioIntervalloHH_Arrange_Coppie_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_OrarioIntervalloHH_Arrange_Coppie_OutModel retVal = new Par_OrarioIntervalloHH_Arrange_Coppie_OutModel();


                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    retVal.Par_OrarioIntervalloHH = model.Data.Par_OrarioIntervalloHH;

                    int tmpCounter = retVal.Par_OrarioIntervalloHH.Any(g => g.Id < 0) ? retVal.Par_OrarioIntervalloHH.Where(g => g.Id < 0).Min(g => g.Id) : 0;

                    if (model.Data.NumCoppie > model.Data.Par_OrarioIntervalloHH.Count)
                    {
                        for (var i = model.Data.Par_OrarioIntervalloHH.Count; i < model.Data.NumCoppie; i++)
                        {
                            var cop = init_Par_OrarioIntervalloHH(i+1, company_DATA.az_Anagrafica.Id);
                            cop.IdPar_Orario = model.Data.Id;
                            cop.Id = --tmpCounter;
                            retVal.Par_OrarioIntervalloHH.Add(cop);
                        }
                    }
                    else if (model.Data.NumCoppie < model.Data.Par_OrarioIntervalloHH.Count)
                    {
                        retVal.Par_OrarioIntervalloHH = retVal.Par_OrarioIntervalloHH.Where(x => x.NumCoppia <= model.Data.NumCoppie).ToList();
                    }



                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }


        private Par_OrarioIntervalloHHModel init_Par_OrarioIntervalloHH(int numCoppia,int IdAz_Anagrafica)
        {
            Par_OrarioIntervalloHHModel _par_OrarioIntervalloHH = new Par_OrarioIntervalloHHModel();

            _par_OrarioIntervalloHH.NumCoppia = numCoppia;
            var par_causali = _par_CausaliRepository.FindAll(x => x.IdAz_Anagrafica == IdAz_Anagrafica).FirstOrDefault();

            _par_OrarioIntervalloHH.IdCausale_HH_Lav = par_causali != null ? par_causali.Id : 0; //MIGLIORARE

            switch (numCoppia)
            {
                case 1:
                    _par_OrarioIntervalloHH.Dalle_Limite_SX = new TimeOnly(8,45,0);  
                    _par_OrarioIntervalloHH.Dalle = new TimeOnly(9,0,0);
                    _par_OrarioIntervalloHH.Dalle_Limite_DX = new TimeOnly(9,15,0);
                    _par_OrarioIntervalloHH.Dalle_Arrotondamento = data.Extensions.TimeRoundInterval.Min15;
                    _par_OrarioIntervalloHH.Dalle_Arrotondamento_Verso = data.Extensions.RoundDirection.Up;

                    _par_OrarioIntervalloHH.Alle_Limite_SX =  new TimeOnly(12,45,0);
                    _par_OrarioIntervalloHH.Alle = new TimeOnly(13,0,0);
                    _par_OrarioIntervalloHH.Alle_Limite_DX = new TimeOnly(13,15,0);
                    _par_OrarioIntervalloHH.Alle_Arrotondamento = data.Extensions.TimeRoundInterval.Min15;
                    _par_OrarioIntervalloHH.Alle_Arrotondamento_Verso = data.Extensions.RoundDirection.Up;
                    

                    break;

                case 2:
                    _par_OrarioIntervalloHH.Dalle_Limite_SX = new TimeOnly(13,45,0);
                    _par_OrarioIntervalloHH.Dalle = new TimeOnly(14,0,0);
                    _par_OrarioIntervalloHH.Dalle_Limite_DX = new TimeOnly(14,150,0);
                    _par_OrarioIntervalloHH.Dalle_Arrotondamento = data.Extensions.TimeRoundInterval.Min15;
                    _par_OrarioIntervalloHH.Dalle_Arrotondamento_Verso = data.Extensions.RoundDirection.Up;

                    _par_OrarioIntervalloHH.Alle_Limite_SX = new TimeOnly(17,45,0);
                    _par_OrarioIntervalloHH.Alle = new TimeOnly(18,0,0);
                    _par_OrarioIntervalloHH.Alle_Limite_DX = new TimeOnly(18,15,0);
                    _par_OrarioIntervalloHH.Alle_Arrotondamento = data.Extensions.TimeRoundInterval.Min15;
                    _par_OrarioIntervalloHH.Alle_Arrotondamento_Verso = data.Extensions.RoundDirection.Up;
                    break;

                case 3:
                    _par_OrarioIntervalloHH.Dalle_Limite_SX = new TimeOnly(19,50,0);
                    _par_OrarioIntervalloHH.Dalle = new TimeOnly(20,0,0);
                    _par_OrarioIntervalloHH.Dalle_Limite_DX = new TimeOnly(20,10,0);
                    _par_OrarioIntervalloHH.Dalle_Arrotondamento = data.Extensions.TimeRoundInterval.Min5;
                    _par_OrarioIntervalloHH.Dalle_Arrotondamento_Verso = data.Extensions.RoundDirection.Up;

                    _par_OrarioIntervalloHH.Alle_Limite_SX = new TimeOnly(21,0,0);
                    _par_OrarioIntervalloHH.Alle = new TimeOnly(21,0,0);
                    _par_OrarioIntervalloHH.Alle_Limite_DX = new TimeOnly(21,10,0);
                    _par_OrarioIntervalloHH.Alle_Arrotondamento = data.Extensions.TimeRoundInterval.Min5;
                    _par_OrarioIntervalloHH.Alle_Arrotondamento_Verso = data.Extensions.RoundDirection.Up;
                    break;

                case 4:
                    _par_OrarioIntervalloHH.Dalle_Limite_SX = new TimeOnly(21,50,0);
                    _par_OrarioIntervalloHH.Dalle = new TimeOnly(22,0,0);
                    _par_OrarioIntervalloHH.Dalle_Limite_DX = new TimeOnly(22,10,0);
                    _par_OrarioIntervalloHH.Dalle_Arrotondamento = data.Extensions.TimeRoundInterval.Min5;
                    _par_OrarioIntervalloHH.Dalle_Arrotondamento_Verso = data.Extensions.RoundDirection.Up;

                    _par_OrarioIntervalloHH.Alle_Limite_SX = new TimeOnly(23,0,0);
                    _par_OrarioIntervalloHH.Alle = new TimeOnly(23,0,0);
                    _par_OrarioIntervalloHH.Alle_Limite_DX = new TimeOnly(23,10,0);
                    _par_OrarioIntervalloHH.Alle_Arrotondamento = data.Extensions.TimeRoundInterval.Min5;
                    _par_OrarioIntervalloHH.Alle_Arrotondamento_Verso = data.Extensions.RoundDirection.Up;
                    break;
            }

            return _par_OrarioIntervalloHH;
        }

    }

    public interface IPar_OrarioIntervalloHHService : IServiceBase
    {
        public Task<GenericResult<Par_OrarioIntervalloHHOutModel>> GetAll(GenericRequest<Par_OrarioIntervalloHHInModel> model, Boolean isSubProcess);
        Task<GenericResult<Par_OrarioIntervalloHH_Get_4Edit_OutModel>> Par_OrarioIntervalloHH_Get(GenericRequest<Par_OrarioIntervalloHH_Get_4Edit_InModel> model, bool isSubProcess);
        Task<GenericResult<Par_OrarioIntervalloHH_Put_4Edit_OutModel>> Par_OrarioIntervalloHH_Put(GenericRequest<Par_OrarioIntervalloHH_Put_4Edit_InModel> model, bool isSubProcess);
        Task<GenericResult<Par_OrarioIntervalloHH_Arrange_Coppie_OutModel>> Par_OrarioIntervalloHH_Arrange_NumCoppie(GenericRequest<Par_OrarioIntervalloHH_Arrange_Coppie_InModel> model, bool isSubProcess);

    }
}
