
# Client

Questo documento fornisce linee guida per contribuire ai dati di nvxapp. 
Seguire queste istruzioni per garantire che i vostri contributi siano conformi agli standard del progetto.


FARE MOLTA ATTENZIONE !!! il codice client generato NON va nella cartella `nvxapp.client/src/app/nvx` ma nella cartella `nvxapp.client/src/nvx`


## Punti d'attenzione Client
 1. Tutto il codice scritto per il client (frontend) dovra essere scritto nella cartella `nvxapp.client/src/nvx`.
 2. 




|Cartella||
|||
|nvx/ClientServer-Service|Contine la logica dei service che dialogano con il back and|
|nvx/pages|Contine i form che compongono l'applicativo client|
|nvx/utility|Contine tutte le classi di utilità (menu,route, ecc.)|
|nvx/shared|Contine i componenti e le pipe che vengono utilizzati sul client|

Queste macro categorie vengono siddivise in sottocartelle a seconda del branch corrente (Infrastructure o GestionePresenze) a seconda 
che si stia sviluppando codice comune o codice specifico per la gestione presenze.


# Specifiche di sviluppo

## Models
La classi che definiscono i modelli per il client dovranno rispecchiare i models del server

Ecco un esempio di modelli per la tabella Par_Causali
```linguaggio
import { ModelResult } from "../../../ModelsBase/model-result";

export class Par_CausaliModel {
    public id!: number;
    public idAz_Anagrafica!: number;
    public descrizione!: string;
    public codice!: string;
}

export class Par_CausaliInModel {
}

export class Par_CausaliOutModel extends ModelResult {
    public par_Causali: Par_CausaliModel[];
}

export class Par_CausaliGetInModel {
    public id: number;
}

export class Par_CausaliGetOutModel extends ModelResult {
    public par_Causale: Par_CausaliModel;
}

export class Par_CausaliPutInModel {
    public par_Causale: Par_CausaliModel;
}

export class Par_CausaliPutOutModel extends ModelResult {
    public par_Causale: Par_CausaliModel;
}
```

## Punti d'attenzione Models
 1. Quando si definisce un nuovo modello importare sempre `import { ModelResult } from "../../../ModelsBase/model-result";`
 2. 



## Service
I service che dialogano con il back end dovranno essere posizionati nella cartella `nvxapp.client/src/app/nvx/ClientServer-Service/` a seconda del branch corrente (Infrastructure o GestionePresenze).

Ecco un esempio di service per la tabella ParCausali

```linguaggio
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_CausaliGetInModel, Par_CausaliGetOutModel, Par_CausaliInModel, Par_CausaliOutModel, Par_CausaliPutInModel, Par_CausaliPutOutModel } from './Models/par-causali-model';

@Injectable({
  providedIn: 'root'
})
export class ParCausaliService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Par_CausaliInModel>): Observable<GenericResult<Par_CausaliOutModel>> {

    return this.http.post<GenericResult<Par_CausaliOutModel>>(environment.remoteData.apiUri + 'Par_Causali/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  Par_CausaliGet(model: GenericRequest<Par_CausaliGetInModel>): Observable<GenericResult<Par_CausaliGetOutModel>> {

    return this.http.post<GenericResult<Par_CausaliGetOutModel>>(environment.remoteData.apiUri + 'Par_Causali/Par_CausaliGet', model)
      .pipe(
          map(r => {
            return r;
          }
        )
      );

  }

  Par_CausaliPut(model: GenericRequest<Par_CausaliPutInModel>): Observable<GenericResult<Par_CausaliPutOutModel>> {

    return this.http.post<GenericResult<Par_CausaliPutOutModel>>(environment.remoteData.apiUri + 'Par_Causali/Par_CausaliPut', model)
      .pipe(
          map(r => {
              return r;
            }
          )
      );

  }

}
```
## Punti d'attenzione Service
 1. Attenzione rispettare il path degli import ( prendere spunto dall esempio )
 2.



## Pagine
    
1) Tutte la pagine che espogono una toolbar  (che siano liste o form di inserimento dati) dovranno utilizzare il componente `app-page-toolbar` 


```linguaggio
<app-page-toolbar [title]="title"
                  [showFilter]="false"
                  [showBreadcrumbs]="true">
</app-page-toolbar>
```
2) La pagine di inserimento dati, per esporre i bottoni Conferma/Annulla dovranno utilizzare il componente `app-page-buttonbar`

```linguaggio
<app-page-buttonbar [buttonbar]="buttonbar"></app-page-buttonbar>
``` 

3) Le pagine di elenco dovranno essere collegate al menu di navigazione utilizzando il servizio MainMenuInfrastructureService o MainMenuAttendanceTrackingService a seconda del branch corrente.
```linguaggio
{ menuType: MenuType.MenuItem, zorder: 400, title: 'Centri', component: "DealerListPageComponent", url: '/dealerlist', icon: 'list-circle' }
``` 

4) Tutte la pagine create dovranno essere collegate al routing RouteInfrastructureService o RouteAttendanceTrackingService a seconda del branch corrente. 
   Esempio di una route di una pagina
```linguaggio
{ path: 'companyedit', loadChildren: () => import('../../pages/infrastructure/company-edit-page/company-edit-page.module').then(m => m.CompanyEditPageModule), canActivate: [RoleGuard4CompanyEdit] },
``` 


## Punti d'attenzione Page
 1. La cartella principale per le pagine è nvxapp.client\src\nvx\pages
 2. Se il branch corrente è Infrastructure posizionare le pagine in nvxapp.client\src\nvx\pages\infrastructure
 3. Se il branch corrente è GestionePresenze posizionare il service nvxapp.client\src\nvx\pages\GestionePresenze



## Elengo modelli di esempio per la generazione pagine client
- [Elenco 1](./CONTRIBUTING_nvxapp_client_Template_Elenco_1.md)
- [Modifica 1](./CONTRIBUTING_nvxapp_client_Template_Modifica_1.md)
- [Home](./CONTRIBUTING.md)