import { Component, OnInit } from '@angular/core';
import { SignalrService } from '../../../Utility/signalr.service';
import { environment } from '../../../../environments/environment';
import { Observable, of } from 'rxjs';

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
  weeks: Array<Array<{ day: number, records: TimeStamp[], isCurrentMonth: boolean }>>;
  monthNames = ['GENNAIO', 'FEBBRAIO', 'MARZO', 'APRILE', 'MAGGIO', 'GIUGNO',
    'LUGLIO', 'AGOSTO', 'SETTEMBRE', 'OTTOBRE', 'NOVEMBRE', 'DICEMBRE'];
  currentMonthDisplay: string;
  currentYear: number;
  currentMonthIndex: number;
  ///////////CALENDAR END

  constructor(private signalrService: SignalrService) {
    this.title = 'UserPage';
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

    // Determina il primo giorno del mese
    const firstDay = new Date(this.currentYear, this.currentMonthIndex, 1);
    let dayOfWeek = firstDay.getDay() || 7; // Converti 0 (domenica) a 7
    dayOfWeek = dayOfWeek - 1; // Converti a 0-6 con lunedì come 0

    // Determina l'ultimo giorno del mese
    const lastDay = new Date(this.currentYear, this.currentMonthIndex + 1, 0).getDate();

    // Inizia con i giorni del mese precedente se necessario
    const prevMonthLastDay = new Date(this.currentYear, this.currentMonthIndex, 0).getDate();

    // Costruisci le settimane
    let currentWeek = [];

    // Aggiungi giorni del mese precedente
    for (let i = 0; i < dayOfWeek; i++) {
      const day = prevMonthLastDay - dayOfWeek + i + 1;
      currentWeek.push({
        day: day,
        records: [],
        isCurrentMonth: false
      });
    }

    // Aggiungi giorni del mese corrente
    for (let i = 1; i <= lastDay; i++) {
      const dayData = this.currentMonth.days[i] || { date: new Date(this.currentYear, this.currentMonthIndex, i), timestamps: [] };

      currentWeek.push({
        day: i,
        records: dayData.timestamps,
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
          isCurrentMonth: false
        });
        nextMonthDay++; // Incrementa il giorno per ciascuna cella aggiunta
      }
      this.weeks.push(currentWeek);
    }
  }

  getMonthData(year: number, month: number): Observable<MonthData> {
    // Per test, generare dati fittizi che corrispondono all'esempio
    if (year === 2025 && month === 3) { // Aprile è 3 in JavaScript (0-based)
      return of(this.getMockApril2025Data());
    }

    // Fallback per altri mesi (dati vuoti)
    return of({
      year: year,
      month: month,
      days: {}
    });
  }

  private getMockApril2025Data(): MonthData {
    const aprilData: MonthData = {
      year: 2025,
      month: 3,
      days: {}
    };

    // Dati per i primi 15 giorni come mostrato nell'immagine
    const daysData: Array<{ day: number, timestamps: TimeStamp[] }> = [
      {
        day: 1, timestamps: [
          { type: 'E' as const, time: '09:02' },
          { type: 'U' as const, time: '13:03' },
          { type: 'E' as const, time: '13:59' },
          { type: 'U' as const, time: '18:00' }
        ]
      },
      {
        day: 2, timestamps: [
          { type: 'E' as const, time: '08:59' },
          { type: 'U' as const, time: '13:02' },
          { type: 'E' as const, time: '13:58' },
          { type: 'U' as const, time: '18:08' }
        ]
      },
      {
        day: 3, timestamps: [
          { type: 'E' as const, time: '08:49' },
          { type: 'U' as const, time: '13:05' },
          { type: 'E' as const, time: '13:51' },
          { type: 'U' as const, time: '18:00' }
        ]
      },
      {
        day: 4, timestamps: [
          { type: 'E' as const, time: '09:03' },
          { type: 'U' as const, time: '13:00' },
          { type: 'E' as const, time: '13:51' },
          { type: 'U' as const, time: '18:00' }
        ]
      },
      {
        day: 7, timestamps: [
          { type: 'E' as const, time: '08:50' },
          { type: 'U' as const, time: '13:03' },
          { type: 'E' as const, time: '13:46' },
          { type: 'U' as const, time: '18:05' }
        ]
      },
      {
        day: 8, timestamps: [
          { type: 'E' as const, time: '08:59' },
          { type: 'U' as const, time: '13:00' },
          { type: 'E' as const, time: '13:46' },
          { type: 'U' as const, time: '18:00' }
        ]
      },
      {
        day: 9, timestamps: [
          { type: 'E' as const, time: '08:59' },
          { type: 'U' as const, time: '13:01' },
          { type: 'E' as const, time: '13:44' },
          { type: 'U' as const, time: '18:00' }
        ]
      },
      {
        day: 10, timestamps: [
          { type: 'E' as const, time: '08:45' },
          { type: 'U' as const, time: '13:00' },
          { type: 'E' as const, time: '13:51' },
          { type: 'U' as const, time: '18:03' }
        ]
      },
      {
        day: 11, timestamps: [
          { type: 'E' as const, time: '08:56' },
          { type: 'U' as const, time: '12:00' },
          { type: 'E' as const, time: '13:46' },
          { type: 'U' as const, time: '18:00' }
        ]
      },
      {
        day: 14, timestamps: [
          { type: 'E' as const, time: '08:44' },
          { type: 'U' as const, time: '13:03' },
          { type: 'E' as const, time: '13:56' },
          { type: 'U' as const, time: '18:19' }
        ]
      },
      {
        day: 15, timestamps: [
          { type: 'E' as const, time: '08:58' },
          { type: 'U' as const, time: '13:02' },
          { type: 'E' as const, time: '13:59' }
        ]
      }
    ];

    // Inserisci i dati ordinati per orario
    daysData.forEach(dayData => {
      // Ordina le timbrature per orario
      const sortedTimestamps = this.sortTimestampsByTime(dayData.timestamps);

      aprilData.days[dayData.day] = {
        date: new Date(2025, 3, dayData.day),
        timestamps: sortedTimestamps
      };
    });

    return aprilData;
  }

  // Versione alternativa che utilizza il metodo precedente con entries/exits
  private getMockApril2025DataAlternative(): MonthData {
    const aprilData: MonthData = {
      year: 2025,
      month: 3,
      days: {}
    };

    // Aggiungiamo i dati per i primi 15 giorni come mostrato nell'immagine
    const days = [
      { day: 1, entries: ['09:02', '13:59'], exits: ['13:03', '18:00'] },
      { day: 2, entries: ['08:59', '13:58'], exits: ['13:02', '18:08'] },
      { day: 3, entries: ['08:49', '13:51'], exits: ['13:05', '18:00'] },
      { day: 4, entries: ['09:03', '13:51'], exits: ['13:00', '18:00'] },
      { day: 7, entries: ['08:50', '13:46'], exits: ['13:03', '18:05'] },
      { day: 8, entries: ['08:59', '13:46'], exits: ['13:00', '18:00'] },
      { day: 9, entries: ['08:59', '13:44'], exits: ['13:01', '18:00'] },
      { day: 10, entries: ['08:45', '13:51'], exits: ['13:00', '18:03'] },
      { day: 11, entries: ['08:56', '13:46'], exits: ['12:00', '18:00'] },
      { day: 14, entries: ['08:44', '13:56'], exits: ['13:03', '18:19'] },
      { day: 15, entries: ['08:58', '13:59'], exits: ['13:02', ''] }
    ];

    days.forEach(day => {
      const timestamps: TimeStamp[] = [];

      day.entries.forEach(time => {
        if (time) timestamps.push({ type: 'E', time });
      });

      day.exits.forEach(time => {
        if (time) timestamps.push({ type: 'U', time });
      });

      // Ordina le timbrature per orario
      const sortedTimestamps = this.sortTimestampsByTime(timestamps);

      aprilData.days[day.day] = {
        date: new Date(2025, 3, day.day),
        timestamps: sortedTimestamps
      };
    });

    return aprilData;
  }

  // Metodo per ordinare le timbrature esclusivamente per orario
  private sortTimestampsByTime(timestamps: TimeStamp[]): TimeStamp[] {
    return [...timestamps].sort((a, b) => {
      // Converti i tempi in minuti per un confronto numerico
      const aMinutes = this.timeToMinutes(a.time);
      const bMinutes = this.timeToMinutes(b.time);
      return aMinutes - bMinutes;
    });
  }

  // Utility per convertire orario HH:MM in minuti
  private timeToMinutes(time: string): number {
    if (!time) return 0;
    const [hours, minutes] = time.split(':').map(Number);
    return hours * 60 + minutes;
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

  // Questo metodo filtra per tipo ma mantiene l'ordine originale
  getTimestampsByType(records: TimeStamp[], type: 'E' | 'U'): TimeStamp[] {
    // Filtra per tipo ma non riordina, mantiene l'ordine originale basato sul tempo
    return records.filter(record => record.type === type);
  }

  // Se hai bisogno di avere tutti i record ordinati per orario
  getOrderedTimestamps(day: { records: TimeStamp[] }): TimeStamp[] {
    return this.sortTimestampsByTime(day.records);
  }
}
export interface TimeStamp {
  type: 'E' | 'U';  // Entrata o Uscita
  time: string;     // Orario in formato HH:MM
}

export interface DayRecord {
  date: Date;
  timestamps: TimeStamp[];
}

export interface MonthData {
  year: number;
  month: number;  // 0-11 (gennaio = 0)
  days: { [key: number]: DayRecord };  // Mappa giorno -> record
}
