import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';
import { RouteService } from '../nvx/Utility/infrastructure/route.service';
import { RouteInfrastructureService } from '../nvx/Utility/infrastructure/route-infrastructure.service';
import { RouteAttendanceTrackingService } from '../nvx/Utility/infrastructure/route-attendance-tracking.service';

const routeInfrastructureService = new RouteInfrastructureService();
const routeAttendanceTrackingService = new RouteAttendanceTrackingService();
const routeService = new RouteService(routeInfrastructureService, routeAttendanceTrackingService); // Creazione istanza del servizio
const routes: Routes = routeService.getRoutes(); // Ottieni dinamicamente le rotte



@NgModule({
  imports: [
    RouterModule.forRoot(routes, { preloadingStrategy: PreloadAllModules })
  ],
  exports: [RouterModule]
})
export class AppRoutingModule {}
