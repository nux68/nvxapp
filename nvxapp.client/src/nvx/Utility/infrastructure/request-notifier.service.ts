import { Injectable, Signal, signal } from '@angular/core';
import { combineLatest, debounceTime, Observable, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { RequestCountApp, RequestCountInModel, RequestNotifierData } from '../../ClientServer-Service/Infrastructure/Request/Models/request-model';
import { ApplicationType } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { RequestService } from '../../ClientServer-Service/Infrastructure/Request/request.service';
import { AuthService } from './auth.service';
import { SignalrService } from './signalr.service';
import { APPLICATIVI_RICHIESTE } from './request-ui.model';

/*
 Contatori delle richieste comuni a tutti gli applicativi (ANALISI_INFRASTRUCTURE_RICHIESTE.md), badge delle voci
 /requestsadmin e /requests (MainMenuInfrastructureService):
   - daApprovare: richieste che l'utente (amministratore) puo' approvare adesso
   - inAttesa: richieste dell'utente in attesa di decisione
   - daApprovareDi / inAttesaDi: gli stessi contatori di un solo applicativo (badge delle voci dei moduli, es. "Prenotazioni")
 Si ricalcolano dopo il login (e l'impersonificazione) e all'evento SignalR "RequestNotifier" mandato dai moduli;
 changed$: le pagine delle richieste aperte si ricaricano.
*/
@Injectable({
  providedIn: 'root'
})
export class RequestNotifierService {

  private readonly _daApprovare = signal(0);
  public readonly daApprovare: Signal<number> = this._daApprovare.asReadonly();

  private readonly _inAttesa = signal(0);
  public readonly inAttesa: Signal<number> = this._inAttesa.asReadonly();

  private readonly _perApplicativo = signal<RequestCountApp[]>([]);

  private readonly _changed = new Subject<RequestNotifierData>();
  public readonly changed$: Observable<RequestNotifierData> = this._changed.asObservable();

  constructor(private requestService: RequestService,
              private authService: AuthService,
              signalrService: SignalrService) {

    if (environment.signalR.useSignalR) {
      // l'evento resta collegato anche alle connessioni successive (SignalrService.on)
      signalrService.on('RequestNotifier').subscribe((data: RequestNotifierData) => {
        this._changed.next(data);
        this.refreshCount();
      });
    }

    // ruoli e applicativi arrivano insieme dopo il login: un solo ricalcolo
    combineLatest([this.authService.Roles$, this.authService.ActiveApplications$])
      .pipe(debounceTime(50))
      .subscribe(() => this.refreshCount());
  }

  public refreshCount(): void {
    if (!this.authService.Token || !APPLICATIVI_RICHIESTE.some(a => this.authService.hasApplication(a))) {
      this.set(0, 0);
      return;
    }

    this.requestService.RequestCount(new GenericRequest<RequestCountInModel>(RequestCountInModel)).subscribe(res => {
      this.set(res.success && res.data ? res.data.daApprovare : 0, res.success && res.data ? res.data.inAttesa : 0,
               res.success && res.data ? res.data.perApplicativo : []);
    });
  }

  /** contatori gia' noti (es. restituiti dall'elenco o da un'azione) */
  public set(daApprovare: number, inAttesa: number, perApplicativo?: RequestCountApp[] | null): void {
    this._daApprovare.set(daApprovare);
    this._inAttesa.set(inAttesa);
    this._perApplicativo.set(perApplicativo ?? []);
  }

  /** richieste di un applicativo che l'utente (amministratore) puo' approvare adesso */
  public daApprovareDi(application: ApplicationType): number {
    return this._perApplicativo().find(x => x.application === application)?.daApprovare ?? 0;
  }

  /** richieste dell'utente di un applicativo in attesa di decisione */
  public inAttesaDi(application: ApplicationType): number {
    return this._perApplicativo().find(x => x.application === application)?.inAttesa ?? 0;
  }
}
