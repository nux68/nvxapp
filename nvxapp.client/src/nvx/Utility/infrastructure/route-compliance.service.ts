import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { authenticatedGuard } from '../../pages/RoleGuard/infrastructure/showcase-guard';

/*
 Rotte di Compliance (RouteService aggiunge la guardia dell'applicativo attivo; i permessi veri li controlla il server).
    - getRoutes()       : pagine dell'azienda
    - getStudioRoutes() : pagine lato studio (nessuna per ora)
*/
@Injectable({
  providedIn: 'root'
})
export class RouteComplianceService {

  constructor() { }

  public getRoutes(): Routes {
    return [
      { path: 'cmpsettings', loadChildren: () => import('../../pages/Compliance/cmp-settings-page/cmp-settings-page.module').then(m => m.CmpSettingsPageModule), canActivate: [authenticatedGuard] }
    ];
  }

  public getStudioRoutes(): Routes {
    return [];
  }
}
