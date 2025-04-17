import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { RoleGuard4User } from '../pages/RoleGuard/infrastructure/role-guard-4-user';
import { RoleGuard4CompanyAdmin } from '../pages/RoleGuard/infrastructure/role-guard-4-company-admin';

@Injectable({
  providedIn: 'root'
})
export class RouteAttendanceTrackingService {

  constructor() { }

  public getRoutes(): Routes {
    return [
      { path: 'usertimesheet',  loadChildren: () => import('../pages/GestionePresenze/time-sheet-user-page/time-sheet-user-page.module').then(m => m.TimeSheetUserModule), canActivate: [RoleGuard4User] },
      { path: 'admintimesheet', loadChildren: () => import('../pages/GestionePresenze/time-sheet-admin-page/time-sheet-admin-page.module').then(m => m.TimeSheetAdminModule), canActivate: [RoleGuard4CompanyAdmin] },

    ];
  }
}
