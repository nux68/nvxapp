
# Client

Questo documento fornisce linee guida per contribuire ai dati di nvxapp. 
Seguire queste istruzioni per garantire che i vostri contributi siano conformi agli standard del progetto.


## Folder Structure

Tutto il codice scritto per il client (frontend) dovra essere scritto nella cartella `nvxapp.client/src/app/nvx`.




|Cartella||
|||
|nvx/ClientServer-Service|Contine la logica dei service che dialogano con il back and|
|nvx/pages|Contine i form che compongono l'applicativo client|
|nvx/utility|Contine tutte le classi di utilità (menu,route, ecc.)|
|nvx/shared|Contine i componenti e le pipe che vengono utilizzati sul client|

Queste macro categorie vengono siddivise in sottocartelle a seconda del branch corrente (Infrastructure o GestionePresenze) a seconda 
che si stia sviluppando codice comune o codice specifico per la gestione presenze.


# Specifiche di sviluppo
## Service
I service che dialogano con il back end dovranno essere posizionati nella cartella `nvxapp.client/src/app/nvx/ClientServer-Service/` a seconda del branch corrente (Infrastructure o GestionePresenze).

```linguaggio
@Injectable({
  providedIn: 'root'
})
export class MyTabellaService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<MyTabellaGetInAllModel>): Observable<GenericResult<MyTabellaGetOutAllModel >> {

    return this.http.post<GenericResult<MyTabellaGetOutAllModel >>(environment.remoteData.apiUri + 'MyTabella/MyTabellaGetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  MyTabellaGet(model: GenericRequest<MyTabellaGetInModel>): Observable<GenericResult<MyTabellaGetOutModel>> {

    return this.http.post<GenericResult<MyTabellaGetOutModel>>(environment.remoteData.apiUri + 'MyTabella/MyTabellaGet', model)
      .pipe(
          map(r => {
            return r;
          }
        )
      );

  }

  MyTabellaPut(model: GenericRequest<MyTabellaPutInModel>): Observable<GenericResult<MyTabellaPutOutModel>> {

    return this.http.post<GenericResult<MyTabellaPutOutModel>>(environment.remoteData.apiUri + 'MyTabella/MyTabellaPut', model)
      .pipe(
          map(r => {
              return r;
            }
          )
      );

  }
  
  MyTabellaDelete(model: GenericRequest<MyTabellaDeleteInModel>): Observable<GenericResult<MyTabellaDeleteOutModel>> {

    return this.http.post<GenericResult<MyTabellaDeleteOutModel>>(environment.remoteData.apiUri + 'MyTabella/MyTabellaDelete', model)
      .pipe(
          map(r => {
              return r;
            }
          )
      );

  }

}
```



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


## Elengo modelli di esempio per la generazione pagine client
- [Elenco 1](./CONTRIBUTING_nvxapp_client_Template_Elenco_1.md)
- [Modifica 1](./CONTRIBUTING_nvxapp_client_Template_Modifica_1.md)
- [Home](./CONTRIBUTING.md)