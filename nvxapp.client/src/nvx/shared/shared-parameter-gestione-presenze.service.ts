import { Injectable } from '@angular/core';
import { catchError, of, retry, tap, timer } from 'rxjs';
import { GenericRequest } from '../ClientServer-Service/ModelsBase/generic-request';
import { Par_GiustificativiInModel, Par_GiustificativiModel } from '../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { ParGiustificativiService } from '../ClientServer-Service/GestionePresenze/Par_Giustificativi/par-giustificativi.service';
import { RolesListInModel } from '../ClientServer-Service/Infrastructure/Parameter/Models/roles-model';

@Injectable({
  providedIn: 'root'
})
export class SharedParameterGestionePresenzeService {

  private _isLoad = false;
  get IsLoad() { return this._isLoad; }
  set IsLoad(newValue) { this._isLoad = newValue; }



  constructor(private parGiustificativiService: ParGiustificativiService) {}

  

  public InitCall(updateProgress: (calls: any[]) => void): any[] {
    let calls: any[] = [];

    calls.push(

      this.parGiustificativiService.GetAll(new GenericRequest<Par_GiustificativiInModel>(Par_GiustificativiInModel)).pipe(
          tap((result) => {
            this._par_Giustificativi = result.data.par_Giustificativi
            updateProgress(calls)
          }),
          retry({
            count: 20, // Numero massimo di tentativi
            delay: (error, retryCount) => {
              console.error(`Errore rilevato, ritento dopo ${retryCount} secondi:`, error);
              return timer(500); // Ritenta dopo 0.5 secondi
            }
          }),
          catchError((error) => {
            console.error(`Errore durante il caricamento dei giustificativi:`, error);
            // Puoi decidere cosa fare in caso di fallimento finale
            return of(null); // Continua per evitare il blocco
          })),

      this.parGiustificativiService.GetAll(new GenericRequest<Par_GiustificativiInModel>(Par_GiustificativiInModel)).pipe(
        tap((result) => {
          this._par_Giustificativi = result.data.par_Giustificativi
          updateProgress(calls)
        }),
        retry({
          count: 20, // Numero massimo di tentativi
          delay: (error, retryCount) => {
            console.error(`Errore rilevato, ritento dopo ${retryCount} secondi:`, error);
            return timer(500); // Ritenta dopo 0.5 secondi
          }
        }),
        catchError((error) => {
          console.error(`Errore durante il caricamento dei giustificativi:`, error);
          // Puoi decidere cosa fare in caso di fallimento finale
          return of(null); // Continua per evitare il blocco
        })),

      this.parGiustificativiService.GetAll(new GenericRequest<Par_GiustificativiInModel>(Par_GiustificativiInModel)).pipe(
        tap((result) => {
          this._par_Giustificativi = result.data.par_Giustificativi
          updateProgress(calls)
        }),
        retry({
          count: 20, // Numero massimo di tentativi
          delay: (error, retryCount) => {
            console.error(`Errore rilevato, ritento dopo ${retryCount} secondi:`, error);
            return timer(500); // Ritenta dopo 0.5 secondi
          }
        }),
        catchError((error) => {
          console.error(`Errore durante il caricamento dei giustificativi:`, error);
          // Puoi decidere cosa fare in caso di fallimento finale
          return of(null); // Continua per evitare il blocco
        })),

      this.parGiustificativiService.GetAll(new GenericRequest<Par_GiustificativiInModel>(Par_GiustificativiInModel)).pipe(
        tap((result) => {
          this._par_Giustificativi = result.data.par_Giustificativi
          updateProgress(calls)
        }),
        retry({
          count: 20, // Numero massimo di tentativi
          delay: (error, retryCount) => {
            console.error(`Errore rilevato, ritento dopo ${retryCount} secondi:`, error);
            return timer(500); // Ritenta dopo 0.5 secondi
          }
        }),
        catchError((error) => {
          console.error(`Errore durante il caricamento dei giustificativi:`, error);
          // Puoi decidere cosa fare in caso di fallimento finale
          return of(null); // Continua per evitare il blocco
        })),


    );
    


    return calls;
  }


  private _par_Giustificativi: Par_GiustificativiModel[] | null = [];

  public get Par_Giustificativi(): Par_GiustificativiModel[] | null {
    return this._par_Giustificativi;
  }



}
