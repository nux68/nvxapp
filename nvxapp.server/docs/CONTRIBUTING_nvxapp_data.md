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


## Punti d'attenzione
 1. Il nome delle tabelle nel context non va al plurale
 2. le entita vanno aggiunte ad dbcontext con le key public virtual 
 3. Prima di generare le classi Entity, è fondamantale verificare il branch corrente (Infrastructure o GestionePresenze) e posizionare le entita nella cartella corretta
 4. Se il branch corrente è Infrastructure o Infrastructure_Dev  posizionare le entita nella cartella nvxapp.server.data\Entities\Public con namespace namespace nvxapp.server.data.Entities.Public
 5. Se il branch corrente è GestionePresenze o GestionePresenze_Dev posizionare le entita nella cartella nvxapp.server.data\Entities\Tenant\GestionePresenze con namespace nvxapp.server.data.Entities.Tenant    


## Entità

Le classi che definiscono le entita per il database derivano da BaseEntity prendere come esempio l' entità riportata di seguito

```linguaggio
using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class MyTabella : BaseEntity
    {
       /* campi della tabella  */
       public int IdAz_Anagrafica { get; set; }
       public string? Codice { get; set; }
       public string? Descrizione { get; set; }
    }
}
```

Dopo la creazione della entità, aggiungere la tabella al DbContext
```linguaggio
    public virtual DbSet<MyTabella> MyTabella { get; set; }
```
e creare la migrazione per aggiornare il database


## REGOLA BASE: si accede al database solo mediante i repository

Nessun servizio, utilita', controller o lavoro in background usa direttamente un DbContext (PublicDbContext, CommonDbContext,
AttendanceTrackingDbContext, CrmDbContext, DocumentManagementDbContext...): letture e scritture passano sempre da un repository.

 1. **Un repository per entita'**, nella cartella del suo contesto:
    - `Repositories/Public`                 : tabelle di public (PublicDbContext)
    - `Repositories/Tenant/Common`          : tabelle comuni dell'azienda, schema Common (CommonDbContext)
    - `Repositories/Tenant/<Applicativo>`   : tabelle dell'applicativo (es. `GestionePresenze`, `Crm`, `DocumentManagement`), sul contesto dell'applicativo
    Classe `<Entita>Repository : Repository<<Contesto>, <Entita>>` e interfaccia `I<Entita>Repository` nello stesso file.
 2. **I servizi iniettano solo interfacce di repository**, mai un DbContext.
 3. **Letture**: `FindAll(where)` (senza tracking) con Include/Select/OrderBy/... ; `FindByIdAsync`. Una query deve stare su un
    solo contesto: le subquery e le join usano repository dello stesso contesto.
 4. **Scritture**: `CreateAsync`, `UpdateAsync`, `UpsertAsync`, `DeleteAsync`, `DeleteRangeAsync`, `SaveChange` (impostano
    date e utente della modifica). Modifiche massive: `ExecuteUpdateAsync` / `ExecuteDeleteAsync` sull'IQueryable di `FindAll(where)`.
 5. **Operazioni particolari** (SQL atomico, UPDATE condizionali, chiavi non intere, salvataggi che richiedono una transazione)
    sono **metodi del repository**, dichiarati nella sua interfaccia (es. `ICompanyStorageRepository.ReserveAsync`,
    `IAppSettingRepository.GetValueAsync/SetValueAsync`).
 6. **Transazioni**: `BeginTransactionAsync()` di un repository; vale per tutti i repository dello stesso contesto nella stessa
    richiesta (es. tutti quelli di public, insieme a UserManager).
 7. **Tabelle comuni (schema Common)**:
    - scritture solo con i repository di `Repositories/Tenant/Common` (es. `IAz_ClienteRepository`, `IAz_ClienteContattoRepository`,
      `IDip_AnagraficaCommonRepository`);
    - letture dentro le query di un applicativo (join e subquery con le tabelle dell'applicativo) con **repository di sola lettura
      sul contesto dell'applicativo**, nella cartella dell'applicativo e con il prefisso delle sue tabelle:
      `<Prefisso>_<Entita>ReadRepository` (es. `Crm_Az_ClienteReadRepository`, `Doc_Dip_AnagraficaReadRepository`). Non si usano per scrivere.
    - nota: `Dip_AnagraficaCommonRepository` si chiama cosi' perche' esiste anche `Repositories.Tenant.GestionePresenze.Dip_AnagraficaRepository`
      (sul contesto delle presenze): con lo stesso nome i file che importano entrambi i namespace non compilerebbero.
 8. **Fuori da una richiesta HTTP** (hosted service, deleter e provider che lavorano su uno schema preciso con un contesto creato
    da `IApplicationDbContextFactory`): i repository si prendono dallo scope (`scope.ServiceProvider.GetRequiredService<I...Repository>()`)
    oppure si costruiscono sul contesto della factory: `new XRepository(db, serviceProvider, httpContextAccessor)`.

## Repository
Il Repository è il componente utilizzato per accedere al database

Creare un repository (che deriva dalla classe Repository) per ogni entità definita
```linguaggio

using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data;
using nvxapp.server.data.Repositories;
using nvxapp.server.data.Repositories.Tenant;
using nvxapp.server.data.Repositories.Public;
using System;
using nvxapp.server.data.Interfaces;
using nvxapp.server.data.Infrastructure;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
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
}



```

I repository vengono posizionate nella sottocartella `nvxapp.server.data\Repositories\public` se contengono dati comuni o nella sottocartella `nvxapp.server.data\Repositories\Tenant\GestionePresenze`  se sono dedicate a un argomento specifico es GestionePresenze (comunque in fase di generazione , tenere sempre presente il branch corrente)


## Punti d'attenzione Repository
 1. La definizione dell interface del repository deve essere sempre posizionata nello stesso file della classe che implementa il repository
 2. Il namespace del repository deve rispecchiare la cartella in cui viene posizionato ad esempio nvxapp.server.data.Repositories.Tenant.GestionePresenze
 3. Un repository per ogni entita', anche per le tabelle dei nuovi applicativi (Crm, DocumentManagement...): vedi la REGOLA BASE sopra
 4. Le tabelle comuni lette nelle query di un applicativo hanno un repository di sola lettura sul contesto dell'applicativo (`<Prefisso>_<Entita>ReadRepository`)



- [Home](./CONTRIBUTING.md)





 