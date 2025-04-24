import { Injectable } from '@angular/core';
import { catchError, Observable, of, throwError } from 'rxjs';
import { map } from 'rxjs/operators'; // Import map operator if you plan real sorting/processing
import { Dip_GG_TimbraturaInModel, Dip_GG_TimbraturaModel, TipoTimbratura } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { StatoRichiesta } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { Dip_GG_GiustificativiInModel, Dip_GG_GiustificativiModel } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';
import { DipGGGiustificativiService } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/dip-gg-giustificativi.service';
import { DipGGTimbraturaService } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/dip-gg-timbratura.service';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { forkJoin } from 'rxjs/internal/observable/forkJoin';

// --- INTERFACE DEFINITIONS (Moved Here) ---
export interface TimeStamp {
  type: 'E' | 'U';
  time: string;     // Orario in formato HH:MM
}

export interface Justification {
  code: string;      // e.g., 'FER', 'PER', 'MAL'
  description: string; // e.g., "Ferie", "Permesso 4h", "Malattia"
  isFullDay: boolean; // Indica se copre l'intera giornata
}

export interface DayRecord {
  date: Date;
  //timestamps: TimeStamp[];
  //justifications: Justification[];

  dip_GG_Timbratura: Dip_GG_TimbraturaModel[];
  dip_GG_Giustificativi: Dip_GG_GiustificativiModel[];
}


export interface TimeSheetRemoteData {
  dip_GG_Timbratura: Dip_GG_TimbraturaModel[];
  dip_GG_Giustificativi: Dip_GG_GiustificativiModel[];
}


export interface MonthData {
  year: number;
  month: number;  // 0-11 (gennaio = 0)
  days: { [key: number]: DayRecord };  // Mappa giorno -> record
}
// --- FINE INTERFACCE ---

@Injectable({
  providedIn: 'root'
})
export class MokeTimeSheetService {

  constructor(private dipGGGiustificativiService: DipGGGiustificativiService,
              private dipGGTimbraturaService: DipGGTimbraturaService) { }

  /**
   * Fetches calendar data for a specific month and year.
   * Currently returns mock data, but can be extended for API calls.
   * Ensures timestamps within each day record are sorted.
   * @param year The full year (e.g., 2024)
   * @param month The month index (0-11)
   * @returns Observable<MonthData>
   */
  //////getMonthData(year: number, month: number): Observable<MonthData> {
  //////  // --- SIMULAZIONE API CALL ---
  //////  // In a real scenario, you would use HttpClient here.
  //////  // For now, we use the mock data generator.

  //////  let dataObservable: Observable<MonthData>;


  //////  this.getMonthDataFromServer(year, month).subscribe(res => {
  //////    let data = this.transformRemoteDataToMonthData(res, year, month);
  //////    dataObservable = of(data );
  //////  });

  //////  //if (year === 2025 && month === 3) { // Aprile (0-based index)
  //////  //  console.log("CalendarDataService: Caricamento dati Mock Aprile 2025");
  //////  //  // Wrap mock data generation in 'of' to return an Observable
  //////  //  dataObservable = of(this.getMockApril2025Data());
  //////  //} else {
  //////  //  console.log(`CalendarDataService: Caricamento dati vuoti per ${year}-${month + 1}`);
  //////  //  // Return empty data structure as an Observable
  //////  //  dataObservable = of({
  //////  //    year: year,
  //////  //    month: month,
  //////  //    days: {}
  //////  //  });
  //////  //}

  //////  // Optional: Add processing/sorting if the source doesn't guarantee it
  //////  // Example: Ensure all day records have sorted timestamps
  //////  //return dataObservable.pipe(
  //////  //  map(monthData => {
  //////  //    // Ensure timestamps are sorted for every day in the returned data
  //////  //    //nvx
  //////  //    //Object.values(monthData.days).forEach(dayRecord => {
  //////  //    //  dayRecord.timestamps = this.sortTimestampsByTime(dayRecord.timestamps);
  //////  //    //});
  //////  //    Object.values(monthData.days).forEach(dayRecord  => {
  //////  //      dayRecord.dip_GG_Timbratura = this.sortTimestampsByTime(dayRecord.dip_GG_Timbratura);
  //////  //    });
  //////  //    return monthData;
  //////  //  })
  //////  //);

  //////  return dataObservable;

  //////  // --- FINE SIMULAZIONE ---
  //////}

  getMonthData(year: number, month: number): Observable<MonthData> {
    // Prova prima a ottenere i dati dal server
    return this.getMonthDataFromServer(year, month).pipe(
      // Trasforma i dati remoti in MonthData
      map(remoteData => this.transformRemoteDataToMonthData(remoteData, year, month)),
      // Gestisci eventuali errori dal server
      catchError(error => {
        console.warn(`Error fetching data from server: ${error}. Falling back to mock data.`);

        //// Fallback su dati mock in caso di errore
        //if (year === 2025 && month === 3) { // Aprile (0-based index)
        //  console.log("CalendarDataService: Caricamento dati Mock Aprile 2025");
        //  return of(this.getMockApril2025Data());
        //} else {
        //  console.log(`CalendarDataService: Caricamento dati vuoti per ${year}-${month + 1}`);
        //  // Return empty data structure as an Observable
          return of({
            year: year,
            month: month,
            days: {}
          });
        //}

      })
    );
  }

  getMonthDataFromServer(year: number, month: number): Observable<TimeSheetRemoteData> {
    
    let request_Just  = new GenericRequest<Dip_GG_GiustificativiInModel>(Dip_GG_GiustificativiInModel);
    let request_clock = new GenericRequest<Dip_GG_TimbraturaInModel>(Dip_GG_TimbraturaInModel);

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

  /**
 * Trasforma i dati remoti ricevuti dal server in un oggetto MonthData utilizzabile dal componente
 * @param remoteData Dati ricevuti dal server (timbrature e giustificativi)
 * @param year Anno di riferimento
 * @param month Mese di riferimento (0-11)
 * @returns MonthData strutturato per il calendario
 */
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


  /**
   * Generates mock data for April 2025.
   * (Private helper method)
   * @returns MonthData
   */
  //private getMockApril2025Data(): MonthData {
  //  const aprilData: MonthData = {
  //    year: 2025, month: 3, days: {
  //      // Note: Timestamps don't strictly NEED to be pre-sorted here
  //      // because the getMonthData method pipes through a sort.
  //      1: { date: new Date(2025, 3, 1), timestamps: [{ type: 'E', time: '09:02' }, { type: 'U', time: '13:03' }, { type: 'E', time: '13:59' }, { type: 'U', time: '18:00' }], justifications: [] },
  //      2: { date: new Date(2025, 3, 2), timestamps: [{ type: 'E', time: '08:59' }, { type: 'U', time: '13:02' }, { type: 'E', time: '13:58' }, { type: 'U', time: '18:08' }], justifications: [] },
  //      3: { date: new Date(2025, 3, 3), timestamps: [{ type: 'E', time: '08:49' }, { type: 'U', time: '13:05' }, { type: 'E', time: '13:51' }, { type: 'U', time: '18:00' }], justifications: [] },
  //      // ... (include other mock days as needed)
  //      16: { date: new Date(2025, 3, 16), timestamps: [], justifications: [{ code: 'MAL', description: 'Malattia', isFullDay: true }] },
  //      17: { date: new Date(2025, 3, 17), timestamps: [{ type: 'E', time: '09:00' }, { type: 'U', time: '13:00' }], justifications: [{ code: 'PER', description: 'Permesso Pom.', isFullDay: false }] },
  //      18: { date: new Date(2025, 3, 18), timestamps: [], justifications: [{ code: 'FER', description: 'Ferie', isFullDay: true }] },
  //      21: { date: new Date(2025, 3, 21), timestamps: [], justifications: [{ code: 'FST', description: 'Pasquetta', isFullDay: true }] },
  //      25: { date: new Date(2025, 3, 25), timestamps: [], justifications: [{ code: 'FST', description: 'Liberazione', isFullDay: true }] },
  //    }
  //  };
  //   Sorting is now handled reliably in getMonthData's pipe
  //   Object.values(aprilData.days).forEach(dayRec => {
  //     dayRec.timestamps = this.sortTimestampsByTime(dayRec.timestamps);
  //   });




  //  return aprilData;
  //}


  ////private getMockApril2025Data(): MonthData {
  ////  // Funzione helper per convertire il type del timestamp nel TimbraturaTipo corrispondente
  ////  const mapTipoTimbratura = (type: 'E' | 'U' | 'S' | 'A'): TipoTimbratura => {
  ////    switch (type) {
  ////      case 'E': return TipoTimbratura.Entrata;
  ////      case 'U': return TipoTimbratura.Uscita;;
  ////      case 'S': return TipoTimbratura.SenzaVerso;;
  ////      case 'A': return TipoTimbratura.Attivita;;
  ////      default: return TipoTimbratura.SenzaVerso;;
  ////    }
  ////  };

  ////  // Funzione helper per convertire una stringa oraria in un oggetto Date
  ////  const createTimeObject = (dateObj: Date, timeStr: string): Date => {
  ////    const [hours, minutes] = timeStr.split(':').map(Number);
  ////    const newDate = new Date(dateObj);
  ////    newDate.setHours(hours, minutes, 0, 0);
  ////    return newDate;
  ////  };

  ////  // Funzione helper per mappare timestamps in dip_GG_Timbratura
  ////  const mapToDipGGTimbratura = (date: Date, timestamps: TimeStamp[]): Dip_GG_TimbraturaModel[] => {
  ////    return timestamps.map(ts => {
  ////      const timeObject = createTimeObject(date, ts.time);
  ////      return {
  ////        id:0,
  ////        idDip_RapportoLavoro: 0, 
  ////        // Date e timbrature
  ////        timbratura: timeObject,
  ////        timbraturaOriginale: timeObject,
  ////        timbraturaArrotondata: timeObject, 
  ////        giornoCompetenza: timeObject, 
  ////        // Tipo di timbratura
  ////        timbraturaTipo: mapTipoTimbratura(ts.type),
  ////        richiestaStato: StatoRichiesta.Diretta,
  ////        idDip_Richiesta: 0
  ////      };
  ////    });
  ////  };

  ////  const aprilData: MonthData = {
  ////    year: 2025,
  ////    month: 3,
  ////    days: {
  ////      1: {
  ////        date: new Date(2025, 3, 1),
  ////        timestamps: [
  ////          { type: 'E', time: '09:02' },
  ////          { type: 'U', time: '13:03' },
  ////          { type: 'E', time: '13:59' },
  ////          { type: 'U', time: '18:00' }
  ////        ],
  ////        justifications: [],
  ////        dip_GG_Timbratura: mapToDipGGTimbratura(
  ////          new Date(2025, 3, 1),
  ////          [
  ////            { type: 'E', time: '09:02' },
  ////            { type: 'U', time: '13:03' },
  ////            { type: 'E', time: '13:59' },
  ////            { type: 'U', time: '18:00' }
  ////          ]
  ////        ),
  ////        dip_GG_Giustificativi:[]
  ////      },
  ////      2: {
  ////        date: new Date(2025, 3, 2),
  ////        timestamps: [
  ////          { type: 'E', time: '08:59' },
  ////          { type: 'U', time: '13:02' },
  ////          { type: 'E', time: '13:58' },
  ////          { type: 'U', time: '18:08' }
  ////        ],
  ////        justifications: [],
  ////        dip_GG_Timbratura: mapToDipGGTimbratura(
  ////          new Date(2025, 3, 2),
  ////          [
  ////            { type: 'E', time: '08:59' },
  ////            { type: 'U', time: '13:02' },
  ////            { type: 'E', time: '13:58' },
  ////            { type: 'U', time: '18:08' }
  ////          ]
  ////        ),
  ////        dip_GG_Giustificativi: []
  ////      },
  ////      3: {
  ////        date: new Date(2025, 3, 3),
  ////        timestamps: [
  ////          { type: 'E', time: '08:49' },
  ////          { type: 'U', time: '13:05' },
  ////          { type: 'E', time: '13:51' },
  ////          { type: 'U', time: '18:00' }
  ////        ],
  ////        justifications: [],
  ////        dip_GG_Timbratura: mapToDipGGTimbratura(
  ////          new Date(2025, 3, 3),
  ////          [
  ////            { type: 'E', time: '08:49' },
  ////            { type: 'U', time: '13:05' },
  ////            { type: 'E', time: '13:51' },
  ////            { type: 'U', time: '18:00' }
  ////          ]
  ////        ),
  ////        dip_GG_Giustificativi: []
  ////      },
  ////      16: {
  ////        date: new Date(2025, 3, 16),
  ////        timestamps: [],
  ////        justifications: [{ code: 'MAL', description: 'Malattia', isFullDay: true }],
  ////        dip_GG_Timbratura: [],
  ////        dip_GG_Giustificativi: [{ id: 1, idDip_RapportoLavoro: 0, data: new Date(2025, 1, 1), idJustificationType: 0, inputType: 0, hours: "", from: "", idPar_Giustificativi: 1, richiestaStato: 0, idDip_Richiesta: 0 }]
  ////      },
  ////      17: {
  ////        date: new Date(2025, 3, 17),
  ////        timestamps: [
  ////          { type: 'E', time: '09:00' },
  ////          { type: 'U', time: '13:00' }
  ////        ],
  ////        justifications: [{ code: 'PER', description: 'Permesso Pom.', isFullDay: false }],
  ////        dip_GG_Timbratura: mapToDipGGTimbratura(
  ////          new Date(2025, 3, 17),
  ////          [
  ////            { type: 'E', time: '09:00' },
  ////            { type: 'U', time: '13:00' }
  ////          ]
  ////        ),
  ////        dip_GG_Giustificativi: [{ id: 2, idDip_RapportoLavoro: 0, data: new Date(2025, 1, 1), idJustificationType: 0, inputType: 0, hours: "", from: "", idPar_Giustificativi: 1, richiestaStato: 0, idDip_Richiesta: 0 }]
  ////      },
  ////      18: {
  ////        date: new Date(2025, 3, 18),
  ////        timestamps: [],
  ////        justifications: [{ code: 'FER', description: 'Ferie', isFullDay: true }],
  ////        dip_GG_Timbratura: [],
  ////        dip_GG_Giustificativi: [{ id: 1, idDip_RapportoLavoro: 0, data: new Date(2025, 1, 1), idJustificationType: 0, inputType: 0, hours: "", from: "", idPar_Giustificativi: 1, richiestaStato: 0, idDip_Richiesta: 0 }]
  ////      },
  ////      21: {
  ////        date: new Date(2025, 3, 21),
  ////        timestamps: [],
  ////        justifications: [{ code: 'FST', description: 'Pasquetta', isFullDay: true }],
  ////        dip_GG_Timbratura: [],
  ////        dip_GG_Giustificativi: [{ id: 3, idDip_RapportoLavoro: 0, data: new Date(2025, 1, 1), idJustificationType: 0, inputType: 0, hours: "", from: "", idPar_Giustificativi: 1, richiestaStato: 0, idDip_Richiesta: 0 }]
  ////      },
  ////      25: {
  ////        date: new Date(2025, 3, 25),
  ////        timestamps: [],
  ////        justifications: [{ code: 'FST', description: 'Liberazione', isFullDay: true }],
  ////        dip_GG_Timbratura: [],
  ////        dip_GG_Giustificativi: [{ id: 2, idDip_RapportoLavoro: 0, data: new Date(2025, 1, 1), idJustificationType: 0, inputType: 0, hours: "", from: "", idPar_Giustificativi: 1, richiestaStato: 0, idDip_Richiesta: 0 }]
  ////      },
  ////    }
  ////  };

  ////  return aprilData;
  ////}

  /**
   * Sorts an array of TimeStamp objects by time.
   * (Private helper method)
   * @param timestamps Array of TimeStamp objects
   * @returns Sorted array of TimeStamp objects
   */
  private sortTimestampsByTime(timestamps: TimeStamp[] | undefined): TimeStamp[] {
    if (!timestamps) return [];
    // Create a copy before sorting to avoid modifying the original array if needed elsewhere
    return [...timestamps].sort((a, b) => this.timeToMinutes(a.time) - this.timeToMinutes(b.time));
  }
  //private sortTimestampsByTime(timestamps: Date[] | undefined): Date[] {
  //  if (!timestamps) return [];
  //  // Create a copy before sorting to avoid modifying the original array if needed elsewhere
  //  return [...timestamps].sort((a, b) => a.getUTCDate() - b.getUTCDate());
  //}

  /**
   * Converts a HH:MM time string to total minutes from midnight.
   * (Private helper method)
   * @param time Time string in HH:MM format
   * @returns Total minutes
   */
  private timeToMinutes(time: string): number {
    if (!time) return 0;
    const [hours, minutes] = time.split(':').map(Number);
    // Handle potential NaN issues if split or map fails
    return (isNaN(hours) ? 0 : hours) * 60 + (isNaN(minutes) ? 0 : minutes);
  }
}
// --- END OF FILE calendar-data.service.ts ---
