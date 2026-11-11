import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { authenticatedGuard } from '../../pages/RoleGuard/infrastructure/showcase-guard';

/*
 Rotte di Risorse Umane (RouteService aggiunge la guardia dell'applicativo attivo; i permessi veri li controlla il server).
    - getRoutes()       : pagine dell'azienda
    - getStudioRoutes() : pagine lato studio (nessuna per ora)
*/
@Injectable({
  providedIn: 'root'
})
export class RouteHumanResourcesService {

  constructor() { }

  public getRoutes(): Routes {
    return [
      { path: 'hrsettings', loadChildren: () => import('../../pages/HumanResources/hr-settings-page/hr-settings-page.module').then(m => m.HrSettingsPageModule), canActivate: [authenticatedGuard] }
    ];
  }

  public getStudioRoutes(): Routes {
    return [];
  }
}
