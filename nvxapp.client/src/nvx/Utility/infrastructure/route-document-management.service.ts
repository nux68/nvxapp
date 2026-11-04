import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { authenticatedGuard } from '../../pages/RoleGuard/infrastructure/showcase-guard';

/*
 Rotte del documentale (RouteService aggiunge la guardia dell'applicativo attivo; i permessi veri li controlla il server).
    - getRoutes()       : pagine dell'azienda
    - getStudioRoutes() : pagine lato studio (Fase 2)
*/
@Injectable({
  providedIn: 'root'
})
export class RouteDocumentManagementService {

  constructor() { }

  public getRoutes(): Routes {
    return [
      { path: 'doclist', loadChildren: () => import('../../pages/DocumentManagement/doc-list-page/doc-list-page.module').then(m => m.DocListPageModule), canActivate: [authenticatedGuard] },
      { path: 'docedit', loadChildren: () => import('../../pages/DocumentManagement/doc-edit-page/doc-edit-page.module').then(m => m.DocEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'docmine', loadChildren: () => import('../../pages/DocumentManagement/doc-mine-page/doc-mine-page.module').then(m => m.DocMinePageModule), canActivate: [authenticatedGuard] },
      { path: 'doctypelist', loadChildren: () => import('../../pages/DocumentManagement/doc-type-list-page/doc-type-list-page.module').then(m => m.DocTypeListPageModule), canActivate: [authenticatedGuard] },
      { path: 'doctypeedit', loadChildren: () => import('../../pages/DocumentManagement/doc-type-edit-page/doc-type-edit-page.module').then(m => m.DocTypeEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'docfolderlist', loadChildren: () => import('../../pages/DocumentManagement/doc-folder-list-page/doc-folder-list-page.module').then(m => m.DocFolderListPageModule), canActivate: [authenticatedGuard] },
      { path: 'docfolderedit', loadChildren: () => import('../../pages/DocumentManagement/doc-folder-edit-page/doc-folder-edit-page.module').then(m => m.DocFolderEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'docemployeelist', loadChildren: () => import('../../pages/DocumentManagement/doc-employee-list-page/doc-employee-list-page.module').then(m => m.DocEmployeeListPageModule), canActivate: [authenticatedGuard] },
      { path: 'docemployeeedit', loadChildren: () => import('../../pages/DocumentManagement/doc-employee-edit-page/doc-employee-edit-page.module').then(m => m.DocEmployeeEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'docpermissions', loadChildren: () => import('../../pages/DocumentManagement/doc-permission-page/doc-permission-page.module').then(m => m.DocPermissionPageModule), canActivate: [authenticatedGuard] },
      { path: 'docsettings', loadChildren: () => import('../../pages/DocumentManagement/doc-settings-page/doc-settings-page.module').then(m => m.DocSettingsPageModule), canActivate: [authenticatedGuard] },
      { path: 'doclog', loadChildren: () => import('../../pages/DocumentManagement/doc-log-page/doc-log-page.module').then(m => m.DocLogPageModule), canActivate: [authenticatedGuard] },
      { path: 'docstorage', loadChildren: () => import('../../pages/DocumentManagement/doc-storage-page/doc-storage-page.module').then(m => m.DocStoragePageModule), canActivate: [authenticatedGuard] }
    ];
  }

  public getStudioRoutes(): Routes {
    return [];
  }
}
