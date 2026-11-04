using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    // fotografia mensile dello spazio di un'azienda: andamento e base per la fatturazione del centro
    public class CompanyStorageHistory : BaseEntity
    {
        [Required]
        public int IdCompany { get; set; }

        public int Anno { get; set; }

        public int Mese { get; set; }

        [MaxLength(50)]
        public string? Pacchetto { get; set; }

        public int QuotaMB { get; set; }

        public long UsedBytes { get; set; }

        public int FileCount { get; set; }
    }
}
