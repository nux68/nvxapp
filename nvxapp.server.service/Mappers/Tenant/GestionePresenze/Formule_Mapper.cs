using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloFormuleService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FormuleService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloFormuleService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    // Formule personalizzate del calcolo giornata (ANALISI_FORMULE.md)
    public class Formule_Mapper : Profile
    {
        public Formule_Mapper()
        {
            CreateMap<Par_Formule, Par_FormuleModel>().ReverseMap();
            CreateMap<Par_ProfiloFormule, Par_ProfiloFormuleModel>().ReverseMap();
            CreateMap<Par_ProfiloFormuleDett, Par_ProfiloFormuleDettModel>().ReverseMap();
            CreateMap<Dip_ProfiloFormule, Dip_ProfiloFormuleModel>().ReverseMap();
        }
    }
}
