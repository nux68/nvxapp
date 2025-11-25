# Guida alla Generazione Automatica di Tabelle - nvxapp

Questa guida descrive il processo completo per generare tutti i files necessari per una nuova tabella nel progetto nvxapp, seguendo rigorosamente i pattern e le convenzioni del framework.

## ?? Indice


1. [Prerequisiti e Setup](#prerequisiti-e-setup)
2. [Struttura del Progetto](#struttura-del-progetto)
3. [Generazione Backend](#generazione-backend)
4. [Generazione Frontend](#generazione-frontend)
5. [Pattern e Convenzioni](#pattern-e-convenzioni)
6. [Esempi Completi](#esempi-completi)
7. [Troubleshooting](#troubleshooting)

## ?? Prerequisiti e Setup

### Tecnologie Utilizzate
- **.NET 9** - Framework backend
- **Angular/Ionic** - Framework frontend
- **Entity Framework Core** - ORM per database
- **AutoMapper** - Mapping oggetti
- **PostgreSQL** - Database

### Branch di Lavoro
- **AttendanceTracking_Dev** - Branch per sviluppo GestionePresenze

### Struttura dei Files
```
nvxapp/
??? nvxapp.server/                    # Web API Controllers
??? nvxapp.server.data/              # Entità, Repository, DbContext
??? nvxapp.server.service/           # Business Logic, Services, DTOs
??? nvxapp.client/                   # Frontend Angular/Ionic
```

## ??? Struttura del Progetto

### Convenzioni di Naming
- **Tabelle**: `MyTableAI_1`, `MyTableAI_A` (PascalCase)
- **Files**: `mytableai-1-list-page` (kebab-case per Angular)
- **Cartelle**: Seguono la struttura esistente del progetto

### Pattern Architetturale
```
Database Table ? Entity ? Repository ? Service ? Controller ? API
                                                              ?
TypeScript Models ? Angular Service ? HTTP Client ? API Endpoint
       ?
Angular Components (List/Edit) ? UI Templates
```

## ?? Generazione Backend

### 1. Entità (Entity)

**Percorso**: `nvxapp.server.data\Entities\Tenant\GestionePresenze\`

```csharp
using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class MyTableAI_A : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        [MaxLength(500)]
        public string? Descrizione { get; set; }
    }
}
```

**? Pattern Obbligatori:**
- Eredita da `BaseEntity`
- Namespace `nvxapp.server.data.Entities.Tenant`
- Campo `IdAz_Anagrafica` obbligatorio per multi-tenancy
- Attributi di validazione appropriati

### 2. Repository

**Percorso**: `nvxapp.server.data\Repositories\Tenant\GestionePresenze\`

```csharp
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Interfaces;
using nvxapp.server.data.Infrastructure;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public interface IMyTableAI_ARepository : IRepository<MyTableAI_A>
    {
    }

    public class MyTableAI_ARepository : Repository<ApplicationDbContext, MyTableAI_A>, IMyTableAI_ARepository
    {
        public MyTableAI_ARepository(ApplicationDbContext context, IServiceProvider serviceProvider, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(context, serviceProvider, httpContextAccessor)
        {
        }
    }
}
```

**? Pattern Obbligatori:**
- Interfaccia e implementazione nello stesso file
- Eredita da `Repository<ApplicationDbContext, TEntity>`
- Constructor con dependency injection standard

### 3. Aggiornamento DbContext

**File**: `nvxapp.server.data\Infrastructure\ApplicationDbContext_GestionePresenze.cs`

```csharp
public virtual DbSet<MyTableAI_A> MyTableAI_A { get; set; }
```

**? Pattern Obbligatori:**
- Nome della proprietà uguale al nome dell'entità
- Keyword `virtual` per lazy loading
- Nessun plurale nel nome

### 4. Modelli DTO

**Percorso**: `nvxapp.server.service\ClientServer-Service\GestionePresenze\MyTableAI_AService\Models\`

```csharp
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.MyTableAI_AService.Models
{
    public class MyTableAI_AModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string Descrizione { get; set; } = string.Empty;
    }

    public class MyTableAI_A_GetAllInModel { }
    
    public class MyTableAI_A_GetAllOutModel : ModelResult
    {
        public List<MyTableAI_AModel> myTableAI_A { get; set; }
        public MyTableAI_A_GetAllOutModel()
        {
            myTableAI_A = new List<MyTableAI_AModel>();
        }
    }

    public class MyTableAI_A_GetInModel { public int Id { get; set; } }
    public class MyTableAI_A_GetOutModel : ModelResult
    {
        public MyTableAI_AModel myTableAI_A { get; set; } = new MyTableAI_AModel();
    }

    public class MyTableAI_A_PutInModel
    {
        public MyTableAI_AModel myTableAI_A { get; set; } = new MyTableAI_AModel();
    }
    public class MyTableAI_A_PutOutModel : ModelResult
    {
        public MyTableAI_AModel myTableAI_A { get; set; } = new MyTableAI_AModel();
    }

    public class MyTableAI_A_DeleteInModel { public int Id { get; set; } }
    public class MyTableAI_A_DeleteOutModel : ModelResult { }
}
```

**? Pattern Obbligatori:**
- Modelli separati per ogni operazione (Get, GetAll, Put, Delete)
- `OutModel` eredita da `ModelResult`
- Namespace con **trattino** non underscore: `ClientServer-Service`

### 5. AutoMapper Profile

**Percorso**: `nvxapp.server.service\Mappers\Tenant\GestionePresenze\`

```csharp
using AutoMapper;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.MyTableAI_AService.Models;

namespace nvxapp.server.service.Mappers.Tenant.GestionePresenze
{
    public class MyTableAI_A_To_MyTableAI_AModel_Mapper : Profile
    {
        public MyTableAI_A_To_MyTableAI_AModel_Mapper()
        {
            CreateMap<MyTableAI_A, MyTableAI_AModel>();
        }
    }

    public class MyTableAI_AModel_To_MyTableAI_A_Mapper : Profile
    {
        public MyTableAI_AModel_To_MyTableAI_A_Mapper()
        {
            CreateMap<MyTableAI_AModel, MyTableAI_A>();
        }
    }
}
```

**? Pattern Obbligatori:**
- Due profili separati per ogni direzione di mapping
- Seguire convenzione di naming dei profili esistenti

### 6. Service Business Logic

**Percorso**: `nvxapp.server.service\ClientServer-Service\GestionePresenze\MyTableAI_AService\`

```csharp
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.MyTableAI_AService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.MyTableAI_AService
{
    public class MyTableAI_AService : ServiceBase, IMyTableAI_AService
    {
        private readonly IMyTableAI_ARepository _myTableAI_ARepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public MyTableAI_AService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IMyTableAI_ARepository myTableAI_ARepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _myTableAI_ARepository = myTableAI_ARepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<MyTableAI_A_GetAllOutModel>> GetAll(GenericRequest<MyTableAI_A_GetAllInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                MyTableAI_A_GetAllOutModel retVal = new MyTableAI_A_GetAllOutModel();
                
                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);

                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica != null)
                {
                    var entities = _myTableAI_ARepository.FindAll(x => x.IdAz_Anagrafica == company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id).ToList();
                    retVal.myTableAI_A = _mapper.Map<List<MyTableAI_AModel>>(entities);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        // Altri metodi: MyTableAI_AGet, MyTableAI_APut, MyTableAI_ADelete
    }

    public interface IMyTableAI_AService : IServiceBase
    {
        public Task<GenericResult<MyTableAI_A_GetAllOutModel>> GetAll(GenericRequest<MyTableAI_A_GetAllInModel> model, Boolean isSubProcess);
        public Task<GenericResult<MyTableAI_A_GetOutModel>> MyTableAI_AGet(GenericRequest<MyTableAI_A_GetInModel> model, Boolean isSubProcess);
        public Task<GenericResult<MyTableAI_A_PutOutModel>> MyTableAI_APut(GenericRequest<MyTableAI_A_PutInModel> model, Boolean isSubProcess);
        public Task<GenericResult<MyTableAI_A_DeleteOutModel>> MyTableAI_ADelete(GenericRequest<MyTableAI_A_DeleteInModel> model, Boolean isSubProcess);
    }
}
```

**? Pattern Obbligatori:**
- Eredita da `ServiceBase`
- Implementa interfaccia con tutti i metodi CRUD
- Gestione multi-tenancy con `IdAz_Anagrafica`
- Uso di `ExecuteAction` per wrapping delle operazioni
- AutoMapper per conversioni
- Dependency injection standard

### 7. Controller API

**Percorso**: `nvxapp.server\Controllers\Tenant\GestionePresenze\`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.MyTableAI_AService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.MyTableAI_AService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    [ApiController]
    [Route("api/[controller]")]
    public class MyTableAI_AController : NvxControllerBase
    {
        private readonly IMyTableAI_AService _myTableAI_AService;

        public MyTableAI_AController(
            IHttpContextAccessor httpContextAccessor,
            IMyTableAI_AService myTableAI_AService
        ) : base(httpContextAccessor)
        {
            _myTableAI_AService = myTableAI_AService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<MyTableAI_A_GetAllOutModel>> GetAll(GenericRequest<MyTableAI_A_GetAllInModel> inModel)
        {
            var res = await _myTableAI_AService.GetAll(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("MyTableAI_AGet")]
        public async Task<GenericResult<MyTableAI_A_GetOutModel>> MyTableAI_AGet(GenericRequest<MyTableAI_A_GetInModel> inModel)
        {
            var res = await _myTableAI_AService.MyTableAI_AGet(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("MyTableAI_APut")]
        public async Task<GenericResult<MyTableAI_A_PutOutModel>> MyTableAI_APut(GenericRequest<MyTableAI_A_PutInModel> inModel)
        {
            var res = await _myTableAI_AService.MyTableAI_APut(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("MyTableAI_ADelete")]
        public async Task<GenericResult<MyTableAI_A_DeleteOutModel>> MyTableAI_ADelete(GenericRequest<MyTableAI_A_DeleteInModel> inModel)
        {
            var res = await _myTableAI_AService.MyTableAI_ADelete(inModel, false);
            return res;
        }
    }
}
```

**? Pattern Obbligatori:**
- Eredita da `NvxControllerBase`
- Tutti gli endpoint con `[Authorize]`
- Usa `[HttpPost]` anche per GET operations
- Route personalizzate per ogni operazione
- Dependency injection del service

---

## ?? Generazione Frontend

### 8. TypeScript Models

**Percorso**: `nvxapp.client\src\nvx\ClientServer-Service\GestionePresenze\MyTableAI_AService\Models\`

```typescript
import { ModelResult } from "../../../ModelsBase/model-result";

export class MyTableAI_AModel {
  public id: number;
  public idAz_Anagrafica!: number;
  public descrizione!: string; 

  constructor() {
    this.id = 0;
    this.idAz_Anagrafica = 0;
    this.descrizione = '';
  }
}

export class MyTableAI_A_GetAllInModel {
  
}

export class MyTableAI_A_GetAllOutModel extends ModelResult {
  public myTableAI_A: MyTableAI_AModel[] = [];
}

export class MyTableAI_A_GetInModel {
  public id: number = 0;
}

export class MyTableAI_A_GetOutModel extends ModelResult {
  public myTableAI_A: MyTableAI_AModel = new MyTableAI_AModel();
}

export class MyTableAI_A_PutInModel {
  public myTableAI_A: MyTableAI_AModel = new MyTableAI_AModel();
}

export class MyTableAI_A_PutOutModel extends ModelResult {
  public myTableAI_A: MyTableAI_AModel = new MyTableAI_AModel();
}

export class MyTableAI_A_DeleteInModel {
  public id: number = 0;
}

export class MyTableAI_A_DeleteOutModel extends ModelResult {
}
```

**? Pattern Obbligatori:**
- Stessi modelli del backend ma in TypeScript
- Constructor con inizializzazione
- `OutModel` estende `ModelResult`
- Naming identico ai modelli C#

### 9. Angular HTTP Service

**Percorso**: `nvxapp.client\src\nvx\ClientServer-Service\GestionePresenze\MyTableAI_AService\`

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
  MyTableAI_A_GetAllInModel,
  MyTableAI_A_GetAllOutModel,
  MyTableAI_A_GetInModel,
  MyTableAI_A_GetOutModel,
  MyTableAI_A_PutInModel,
  MyTableAI_A_PutOutModel,
  MyTableAI_A_DeleteInModel,
  MyTableAI_A_DeleteOutModel
} from './Models/mytableai-a-model';

@Injectable({
  providedIn: 'root'
})
export class MyTableAI_AService {

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<MyTableAI_A_GetAllInModel>): Observable<GenericResult<MyTableAI_A_GetAllOutModel>> {
    return this.http.post<GenericResult<MyTableAI_A_GetAllOutModel>>(
      environment.remoteData.apiUri + 'MyTableAI_A/GetAll', model
    ).pipe(map(r => r));
  }

  MyTableAI_AGet(model: GenericRequest<MyTableAI_A_GetInModel>): Observable<GenericResult<MyTableAI_A_GetOutModel>> {
    return this.http.post<GenericResult<MyTableAI_A_GetOutModel>>(
      environment.remoteData.apiUri + 'MyTableAI_A/MyTableAI_AGet', model
    ).pipe(map(r => r));
  }

  MyTableAI_APut(model: GenericRequest<MyTableAI_A_PutInModel>): Observable<GenericResult<MyTableAI_A_PutOutModel>> {
    return this.http.post<GenericResult<MyTableAI_A_PutOutModel>>(
      environment.remoteData.apiUri + 'MyTableAI_A/MyTableAI_APut', model
    ).pipe(map(r => r));
  }

  MyTableAI_ADelete(model: GenericRequest<MyTableAI_A_DeleteInModel>): Observable<GenericResult<MyTableAI_A_DeleteOutModel>> {
    return this.http.post<GenericResult<MyTableAI_A_DeleteOutModel>>(
      environment.remoteData.apiUri + 'MyTableAI_A/MyTableAI_ADelete', model
    ).pipe(map(r => r));
  }
}
```

**? Pattern Obbligatori:**
- Injectable service con `providedIn: 'root'`
- Usa `HttpClient` per chiamate API
- Endpoint URLs corrispondenti al controller
- `Observable` con `pipe(map())` pattern
- Import da `environment` per base URL

### 10. Lista Component

**Percorso**: `nvxapp.client\src\nvx\pages\GestionePresenze\mytableai-a-list-page\`

```typescript
import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { MyTableAI_A_GetAllInModel, MyTableAI_AModel } from '../../../ClientServer-Service/GestionePresenze/MyTableAI_AService/Models/mytableai-a-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { MyTableAI_AService } from '../../../ClientServer-Service/GestionePresenze/MyTableAI_AService/mytableai-a.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

@Component({
  selector: 'app-mytableai-a-list-page',
  templateUrl: './mytableai-a-list-page.component.html',
  styleUrls: ['./mytableai-a-list-page.component.scss'],
  standalone: false
})
export class MyTableAI_AListPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public myTableAI_AList: MyTableAI_AModel[] | null = null;
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              public fabMenuService: FabMenuService,
              private myTableAI_AService: MyTableAI_AService,
              private userInterfaceService: UserInterfaceService,
              private collectionDialogService: CollectionDialogService,
              private userNavigationService: UserNavigationService) {

    this.title = 'MyTableAI_A';
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {
    let request: GenericRequest<MyTableAI_A_GetAllInModel> = new GenericRequest<MyTableAI_A_GetAllInModel>(MyTableAI_A_GetAllInModel);
    this.myTableAI_AService.GetAll(request).subscribe(res => {
      this.myTableAI_AList = res.data.myTableAI_A;
    });

    this.fabMenuService.fabMenuItem = [
      new FabMenuItem('Nuovo MyTableAI_A', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/mytableaiaedit', {
          state: { id: 0 }
        });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  ngOnInit() { }

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/mytableaiaedit', {
      state: { id: item.id }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  isAdmin(item: any) {
    return false;
  }

  getAll() {
    return this.myTableAI_AList?.sort((a, b) => a.descrizione.localeCompare(b.descrizione));
  }
}
```

**? Pattern Obbligatori:**
- Implementa lifecycle hooks Ionic (`ionViewWillEnter`, `ionViewWillLeave`)
- Gestione `FabMenuService` per floating action button
- Pattern di navigation con `state`
- Metodi standard: `Filter`, `isAdmin`, `getAll`
- Dependency injection standard services

### 11. Lista Template HTML

```html
<app-page-toolbar [title]="title"
                  [showFilter]="true"
                  [showBreadcrumbs]="true"
                  (ev_Filter)="Filter($event)">
</app-page-toolbar>

<ion-content [fullscreen]="true">

  <ion-card>
    <ion-card-content>

      <ion-grid *ngIf="myTableAI_AList!=null">

        <ion-row *ngFor="let item of getAll() | genericFilter: 'descrizione' : searchText" class="nvx-ion-row-4table">

          <ion-item-sliding class="ion-hide-sm-up">
            <ion-item>
              <ion-icon [name]="isAdmin(item) ? 'server-outline' : 'server-outline'"></ion-icon>
              <ion-label [ngStyle]="{ color: isAdmin(item) ? 'var(--ion-color-primary)' : '' }">
                <h2>{{ item.descrizione }}</h2>
              </ion-label>
            </ion-item>

            <ion-item-options side="end">
              <ion-item-option color="{{btnEdit.color}}" (click)="btnEdit.event(item)">
                <ion-icon slot="start" name="{{btnEdit.image}}"></ion-icon>
                {{ btnEdit.text }}
              </ion-item-option>
            </ion-item-options>

          </ion-item-sliding>

          <!-- Desktop View -->
          <ion-col size="8" class="ion-hide-xs-down">
            <ion-icon [name]="isAdmin(item) ? 'server-outline' : 'server-outline'"></ion-icon>
            <ion-label [ngStyle]="{ color: isAdmin(item) ? 'var(--ion-color-primary)' : '' }">
              {{ item.descrizione }}
            </ion-label>
          </ion-col>
          <ion-col size="4" class="ion-hide-xs-down">
            <ion-button fill="clear" color="{{btnEdit.color}}" (click)="btnEdit.event(item)">
              <ion-icon slot="start" name="{{btnEdit.image}}"></ion-icon>
              {{ btnEdit.text }}
            </ion-button>
          </ion-col>

        </ion-row>

      </ion-grid>

    </ion-card-content>
  </ion-card>

</ion-content>
```

**? Pattern Obbligatori:**
- Usa `app-page-toolbar` component
- Pattern responsive: mobile sliding items + desktop columns
- `genericFilter` pipe per filtri
- Ionic component standard (`ion-card`, `ion-grid`, etc.)

### 12. Lista Module

```typescript
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { MyTableAI_AListPageComponent } from './mytableai-a-list-page.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: MyTableAI_AListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [MyTableAI_AListPageComponent]
})
export class MyTableAI_AListPageModule {}
```

**? Pattern Obbligatori:**
- Import `SharedComponentInfrastructureModule` (non SharedModule!)
- Routing inline con `RouterModule.forChild`
- Import order specifico del progetto

## ?? Checklist Completa

### Backend ?
- [ ] Entità creata in `Entities\Tenant\GestionePresenze\`
- [ ] Repository con interfaccia
- [ ] DbContext aggiornato
- [ ] Modelli DTO con pattern corretto
- [ ] AutoMapper profile
- [ ] Service con business logic
- [ ] Controller API con autorizzazione
- [ ] Migrazione database: `Add-Migration`, `Update-Database`

### Frontend ?
- [ ] Modelli TypeScript
- [ ] Servizio Angular HTTP
- [ ] Lista component con lifecycle
- [ ] Lista template responsive
- [ ] Lista module con import corretti
- [ ] Edit component con `BasePageConfirmCancelComponent`
- [ ] Edit template con `[buttonbar]="buttonbar"`
- [ ] Edit module con `ReactiveFormsModule`
- [ ] Routing aggiornato
- [ ] Menu navigation aggiornato

### Testing ?
- [ ] Build backend senza errori
- [ ] Build frontend senza errori
- [ ] Navigation funzionante
- [ ] CRUD operations testate
- [ ] Validazioni form

---

**?? Prossimo:** [Pattern e Convenzioni Dettagliate ?](./PATTERN_AND_CONVENTIONS.md)