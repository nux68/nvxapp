import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators'; // Import map operator if you plan real sorting/processing

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
  timestamps: TimeStamp[];
  justifications: Justification[];
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

  constructor() { }

  /**
   * Fetches calendar data for a specific month and year.
   * Currently returns mock data, but can be extended for API calls.
   * Ensures timestamps within each day record are sorted.
   * @param year The full year (e.g., 2024)
   * @param month The month index (0-11)
   * @returns Observable<MonthData>
   */
  getMonthData(year: number, month: number): Observable<MonthData> {
    // --- SIMULAZIONE API CALL ---
    // In a real scenario, you would use HttpClient here.
    // For now, we use the mock data generator.

    let dataObservable: Observable<MonthData>;

    if (year === 2025 && month === 3) { // Aprile (0-based index)
      console.log("CalendarDataService: Caricamento dati Mock Aprile 2025");
      // Wrap mock data generation in 'of' to return an Observable
      dataObservable = of(this.getMockApril2025Data());
    } else {
      console.log(`CalendarDataService: Caricamento dati vuoti per ${year}-${month + 1}`);
      // Return empty data structure as an Observable
      dataObservable = of({
        year: year,
        month: month,
        days: {}
      });
    }

    // Optional: Add processing/sorting if the source doesn't guarantee it
    // Example: Ensure all day records have sorted timestamps
    return dataObservable.pipe(
      map(monthData => {
        // Ensure timestamps are sorted for every day in the returned data
        Object.values(monthData.days).forEach(dayRecord => {
          dayRecord.timestamps = this.sortTimestampsByTime(dayRecord.timestamps);
        });
        return monthData;
      })
    );
    // --- FINE SIMULAZIONE ---
  }

  /**
   * Generates mock data for April 2025.
   * (Private helper method)
   * @returns MonthData
   */
  private getMockApril2025Data(): MonthData {
    const aprilData: MonthData = {
      year: 2025, month: 3, days: {
        // Note: Timestamps don't strictly NEED to be pre-sorted here
        // because the getMonthData method pipes through a sort.
        1: { date: new Date(2025, 3, 1), timestamps: [{ type: 'E', time: '09:02' }, { type: 'U', time: '13:03' }, { type: 'E', time: '13:59' }, { type: 'U', time: '18:00' }], justifications: [] },
        2: { date: new Date(2025, 3, 2), timestamps: [{ type: 'E', time: '08:59' }, { type: 'U', time: '13:02' }, { type: 'E', time: '13:58' }, { type: 'U', time: '18:08' }], justifications: [] },
        3: { date: new Date(2025, 3, 3), timestamps: [{ type: 'E', time: '08:49' }, { type: 'U', time: '13:05' }, { type: 'E', time: '13:51' }, { type: 'U', time: '18:00' }], justifications: [] },
        // ... (include other mock days as needed)
        16: { date: new Date(2025, 3, 16), timestamps: [], justifications: [{ code: 'MAL', description: 'Malattia', isFullDay: true }] },
        17: { date: new Date(2025, 3, 17), timestamps: [{ type: 'E', time: '09:00' }, { type: 'U', time: '13:00' }], justifications: [{ code: 'PER', description: 'Permesso Pom.', isFullDay: false }] },
        18: { date: new Date(2025, 3, 18), timestamps: [], justifications: [{ code: 'FER', description: 'Ferie', isFullDay: true }] },
        21: { date: new Date(2025, 3, 21), timestamps: [], justifications: [{ code: 'FST', description: 'Pasquetta', isFullDay: true }] },
        25: { date: new Date(2025, 3, 25), timestamps: [], justifications: [{ code: 'FST', description: 'Liberazione', isFullDay: true }] },
      }
    };
    // Sorting is now handled reliably in getMonthData's pipe
    // Object.values(aprilData.days).forEach(dayRec => {
    //   dayRec.timestamps = this.sortTimestampsByTime(dayRec.timestamps);
    // });
    return aprilData;
  }

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
