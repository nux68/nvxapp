import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';

/*
 Rotte di Risorse aziendali. In Infrastructure e' vuoto: lo valorizza il ramo Assets.
    - getRoutes()       : pagine dell'azienda, accessibili se l'applicativo e' attivo per l'azienda
    - getStudioRoutes() : pagine lato studio, accessibili se l'applicativo e' abilitato per lo studio
*/
@Injectable({
  providedIn: 'root'
})
export class RouteAssetsService {

  constructor() { }

  public getRoutes(): Routes {
    return [];
  }

  public getStudioRoutes(): Routes {
    return [];
  }
}
