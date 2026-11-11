import { Injectable } from '@angular/core';
import { iWidgetProvider, WidgetDefinition } from './widget.model';

/*
 Widget della dashboard del Documentale (ANALISI_INFRASTRUCTURE_WIDGET.md). In Infrastructure e' vuoto: lo valorizza il ramo
 DocumentManagement (ANALISI_WIDGET_DOCUMENTALE.md).
*/
@Injectable({
  providedIn: 'root'
})
export class WidgetDocumentManagementService implements iWidgetProvider {
  public get Widgets(): WidgetDefinition[] { return []; }
}
