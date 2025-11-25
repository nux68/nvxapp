# Troubleshooting e Checklist - nvxapp

Questa guida risolve i problemi più comuni durante la generazione di tabelle e fornisce checklist dettagliate per il debug.

## ?? Problemi Comuni e Soluzioni

### 1. Errori di Compilazione Backend

#### ? **Problema: "The type or namespace name 'MyTableAI_A' could not be found"**


**Cause Possibili:**
- Namespace sbagliato nell'entità
- Using statement mancante
- Nome dell'entità diverso da quello utilizzato

**? Soluzione:**
```csharp
// Verifica namespace nell'entità
namespace nvxapp.server.data.Entities.Tenant    // ? Deve essere questo

// Verifica using nel repository
using nvxapp.server.data.Entities.Tenant;       // ? Aggiungi questo using

// Verifica nome esatto dell'entità
public class MyTableAI_A : BaseEntity           // ? Nome deve corrispondere esattamente
```

#### ? **Problema: "Cannot resolve symbol 'IMyTableAI_ARepository'"**

**Cause Possibili:**
- Repository non registrato nel DI container
- Interfaccia non definita correttamente

**? Soluzione:**
```csharp
// 1. Verifica interfaccia nel repository file
public interface IMyTableAI_ARepository : IRepository<MyTableAI_A>
{
}

// 2. Verifica registrazione automatica (dovrebbe essere automatica)
// Se non funziona, registra manualmente in Program.cs
services.AddScoped<IMyTableAI_ARepository, MyTableAI_ARepository>();
```

#### ? **Problema: AutoMapper Configuration Error**

**Cause Possibili:**
- Profile AutoMapper non registrato
- Mapping configuration sbagliata

**? Soluzione:**
```csharp
// Verifica che il profile sia in namespace corretto
namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze

// Verifica che erediti da Profile
public class MyTableAI_A_To_MyTableAI_AModel_Mapper : Profile

// AutoMapper dovrebbe registrare automaticamente tutti i Profile
// Se non funziona, verifica che il file sia incluso nel build
```

### 2. Errori di Compilazione Frontend

#### ? **Problema: "Cannot find module './Models/mytableai-a-model'"**

**Cause Possibili:**
- Nome file non corrisponde all'import
- Percorso relativo sbagliato

**? Soluzione:**
```typescript
// Verifica nome file esatto (case-sensitive)
mytableai-a-model.ts                           // ? Deve essere kebab-case

// Verifica import path
import { ... } from './Models/mytableai-a-model';  // ? Percorso relativo corretto

// Verifica export dal model
export class MyTableAI_AModel { ... }          // ? Deve essere exportato
```

#### ? **Problema: "Property 'buttonbar' does not exist on type"**

**Cause Possibili:**
- Edit component non estende BasePageConfirmCancelComponent correttamente
- Template HTML usa sintassi sbagliata

**? Soluzione:**
```typescript
// Verifica extends nel component
export class MyTableAI_AEditPageComponent extends BasePageConfirmCancelComponent<MyTableAI_AModel>

// Verifica template HTML
<app-page-buttonbar [buttonbar]="buttonbar">     // ? Usa buttonbar non btnConfirm/btnCancel
</app-page-buttonbar>
```

#### ? **Problema: "Cannot find name 'SharedComponentInfrastructureModule'"**

**Cause Possibili:**
- Import sbagliato nel module
- Path relativo non corretto

**? Soluzione:**
```typescript
// Verifica import corretto
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

// NON usare SharedModule
// import { SharedModule } from '../../../shared/shared.module';  // ? Sbagliato
```

### 3. Errori di Runtime

#### ? **Problema: "404 Not Found" su chiamate API**

**Cause Possibili:**
- Route nel controller sbagliata
- URL nel service Angular non corrisponde

**? Soluzione:**
```csharp
// Verifica route nel controller
[Route("api/[controller]")]              // ? Deve essere api/MyTableAI_A
public class MyTableAI_AController : NvxControllerBase

[Route("GetAll")]                        // ? Action route
public async Task<...> GetAll(...)
```

```typescript
// Verifica URL nel service Angular
environment.remoteData.apiUri + 'MyTableAI_A/GetAll'  // ? Deve corrispondere
```

#### ? **Problema: "No data returned" dalle API**

**Cause Possibili:**
- Multi-tenancy non configurato correttamente
- Filtro IdAz_Anagrafica non applicato

**? Soluzione:**
```csharp
// Verifica filtro multi-tenancy nel service
var entities = _repository.FindAll(x => x.IdAz_Anagrafica == companyData.az_Anagrafica.Id);

// Verifica che companyData non sia null
if (companyData?.az_Anagrafica != null)
{
    // Query qui
}
```

#### ? **Problema: Navigation non funziona**

**Cause Possibili:**
- Route non registrata
- Path nel navCtrl non corrisponde alla route

**? Soluzione:**
```typescript
// Verifica route in route-attendance-tracking.service.ts
{ path: 'mytableaialist', loadChildren: ... },
{ path: 'mytableaiaedit', loadChildren: ... },

// Verifica navigation
this.navCtrl.navigateForward('/mytableaialist');    // ? Deve corrispondere al path
```

### 4. Problemi di Database

#### ? **Problema: "Table doesn't exist"**

**Cause Possibili:**
- Migration non applicata
- DbSet non aggiunto al context

**? Soluzione:**
```bash
# Genera migration
Add-Migration Add_MyTableAI_A_Table

# Applica migration
Update-Database

# Verifica che non ci siano errori nella migration
```

```csharp
// Verifica DbSet nel ApplicationDbContext_GestionePresenze.cs
public virtual DbSet<MyTableAI_A> MyTableAI_A { get; set; }
```

## ?? Checklist di Debug

### Checklist Backend

#### 1. Entità ?
- [ ] File in `nvxapp.server.data\Entities\Tenant\GestionePresenze\`
- [ ] Namespace: `nvxapp.server.data.Entities.Tenant`
- [ ] Eredita da `BaseEntity`
- [ ] Ha campo `IdAz_Anagrafica` con `[Required]`
- [ ] Attributi di validazione appropriati

#### 2. Repository ?
- [ ] File in `nvxapp.server.data\Repositories\Tenant\GestionePresenze\`
- [ ] Interfaccia e implementazione nello stesso file
- [ ] Namespace: `nvxapp.server.data.Repositories.Tenant.GestionePresenze`
- [ ] Eredita da `Repository<ApplicationDbContext, TEntity>`
- [ ] Constructor con dependency injection corretto

#### 3. DbContext ?
- [ ] `DbSet<TEntity>` aggiunto in `ApplicationDbContext_GestionePresenze.cs`
- [ ] Nome proprietà uguale al nome entità
- [ ] Keyword `virtual` presente
- [ ] Migration generata e applicata

#### 4. Modelli DTO ?
- [ ] File in `nvxapp.server.service\ClientServer-Service\GestionePresenze\EntityService\Models\`
- [ ] Namespace corretto con **trattino**: `ClientServer-Service`
- [ ] Modelli per: Model, GetAllIn/Out, GetIn/Out, PutIn/Out, DeleteIn/Out
- [ ] OutModel eredita da `ModelResult`

#### 5. AutoMapper ?
- [ ] File in `nvxapp.server.service\Mappers\Tenant\GestionePresenze\`
- [ ] Due profile: Entity?Model e Model?Entity
- [ ] Namespace: `nvxapp.server.service.Mappers.Tenant.GestionePresenze`

#### 6. Service ?
- [ ] File in `nvxapp.server.service\ClientServer-Service\GestionePresenze\EntityService\`
- [ ] Eredita da `ServiceBase`
- [ ] Implementa interfaccia con metodi CRUD
- [ ] Constructor con parametri base + specifici
- [ ] Usa `ExecuteAction` per wrapping
- [ ] Gestione multi-tenancy con filtro `IdAz_Anagrafica`

#### 7. Controller ?
- [ ] File in `nvxapp.server\Controllers\Tenant\GestionePresenze\`
- [ ] Eredita da `NvxControllerBase`
- [ ] Route: `[Route("api/[controller]")]`
- [ ] Tutti gli endpoint con `[Authorize]`
- [ ] Endpoint: GetAll, EntityGet, EntityPut, EntityDelete

### Checklist Frontend

#### 8. TypeScript Models ?
- [ ] File in `nvxapp.client\src\nvx\ClientServer-Service\GestionePresenze\EntityService\Models\`
- [ ] Nome file kebab-case: `entity-model.ts`
- [ ] Stessi modelli del backend
- [ ] Constructor con inizializzazione
- [ ] OutModel estende `ModelResult`

#### 9. Angular Service ?
- [ ] File in `nvxapp.client\src\nvx\ClientServer-Service\GestionePresenze\EntityService\`
- [ ] Injectable con `providedIn: 'root'`
- [ ] Metodi corrispondenti al controller
- [ ] URL endpoint corretti
- [ ] `Observable` con `pipe(map())`

#### 10. Lista Component ?
- [ ] File in `nvxapp.client\src\nvx\pages\GestionePresenze\entity-list-page\`
- [ ] Implements `OnInit`
- [ ] Lifecycle hooks: `ionViewWillEnter`, `ionViewWillLeave`
- [ ] Properties: title, searchText, dataList, btnEdit
- [ ] Methods: Filter, isAdmin, getAll, handleButtonEditClick
- [ ] FabMenu configuration
- [ ] Navigation con state

#### 11. Lista Template ?
- [ ] Usa `app-page-toolbar` con filtro
- [ ] Pattern responsive: mobile sliding + desktop columns
- [ ] `*ngFor` con `genericFilter` pipe
- [ ] Button events collegati
- [ ] Ionic components standard

#### 12. Lista Module ?
- [ ] Import order corretto
- [ ] `SharedComponentInfrastructureModule` (non SharedModule!)
- [ ] Routing inline con `RouterModule.forChild`
- [ ] Solo `FormsModule` per liste

#### 13. Edit Component ?
- [ ] Estende `BasePageConfirmCancelComponent<TModel>`
- [ ] Abstract methods implementati: Title, EditForm, LoadData, SaveData
- [ ] Constructor con `protected override` per base params
- [ ] LoadData gestisce new/edit con `history.state`
- [ ] SaveData con error handling
- [ ] Form validation configurata

#### 14. Edit Template ?
- [ ] Usa `app-page-toolbar`
- [ ] Form con `[formGroup]="_editForm"`
- [ ] Validation messages per ogni campo
- [ ] **IMPORTANTE**: `[buttonbar]="buttonbar"` non btnConfirm/btnCancel

#### 15. Edit Module ?
- [ ] Import order corretto
- [ ] `FormsModule` E `ReactiveFormsModule`
- [ ] `SharedComponentInfrastructureModule`
- [ ] Routing inline

#### 16. Navigation ?
- [ ] Routes aggiunte in `route-attendance-tracking.service.ts`
- [ ] Menu aggiunto in `main-menu-attendance-tracking.service.ts`
- [ ] Path navigation corrisponde alle route
- [ ] RoleGuard appropriato

## ?? Debug Tools e Tecniche

### Browser DevTools

#### Network Tab
```javascript
// Verifica chiamate API
// Status 200 OK = Success
// Status 404 = Endpoint not found
// Status 401 = Unauthorized
// Status 500 = Server error

// Headers request:
// Authorization: Bearer <token>
// Content-Type: application/json
```

#### Console Errors
```javascript
// Errori comuni Angular:
// "Cannot read property 'data' of undefined" = Response null/undefined
// "Cannot find module" = Import path sbagliato
// "Component not found" = Module non importato correttamente
```

### Backend Debugging

#### Visual Studio Debug
```csharp
// Breakpoints utili:
// 1. Controller action entry point
// 2. Service method entry point  
// 3. Repository FindAll query
// 4. AutoMapper mapping

// Watch expressions utili:
// model.Data
// companyData.az_Anagrafica.Id
// _mapper.Map<T>(entity)
```

#### Logging
```csharp
// Aggiungi logging temporaneo
public async Task<GenericResult<OutModel>> GetAll(...)
{
    return await ExecuteAction(model, async () =>
    {
        // Debug logging
        Console.WriteLine($"Company ID: {this.CurrentCompany}");
        Console.WriteLine($"Entities count: {entities.Count}");
        
        // Business logic
        return retVal;
    }, isSubProcess);
}
```

### Frontend Debugging

#### Angular DevTools
```typescript
// Console debugging
console.log('Service call result:', res);
console.log('Data list:', this.dataList);
console.log('Navigation state:', history.state);

// Component properties inspection
// ng.probe($0).componentInstance nel browser console
```

#### Network Inspection
```typescript
// Verifica payload request/response
// POST body deve contenere:
{
    "data": {
        // InModel properties
    },
    "requestId": "uuid",
    "timestamp": "datetime"
}
```

## ??? Performance Optimization

### Backend Optimizations

```csharp
// ? Usa IQueryable per query grandi
var entities = _repository.FindAll(x => x.IdAz_Anagrafica == companyId)
    .OrderBy(x => x.Descrizione)
    .Take(100);  // Pagination

// ? Async/await corretto
await Task.Delay(DelayAsyncMethod);

// ? Dispose automatico con using (già gestito da Repository base)
```

### Frontend Optimizations

```typescript
// ? OnPush change detection per performance
@Component({
    selector: 'app-component',
    changeDetection: ChangeDetectionStrategy.OnPush  // Opzionale
})

// ? Unsubscribe automatico con async pipe
// Observable$ | async nel template invece di manual subscribe

// ? TrackBy per ngFor con liste grandi
<ion-row *ngFor="let item of getAll(); trackBy: trackByFn">

trackByFn(index: number, item: any): any {
    return item.id;
}
```

## ?? Supporto e Risorse

### Documentazione di Riferimento
- [Angular Official Docs](https://angular.io/docs)
- [Ionic Framework Docs](https://ionicframework.com/docs)
- [Entity Framework Core Docs](https://docs.microsoft.com/en-us/ef/core/)
- [AutoMapper Docs](https://docs.automapper.org/)

### Pattern Files di Riferimento nel Progetto
- **Entità**: `My_Template1.cs`
- **Service**: `My_Template1Service.cs`
- **Controller**: `My_Template1Controller.cs`
- **Lista Component**: `causali-list-page.component.ts`
- **Edit Component**: `mytemplate1-edit-page.component.ts`

### Quick Reference Commands

```bash
# Backend
Add-Migration Add_MyTableAI_A_Table
Update-Database
dotnet build

# Frontend  
ng build
ng serve
npm install
```

---

**?? Con queste guide complete, hai tutto il necessario per generare tabelle complete nel progetto nvxapp seguendo i pattern corretti!**