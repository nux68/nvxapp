import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { RouteInfrastructureService } from './route-infrastructure.service';
import { RouteAttendanceTrackingService } from './route-attendance-tracking.service';


@Injectable({
  providedIn: 'root'
})
export class RouteService {

  constructor(private routeInfrastructureService: RouteInfrastructureService,
              private routeAttendanceTrackingService: RouteAttendanceTrackingService) { }

  public getRoutes(): Routes {
    const routesFromService1 = this.routeInfrastructureService.getRoutes();
    const routesFromService2 = this.routeAttendanceTrackingService.getRoutes();

    return routesFromService1.concat(routesFromService2);
  }

}
