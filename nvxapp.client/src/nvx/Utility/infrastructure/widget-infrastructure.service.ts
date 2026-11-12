import { Injectable } from '@angular/core';
import { iWidgetProvider, WidgetDefinition } from './widget.model';
import { AskAssistantWidgetComponent } from '../../shared/components/infrastructure/widgets/ask-assistant-widget/ask-assistant-widget.component';
import { DeadlinesWidgetComponent } from '../../shared/components/infrastructure/widgets/deadlines-widget/deadlines-widget.component';
import { RequestsWidgetComponent } from '../../shared/components/infrastructure/widgets/requests-widget/requests-widget.component';
import { RUOLI_AZIENDA } from './notification-notifier.service';
import { APPLICATIVI_RICHIESTE } from './request-ui.model';

/*
 Widget delle funzioni base (ANALISI_INFRASTRUCTURE_WIDGET.md), disponibili in ogni ramo e per ogni utente.
*/
@Injectable({
  providedIn: 'root'
})
export class WidgetInfrastructureService implements iWidgetProvider {
  public get Widgets(): WidgetDefinition[] {
    return [
      {
        key: 'infrastructure.askAssistant',
        title: 'widgets.askAssistant.title',
        description: 'widgets.askAssistant.description',
        icon: 'sparkles-outline',
        size: 'large',
        default: true,
        zorder: 100,
        frameless: true,
        component: AskAssistantWidgetComponent
      },
      // scadenzario (INF-SVC.3): per chi lavora in un'azienda; non predefinito, si aggiunge dalla configurazione della dashboard
      {
        key: 'infrastructure.deadlines',
        title: 'widgets.deadlines.title',
        description: 'widgets.deadlines.description',
        icon: 'calendar-number-outline',
        roles: RUOLI_AZIENDA,
        size: 'medium',
        default: false,
        zorder: 200,
        component: DeadlinesWidgetComponent
      },
      // richieste da approvare comuni (ANALISI_INFRASTRUCTURE_RICHIESTE.md, RICH-INF.4): solo con un applicativo che porta richieste
      {
        key: 'infrastructure.requests',
        title: 'widgets.requests.title',
        description: 'widgets.requests.description',
        icon: 'checkmark-done-outline',
        roles: RUOLI_AZIENDA,
        anyApplication: APPLICATIVI_RICHIESTE,
        size: 'small',
        default: true,
        zorder: 150,
        component: RequestsWidgetComponent
      }
    ];
  }
}
