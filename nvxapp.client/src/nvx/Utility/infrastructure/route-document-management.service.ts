import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';

/*
 Rotte del documentale. In Infrastructure e' vuoto: lo valorizza il ramo DocumentManagement.
    - getRoutes()       : pagine dell'azienda, accessibili se il documentale e' attivo per l'azienda
    - getStudioRoutes() : pagine lato studio, accessibili se il documentale e' abilitato per lo studio
*/
@Injectable({
  providedIn: 'root'
})
export class RouteDocumentManagementService {

  constructor() { }

  public getRoutes(): Routes {
    return [];
  }

  public getStudioRoutes(): Routes {
    return [];
  }
}
