import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';

/*
 Rotte del CRM. In Infrastructure e' vuoto: lo valorizza il ramo CRM.
    - getRoutes()       : pagine dell'azienda, accessibili se il CRM e' attivo per l'azienda
    - getStudioRoutes() : pagine lato studio, accessibili se il CRM e' abilitato per lo studio
*/
@Injectable({
  providedIn: 'root'
})
export class RouteCrmService {

  constructor() { }

  public getRoutes(): Routes {
    return [];
  }

  public getStudioRoutes(): Routes {
    return [];
  }
}
