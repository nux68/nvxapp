using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.Base;
using nvxapp.server.service.Interfaces;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Microsoft.Extensions.Options;
using nvxapp.server.service.ServerModels;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using System.Security.Cryptography.Xml;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService
{

    public class Par_GiustificativiService : ServiceBase, IPar_GiustificativiService
    {
        private readonly IPar_GiustificativiRepository _par_GiustificativiRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Par_GiustificativiService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IPar_GiustificativiRepository par_GiustificativiRepository,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility
            ) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_GiustificativiRepository = par_GiustificativiRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Par_GiustificativiOutModel>> GetAll(GenericRequest<Par_GiustificativiInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_GiustificativiOutModel retVal = new Par_GiustificativiOutModel();


                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto(IdCompany,true);

                Par_Giustificativi? par_Giustificativi=null;
                if (company_DATA_COMB_AzAna_AzSedi_AzReparto !=null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica!= null)
                {
                    //MA
                    par_Giustificativi = _par_GiustificativiRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id &&
                                                                               x.Codice =="MA").FirstOrDefault();
                    if( par_Giustificativi==null)
                    {
                        par_Giustificativi = new Par_Giustificativi() { 
                             IdAz_Anagrafica= company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id,
                             Codice="MA",
                             Descrizione="Malattia"
                        };
                        par_Giustificativi = await _par_GiustificativiRepository.UpsertAsync(par_Giustificativi);
                    }
                    retVal.Par_Giustificativi.Add(_mapper.Map<Par_GiustificativiModel>(par_Giustificativi));

                    //ROL
                    par_Giustificativi = _par_GiustificativiRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id &&
                                                                               x.Codice == "ROL").FirstOrDefault();
                    if (par_Giustificativi == null)
                    {
                        par_Giustificativi = new Par_Giustificativi()
                        {
                            IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id,
                            Codice = "ROL",
                            Descrizione = "ROL"
                        };
                        par_Giustificativi = await _par_GiustificativiRepository.UpsertAsync(par_Giustificativi);
                    }
                    retVal.Par_Giustificativi.Add(_mapper.Map<Par_GiustificativiModel>(par_Giustificativi));

                    //FERIE
                    par_Giustificativi = _par_GiustificativiRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id &&
                                                                               x.Codice == "FE").FirstOrDefault();
                    if (par_Giustificativi == null)
                    {
                        par_Giustificativi = new Par_Giustificativi()
                        {
                            IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id,
                            Codice = "FE",
                            Descrizione = "Ferie"
                        };
                        par_Giustificativi = await _par_GiustificativiRepository.UpsertAsync(par_Giustificativi);
                    }
                    retVal.Par_Giustificativi.Add(_mapper.Map<Par_GiustificativiModel>(par_Giustificativi));
                }

                


                //retVal.Par_Giustificativi.Add( new Par_GiustificativiModel()
                //{
                //       Id=1,
                //       IdAz_Anagrafica =1,
                //       Descrizione ="Ferie",
                //       Codice ="FE"
                //}  );

                //retVal.Par_Giustificativi.Add(new Par_GiustificativiModel()
                //{
                //    Id = 2,
                //    IdAz_Anagrafica = 1,
                //    Descrizione = "ROL",
                //    Codice = "ROL"
                //});

                //retVal.Par_Giustificativi.Add(new Par_GiustificativiModel()
                //{
                //    Id = 3,
                //    IdAz_Anagrafica = 1,
                //    Descrizione = "Malattia",
                //    Codice = "MA"
                //});


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IPar_GiustificativiService : IServiceBase
    {
        public Task<GenericResult<Par_GiustificativiOutModel>> GetAll( GenericRequest<Par_GiustificativiInModel> model, Boolean isSubProcess);
    }
}
