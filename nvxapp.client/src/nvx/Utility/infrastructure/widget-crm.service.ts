import { Injectable } from '@angular/core';
import { iWidgetProvider, WidgetDefinition } from './widget.model';

/*
 Widget della dashboard del CRM (ANALISI_INFRASTRUCTURE_WIDGET.md). In Infrastructure e' vuoto: lo valorizza il ramo CRM
 (ANALISI_CRM_WIDGET.md).
*/
@Injectable({
  providedIn: 'root'
})
export class WidgetCrmService implements iWidgetProvider {
  public get Widgets(): WidgetDefinition[] { return []; }
}
