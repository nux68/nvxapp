using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.HumanResources.Models
{
    public class Hr_IdInModel { public int Id { get; set; } }
    public class Hr_IdOutModel : ModelResult { public int Id { get; set; } }

    // ------------------------------------------------------------------------------------------------ corsi
    public class Hr_CorsoModel
    {
        public int Id { get; set; }
        public string Titolo { get; set; } = string.Empty;
        public string? Ente { get; set; }
        public decimal? DurataOre { get; set; }
        public int? ValiditaMesi { get; set; }
        public int? IdPar_Competenza { get; set; }
        public string? Competenza { get; set; }
        public bool Obbligatorio { get; set; }
        public bool Attivo { get; set; } = true;
        public string? Note { get; set; }
        public int NumeroEdizioni { get; set; }
    }

    public class Hr_EdizioneBreveModel
    {
        public int Id { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        public string? Sede { get; set; }
        public string? Docente { get; set; }
        public int NumeroPartecipanti { get; set; }
        public int NumeroSuperati { get; set; }
    }

    public class Hr_CorsoListOutModel : ModelResult { public List<Hr_CorsoModel> Corsi { get; set; } = new(); }
    public class Hr_CorsoGetOutModel : ModelResult
    {
        public Hr_CorsoModel Corso { get; set; } = new();
        public List<Hr_EdizioneBreveModel> Edizioni { get; set; } = new();
    }
    public class Hr_CorsoPutInModel { public Hr_CorsoModel Corso { get; set; } = new(); }
    public class Hr_CorsoPutOutModel : ModelResult { public Hr_CorsoModel Corso { get; set; } = new(); }

    // ------------------------------------------------------------------------------------------------ edizioni e partecipanti
    public class Hr_PartecipanteModel
    {
        public int Id { get; set; }
        public int IdDip_Anagrafica { get; set; }
        public string? Dipendente { get; set; }
        public int Esito { get; set; }
        public DateTime? DataConseguimento { get; set; }
        public DateTime? DataScadenza { get; set; }
        public string? AttestatoNomeFile { get; set; }
    }

    public class Hr_EdizioneModel
    {
        public int Id { get; set; }
        public int IdHr_Corso { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        public int? IdAz_Sedi { get; set; }
        public string? Luogo { get; set; }
        public string? Docente { get; set; }
        public string? Note { get; set; }
        public List<Hr_PartecipanteModel> Partecipanti { get; set; } = new();
    }

    public class Hr_EdizioneGetInModel { public int Id { get; set; } public int IdHr_Corso { get; set; } }
    public class Hr_EdizioneGetOutModel : ModelResult
    {
        public string Corso { get; set; } = string.Empty;
        public int? ValiditaMesi { get; set; }
        public Hr_EdizioneModel Edizione { get; set; } = new();
        public List<Hr_DipendenteBreveModel> Dipendenti { get; set; } = new();
    }
    public class Hr_EdizionePutInModel { public Hr_EdizioneModel Edizione { get; set; } = new(); }
    public class Hr_EdizionePutOutModel : ModelResult { public Hr_EdizioneModel Edizione { get; set; } = new(); }

    public class Hr_AttestatoUploadModel { public int IdPartecipante { get; set; } }
    public class Hr_AttestatoUploadOutModel : ModelResult { public Hr_PartecipanteModel Partecipante { get; set; } = new(); }

    // ------------------------------------------------------------------------------------------------ piani di formazione
    public class Hr_PianoModel
    {
        public int Id { get; set; }
        public int Anno { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public string? Note { get; set; }
        public int NumeroRighe { get; set; }
        public int NumeroSvolte { get; set; }
        public List<Hr_PianoRigaModel> Righe { get; set; } = new();
    }

    public class Hr_PianoRigaModel
    {
        public int Id { get; set; }
        public int IdHr_Corso { get; set; }
        public string? Corso { get; set; }
        public int IdDip_Anagrafica { get; set; }
        public string? Dipendente { get; set; }
        public DateTime? EntroIl { get; set; }
        public string? Note { get; set; }
        // corso superato nell'anno del piano (calcolato)
        public bool Svolto { get; set; }
        public DateTime? DataSvolto { get; set; }
    }

    public class Hr_PianoListInModel { public int? Anno { get; set; } }
    public class Hr_PianoListOutModel : ModelResult { public List<Hr_PianoModel> Piani { get; set; } = new(); }
    public class Hr_PianoGetOutModel : ModelResult
    {
        public Hr_PianoModel Piano { get; set; } = new();
        public List<Hr_CorsoModel> Corsi { get; set; } = new();
        public List<Hr_DipendenteBreveModel> Dipendenti { get; set; } = new();
    }
    public class Hr_PianoPutInModel { public Hr_PianoModel Piano { get; set; } = new(); }
    public class Hr_PianoPutOutModel : ModelResult { public Hr_PianoModel Piano { get; set; } = new(); }
}
