import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';

/*
 Rotte di Compliance. In Infrastructure e' vuoto: lo valorizza il ramo Compliance.
    - getRoutes()       : pagine dell'azienda, accessibili se l'applicativo e' attivo per l'azienda
    - getStudioRoutes() : pagine lato studio, accessibili se l'applicativo e' abilitato per lo studio
*/
@Injectable({
  providedIn: 'root'
})
export class RouteComplianceService {

  constructor() { }

  public getRoutes(): Routes {
    return [];
  }

  public getStudioRoutes(): Routes {
    return [];
  }
}
