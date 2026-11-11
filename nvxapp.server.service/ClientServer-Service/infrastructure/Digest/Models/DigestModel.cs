using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Digest.Models
{
    // riepiloghi periodici dell'utente corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-AI.3)
    public class DigestModel
    {
        public int Id { get; set; }
        /// <summary>0 = azienda, 1 = studio</summary>
        public TipoRiepilogo Tipo { get; set; }
        public int? IdCompany { get; set; }
        public int? IdFinancialAdvisor { get; set; }
        /// <summary>azienda o studio</summary>
        public string? Soggetto { get; set; }
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        public string Titolo { get; set; } = string.Empty;
        /// <summary>inizio del testo (elenco) o testo completo (dettaglio)</summary>
        public string Testo { get; set; } = string.Empty;
        public bool GeneratoConAI { get; set; }
        public bool Manuale { get; set; }
        public DateTime? Data { get; set; }
    }

    public class DigestDetailModel : DigestModel
    {
        /// <summary>fatti da cui e' stato scritto il testo</summary>
        public DigestFacts? Fatti { get; set; }
    }

    public class DigestListInModel
    {
    }

    public class DigestListOutModel : ModelResult
    {
        public List<DigestModel> Digest { get; set; } = new List<DigestModel>();
        /// <summary>l'utente puo' usare "Genera ora" (CompanyPowerAdmin nell'azienda corrente o FinancialAdvisorPowerAdmin nello studio corrente)</summary>
        public bool PuoGenerare { get; set; }
    }

    public class DigestGetInModel
    {
        public int Id { get; set; }
    }

    public class DigestGetOutModel : ModelResult
    {
        public DigestDetailModel Digest { get; set; } = new DigestDetailModel();
    }

    public class DigestGenerateInModel
    {
    }

    public class DigestGenerateOutModel : ModelResult
    {
        /// <summary>riepiloghi creati (vuoto = nulla da riepilogare)</summary>
        public List<int> Id { get; set; } = new List<int>();
    }
}
