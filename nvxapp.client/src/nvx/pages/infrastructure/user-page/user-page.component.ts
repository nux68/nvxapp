// --- START OF FILE user-page.component.ts ---

import { Component, OnInit } from '@angular/core';
import { SignalrService } from '../../../Utility/signalr.service';
import { environment } from '../../../../environments/environment';
import { Observable, of } from 'rxjs';
import { MonthNavigatorService } from '../../../Utility/month-navigator.service';


// Interfacce (invariate)
export interface TimeStamp { type: 'E' | 'U'; time: string; }
export interface Justification { code: string; description: string; isFullDay: boolean; }
export interface DayRecord { date: Date; timestamps: TimeStamp[]; justifications: Justification[]; }
export interface MonthData { year: number; month: number; days: { [key: number]: DayRecord }; }

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
  // RIMOSSI: weekDays e monthNames - ora nel servizio
  weeks: Array<Array<{ day: number, records: TimeStamp[], justifications: Justification[], isCurrentMonth: boolean }>>;
  currentMonthDisplay: string;
  ///////////CALENDAR END

  constructor(
    private signalrService: SignalrService,
    // MODIFICATO: Reso pubblico per accesso dal template a weekDays
    public monthNavigatorService: MonthNavigatorService
  ) {
    this.title = 'UserPage';
    this.weeks = [];
    this.currentMonth = { year: 0, month: 0, days: {} };
    this.currentMonthDisplay = '';
  }

  ionViewWillEnter() {
    if (environment.signalR.useSignalR) {
      this.signalrService.send("SendMessage", { 'text': "ciao" });
    }
  }

  ngOnInit() {
    this.loadMonth(); 
  }

  loadMonth() {
    const year = this.monthNavigatorService.currentYear;
    const month = this.monthNavigatorService.currentMonth;

    console.log(`Loading data for: ${year}-${month + 1}`);

    this.getMonthData(year, month).subscribe(monthData => {
      this.currentMonth = monthData;
      // MODIFICATO: Usa monthNames dal servizio
      this.currentMonthDisplay = `${this.monthNavigatorService.monthNames[month]} - ${year}`;
      this.buildCalendarWeeks();
    });
  }

  // buildCalendarWeeks usa solo monthNavigatorService.currentYear/Month, non richiede modifiche qui
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

    for (let i = 0; i < dayOfWeek; i++) {
      const day = prevMonthLastDay - dayOfWeek + i + 1;
      currentWeek.push({ day: day, records: [], justifications: [], isCurrentMonth: false });
    }

    for (let i = 1; i <= lastDay; i++) {
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

    if (currentWeek.length > 0) {
      let nextMonthDay = 1;
      while (currentWeek.length < 7) {
        currentWeek.push({ day: nextMonthDay, records: [], justifications: [], isCurrentMonth: false });
        nextMonthDay++;
      }
      this.weeks.push(currentWeek);
    }
  }

  getMonthData(year: number, month: number): Observable<MonthData> {
    if (year === 2025 && month === 3) {
      console.log("Caricamento dati Mock Aprile 2025");
      return of(this.getMockApril2025Data());
    }
    console.log(`Caricamento dati vuoti per ${year}-${month + 1}`);
    return of({ year: year, month: month, days: {} });
  }

  private getMockApril2025Data(): MonthData {
    // (Contenuto del metodo invariato... omettiamo per brevità)
    const aprilData: MonthData = {
      year: 2025, month: 3, days: {
        1: { date: new Date(2025, 3, 1), timestamps: [{ type: 'E', time: '09:02' }, { type: 'U', time: '13:03' }, { type: 'E', time: '13:59' }, { type: 'U', time: '18:00' }], justifications: [] },
        2: { date: new Date(2025, 3, 1), timestamps: [{ type: 'E', time: '09:02' }, { type: 'U', time: '13:03' }, { type: 'E', time: '13:59' }, { type: 'U', time: '18:00' }], justifications: [] },
        3: { date: new Date(2025, 3, 1), timestamps: [{ type: 'E', time: '09:02' }, { type: 'U', time: '13:03' }, { type: 'E', time: '13:59' }, { type: 'U', time: '18:00' }], justifications: [] },
        // ... altri giorni mock ...
        16: { date: new Date(2025, 3, 16), timestamps: [], justifications: [{ code: 'MAL', description: 'Malattia', isFullDay: true }] },
        17: { date: new Date(2025, 3, 17), timestamps: [{ type: 'E', time: '09:00' }, { type: 'U', time: '13:00' }], justifications: [{ code: 'PER', description: 'Permesso Pom.', isFullDay: false }] },
        18: { date: new Date(2025, 3, 18), timestamps: [], justifications: [{ code: 'FER', description: 'Ferie', isFullDay: true }] },
        21: { date: new Date(2025, 3, 21), timestamps: [], justifications: [{ code: 'FST', description: 'Pasquetta', isFullDay: true }] },
        25: { date: new Date(2025, 3, 25), timestamps: [], justifications: [{ code: 'FST', description: 'Liberazione', isFullDay: true }] },
      }
    };
    // Aggiungere qui la logica per popolare aprilData.days con i dati mock completi se necessario
    // Ad esempio, usando un loop o definendoli direttamente come sopra.
    // Assicurarsi che la struttura DayRecord sia rispettata.
    Object.values(aprilData.days).forEach(dayRec => {
      dayRec.timestamps = this.sortTimestampsByTime(dayRec.timestamps);
    });
    return aprilData;
  }


  private sortTimestampsByTime(timestamps: TimeStamp[] | undefined): TimeStamp[] {
    if (!timestamps) return [];
    return [...timestamps].sort((a, b) => this.timeToMinutes(a.time) - this.timeToMinutes(b.time));
  }

  private timeToMinutes(time: string): number {
    if (!time) return 0;
    const [hours, minutes] = time.split(':').map(Number);
    return (hours || 0) * 60 + (minutes || 0);
  }

  previousMonth() {
    this.monthNavigatorService.previousMonth();
    this.loadMonth();
  }

  nextMonth() {
    this.monthNavigatorService.nextMonth();
    this.loadMonth();
  }

  // Helper (invariati)
  getTimestampsByType(records: TimeStamp[] | undefined, type: 'E' | 'U'): TimeStamp[] { /* ... */ return records?.filter(r => r.type === type) || []; }
  getOrderedTimestamps(day: { records: TimeStamp[] } | undefined): TimeStamp[] { /* ... */ return this.sortTimestampsByTime(day?.records); }
  hasFullDayJustification(justifications: Justification[] | undefined): boolean { /* ... */ return justifications?.some(j => j.isFullDay) || false; }
  getJustificationClass(justification: Justification): string { /* ... */
    switch (justification.code.toUpperCase()) {
      case 'FER': case 'FST': return 'justification-vacation';
      case 'MAL': return 'justification-sick';
      case 'PER': return 'justification-leave';
      default: return 'justification-other';
    }
  }
}
// --- END OF FILE user-page.component.ts ---
