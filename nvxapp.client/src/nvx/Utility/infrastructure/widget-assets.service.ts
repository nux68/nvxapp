import { Injectable } from '@angular/core';
import { iWidgetProvider, WidgetDefinition } from './widget.model';

/*
 Widget della dashboard di Risorse aziendali (ANALISI_INFRASTRUCTURE_WIDGET.md). In Infrastructure e' vuoto: lo valorizza il ramo Assets.
*/
@Injectable({
  providedIn: 'root'
})
export class WidgetAssetsService implements iWidgetProvider {
  public get Widgets(): WidgetDefinition[] { return []; }
}
