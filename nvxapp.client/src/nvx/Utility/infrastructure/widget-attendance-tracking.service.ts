import { Injectable } from '@angular/core';
import { iWidgetProvider, WidgetDefinition } from './widget.model';

/*
 Widget della dashboard delle Presenze (ANALISI_INFRASTRUCTURE_WIDGET.md). In Infrastructure e' vuoto: lo valorizza il ramo
 AttendanceTracking (ANALISI_WIDGET_PRESENZE.md).
*/
@Injectable({
  providedIn: 'root'
})
export class WidgetAttendanceTrackingService implements iWidgetProvider {
  public get Widgets(): WidgetDefinition[] { return []; }
}
