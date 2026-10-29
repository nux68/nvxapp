using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_CausaliService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService
{
    public class Par_ExportCauService : ServiceBase, IPar_ExportCauService
    {
        private readonly IPar_ExportCauRepository          _par_ExportCauRepository;
        private readonly IGestionePresenzeUserUtility      _gestionePresenzeUserUtility;
        private readonly IPar_ExportCau_CausaliService     _par_ExportCau_CausaliService;
        private readonly IPar_ExportCau_DocsRepository     _par_ExportCau_DocsRepository;

        public Par_ExportCauService(IMapper mapper,
                                    UserManager<ApplicationUser> userManager,
                                    IAspNetUsersRepository aspNetUsersRepository,
                                    IOptions<JwtParameter> jwtParameter,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration,
                                    IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                    IPar_ExportCau_CausaliService par_ExportCau_CausaliService,
                                    IPar_ExportCauRepository par_ExportCauRepository,
                                    IPar_ExportCau_DocsRepository par_ExportCau_DocsRepository)
            : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_ExportCauRepository      = par_ExportCauRepository;
            _par_ExportCau_DocsRepository = par_ExportCau_DocsRepository;
            _gestionePresenzeUserUtility  = gestionePresenzeUserUtility;
            _par_ExportCau_CausaliService = par_ExportCau_CausaliService;
        }

        public virtual async Task<GenericResult<Par_ExportCau_GetAll_OutModel>> GetAll(GenericRequest<Par_ExportCau_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ExportCau_GetAll_OutModel();
                var entities = await _par_ExportCauRepository.FindAll();
                retVal.Par_ExportCau = _mapper.Map<List<Par_ExportCauModel>>(entities);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ExportCau_Get_OutModel>> Par_ExportCauGet(GenericRequest<Par_ExportCau_Get_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ExportCau_Get_OutModel();

                int.TryParse(this.CurrentCompany, out int idCompany);
                var companyData = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);
                if (companyData?.az_Anagrafica != null)
                {
                    var entity = await _par_ExportCauRepository.FindByIdAsync(model.Data.Id);
                    if (entity == null)
                    {
                        entity = new Par_ExportCau()
                        {
                            IdAz_Anagrafica = companyData.az_Anagrafica.Id,
                            Codice          = "0000",
                            Descrizione     = "Nuovo Export"
                        };
                    }
                    retVal.Par_ExportCau = _mapper.Map<Par_ExportCauModel>(entity);

                    var req_1 = new GenericRequest<Par_ExportCau_Causali_GetAll_4Edit_InModel>();
                    req_1.Data = new Par_ExportCau_Causali_GetAll_4Edit_InModel() { Id = model.Data.Id };
                    var res_1 = await _par_ExportCau_CausaliService.Par_ExportCau_Causali_GetAll_4Edit(req_1, true);
                    if (res_1.Success && res_1.Data != null)
                        retVal.Par_ExportCau_Causali = res_1.Data.Par_ExportCau_Causali;
                }

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ExportCau_Put_OutModel>> Par_ExportCauPut(GenericRequest<Par_ExportCau_Put_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ExportCau_Put_OutModel();
                // righe del modello coerenti prima di salvare la testata (altrimenti si salverebbe a meta')
                _par_ExportCau_CausaliService.ControllaCausali(model.Data.Par_ExportCau_Causali);

                var entity = _mapper.Map<Par_ExportCau>(model.Data.Par_ExportCau);

                // la ricetta del formato non arriva dal client: resta quella salvata
                var esistente = _par_ExportCauRepository.FindAll(x => x.Id == entity.Id).Select(x => new { x.RicettaAI, x.RicettaHash }).FirstOrDefault();
                if (esistente != null)
                {
                    entity.RicettaAI = esistente.RicettaAI;
                    entity.RicettaHash = esistente.RicettaHash;
                }

                int.TryParse(this.CurrentCompany, out int idCompany);
                var companyData = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);
                if (companyData?.az_Anagrafica != null)
                    entity.IdAz_Anagrafica = companyData.az_Anagrafica.Id;

                var nuovo = entity.Id == 0;
                var updatedEntity = await _par_ExportCauRepository.UpsertAsync(entity);
                retVal.Par_ExportCau = _mapper.Map<Par_ExportCauModel>(updatedEntity);

                // duplicazione: il modello nuovo riceve anche documentazione e ricetta di quello copiato
                // (con gli stessi documenti e le stesse note la ricetta resta valida e non si chiede di nuovo all'AI)
                if (nuovo && model.Data.DuplicaDa is int duplicaDa && duplicaDa > 0)
                    await CopiaDocumentazione(duplicaDa, updatedEntity);

                var req_1 = new GenericRequest<Par_ExportCau_Causali_PutAll_4Edit_InModel>();
                req_1.Data = new Par_ExportCau_Causali_PutAll_4Edit_InModel()
                {
                    IdPar_ExportCau     = retVal.Par_ExportCau.Id,
                    Par_ExportCau_Causali = model.Data.Par_ExportCau_Causali
                };
                var res_1 = await _par_ExportCau_CausaliService.Par_ExportCau_Causali_PutAll_4Edit(req_1, true);
                if (res_1.Success && res_1.Data != null)
                    retVal.Par_ExportCau_Causali = res_1.Data.Par_ExportCau_Causali;

                return retVal;
            }, isSubProcess);
        }

        private async Task CopiaDocumentazione(int idOrigine, Par_ExportCau destinazione)
        {
            var origine = _par_ExportCauRepository.FindAll(x => x.Id == idOrigine).Select(x => new { x.RicettaAI, x.RicettaHash }).FirstOrDefault();
            if (origine == null)
                return;

            var documenti = _par_ExportCau_DocsRepository.FindAll(x => x.IdPar_ExportCau == idOrigine).OrderBy(x => x.Ordine).ThenBy(x => x.Id).ToList();
            foreach (var d in documenti)
                await _par_ExportCau_DocsRepository.UpsertAsync(new Par_ExportCau_Docs
                {
                    IdPar_ExportCau = destinazione.Id,
                    NomeFile = d.NomeFile,
                    ContentType = d.ContentType,
                    Dimensione = d.Dimensione,
                    Contenuto = d.Contenuto,
                    Testo = d.Testo,
                    Descrizione = d.Descrizione,
                    Ordine = d.Ordine
                });

            destinazione.RicettaAI = origine.RicettaAI;
            destinazione.RicettaHash = origine.RicettaHash;
            await _par_ExportCauRepository.UpsertAsync(destinazione);
        }

        public virtual async Task<GenericResult<Par_ExportCau_Delete_OutModel>> Par_ExportCauDelete(GenericRequest<Par_ExportCau_Delete_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var entity = await _par_ExportCauRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                    await _par_ExportCauRepository.DeleteAsync(entity);

                return new Par_ExportCau_Delete_OutModel();
            }, isSubProcess);
        }
    }

    public interface IPar_ExportCauService : IServiceBase
    {
        Task<GenericResult<Par_ExportCau_GetAll_OutModel>>  GetAll(GenericRequest<Par_ExportCau_GetAll_InModel>   model, bool isSubProcess);
        Task<GenericResult<Par_ExportCau_Get_OutModel>>     Par_ExportCauGet(GenericRequest<Par_ExportCau_Get_InModel>    model, bool isSubProcess);
        Task<GenericResult<Par_ExportCau_Put_OutModel>>     Par_ExportCauPut(GenericRequest<Par_ExportCau_Put_InModel>    model, bool isSubProcess);
        Task<GenericResult<Par_ExportCau_Delete_OutModel>>  Par_ExportCauDelete(GenericRequest<Par_ExportCau_Delete_InModel> model, bool isSubProcess);
    }
}
