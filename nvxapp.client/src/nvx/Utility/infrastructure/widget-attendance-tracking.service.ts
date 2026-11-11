import { Injectable } from '@angular/core';
import { iWidgetProvider, WidgetDefinition } from './widget.model';
import { PresentTodayWidgetComponent } from '../../shared/components/GestionePresenze/widgets/present-today-widget/present-today-widget.component';
import { RequestsWidgetComponent } from '../../shared/components/GestionePresenze/widgets/requests-widget/requests-widget.component';

/*
 Widget della dashboard delle Presenze (ANALISI_INFRASTRUCTURE_WIDGET.md, dettaglio in ANALISI_ATTENDANCETRACKING_WIDGET.md).
 Compaiono solo se le presenze sono attive per l'azienda (WidgetService).
*/
@Injectable({
  providedIn: 'root'
})
export class WidgetAttendanceTrackingService implements iWidgetProvider {
  public get Widgets(): WidgetDefinition[] {
    return [
      {
        key: 'attendance.presentToday',
        title: 'attendance.widgets.presentToday.title',
        description: 'attendance.widgets.presentToday.description',
        icon: 'people-circle-outline',
        // come la voce di menu "Personale presente"
        roles: ['CompanyPowerAdmin', 'CompanyAdmin'],
        size: 'small',
        default: true,
        zorder: 200100,
        component: PresentTodayWidgetComponent
      },
      {
        key: 'attendance.requests',
        title: 'attendance.widgets.requests.title',
        description: 'attendance.widgets.requests.description',
        icon: 'create-outline',
        // amministratori: da approvare; utenti: proprie richieste
        roles: ['CompanyPowerAdmin', 'CompanyAdmin', 'User'],
        size: 'small',
        default: true,
        zorder: 200200,
        component: RequestsWidgetComponent
      }
    ];
  }
}
