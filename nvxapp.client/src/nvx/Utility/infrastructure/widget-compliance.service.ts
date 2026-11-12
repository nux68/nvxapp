import { Injectable } from '@angular/core';
import { iWidgetProvider, WidgetDefinition } from './widget.model';
import { CmpDeadlinesWidgetComponent } from '../../shared/components/Compliance/widgets/cmp-deadlines-widget/cmp-deadlines-widget.component';
import { CmpTrainingWidgetComponent } from '../../shared/components/Compliance/widgets/cmp-training-widget/cmp-training-widget.component';
import { CmpGdprWidgetComponent } from '../../shared/components/Compliance/widgets/cmp-gdpr-widget/cmp-gdpr-widget.component';
import { CmpMineWidgetComponent } from '../../shared/components/Compliance/widgets/cmp-mine-widget/cmp-mine-widget.component';

// come le pagine Sicurezza sul lavoro e Registro GDPR (il server ricontrolla: CmpContesto.Amministratore)
const RUOLI_AMMINISTRATORI = ['CompanyPowerAdmin', 'CompanyAdmin'];

/*
 Widget della dashboard di Compliance (ANALISI_INFRASTRUCTURE_WIDGET.md, dettaglio in ANALISI_COMPLIANCE_WIDGET.md).
 Compaiono solo se Compliance e' attivo per l'azienda (WidgetService).
*/
@Injectable({
  providedIn: 'root'
})
export class WidgetComplianceService implements iWidgetProvider {
  public get Widgets(): WidgetDefinition[] {
    return [
      {
        // scadenze di sicurezza e GDPR per tipo
        key: 'compliance.deadlines',
        title: 'cmp.widgets.deadlines.title',
        description: 'cmp.widgets.deadlines.description',
        icon: 'shield-checkmark-outline',
        roles: RUOLI_AMMINISTRATORI,
        size: 'medium',
        default: true,
        zorder: 600100,
        component: CmpDeadlinesWidgetComponent
      },
      {
        // formazione obbligatoria richiesta dalle mansioni
        key: 'compliance.trainingStatus',
        title: 'cmp.widgets.training.title',
        description: 'cmp.widgets.training.description',
        icon: 'school-outline',
        roles: RUOLI_AMMINISTRATORI,
        size: 'small',
        default: false,
        zorder: 600200,
        component: CmpTrainingWidgetComponent
      },
      {
        // registro GDPR: violazioni da notificare, responsabili esterni, trattamenti
        key: 'compliance.gdpr',
        title: 'cmp.widgets.gdpr.title',
        description: 'cmp.widgets.gdpr.description',
        icon: 'lock-closed-outline',
        roles: RUOLI_AMMINISTRATORI,
        size: 'small',
        default: false,
        zorder: 600300,
        component: CmpGdprWidgetComponent
      },
      {
        // i propri corsi, DPI e visita: per chiunque in azienda sia collegato a un dipendente
        key: 'compliance.mine',
        title: 'cmp.widgets.mine.title',
        description: 'cmp.widgets.mine.description',
        icon: 'person-circle-outline',
        roles: ['CompanyPowerAdmin', 'CompanyAdmin', 'User'],
        size: 'small',
        default: true,
        zorder: 600400,
        component: CmpMineWidgetComponent
      }
    ];
  }
}
