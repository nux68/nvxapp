# Quick Reference - Comandi e Template nvxapp

Questa guida fornisce un riferimento rapido con tutti i template di codice e comandi necessari per generare tabelle nel progetto nvxapp.

## ?? Comandi Rapidi


### Setup Iniziale
```bash
# Clone repository
git clone https://github.com/nux68/nvxapp
cd nvxapp
git checkout AttendanceTracking_Dev

# Backend build
dotnet restore
dotnet build

# Frontend setup
cd nvxapp.client
npm install
ng serve
```

### Database Commands
```bash
# Crea migration
Add-Migration Add_MyTableAI_A_Table

# Applica migration
Update-Database

# Verifica migrations
Get-Migration
```

### Build Commands
```bash
# Backend
dotnet build
dotnet run

# Frontend
ng build
ng serve
ng build --prod
```

## ?? Struttura Files Template

### Backend Files Structure
```
nvxapp.server.data/
??? Entities/Tenant/GestionePresenze/
?   ??? MyTableAI_A.cs
??? Repositories/Tenant/GestionePresenze/
?   ??? MyTableAI_ARepository.cs
??? Infrastructure/
    ??? ApplicationDbContext_GestionePresenze.cs (update)

nvxapp.server.service/
??? ClientServer-Service/GestionePresenze/MyTableAI_AService/
?   ??? Models/
?   ?   ??? MyTableAI_AModel.cs
?   ??? MyTableAI_AService.cs
??? Mappers/Tenant/GestionePresenze/
    ??? MyTableAI_A_AND_MyTableAI_AModel_Mapper.cs

nvxapp.server/
??? Controllers/Tenant/GestionePresenze/
    ??? MyTableAI_AController.cs
```

### Frontend Files Structure
```
nvxapp.client/src/nvx/
??? ClientServer-Service/GestionePresenze/MyTableAI_AService/
?   ??? Models/
?   ?   ??? mytableai-a-model.ts
?   ??? mytableai-a.service.ts
??? pages/GestionePresenze/
?   ??? mytableai-a-list-page/
?   ?   ??? mytableai-a-list-page.component.ts
?   ?   ??? mytableai-a-list-page.component.html
?   ?   ??? mytableai-a-list-page.component.scss
?   ?   ??? mytableai-a-list-page.module.ts
?   ??? mytableai-a-edit-page/
?       ??? mytableai-a-edit-page.component.ts
?       ??? mytableai-a-edit-page.component.html
?       ??? mytableai-a-edit-page.component.scss
?       ??? mytableai-a-edit-page.module.ts
??? Utility/infrastructure/
    ??? route-attendance-tracking.service.ts (update)
    ??? main-menu-attendance-tracking.service.ts (update)
```

## ?? Template Codice

### 1. Entità Template
```csharp
using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class {{ENTITY_NAME}} : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        
        [MaxLength({{MAX_LENGTH}})]
        public string? {{PROPERTY_NAME}} { get; set; }
    }
}
```

### 2. Repository Template
```csharp
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Interfaces;
using nvxapp.server.data.Infrastructure;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public interface I{{ENTITY_NAME}}Repository : IRepository<{{ENTITY_NAME}}>
    {
    }

    public class {{ENTITY_NAME}}Repository : Repository<ApplicationDbContext, {{ENTITY_NAME}}>, I{{ENTITY_NAME}}Repository
    {
        public {{ENTITY_NAME}}Repository(ApplicationDbContext context, IServiceProvider serviceProvider, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(context, serviceProvider, httpContextAccessor)
        {
        }
    }
}
```

### 3. Service Template
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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.{{ENTITY_NAME}}Service.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.{{ENTITY_NAME}}Service
{
    public class {{ENTITY_NAME}}Service : ServiceBase, I{{ENTITY_NAME}}Service
    {
        private readonly I{{ENTITY_NAME}}Repository _{{ENTITY_NAME_LOWER}}Repository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public {{ENTITY_NAME}}Service(
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IAspNetUsersRepository aspNetUsersRepository,
            IOptions<JwtParameter> jwtParameter,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IGestionePresenzeUserUtility gestionePresenzeUserUtility,
            I{{ENTITY_NAME}}Repository {{ENTITY_NAME_LOWER}}Repository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _{{ENTITY_NAME_LOWER}}Repository = {{ENTITY_NAME_LOWER}}Repository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<{{ENTITY_NAME}}_GetAllOutModel>> GetAll(GenericRequest<{{ENTITY_NAME}}_GetAllInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                {{ENTITY_NAME}}_GetAllOutModel retVal = new {{ENTITY_NAME}}_GetAllOutModel();
                
                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);

                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica != null)
                {
                    var entities = _{{ENTITY_NAME_LOWER}}Repository.FindAll(x => x.IdAz_Anagrafica == company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id).ToList();
                    retVal.{{ENTITY_NAME_LOWER}} = _mapper.Map<List<{{ENTITY_NAME}}Model>>(entities);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        // Altri metodi: Get, Put, Delete
    }

    public interface I{{ENTITY_NAME}}Service : IServiceBase
    {
        Task<GenericResult<{{ENTITY_NAME}}_GetAllOutModel>> GetAll(GenericRequest<{{ENTITY_NAME}}_GetAllInModel> model, Boolean isSubProcess);
        Task<GenericResult<{{ENTITY_NAME}}_GetOutModel>> {{ENTITY_NAME}}Get(GenericRequest<{{ENTITY_NAME}}_GetInModel> model, Boolean isSubProcess);
        Task<GenericResult<{{ENTITY_NAME}}_PutOutModel>> {{ENTITY_NAME}}Put(GenericRequest<{{ENTITY_NAME}}_PutInModel> model, Boolean isSubProcess);
        Task<GenericResult<{{ENTITY_NAME}}_DeleteOutModel>> {{ENTITY_NAME}}Delete(GenericRequest<{{ENTITY_NAME}}_DeleteInModel> model, Boolean isSubProcess);
    }
}
```

### 4. Angular Service Template
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
  {{ENTITY_NAME}}_GetAllInModel,
  {{ENTITY_NAME}}_GetAllOutModel,
  {{ENTITY_NAME}}_GetInModel,
  {{ENTITY_NAME}}_GetOutModel,
  {{ENTITY_NAME}}_PutInModel,
  {{ENTITY_NAME}}_PutOutModel,
  {{ENTITY_NAME}}_DeleteInModel,
  {{ENTITY_NAME}}_DeleteOutModel
} from './Models/{{ENTITY_NAME_KEBAB}}-model';

@Injectable({
  providedIn: 'root'
})
export class {{ENTITY_NAME}}Service {

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<{{ENTITY_NAME}}_GetAllInModel>): Observable<GenericResult<{{ENTITY_NAME}}_GetAllOutModel>> {
    return this.http.post<GenericResult<{{ENTITY_NAME}}_GetAllOutModel>>(
      environment.remoteData.apiUri + '{{ENTITY_NAME}}/GetAll', model
    ).pipe(map(r => r));
  }

  {{ENTITY_NAME}}Get(model: GenericRequest<{{ENTITY_NAME}}_GetInModel>): Observable<GenericResult<{{ENTITY_NAME}}_GetOutModel>> {
    return this.http.post<GenericResult<{{ENTITY_NAME}}_GetOutModel>>(
      environment.remoteData.apiUri + '{{ENTITY_NAME}}/{{ENTITY_NAME}}Get', model
    ).pipe(map(r => r));
  }

  {{ENTITY_NAME}}Put(model: GenericRequest<{{ENTITY_NAME}}_PutInModel>): Observable<GenericResult<{{ENTITY_NAME}}_PutOutModel>> {
    return this.http.post<GenericResult<{{ENTITY_NAME}}_PutOutModel>>(
      environment.remoteData.apiUri + '{{ENTITY_NAME}}/{{ENTITY_NAME}}Put', model
    ).pipe(map(r => r));
  }

  {{ENTITY_NAME}}Delete(model: GenericRequest<{{ENTITY_NAME}}_DeleteInModel>): Observable<GenericResult<{{ENTITY_NAME}}_DeleteOutModel>> {
    return this.http.post<GenericResult<{{ENTITY_NAME}}_DeleteOutModel>>(
      environment.remoteData.apiUri + '{{ENTITY_NAME}}/{{ENTITY_NAME}}Delete', model
    ).pipe(map(r => r));
  }
}
```

### 5. Angular Lista Component Template
```typescript
import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { {{ENTITY_NAME}}_GetAllInModel, {{ENTITY_NAME}}Model } from '../../../ClientServer-Service/GestionePresenze/{{ENTITY_NAME}}Service/Models/{{ENTITY_NAME_KEBAB}}-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { {{ENTITY_NAME}}Service } from '../../../ClientServer-Service/GestionePresenze/{{ENTITY_NAME}}Service/{{ENTITY_NAME_KEBAB}}.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

@Component({
  selector: 'app-{{ENTITY_NAME_KEBAB}}-list-page',
  templateUrl: './{{ENTITY_NAME_KEBAB}}-list-page.component.html',
  styleUrls: ['./{{ENTITY_NAME_KEBAB}}-list-page.component.scss'],
  standalone: false
})
export class {{ENTITY_NAME}}ListPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public {{ENTITY_NAME_LOWER}}List: {{ENTITY_NAME}}Model[] | null = null;
  public btnEdit: ButtonItem;

  constructor(
    private navCtrl: NavController,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    public fabMenuService: FabMenuService,
    private {{ENTITY_NAME_LOWER}}Service: {{ENTITY_NAME}}Service,
    private userInterfaceService: UserInterfaceService,
    private collectionDialogService: CollectionDialogService,
    private userNavigationService: UserNavigationService) {

    this.title = '{{ENTITY_DISPLAY_NAME}}';
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {
    let request: GenericRequest<{{ENTITY_NAME}}_GetAllInModel> = new GenericRequest<{{ENTITY_NAME}}_GetAllInModel>({{ENTITY_NAME}}_GetAllInModel);
    this.{{ENTITY_NAME_LOWER}}Service.GetAll(request).subscribe(res => {
      this.{{ENTITY_NAME_LOWER}}List = res.data.{{ENTITY_NAME_LOWER}};
    });

    this.fabMenuService.fabMenuItem = [
      new FabMenuItem('Nuovo {{ENTITY_DISPLAY_NAME}}', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/{{ROUTE_EDIT}}', {
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
    this.navCtrl.navigateForward('/{{ROUTE_EDIT}}', {
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
    return this.{{ENTITY_NAME_LOWER}}List?.sort((a, b) => a.{{SORT_FIELD}}.localeCompare(b.{{SORT_FIELD}}));
  }
}
```

## ?? Script di Generazione Automatica

### PowerShell Script per Backend
```powershell
# GenerateBackend.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$EntityName,
    
    [Parameter(Mandatory=$true)]
    [string]$PropertyName,
    
    [Parameter(Mandatory=$false)]
    [int]$MaxLength = 500
)

$EntityNameLower = $EntityName.ToLower()
$EntityNameKebab = $EntityName -creplace '([A-Z])', '-$1' | ForEach-Object { $_.Trim('-').ToLower() }

# Generate Entity
$EntityContent = @"
using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class $EntityName : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        
        [MaxLength($MaxLength)]
        public string? $PropertyName { get; set; }
    }
}
"@

New-Item -Path "nvxapp.server.data\Entities\Tenant\GestionePresenze\$EntityName.cs" -Value $EntityContent -Force

Write-Host "? Generated Entity: $EntityName.cs"
Write-Host "?? Next steps:"
Write-Host "   1. Update ApplicationDbContext_GestionePresenze.cs"
Write-Host "   2. Run: Add-Migration Add_$EntityName"
Write-Host "   3. Run: Update-Database"
```

### Node.js Script per Frontend
```javascript
// generate-frontend.js
const fs = require('fs');
const path = require('path');

function generateFrontend(entityName, propertyName) {
    const entityNameLower = entityName.toLowerCase();
    const entityNameKebab = entityName.replace(/([A-Z])/g, '-$1').toLowerCase().substring(1);
    
    // Generate Angular Service
    const serviceContent = `
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
// ... rest of template
`;

    const serviceDir = `nvxapp.client/src/nvx/ClientServer-Service/GestionePresenze/${entityName}Service`;
    
    if (!fs.existsSync(serviceDir)) {
        fs.mkdirSync(serviceDir, { recursive: true });
    }
    
    fs.writeFileSync(path.join(serviceDir, `${entityNameKebab}.service.ts`), serviceContent);
    
    console.log(`? Generated Service: ${entityNameKebab}.service.ts`);
}

// Usage: node generate-frontend.js MyTableAI_B Descrizione
const entityName = process.argv[2];
const propertyName = process.argv[3];

if (!entityName || !propertyName) {
    console.log('Usage: node generate-frontend.js <EntityName> <PropertyName>');
    process.exit(1);
}

generateFrontend(entityName, propertyName);
```

## ? Shortcuts e Alias

### PowerShell Aliases
```powershell
# Aggiungi al tuo PowerShell Profile ($PROFILE)

# Backend shortcuts
function New-Migration { param($name) Add-Migration $name }
function Update-DB { Update-Database }
function Build-Backend { dotnet build }

# Frontend shortcuts
function Start-Client { cd nvxapp.client; ng serve }
function Build-Client { cd nvxapp.client; ng build }
function Test-Client { cd nvxapp.client; ng test }

# Git shortcuts
function Git-Status { git status }
function Git-Pull { git pull origin AttendanceTracking_Dev }
function Git-Push { git push origin AttendanceTracking_Dev }

Set-Alias -Name nm -Value New-Migration
Set-Alias -Name udb -Value Update-DB
Set-Alias -Name bb -Value Build-Backend
Set-Alias -Name sc -Value Start-Client
Set-Alias -Name bc -Value Build-Client
Set-Alias -Name gs -Value Git-Status
Set-Alias -Name gp -Value Git-Pull
```

### VS Code Snippets
```json
// .vscode/snippets/csharp.json
{
    "Entity Template": {
        "prefix": "nvx-entity",
        "body": [
            "using nvxapp.server.data.Entities.Public;",
            "using System.ComponentModel.DataAnnotations;",
            "",
            "namespace nvxapp.server.data.Entities.Tenant",
            "{",
            "    public class ${1:EntityName} : BaseEntity",
            "    {",
            "        [Required]",
            "        public int IdAz_Anagrafica { get; set; }",
            "        ",
            "        [MaxLength(${2:500})]",
            "        public string? ${3:PropertyName} { get; set; }",
            "    }",
            "}"
        ],
        "description": "Generate nvxapp entity template"
    }
}

// .vscode/snippets/typescript.json
{
    "Angular Service Template": {
        "prefix": "nvx-service",
        "body": [
            "import { Injectable } from '@angular/core';",
            "import { HttpClient } from '@angular/common/http';",
            "import { AuthService } from '../../../Utility/infrastructure/auth.service';",
            "import { environment } from '../../../../environments/environment';",
            "",
            "@Injectable({",
            "  providedIn: 'root'",
            "})",
            "export class ${1:EntityName}Service {",
            "",
            "  constructor(",
            "    private http: HttpClient,",
            "    private authService: AuthService",
            "  ) { }",
            "",
            "  // Methods here",
            "}"
        ],
        "description": "Generate nvxapp Angular service template"
    }
}
```

## ?? Variabili di Sostituzione

### Template Variables Reference
| Variabile | Descrizione | Esempio |
|-----------|-------------|---------|
| `{{ENTITY_NAME}}` | Nome entità PascalCase | `MyTableAI_A` |
| `{{ENTITY_NAME_LOWER}}` | Nome entità lowercase | `mytableai_a` |
| `{{ENTITY_NAME_KEBAB}}` | Nome entità kebab-case | `mytableai-a` |
| `{{PROPERTY_NAME}}` | Nome proprietà PascalCase | `Descrizione` |
| `{{PROPERTY_NAME_LOWER}}` | Nome proprietà camelCase | `descrizione` |
| `{{MAX_LENGTH}}` | Lunghezza massima campo | `500` |
| `{{ENTITY_DISPLAY_NAME}}` | Nome visualizzato UI | `My Table AI A` |
| `{{ROUTE_LIST}}` | Route per lista | `mytableaialist` |
| `{{ROUTE_EDIT}}` | Route per edit | `mytableaiaedit` |
| `{{SORT_FIELD}}` | Campo per ordinamento | `descrizione` |
| `{{MENU_ZORDER}}` | Ordine nel menu | `205670` |

### Naming Convention Conversion
```javascript
// Utility functions per conversioni
function toPascalCase(str) {
    return str.charAt(0).toUpperCase() + str.slice(1);
}

function toCamelCase(str) {
    return str.charAt(0).toLowerCase() + str.slice(1);
}

function toKebabCase(str) {
    return str.replace(/([A-Z])/g, '-$1').toLowerCase().substring(1);
}

function toSnakeCase(str) {
    return str.replace(/([A-Z])/g, '_$1').toLowerCase().substring(1);
}

// Esempi:
// MyTableAI_A -> mytableai-a (kebab)
// MyTableAI_A -> myTableAI_A (camel)  
// MyTableAI_A -> mytableai_a (snake)
```

---

**? Con questi template e scripts, puoi generare rapidamente tutte le componenti necessarie per una nuova tabella nel progetto nvxapp!**