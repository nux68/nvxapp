import { Injectable } from '@angular/core';
import { iWidgetProvider, WidgetDefinition } from './widget.model';
import { AskAssistantWidgetComponent } from '../../shared/components/infrastructure/widgets/ask-assistant-widget/ask-assistant-widget.component';

/*
 Widget delle funzioni base (ANALISI_WIDGET.md), disponibili in ogni ramo e per ogni utente.
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
      }
    ];
  }
}
