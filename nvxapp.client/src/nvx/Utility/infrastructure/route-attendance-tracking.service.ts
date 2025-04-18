import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { RoleGuard4CompanyAdmin } from '../../pages/RoleGuard/infrastructure/role-guard-4-company-admin';
import { RoleGuard4User } from '../../pages/RoleGuard/infrastructure/role-guard-4-user';


@Injectable({
  providedIn: 'root'
})
export class RouteAttendanceTrackingService {

  constructor() { }

  public getRoutes(): Routes {
    return [
      //user
      { path: 'usertimesheet', loadChildren: () => import('../../pages/GestionePresenze/time-sheet-user-page/time-sheet-user-page.module').then(m => m.TimeSheetUserModule), canActivate: [RoleGuard4User] },
      { path: 'timeclockuser', loadChildren: () => import('../../pages/GestionePresenze/time-clock-user-page/time-clock-user-page.module').then(m => m.TimeClockUserPageModule), canActivate: [RoleGuard4User] },
      { path: 'requestjustificationuser', loadChildren: () => import('../../pages/GestionePresenze/request-justification-user-page/request-justification-user-page.module').then(m => m.RequestJustificationUserPageModule), canActivate: [RoleGuard4User] },
      { path: 'requestclockinguser', loadChildren: () => import('../../pages/GestionePresenze/request-clocking-user-page/request-clocking-user-page.module').then(m => m.RequestClockingUserPageModule), canActivate: [RoleGuard4User] },


      //admin
      { path: 'admintimesheet', loadChildren: () => import('../../pages/GestionePresenze/time-sheet-admin-page/time-sheet-admin-page.module').then(m => m.TimeSheetAdminModule), canActivate: [RoleGuard4CompanyAdmin] },

    ];
  }
}
