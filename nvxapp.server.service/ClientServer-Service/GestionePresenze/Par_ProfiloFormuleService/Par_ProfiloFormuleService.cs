using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.FormuleEngine;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloFormuleService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloFormuleService
{
    // Profili formule: testata + righe (formula, ordine, Dal/Al, fase, sospesa). CompanyPowerAdmin, piano Pro. ANALISI_FORMULE.md
    public class Par_ProfiloFormuleService : ServiceBase, IPar_ProfiloFormuleService
    {
        private readonly IPar_ProfiloFormuleRepository _par_ProfiloFormuleRepository;
        private readonly IPar_ProfiloFormuleDettRepository _par_ProfiloFormuleDettRepository;
        private readonly IPar_FormuleRepository _par_FormuleRepository;
        private readonly IDip_ProfiloFormuleRepository _dip_ProfiloFormuleRepository;
        private readonly IFormuleAccesso _formuleAccesso;

        public Par_ProfiloFormuleService(IMapper mapper,
                                         UserManager<ApplicationUser> userManager,
                                         IAspNetUsersRepository aspNetUsersRepository,
                                         IOptions<JwtParameter> jwtParameter,
                                         IHttpContextAccessor httpContextAccessor,
                                         IConfiguration configuration,

                                         IPar_ProfiloFormuleRepository par_ProfiloFormuleRepository,
                                         IPar_ProfiloFormuleDettRepository par_ProfiloFormuleDettRepository,
                                         IPar_FormuleRepository par_FormuleRepository,
                                         IDip_ProfiloFormuleRepository dip_ProfiloFormuleRepository,
                                         IFormuleAccesso formuleAccesso) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_ProfiloFormuleRepository = par_ProfiloFormuleRepository;
            _par_ProfiloFormuleDettRepository = par_ProfiloFormuleDettRepository;
            _par_FormuleRepository = par_FormuleRepository;
            _dip_ProfiloFormuleRepository = dip_ProfiloFormuleRepository;
            _formuleAccesso = formuleAccesso;
        }

        private Task<int> Azienda() => _formuleAccesso.VerificaAsync(this.CurrentUserId, this.CurrentCompany);

        private static string T(string key, params object?[] args) => AttendanceTrackingMessages.Get(key, args);

        private List<Par_ProfiloFormuleDettModel> Righe(int idProfilo)
            => _mapper.Map<List<Par_ProfiloFormuleDettModel>>(
                _par_ProfiloFormuleDettRepository.FindAll(r => r.IdPar_ProfiloFormule == idProfilo).OrderBy(r => r.Ordine).ThenBy(r => r.Id).ToList());

        public virtual async Task<GenericResult<Par_ProfiloFormuleOutModel>> GetAll(GenericRequest<Par_ProfiloFormuleInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var idAzienda = await Azienda();
                var profili = _par_ProfiloFormuleRepository.FindAll(x => x.IdAz_Anagrafica == idAzienda).OrderBy(x => x.Codice).ToList();
                return new Par_ProfiloFormuleOutModel { Par_ProfiloFormule = _mapper.Map<List<Par_ProfiloFormuleModel>>(profili) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ProfiloFormuleGetOutModel>> Par_ProfiloFormuleGet(GenericRequest<Par_ProfiloFormuleGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var idAzienda = await Azienda();
                var retVal = new Par_ProfiloFormuleGetOutModel();
                var profilo = model.Data.Id > 0 ? await _par_ProfiloFormuleRepository.FindByIdAsync(model.Data.Id) : null;
                if (profilo != null && profilo.IdAz_Anagrafica == idAzienda)
                {
                    retVal.Par_ProfiloFormule = _mapper.Map<Par_ProfiloFormuleModel>(profilo);
                    retVal.Par_ProfiloFormuleDett = Righe(profilo.Id);
                }
                else
                    retVal.Par_ProfiloFormule = new Par_ProfiloFormuleModel { IdAz_Anagrafica = idAzienda };
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ProfiloFormulePutOutModel>> Par_ProfiloFormulePut(GenericRequest<Par_ProfiloFormulePutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var idAzienda = await Azienda();
                var m = model.Data.Par_ProfiloFormule;
                m.Codice = m.Codice?.Trim();
                m.Descrizione = m.Descrizione?.Trim();
                if (string.IsNullOrWhiteSpace(m.Codice) || string.IsNullOrWhiteSpace(m.Descrizione))
                    throw new UserMessageException(T("ProfiloFormuleCodiceObbligatorio"));
                if (_par_ProfiloFormuleRepository.FindAll(x => x.IdAz_Anagrafica == idAzienda && x.Codice == m.Codice && x.Id != m.Id).Any())
                    throw new UserMessageException(T("ProfiloFormuleCodiceDuplicato", m.Codice));

                // righe: formula dell'azienda, ordine > 0, periodo valido
                var formuleAzienda = _par_FormuleRepository.FindAll(f => f.IdAz_Anagrafica == idAzienda).Select(f => f.Id).ToHashSet();
                foreach (var r in model.Data.Par_ProfiloFormuleDett)
                    if (!formuleAzienda.Contains(r.IdPar_Formule) || r.Ordine <= 0 || r.Al.Date < r.Dal.Date)
                        throw new UserMessageException(T("ProfiloFormuleRigaNonValida"));

                if (m.Id > 0)
                {
                    var esistente = await _par_ProfiloFormuleRepository.FindByIdAsync(m.Id);
                    if (esistente == null || esistente.IdAz_Anagrafica != idAzienda)
                        m.Id = 0;
                }
                var testata = _mapper.Map<Par_ProfiloFormule>(m);
                testata.IdAz_Anagrafica = idAzienda;
                testata = await _par_ProfiloFormuleRepository.UpsertAsync(testata);

                // righe tolte nel client
                var idClient = model.Data.Par_ProfiloFormuleDett.Where(r => r.Id > 0).Select(r => r.Id).ToHashSet();
                foreach (var vecchia in _par_ProfiloFormuleDettRepository.FindAll(r => r.IdPar_ProfiloFormule == testata.Id).ToList())
                    if (!idClient.Contains(vecchia.Id))
                        await _par_ProfiloFormuleDettRepository.DeleteAsync(vecchia);

                // upsert (le righe con id negativo sono nuove)
                foreach (var r in model.Data.Par_ProfiloFormuleDett)
                {
                    var riga = _mapper.Map<Par_ProfiloFormuleDett>(r);
                    if (riga.Id < 0) riga.Id = 0;
                    riga.IdPar_ProfiloFormule = testata.Id;
                    riga.Dal = r.Dal.Date;
                    riga.Al = r.Al.Date;
                    await _par_ProfiloFormuleDettRepository.UpsertAsync(riga);
                }

                return new Par_ProfiloFormulePutOutModel
                {
                    Par_ProfiloFormule = _mapper.Map<Par_ProfiloFormuleModel>(testata),
                    Par_ProfiloFormuleDett = Righe(testata.Id)
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ProfiloFormule_DeleteOutModel>> Par_ProfiloFormuleDelete(GenericRequest<Par_ProfiloFormule_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var idAzienda = await Azienda();
                var retVal = new Par_ProfiloFormule_DeleteOutModel();

                int usato = _dip_ProfiloFormuleRepository.FindAll(x => x.IdPar_ProfiloFormule == model.Data.Id).Count();
                if (usato > 0)
                {
                    retVal.Messages.Add(new Message(T("ProfiloFormuleInUso", usato), MessageType.Exception));
                    return retVal;
                }

                var entity = await _par_ProfiloFormuleRepository.FindByIdAsync(model.Data.Id);
                if (entity != null && entity.IdAz_Anagrafica == idAzienda)
                    await _par_ProfiloFormuleRepository.DeleteAsync(entity);   // le righe vanno in cascata
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ProfiloFormule_DuplicaOutModel>> Par_ProfiloFormuleDuplica(GenericRequest<Par_ProfiloFormule_DuplicaInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var idAzienda = await Azienda();
                var origine = await _par_ProfiloFormuleRepository.FindByIdAsync(model.Data.Id);
                var codice = model.Data.Codice?.Trim();
                if (origine == null || origine.IdAz_Anagrafica != idAzienda || string.IsNullOrWhiteSpace(codice))
                    throw new UserMessageException(T("ProfiloFormuleCodiceObbligatorio"));
                if (_par_ProfiloFormuleRepository.FindAll(x => x.IdAz_Anagrafica == idAzienda && x.Codice == codice).Any())
                    throw new UserMessageException(T("ProfiloFormuleCodiceDuplicato", codice));

                var copia = await _par_ProfiloFormuleRepository.UpsertAsync(new Par_ProfiloFormule
                {
                    IdAz_Anagrafica = idAzienda,
                    Codice = codice,
                    Descrizione = origine.Descrizione
                });
                foreach (var r in _par_ProfiloFormuleDettRepository.FindAll(r => r.IdPar_ProfiloFormule == origine.Id).ToList())
                    await _par_ProfiloFormuleDettRepository.UpsertAsync(new Par_ProfiloFormuleDett
                    {
                        IdPar_ProfiloFormule = copia.Id,
                        IdPar_Formule = r.IdPar_Formule,
                        Ordine = r.Ordine,
                        Dal = r.Dal,
                        Al = r.Al,
                        Sospesa = r.Sospesa,
                        Fase = r.Fase
                    });

                return new Par_ProfiloFormule_DuplicaOutModel { Par_ProfiloFormule = _mapper.Map<Par_ProfiloFormuleModel>(copia) };
            }, isSubProcess);
        }
    }

    public interface IPar_ProfiloFormuleService : IServiceBase
    {
        Task<GenericResult<Par_ProfiloFormuleOutModel>> GetAll(GenericRequest<Par_ProfiloFormuleInModel> model, bool isSubProcess);
        Task<GenericResult<Par_ProfiloFormuleGetOutModel>> Par_ProfiloFormuleGet(GenericRequest<Par_ProfiloFormuleGetInModel> model, bool isSubProcess);
        Task<GenericResult<Par_ProfiloFormulePutOutModel>> Par_ProfiloFormulePut(GenericRequest<Par_ProfiloFormulePutInModel> model, bool isSubProcess);
        Task<GenericResult<Par_ProfiloFormule_DeleteOutModel>> Par_ProfiloFormuleDelete(GenericRequest<Par_ProfiloFormule_DeleteInModel> model, bool isSubProcess);
        Task<GenericResult<Par_ProfiloFormule_DuplicaOutModel>> Par_ProfiloFormuleDuplica(GenericRequest<Par_ProfiloFormule_DuplicaInModel> model, bool isSubProcess);
    }
}
