using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_Rapporto_Giustificativi_MaturazioneService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class Dip_Rapporto_Giustificativi_Maturazione_To_Dip_Rapporto_Giustificativi_MaturazioneModel_Mapper : Profile
    {
        public Dip_Rapporto_Giustificativi_Maturazione_To_Dip_Rapporto_Giustificativi_MaturazioneModel_Mapper()
        {
            CreateMap<Dip_Rapporto_Giustificativi_Maturazione, Dip_Rapporto_Giustificativi_MaturazioneModel>()
                .ForMember(dest => dest.OreMaturazione,
                           opt  => opt.MapFrom(src => TimeSpanToString(src.OreMaturazione)));
        }

        /// <summary>Formato "HHH:MM" — le ore totali possono superare 23.</summary>
        private static string TimeSpanToString(TimeSpan ts)
            => $"{(long)ts.TotalHours:D2}:{ts.Minutes:D2}";
    }

    public class Dip_Rapporto_Giustificativi_MaturazioneModel_To_Dip_Rapporto_Giustificativi_Maturazione_Mapper : Profile
    {
        public Dip_Rapporto_Giustificativi_MaturazioneModel_To_Dip_Rapporto_Giustificativi_Maturazione_Mapper()
        {
            CreateMap<Dip_Rapporto_Giustificativi_MaturazioneModel, Dip_Rapporto_Giustificativi_Maturazione>()
                .ForMember(dest => dest.OreMaturazione,
                           opt  => opt.MapFrom(src => ParseTimeSpan(src.OreMaturazione)));
        }

        /// <summary>
        /// Deserializza "HHH:MM" o "HHH:MM:SS" in TimeSpan.
        /// Gestisce ore > 23 senza usare TimeSpan.Parse (che richiede d.hh:mm:ss).
        /// </summary>
        private static TimeSpan ParseTimeSpan(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return TimeSpan.Zero;
            var parts = s.Trim().Split(':');
            if (parts.Length >= 2
                && long.TryParse(parts[0].Trim(), out long totalHours)
                && int.TryParse(parts[1].Trim(), out int minutes))
            {
                int seconds = parts.Length >= 3 && int.TryParse(parts[2].Trim(), out int sec) ? sec : 0;
                return TimeSpan.FromSeconds(totalHours * 3600L + minutes * 60 + seconds);
            }
            return TimeSpan.Zero;
        }
    }
}
