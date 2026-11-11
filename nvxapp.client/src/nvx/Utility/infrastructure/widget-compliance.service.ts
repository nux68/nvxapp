import { Injectable } from '@angular/core';
import { iWidgetProvider, WidgetDefinition } from './widget.model';

/*
 Widget della dashboard di Compliance (ANALISI_INFRASTRUCTURE_WIDGET.md). In Infrastructure e' vuoto: lo valorizza il ramo Compliance.
*/
@Injectable({
  providedIn: 'root'
})
export class WidgetComplianceService implements iWidgetProvider {
  public get Widgets(): WidgetDefinition[] { return []; }
}
