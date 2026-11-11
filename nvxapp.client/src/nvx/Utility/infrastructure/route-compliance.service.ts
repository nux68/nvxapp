import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { authenticatedGuard } from '../../pages/RoleGuard/infrastructure/showcase-guard';

/*
 Rotte di Compliance (RouteService aggiunge la guardia dell'applicativo attivo; i permessi veri li controlla il server).
    - getRoutes()       : pagine dell'azienda
    - getStudioRoutes() : pagine lato studio, accessibili se l'applicativo e' abilitato per lo studio (registro GDPR, CMP-GDPR.4)
 Elenco e modifica sono pagine uniche per tutte le sezioni (area nei data della rotta, vedi cmp-sezioni.ts).
*/
@Injectable({
  providedIn: 'root'
})
export class RouteComplianceService {

  constructor() { }

  public getRoutes(): Routes {
    return [
      { path: 'cmpsettings', loadChildren: () => import('../../pages/Compliance/cmp-settings-page/cmp-settings-page.module').then(m => m.CmpSettingsPageModule), canActivate: [authenticatedGuard] },
      { path: 'cmp81', data: { area: '81' }, loadChildren: () => import('../../pages/Compliance/cmp-list-page/cmp-list-page.module').then(m => m.CmpListPageModule), canActivate: [authenticatedGuard] },
      { path: 'cmpgdpr', data: { area: 'gdpr' }, loadChildren: () => import('../../pages/Compliance/cmp-list-page/cmp-list-page.module').then(m => m.CmpListPageModule), canActivate: [authenticatedGuard] },
      { path: 'cmpedit', loadChildren: () => import('../../pages/Compliance/cmp-edit-page/cmp-edit-page.module').then(m => m.CmpEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'cmpmine', loadChildren: () => import('../../pages/Compliance/cmp-mine-page/cmp-mine-page.module').then(m => m.CmpMinePageModule), canActivate: [authenticatedGuard] }
    ];
  }

  public getStudioRoutes(): Routes {
    return [
      { path: 'cmpstudio', data: { area: 'studio' }, loadChildren: () => import('../../pages/Compliance/cmp-list-page/cmp-list-page.module').then(m => m.CmpListPageModule), canActivate: [authenticatedGuard] },
      { path: 'cmpstudioedit', loadChildren: () => import('../../pages/Compliance/cmp-edit-page/cmp-edit-page.module').then(m => m.CmpEditPageModule), canActivate: [authenticatedGuard] }
    ];
  }
}
