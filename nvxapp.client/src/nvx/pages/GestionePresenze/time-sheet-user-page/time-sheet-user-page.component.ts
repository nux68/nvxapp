import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { environment } from '../../../../environments/environment';
import { MonthNavigatorService } from '../../../Utility/infrastructure/month-navigator.service';
import { SignalrService } from '../../../Utility/infrastructure/signalr.service';
import { TimeSheetService } from '../../../Utility/GestionePresenze/time-sheet.service';
import { MonthData } from '../../../Utility/GestionePresenze/time-sheet-common-data';
import { Dip_GG_TimbraturaModel, TipoTimbratura } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { Dip_GG_GiustificativiModel, JustificationInputType } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';
import { StatoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';


@Component({
  selector: 'app-time-sheet-user-page',
  templateUrl: './time-sheet-user-page.component.html',
  styleUrls: ['./time-sheet-user-page.component.scss'],
  standalone: false
})

export class TimeSheetUserPageComponent implements OnInit {
  public title!: string;

  TipoTimbratura = TipoTimbratura;
  StatoRichiesta = StatoRichiesta;

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
    private timeSheetService: TimeSheetService,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
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
    this.timeSheetService.getMonthData(year, month).subscribe(monthData => {
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
      isCurrentMonth: boolean,
      dip_GG_Timbratura: Dip_GG_TimbraturaModel[],
      dip_GG_Giustificativi: Dip_GG_GiustificativiModel[]
    }> = [];

    // Giorni mese precedente
    for (let i = 0; i < dayOfWeek; i++) {
      const day = prevMonthLastDay - dayOfWeek + i + 1;
      currentWeek.push({
        day: day,
        isCurrentMonth: false,
        dip_GG_Timbratura: [],
        dip_GG_Giustificativi: []
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
        isCurrentMonth: true,
        dip_GG_Timbratura: dayData?.dip_GG_Timbratura || [],
        dip_GG_Giustificativi: dayData?.dip_GG_Giustificativi || []
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
          isCurrentMonth: false,
          dip_GG_Timbratura: [],
          dip_GG_Giustificativi: []
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
    return justifications?.some(j => j.inputType == JustificationInputType.AllDay,) || false;
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

  // Helper method to get appropriate icon for request status
  getStatusIcon(status: StatoRichiesta): string {
    switch (status) {
      case StatoRichiesta.Diretta:
        return 'checkmark-circle'; // Direct entry
      case StatoRichiesta.Immessa:
        return 'time-outline'; // Submitted
      case StatoRichiesta.ApprovazioneInCorso:
        return 'hourglass-outline'; // In progress
      case StatoRichiesta.ParzialmenteApprovata:
        return 'alert-circle-outline'; // Partially approved
      case StatoRichiesta.Approvata:
        return 'checkmark-circle-outline'; // Approved
      case StatoRichiesta.Rifiutata:
        return 'close-circle-outline'; // Rejected
      case StatoRichiesta.Cancellata:
        return 'trash-outline'; // Cancelled
      default:
        return 'help-circle-outline'; // Unknown status
    }
  }

  // Helper method to get text description for request status
  getStatusText(status: StatoRichiesta): string {
    switch (status) {
      case StatoRichiesta.Diretta:
        return 'Direct Entry';
      case StatoRichiesta.Immessa:
        return 'Submitted';
      case StatoRichiesta.ApprovazioneInCorso:
        return 'Approval In Progress';
      case StatoRichiesta.ParzialmenteApprovata:
        return 'Partially Approved';
      case StatoRichiesta.Approvata:
        return 'Approved';
      case StatoRichiesta.Rifiutata:
        return 'Rejected';
      case StatoRichiesta.Cancellata:
        return 'Cancelled';
      default:
        return 'Unknown Status';
    }
  }

  get_dip_GG_Giustificativi_backColor(ggJust: Dip_GG_GiustificativiModel): string {
    
    var just = this.sharedParameterGestionePresenzeService.Par_Giustificativi.find(x => x.id == ggJust.idPar_Giustificativi);
    if (just)
      return just.backgroundColor;
    else
      return null;
  }

  get_dip_GG_Giustificativi_txtColor(ggJust: Dip_GG_GiustificativiModel): string {

    var just = this.sharedParameterGestionePresenzeService.Par_Giustificativi.find(x => x.id == ggJust.idPar_Giustificativi);
    if (just)
      return just.textColor;
    else
      return null;
  }

}
