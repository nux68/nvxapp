using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket
{
    /*
     Cosa serve alla risposta AI di un ticket che arriva al livello "level" (ANALISI_DOCUMENTAZIONE_AI.md §4.3, §6):
       - impostazione effettiva del tipo: riga del nodo, altrimenti default del livello, altrimenti AI disattivata
       - documenti attivi della sezione TicketAi:
           Dev              -> documenti del SuperUser
           Dealer / FA / Company -> documenti del nodo + quelli dei nodi sopra (e del SuperUser) che valgono
                                    anche per i livelli sotto (IncludeSubLevels), salvo che l'impostazione del tipo
                                    escluda la documentazione dei livelli sopra (UseParentDocuments = false)
         del tipo del ticket o senza tipi, generali o dell'applicativo del tipo.
         Ordine: prima quelli legati al tipo, poi il nodo prima dei livelli sopra, poi Ordine e titolo.
     Il nodo si legge dal percorso del ticket (IdDealer, IdFinancialAdvisor, IdCompany).
    */
    public class TicketAiKnowledge : ITicketAiKnowledge
    {
        private readonly ITicketTypeAiSettingRepository _ticketTypeAiSettingRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentTicketTypeRepository _documentTicketTypeRepository;

        public TicketAiKnowledge(ITicketTypeAiSettingRepository ticketTypeAiSettingRepository,
                                 IDocumentRepository documentRepository,
                                 IDocumentTicketTypeRepository documentTicketTypeRepository)
        {
            _ticketTypeAiSettingRepository = ticketTypeAiSettingRepository;
            _documentRepository = documentRepository;
            _documentTicketTypeRepository = documentTicketTypeRepository;
        }

        public (bool AiEnabled, int AiMaxAttempts, bool UseParentDocuments) Setting(Ticket ticket, TicketLevel level)
        {
            if (!TicketTypeAiSettingService.AiLevels.Contains(level))
                return (false, 1, true);

            int? idDealer = level == TicketLevel.Dealer ? ticket.IdDealer : null;
            int? idFinancialAdvisor = level == TicketLevel.FinancialAdvisor ? ticket.IdFinancialAdvisor : null;
            int? idCompany = level == TicketLevel.Company ? ticket.IdCompany : null;
            if (level != TicketLevel.Dev && idDealer == null && idFinancialAdvisor == null && idCompany == null)
                return (false, 1, true);

            int idTicketType = ticket.IdTicketType;
            var rows = _ticketTypeAiSettingRepository.FindAll(s => s.IdTicketType == idTicketType && s.Level == level).ToList();
            var levelDefault = rows.FirstOrDefault(s => s.IdDealer == null && s.IdFinancialAdvisor == null && s.IdCompany == null);
            var node = level == TicketLevel.Dev
                ? null
                : rows.FirstOrDefault(s => s.IdDealer == idDealer && s.IdFinancialAdvisor == idFinancialAdvisor && s.IdCompany == idCompany);

            return TicketTypeAiSettingService.Effective(levelDefault, node);
        }

        public List<TicketAiDocumentModel> Documents(Ticket ticket, TicketType type, TicketLevel level)
        {
            // proprietari possibili, dal nodo (distanza 0) verso l'alto; "own" = il documento vale anche senza IncludeSubLevels
            var owners = new List<(TicketLevel Level, int? Id, int Distance, bool Own)>();
            switch (level)
            {
                case TicketLevel.Dev:
                    owners.Add((TicketLevel.SuperUser, null, 0, true));
                    break;
                case TicketLevel.Dealer:
                    owners.Add((TicketLevel.Dealer, ticket.IdDealer, 0, true));
                    owners.Add((TicketLevel.SuperUser, null, 1, false));
                    break;
                case TicketLevel.FinancialAdvisor:
                    owners.Add((TicketLevel.FinancialAdvisor, ticket.IdFinancialAdvisor, 0, true));
                    owners.Add((TicketLevel.Dealer, ticket.IdDealer, 1, false));
                    owners.Add((TicketLevel.SuperUser, null, 2, false));
                    break;
                case TicketLevel.Company:
                    owners.Add((TicketLevel.Company, ticket.IdCompany, 0, true));
                    owners.Add((TicketLevel.FinancialAdvisor, ticket.IdFinancialAdvisor, 1, false));
                    owners.Add((TicketLevel.Dealer, ticket.IdDealer, 2, false));
                    owners.Add((TicketLevel.SuperUser, null, 3, false));
                    break;
                default:
                    return new List<TicketAiDocumentModel>();
            }
            // nodo mancante nel percorso: quel livello non ha documenti
            owners.RemoveAll(o => o.Level != TicketLevel.SuperUser && o.Id == null);
            // il tipo esclude la documentazione dei livelli sopra: solo quella del nodo
            if (!Setting(ticket, level).UseParentDocuments)
                owners.RemoveAll(o => o.Distance > 0);

            int? idDealer = ticket.IdDealer, idFinancialAdvisor = ticket.IdFinancialAdvisor, idCompany = ticket.IdCompany;
            var application = type.ApplicationType;

            // candidati senza il file (resta nel database)
            var candidates = _documentRepository.FindAll(d => d.Section == DocumentSection.TicketAi && d.Active
                                                              && (d.ApplicationType == null || d.ApplicationType == application)
                                                              && ((d.Level == TicketLevel.SuperUser)
                                                                  || (d.Level == TicketLevel.Dealer && d.IdDealer == idDealer)
                                                                  || (d.Level == TicketLevel.FinancialAdvisor && d.IdFinancialAdvisor == idFinancialAdvisor)
                                                                  || (d.Level == TicketLevel.Company && d.IdCompany == idCompany)))
                                                .Select(d => new { d.Id, d.Level, d.Titolo, d.Descrizione, d.Contenuto, d.IncludeSubLevels, d.Ordine })
                                                .ToList();

            var ids = candidates.Select(d => d.Id).ToList();
            var links = _documentTicketTypeRepository.FindAll(x => ids.Contains(x.IdDocument)).Select(x => new { x.IdDocument, x.IdTicketType }).ToList();

            var retVal = new List<TicketAiDocumentModel>();
            foreach (var d in candidates)
            {
                var owner = owners.Where(o => o.Level == d.Level).Select(o => ((int Distance, bool Own)?)(o.Distance, o.Own)).FirstOrDefault();
                if (owner == null || (!owner.Value.Own && !d.IncludeSubLevels))
                    continue;

                var types = links.Where(l => l.IdDocument == d.Id).Select(l => l.IdTicketType).ToList();
                if (types.Count > 0 && !types.Contains(ticket.IdTicketType))
                    continue;

                retVal.Add(new TicketAiDocumentModel
                {
                    Id = d.Id,
                    Titolo = d.Titolo,
                    Descrizione = d.Descrizione,
                    Contenuto = d.Contenuto,
                    LinkedToType = types.Count > 0,
                    Distance = owner.Value.Distance,
                    Ordine = d.Ordine,
                });
            }

            return retVal.OrderByDescending(d => d.LinkedToType).ThenBy(d => d.Distance).ThenBy(d => d.Ordine).ThenBy(d => d.Titolo).ToList();
        }
    }

    public interface ITicketAiKnowledge
    {
        (bool AiEnabled, int AiMaxAttempts, bool UseParentDocuments) Setting(Ticket ticket, TicketLevel level);
        List<TicketAiDocumentModel> Documents(Ticket ticket, TicketType type, TicketLevel level);
    }
}
