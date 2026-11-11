import { Injectable } from '@angular/core';
import { iWidgetProvider, WidgetDefinition } from './widget.model';
import { PresentTodayWidgetComponent } from '../../shared/components/GestionePresenze/widgets/present-today-widget/present-today-widget.component';
import { RequestsWidgetComponent } from '../../shared/components/GestionePresenze/widgets/requests-widget/requests-widget.component';
import { DaysToFixWidgetComponent } from '../../shared/components/GestionePresenze/widgets/days-to-fix-widget/days-to-fix-widget.component';
import { AbsenceForecastWidgetComponent } from '../../shared/components/GestionePresenze/widgets/absence-forecast-widget/absence-forecast-widget.component';
import { JobHoursWidgetComponent } from '../../shared/components/GestionePresenze/widgets/job-hours-widget/job-hours-widget.component';

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
      },
      {
        // giornate da sistemare prima dell'export paghe (ANALISI_ATTENDANCETRACKING_ANOMALIE_AI.md)
        key: 'attendance.daysToFix',
        title: 'attendance.widgets.daysToFix.title',
        description: 'attendance.widgets.daysToFix.description',
        icon: 'alert-circle-outline',
        roles: ['CompanyPowerAdmin', 'CompanyAdmin'],
        size: 'small',
        default: false,
        zorder: 200300,
        component: DaysToFixWidgetComponent
      },
      {
        // assenze previste nei prossimi 7 giorni
        key: 'attendance.absenceForecast',
        title: 'attendance.widgets.absenceForecast.title',
        description: 'attendance.widgets.absenceForecast.description',
        icon: 'trending-up-outline',
        roles: ['CompanyPowerAdmin', 'CompanyAdmin'],
        size: 'small',
        default: false,
        zorder: 200400,
        component: AbsenceForecastWidgetComponent
      },
      {
        // ore sulle commesse del mese (ANALISI_ATTENDANCETRACKING_CONTROLLO_GESTIONE.md): come la pagina, solo CompanyPowerAdmin
        key: 'attendance.jobHours',
        title: 'attendance.widgets.jobHours.title',
        description: 'attendance.widgets.jobHours.description',
        icon: 'briefcase-outline',
        roles: ['CompanyPowerAdmin'],
        size: 'small',
        default: false,
        zorder: 200500,
        component: JobHoursWidgetComponent
      }
    ];
  }
}
