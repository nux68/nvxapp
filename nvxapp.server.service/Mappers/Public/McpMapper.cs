using AutoMapper;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Mcp.Models;

namespace nvxapp.server.service.Mappers.Public
{
    /*
     Accesso AI tramite MCP (ANALISI_INFRASTRUCTURE_MCP.md). Del token arrivano al client solo i dati descrittivi
     (mai l'impronta); nome dell'utente, azienda e stato li completa il servizio.
    */
    public class McpMapper : Profile
    {
        public McpMapper()
        {
            CreateMap<McpAccessToken, McpTokenModel>()
                .ForMember(x => x.UserName, o => o.Ignore())
                .ForMember(x => x.Company, o => o.Ignore())
                .ForMember(x => x.Impersonated, o => o.MapFrom(s => s.IdAspNetUsers != s.IdAspNetUsersLogin))
                .ForMember(x => x.State, o => o.Ignore());

            CreateMap<McpCompanySetting, McpCompanySettingModel>()
                .ForMember(x => x.MaxLevel, o => o.MapFrom(s => (int)s.MaxLevel));

            CreateMap<McpCallLog, McpCallLogModel>()
                .ForMember(x => x.TokenName, o => o.MapFrom(s => s.TokenName ?? string.Empty))
                .ForMember(x => x.UserNameLogin, o => o.MapFrom(s => s.UserNameLogin ?? string.Empty))
                .ForMember(x => x.UserName, o => o.MapFrom(s => s.UserName ?? string.Empty))
                .ForMember(x => x.Arguments, o => o.MapFrom(s => s.Arguments ?? string.Empty))
                .ForMember(x => x.Message, o => o.MapFrom(s => s.Message ?? string.Empty));
        }
    }
}
