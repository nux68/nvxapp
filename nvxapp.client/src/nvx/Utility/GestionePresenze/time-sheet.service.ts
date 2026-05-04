import { Injectable } from '@angular/core';
import { catchError, Observable, of, throwError } from 'rxjs';
import { map } from 'rxjs/operators'; // Import map operator if you plan real sorting/processing
import { Dip_GG_Timbratura_GetAll_InModel, Dip_GG_TimbraturaModel, TipoTimbratura } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { HoverPopupData } from '../../shared/components/infrastructure/hover-popup/hover-popup.component';
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
import { Dip_GG_ResultModel, GG_ResultStato } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Result/Models/dip-gg-result-model';
import { Dip_GG_CausaliModel } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Causali/Models/dip-gg-causali-model';





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
            daySlot: []
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
        dip_GG_Result: [],
        dip_GG_Causali: [],
        daySlot:[]
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
          dip_GG_Result: x.data?.dip_GG_AllData_OutModel?.dip_GG_Result ?? [],
          dip_GG_Causali: x.data?.dip_GG_AllData_OutModel?.dip_GG_Causali ?? [],
          daySlot: x.data?.orariSchema_4User_OutModel?.daySlots ?? []
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
      dip_GG_Richiesta: [],
      daySlot: []
    };

    monthData.dip_GG_Richiesta = remoteData.dip_GG_Richiesta;
    monthData.daySlot = remoteData.daySlot;
    

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


    // Raggruppa le causali per giorno
    const causaliByDay = new Map<number, Dip_GG_CausaliModel[]>();
    remoteData.dip_GG_Causali.forEach(causale => {
      const day = new Date(causale.data).getDate();

      // Verifica se la causale appartiene al mese corretto
      const cauMonth = new Date(causale.data).getMonth();
      const cauYear = new Date(causale.data).getFullYear();

      if (cauMonth === month && cauYear === year) {
        if (!causaliByDay.has(day)) {
          causaliByDay.set(day, []);
        }
        causaliByDay.get(day)?.push(causale);
      }
    });


    // Assegna il Result al giorno
    const resultByDay = new Map<number, Dip_GG_ResultModel[]>();
    remoteData.dip_GG_Result.forEach(result => {

      
      const day = parseInt(result.data.substring(8, 10));
      

      if (!resultByDay.has(day)) {
        resultByDay.set(day, []);
      }
      resultByDay.get(day)?.push(result);

    });


    // Unisci i dati per creare i record giornalieri
    const allDays = new Set<number>([
      ...Array.from(timbratureByDay.keys()),
      ...Array.from(giustificativiByDay.keys()),
      ...Array.from(resultByDay.keys()),
      ...Array.from(causaliByDay.keys())
    ]);

    allDays.forEach(day => {
      monthData.days[day] = {
        date: new Date(year, month, day),
        dip_GG_Timbratura: timbratureByDay.get(day) || [],
        dip_GG_Giustificativi: giustificativiByDay.get(day) || [],
        dip_GG_Result: this.get_dip_GG_Result(remoteData, day),
        dip_GG_Causali: causaliByDay.get(day) || [],
        idDip_RapportoLavoro:0
      };
    });

    return monthData;
  }

  private get_dip_GG_Result(remoteData: TimeSheetRemoteData, day:number): Dip_GG_ResultModel {
    var retVal = remoteData.dip_GG_Result.filter(t => parseInt(t.data.substring(8, 10))  === day)
    return retVal.length > 0 ? retVal[0] : null;
  }


  private sortTimestampsByTime(timestamps: Dip_GG_TimbraturaModel[] | undefined): Dip_GG_TimbraturaModel[] {
    if (!timestamps) return [];

    return [...timestamps].sort((a, b) => this.dateTimeUtilService.timeToMinutes(a.timbratura) - this.dateTimeUtilService.timeToMinutes(b.timbratura));

  }

  get_IdDip_RapportoLavoro(currentMonth: MonthData,idAspNetUsers: string, data: Date): number {
    const dataStr = this.dateTimeUtilService.DateTo_ggmmyyyy(data) // "yyyy-MM-dd"

    const slot = currentMonth.daySlot?.find(s => s.idAspNetUsers === idAspNetUsers &&
                                                 this.dateTimeUtilService.DateTo_ggmmyyyy(new Date(s.data))  === dataStr );

    return slot?.idDip_RapportoLavoro ?? 0;
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
    //if (just)
    //  return just.backgroundColor;
    //else
    //  return null;

    switch (ggJust.richiestaStato) {

      case StatoRichiesta.Diretta:
      case StatoRichiesta.Approvata:
        return null;
        break;

      case StatoRichiesta.Immessa:
      if (just)
        return just.backgroundColor;
      else
        return null;

      default:
        return null;
        break;
    }


  }

  get_Dip_GG_Giustificativi_txtColor(ggJust: Dip_GG_GiustificativiModel): string {
    const just = this.sharedParameterGestionePresenzeService.Par_Giustificativi.find(x => x.id == ggJust.idPar_Giustificativi);
    //if (just)
    //  return just.textColor;
    //else
    //  return null;

    switch (ggJust.richiestaStato) {

      case StatoRichiesta.Diretta:
      case StatoRichiesta.Approvata:
        if (just)
          return just.backgroundColor;
        else
          return null;

      case StatoRichiesta.Immessa:
        if (just)
          return just.textColor;
        else
          return null;

      default:
        return null;
        break;
    }

  }


  get_StatoDay_icon(dip_GG_ResultModel: Dip_GG_ResultModel): string {

    const stato = Number(dip_GG_ResultModel.stato);

    let main: GG_ResultStato;

    if (stato & GG_ResultStato.Err) {
      main = GG_ResultStato.Err;
    } else if (stato & GG_ResultStato.Warning) {
      main = GG_ResultStato.Warning;
    } else if (stato & GG_ResultStato.Locked) {
      main = GG_ResultStato.Locked;
    } else if (stato & GG_ResultStato.OK) {
      main = GG_ResultStato.OK;
    } else {
      main = GG_ResultStato.Init;
    }

    switch (main) {
      case GG_ResultStato.Init:
        return null;

      case GG_ResultStato.OK:
        return 'checkmark-done-outline';

      case GG_ResultStato.Locked:
        return 'lock-closed-outline';

      case GG_ResultStato.Warning:
        return 'alert-outline';

      case GG_ResultStato.Err:
        return 'close-outline';

      default:
        return null;
    }
  }

  get_StatoDay_color(dip_GG_ResultModel: Dip_GG_ResultModel): string {
    

    const stato = Number(dip_GG_ResultModel.stato);

    let main: GG_ResultStato;

    if (stato & GG_ResultStato.Err) {
      main = GG_ResultStato.Err;
    } else if (stato & GG_ResultStato.Warning) {
      main = GG_ResultStato.Warning;
    } else if (stato & GG_ResultStato.Locked) {
      main = GG_ResultStato.Locked;
    } else if (stato & GG_ResultStato.OK) {
      main = GG_ResultStato.OK;
    } else {
      main = GG_ResultStato.Init;
    }

    switch (main) {
      case GG_ResultStato.Init:
        return "var(--ion-color-medium)" //grigio
        

      case GG_ResultStato.OK:
        return "var(--ion-color-success)"

      case GG_ResultStato.Locked:
        return "var(--ion-color-medium)" // grigio

      case GG_ResultStato.Warning:
        return "var(--ion-color-warning)"

      case GG_ResultStato.Err:
        return "var(--ion-color-danger)"

      default:
        return "var(--ion-color-medium)" // grigio
    }


  }


  //get_Dip_GG_Timbratura_backColor(record: Dip_GG_TimbraturaModel): string {
  //  // Ottieni il valore della variabile CSS dal root
  //  const root = document.documentElement;

  //  let value = '';

  //  switch (record.timbraturaTipo) {
  //    case TipoTimbratura.Entrata:
  //      value = getComputedStyle(root).getPropertyValue('--ion-color-primary').trim();
  //      return value || '#3880ff';
  //      break;
  //    case TipoTimbratura.Uscita:
  //      value = getComputedStyle(root).getPropertyValue('--ion-color-medium').trim();
  //      return value || '#92949c';
  //      break;
  //  }


  //  return '#3880ff';
  //}

  //get_Dip_GG_Timbratura_txtColor(record: Dip_GG_TimbraturaModel): string {
  //  // Ottieni il valore della variabile CSS dal root
  //  const root = document.documentElement;

  //  let value = '';

  //  switch (record.timbraturaTipo) {
  //    case TipoTimbratura.Entrata:
  //      value = getComputedStyle(root).getPropertyValue('--ion-color-primary-contrast').trim();
  //      return value || '#ffffff';
  //      break;
  //    case TipoTimbratura.Uscita:
  //      value = getComputedStyle(root).getPropertyValue('--ion-color-medium-contrast').trim();
  //      return value || '#ffffff';
  //      break;
  //  }


  //  return '#ffffff';
  //}

  get_Dip_GG_Timbratura_backColor(record: Dip_GG_TimbraturaModel): string {
    const root = document.documentElement;

    


    let value = '';

    switch (record.richiestaStato) {

      case StatoRichiesta.Diretta:
      case StatoRichiesta.Approvata:

        switch (record.timbraturaTipo) {
          case TipoTimbratura.SenzaVerso:  // ancora da riconoscere
          case TipoTimbratura.Entrata:
          case TipoTimbratura.Uscita:
          case TipoTimbratura.Attivita:
            return null;
        }

        break;

      case StatoRichiesta.Immessa:
          value = getComputedStyle(root).getPropertyValue('--ion-color-secondary').trim();
          return value || '#ffffff';

      default:
        return null;
        break;

    }


    return '#3880ff';
  }

  get_Dip_GG_Timbratura_txtColor(record: Dip_GG_TimbraturaModel): string {
    const root = document.documentElement;

    
    let value = '';

    switch (record.richiestaStato) {
      case  StatoRichiesta.Diretta:
      case StatoRichiesta.Approvata:

        switch (record.timbraturaTipo)
        {
          case TipoTimbratura.SenzaVerso:  // ancora da riconoscere
            value = getComputedStyle(root).getPropertyValue('--ion-color-secondary').trim();  //(giallo)
            return value || '#ffffff';
            break;

          case TipoTimbratura.Entrata:
              value = getComputedStyle(root).getPropertyValue('--ion-color-primary').trim(); // (blue)
              return value || '#ffffff';

          case TipoTimbratura.Uscita:
              value = getComputedStyle(root).getPropertyValue('--ion-color-medium').trim(); // (grigio)
            return value || '#ffffff';

          case TipoTimbratura.Attivita:
            value = getComputedStyle(root).getPropertyValue('--ion-color-secondary').trim(); //(giallo) da gestire
            return value || '#ffffff';
            break;
        }

        break;

      case StatoRichiesta.Immessa:
        
        value = getComputedStyle(root).getPropertyValue('--ion-color-secondary-contrast').trim(); // grigio scuro - su fondo giallo
        return value || '#ffffff';
        break;

      default:
        return getComputedStyle(root).getPropertyValue('--ion-text-color').trim() || '#000000';
        break;

      }


    return '#ffffff';
  }

  get_Dip_GG_Causali_backColor(ggCau: Dip_GG_CausaliModel): string {
    //const just = this.sharedParameterGestionePresenzeService.Par_Causali.find(x => x.id == ggCau.idPar_Causali);
    //if (just)
    //  return just.backgroundColor;
    //else
    //  return null;

    const root = document.documentElement;

    let value = '';

    value = getComputedStyle(root).getPropertyValue('--ion-color-medium').trim();
    return value || '#92949c';

  }

  get_Dip_GG_Causali_txtColor(ggCau: Dip_GG_CausaliModel): string {
    //const just = this.sharedParameterGestionePresenzeService.Par_Causali.find(x => x.id == ggCau.idPar_Causali);
    //if (just)
    //  return just.textColor;
    //else
    //  return null;

    const root = document.documentElement;

    let value = '';
    
    value = getComputedStyle(root).getPropertyValue('--ion-color-medium-contrast').trim();
    return value || '#ffffff';

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

  get_DayPopupStato(result: Dip_GG_ResultModel): HoverPopupData {
    if (!result) {
      return { title: 'Stato giornata', content: 'Nessun dato disponibile' };
    }

    const stato = Number(result.stato);
    const activeFlags: string[] = [];

    // --- Stato principale ---
    const statoLabels: Partial<Record<GG_ResultStato, string>> = {
      [GG_ResultStato.Init]:    '⬜ Non ancora elaborata',
      [GG_ResultStato.OK]:      '✅ Giornata corretta',
      [GG_ResultStato.Locked]:  '🔒 Giornata bloccata',
      [GG_ResultStato.Warning]: '⚠️ Presenza di avvertimenti',
      [GG_ResultStato.Err]:     '❌ Presenza di errori',
    };

    // --- Dettagli errori ---
    const errLabels: Partial<Record<GG_ResultStato, string>> = {
      [GG_ResultStato.Err_TimbratureMancanti]: 'Timbrature mancanti rispetto all\'orario atteso',
      [GG_ResultStato.Err_2]:  'Ore lavorate inferiori alle ore previste',
      [GG_ResultStato.Err_3]:  'Ore lavorate superiori alle ore previste',
      [GG_ResultStato.Err_4]:  'Errore 4',
      [GG_ResultStato.Err_5]:  'Errore 5',
      [GG_ResultStato.Err_6]:  'Errore 6',
      [GG_ResultStato.Err_7]:  'Errore 7',
      [GG_ResultStato.Err_8]:  'Errore 8',
      [GG_ResultStato.Err_9]:  'Errore 9',
      [GG_ResultStato.Err_10]: 'Errore 10',
      [GG_ResultStato.Err_11]: 'Errore 11',
      [GG_ResultStato.Err_12]: 'Errore 12',
      [GG_ResultStato.Err_13]: 'Errore 13',
      [GG_ResultStato.Err_14]: 'Errore 14',
      [GG_ResultStato.Err_15]: 'Errore 15',
      [GG_ResultStato.Err_16]: 'Errore 16',
      [GG_ResultStato.Err_17]: 'Errore 17',
      [GG_ResultStato.Err_18]: 'Errore 18',
      [GG_ResultStato.Err_19]: 'Errore 19',
      [GG_ResultStato.Err_20]: 'Errore 20',
    };

    // --- Dettagli warning ---
    const warnLabels: Partial<Record<GG_ResultStato, string>> = {
      [GG_ResultStato.Warning_1]:  'Avvertimento 1',
      [GG_ResultStato.Warning_2]:  'Avvertimento 2',
      [GG_ResultStato.Warning_3]:  'Avvertimento 3',
      [GG_ResultStato.Warning_4]:  'Avvertimento 4',
      [GG_ResultStato.Warning_5]:  'Avvertimento 5',
      [GG_ResultStato.Warning_6]:  'Avvertimento 6',
      [GG_ResultStato.Warning_7]:  'Avvertimento 7',
      [GG_ResultStato.Warning_8]:  'Avvertimento 8',
      [GG_ResultStato.Warning_9]:  'Avvertimento 9',
      [GG_ResultStato.Warning_10]: 'Avvertimento 10',
      [GG_ResultStato.Warning_11]: 'Avvertimento 11',
      [GG_ResultStato.Warning_12]: 'Avvertimento 12',
      [GG_ResultStato.Warning_13]: 'Avvertimento 13',
      [GG_ResultStato.Warning_14]: 'Avvertimento 14',
      [GG_ResultStato.Warning_15]: 'Avvertimento 15',
      [GG_ResultStato.Warning_16]: 'Avvertimento 16',
      [GG_ResultStato.Warning_17]: 'Avvertimento 17',
      [GG_ResultStato.Warning_18]: 'Avvertimento 18',
      [GG_ResultStato.Warning_19]: 'Avvertimento 19',
      [GG_ResultStato.Warning_20]: 'Avvertimento 20',
    };

    // Stato principale (primo attivo tra Init/OK/Locked/Warning/Err)
    const statoPrincipale = Object.entries(statoLabels).find(([flag]) => stato & Number(flag));
    const titleText = statoPrincipale ? statoPrincipale[1] : 'Stato sconosciuto';

    // Dettagli errori attivi
    for (const [flag, label] of Object.entries(errLabels)) {
      if (stato & Number(flag)) activeFlags.push(`• ${label}`);
    }

    // Dettagli warning attivi (bit > 31: usa confronto numerico)
    for (const [flag, label] of Object.entries(warnLabels)) {
      if (stato & Number(flag)) activeFlags.push(`• ${label}`);
    }

    const extraInfo: Record<string, string> = {};
    if (result.hH_Teo != null) extraInfo['Ore teoriche'] = result.hH_Teo;
    if (result.hH_Lav != null) extraInfo['Ore lavorate'] = result.hH_Lav;

    return {
      title: result.data
        ? new Date(result.data).toLocaleDateString('it-IT', { weekday: 'long', day: '2-digit', month: 'long' })
        : 'Stato giornata',
      content: activeFlags.length > 0 ? activeFlags.join('\n') : titleText,
      extraInfo
    };
  }

  get_DayPopupData(
    result: Dip_GG_ResultModel,
    timbrature: Dip_GG_TimbraturaModel[],
    giustificativi: Dip_GG_GiustificativiModel[],
    causali: Dip_GG_CausaliModel[]
  ): HoverPopupData {

    const extraInfo: Record<string, string> = {};

    if (result?.hH_Teo != null)  extraInfo['Ore teoriche']  = result.hH_Teo;
    if (result?.hH_Lav != null)  extraInfo['Ore lavorate']  = result.hH_Lav;
    if (timbrature?.length > 0)  extraInfo['Timbrature']    = `${timbrature.length}`;
    if (giustificativi?.length > 0) extraInfo['Giustificativi'] = `${giustificativi.length}`;
    if (causali?.length > 0)     extraInfo['Causali']       = `${causali.length}`;

    return {
      title: result?.data ? new Date(result.data).toLocaleDateString('it-IT', { weekday: 'long', day: '2-digit', month: 'long' }) : 'Dettaglio giorno',
      content: this.get_StatoDay_icon(result) ? `Stato: ${this.get_StatoDay_color(result)}` : 'Nessun risultato calcolato',
      extraInfo
    };
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

