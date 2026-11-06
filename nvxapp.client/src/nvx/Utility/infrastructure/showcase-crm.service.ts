import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Routes } from '@angular/router';
import { MainMenuItem } from './main-menu.service';
import { ShowcaseContenutiRidefiniti } from './showcase.model';

// Ridefinizione dello showcase per il CRM: in Infrastructure e' vuota (vale lo showcase generico),
// il ramo CRM la riempie (vedi ShowcaseService).
@Injectable({
  providedIn: 'root'
})
export class ShowcaseCrmService {

  // transloco facoltativo: RouteService crea il servizio con new (fuori dalla DI) solo per le rotte
  constructor(private transloco?: TranslocoService) { }

  public get Pages(): MainMenuItem[] { return []; }
  public getRoutes(): Routes { return []; }
  public get Contenuti(): ShowcaseContenutiRidefiniti { return {}; }
}
