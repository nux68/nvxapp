using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.DocumentManagement;
using nvxapp.server.service.ClientServer_Service.DocumentManagement._utility;
using nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_WidgetService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_WidgetService
{
    /*
     Dati dei widget della dashboard del documentale (ANALISI_WIDGET_DOCUMENTALE.md).
     Lo spazio occupato/disponibile usa CompanyStorageGet (Infrastructure); qui c'e' solo la ripartizione per tipo.
     Spazio per tipo documento: somma di Doc_Versione.Dimensione (tutte le versioni) per tipologia, con il cestino a parte,
     come il ricalcolo notturno dello spazio (DocumentManagementStorageUsage). Riservato a chi amministra l'archivio
     (CompanyPowerAdmin, CompanyAdmin): conteggi e dimensioni per tipo rivelerebbero i documenti del personale.
    */
    public class Doc_WidgetService : ServiceBase, IDoc_WidgetService
    {
        private readonly IDoc_VersioneRepository _versioneRepository;
        private readonly IDoc_DocumentoRepository _documentoRepository;
        private readonly IDoc_Par_TipoRepository _tipoRepository;
        private readonly IDocumentManagementUtility _utility;

        public Doc_WidgetService(IMapper mapper,
                                 UserManager<ApplicationUser> userManager,
                                 IAspNetUsersRepository aspNetUsersRepository,
                                 IOptions<JwtParameter> jwtParameter,
                                 IHttpContextAccessor httpContextAccessor,
                                 IConfiguration configuration,
                                 IDoc_VersioneRepository versioneRepository,
                                 IDoc_DocumentoRepository documentoRepository,
                                 IDoc_Par_TipoRepository tipoRepository,
                                 IDocumentManagementUtility utility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _versioneRepository = versioneRepository;
            _documentoRepository = documentoRepository;
            _tipoRepository = tipoRepository;
            _utility = utility;
        }

        public virtual async Task<GenericResult<Doc_SpazioPerTipoGetOutModel>> Doc_SpazioPerTipoGet(GenericRequest<Doc_SpazioPerTipoGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                if (!ctx.AmministraArchivio)
                    throw new UserMessageException(DocumentManagementMessages.Get("NotAllowed"));

                var retVal = new Doc_SpazioPerTipoGetOutModel();

                // byte per tipologia e cestino (tutte le versioni)
                var byte4Tipo = await _versioneRepository.FindAll(v => v.Doc_DocumentoNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                    .GroupBy(v => new { v.Doc_DocumentoNavigation!.IdDoc_Par_Tipo, v.Doc_DocumentoNavigation.NelCestino })
                                    .Select(g => new { g.Key.IdDoc_Par_Tipo, g.Key.NelCestino, Bytes = g.Sum(x => x.Dimensione) })
                                    .ToListAsync();
                var documenti4Tipo = await _documentoRepository.FindAll(d => d.IdAz_Anagrafica == ctx.IdAz_Anagrafica && !d.NelCestino)
                                    .GroupBy(d => d.IdDoc_Par_Tipo)
                                    .Select(g => new { g.Key, Count = g.Count() })
                                    .ToListAsync();
                var tipi = await _tipoRepository.FindAll(t => t.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                    .Select(t => new { t.Id, t.Descrizione })
                                    .ToListAsync();

                retVal.Tipi = byte4Tipo.Where(x => !x.NelCestino)
                                    .GroupBy(x => x.IdDoc_Par_Tipo)
                                    .Select(g => new Doc_SpazioPerTipoModel
                                    {
                                        IdDoc_Par_Tipo = g.Key,
                                        Descrizione = tipi.FirstOrDefault(t => t.Id == g.Key)?.Descrizione ?? string.Empty,
                                        Bytes = g.Sum(x => x.Bytes),
                                        Documenti = documenti4Tipo.FirstOrDefault(d => d.Key == g.Key)?.Count ?? 0
                                    })
                                    .Where(x => x.Bytes > 0)
                                    .OrderByDescending(x => x.Bytes)
                                    .ToList();
                retVal.CestinoBytes = byte4Tipo.Where(x => x.NelCestino).Sum(x => x.Bytes);
                retVal.TotaleBytes = retVal.Tipi.Sum(x => x.Bytes) + retVal.CestinoBytes;

                return retVal;
            }, isSubProcess);
        }
    }

    public interface IDoc_WidgetService : IServiceBase
    {
        Task<GenericResult<Doc_SpazioPerTipoGetOutModel>> Doc_SpazioPerTipoGet(GenericRequest<Doc_SpazioPerTipoGetInModel> model, Boolean isSubProcess);
    }
}
