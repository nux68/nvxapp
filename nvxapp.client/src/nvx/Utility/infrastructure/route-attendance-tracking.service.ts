import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { RoleGuard4CompanyAdmin } from '../../pages/RoleGuard/infrastructure/role-guard-4-company-admin';
import { RoleGuard4User } from '../../pages/RoleGuard/infrastructure/role-guard-4-user';
import { RoleGuard4CompanyPowerAdmin } from '../../pages/RoleGuard/infrastructure/role-guard-4-company-power-admin';
import { RoleGuard4JustClockRequest } from '../../pages/RoleGuard/infrastructure/role-guard-4-just-clock-request';


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
      { path: 'requestjustificationuser', loadChildren: () => import('../../pages/GestionePresenze/request-justification-user-page/request-justification-user-page.module').then(m => m.RequestJustificationUserPageModule), canActivate: [RoleGuard4JustClockRequest] },
      { path: 'requestclockinguser', loadChildren: () => import('../../pages/GestionePresenze/request-clocking-user-page/request-clocking-user-page.module').then(m => m.RequestClockingUserPageModule), canActivate: [RoleGuard4JustClockRequest] },
      { path: 'requestlistuser', loadChildren: () => import('../../pages/GestionePresenze/request-list-user-page/request-list-user-page.module').then(m => m.RequestListUserPageModule), canActivate: [RoleGuard4User] },


      //power admin
      { path: 'poweradmintimesheet', loadChildren: () => import('../../pages/GestionePresenze/time-sheet-power-admin-page/time-sheet-power-admin-page.module').then(m => m.TimeSheetPowerAdminPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'justificationlist', loadChildren: () => import('../../pages/GestionePresenze/justification-list-page/justification-list-page.module').then(m => m.JustificationListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'justificationedit', loadChildren: () => import('../../pages/GestionePresenze/justification-edit-page/justification-edit-page.module').then(m => m.JustificationEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'departmentlist', loadChildren: () => import('../../pages/GestionePresenze/department-list-page/department-list-page.module').then(m => m.DepartmentListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'departmentedit', loadChildren: () => import('../../pages/GestionePresenze/department-edit-page/department-edit-page.module').then(m => m.DepartmentEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'companycfgedit', loadChildren: () => import('../../pages/GestionePresenze/company-cfg-page/company-cfg-page.module').then(m => m.CompanyCfgPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'userdepartmentlist', loadChildren: () => import('../../pages/GestionePresenze/user-department-list-page/user-department-list-page.module').then(m => m.UserDepartmentListPageModule), canActivate: [RoleGuard4CompanyAdmin] },
      { path: 'competencelist', loadChildren: () => import('../../pages/GestionePresenze/competence-list-page/competence-list-page.module').then(m => m.CompetenceListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'competenceedit', loadChildren: () => import('../../pages/GestionePresenze/competence-edit-page/competence-edit-page.module').then(m => m.CompetenceEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'activitylist', loadChildren: () => import('../../pages/GestionePresenze/activity-list-page/activity-list-page.module').then(m => m.ActivityListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'activityedit', loadChildren: () => import('../../pages/GestionePresenze/activity-edit-page/activity-edit-page.module').then(m => m.ActivityEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },

      //admin
      { path: 'admintimesheet', loadChildren: () => import('../../pages/GestionePresenze/time-sheet-admin-page/time-sheet-admin-page.module').then(m => m.TimeSheetAdminModule), canActivate: [RoleGuard4CompanyAdmin] },
      { path: 'requestlistadmin', loadChildren: () => import('../../pages/GestionePresenze/request-list-admin-page/request-list-admin-page.module').then(m => m.RequestListAdminPageModule), canActivate: [RoleGuard4CompanyAdmin] },

      
      
      
    ];
  }
}
