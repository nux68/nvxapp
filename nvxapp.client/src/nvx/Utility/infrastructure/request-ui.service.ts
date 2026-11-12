import { Injectable } from '@angular/core';
import { ApplicationType } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { AuthService } from './auth.service';
import { RequestNewItem, RequestUiAction } from './request-ui.model';
import { RequestUiAttendanceTrackingService } from './request-ui-attendance-tracking.service';
import { RequestUiAssetsService } from './request-ui-assets.service';

/*
 Unisce i fornitori dei rami per le pagine comuni delle richieste (ANALISI_INFRASTRUCTURE_RICHIESTE.md):
 tiene solo cio' che appartiene agli applicativi attivi dell'azienda e, per le pagine di invio, ai ruoli dell'utente.
*/
@Injectable({
  providedIn: 'root'
})
export class RequestUiService {

  constructor(private attendanceTracking: RequestUiAttendanceTrackingService,
              private assets: RequestUiAssetsService,
              private authService: AuthService) { }

  private get fornitori(): { application: ApplicationType, actions: RequestUiAction[], newRequests: RequestNewItem[] }[] {
    return [
      { application: ApplicationType.AttendanceTracking, actions: this.attendanceTracking.Actions, newRequests: this.attendanceTracking.NewRequests },
      { application: ApplicationType.Assets, actions: this.assets.Actions, newRequests: this.assets.NewRequests },
    ].filter(f => this.authService.hasApplication(f.application));
  }

  public get Actions(): RequestUiAction[] {
    return this.fornitori.flatMap(f => f.actions);
  }

  public get NewRequests(): RequestNewItem[] {
    return this.fornitori
      .flatMap(f => f.newRequests.map(n => ({ ...n, application: n.application ?? f.application })))
      .filter(n => !n.roles || n.roles.some(r => this.authService.hasRole(r)));
  }
}
