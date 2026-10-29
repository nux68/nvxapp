using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Extensions;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService
{

    public class Par_GiustificativiService : ServiceBase, IPar_GiustificativiService
    {
        private readonly IPar_GiustificativiRepository _par_GiustificativiRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IPar_CausaliRepository _par_CausaliRepository;
        private readonly IDip_GG_GiustificativiRepository _dip_GG_GiustificativiRepository;
        private readonly IDip_Rapporto_Giustificativi_MaturazioneRepository _maturazioneRepository;

        public Par_GiustificativiService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IPar_GiustificativiRepository par_GiustificativiRepository,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IPar_CausaliRepository par_CausaliRepository,
                                  IDip_GG_GiustificativiRepository dip_GG_GiustificativiRepository,
                                  IDip_Rapporto_Giustificativi_MaturazioneRepository maturazioneRepository
            ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_GiustificativiRepository = par_GiustificativiRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _par_CausaliRepository = par_CausaliRepository;
            _dip_GG_GiustificativiRepository = dip_GG_GiustificativiRepository;
            _maturazioneRepository = maturazioneRepository;
        }

        public virtual async Task<GenericResult<Par_GiustificativiOutModel>> GetAll(GenericRequest<Par_GiustificativiInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_GiustificativiOutModel retVal = new Par_GiustificativiOutModel();


                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);

                Par_Giustificativi? par_Giustificativi = null;
                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica != null)
                {
                    var par_Giustificativi_all = _par_GiustificativiRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id ).ToList();


                    if(!par_Giustificativi_all.Any())
                    {
                        //MA
                    par_Giustificativi  = par_Giustificativi_all.Where(x=> x.Codice=="MAL").FirstOrDefault();
                    if (par_Giustificativi == null)
                    {
                        par_Giustificativi = new Par_Giustificativi()
                        {
                            IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id,
                            Codice = "MAL",
                            Descrizione = "Malattia",
                            BackgroundColor = "#ff0000",
                            TextColor = "#ffffff"
                        };
                        par_Giustificativi = await _par_GiustificativiRepository.UpsertAsync(par_Giustificativi);
                        par_Giustificativi_all.Add(par_Giustificativi);
                    }

                    //ROL
                    par_Giustificativi  = par_Giustificativi_all.Where(x=> x.Codice=="ROL").FirstOrDefault();
                    if (par_Giustificativi == null)
                    {
                        par_Giustificativi = new Par_Giustificativi()
                        {
                            IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id,
                            Codice = "ROL",
                            Descrizione = "ROL",
                            BackgroundColor = "#7fff00",
                            TextColor = "#000000"
                        };
                        par_Giustificativi = await _par_GiustificativiRepository.UpsertAsync(par_Giustificativi);
                        par_Giustificativi_all.Add(par_Giustificativi);
                    }

                    //FERIE
                    par_Giustificativi  = par_Giustificativi_all.Where(x=> x.Codice=="FE").FirstOrDefault();
                    if (par_Giustificativi == null)
                    {
                        par_Giustificativi = new Par_Giustificativi()
                        {
                            IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id,
                            Codice = "FE",
                            Descrizione = "Ferie",
                            BackgroundColor = "#ff8c00",
                            TextColor = "#ffffff"
                        };
                        par_Giustificativi = await _par_GiustificativiRepository.UpsertAsync(par_Giustificativi);
                        par_Giustificativi_all.Add(par_Giustificativi);
                    }
                    }

                    

                    retVal.Par_Giustificativi = _mapper.Map<List<Par_GiustificativiModel>>(par_Giustificativi_all);


                }



                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_GiustificativiGetOutModel>> Par_GiustificativiGet(GenericRequest<Par_GiustificativiGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {

                Par_GiustificativiGetOutModel retVal = new Par_GiustificativiGetOutModel();

                var par_Giustificativi = await _par_GiustificativiRepository.FindByIdAsync(model.Data.Id);
                if (par_Giustificativi != null)
                {
                    retVal.Par_Giustificativi = _mapper.Map<Par_GiustificativiModel>(par_Giustificativi);
                }
                else
                {
                    retVal.Par_Giustificativi = new Par_GiustificativiModel() { };
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_GiustificativiPutOutModel>> Par_GiustificativiPut(GenericRequest<Par_GiustificativiPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_GiustificativiPutOutModel retVal = new Par_GiustificativiPutOutModel();
                retVal.Par_Giustificativi = model.Data.Par_Giustificativi;

                
                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);

                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica != null)
                {
                    ControllaTipo(model.Data.Par_Giustificativi);

                    Par_Giustificativi? par_Giustificativi = await _par_GiustificativiRepository.FindByIdAsync(model.Data.Par_Giustificativi.Id);
                    if (par_Giustificativi != null && par_Giustificativi.Tipo != model.Data.Par_Giustificativi.Tipo && GiustificativoUsato(par_Giustificativi.Id))
                        throw new UserMessageException(AttendanceTrackingMessages.Get("ParGiustificativoTipoNonModificabile", par_Giustificativi.Codice));

                    if (par_Giustificativi == null)
                    {
                        par_Giustificativi = _mapper.Map<Par_Giustificativi>(model.Data.Par_Giustificativi);
                        par_Giustificativi.IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id;
                    }
                    else
                    {
                        //update 
                        par_Giustificativi = _mapper.Map<Par_Giustificativi>(model.Data.Par_Giustificativi);
                    }

                    //aggiurna il valore ritornato al client
                    par_Giustificativi = await _par_GiustificativiRepository.UpsertAsync(par_Giustificativi);
                    retVal.Par_Giustificativi = _mapper.Map<Par_GiustificativiModel>(par_Giustificativi);
                }
                

                

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
    
        /// <summary>
        /// Giustificativo a importo: si inserisce solo un importo, quindi niente effetti sulle ore (segno neutro),
        /// niente contatore, banca ore, autorizzazione allo straordinario e piano ferie. La causale collegata
        /// deve essere dello stesso tipo del giustificativo (le ore vanno su causali orarie, gli importi su causali a importo).
        /// </summary>
        private void ControllaTipo(Par_GiustificativiModel giust)
        {
            if (giust.Tipo == TipoValore.Importo)
            {
                giust.Segno = SignWithNeutral.Neutral;
                giust.TipoContatore = TipoContatore.NoContatore;
                giust.BancaOre = false;
                giust.AutorizzaStraordinario = false;
                giust.VisualizzaInPianoFerie = false;
            }

            if (giust.IdCausale is int idCausale && idCausale > 0)
            {
                var causale = _par_CausaliRepository.FindAll(x => x.Id == idCausale).FirstOrDefault();
                if (causale != null && causale.Tipo != giust.Tipo)
                    throw new UserMessageException(AttendanceTrackingMessages.Get(giust.Tipo == TipoValore.Importo
                        ? "ParGiustificativoCausaleNonImporto" : "ParGiustificativoCausaleNonOraria", causale.Codice));
            }
        }

        /// <summary>giustificativo gia' registrato nei giorni dei dipendenti o con una maturazione configurata</summary>
        private bool GiustificativoUsato(int id) =>
            _dip_GG_GiustificativiRepository.FindAll(x => x.IdPar_Giustificativi == id).Any()
            || _maturazioneRepository.FindAll(x => x.IdPar_Giustificativi == id).Any();

        public virtual async Task<GenericResult<Par_Giustificativi_DeleteOutModel>> Par_GiustificativiDelete(GenericRequest<Par_Giustificativi_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var entity = await _par_GiustificativiRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                {
                    await _par_GiustificativiRepository.DeleteAsync(entity);
                }
                return new Par_Giustificativi_DeleteOutModel();
            }, isSubProcess);
        }
        
    }

    public interface IPar_GiustificativiService : IServiceBase
    {
        public Task<GenericResult<Par_GiustificativiOutModel>> GetAll(GenericRequest<Par_GiustificativiInModel> model, Boolean isSubProcess);
        public Task<GenericResult<Par_GiustificativiGetOutModel>> Par_GiustificativiGet(GenericRequest<Par_GiustificativiGetInModel> model, Boolean isSubProcess);
        public Task<GenericResult<Par_GiustificativiPutOutModel>> Par_GiustificativiPut(GenericRequest<Par_GiustificativiPutInModel> model, Boolean isSubProcess);
        public Task<GenericResult<Par_Giustificativi_DeleteOutModel>> Par_GiustificativiDelete(GenericRequest<Par_Giustificativi_DeleteInModel> model, bool isSubProcess);
    }
}
