import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { authenticatedGuard } from '../../pages/RoleGuard/infrastructure/showcase-guard';

/*
 Rotte di Risorse aziendali (RouteService aggiunge la guardia dell'applicativo attivo; i permessi veri li controlla il server).
    - getRoutes()       : pagine dell'azienda
    - getStudioRoutes() : pagine lato studio (nessuna per ora)
*/
@Injectable({
  providedIn: 'root'
})
export class RouteAssetsService {

  constructor() { }

  public getRoutes(): Routes {
    return [
      { path: 'astsettings', loadChildren: () => import('../../pages/Assets/ast-settings-page/ast-settings-page.module').then(m => m.AstSettingsPageModule), canActivate: [authenticatedGuard] }
    ];
  }

  public getStudioRoutes(): Routes {
    return [];
  }
}
