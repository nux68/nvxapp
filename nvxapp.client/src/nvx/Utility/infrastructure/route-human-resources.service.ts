import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { authenticatedGuard } from '../../pages/RoleGuard/infrastructure/showcase-guard';

/*
 Rotte di Risorse Umane (RouteService aggiunge la guardia dell'applicativo attivo; i permessi veri li controlla il server).
    - getRoutes()       : pagine dell'azienda (ufficio del personale e area del dipendente)
    - getStudioRoutes() : pagine lato studio (cedolini delle aziende dello studio): stesse pagine dei cedolini con il percorso
                          /hrstudio..., che le fa lavorare sull'azienda scelta dallo studio
*/
@Injectable({
  providedIn: 'root'
})
export class RouteHumanResourcesService {

  constructor() { }

  public getRoutes(): Routes {
    return [
      { path: 'hrsettings', loadChildren: () => import('../../pages/HumanResources/hr-settings-page/hr-settings-page.module').then(m => m.HrSettingsPageModule), canActivate: [authenticatedGuard] },
      { path: 'hremployeelist', loadChildren: () => import('../../pages/HumanResources/hr-employee-list-page/hr-employee-list-page.module').then(m => m.HrEmployeeListPageModule), canActivate: [authenticatedGuard] },
      { path: 'hremployeeedit', loadChildren: () => import('../../pages/HumanResources/hr-employee-edit-page/hr-employee-edit-page.module').then(m => m.HrEmployeeEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'hrpayslips', loadChildren: () => import('../../pages/HumanResources/hr-payslip-list-page/hr-payslip-list-page.module').then(m => m.HrPayslipListPageModule), canActivate: [authenticatedGuard] },
      { path: 'hrpayslipupload', loadChildren: () => import('../../pages/HumanResources/hr-payslip-upload-page/hr-payslip-upload-page.module').then(m => m.HrPayslipUploadPageModule), canActivate: [authenticatedGuard] },
      { path: 'hrpayslipdetail', loadChildren: () => import('../../pages/HumanResources/hr-payslip-detail-page/hr-payslip-detail-page.module').then(m => m.HrPayslipDetailPageModule), canActivate: [authenticatedGuard] },
      { path: 'hrmypayslips', loadChildren: () => import('../../pages/HumanResources/hr-my-payslips-page/hr-my-payslips-page.module').then(m => m.HrMyPayslipsPageModule), canActivate: [authenticatedGuard] },
      { path: 'hrcourselist', loadChildren: () => import('../../pages/HumanResources/hr-course-list-page/hr-course-list-page.module').then(m => m.HrCourseListPageModule), canActivate: [authenticatedGuard] },
      { path: 'hrcourseedit', loadChildren: () => import('../../pages/HumanResources/hr-course-edit-page/hr-course-edit-page.module').then(m => m.HrCourseEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'hrcourseedition', loadChildren: () => import('../../pages/HumanResources/hr-course-edition-page/hr-course-edition-page.module').then(m => m.HrCourseEditionPageModule), canActivate: [authenticatedGuard] },
      { path: 'hrplanlist', loadChildren: () => import('../../pages/HumanResources/hr-plan-list-page/hr-plan-list-page.module').then(m => m.HrPlanListPageModule), canActivate: [authenticatedGuard] },
      { path: 'hrplanedit', loadChildren: () => import('../../pages/HumanResources/hr-plan-edit-page/hr-plan-edit-page.module').then(m => m.HrPlanEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'hrchecklisttemplatelist', loadChildren: () => import('../../pages/HumanResources/hr-checklist-template-list-page/hr-checklist-template-list-page.module').then(m => m.HrChecklistTemplateListPageModule), canActivate: [authenticatedGuard] },
      { path: 'hrchecklisttemplateedit', loadChildren: () => import('../../pages/HumanResources/hr-checklist-template-edit-page/hr-checklist-template-edit-page.module').then(m => m.HrChecklistTemplateEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'hrchecklistlist', loadChildren: () => import('../../pages/HumanResources/hr-checklist-list-page/hr-checklist-list-page.module').then(m => m.HrChecklistListPageModule), canActivate: [authenticatedGuard] },
      { path: 'hrchecklistedit', loadChildren: () => import('../../pages/HumanResources/hr-checklist-edit-page/hr-checklist-edit-page.module').then(m => m.HrChecklistEditPageModule), canActivate: [authenticatedGuard] },
    ];
  }

  public getStudioRoutes(): Routes {
    return [
      { path: 'hrstudiopayslips', loadChildren: () => import('../../pages/HumanResources/hr-payslip-list-page/hr-payslip-list-page.module').then(m => m.HrPayslipListPageModule), canActivate: [authenticatedGuard] },
      { path: 'hrstudiopayslipupload', loadChildren: () => import('../../pages/HumanResources/hr-payslip-upload-page/hr-payslip-upload-page.module').then(m => m.HrPayslipUploadPageModule), canActivate: [authenticatedGuard] },
      { path: 'hrstudiopayslipdetail', loadChildren: () => import('../../pages/HumanResources/hr-payslip-detail-page/hr-payslip-detail-page.module').then(m => m.HrPayslipDetailPageModule), canActivate: [authenticatedGuard] },
    ];
  }
}
