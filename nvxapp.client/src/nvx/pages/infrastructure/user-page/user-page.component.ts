import { Component, OnInit } from '@angular/core';
import { SignalrService } from '../../../Utility/signalr.service'; // Assicurati che il percorso sia corretto
import { environment } from '../../../../environments/environment'; // Assicurati che il percorso sia corretto
import { Observable, of } from 'rxjs';

// --- NUOVE INTERFACCE ---
export interface TimeStamp {
  type: 'E' | 'U';  // Entrata o Uscita
  time: string;     // Orario in formato HH:MM
}

// NUOVA: Definizione Giustificativo
export interface Justification {
  code: string;      // e.g., 'FER', 'PER', 'MAL'
  description: string; // e.g., "Ferie", "Permesso 4h", "Malattia"
  isFullDay: boolean; // Indica se copre l'intera giornata
}

// MODIFICATA: Aggiunto justifications a DayRecord
export interface DayRecord {
  date: Date;
  timestamps: TimeStamp[];
  justifications: Justification[]; // <-- AGGIUNTO
}

export interface MonthData {
  year: number;
  month: number;  // 0-11 (gennaio = 0)
  days: { [key: number]: DayRecord };  // Mappa giorno -> record
}
// --- FINE INTERFACCE ---


@Component({
  selector: 'app-user-page',
  templateUrl: './user-page.component.html',
  styleUrls: ['./user-page.component.scss'],
  standalone: false
})
export class UserPageComponent implements OnInit {
  public title!: string;

  ///////////CALENDAR
  currentMonth: MonthData;
  weekDays = ['lun', 'mar', 'mer', 'gio', 'ven', 'sab', 'dom'];
  // MODIFICATO: Aggiunto justifications alla struttura weeks
  weeks: Array<Array<{ day: number, records: TimeStamp[], justifications: Justification[], isCurrentMonth: boolean }>>;
  monthNames = ['GENNAIO', 'FEBBRAIO', 'MARZO', 'APRILE', 'MAGGIO', 'GIUGNO',
    'LUGLIO', 'AGOSTO', 'SETTEMBRE', 'OTTOBRE', 'NOVEMBRE', 'DICEMBRE'];
  currentMonthDisplay: string;
  currentYear: number;
  currentMonthIndex: number;
  ///////////CALENDAR END

  constructor(private signalrService: SignalrService) {
    this.title = 'UserPage';
    // Inizializzazione delle proprietà per evitare errori undefined
    this.weeks = [];
    this.currentMonth = { year: 0, month: 0, days: {} };
    this.currentMonthDisplay = '';
    this.currentYear = 0;
    this.currentMonthIndex = 0;
  }

  ionViewWillEnter() {
    if (environment.signalR.useSignalR) {
      this.signalrService.send("SendMessage", { 'text': "ciao" });
    }
  }

  ngOnInit() {
    const today = new Date();
    this.currentMonthIndex = today.getMonth();
    this.currentYear = today.getFullYear();
    this.loadMonth(this.currentYear, this.currentMonthIndex);
  }

  loadMonth(year: number, month: number) {
    this.getMonthData(year, month).subscribe(monthData => {
      this.currentMonth = monthData;
      this.currentMonthDisplay = `${this.monthNames[month]} - ${year}`;
      this.buildCalendarWeeks();
    });
  }

  buildCalendarWeeks() {
    this.weeks = [];

    const firstDay = new Date(this.currentYear, this.currentMonthIndex, 1);
    let dayOfWeek = firstDay.getDay() || 7; // Converti 0 (domenica) a 7
    dayOfWeek = dayOfWeek - 1; // Converti a 0-6 con lunedì come 0

    const lastDay = new Date(this.currentYear, this.currentMonthIndex + 1, 0).getDate();
    const prevMonthLastDay = new Date(this.currentYear, this.currentMonthIndex, 0).getDate();

    // Tipo esplicito per currentWeek
    let currentWeek: Array<{ day: number, records: TimeStamp[], justifications: Justification[], isCurrentMonth: boolean }> = [];

    // Aggiungi giorni del mese precedente
    for (let i = 0; i < dayOfWeek; i++) {
      const day = prevMonthLastDay - dayOfWeek + i + 1;
      currentWeek.push({
        day: day,
        records: [],
        justifications: [], // <-- AGGIUNTO: Giustificativi vuoti
        isCurrentMonth: false
      });
    }

    // Aggiungi giorni del mese corrente
    for (let i = 1; i <= lastDay; i++) {
      // MODIFICATO: Fallback per dayData include justifications vuoti
      const dayData = this.currentMonth.days[i] || { date: new Date(this.currentYear, this.currentMonthIndex, i), timestamps: [], justifications: [] };

      currentWeek.push({
        day: i,
        records: dayData.timestamps || [], // Assicura che sia sempre un array
        justifications: dayData.justifications || [], // <-- AGGIUNTO: Includi giustificativi
        isCurrentMonth: true
      });

      if (currentWeek.length === 7) {
        this.weeks.push(currentWeek);
        currentWeek = [];
      }
    }

    // Completa l'ultima settimana con giorni del mese successivo se necessario
    if (currentWeek.length > 0) {
      let nextMonthDay = 1;
      while (currentWeek.length < 7) {
        currentWeek.push({
          day: nextMonthDay,
          records: [],
          justifications: [], // <-- AGGIUNTO: Giustificativi vuoti
          isCurrentMonth: false
        });
        nextMonthDay++;
      }
      this.weeks.push(currentWeek);
    }
  }

  getMonthData(year: number, month: number): Observable<MonthData> {
    // Per test, generare dati fittizi che corrispondono all'esempio
    if (year === 2025 && month === 3) { // Aprile è 3 in JavaScript (0-based)
      console.log("Caricamento dati Mock Aprile 2025");
      return of(this.getMockApril2025Data()); // Usa la versione aggiornata
    }

    // Fallback per altri mesi (dati vuoti)
    console.log(`Caricamento dati vuoti per ${year}-${month + 1}`);
    return of({
      year: year,
      month: month,
      days: {} // Assicura che days sia inizializzato
    });
  }

  // MODIFICATO: Aggiunti giustificativi ai dati mock
  private getMockApril2025Data(): MonthData {
    const aprilData: MonthData = {
      year: 2025,
      month: 3, // April
      days: {}
    };

    // MODIFICATO: Tipo dell'array include justifications opzionale
    const daysData: Array<{ day: number, timestamps: TimeStamp[], justifications?: Justification[] }> = [
      { day: 1, timestamps: [{ type: 'E', time: '09:02' }, { type: 'U', time: '13:03' }, { type: 'E', time: '13:59' }, { type: 'U', time: '18:00' }] },
      { day: 2, timestamps: [{ type: 'E', time: '08:59' }, { type: 'U', time: '13:02' }, { type: 'E', time: '13:58' }, { type: 'U', time: '18:08' }] },
      { day: 3, timestamps: [{ type: 'E', time: '08:49' }, { type: 'U', time: '13:05' }, { type: 'E', time: '13:51' }, { type: 'U', time: '18:00' }] },
      { day: 4, timestamps: [{ type: 'E', time: '09:03' }, { type: 'U', time: '13:00' }, { type: 'E', time: '13:51' }, { type: 'U', time: '18:00' }] },
      // Weekend 5, 6 - No data
      { day: 7, timestamps: [{ type: 'E', time: '08:50' }, { type: 'U', time: '13:03' }, { type: 'E', time: '13:46' }, { type: 'U', time: '18:05' }] },
      { day: 8, timestamps: [{ type: 'E', time: '08:59' }, { type: 'U', time: '13:00' }, { type: 'E', time: '13:46' }, { type: 'U', time: '18:00' }] },
      { day: 9, timestamps: [{ type: 'E', time: '08:59' }, { type: 'U', time: '13:01' }, { type: 'E', time: '13:44' }, { type: 'U', time: '18:00' }] },
      { day: 10, timestamps: [{ type: 'E', time: '08:45' }, { type: 'U', time: '13:00' }, { type: 'E', time: '13:51' }, { type: 'U', time: '18:03' }] },
      { day: 11, timestamps: [{ type: 'E', time: '08:56' }, { type: 'U', time: '12:00' }, { type: 'E', time: '13:46' }, { type: 'U', time: '18:00' }] },
      // Weekend 12, 13 - No data
      { day: 14, timestamps: [{ type: 'E', time: '08:44' }, { type: 'U', time: '13:03' }, { type: 'E', time: '13:56' }, { type: 'U', time: '18:19' }] },
      { day: 15, timestamps: [{ type: 'E', time: '08:58' }, { type: 'U', time: '13:02' }, { type: 'E', time: '13:59' }] }, // Uscita mancante o parziale
      // --- NUOVI: Esempi con Giustificativi ---
      { day: 16, timestamps: [], justifications: [{ code: 'MAL', description: 'Malattia', isFullDay: true }] },
      { day: 17, timestamps: [{ type: 'E', time: '09:00' }, { type: 'U', time: '13:00' }], justifications: [{ code: 'PER', description: 'Permesso Pom.', isFullDay: false }] },
      { day: 18, timestamps: [], justifications: [{ code: 'FER', description: 'Ferie', isFullDay: true }] },
      // Weekend 19, 20
      { day: 21, timestamps: [], justifications: [{ code: 'FST', description: 'Pasquetta', isFullDay: true }] }, // Assumendo Pasquetta
      // ... altri giorni ...
      { day: 25, timestamps: [], justifications: [{ code: 'FST', description: 'Liberazione', isFullDay: true }] },
    ];

    daysData.forEach(dayData => {
      const sortedTimestamps = this.sortTimestampsByTime(dayData.timestamps);
      // MODIFICATO: Aggiunta justifications al DayRecord
      aprilData.days[dayData.day] = {
        date: new Date(2025, 3, dayData.day),
        timestamps: sortedTimestamps,
        justifications: dayData.justifications || [] // Assicura che l'array esista
      };
    });

    return aprilData;
  }

  // Rimosso getMockApril2025DataAlternative per semplicità

  private sortTimestampsByTime(timestamps: TimeStamp[] | undefined): TimeStamp[] {
    if (!timestamps) return []; // Guardia per undefined/null
    return [...timestamps].sort((a, b) => {
      const aMinutes = this.timeToMinutes(a.time);
      const bMinutes = this.timeToMinutes(b.time);
      return aMinutes - bMinutes;
    });
  }

  private timeToMinutes(time: string): number {
    if (!time) return 0; // Guardia per stringa vuota/null
    const [hours, minutes] = time.split(':').map(Number);
    return (hours || 0) * 60 + (minutes || 0); // Gestisce NaN se map fallisce
  }

  previousMonth() {
    this.currentMonthIndex--;
    if (this.currentMonthIndex < 0) {
      this.currentMonthIndex = 11;
      this.currentYear--;
    }
    this.loadMonth(this.currentYear, this.currentMonthIndex);
  }

  nextMonth() {
    this.currentMonthIndex++;
    if (this.currentMonthIndex > 11) {
      this.currentMonthIndex = 0;
      this.currentYear++;
    }
    this.loadMonth(this.currentYear, this.currentMonthIndex);
  }

  // Questo metodo filtra per tipo ma mantiene l'ordine originale (meno utile ora)
  getTimestampsByType(records: TimeStamp[] | undefined, type: 'E' | 'U'): TimeStamp[] {
    if (!records) return []; // Guardia
    return records.filter(record => record.type === type);
  }

  // Restituisce tutti i record ordinati per orario
  getOrderedTimestamps(day: { records: TimeStamp[] } | undefined): TimeStamp[] {
    if (!day || !day.records) return []; // Guardia
    return this.sortTimestampsByTime(day.records);
  }

  // --- NUOVI METODI HELPER per Giustificativi ---

  // Verifica se c'è almeno un giustificativo per l'intera giornata
  hasFullDayJustification(justifications: Justification[] | undefined): boolean {
    if (!justifications) return false;
    return justifications.some(j => j.isFullDay);
  }

  // Restituisce classi CSS specifiche per tipo di giustificativo
  getJustificationClass(justification: Justification): string {
    switch (justification.code.toUpperCase()) {
      case 'FER': // Ferie
      case 'FST': // Festivo
        return 'justification-vacation';
      case 'MAL': // Malattia
        return 'justification-sick';
      case 'PER': // Permesso
        return 'justification-leave';
      default:
        return 'justification-other'; // Altri tipi non specificati
    }
  }
  // --- FINE METODI HELPER ---
}

// Le interfacce sono definite all'inizio del file ora
