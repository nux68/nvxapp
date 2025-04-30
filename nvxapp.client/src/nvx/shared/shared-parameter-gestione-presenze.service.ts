import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, Observable, of, retry, tap, timer } from 'rxjs';
import { GenericRequest } from '../ClientServer-Service/ModelsBase/generic-request';
import { Par_GiustificativiInModel, Par_GiustificativiModel } from '../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { ParGiustificativiService } from '../ClientServer-Service/GestionePresenze/Par_Giustificativi/par-giustificativi.service';
import { RolesListInModel, RolesModel } from '../ClientServer-Service/Infrastructure/Parameter/Models/roles-model';
import { Dip_Anagrafica_GetAll_InModel, Dip_AnagraficaModel } from '../ClientServer-Service/GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model';
import { DipAnagraficaService } from '../ClientServer-Service/GestionePresenze/Dip_Anagrafica/dip-anagrafica.service';

@Injectable({
  providedIn: 'root'
})
export class SharedParameterGestionePresenzeService {

  private _isLoad = false;
  get IsLoad() { return this._isLoad; }
  set IsLoad(newValue) { this._isLoad = newValue; }



  constructor(private parGiustificativiService: ParGiustificativiService,
              private dipAnagraficaService: DipAnagraficaService   
  ) { }

  

  public InitCall(updateProgress: (calls: any[]) => void): any[] {
    let calls: any[] = [];

    calls.push(

      this.parGiustificativiService.GetAll(new GenericRequest<Par_GiustificativiInModel>(Par_GiustificativiInModel)).pipe(
          tap((result) => {
            this.Par_Giustificativi = result.data.par_Giustificativi
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

      this.dipAnagraficaService.GetAll(new GenericRequest<Dip_Anagrafica_GetAll_InModel>(Dip_Anagrafica_GetAll_InModel)).pipe(
        tap((result) => {
          this.Dip_Anagrafica = result.data.dip_Anagrafica
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
          console.error(`Errore durante il caricamento delle anagrafiche:`, error);
          // Puoi decidere cosa fare in caso di fallimento finale
          return of(null); // Continua per evitare il blocco
        })),

      //this.parGiustificativiService.GetAll(new GenericRequest<Par_GiustificativiInModel>(Par_GiustificativiInModel)).pipe(
      //  tap((result) => {
      //    this.Par_Giustificativi = result.data.par_Giustificativi
      //    updateProgress(calls)
      //  }),
      //  retry({
      //    count: 20, // Numero massimo di tentativi
      //    delay: (error, retryCount) => {
      //      console.error(`Errore rilevato, ritento dopo ${retryCount} secondi:`, error);
      //      return timer(500); // Ritenta dopo 0.5 secondi
      //    }
      //  }),
      //  catchError((error) => {
      //    console.error(`Errore durante il caricamento dei giustificativi:`, error);
      //    // Puoi decidere cosa fare in caso di fallimento finale
      //    return of(null); // Continua per evitare il blocco
      //  })),

      //this.parGiustificativiService.GetAll(new GenericRequest<Par_GiustificativiInModel>(Par_GiustificativiInModel)).pipe(
      //  tap((result) => {
      //    this.Par_Giustificativi = result.data.par_Giustificativi
      //    updateProgress(calls)
      //  }),
      //  retry({
      //    count: 20, // Numero massimo di tentativi
      //    delay: (error, retryCount) => {
      //      console.error(`Errore rilevato, ritento dopo ${retryCount} secondi:`, error);
      //      return timer(500); // Ritenta dopo 0.5 secondi
      //    }
      //  }),
      //  catchError((error) => {
      //    console.error(`Errore durante il caricamento dei giustificativi:`, error);
      //    // Puoi decidere cosa fare in caso di fallimento finale
      //    return of(null); // Continua per evitare il blocco
      //  })),

      //this.parGiustificativiService.GetAll(new GenericRequest<Par_GiustificativiInModel>(Par_GiustificativiInModel)).pipe(
      //  tap((result) => {
      //    this.Par_Giustificativi = result.data.par_Giustificativi
      //    updateProgress(calls)
      //  }),
      //  retry({
      //    count: 20, // Numero massimo di tentativi
      //    delay: (error, retryCount) => {
      //      console.error(`Errore rilevato, ritento dopo ${retryCount} secondi:`, error);
      //      return timer(500); // Ritenta dopo 0.5 secondi
      //    }
      //  }),
      //  catchError((error) => {
      //    console.error(`Errore durante il caricamento dei giustificativi:`, error);
      //    // Puoi decidere cosa fare in caso di fallimento finale
      //    return of(null); // Continua per evitare il blocco
      //  })),


    );
    


    return calls;
  }


  private _par_Giustificativi: Par_GiustificativiModel[] | null = [];

  public get Par_Giustificativi(): Par_GiustificativiModel[] | null {
    return this._par_Giustificativi;
  }
  public set Par_Giustificativi(value: Par_GiustificativiModel[] | null) {
    this._par_Giustificativi = value;
    this._par_GiustificativiSubject.next(value);
  }

  private _par_GiustificativiSubject = new BehaviorSubject<Par_GiustificativiModel[]>([]);
  public get Par_Giustificativi$(): Observable<Par_GiustificativiModel[] | []> {
    return this._par_GiustificativiSubject.asObservable();
  }

  
  

  private _dip_Anagrafica: Dip_AnagraficaModel[] | null = [];

  public get Dip_Anagrafica(): Dip_AnagraficaModel[] | null {
    return this._dip_Anagrafica;
  }
  public set Dip_Anagrafica(value: Dip_AnagraficaModel[] | null) {
    this._dip_Anagrafica = value;
    this._dip_AnagraficaSubject.next(value);
  }

  private _dip_AnagraficaSubject = new BehaviorSubject<Dip_AnagraficaModel[]>([]);
  public get Dip_Anagrafica$(): Observable<Dip_AnagraficaModel[] | []> {
    return this._dip_AnagraficaSubject.asObservable();
  }


}
