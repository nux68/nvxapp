import { Injectable, Signal, signal } from '@angular/core';
import { combineLatest, debounceTime, Observable, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { ApplicationType } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { Dip_GG_Richiesta_Count_InModel, RichiestaNotifierData } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { DipGGRichiestaService } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { AuthService } from '../infrastructure/auth.service';
import { SignalrService } from '../infrastructure/signalr.service';

/*
 Contatori del menu "Richieste" (badge delle voci /requestlistadmin e /requestlistuser, vedi MainMenuAttendanceTrackingService):
   - daApprovare: richieste che l'utente (amministratore) puo' approvare adesso
   - inAttesa: richieste dell'utente in attesa di approvazione
 Si ricalcolano dopo il login (e l'impersonificazione) e all'evento SignalR "RichiestaNotifier";
 changed$: le pagine delle richieste aperte si ricaricano.
*/
@Injectable({
  providedIn: 'root'
})
export class RichiestaNotifierService {

  private readonly _daApprovare = signal(0);
  public readonly daApprovare: Signal<number> = this._daApprovare.asReadonly();

  private readonly _inAttesa = signal(0);
  public readonly inAttesa: Signal<number> = this._inAttesa.asReadonly();

  private readonly _changed = new Subject<RichiestaNotifierData>();
  public readonly changed$: Observable<RichiestaNotifierData> = this._changed.asObservable();

  constructor(private dipGGRichiestaService: DipGGRichiestaService,
              private authService: AuthService,
              signalrService: SignalrService) {

    if (environment.signalR.useSignalR) {
      // l'evento resta collegato anche alle connessioni successive (SignalrService.on)
      signalrService.on('RichiestaNotifier').subscribe((data: RichiestaNotifierData) => {
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
    if (!this.authService.Token || !this.authService.hasApplication(ApplicationType.AttendanceTracking)) {
      this._daApprovare.set(0);
      this._inAttesa.set(0);
      return;
    }

    const request = new GenericRequest<Dip_GG_Richiesta_Count_InModel>(Dip_GG_Richiesta_Count_InModel);
    request.data.admin = this.authService.IsInGroupCompanyAdmin;
    this.dipGGRichiestaService.RichiestaCount(request).subscribe(res => {
      this._daApprovare.set(res.success && res.data ? res.data.daApprovare : 0);
      this._inAttesa.set(res.success && res.data ? res.data.inAttesa : 0);
    });
  }
}
