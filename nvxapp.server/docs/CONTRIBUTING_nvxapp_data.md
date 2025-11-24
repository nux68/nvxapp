# Server.Data

Questo documento fornisce linee guida per contribuire ai dati di nvxapp. Seguire queste istruzioni per garantire che i vostri contributi siano conformi agli standard del progetto.

## Architectural Guidelines
- **Database**: Il progetto utilizza un Db postgreSQL
- **Accesso**: Mediante entity framework core

## Db Context
- ApplicationDbContext contiene tutte le funzioni comuni
- `ApplicationDbContext_Infrastructure.cs`: Contiene tutte le tabelle comuni
- `ApplicationDbContext_GestionePresenze.cs`: Contiene tutte le tabelle del argomento presenze
- `Tenant`: in funzione di un parametro di configurazione, il database puo essere multi tenant o singolo tenant. In multi tenant ogni tenant ha il suo database separato, in singolo tenant tutti i dati sono nello stesso database.
- il DbContext è unico viene esteso mediante partial class , ApplicationDbContext_Infrastructure va modificato solo nel branch Infrastructure, ApplicationDbContext_GestionePresenze va modificato solo nel branch GestionePresenze
- `Cartella Entities`: La cartelle Entities è composta da piu sotto cartelle. In Infrastructure ci canno le entità comuni, in Public\Tenant\GestionePresenze ci sono le entità specifiche per la gestione presenze.


## Entità

Le classi che definiscono le entita per il database derivano da BaseEntity

```linguaggio
public class MyTabella : BaseEntity
{
   /* campi della tabella  */
   public int IdAz_Anagrafica { get; set; }
   public string? Codice { get; set; }
   public string? Descrizione { get; set; }
}
```
Vengono posizionate nella sottocartella `Entities\public` se contengono dati comuni o nella sottocartella `Entities\Tenant`  se sono dedicate a un argomento specifico es GestionePresenze (comunque in fase di generazione , tenere sempre presente il branch corrente)


## Repository
Il Repository è il componente utilizzato per accedere al database

Creare un repository (che deriva dalla classe Repository) per ogni entità definita
```linguaggio
public class MyTabellaRepository : Repository<ApplicationDbContext, MyTabella>, IMyTabellaRepository
{
    public MyTabellaRepository(ApplicationDbContext dbContext,
                               IServiceProvider provider,
                               IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
    {
    }
}
    public interface IMyTabellaRepository : IRepository<MyTabella>
{

}
```

I repository vengono posizionate nella sottocartella `Repositories\public` se contengono dati comuni o nella sottocartella `Repositories\Tenant`  se sono dedicate a un argomento specifico es GestionePresenze (comunque in fase di generazione , tenere sempre presente il branch corrente)









 