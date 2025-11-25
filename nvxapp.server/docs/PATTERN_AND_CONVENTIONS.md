# Pattern e Convenzioni - nvxapp

Questo documento descrive in dettaglio tutti i pattern, convenzioni e best practices utilizzati nel progetto nvxapp per garantire coerenza e qualità del codice.

## ?? Pattern Architetturali

### Clean Architecture Layers

```
???????????????????????????????????????
?           Presentation              ?
?      (Angular Components)          ?
???????????????????????????????????????
?           Application               ?
?      (Angular Services)            ?
???????????????????????????????????????
?            Web API                  ?
?        (Controllers)               ?
???????????????????????????????????????
?          Business Logic             ?
?         (Services)                 ?
???????????????????????????????????????
?          Data Access               ?
?      (Repositories)                ?
???????????????????????????????????????
?          Database                  ?
?      (Entity Framework)           ?
???????????????????????????????????????
```

### Dependency Injection Pattern

**Backend (C#)**
```csharp
// Service Registration (automatica tramite convenzioni)
services.AddScoped<IMyTableAI_AService, MyTableAI_AService>();
services.AddScoped<IMyTableAI_ARepository, MyTableAI_ARepository>();

// Constructor Injection
public class MyTableAI_AService : ServiceBase
{
    private readonly IMyTableAI_ARepository _repository;
    private readonly IGestionePresenzeUserUtility _utility;
    
    public MyTableAI_AService(
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        // ... altri parametri base
        IMyTableAI_ARepository repository,
        IGestionePresenzeUserUtility utility) : base(...)
    {
        _repository = repository;
        _utility = utility;
    }
}
```

**Frontend (Angular)**
```typescript
@Injectable({ providedIn: 'root' })
export class MyTableAI_AService {
    constructor(
        private http: HttpClient,
        private authService: AuthService
    ) { }
}

// Component Injection
export class MyTableAI_AListPageComponent {
    constructor(
        private navCtrl: NavController,
        private fabMenuService: FabMenuService,
        private myTableAI_AService: MyTableAI_AService,
        private userInterfaceService: UserInterfaceService
    ) { }
}
```

## ??? Convenzioni di Naming

### Backend C# Naming
```csharp
// ? Corretto
public class MyTableAI_A : BaseEntity              // PascalCase per classi
public string Descrizione { get; set; }            // PascalCase per proprietà
private readonly IMyTableAI_ARepository _repository; // _camelCase per campi privati
public async Task<GenericResult<T>> GetAll(...)    // PascalCase per metodi

// ? Sbagliato
public class myTableAI_A                           // Minuscolo
public string descrizione                          // camelCase
private IMyTableAI_ARepository repository          // Senza underscore
```

### Frontend TypeScript Naming
```typescript
// ? Corretto
export class MyTableAI_AModel                     // PascalCase per classi
public descrizione!: string;                      // camelCase per proprietà
private myTableAI_AService: MyTableAI_AService;   // camelCase per campi

// ? Sbagliato
export class myTableAI_AModel                     // Minuscolo
public Descrizione!: string;                      // PascalCase (solo per C#)
```

### Files e Cartelle Naming
```
? Corretto:
mytableai-a-list-page.component.ts               # kebab-case
mytableai-a-edit-page.component.html             # kebab-case
MyTableAI_AService/                               # PascalCase per service folders
Models/mytableai-a-model.ts                      # kebab-case per file

? Sbagliato:
MyTableAI_AListPage.component.ts                 # PascalCase per file
mytableai_a_edit_page.component.html             # snake_case
myTableAIAService/                                # camelCase per folders
```

## ??? Pattern di Strutturazione

### Backend Service Pattern

```csharp
public class MyTableAI_AService : ServiceBase, IMyTableAI_AService
{
    // ? Constructor Pattern Standard
    public MyTableAI_AService(
        IMapper mapper,                    // Sempre primo per ServiceBase
        UserManager<ApplicationUser> userManager,
        IAspNetUsersRepository aspNetUsersRepository,
        IOptions<JwtParameter> jwtParameter,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        // Dipendenze specifiche sempre dopo quelle base
        IGestionePresenzeUserUtility gestionePresenzeUserUtility,
        IMyTableAI_ARepository myTableAI_ARepository) 
        : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
    {
        _myTableAI_ARepository = myTableAI_ARepository;
        _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
    }

    // ? CRUD Methods Pattern
    public virtual async Task<GenericResult<T_GetAllOutModel>> GetAll(GenericRequest<T_GetAllInModel> model, Boolean isSubProcess)
    {
        return await ExecuteAction(model, async () =>
        {
            // 1. Inizializza output model
            var retVal = new T_GetAllOutModel();
            
            // 2. Multi-tenancy check
            int IdCompany;
            int.TryParse(this.CurrentCompany, out IdCompany);
            var companyData = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
            
            // 3. Business logic
            if (companyData?.az_Anagrafica != null)
            {
                var entities = _repository.FindAll(x => x.IdAz_Anagrafica == companyData.az_Anagrafica.Id).ToList();
                retVal.data = _mapper.Map<List<TModel>>(entities);
            }
            
            // 4. Delay per consistency (testing)
            await Task.Delay(DelayAsyncMethod);
            return retVal;
        }, isSubProcess);
    }
}
```

### Frontend Angular Component Pattern

```typescript
export class MyTableAI_AListPageComponent implements OnInit {
    
    // ? Properties Pattern
    public title!: string;                    // UI properties
    public searchText!: string;               // Filter state
    public dataList: Model[] | null = null;   // Data properties
    public btnEdit: ButtonItem;               // UI buttons

    // ? Constructor Pattern
    constructor(
        // Core Ionic services sempre primi
        private navCtrl: NavController,
        // UI services
        public fabMenuService: FabMenuService,
        private userInterfaceService: UserInterfaceService,
        // Business services
        private myTableAI_AService: MyTableAI_AService,
        // Shared services sempre ultimi
        private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
    ) {
        // Inizializzazione UI
        this.title = 'MyTableAI_A';
        this.btnEdit = userInterfaceService.Btn_Modifica;
        this.btnEdit.event = this.handleButtonEditClick;
    }

    // ? Lifecycle Hooks Pattern
    ionViewWillEnter() {
        this.loadData();
        this.setupFabMenu();
    }

    ionViewWillLeave() {
        this.fabMenuService.fabMenuItem = [];
    }

    ngOnInit() { 
        // Subscribe a refresher se necessario
    }

    // ? Event Handlers Pattern
    handleButtonEditClick = (item: any) => {
        this.navCtrl.navigateForward('/mytableaiaedit', {
            state: { id: item.id }
        });
    }

    // ? Data Methods Pattern
    private loadData() {
        let request: GenericRequest<InModel> = new GenericRequest<InModel>(InModel);
        this.service.GetAll(request).subscribe(res => {
            this.dataList = res.data.collection;
        });
    }

    // ? Helper Methods Pattern
    Filter(CurrFilter: any) {
        this.searchText = CurrFilter;
    }

    getAll() {
        return this.dataList?.sort((a, b) => a.descrizione.localeCompare(b.descrizione));
    }
}
```

### Frontend Edit Component Pattern

```typescript
export class MyTableAI_AEditPageComponent extends BasePageConfirmCancelComponent<MyTableAI_AModel> {
    
    // ? Constructor per BasePageConfirmCancelComponent
    constructor(
        protected override navCtrl: NavController,
        protected override userInterfaceService: UserInterfaceService,
        protected override fb: FormBuilder,
        // Servizi specifici sempre dopo i protected override
        private stringHelperService: StringHelperService,
        private myTableAI_AService: MyTableAI_AService,
        private refresherService: RefresherService
    ) {
        super(navCtrl, userInterfaceService, fb);
    }

    // ? Abstract Methods Implementation
    get Title(): string { return "MyTableAI_A"; }
    
    get EditForm(): FormGroup {
        return this.fb.group({
            descrizione: [null, [Validators.required, Validators.maxLength(500)]],
        });
    }

    LoadData = (): Observable<MyTableAI_AModel | null> => {
        const state = history.state;
        if (state && state.id) {
            let request: GenericRequest<GetInModel> = new GenericRequest<GetInModel>(GetInModel);
            request.data.id = state.id;
            return this.service.Get(request).pipe(
                map((res) => res.data.item),
                catchError((error) => {
                    console.error('Errore durante la chiamata API:', error);
                    return [null];
                })
            );
        } else {
            // Nuovo record
            return new Observable<Model | null>((subscriber) => {
                subscriber.next(new Model());
                subscriber.complete();
            });
        }
    };

    SaveData = (editModel: MyTableAI_AModel): Observable<boolean> => {
        let request: GenericRequest<PutInModel> = new GenericRequest<PutInModel>(PutInModel);
        request.data.item = editModel;
        return this.service.Put(request).pipe(
            map(() => {
                this.refresherService.SharedParameterGestionePresenze_triggerRefresh();
                return true;
            }),
            catchError((error) => {
                console.error('Errore durante la chiamata API:', error);
                return [false];
            })
        );
    };
}
```

## ?? Security Pattern

### Authorization Pattern

**Controller Level**
```csharp
[ApiController]
[Route("api/[controller]")]
public class MyTableAI_AController : NvxControllerBase
{
    [Authorize]                    // ? Sempre su ogni action
    [HttpPost]                     // ? Usa POST anche per GET operations
    [Route("GetAll")]             // ? Route esplicita
    public async Task<GenericResult<OutModel>> GetAll(GenericRequest<InModel> inModel)
    {
        var res = await _service.GetAll(inModel, false);
        return res;                // ? Return diretto del GenericResult
    }
}
```

**Multi-Tenancy Pattern**
```csharp
// ? Sempre filtrare per IdAz_Anagrafica
var entities = _repository.FindAll(x => x.IdAz_Anagrafica == companyData.az_Anagrafica.Id);

// ? Mai query globali senza filtro tenancy
var entities = _repository.FindAll();
```

### Frontend Route Guards
```typescript
// ? Route con guard appropriato
{ path: 'mytableaialist', 
  loadChildren: () => import('./mytableai-a-list-page.module').then(m => m.Module), 
  canActivate: [RoleGuard4CompanyPowerAdmin] },

// Possibili guards:
// RoleGuard4User - Per utenti base
// RoleGuard4CompanyAdmin - Per admin azienda
// RoleGuard4CompanyPowerAdmin - Per super admin
// RoleGuard4JustClockRequest - Per richieste timbratura
```

## ?? UI/UX Pattern

### Template HTML Pattern

```html
<!-- ? Standard Page Structure -->
<app-page-toolbar [title]="title"
                  [showFilter]="true"
                  [showBreadcrumbs]="true"
                  (ev_Filter)="Filter($event)">
</app-page-toolbar>

<ion-content [fullscreen]="true">
  <ion-card>
    <ion-card-content>
      
      <!-- Lista con pattern responsive -->
      <ion-grid *ngIf="dataList!=null">
        <ion-row *ngFor="let item of getAll() | genericFilter: 'descrizione' : searchText" 
                 class="nvx-ion-row-4table">

          <!-- Mobile View -->
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

### Form Pattern per Edit Page

```html
<!-- ? Edit Form Structure -->
<app-page-toolbar [title]="Title"
                  [showFilter]="ShowFilter"
                  [showBreadcrumbs]="true">
</app-page-toolbar>

<ion-content>
  <ion-card>
    <ion-card-content>
      
      <br />
      <ion-item class="nvx-item-4-title">
        <ion-label>Info </ion-label>
      </ion-item>

      <form [formGroup]="_editForm">
        
        <ion-item>
          <ion-label position="floating">Descrizione</ion-label>
          <ion-textarea formControlName="descrizione" 
                       placeholder="Inserisci la descrizione"
                       [rows]="3">
          </ion-textarea>
        </ion-item>
        
        <!-- ? Validation Messages Pattern -->
        <ion-note color="danger" 
                  *ngIf="_editForm.get('descrizione')?.hasError('required') && 
                         _editForm.get('descrizione')?.touched">
          Descrizione è obbligatorio.
        </ion-note>
        <ion-note color="danger" 
                  *ngIf="_editForm.get('descrizione')?.hasError('maxlength') && 
                         _editForm.get('descrizione')?.touched">
          Descrizione non può superare i 500 caratteri.
        </ion-note>

      </form>

    </ion-card-content>
  </ion-card>
</ion-content>

<!-- ? IMPORTANTE: Non btnConfirm/btnCancel ma buttonbar -->
<app-page-buttonbar [buttonbar]="buttonbar">
</app-page-buttonbar>
```

## ?? Module Import Pattern

### Lista Page Module
```typescript
// ? Import Order corretto
import { NgModule } from '@angular/core';           // Angular core sempre primo
import { CommonModule } from '@angular/common';     // Angular common
import { FormsModule } from '@angular/forms';       // Forms module
import { IonicModule } from '@ionic/angular';       // Ionic module
import { RouterModule } from '@angular/router';     // Router module
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';  // Shared SEMPRE per ultimo
import { ComponentName } from './component';        // Component import per ultimo

@NgModule({
  imports: [
    CommonModule,
    FormsModule,                                     // Solo FormsModule per liste
    IonicModule,
    RouterModule.forChild([{                         // ? Routing INLINE
      path: '',
      component: ComponentName
    }]),
    SharedComponentInfrastructureModule,             // ? NON SharedModule!
  ],
  declarations: [ComponentName]
})
export class ModuleName {}
```

### Edit Page Module
```typescript
// ? Edit page ha anche ReactiveFormsModule
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';  // ? Entrambi!
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';
import { ComponentName } from './component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,                             // ? Necessario per reactive forms
    IonicModule,
    RouterModule.forChild([{
      path: '',
      component: ComponentName
    }]),
    SharedComponentInfrastructureModule
  ],
  declarations: [ComponentName],
})
export class ModuleName { }
```

## ?? Error Handling Pattern

### Backend Error Handling
```csharp
public virtual async Task<GenericResult<OutModel>> GetAll(GenericRequest<InModel> model, Boolean isSubProcess)
{
    return await ExecuteAction(model, async () =>     // ? ExecuteAction wrappa tutto
    {
        // Business logic qui
        // Gli errori sono gestiti automaticamente da ExecuteAction
        return result;
    }, isSubProcess);
}
```

### Frontend Error Handling
```typescript
// ? Pattern con catchError
LoadData = (): Observable<Model | null> => {
    if (state && state.id) {
        let request: GenericRequest<GetInModel> = new GenericRequest<GetInModel>(GetInModel);
        request.data.id = state.id;
        return this.service.Get(request).pipe(
            map((res) => res.data.item),
            catchError((error) => {
                console.error('Errore durante la chiamata API:', error);
                return [null];                        // ? Sempre return array con null
            })
        );
    } else {
        return new Observable<Model | null>((subscriber) => {
            subscriber.next(new Model());
            subscriber.complete();
        });
    }
};
```

## ?? Validation Pattern

### Backend Validation
```csharp
public class MyTableAI_A : BaseEntity
{
    [Required]                                       // ? Data annotations
    public int IdAz_Anagrafica { get; set; }
    
    [MaxLength(500)]
    public string? Descrizione { get; set; }
}
```

### Frontend Validation
```typescript
get EditForm(): FormGroup {
    return this.fb.group({
        descrizione: [null, [
            Validators.required,                     // ? Validatori standard
            Validators.maxLength(500)
        ]],
    });
}
```

## ?? Navigation Pattern

### Route Configuration
```typescript
// ? Route in route-attendance-tracking.service.ts
{ 
    path: 'mytableaialist', 
    loadChildren: () => import('../../pages/GestionePresenze/mytableai-a-list-page/mytableai-a-list-page.module').then(m => m.MyTableAI_AListPageModule), 
    canActivate: [RoleGuard4CompanyPowerAdmin] 
},
{ 
    path: 'mytableaiaedit', 
    loadChildren: () => import('../../pages/GestionePresenze/mytableai-a-edit-page/mytableai-a-edit-page.module').then(m => m.MyTableAI_AEditPageModule), 
    canActivate: [RoleGuard4CompanyPowerAdmin] 
}
```

### Menu Configuration
```typescript
// ? Menu in main-menu-attendance-tracking.service.ts
{ 
    menuType: MenuType.MenuItem, 
    zorder: 205660, 
    title: 'MyTableAI_A', 
    roles: ['CompanyPowerAdmin'], 
    component: "MyTableAI_AListPageComponent", 
    url: '/mytableaialist', 
    icon: 'ellipse' 
}
```

### Navigation Between Pages
```typescript
// ? Navigation con state
handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/mytableaiaedit', {
        state: { id: item.id }                      // ? Passa ID come state
    });
}

// ? Navigation per nuovo record
handleButtonNewClick = () => {
    this.navCtrl.navigateForward('/mytableaiaedit', {
        state: { id: 0 }                           // ? ID 0 per nuovo
    });
}

// ? Lettura state in edit page
LoadData = (): Observable<Model | null> => {
    const state = history.state;                   // ? Usa history.state
    if (state && state.id) {                       // ? Check esistenza state
        // Carica existing record
    } else {
        // Nuovo record
    }
}
```

---

**?? Prossimo:** [Troubleshooting e FAQ ?](./TROUBLESHOOTING.md)