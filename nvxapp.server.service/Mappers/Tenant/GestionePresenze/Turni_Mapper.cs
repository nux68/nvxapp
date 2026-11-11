using AutoMapper;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    // turni e pianificazione (AT-TURNI)
    public class Turni_Mapper : Profile
    {
        public Turni_Mapper()
        {
            CreateMap<Az_SediRepartoTurno, Az_SediRepartoTurnoModel>();
            CreateMap<Dip_Turno, Dip_TurnoModel>();
        }
    }
}
