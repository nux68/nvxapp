import { Injectable } from '@angular/core';
import { catchError, Observable, of, throwError } from 'rxjs';
import { map } from 'rxjs/operators'; // Import map operator if you plan real sorting/processing
import { Dip_GG_Timbratura_GetAll_InModel, Dip_GG_TimbraturaModel, TipoTimbratura } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { StatoRichiesta } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { Dip_GG_Giustificativi_GetAll_InModel, Dip_GG_GiustificativiModel } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';
import { DipGGGiustificativiService } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/dip-gg-giustificativi.service';
import { DipGGTimbraturaService } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/dip-gg-timbratura.service';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { forkJoin } from 'rxjs/internal/observable/forkJoin';



export interface DayRecord {
  date: Date;
  dip_GG_Timbratura: Dip_GG_TimbraturaModel[];
  dip_GG_Giustificativi: Dip_GG_GiustificativiModel[];
}


export interface MonthData {
  year: number;
  month: number;  // 0-11 (gennaio = 0)
  days: { [key: number]: DayRecord };  // Mappa giorno -> record
}


export interface TimeSheetRemoteData {
  dip_GG_Timbratura: Dip_GG_TimbraturaModel[];
  dip_GG_Giustificativi: Dip_GG_GiustificativiModel[];
}



@Injectable({
  providedIn: 'root'
})
export class MokeTimeSheetService {

  constructor(private dipGGGiustificativiService: DipGGGiustificativiService,
              private dipGGTimbraturaService: DipGGTimbraturaService) { }


  getMonthData(year: number, month: number): Observable<MonthData> {
    return this.getMonthDataFromServer(year, month).pipe(
      map(remoteData => this.transformRemoteDataToMonthData(remoteData, year, month)),
      catchError(error => {
        console.warn(`Error fetching data from server: ${error}. Falling back to mock data.`);
          return of({
            year: year,
            month: month,
            days: {}
          });
      })
    );
  }

  getMonthDataFromServer(year: number, month: number): Observable<TimeSheetRemoteData> {
    
    let request_Just = new GenericRequest<Dip_GG_Giustificativi_GetAll_InModel>(Dip_GG_Giustificativi_GetAll_InModel);
    request_Just.data.year = year;
    request_Just.data.month = month+1;

    let request_clock = new GenericRequest<Dip_GG_Timbratura_GetAll_InModel>(Dip_GG_Timbratura_GetAll_InModel);
    request_clock.data.year = year;
    request_clock.data.month = month+1;

    // 2. Define the Observables for the API calls (DO NOT subscribe yet)
    const justificationsObservable$ = this.dipGGGiustificativiService.GetAll(request_Just);
    const clockingsObservable$ = this.dipGGTimbraturaService.GetAll(request_clock);

    // 3. Use forkJoin to execute both Observables in parallel
    // It will emit an object with the results once BOTH calls complete
    return forkJoin({
      // Assign keys to easily access the results later
      justResult: justificationsObservable$,
      clockResult: clockingsObservable$
    }).pipe(
      // 4. Use the 'map' operator to transform the combined results
      map(results => {

        const giustificativiArray = results.justResult?.data?.dip_GG_Giustificativi || [];
        const timbratureArray = results.clockResult?.data?.dip_GG_Timbratura || [];

        // Crea l'oggetto finale TimeSheetRemoteData
        const remoteData: TimeSheetRemoteData = {
          dip_GG_Giustificativi: giustificativiArray,
          dip_GG_Timbratura: timbratureArray
        };

        console.log('Both calls finished. Combined data:', remoteData);
        return remoteData; // Return the structured data
      }),
      // 5. Optional: Add error handling for the forkJoin
      catchError(error => {
        console.error("Error fetching month data (one or both calls failed):", error);
        return throwError(() => new Error('Failed to load data for month ' + month + '/' + year));
      })
    );
    // The method now correctly returns an Observable<MonthData>
  }
    
  private transformRemoteDataToMonthData(remoteData: TimeSheetRemoteData, year: number, month: number): MonthData {
    const monthData: MonthData = {
      year: year,
      month: month,
      days: {}
    };

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

    // Unisci i dati per creare i record giornalieri
    const allDays = new Set<number>([
      ...Array.from(timbratureByDay.keys()),
      ...Array.from(giustificativiByDay.keys())
    ]);

    allDays.forEach(day => {
      monthData.days[day] = {
        date: new Date(year, month, day),
        //timestamps: [], // Campo che verrà rimosso
        //justifications: [], // Campo che verrà rimosso
        dip_GG_Timbratura: timbratureByDay.get(day) || [],
        dip_GG_Giustificativi: giustificativiByDay.get(day) || []
      };
    });

    return monthData;
  }

  private sortTimestampsByTime(timestamps: Dip_GG_TimbraturaModel[] | undefined): Dip_GG_TimbraturaModel[] {
    if (!timestamps) return [];

    return [...timestamps].sort((a, b) => this.timeToMinutes(a.timbratura) - this.timeToMinutes(b.timbratura));

  }
  
  private timeToMinutes(time: Date): number {
    if (!time) return 0;
    const hours = time.getHours();
    const minutes = time.getMinutes();
    return hours * 60 + minutes;
  }

}

