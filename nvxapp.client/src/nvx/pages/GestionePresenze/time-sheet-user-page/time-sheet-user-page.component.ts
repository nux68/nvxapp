import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/user-navigation.service';
import { environment } from '../../../../environments/environment';
import { MonthNavigatorService } from '../../../Utility/month-navigator.service';
import { SignalrService } from '../../../Utility/signalr.service';
import { MonthData, TimeStamp, Justification } from '../../infrastructure/user-page/user-page.component';
import { MokeTimeSheetService } from '../../../Utility/GestionePresenze/moke-time-sheet.service';

@Component({
  selector: 'app-time-sheet-user-page',
  templateUrl: './time-sheet-user-page.component.html',
  styleUrls: ['./time-sheet-user-page.component.scss'],
  standalone:false
}) 

export class TimeSheetUserPageComponent implements OnInit {
  public title!: string;

  currentMonth: MonthData; // Usa l'interfaccia importata
  // Usa le interfacce importate nella definizione di 'weeks'
  weeks: Array<Array<{ day: number, records: TimeStamp[], justifications: Justification[], isCurrentMonth: boolean }>>;
  currentMonthDisplay: string;

  constructor(
    private signalrService: SignalrService,
    public monthNavigatorService: MonthNavigatorService,
    private calendarDataService: MokeTimeSheetService // <-- INIETTA IL NUOVO SERVIZIO
  ) {
    this.title = 'TimeSheetUser';
    this.weeks = [];
    // Inizializza con una struttura valida ma vuota
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

    console.log(`UserPageComponent: Loading data for: ${year}-${month + 1} via CalendarDataService`);

    // MODIFICATO: Chiama il metodo del servizio dati
    this.calendarDataService.getMonthData(year, month).subscribe(monthData => {
      this.currentMonth = monthData; // monthData è già del tipo corretto MonthData
      // Usa monthNames dal servizio di navigazione come prima
      this.currentMonthDisplay = `${this.monthNavigatorService.monthNames[month]} - ${year}`;
      this.buildCalendarWeeks(); // Costruisce la UI dopo aver ricevuto i dati
    });
  }

  // buildCalendarWeeks rimane quasi invariato, usa this.currentMonth popolato dal servizio
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

    // Giorni mese precedente
    for (let i = 0; i < dayOfWeek; i++) {
      const day = prevMonthLastDay - dayOfWeek + i + 1;
      currentWeek.push({ day: day, records: [], justifications: [], isCurrentMonth: false });
    }

    // Giorni mese corrente
    for (let i = 1; i <= lastDay; i++) {
      // Usa i dati da this.currentMonth popolato dal servizio
      const dayData = this.currentMonth?.days?.[i]; // Può essere undefined se non ci sono dati per quel giorno

      currentWeek.push({
        day: i,
        // Usa fallback se dayData non esiste o se le proprietà sono vuote
        // Il servizio dati dovrebbe già fornire array vuoti dove appropriato
        records: dayData?.timestamps || [],
        justifications: dayData?.justifications || [],
        isCurrentMonth: true
      });

      if (currentWeek.length === 7) {
        this.weeks.push(currentWeek);
        currentWeek = [];
      }
    }

    // Giorni mese successivo
    if (currentWeek.length > 0) {
      let nextMonthDay = 1;
      while (currentWeek.length < 7) {
        currentWeek.push({ day: nextMonthDay, records: [], justifications: [], isCurrentMonth: false });
        nextMonthDay++;
      }
      this.weeks.push(currentWeek);
    }
  }

  // RIMOSSI: getMonthData, getMockApril2025Data, sortTimestampsByTime, timeToMinutes
  // Questi metodi sono ora privati dentro CalendarDataService

  // --- Metodi Helper per la VISTA (rimangono nel componente) ---

  previousMonth() {
    this.monthNavigatorService.previousMonth();
    this.loadMonth(); // Ricarica i dati usando il servizio
  }

  nextMonth() {
    this.monthNavigatorService.nextMonth();
    this.loadMonth(); // Ricarica i dati usando il servizio
  }

  // Restituisce timbrature filtrate per tipo (utile per UI specifiche?)
  getTimestampsByType(records: TimeStamp[] | undefined, type: 'E' | 'U'): TimeStamp[] {
    return records?.filter(r => r.type === type) || [];
  }

  // RIMOSSO: getOrderedTimestamps - Il servizio dati ora garantisce che i record siano ordinati.
  // La template dovrà essere aggiornata per iterare direttamente su day.records.
  // getOrderedTimestamps(day: { records: TimeStamp[] } | undefined): TimeStamp[] {
  //   // logica rimossa
  // }

  // Verifica se c'è almeno un giustificativo per l'intera giornata
  hasFullDayJustification(justifications: Justification[] | undefined): boolean {
    return justifications?.some(j => j.isFullDay) || false;
  }

  // Restituisce classi CSS specifiche per tipo di giustificativo
  getJustificationClass(justification: Justification): string {
    switch (justification.code.toUpperCase()) {
      case 'FER': case 'FST': return 'justification-vacation';
      case 'MAL': return 'justification-sick';
      case 'PER': return 'justification-leave';
      default: return 'justification-other';
    }
  }
}
