using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Common.Az_ClienteContattoService.Models
{
    public class Az_ClienteContattoModel
    {
        public int Id { get; set; }
        public int IdAz_Cliente { get; set; }
        public string? Cognome { get; set; }
        public string? Nome { get; set; }
        public string? Ruolo { get; set; }
        public string? Telefono { get; set; }
        public string? Cellulare { get; set; }
        public string? Email { get; set; }
        public Boolean Principale { get; set; }
        public string? Note { get; set; }
    }

    public class Az_ClienteContatto_GetAll_InModel
    {
        public int IdAz_Cliente { get; set; } = 0;
    }

    public class Az_ClienteContatto_GetAll_OutModel : ModelResult
    {
        public List<Az_ClienteContattoModel> Az_ClienteContatto { get; set; } = new List<Az_ClienteContattoModel>();
    }

    public class Az_ClienteContattoGetInModel
    {
        public int Id { get; set; } = 0;
    }

    public class Az_ClienteContattoGetOutModel : ModelResult
    {
        public Az_ClienteContattoModel Az_ClienteContatto { get; set; } = new Az_ClienteContattoModel();
    }

    public class Az_ClienteContattoPutInModel : ModelResult
    {
        public Az_ClienteContattoModel Az_ClienteContatto { get; set; } = new Az_ClienteContattoModel();
    }

    public class Az_ClienteContattoPutOutModel : ModelResult
    {
        public Az_ClienteContattoModel Az_ClienteContatto { get; set; } = new Az_ClienteContattoModel();
    }

    public class Az_ClienteContattoDeleteInModel
    {
        public int Id { get; set; } = 0;
    }

    public class Az_ClienteContattoDeleteOutModel : ModelResult
    {
        public Az_ClienteContattoModel Az_ClienteContatto { get; set; } = new Az_ClienteContattoModel();
    }
}
