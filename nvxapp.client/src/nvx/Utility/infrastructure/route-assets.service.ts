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
      { path: 'astsettings', loadChildren: () => import('../../pages/Assets/ast-settings-page/ast-settings-page.module').then(m => m.AstSettingsPageModule), canActivate: [authenticatedGuard] },
      { path: 'astmine', loadChildren: () => import('../../pages/Assets/ast-mine-page/ast-mine-page.module').then(m => m.AstMinePageModule), canActivate: [authenticatedGuard] },
      // flotta (AST-FLO)
      { path: 'astvehicles', loadChildren: () => import('../../pages/Assets/ast-vehicle-list-page/ast-vehicle-list-page.module').then(m => m.AstVehicleListPageModule), canActivate: [authenticatedGuard] },
      { path: 'astvehicleedit', loadChildren: () => import('../../pages/Assets/ast-vehicle-edit-page/ast-vehicle-edit-page.module').then(m => m.AstVehicleEditPageModule), canActivate: [authenticatedGuard] },
      // beni assegnati (AST-BENI)
      { path: 'astgoods', loadChildren: () => import('../../pages/Assets/ast-goods-list-page/ast-goods-list-page.module').then(m => m.AstGoodsListPageModule), canActivate: [authenticatedGuard] },
      { path: 'astgoodedit', loadChildren: () => import('../../pages/Assets/ast-good-edit-page/ast-good-edit-page.module').then(m => m.AstGoodEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'astgooditemedit', loadChildren: () => import('../../pages/Assets/ast-good-item-edit-page/ast-good-item-edit-page.module').then(m => m.AstGoodItemEditPageModule), canActivate: [authenticatedGuard] },
      // prenotazioni (AST-PREN)
      { path: 'astbookings', loadChildren: () => import('../../pages/Assets/ast-booking-page/ast-booking-page.module').then(m => m.AstBookingPageModule), canActivate: [authenticatedGuard] },
      { path: 'astbookingedit', loadChildren: () => import('../../pages/Assets/ast-booking-edit-page/ast-booking-edit-page.module').then(m => m.AstBookingEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'astresources', loadChildren: () => import('../../pages/Assets/ast-resource-list-page/ast-resource-list-page.module').then(m => m.AstResourceListPageModule), canActivate: [authenticatedGuard] },
      { path: 'astresourceedit', loadChildren: () => import('../../pages/Assets/ast-resource-edit-page/ast-resource-edit-page.module').then(m => m.AstResourceEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'astvehicleitemedit', loadChildren: () => import('../../pages/Assets/ast-vehicle-item-edit-page/ast-vehicle-item-edit-page.module').then(m => m.AstVehicleItemEditPageModule), canActivate: [authenticatedGuard] },
    ];
  }

  public getStudioRoutes(): Routes {
    return [];
  }
}
