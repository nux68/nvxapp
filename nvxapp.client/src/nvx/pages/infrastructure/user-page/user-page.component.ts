import { Component, OnInit } from '@angular/core';
import { SignalrService } from '../../../Utility/signalr.service'; // Assicurati che il percorso sia corretto
import { environment } from '../../../../environments/environment'; // Assicurati che il percorso sia corretto
import { Observable, of } from 'rxjs';
import { MonthNavigatorService } from '../../../Utility/month-navigator.service';


// --- INTERFACCE (invariate) ---
export interface TimeStamp {
  type: 'E' | 'U';
  time: string;
}

export interface Justification {
  code: string;
  description: string;
  isFullDay: boolean;
}

export interface DayRecord {
  date: Date;
  timestamps: TimeStamp[];
  justifications: Justification[];
}

export interface MonthData {
  year: number;
  month: number;
  days: { [key: number]: DayRecord };
}
// --- FINE INTERFACCE ---


@Component({
  selector: 'app-user-page',
  templateUrl: './user-page.component.html',
  styleUrls: ['./user-page.component.scss'],
  standalone: false // Assumendo che non sia standalone, altrimenti importa il servizio nel component
})
export class UserPageComponent implements OnInit {
  public title!: string;

  ///////////CALENDAR
  currentMonth: MonthData;
  weekDays = ['lun', 'mar', 'mer', 'gio', 'ven', 'sab', 'dom'];
  weeks: Array<Array<{ day: number, records: TimeStamp[], justifications: Justification[], isCurrentMonth: boolean }>>;
  monthNames = ['GENNAIO', 'FEBBRAIO', 'MARZO', 'APRILE', 'MAGGIO', 'GIUGNO',
    'LUGLIO', 'AGOSTO', 'SETTEMBRE', 'OTTOBRE', 'NOVEMBRE', 'DICEMBRE'];
  currentMonthDisplay: string;
  // RIMOSSI: currentYear e currentMonthIndex - ora gestiti dal servizio
  ///////////CALENDAR END

  constructor(
    private signalrService: SignalrService,
    private monthNavigatorService: MonthNavigatorService // <-- INIETTA IL SERVIZIO
  ) {
    this.title = 'UserPage';
    // Inizializzazione delle proprietà
    this.weeks = [];
    this.currentMonth = { year: 0, month: 0, days: {} };
    this.currentMonthDisplay = '';
    // Non inizializzare più currentYear e currentMonthIndex qui
  }

  ionViewWillEnter() {
    if (environment.signalR.useSignalR) {
      this.signalrService.send("SendMessage", { 'text': "ciao" });
    }
  }

  ngOnInit() {
    // Il servizio si inizializza da solo con la data corrente.
    // Carica il mese iniziale basato sullo stato del servizio.
    this.loadMonth(); // Non servono più argomenti
  }

  // MODIFICATO: Non accetta più parametri, usa il servizio
  loadMonth() {
    // Ottieni anno e mese dal servizio
    const year = this.monthNavigatorService.currentYear;
    const month = this.monthNavigatorService.currentMonth;

    console.log(`Loading data for: ${year}-${month + 1}`); // Log per debug

    this.getMonthData(year, month).subscribe(monthData => {
      this.currentMonth = monthData;
      // Aggiorna la stringa del display usando i dati del servizio
      this.currentMonthDisplay = `${this.monthNames[month]} - ${year}`;
      // Costruisci il calendario DOPO aver caricato i dati
      this.buildCalendarWeeks();
    });
  }

  // MODIFICATO: Usa il servizio per ottenere anno e mese correnti
  buildCalendarWeeks() {
    this.weeks = [];
    const year = this.monthNavigatorService.currentYear;
    const month = this.monthNavigatorService.currentMonth;

    const firstDay = new Date(year, month, 1);
    let dayOfWeek = firstDay.getDay() || 7;
    dayOfWeek = dayOfWeek - 1;

    const lastDay = new Date(year, month + 1, 0).getDate();
    const prevMonthLastDay = new Date(year, month, 0).getDate();

    let currentWeek: Array<{ day: number, records: TimeStamp[], justifications: Justification[], isCurrentMonth: boolean }> = [];

    // Aggiungi giorni del mese precedente
    for (let i = 0; i < dayOfWeek; i++) {
      const day = prevMonthLastDay - dayOfWeek + i + 1;
      currentWeek.push({
        day: day,
        records: [],
        justifications: [],
        isCurrentMonth: false
      });
    }

    // Aggiungi giorni del mese corrente
    for (let i = 1; i <= lastDay; i++) {
      // Assicura fallback corretto anche se il giorno non esiste in currentMonth.days
      const dayData = this.currentMonth?.days?.[i] || { date: new Date(year, month, i), timestamps: [], justifications: [] };

      currentWeek.push({
        day: i,
        records: dayData.timestamps || [],
        justifications: dayData.justifications || [],
        isCurrentMonth: true
      });

      if (currentWeek.length === 7) {
        this.weeks.push(currentWeek);
        currentWeek = [];
      }
    }

    // Completa l'ultima settimana
    if (currentWeek.length > 0) {
      let nextMonthDay = 1;
      while (currentWeek.length < 7) {
        currentWeek.push({
          day: nextMonthDay,
          records: [],
          justifications: [],
          isCurrentMonth: false
        });
        nextMonthDay++;
      }
      this.weeks.push(currentWeek);
    }
  }

  // getMonthData rimane invariato nel suo funzionamento interno (prende anno e mese)
  // Ma viene chiamato da loadMonth() con i valori presi dal servizio
  getMonthData(year: number, month: number): Observable<MonthData> {
    // Mock data (come prima)
    if (year === 2025 && month === 3) {
      console.log("Caricamento dati Mock Aprile 2025");
      return of(this.getMockApril2025Data());
    }
    console.log(`Caricamento dati vuoti per ${year}-${month + 1}`);
    return of({
      year: year,
      month: month,
      days: {}
    });
  }

  // getMockApril2025Data rimane invariato
  private getMockApril2025Data(): MonthData {
    const aprilData: MonthData = {
      year: 2025,
      month: 3, // April
      days: {}
    };
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
      aprilData.days[dayData.day] = {
        date: new Date(2025, 3, dayData.day),
        timestamps: sortedTimestamps,
        justifications: dayData.justifications || []
      };
    });
    return aprilData;
  }

  // sortTimestampsByTime e timeToMinutes rimangono invariati
  private sortTimestampsByTime(timestamps: TimeStamp[] | undefined): TimeStamp[] {
    if (!timestamps) return [];
    return [...timestamps].sort((a, b) => {
      const aMinutes = this.timeToMinutes(a.time);
      const bMinutes = this.timeToMinutes(b.time);
      return aMinutes - bMinutes;
    });
  }

  private timeToMinutes(time: string): number {
    if (!time) return 0;
    const [hours, minutes] = time.split(':').map(Number);
    return (hours || 0) * 60 + (minutes || 0);
  }

  // MODIFICATO: Usa il servizio per navigare e poi ricarica
  previousMonth() {
    this.monthNavigatorService.previousMonth(); // Delega la logica al servizio
    this.loadMonth(); // Ricarica i dati per il nuovo mese/anno
  }

  // MODIFICATO: Usa il servizio per navigare e poi ricarica
  nextMonth() {
    this.monthNavigatorService.nextMonth(); // Delega la logica al servizio
    this.loadMonth(); // Ricarica i dati per il nuovo mese/anno
  }

  // Metodi helper getTimestampsByType, getOrderedTimestamps, hasFullDayJustification, getJustificationClass rimangono invariati

  getTimestampsByType(records: TimeStamp[] | undefined, type: 'E' | 'U'): TimeStamp[] {
    if (!records) return [];
    return records.filter(record => record.type === type);
  }

  getOrderedTimestamps(day: { records: TimeStamp[] } | undefined): TimeStamp[] {
    if (!day || !day.records) return [];
    return this.sortTimestampsByTime(day.records);
  }

  hasFullDayJustification(justifications: Justification[] | undefined): boolean {
    if (!justifications) return false;
    return justifications.some(j => j.isFullDay);
  }

  getJustificationClass(justification: Justification): string {
    switch (justification.code.toUpperCase()) {
      case 'FER':
      case 'FST':
        return 'justification-vacation';
      case 'MAL':
        return 'justification-sick';
      case 'PER':
        return 'justification-leave';
      default:
        return 'justification-other';
    }
  }
}
