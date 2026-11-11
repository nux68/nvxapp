import { Injectable } from '@angular/core';
import { iMainMenuService, MainMenuItem, WorkingMode } from './main-menu.service';

/*
 Voci di menu di Risorse Umane. In Infrastructure e' vuoto: lo valorizza il ramo HumanResources.
 Le voci dello studio (Pages4FinancialAdvisorAdmin) compaiono se l'applicativo e' abilitato per lo studio
 dell'utente; le altre se e' attivo per l'azienda (vedi MainMenuService).
*/
@Injectable({
  providedIn: 'root'
})
export class MainMenuHumanResourcesService implements iMainMenuService {

  constructor() { }

  public get Pages4SuperUser(): MainMenuItem[] { return []; }
  public get Pages4Admin(): MainMenuItem[] { return []; }
  public get Pages4DealerAdmin(): MainMenuItem[] { return []; }
  public get Pages4FinancialAdvisorAdmin(): MainMenuItem[] { return []; }
  public get Pages4CompanyAdmin(): MainMenuItem[] { return []; }
  public get Pages4User(): MainMenuItem[] { return []; }
  public get GetWorkingMode(): WorkingMode { return WorkingMode.Infrastructure; }
}
