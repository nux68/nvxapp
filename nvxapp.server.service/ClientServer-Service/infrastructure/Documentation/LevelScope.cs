using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Documentation
{
    /*
     Livelli della gerarchia che l'utente corrente gestisce, con il nodo preso dal token (mai dal client):
       SuperUser                                 -> livello SuperUser, nessun nodo
       DealerPowerAdmin / DealerAdmin            -> Dealer del token
       FinancialAdvisorPowerAdmin / ...Admin     -> FinancialAdvisor del token
       CompanyPowerAdmin / CompanyAdmin          -> Company del token
     Modifica solo il SuperUser e i PowerAdmin; gli Admin vedono in sola lettura.
     Un utente con piu' ruoli ha piu' livelli (si leggono tutti i ruoli, mai solo il primo).
    */
    public class LevelScope
    {
        public TicketLevel Level { get; set; }
        public int? IdDealer { get; set; }
        public int? IdFinancialAdvisor { get; set; }
        public int? IdCompany { get; set; }
        public bool CanEdit { get; set; }

        // l'elemento (documento o impostazione) appartiene al nodo di questo livello
        public bool Owns(TicketLevel level, int? idDealer, int? idFinancialAdvisor, int? idCompany)
            => level == Level && idDealer == IdDealer && idFinancialAdvisor == IdFinancialAdvisor && idCompany == IdCompany;

        public static List<LevelScope> For(IList<string> roles, string dealer, string financialAdvisor, string company)
        {
            bool Has(RoleCode r) => roles.Contains(r.ToString());

            var retVal = new List<LevelScope>();

            if (Has(RoleCode.SuperUser))
                retVal.Add(new LevelScope { Level = TicketLevel.SuperUser, CanEdit = true });

            if ((Has(RoleCode.DealerPowerAdmin) || Has(RoleCode.DealerAdmin)) && int.TryParse(dealer, out int idDealer))
                retVal.Add(new LevelScope { Level = TicketLevel.Dealer, IdDealer = idDealer, CanEdit = Has(RoleCode.DealerPowerAdmin) });

            if ((Has(RoleCode.FinancialAdvisorPowerAdmin) || Has(RoleCode.FinancialAdvisorAdmin)) && int.TryParse(financialAdvisor, out int idFinancialAdvisor))
                retVal.Add(new LevelScope { Level = TicketLevel.FinancialAdvisor, IdFinancialAdvisor = idFinancialAdvisor, CanEdit = Has(RoleCode.FinancialAdvisorPowerAdmin) });

            if ((Has(RoleCode.CompanyPowerAdmin) || Has(RoleCode.CompanyAdmin)) && int.TryParse(company, out int idCompany))
                retVal.Add(new LevelScope { Level = TicketLevel.Company, IdCompany = idCompany, CanEdit = Has(RoleCode.CompanyPowerAdmin) });

            return retVal;
        }
    }
}
