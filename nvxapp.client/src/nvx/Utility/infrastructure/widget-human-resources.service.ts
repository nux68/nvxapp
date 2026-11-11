import { Injectable } from '@angular/core';
import { iWidgetProvider, WidgetDefinition } from './widget.model';

/*
 Widget della dashboard di Risorse Umane (ANALISI_INFRASTRUCTURE_WIDGET.md). In Infrastructure e' vuoto: lo valorizza il ramo HumanResources.
*/
@Injectable({
  providedIn: 'root'
})
export class WidgetHumanResourcesService implements iWidgetProvider {
  public get Widgets(): WidgetDefinition[] { return []; }
}
