import { Injectable } from '@angular/core';
import { Route, Routes } from '@angular/router';
import { RouteInfrastructureService } from './route-infrastructure.service';
import { RouteAttendanceTrackingService } from './route-attendance-tracking.service';
import { RouteDocumentManagementService } from './route-document-management.service';
import { RouteCrmService } from './route-crm.service';
import { RouteHumanResourcesService } from './route-human-resources.service';
import { RouteComplianceService } from './route-compliance.service';
import { RouteAssetsService } from './route-assets.service';
import { applicationGuard } from '../../pages/RoleGuard/infrastructure/application-guard';
import { financialAdvisorApplicationGuard } from '../../pages/RoleGuard/infrastructure/financial-advisor-application-guard';
import { menuRestrictionGuard } from '../../pages/RoleGuard/infrastructure/menu-restriction-guard';
import { ApplicationType } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { ShowcaseService } from './showcase.service';
import { ShowcaseInfrastructureService } from './showcase-infrastructure.service';
import { ShowcaseAttendanceTrackingService } from './showcase-attendance-tracking.service';
import { ShowcaseCrmService } from './showcase-crm.service';
import { ShowcaseDocumentManagementService } from './showcase-document-management.service';
import { ShowcaseAllFunctionService } from './showcase-all-function.service';


@Injectable({
  providedIn: 'root'
})
export class RouteService {

  constructor(private routeInfrastructureService: RouteInfrastructureService,
              private routeAttendanceTrackingService: RouteAttendanceTrackingService,
              private routeDocumentManagementService: RouteDocumentManagementService,
              private routeCrmService: RouteCrmService,
              private routeHumanResourcesService: RouteHumanResourcesService,
              private routeComplianceService: RouteComplianceService,
              private routeAssetsService: RouteAssetsService) { }

  public getRoutes(): Routes {
    const routesFromService1 = this.routeInfrastructureService.getRoutes();
    const routesFromService2 = this.forApplication(this.routeAttendanceTrackingService.getRoutes(), ApplicationType.AttendanceTracking);
    // documentale: pagine dell'azienda (applicativo attivo) e pagine lato studio (applicativo abilitato per lo studio)
    const routesDocumentManagement = this.forApplication(this.routeDocumentManagementService.getRoutes(), ApplicationType.DocumentManagement)
      .concat(this.forFinancialAdvisorApplication(this.routeDocumentManagementService.getStudioRoutes(), ApplicationType.DocumentManagement));
    // CRM: come il documentale
    const routesCrm = this.forApplication(this.routeCrmService.getRoutes(), ApplicationType.Crm)
      .concat(this.forFinancialAdvisorApplication(this.routeCrmService.getStudioRoutes(), ApplicationType.Crm));
    // Risorse Umane, Compliance, Risorse aziendali: come il CRM
    const routesNuoviModuli = [
      ...this.forApplication(this.routeHumanResourcesService.getRoutes(), ApplicationType.HumanResources)
        .concat(this.forFinancialAdvisorApplication(this.routeHumanResourcesService.getStudioRoutes(), ApplicationType.HumanResources)),
      ...this.forApplication(this.routeComplianceService.getRoutes(), ApplicationType.Compliance)
        .concat(this.forFinancialAdvisorApplication(this.routeComplianceService.getStudioRoutes(), ApplicationType.Compliance)),
      ...this.forApplication(this.routeAssetsService.getRoutes(), ApplicationType.Assets)
        .concat(this.forFinancialAdvisorApplication(this.routeAssetsService.getStudioRoutes(), ApplicationType.Assets)),
    ];
    // showcase (pagine pubbliche prima del login): non dipende dagli applicativi attivi
    const routesShowcase = new ShowcaseService(new ShowcaseInfrastructureService(), new ShowcaseAttendanceTrackingService(),
      new ShowcaseCrmService(), new ShowcaseDocumentManagementService(), new ShowcaseAllFunctionService()).getRoutes();

    return this.withMenuRestriction(routesShowcase.concat(routesFromService1, routesFromService2, routesDocumentManagement, routesCrm, routesNuoviModuli));
  }

  // A tutte le rotte (di ogni applicativo) si aggiunge menuRestrictionGuard: blocca le voci di menu
  // nascoste dal CompanyPowerAdmin. Le rotte di solo reindirizzamento non hanno guardie.
  private withMenuRestriction(routes: Routes): Routes {
    return routes.map((route: Route) => route.redirectTo != null ? route : ({
      ...route,
      canActivate: [...(route.canActivate ?? []), menuRestrictionGuard],
    }));
  }

  // Le rotte di un applicativo sono accessibili solo se l'applicativo e' attivo per l'azienda:
  // a ogni rotta del modulo si aggiunge applicationGuard, oltre alle guardie di ruolo gia' presenti.
  private forApplication(routes: Routes, application: ApplicationType): Routes {
    return routes.map((route: Route) => ({
      ...route,
      canActivate: [...(route.canActivate ?? []), applicationGuard(application)],
    }));
  }

  // Le rotte lato studio di un applicativo sono accessibili solo se l'applicativo e' abilitato per lo studio.
  private forFinancialAdvisorApplication(routes: Routes, application: ApplicationType): Routes {
    return routes.map((route: Route) => ({
      ...route,
      canActivate: [...(route.canActivate ?? []), financialAdvisorApplicationGuard(application)],
    }));
  }

}
