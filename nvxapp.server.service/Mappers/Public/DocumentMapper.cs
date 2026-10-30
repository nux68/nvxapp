using AutoMapper;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Documentation.Models;

namespace nvxapp.server.service.Mappers.Public
{
    /*
     Documenti dei livelli. Il file non va mai al client nel dettaglio (si scarica con DocumentFileGet);
     tipi collegati e permesso di modifica li completa il servizio.
    */
    public class DocumentMapper : Profile
    {
        public DocumentMapper()
        {
            CreateMap<Document, DocumentEditModel>()
                .ForMember(x => x.File, o => o.Ignore())
                .ForMember(x => x.RemoveFile, o => o.Ignore())
                .ForMember(x => x.IdTicketType, o => o.Ignore())
                .ForMember(x => x.CanEdit, o => o.Ignore());
        }
    }
}
