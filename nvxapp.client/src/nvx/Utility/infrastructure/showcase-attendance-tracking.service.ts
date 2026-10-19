import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { MainMenuItem } from './main-menu.service';
import { ShowcaseContenutiRidefiniti } from './showcase.model';

// Ridefinizione dello showcase per la gestione presenze: in Infrastructure e' vuota
// (vale la showcase generico), il ramo AttendanceTracking la riempie (vedi ShowcaseService).
@Injectable({
  providedIn: 'root'
})
export class ShowcaseAttendanceTrackingService {

  constructor() { }

  public get Pages(): MainMenuItem[] { return []; }
  public getRoutes(): Routes { return []; }
  public get Contenuti(): ShowcaseContenutiRidefiniti { return {}; }
}
