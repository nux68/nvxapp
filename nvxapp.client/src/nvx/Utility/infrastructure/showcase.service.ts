import { Injectable } from '@angular/core';
import { Route, Routes } from '@angular/router';
import { MainMenuItem, MenuType } from './main-menu.service';
import { ShowcaseInfrastructureService } from './showcase-infrastructure.service';
import { ShowcaseAttendanceTrackingService } from './showcase-attendance-tracking.service';
import { ShowcaseContenuti } from './showcase.model';
import { showcaseGuard } from '../../pages/RoleGuard/infrastructure/showcase-guard';

/*
 Showcase (sito pubblico, visibile solo prima del login): unisce quella generica di Infrastructure
 con le ridefinizioni dei rami applicativi. Le voci e le pagine di Infrastructure sono segnaposto:
  - voce di menu con lo stesso url  -> sostituita da quella del ramo; le altre si aggiungono
  - rotta con lo stesso path        -> sostituita da quella del ramo (pagina propria del ramo)
  - sezione dei contenuti           -> sostituita per intero da quella del ramo (stesse pagine, altri testi)
 A differenza delle pagine degli applicativi, lo showcase non dipende dagli applicativi attivi
 (non c'e' ancora un utente): vale quella del ramo in cui gira il codice.
*/
@Injectable({
  providedIn: 'root'
})
export class ShowcaseService {

  constructor(private showcaseInfrastructureService: ShowcaseInfrastructureService,
              private showcaseAttendanceTrackingService: ShowcaseAttendanceTrackingService) { }

  public get Pages(): MainMenuItem[] {
    const ridefinite = this.showcaseAttendanceTrackingService.Pages;
    const chiave = (x: MainMenuItem) => x.menuType === MenuType.MenuHeader ? 'H' + x.group : 'U' + x.url;
    const chiaviRidefinite = new Set(ridefinite.map(chiave));

    return [
      ...this.showcaseInfrastructureService.Pages.filter(x => !chiaviRidefinite.has(chiave(x))),
      ...ridefinite
    ];
  }

  // rotte dello showcase, tutte con showcaseGuard (dopo il login si va alla dashboard)
  public getRoutes(): Routes {
    const ridefinite = this.showcaseAttendanceTrackingService.getRoutes();
    const pathRidefiniti = new Set(ridefinite.map(x => x.path));

    return [
      ...this.showcaseInfrastructureService.getRoutes().filter(x => !pathRidefiniti.has(x.path)),
      ...ridefinite
    ].map((route: Route) => ({
      ...route,
      canActivate: [...(route.canActivate ?? []), showcaseGuard],
    }));
  }

  public get Contenuti(): ShowcaseContenuti {
    return {
      ...this.showcaseInfrastructureService.Contenuti,
      ...this.showcaseAttendanceTrackingService.Contenuti
    };
  }

}
