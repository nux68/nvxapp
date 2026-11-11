using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Digest
{
    /*
     Dati del riepilogo periodico forniti dai moduli (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-AI.1).
     Ogni modulo aggiunge una o piu' classi che implementano IDigestProvider: lo scanner dei servizi le registra e il
     generatore (DigestGenerator) chiede una sezione a tutte quelle degli applicativi attivi dell'azienda.
        - le implementazioni sono chiamate dal servizio in background SENZA utente collegato: non usano CurrentUserId /
          CurrentCompany ma i parametri; le tabelle di public si leggono con i repository dallo scope, quelle degli schemi
          delle aziende con un contesto di IApplicationDbContextFactory e i repository costruiti su di esso, come i
          deleter (CONTRIBUTING_nvxapp_data.md, REGOLA BASE punto 8)
        - restituiscono SOLO fatti (numeri, date, descrizioni degli oggetti): il testo lo scrive l'AI a partire da
          questi fatti, con il divieto di aggiungere altro; se l'AI non c'e' il testo si genera dai fatti cosi' come sono.
          Testi gia' tradotti con le resx del modulo (lingua del server)
        - null (o nessuna riga) = niente da dire: la sezione non compare
        - idAspNetUsers null = vista dell'azienda (riepilogo dello studio); altrimenti cio' che vede quell'utente
          (amministratore dell'azienda)
        - Critica = c'e' qualcosa da fare subito (es. scadenze superate): il riepilogo dello studio elenca solo le aziende
          con almeno una sezione critica
    */
    public interface IDigestProvider
    {
        /// <summary>applicativo del modulo (ApplicationType.Common = funzioni base, sempre attive)</summary>
        ApplicationType Application { get; }

        /// <summary>sezione del riepilogo dell'azienda per il periodo dal-al (giorni compresi; al = giorno della generazione)</summary>
        Task<DigestSection?> SectionAsync(int idCompany, string? idAspNetUsers, DateTime dal, DateTime al);
    }

    /// <summary>Sezione del riepilogo fornita da un modulo (si salva in JSON nei fatti del riepilogo).</summary>
    public class DigestSection
    {
        /// <summary>codice stabile e unico tra i moduli (es. "scadenze", "crm.attivita")</summary>
        public string Codice { get; set; } = string.Empty;

        public ApplicationType Application { get; set; }

        public string Titolo { get; set; } = string.Empty;

        /// <summary>c'e' qualcosa da fare subito</summary>
        public bool Critica { get; set; }

        /// <summary>posizione nel riepilogo (le sezioni critiche vengono comunque prima)</summary>
        public int Ordine { get; set; }

        public List<DigestFact> Righe { get; set; } = new List<DigestFact>();
    }

    /// <summary>Fatto della sezione: frase breve, eventuale numero ed elenco degli oggetti a cui si riferisce.</summary>
    public class DigestFact
    {
        /// <summary>es. "Scadenze superate e ancora aperte: 3"</summary>
        public string Testo { get; set; } = string.Empty;

        public int? Numero { get; set; }

        public bool Critico { get; set; }

        /// <summary>oggetti (es. "15/11/2026 - Revisione estintori"), al massimo una decina</summary>
        public List<string> Dettagli { get; set; } = new List<string>();
    }
}
