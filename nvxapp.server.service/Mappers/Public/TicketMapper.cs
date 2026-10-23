using AutoMapper;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models;

namespace nvxapp.server.service.Mappers.Public
{
    /*
     Ticket di assistenza. I nomi degli utenti e le descrizioni dei tipi non sono nelle entita' del ticket:
     li completa il servizio dopo la mappatura.
    */
    public class TicketMapper : Profile
    {
        public TicketMapper()
        {
            CreateMap<Ticket, TicketListModel>()
                .ForMember(x => x.TipoDescrizione, o => o.Ignore())
                .ForMember(x => x.OpenerUserName, o => o.Ignore())
                .ForMember(x => x.CurrentUserName, o => o.Ignore());

            CreateMap<Ticket, TicketDetailModel>()
                .IncludeBase<Ticket, TicketListModel>()
                .ForMember(x => x.TicketCronology, o => o.Ignore())
                .ForMember(x => x.Actions, o => o.Ignore());

            CreateMap<TicketCronology, TicketCronologyModel>()
                .ForMember(x => x.UserName, o => o.Ignore())
                .ForMember(x => x.AuthorName, o => o.Ignore())
                .ForMember(x => x.ReturnUserName, o => o.Ignore())
                .ForMember(x => x.TipoDescrizione, o => o.Ignore())
                .ForMember(x => x.TicketAttach, o => o.Ignore());

            // il contenuto resta fuori: negli elenchi si proietta senza leggerlo dal database
            CreateMap<TicketAttach, TicketAttachModel>()
                .ForMember(x => x.File, o => o.Ignore());

            CreateMap<TicketType, TicketTypeListModel>()
                .ForMember(x => x.Level, o => o.MapFrom(s => (s.TicketTypeLevel ?? new List<TicketTypeLevel>()).Select(l => l.Level).OrderBy(l => l).ToList()));

            CreateMap<TicketType, TicketTypeEditModel>()
                .ForMember(x => x.Level, o => o.MapFrom(s => (s.TicketTypeLevel ?? new List<TicketTypeLevel>()).Select(l => l.Level).OrderBy(l => l).ToList()));

            // i livelli si salvano nella tabella figlia (servizio)
            CreateMap<TicketTypeEditModel, TicketType>()
                .ForMember(x => x.TicketTypeLevel, o => o.Ignore())
                .ForMember(x => x.CreationDate, o => o.Ignore())
                .ForMember(x => x.ModifiedDate, o => o.Ignore())
                .ForMember(x => x.ChangeUser, o => o.Ignore());
        }
    }
}
