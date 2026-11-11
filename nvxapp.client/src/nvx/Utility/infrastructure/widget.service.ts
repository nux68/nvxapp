import { Injectable } from '@angular/core';
import { Observable, map, of, catchError } from 'rxjs';
import { ApplicationType } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { UserDashboardService } from '../../ClientServer-Service/Infrastructure/UserDashboard/user-dashboard.service';
import { UserDashboardGetInModel, UserDashboardPutInModel } from '../../ClientServer-Service/Infrastructure/UserDashboard/Models/user-dashboard-model';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { AuthService } from './auth.service';
import { WidgetDefinition } from './widget.model';
import { WidgetInfrastructureService } from './widget-infrastructure.service';
import { WidgetAttendanceTrackingService } from './widget-attendance-tracking.service';
import { WidgetCrmService } from './widget-crm.service';
import { WidgetDocumentManagementService } from './widget-document-management.service';
import { WidgetHumanResourcesService } from './widget-human-resources.service';
import { WidgetComplianceService } from './widget-compliance.service';
import { WidgetAssetsService } from './widget-assets.service';

/** stato della dashboard dell'utente corrente */
export interface DashboardStato {
  /** puo' configurare la propria dashboard (User, CompanyAdmin, CompanyPowerAdmin in un'azienda) */
  configurabile: boolean;
  /** ha gia' salvato una configurazione (altrimenti valgono i predefiniti) */
  configurata: boolean;
  /** chiavi salvate, nell'ordine (anche di widget oggi non disponibili) */
  chiavi: string[];
}

/*
 Catalogo dei widget (ANALISI_INFRASTRUCTURE_WIDGET.md): unisce i fornitori dei rami e tiene solo i widget utilizzabili
 dall'utente corrente:
   - dell'infrastruttura, oppure di un applicativo attivo per la sua azienda (AuthService.hasApplication,
     che tiene gia' conto delle licenze);
   - concessi ai suoi ruoli.
 La dashboard salvata contiene chiavi: quelle che il catalogo del ramo non conosce (altro ramo, applicativo
 disattivato, widget tolto) non si mostrano ma restano salvate, cosi' tornano se il widget torna disponibile.
*/
@Injectable({
  providedIn: 'root'
})
export class WidgetService {

  constructor(private widgetInfrastructureService: WidgetInfrastructureService,
              private widgetAttendanceTrackingService: WidgetAttendanceTrackingService,
              private widgetCrmService: WidgetCrmService,
              private widgetDocumentManagementService: WidgetDocumentManagementService,
              private widgetHumanResourcesService: WidgetHumanResourcesService,
              private widgetComplianceService: WidgetComplianceService,
              private widgetAssetsService: WidgetAssetsService,
              private userDashboardService: UserDashboardService,
              private authService: AuthService) { }

  /** tutti i widget del ramo compilato, ognuno con il suo applicativo */
  public get Catalogo(): WidgetDefinition[] {
    const conApplicativo = (widgets: WidgetDefinition[], application: ApplicationType) =>
      widgets.map(w => ({ ...w, application: w.application ?? application }));
    return [
      ...this.widgetInfrastructureService.Widgets,
      ...conApplicativo(this.widgetAttendanceTrackingService.Widgets, ApplicationType.AttendanceTracking),
      ...conApplicativo(this.widgetCrmService.Widgets, ApplicationType.Crm),
      ...conApplicativo(this.widgetDocumentManagementService.Widgets, ApplicationType.DocumentManagement),
      ...conApplicativo(this.widgetHumanResourcesService.Widgets, ApplicationType.HumanResources),
      ...conApplicativo(this.widgetComplianceService.Widgets, ApplicationType.Compliance),
      ...conApplicativo(this.widgetAssetsService.Widgets, ApplicationType.Assets),
    ].sort((a, b) => a.zorder - b.zorder);
  }

  /** widget che l'utente corrente puo' usare */
  public get Disponibili(): WidgetDefinition[] {
    const ruoli = this.authService.RoleNames;
    return this.Catalogo.filter(w =>
      (w.application == null || this.authService.hasApplication(w.application)) &&
      (!w.roles || w.roles.length === 0 || w.roles.some(r => ruoli.includes(r))));
  }

  /** widget predefiniti (dashboard mai configurata) */
  public get Predefiniti(): WidgetDefinition[] {
    return this.Disponibili.filter(w => w.default);
  }

  /** stato salvato sul server; in errore si comporta come una dashboard mai configurata */
  public Carica(): Observable<DashboardStato> {
    return this.userDashboardService.UserDashboardGet(new GenericRequest<UserDashboardGetInModel>(UserDashboardGetInModel)).pipe(
      map(res => res.success && res.data
        ? { configurabile: res.data.configurabile, configurata: res.data.configurata, chiavi: res.data.widgets ?? [] }
        : { configurabile: false, configurata: false, chiavi: [] }),
      catchError(() => of({ configurabile: false, configurata: false, chiavi: [] })));
  }

  /** widget da mostrare, nell'ordine scelto: i predefiniti se la dashboard non e' mai stata configurata */
  public Dashboard(stato: DashboardStato): WidgetDefinition[] {
    if (!stato.configurata)
      return this.Predefiniti;
    const disponibili = this.Disponibili;
    return stato.chiavi
      .map(k => disponibili.find(w => w.key === k))
      .filter((w): w is WidgetDefinition => !!w);
  }

  /** salva l'elenco (chiavi nell'ordine); restituisce lo stato salvato, null in caso di errore */
  public Salva(chiavi: string[]): Observable<DashboardStato | null> {
    const request = new GenericRequest<UserDashboardPutInModel>(UserDashboardPutInModel);
    request.data.widgets = chiavi;
    return this.userDashboardService.UserDashboardPut(request).pipe(
      map(res => res.success && res.data
        ? { configurabile: res.data.configurabile, configurata: res.data.configurata, chiavi: res.data.widgets ?? [] }
        : null));
  }

  /** pagina in cui l'utente corrente configura la dashboard (la pagina del suo ruolo), null se non puo' */
  public get PaginaConfigurazione(): string | null {
    if (this.authService.IsCompanyPowerAdmin) return '/companypoweradmin';
    if (this.authService.IsCompanyAdmin) return '/companyadmin';
    if (this.authService.IsUser) return '/user';
    return null;
  }
}
