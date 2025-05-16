using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{

    // File: Dip_GG_Richiesta_AND_Dip_GG_RichiestaModel_Mapper.cs (o dove si trova questa classe)
    // ... (altre using, inclusa System.Text.Json) ...

    namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze // Assumendo il namespace corretto
    {
        public class Dip_GG_Richiesta_To_Dip_GG_RichiestaModel_Mapper : Profile
        {
            public Dip_GG_Richiesta_To_Dip_GG_RichiestaModel_Mapper()
            {
                // ----- AGGIUNGI QUESTA DEFINIZIONE -----
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // Utile se il case nel JSON non corrisponde perfettamente
                                                        // Converters = { new JsonStringEnumConverter() }, // Aggiungi se gli enum nel JSON sono stringhe
                                                        // Altre opzioni se necessario
                };
                // -----------------------------------------

                CreateMap<Dip_GG_Richiesta, Dip_GG_RichiestaModel>()
                    .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data.ToString("dd/MM/yyyy")))
                    .ForMember(dest => dest.DataA, opt => opt.MapFrom(src => src.DataA.ToString("dd/MM/yyyy")))
                    

                    .ForMember(dest => dest.RichiestaApprovazioneData, opt => 
                        opt.MapFrom(src => !string.IsNullOrEmpty(src.RichiestaApprovazioneData)
                                            ? JsonSerializer.Deserialize<List<Dip_GG_Richiesta_Stato_Cronology>>(src.RichiestaApprovazioneData, jsonOptions)
                                            : new List<Dip_GG_Richiesta_Stato_Cronology>()))
                    .ForMember(dest => dest.RevocaApprovazioneData, opt => 
                        opt.MapFrom(src => !string.IsNullOrEmpty(src.RevocaApprovazioneData)
                                            ? JsonSerializer.Deserialize<List<Dip_GG_Richiesta_Stato_Cronology>>(src.RevocaApprovazioneData, jsonOptions)
                                            : new List<Dip_GG_Richiesta_Stato_Cronology>()));

                
            }
        }
    }


    public class Dip_GG_RichiestaModel_To_Dip_GG_Richiesta_Mapper : Profile
    {
        public Dip_GG_RichiestaModel_To_Dip_GG_Richiesta_Mapper()
        {
            // Definisci jsonOptions localmente per questo mapper
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false, // Output compatto per il DB, non indentato
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // Non scrive proprietà con valore null nel JSON
                // Converters = { new JsonStringEnumConverter() }, // Decommenta se vuoi che gli enum siano stringhe nel JSON
                // PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Decommenta se vuoi che i nomi delle proprietà nel JSON siano camelCase
            };

            CreateMap<Dip_GG_RichiestaModel, Dip_GG_Richiesta>()
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => DateTime.ParseExact(src.Data, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)))
                .ForMember(dest => dest.DataA, opt => opt.MapFrom(src => DateTime.ParseExact(src.DataA, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)))

                // --- SCENARIO 2 ATTIVO: Il Model (Dip_GG_RichiestaModel) ha le cronologie come LISTE DI OGGETTI ---
                // Serializza la lista di oggetti CronologiaApprovazioneOggetti dal Model in una stringa JSON
                // per la proprietà RichiestaApprovazioneData dell'Entità.
                .ForMember(dest => dest.RichiestaApprovazioneData, opt =>
                    opt.MapFrom(src => JsonSerializer.Serialize(
                                            // Se src.CronologiaApprovazioneOggetti è null, serializza una lista vuota.
                                            src.RichiestaApprovazioneData ?? new List<Dip_GG_Richiesta_Stato_Cronology>(),
                                            jsonOptions))) // Usa le opzioni JSON definite sopra

                // Serializza la lista di oggetti CronologiaRevocaOggetti dal Model in una stringa JSON
                // per la proprietà RevocaApprovazioneData dell'Entità.
                .ForMember(dest => dest.RevocaApprovazioneData, opt =>
                    opt.MapFrom(src => JsonSerializer.Serialize(
                                            // Se src.CronologiaRevocaOggetti è null, serializza una lista vuota.
                                            src.RevocaApprovazioneData ?? new List<Dip_GG_Richiesta_Stato_Cronology>(),
                                            jsonOptions))); // Usa le opzioni JSON definite sopra
        }
    }

}
