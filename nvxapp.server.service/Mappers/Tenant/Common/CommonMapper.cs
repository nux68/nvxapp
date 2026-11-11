using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common.Az_ClienteContattoService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common.Az_ClienteService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common.CommonOrganizzazioneService.Models;

namespace nvxapp.server.service.Mappers.Tenant.Common
{
    // tabelle comuni dell'azienda (schema Common)
    public class CommonMapper : Profile
    {
        public CommonMapper()
        {
            CreateMap<Az_Cliente, Az_ClienteModel>();
            CreateMap<Az_ClienteModel, Az_Cliente>()
                .ForMember(d => d.Az_AnagraficaNavigation, o => o.Ignore())
                .ForMember(d => d.Az_ClienteContatto, o => o.Ignore());

            CreateMap<Az_ClienteContatto, Az_ClienteContattoModel>();
            CreateMap<Az_ClienteContattoModel, Az_ClienteContatto>()
                .ForMember(d => d.Az_ClienteNavigation, o => o.Ignore());

            // organizzazione in sola lettura (CommonOrganizzazioneService): l'albero dei reparti lo costruisce il servizio
            CreateMap<Az_Sedi, Az_SediCommonModel>()
                .ForMember(d => d.Az_SediReparto, o => o.Ignore());
            CreateMap<Az_SediReparto, Az_SediRepartoCommonModel>()
                .ForMember(d => d.Az_SediReparto_Sub, o => o.Ignore());
            CreateMap<Par_Competenza, Par_CompetenzaCommonModel>();
        }
    }
}
