namespace nvxapp.server.data.Interfaces
{
    /*
     i repo che devono leggere i dati sul tenat, devono implementare questa interfaccia
     
    (NB. l'implemantazione sta già nella classe base, quindi bastera aggiungere :ICurrentTenant )
     
     */
    public interface ICurrentTenant
    {
        string? CurrentTenant { get; set; }

    }
}
