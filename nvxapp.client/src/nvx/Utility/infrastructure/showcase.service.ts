import { Injectable } from '@angular/core';
import { Route, Routes } from '@angular/router';
import { MainMenuItem, MenuType } from './main-menu.service';
import { ShowcaseInfrastructureService } from './showcase-infrastructure.service';
import { ShowcaseAttendanceTrackingService } from './showcase-attendance-tracking.service';
import { ShowcaseCrmService } from './showcase-crm.service';
import { ShowcaseDocumentManagementService } from './showcase-document-management.service';
import { ShowcaseAllFunctionService } from './showcase-all-function.service';
import { ShowcaseContenuti, ShowcaseContenutiRidefiniti } from './showcase.model';
import { showcaseGuard } from '../../pages/RoleGuard/infrastructure/showcase-guard';

/*
 Showcase (sito pubblico, visibile solo prima del login): unisce quella generica di Infrastructure
 con le ridefinizioni dei rami applicativi. Le voci e le pagine di Infrastructure sono segnaposto:
  - voce di menu con lo stesso url  -> sostituita da quella del ramo; le altre si aggiungono
  - rotta con lo stesso path        -> sostituita da quella del ramo (pagina propria del ramo)
  - sezione dei contenuti           -> sostituita per intero da quella del ramo (stesse pagine, altri testi)
 A differenza delle pagine degli applicativi, lo showcase non dipende dagli applicativi attivi
 (non c'e' ancora un utente): vale quella del ramo in cui gira il codice. Ogni ramo applicativo (presenze, CRM,
 documentale) riempie la propria ridefinizione; il ramo AllFunction riempie quella dell'applicativo completo, con
 tutti gli applicativi. Ordine di precedenza (l'ultima vince): presenze, CRM, documentale, applicativo completo.
*/
interface ShowcaseRamo {
  readonly Pages: MainMenuItem[];
  getRoutes(): Routes;
  readonly Contenuti: ShowcaseContenutiRidefiniti;
}

@Injectable({
  providedIn: 'root'
})
export class ShowcaseService {

  constructor(private showcaseInfrastructureService: ShowcaseInfrastructureService,
              private showcaseAttendanceTrackingService: ShowcaseAttendanceTrackingService,
              private showcaseCrmService: ShowcaseCrmService,
              private showcaseDocumentManagementService: ShowcaseDocumentManagementService,
              private showcaseAllFunctionService: ShowcaseAllFunctionService) { }

  // ridefinizioni dei rami, in ordine di precedenza crescente
  private get rami(): ShowcaseRamo[] {
    return [this.showcaseAttendanceTrackingService, this.showcaseCrmService, this.showcaseDocumentManagementService, this.showcaseAllFunctionService];
  }

  public get Pages(): MainMenuItem[] {
    const chiave = (x: MainMenuItem) => x.menuType === MenuType.MenuHeader ? 'H' + x.group : 'U' + x.url;
    let voci = this.showcaseInfrastructureService.Pages;
    for (const ramo of this.rami) {
      const ridefinite = ramo.Pages;
      const chiaviRidefinite = new Set(ridefinite.map(chiave));
      voci = [...voci.filter(x => !chiaviRidefinite.has(chiave(x))), ...ridefinite];
    }
    return voci;
  }

  // rotte dello showcase, tutte con showcaseGuard (dopo il login si va alla dashboard)
  public getRoutes(): Routes {
    let rotte = this.showcaseInfrastructureService.getRoutes();
    for (const ramo of this.rami) {
      const ridefinite = ramo.getRoutes();
      const pathRidefiniti = new Set(ridefinite.map(x => x.path));
      rotte = [...rotte.filter(x => !pathRidefiniti.has(x.path)), ...ridefinite];
    }
    return rotte.map((route: Route) => ({
      ...route,
      canActivate: [...(route.canActivate ?? []), showcaseGuard],
    }));
  }

  public get Contenuti(): ShowcaseContenuti {
    return Object.assign({}, this.showcaseInfrastructureService.Contenuti, ...this.rami.map(r => r.Contenuti));
  }

}
