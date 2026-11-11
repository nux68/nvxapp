import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { authenticatedGuard } from '../../pages/RoleGuard/infrastructure/showcase-guard';

/*
 Rotte del CRM (RouteService aggiunge la guardia dell'applicativo attivo; i permessi veri li controlla il server).
    - getRoutes()       : pagine dell'azienda
    - getStudioRoutes() : pagine lato studio (nessuna per ora)
*/
@Injectable({
  providedIn: 'root'
})
export class RouteCrmService {

  constructor() { }

  public getRoutes(): Routes {
    return [
      { path: 'crmcustomerlist', loadChildren: () => import('../../pages/Crm/crm-customer-list-page/crm-customer-list-page.module').then(m => m.CrmCustomerListPageModule), canActivate: [authenticatedGuard] },
      { path: 'crmcustomeredit', loadChildren: () => import('../../pages/Crm/crm-customer-edit-page/crm-customer-edit-page.module').then(m => m.CrmCustomerEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'crmleadlist', loadChildren: () => import('../../pages/Crm/crm-lead-list-page/crm-lead-list-page.module').then(m => m.CrmLeadListPageModule), canActivate: [authenticatedGuard] },
      { path: 'crmleadedit', loadChildren: () => import('../../pages/Crm/crm-lead-edit-page/crm-lead-edit-page.module').then(m => m.CrmLeadEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'crmpipeline', loadChildren: () => import('../../pages/Crm/crm-pipeline-page/crm-pipeline-page.module').then(m => m.CrmPipelinePageModule), canActivate: [authenticatedGuard] },
      { path: 'crmopportunityedit', loadChildren: () => import('../../pages/Crm/crm-opportunity-edit-page/crm-opportunity-edit-page.module').then(m => m.CrmOpportunityEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'crmactivitylist', loadChildren: () => import('../../pages/Crm/crm-activity-list-page/crm-activity-list-page.module').then(m => m.CrmActivityListPageModule), canActivate: [authenticatedGuard] },
      { path: 'crmactivityedit', loadChildren: () => import('../../pages/Crm/crm-activity-edit-page/crm-activity-edit-page.module').then(m => m.CrmActivityEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'crmofferlist', loadChildren: () => import('../../pages/Crm/crm-offer-list-page/crm-offer-list-page.module').then(m => m.CrmOfferListPageModule), canActivate: [authenticatedGuard] },
      { path: 'crmofferedit', loadChildren: () => import('../../pages/Crm/crm-offer-edit-page/crm-offer-edit-page.module').then(m => m.CrmOfferEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'crmrequestlist', loadChildren: () => import('../../pages/Crm/crm-request-list-page/crm-request-list-page.module').then(m => m.CrmRequestListPageModule), canActivate: [authenticatedGuard] },
      { path: 'crmrequestedit', loadChildren: () => import('../../pages/Crm/crm-request-edit-page/crm-request-edit-page.module').then(m => m.CrmRequestEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'crmparlist', loadChildren: () => import('../../pages/Crm/crm-par-list-page/crm-par-list-page.module').then(m => m.CrmParListPageModule), canActivate: [authenticatedGuard] },
      { path: 'crmparedit', loadChildren: () => import('../../pages/Crm/crm-par-edit-page/crm-par-edit-page.module').then(m => m.CrmParEditPageModule), canActivate: [authenticatedGuard] },
      { path: 'crmsettings', loadChildren: () => import('../../pages/Crm/crm-settings-page/crm-settings-page.module').then(m => m.CrmSettingsPageModule), canActivate: [authenticatedGuard] },
      { path: 'crmreassign', loadChildren: () => import('../../pages/Crm/crm-reassign-page/crm-reassign-page.module').then(m => m.CrmReassignPageModule), canActivate: [authenticatedGuard] },
      { path: 'crmreport', loadChildren: () => import('../../pages/Crm/crm-report-page/crm-report-page.module').then(m => m.CrmReportPageModule), canActivate: [authenticatedGuard] }
    ];
  }

  public getStudioRoutes(): Routes {
    return [];
  }
}
