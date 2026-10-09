import { Injectable } from '@angular/core';
import { Route, Routes } from '@angular/router';
import { RouteInfrastructureService } from './route-infrastructure.service';
import { RouteAttendanceTrackingService } from './route-attendance-tracking.service';
import { applicationGuard } from '../../pages/RoleGuard/infrastructure/application-guard';
import { ApplicationType } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';


@Injectable({
  providedIn: 'root'
})
export class RouteService {

  constructor(private routeInfrastructureService: RouteInfrastructureService,
              private routeAttendanceTrackingService: RouteAttendanceTrackingService) { }

  public getRoutes(): Routes {
    const routesFromService1 = this.routeInfrastructureService.getRoutes();
    const routesFromService2 = this.forApplication(this.routeAttendanceTrackingService.getRoutes(), ApplicationType.AttendanceTracking);

    return routesFromService1.concat(routesFromService2);
  }

  // Le rotte di un applicativo sono accessibili solo se l'applicativo e' attivo per l'azienda:
  // a ogni rotta del modulo si aggiunge applicationGuard, oltre alle guardie di ruolo gia' presenti.
  private forApplication(routes: Routes, application: ApplicationType): Routes {
    return routes.map((route: Route) => ({
      ...route,
      canActivate: [...(route.canActivate ?? []), applicationGuard(application)],
    }));
  }

}
