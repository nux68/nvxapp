import { Injectable } from '@angular/core';
import { iWidgetProvider, WidgetDefinition } from './widget.model';
import { AskAssistantWidgetComponent } from '../../shared/components/infrastructure/widgets/ask-assistant-widget/ask-assistant-widget.component';
import { DeadlinesWidgetComponent } from '../../shared/components/infrastructure/widgets/deadlines-widget/deadlines-widget.component';
import { RUOLI_AZIENDA } from './notification-notifier.service';

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
      }
    ];
  }
}
