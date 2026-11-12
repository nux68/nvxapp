import { Injectable } from '@angular/core';
import { iWidgetProvider, WidgetDefinition } from './widget.model';
import { HrMyPayslipsWidgetComponent } from '../../shared/components/HumanResources/widgets/hr-my-payslips-widget/hr-my-payslips-widget.component';
import { HrOnboardingWidgetComponent } from '../../shared/components/HumanResources/widgets/hr-onboarding-widget/hr-onboarding-widget.component';
import { HrTrainingExpiringWidgetComponent } from '../../shared/components/HumanResources/widgets/hr-training-expiring-widget/hr-training-expiring-widget.component';

// ufficio del personale (come le voci di menu "Checklist" e "Corsi" e i controlli del server, HrUtility.AmministratoreAsync)
const RUOLI_PERSONALE = ['CompanyPowerAdmin', 'CompanyAdmin'];

/*
 Widget della dashboard di Risorse Umane (ANALISI_INFRASTRUCTURE_WIDGET.md, dettaglio in ANALISI_HUMANRESOURCES_WIDGET.md).
 Compaiono solo se Risorse Umane e' attivo per l'azienda (WidgetService).
*/
@Injectable({
  providedIn: 'root'
})
export class WidgetHumanResourcesService implements iWidgetProvider {
  public get Widgets(): WidgetDefinition[] {
    return [
      {
        // area del dipendente: solo i propri documenti (come la voce di menu "I miei cedolini", per ogni ruolo dell'azienda)
        key: 'hr.myPayslips',
        title: 'hr.widgets.myPayslips.title',
        description: 'hr.widgets.myPayslips.description',
        icon: 'wallet-outline',
        roles: ['CompanyPowerAdmin', 'CompanyAdmin', 'User'],
        size: 'small',
        default: true,
        zorder: 500100,
        component: HrMyPayslipsWidgetComponent
      },
      {
        // checklist di assunzione e cessazione aperte e voci da fare
        key: 'hr.onboarding',
        title: 'hr.widgets.onboarding.title',
        description: 'hr.widgets.onboarding.description',
        icon: 'checkbox-outline',
        roles: RUOLI_PERSONALE,
        size: 'medium',
        default: false,
        zorder: 500200,
        component: HrOnboardingWidgetComponent
      },
      {
        // attestati dei corsi scaduti o in scadenza
        key: 'hr.trainingExpiring',
        title: 'hr.widgets.trainingExpiring.title',
        description: 'hr.widgets.trainingExpiring.description',
        icon: 'school-outline',
        roles: RUOLI_PERSONALE,
        size: 'medium',
        default: false,
        zorder: 500300,
        component: HrTrainingExpiringWidgetComponent
      }
    ];
  }
}
