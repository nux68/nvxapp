import { Injectable } from '@angular/core';
import { catchError, Observable, of, throwError } from 'rxjs';
import { map } from 'rxjs/operators'; // Import map operator if you plan real sorting/processing
import { Dip_GG_Timbratura_GetAll_InModel, Dip_GG_TimbraturaModel, TipoTimbratura } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { Dip_GG_Richiesta_GetAll4User_InModel, Dip_GG_RichiestaModel, StatoRichiesta } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { Dip_GG_Giustificativi_GetAll_InModel, Dip_GG_GiustificativiModel } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';
import { DipGGGiustificativiService } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/dip-gg-giustificativi.service';
import { DipGGTimbraturaService } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/dip-gg-timbratura.service';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { forkJoin } from 'rxjs/internal/observable/forkJoin';
import { DipGGRichiestaService } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { MonthData, TimeSheetRemoteData } from './time-sheet-common-data';
import { SharedParameterGestionePresenzeService } from '../../shared/shared-parameter-gestione-presenze.service';
import { DateTimeUtilService } from '../infrastructure/date-time-util.service';
import { TipoRichiestaToShortTextPipe } from '../../shared/pipe/GestionePresenze/tipo-richiesta-to-short-text.pipe';
import { StatoRichiestaLongTextPipe } from '../../shared/pipe/GestionePresenze/stato-richiesta-long-text.pipe';
import { Timesheet_AllData_InModel } from '../../ClientServer-Service/GestionePresenze/TimeSheet_EngineService/Models/time-sheet-engine-model';
import { TimeSheetEngineService } from '../../ClientServer-Service/GestionePresenze/TimeSheet_EngineService/time-sheet-engine.service';
import { Dip_GG_ResultModel } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Result/Models/dip-gg-result-model';





@Injectable({
  providedIn: 'root'
})
export class TimeSheetService {

  constructor(private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private dipGGGiustificativiService: DipGGGiustificativiService,
              private dipGGTimbraturaService: DipGGTimbraturaService,
              private dipGGRichiestaService: DipGGRichiestaService,
              public timeSheetEngineService: TimeSheetEngineService,
              public dateTimeUtilService: DateTimeUtilService) {
  }


  getMonthData(year: number, month: number, idAspNetUsers: string|null): Observable<MonthData> {
    return this.getMonthDataFromServer(year, month, idAspNetUsers).pipe(
      map(remoteData => this.transformRemoteDataToMonthData(remoteData, year, month)),
      catchError(error => {
        console.warn(`Error fetching data from server: ${error}. Falling back to mock data.`);
          return of({
            year: year,
            month: month,
            days: {},
            dip_GG_Richiesta: [],
          });
      })
    );
  }

  getMonthDataFromServer(year: number, month: number, idAspNetUsers: string | null): Observable<TimeSheetRemoteData> {

    if (year == undefined || month == undefined) {
      const remoteData: TimeSheetRemoteData = {
        dip_GG_Giustificativi: [],
        dip_GG_Timbratura: [],
        dip_GG_Richiesta: [],
        dip_GG_Result:[]
      };

      return of(remoteData);
    }

    let request: GenericRequest<Timesheet_AllData_InModel> = new GenericRequest<Timesheet_AllData_InModel>(Timesheet_AllData_InModel);
    request.data = new Timesheet_AllData_InModel();
    request.data.dal = new Date(Date.UTC(year, month, 1)).toISOString();
    request.data.al = new Date(Date.UTC(year, month + 1, 0)).toISOString();
    if (idAspNetUsers != null) {
      request.data.usersId = [idAspNetUsers];
    }

    


    return this.timeSheetEngineService.Get_Timesheet_AllData(request).pipe(
      map(x => {
        const remoteData: TimeSheetRemoteData = {
          dip_GG_Giustificativi: x.data?.dip_GG_AllData_OutModel?.dip_GG_Giustificativi ?? [],
          dip_GG_Timbratura: x.data?.dip_GG_AllData_OutModel?.dip_GG_Timbratura ?? [],
          dip_GG_Richiesta: x.data?.dip_GG_AllData_OutModel?.dip_GG_Richiesta ?? [],
          dip_GG_Result: x.data?.dip_GG_AllData_OutModel?.dip_GG_Result ?? []
        };
        console.log('getMonthDataFromServer - combined data:', remoteData);
        return remoteData;
      })
    );

    //////this.timeSheetEngineService.Get_Timesheet_AllData(request).subscribe(x => {

      

    //////  const giustificativiArray = x.data.dip_GG_AllData_OutModel.dip_GG_Giustificativi || [];
    //////  const timbratureArray = x.data.dip_GG_AllData_OutModel.dip_GG_Timbratura || [];
    //////  const richiesteArray = x.data.dip_GG_AllData_OutModel.dip_GG_Richiesta || [];

    //////  // Crea l'oggetto finale TimeSheetRemoteData
    //////  const remoteData: TimeSheetRemoteData = {
    //////    dip_GG_Giustificativi: giustificativiArray,
    //////    dip_GG_Timbratura: timbratureArray,
    //////    dip_GG_Richiesta: richiesteArray
    //////  };

    //////  console.log('Both calls finished. Combined data:', remoteData);
    //////  return remoteData; // Return the structured data

    //////});



    //////let request_Just = new GenericRequest<Dip_GG_Giustificativi_GetAll_InModel>(Dip_GG_Giustificativi_GetAll_InModel);
    //////request_Just.data.year = year;
    //////request_Just.data.month = month + 1;
    //////request_Just.data.idAspNetUsers = idAspNetUsers;

    //////let request_clock = new GenericRequest<Dip_GG_Timbratura_GetAll_InModel>(Dip_GG_Timbratura_GetAll_InModel);
    //////request_clock.data.year = year;
    //////request_clock.data.month = month + 1;
    //////request_clock.data.idAspNetUsers = idAspNetUsers;

    //////let request_rich = new GenericRequest<Dip_GG_Richiesta_GetAll4User_InModel>(Dip_GG_Richiesta_GetAll4User_InModel);
    //////request_rich.data.year = year;
    //////request_rich.data.month = month + 1;
    //////request_rich.data.idAspNetUsers = idAspNetUsers;
    

    //////// 2. Define the Observables for the API calls (DO NOT subscribe yet)
    //////const justificationsObservable$ = this.dipGGGiustificativiService.GetAll(request_Just);
    //////const clockingsObservable$ = this.dipGGTimbraturaService.GetAll(request_clock);
    //////const requestObservable$ = this.dipGGRichiestaService.GetAll4User(request_clock);

    

    //////// 3. Use forkJoin to execute both Observables in parallel
    //////// It will emit an object with the results once BOTH calls complete
    //////return forkJoin({
    //////  // Assign keys to easily access the results later
    //////  justResult: justificationsObservable$,
    //////  clockResult: clockingsObservable$,
    //////  requestResult: requestObservable$
    //////}).pipe(
    //////  // 4. Use the 'map' operator to transform the combined results
    //////  map(results => {

    //////    const giustificativiArray = results.justResult?.data?.dip_GG_Giustificativi || [];
    //////    const timbratureArray = results.clockResult?.data?.dip_GG_Timbratura || [];
    //////    const richiesteArray = results.requestResult?.data?.dip_GG_Richiesta || [];

    //////    // Crea l'oggetto finale TimeSheetRemoteData
    //////    const remoteData: TimeSheetRemoteData = {
    //////      dip_GG_Giustificativi: giustificativiArray,
    //////      dip_GG_Timbratura: timbratureArray,
    //////      dip_GG_Richiesta: richiesteArray
    //////    };

    //////    console.log('Both calls finished. Combined data:', remoteData);
    //////    return remoteData; // Return the structured data
    //////  }),
    //////  // 5. Optional: Add error handling for the forkJoin
    //////  catchError(error => {
    //////    console.error("Error fetching month data (one or both calls failed):", error);
    //////    return throwError(() => new Error('Failed to load data for month ' + month + '/' + year));
    //////  })
    //////);
    //////// The method now correctly returns an Observable<MonthData>


  }
    
  private transformRemoteDataToMonthData(remoteData: TimeSheetRemoteData, year: number, month: number): MonthData {
    const monthData: MonthData = {
      year: year,
      month: month,
      days: {},
      dip_GG_Richiesta:[] 
    };

    monthData.dip_GG_Richiesta = remoteData.dip_GG_Richiesta;

    // Raggruppa le timbrature per giorno
    const timbratureByDay = new Map<number, Dip_GG_TimbraturaModel[]>();
    remoteData.dip_GG_Timbratura.forEach(timbratura => {
      const day = new Date(timbratura.timbratura).getDate();

      // Verifica se la timbratura appartiene al mese corretto
      const timbraMonth = new Date(timbratura.timbratura).getMonth();
      const timbraYear = new Date(timbratura.timbratura).getFullYear();

      if (timbraMonth === month && timbraYear === year) {
        if (!timbratureByDay.has(day)) {
          timbratureByDay.set(day, []);
        }
        timbratureByDay.get(day)?.push(timbratura);
      }
    });

    // Raggruppa i giustificativi per giorno
    const giustificativiByDay = new Map<number, Dip_GG_GiustificativiModel[]>();
    remoteData.dip_GG_Giustificativi.forEach(giustificativo => {
      const day = new Date(giustificativo.data).getDate();

      // Verifica se il giustificativo appartiene al mese corretto
      const giustMonth = new Date(giustificativo.data).getMonth();
      const giustYear = new Date(giustificativo.data).getFullYear();

      if (giustMonth === month && giustYear === year) {
        if (!giustificativiByDay.has(day)) {
          giustificativiByDay.set(day, []);
        }
        giustificativiByDay.get(day)?.push(giustificativo);
      }
    });


    // Raggruppa i giustificativi per giorno
    const resultByDay = new Map<number, Dip_GG_ResultModel[]>();
    remoteData.dip_GG_Result.forEach(result => {
      
      const day = parseInt(result.data.toString().substring(0, 2));

      if (!resultByDay.has(day)) {
        resultByDay.set(day, []);
      }
      resultByDay.get(day)?.push(result);

    });


    // Unisci i dati per creare i record giornalieri
    const allDays = new Set<number>([
      ...Array.from(timbratureByDay.keys()),
      ...Array.from(giustificativiByDay.keys()),
      ...Array.from(resultByDay.keys())
    ]);

    allDays.forEach(day => {
      monthData.days[day] = {
        date: new Date(year, month, day),
        dip_GG_Timbratura: timbratureByDay.get(day) || [],
        dip_GG_Giustificativi: giustificativiByDay.get(day) || [],
        dip_GG_Result: this.get_dip_GG_Result(remoteData, day) 
      };
    });

    return monthData;
  }

  private get_dip_GG_Result(remoteData: TimeSheetRemoteData, day:number): Dip_GG_ResultModel {
    var retVal = remoteData.dip_GG_Result.filter(t => parseInt(t.data.toString().substring(0, 2))  === day)
    return retVal.length > 0 ? retVal[0] : null;
  }


  private sortTimestampsByTime(timestamps: Dip_GG_TimbraturaModel[] | undefined): Dip_GG_TimbraturaModel[] {
    if (!timestamps) return [];

    return [...timestamps].sort((a, b) => this.dateTimeUtilService.timeToMinutes(a.timbratura) - this.dateTimeUtilService.timeToMinutes(b.timbratura));

  }


  get_StatoRichiesta_icon(dip_GG_Richiesta: Dip_GG_RichiestaModel): string {

    if (dip_GG_Richiesta == null || (dip_GG_Richiesta != null && (dip_GG_Richiesta.richiestaStato == undefined && dip_GG_Richiesta.revocaStato == undefined)))
      return null;

    if (dip_GG_Richiesta.revocaStato == null) {
      switch (dip_GG_Richiesta.richiestaStato) {
        case StatoRichiesta.Diretta:
          return 'checkmark-circle'; // Inserimento diretto
        case StatoRichiesta.Immessa:
          return 'time-outline'; // In attesa
        case StatoRichiesta.ApprovazioneInCorso:
          return 'hourglass-outline'; // In corso
        case StatoRichiesta.ParzialmenteApprovata:
          return 'alert-circle-outline'; // Parzialmente approvata
        case StatoRichiesta.Approvata:
          return 'checkmark-circle-outline'; // Approvata
        case StatoRichiesta.Rifiutata:
          return 'close-circle-outline'; // Rifiutata
        case StatoRichiesta.Cancellata:
          return 'trash-outline'; // Cancellata
        default:
          return 'help-circle-outline'; // Stato sconosciuto
      }
    }
    else {
      switch (dip_GG_Richiesta.revocaStato) {
        case StatoRichiesta.Diretta:
          return 'checkmark-circle'; // Inserimento diretto
        case StatoRichiesta.Immessa:
          return 'time-outline'; // In attesa
        case StatoRichiesta.ApprovazioneInCorso:
          return 'hourglass-outline'; // In corso
        case StatoRichiesta.ParzialmenteApprovata:
          return 'alert-circle-outline'; // Parzialmente approvata
        case StatoRichiesta.Approvata:
          return 'checkmark-circle-outline'; // Approvata
        case StatoRichiesta.Rifiutata:
          return 'close-circle-outline'; // Rifiutata
        case StatoRichiesta.Cancellata:
          return 'trash-outline'; // Cancellata
        default:
          return 'help-circle-outline'; // Stato sconosciuto
      }
    }

   

  }

  get_StatoRichiesta_text(status: StatoRichiesta): string {

    const statoRichiesta = new StatoRichiestaLongTextPipe();
    return statoRichiesta.transform(status);
    
  }

  get_Dip_GG_Giustificativi_backColor(ggJust: Dip_GG_GiustificativiModel): string {
    const just = this.sharedParameterGestionePresenzeService.Par_Giustificativi.find(x => x.id == ggJust.idPar_Giustificativi);
    if (just)
      return just.backgroundColor;
    else
      return null;
  }

  get_Dip_GG_Giustificativi_txtColor(ggJust: Dip_GG_GiustificativiModel): string {
    const just = this.sharedParameterGestionePresenzeService.Par_Giustificativi.find(x => x.id == ggJust.idPar_Giustificativi);
    if (just)
      return just.textColor;
    else
      return null;
  }

  get_Dip_GG_Timbratura_backColor(record: Dip_GG_TimbraturaModel): string {
    // Ottieni il valore della variabile CSS dal root
    const root = document.documentElement;

    let value = '';

    switch (record.timbraturaTipo) {
      case TipoTimbratura.Entrata:
        value = getComputedStyle(root).getPropertyValue('--ion-color-primary').trim();
        return value || '#3880ff';
        break;
      case TipoTimbratura.Uscita:
        value = getComputedStyle(root).getPropertyValue('--ion-color-medium').trim();
        return value || '#92949c';
        break;
    }


    return '#3880ff';
  }

  get_Dip_GG_Timbratura_txtColor(record: Dip_GG_TimbraturaModel): string {
    // Ottieni il valore della variabile CSS dal root
    const root = document.documentElement;

    let value = '';

    switch (record.timbraturaTipo) {
      case TipoTimbratura.Entrata:
        value = getComputedStyle(root).getPropertyValue('--ion-color-primary-contrast').trim();
        return value || '#ffffff';
        break;
      case TipoTimbratura.Uscita:
        value = getComputedStyle(root).getPropertyValue('--ion-color-medium-contrast').trim();
        return value || '#ffffff';
        break;
    }


    return '#ffffff';
  }


  get_StatoRichiesta_Approval_text(dip_GG_Richiesta: Dip_GG_RichiestaModel): string {

    if (dip_GG_Richiesta == null || (dip_GG_Richiesta != null && (dip_GG_Richiesta.richiestaStato == undefined && dip_GG_Richiesta.revocaStato == undefined)))
      return '';

    if (dip_GG_Richiesta.revocaStato == null) {
      return this.get_StatoRichiesta_text(dip_GG_Richiesta.richiestaStato);
    }
    else {
      return "(" + this.get_StatoRichiesta_text(dip_GG_Richiesta.richiestaStato) + ") Revoca " + this.get_StatoRichiesta_text(dip_GG_Richiesta.revocaStato); 
    }
    
  }


  public Dip_GG_Richiesta_User_Can_Delete(dip_GG_Richiesta: Dip_GG_RichiestaModel) {

    if (dip_GG_Richiesta == null || (dip_GG_Richiesta != null && (dip_GG_Richiesta.richiestaStato == undefined && dip_GG_Richiesta.revocaStato == undefined)))
      return false;

    if (dip_GG_Richiesta.revocaStato == null) {

      if (dip_GG_Richiesta.richiestaStato == StatoRichiesta.Immessa ||
        dip_GG_Richiesta.richiestaStato == StatoRichiesta.ApprovazioneInCorso ||
        dip_GG_Richiesta.richiestaStato == StatoRichiesta.Approvata)
        return true;
      return false;
    }
    else {
      if (dip_GG_Richiesta.revocaStato == StatoRichiesta.Immessa ||
        dip_GG_Richiesta.revocaStato == StatoRichiesta.ApprovazioneInCorso)
        return true;
      return false;
    }
  }

  public Dip_GG_Richiesta_Admin_Can_Approve(dip_GG_Richiesta: Dip_GG_RichiestaModel) {

    if (dip_GG_Richiesta == null || (dip_GG_Richiesta != null && (dip_GG_Richiesta.richiestaStato == undefined && dip_GG_Richiesta.revocaStato == undefined)))
      return false;

    if (dip_GG_Richiesta.revocaStato == null) {
      

      if (dip_GG_Richiesta.richiestaStato != StatoRichiesta.Immessa)
        return false;

      return true;
    }
    else {
      if (dip_GG_Richiesta.revocaStato != StatoRichiesta.Immessa)
        return false;

      return true;
    }

  }

  public Dip_GG_Richiesta_Admin_Can_Reject(dip_GG_Richiesta: Dip_GG_RichiestaModel) {


    if (dip_GG_Richiesta == null || (dip_GG_Richiesta != null  && (dip_GG_Richiesta.richiestaStato == undefined && dip_GG_Richiesta.revocaStato == undefined) ) )
      return false;

    if (dip_GG_Richiesta.revocaStato == null) {
      if (dip_GG_Richiesta.richiestaStato != StatoRichiesta.Immessa)
        return false;

      return true;
    }
    else {
      if (dip_GG_Richiesta.revocaStato != StatoRichiesta.Immessa)
        return false;

      return true;
    }

  }

  

}

