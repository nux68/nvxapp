import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { environment } from '../../../../environments/environment';
import { MonthNavigatorService } from '../../../Utility/infrastructure/month-navigator.service';
import { SignalrService } from '../../../Utility/infrastructure/signalr.service';
import { MokeTimeSheetService, MonthData } from '../../../Utility/GestionePresenze/moke-time-sheet.service';
import { Dip_GG_TimbraturaModel, TipoTimbratura } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { Dip_GG_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';

@Component({
  selector: 'app-time-sheet-user-page',
  templateUrl: './time-sheet-user-page.component.html',
  styleUrls: ['./time-sheet-user-page.component.scss'],
  standalone:false
}) 

export class TimeSheetUserPageComponent implements OnInit {
  public title!: string;

  TipoTimbratura = TipoTimbratura;

  currentMonth: MonthData; // Usa l'interfaccia importata
  // Usa le interfacce importate nella definizione di 'weeks'
  weeks: Array<Array<{
    day: number,
    isCurrentMonth: boolean,
    dip_GG_Timbratura: Dip_GG_TimbraturaModel[],
    dip_GG_Giustificativi: Dip_GG_GiustificativiModel[]
  }>>;

  currentMonthDisplay: string;

  constructor(
              private signalrService: SignalrService,
              public monthNavigatorService: MonthNavigatorService,
              private calendarDataService: MokeTimeSheetService 
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

    // Chiama il metodo del servizio dati
    this.calendarDataService.getMonthData(year, month).subscribe(monthData => {
      this.currentMonth = monthData; // monthData è già del tipo corretto MonthData
      // Usa monthNames dal servizio di navigazione come prima
      this.currentMonthDisplay = `${this.monthNavigatorService.monthNames[month]} - ${year}`;
      this.buildCalendarWeeks(); // Costruisce la UI dopo aver ricevuto i dati
    });
  }
  
  buildCalendarWeeks() {
    this.weeks = [];
    const year = this.monthNavigatorService.currentYear;
    const month = this.monthNavigatorService.currentMonth;

    const firstDay = new Date(year, month, 1);
    let dayOfWeek = firstDay.getDay() || 7;
    dayOfWeek = dayOfWeek - 1;

    const lastDay = new Date(year, month + 1, 0).getDate();
    const prevMonthLastDay = new Date(year, month, 0).getDate();

    let currentWeek: Array<{
      day: number,
      //records: TimeStamp[],
      //justifications: Justification[],
      isCurrentMonth: boolean,
      //nvx
      dip_GG_Timbratura: Dip_GG_TimbraturaModel[],
      dip_GG_Giustificativi: Dip_GG_GiustificativiModel[]
    }> = [];

    // Giorni mese precedente
    for (let i = 0; i < dayOfWeek; i++) {
      const day = prevMonthLastDay - dayOfWeek + i + 1;
      currentWeek.push({
        day: day,
        //records: [],
        //justifications: [],
        isCurrentMonth: false,
        dip_GG_Timbratura: [],
        dip_GG_Giustificativi:[]
      });
    }

    // Giorni mese corrente
    for (let i = 1; i <= lastDay; i++) {
      // Usa i dati da this.currentMonth popolato dal servizio
      const dayData = this.currentMonth?.days?.[i]; // Può essere undefined se non ci sono dati per quel giorno

      currentWeek.push({
        day: i,
        // Usa fallback se dayData non esiste o se le proprietà sono vuote
        // Il servizio dati dovrebbe già fornire array vuoti dove appropriato
        //records: dayData?.timestamps || [],
        //justifications: dayData?.justifications || [],
        isCurrentMonth: true,
        //nvx
        dip_GG_Timbratura: dayData?.dip_GG_Timbratura || [],
        dip_GG_Giustificativi:dayData?.dip_GG_Giustificativi || []
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
        currentWeek.push({
          day: nextMonthDay,
          //records: [],
          //justifications: [],
          isCurrentMonth: false,
          //nvx
          dip_GG_Timbratura: [],
          dip_GG_Giustificativi:[]
        });
        nextMonthDay++;
      }
      this.weeks.push(currentWeek);
    }
  }

  previousMonth() {
    this.monthNavigatorService.previousMonth();
    this.loadMonth(); // Ricarica i dati usando il servizio
  }

  nextMonth() {
    this.monthNavigatorService.nextMonth();
    this.loadMonth(); // Ricarica i dati usando il servizio
  }

  // Restituisce timbrature filtrate per tipo (utile per UI specifiche?)
  getTimestampsByType(records: Dip_GG_TimbraturaModel[] | undefined, type: TipoTimbratura): Dip_GG_TimbraturaModel[] {
    return records?.filter(r => r.timbraturaTipo === type) || [];
  }
  
  hasFullDayJustification(justifications: Dip_GG_GiustificativiModel[] | undefined): boolean {
    return false; //justifications?.some(j => j.isFullDay) || false;
  }

  // Restituisce classi CSS specifiche per tipo di giustificativo
  getJustificationClass(justification: Dip_GG_GiustificativiModel): string {
    //switch (justification.code.toUpperCase()) {
    //  case 'FER': case 'FST': return 'justification-vacation';
    //  case 'MAL': return 'justification-sick';
    //  case 'PER': return 'justification-leave';
    //  default: return 'justification-other';
    //}
    return 'justification-other';

  }

  getDayClass(day: any, index: number): { [key: string]: boolean } {
    return {
      'non-current-month': !day.isCurrentMonth,
      'weekend': index > 4,
      'has-content': (day.dip_GG_Timbratura && day.dip_GG_Timbratura.length > 0) ||
                     (day.dip_GG_Giustificativi && day.dip_GG_Giustificativi.length > 0),
      'full-day-justification': this.hasFullDayJustification(day.dip_GG_Giustificativi)
    };
  }

  getTimestampClass(record: any): { [key: string]: boolean } {
    return {
      'entry': record.timbraturaTipo === TipoTimbratura.Entrata,
      'exit': record.timbraturaTipo === TipoTimbratura.Uscita
    };
  }


}
