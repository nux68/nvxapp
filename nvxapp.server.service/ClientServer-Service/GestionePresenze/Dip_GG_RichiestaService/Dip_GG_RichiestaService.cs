using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;



namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService
{

    public class Dip_GG_RichiestaService : ServiceBase, IDip_GG_RichiestaService
    {
        private readonly IAccountService _accountService;

        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IDip_GG_RichiestaRepository _dip_GG_RichiestaRepository;
        private readonly IDip_GG_TimbraturaRepository _dip_GG_TimbraturaRepository;
        private readonly IDip_GG_GiustificativiRepository _dip_GG_GiustificativiRepository;
        private readonly IDip_GG_NotaSpesaRepository _dip_GG_NotaSpesaRepository;

        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;

        public Dip_GG_RichiestaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAccountService accountService,
                                  IDip_AnagraficaRepository dip_AnagraficaRepository,
                                  IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IDip_GG_RichiestaRepository dip_GG_RichiestaRepository,
                                  IDip_GG_TimbraturaRepository dip_GG_TimbraturaRepository,
                                  IDip_GG_NotaSpesaRepository dip_GG_NotaSpesaRepository,
                                  IDip_GG_GiustificativiRepository dip_GG_GiustificativiRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _accountService = accountService;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _dip_GG_RichiestaRepository = dip_GG_RichiestaRepository;
            _dip_GG_TimbraturaRepository = dip_GG_TimbraturaRepository;
            _dip_GG_GiustificativiRepository = dip_GG_GiustificativiRepository;
            _dip_GG_NotaSpesaRepository = dip_GG_NotaSpesaRepository;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;

        }

        public virtual async Task<GenericResult<Dip_GG_Richiesta_GetAll4User_OutModel>> GetAll4User(GenericRequest<Dip_GG_Richiesta_GetAll4User_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Richiesta_GetAll4User_OutModel retVal = new Dip_GG_Richiesta_GetAll4User_OutModel();

                //TODO gestire
                User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(this.CurrentUserId, true);

                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {
                    List<int> idDip_RapportoLavoro = new List<int>();
                    idDip_RapportoLavoro.Add(user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id);

                    List<Dip_GG_Richiesta> richiesta = Get_Dip_GG_Richiesta(idDip_RapportoLavoro, model.Data.Year, model.Data.Month);
                    richiesta = richiesta.OrderBy(x => x.Data).ToList();

                    retVal.Dip_GG_Richiesta = _mapper.Map<List<Dip_GG_RichiestaModel>>(richiesta);
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Dip_GG_Richiesta_GetAll4Admin_OutModel>> GetAll4Admin(GenericRequest<Dip_GG_Richiesta_GetAll4Admin_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Richiesta_GetAll4Admin_OutModel retVal = new Dip_GG_Richiesta_GetAll4Admin_OutModel();

                GenericRequest<UserCompanyListInModel> req = new GenericRequest<UserCompanyListInModel>();
                var res = await _accountService.UserCompanyList(req,true);
                if(res.Success && res.Data != null)
                {
                    var UserCompanyList = res.Data.UserCompanyList;
                    List<string?> idAspNetUsers = UserCompanyList.Select(x => x.IdAspNetUsers).ToList();
                    List<int> idDip_Anagrafica = _dip_AnagraficaRepository.FindAll(x=> idAspNetUsers.Contains(x.IdAspNetUsers)).Select(x=> x.Id).ToList();
                    List<int> idDip_RapportoLavoro = _dip_RapportoLavoroRepository.FindAll(x => idDip_Anagrafica.Contains(x.IdDip_Anagrafica)).Select(x => x.Id).ToList();

                    List<Dip_GG_Richiesta> richiesta = Get_Dip_GG_Richiesta(idDip_RapportoLavoro, model.Data.Year, model.Data.Month);
                    richiesta = richiesta.OrderBy(x => x.Data).ToList();

                    retVal.Dip_GG_Richiesta = _mapper.Map<List<Dip_GG_RichiestaModel>>(richiesta);
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Dip_GG_Richiesta_Send_OutModel>> Send(GenericRequest<Dip_GG_Richiesta_Send_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Richiesta_Send_OutModel retVal = new Dip_GG_Richiesta_Send_OutModel();


                User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(this.CurrentUserId, true);

                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {
                    Dip_GG_Richiesta dip_GG_Richiesta = _mapper.Map<Dip_GG_Richiesta>(model.Data.Dip_GG_Richiesta);
                    dip_GG_Richiesta.IdDip_RapportoLavoro = user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id;
                    dip_GG_Richiesta = await _dip_GG_RichiestaRepository.UpsertAsync(dip_GG_Richiesta);

                    switch (dip_GG_Richiesta.RichiestaTipo)
                    {
                        case TipoRichiesta.Timbratura:
                            await Add_Dip_GG_Timbratura(dip_GG_Richiesta, user_DATA_COMB_DipAna_DipRapp);
                            break;

                        case TipoRichiesta.Giustificativo:
                            await Add_Dip_GG_Giustificativi(dip_GG_Richiesta, user_DATA_COMB_DipAna_DipRapp);

                            break;
                        case TipoRichiesta.NotaSpesa:
                            break;
                    }

                }



                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_GG_Richiesta_SetState_OutModel>> SetState(GenericRequest<Dip_GG_Richiesta_SetState_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Richiesta_SetState_OutModel retVal = new Dip_GG_Richiesta_SetState_OutModel();

               
                var richieste = _dip_GG_RichiestaRepository.FindAll(x => model.Data.IdDip_GG_Richiesta.Contains(x.Id)).ToList();
                foreach(var item in richieste)
                {
                    item.RichiestaStato = model.Data.RichiestaStato;
                    await _dip_GG_RichiestaRepository.UpdateAsync(item);

                    switch (model.Data.RichiestaStato)
                    {
                        case StatoRichiesta.Cancellata:
                        case StatoRichiesta.Rifiutata:

                          
                            switch ( item.RichiestaTipo)
                            {
                                case TipoRichiesta.Timbratura:
                                    var timbr = _dip_GG_TimbraturaRepository.FindAll(x => x.IdDip_GG_Richiesta == item.Id).ToList();
                                    await _dip_GG_TimbraturaRepository.DeleteRangeAsync(timbr);

                                    break;
                                case TipoRichiesta.Giustificativo:

                                    var just = _dip_GG_GiustificativiRepository.FindAll(x => x.IdDip_GG_Richiesta == item.Id).ToList();
                                    await _dip_GG_GiustificativiRepository.DeleteRangeAsync(just);

                                    break;
                                case TipoRichiesta.NotaSpesa:

                                    var nota = _dip_GG_NotaSpesaRepository.FindAll(x => x.IdDip_GG_Richiesta == item.Id).ToList();
                                    await _dip_GG_NotaSpesaRepository.DeleteRangeAsync(nota);

                                    break;

                            }
                            break;

                        default:
                            switch (item.RichiestaTipo)
                            {
                                case TipoRichiesta.Timbratura:
                                    var timbr = _dip_GG_TimbraturaRepository.FindAll(x => x.IdDip_GG_Richiesta == item.Id).ToList();
                                    foreach(var idemDett in timbr)
                                    {
                                        idemDett.RichiestaStato = model.Data.RichiestaStato;
                                        await _dip_GG_TimbraturaRepository.UpsertAsync(idemDett);
                                    }


                                    break;
                                case TipoRichiesta.Giustificativo:

                                    var just = _dip_GG_GiustificativiRepository.FindAll(x => x.IdDip_GG_Richiesta == item.Id).ToList();
                                    foreach (var idemDett in just)
                                    {
                                        idemDett.RichiestaStato = model.Data.RichiestaStato;
                                        await _dip_GG_GiustificativiRepository.UpsertAsync(idemDett);
                                    }

                                    break;
                                case TipoRichiesta.NotaSpesa:

                                    var nota = _dip_GG_NotaSpesaRepository.FindAll(x => x.IdDip_GG_Richiesta == item.Id).ToList();
                                    foreach (var idemDett in nota)
                                    {
                                        idemDett.RichiestaStato = model.Data.RichiestaStato;
                                        await _dip_GG_NotaSpesaRepository.UpsertAsync(idemDett);
                                    }

                                    break;

                            }

                            break;
                    }
                }


             


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        private async Task Add_Dip_GG_Timbratura(Dip_GG_Richiesta dip_GG_Richiesta,
                                                 User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp)
        {
            if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
            {

                if (dip_GG_Richiesta.Dati != null)
                {
                    Dip_GG_Richiesta_Body_Timbratura? richiesta = JsonConvert.DeserializeObject<Dip_GG_Richiesta_Body_Timbratura>(dip_GG_Richiesta.Dati);
                    if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                    {
                        if (richiesta != null)
                        {
                            string fullDateTime = $"{dip_GG_Richiesta.Data.ToString("dd/MM/yyyy")} {richiesta.hhmm}";

                            // Fai il parsing della stringa completa
                            DateTime parsedDateTime = DateTime.ParseExact(fullDateTime, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                            Dip_GG_Timbratura dip_GG_Timbratura = new Dip_GG_Timbratura()
                            {
                                IdDip_RapportoLavoro = user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id,
                                IdDip_GG_Richiesta = dip_GG_Richiesta.Id,
                                Timbratura = parsedDateTime,
                                TimbraturaOriginale = parsedDateTime,
                                TimbraturaArrotondata = parsedDateTime,
                                GiornoCompetenza = new DateTime(parsedDateTime.Year, parsedDateTime.Month, parsedDateTime.Day),
                                TimbraturaTipo = TipoTimbratura.SenzaVerso,
                                RichiestaStato = StatoRichiesta.Immessa,
                            };
                            await _dip_GG_TimbraturaRepository.UpsertAsync(dip_GG_Timbratura);
                        }
                    }
                }
            }
        }
        private async Task Add_Dip_GG_Giustificativi(Dip_GG_Richiesta dip_GG_Richiesta,
                                                     User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp)
        {
            if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
            {
                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {

                    if (dip_GG_Richiesta.Dati != null)
                    {
                        Dip_GG_Richiesta_Body_Giustificativo? richiesta = JsonConvert.DeserializeObject<Dip_GG_Richiesta_Body_Giustificativo>(dip_GG_Richiesta.Dati);
                        if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                        {
                            if (richiesta != null)
                            {

                                for (DateTime date = dip_GG_Richiesta.Data; date <= dip_GG_Richiesta.DataA; date = date.AddDays(1))
                                {
                                    string fullDateTime = $"{dip_GG_Richiesta.Data.ToString("dd/MM/yyyy")} {richiesta.hhmm}";

                                    Dip_GG_Giustificativi dip_GG_Giustificativi = new Dip_GG_Giustificativi()
                                    {
                                        Data = date,
                                        IdDip_RapportoLavoro = user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id,
                                        IdDip_GG_Richiesta = dip_GG_Richiesta.Id,
                                        IdPar_Giustificativi = richiesta.IdPar_Giustificativi,
                                        RichiestaStato = StatoRichiesta.Immessa,
                                        InputType = richiesta.AllDay ? JustificationInputType.AllDay : JustificationInputType.Manual,
                                        Hours = TimeSpan.Parse(richiesta.hhmm)
                                    };
                                    await _dip_GG_GiustificativiRepository.UpsertAsync(dip_GG_Giustificativi);
                                }
                            }
                        }
                    }
                }


            }
        }
        private List<Dip_GG_Richiesta> Get_Dip_GG_Richiesta(List<int> idDipRappList, int Year, int Month)
        {
            List<Dip_GG_Richiesta> richiesta = _dip_GG_RichiestaRepository.FindAll(x => idDipRappList.Contains(x.IdDip_RapportoLavoro) &&
                                                                                        (
                                                                                            (x.Data.Year == Year && x.Data.Month == Month) ||
                                                                                            (x.DataA.Year == Year && x.DataA.Month == Month) ||
                                                                                            (x.Data.Year < Year || (x.Data.Year == Year && x.Data.Month < Month)) && (x.DataA.Year > Year || (x.DataA.Year == Year && x.DataA.Month > Month))
                                                                                        )
                                                                                  )
                                                                          .ToList();

            return richiesta;
        }


    }

    public interface IDip_GG_RichiestaService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_Richiesta_GetAll4User_OutModel>> GetAll4User(GenericRequest<Dip_GG_Richiesta_GetAll4User_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Richiesta_GetAll4Admin_OutModel>> GetAll4Admin(GenericRequest<Dip_GG_Richiesta_GetAll4Admin_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Richiesta_Send_OutModel>> Send(GenericRequest<Dip_GG_Richiesta_Send_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Richiesta_SetState_OutModel>> SetState(GenericRequest<Dip_GG_Richiesta_SetState_InModel> model, Boolean isSubProcess);

    }
}
