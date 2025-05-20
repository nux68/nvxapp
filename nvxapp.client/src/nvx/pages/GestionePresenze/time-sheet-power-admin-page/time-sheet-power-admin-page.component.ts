import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { TimeSheetService } from '../../../Utility/GestionePresenze/time-sheet.service';
import { MonthData } from '../../../Utility/GestionePresenze/time-sheet-common-data';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { Dip_GG_TimbraturaModel, TipoTimbratura } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { Dip_GG_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';
import { StatoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';

interface DayData {
  date: Date;
  dayOfMonth: number;
  dip_GG_Timbratura: Dip_GG_TimbraturaModel[];
  dip_GG_Giustificativi: Dip_GG_GiustificativiModel[];
}

@Component({
  selector: 'app-time-sheet-power-admin-page',
  templateUrl: './time-sheet-power-admin-page.component.html',
  styleUrls: ['./time-sheet-power-admin-page.component.scss'],
  standalone: false
}) 
export class TimeSheetPowerAdminPageComponent implements OnInit {

  public currYear: number;
  public currMonth: number;
  public currUserId: string | undefined;

  public title: string;

  currentMonth: MonthData; // Interfaccia importata per i dati mensili
  currentDays: DayData[] = []; // Array ordinato per i giorni del mese

  TipoTimbratura = TipoTimbratura;
  StatoRichiesta = StatoRichiesta;

  constructor(
    private timeSheetService: TimeSheetService,
    public userNavigationService: UserNavigationService,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
  ) {
    this.title = 'Controllo Presenze';
    this.currentMonth = { year: 0, month: 0, days: {} };
  }

  ngOnInit() {
    // Inizializzazione componente
  }

  loadMonth() {
    if (!this.currUserId) {
      // Se non c'è un utente selezionato, resettiamo i dati
      this.currentDays = [];
      return;
    }

    // Chiama il servizio per ottenere i dati del mese
    this.timeSheetService.getMonthData(this.currYear, this.currMonth, this.currUserId).subscribe(monthData => {
      this.currentMonth = monthData;
      this.buildDaysList();
    });
  }

  buildDaysList() {
    this.currentDays = [];

    // Ottiene il numero di giorni nel mese
    const daysInMonth = new Date(this.currYear, this.currMonth + 1, 0).getDate();

    // Crea un array di oggetti giorno per tutti i giorni del mese
    for (let i = 1; i <= daysInMonth; i++) {
      const date = new Date(this.currYear, this.currMonth, i);

      // Controlla se ci sono dati per questo giorno
      const dayData = this.currentMonth.days[i];

      this.currentDays.push({
        date: date,
        dayOfMonth: i,
        dip_GG_Timbratura: dayData?.dip_GG_Timbratura || [],
        dip_GG_Giustificativi: dayData?.dip_GG_Giustificativi || []
      });
    }
  }

  isWeekend(date: Date): boolean {
    const day = date.getDay();
    return day === 0 || day === 6; // 0 = domenica, 6 = sabato
  }

  getTimestampClass(record: Dip_GG_TimbraturaModel): { [key: string]: boolean } {
    return {
      'entry': record.timbraturaTipo === TipoTimbratura.Entrata,
      'exit': record.timbraturaTipo === TipoTimbratura.Uscita
    };
  }

  // Helper method per ottenere l'icona appropriata per lo stato della richiesta
  getStatusIcon(status: StatoRichiesta): string {
    switch (status) {
      case StatoRichiesta.Diretta:
        return 'checkmark-circle'; // Inserimento diretto
      case StatoRichiesta.Immessa:
        return 'time-outline'; // In attesa
      case StatoRichiesta.ApprovazioneInCorso:
        return 'hourglass-outline'; // In corso
      case StatoRichiesta.ParzialmenteApprovata:
        return 'alert-circle-outline'; // Parzialmente approvata
      case StatoRichiesta.Approvata:
        return 'checkmark-circle-outline'; // Approvata
      case StatoRichiesta.Rifiutata:
        return 'close-circle-outline'; // Rifiutata
      case StatoRichiesta.Cancellata:
        return 'trash-outline'; // Cancellata
      default:
        return 'help-circle-outline'; // Stato sconosciuto
    }
  }

  // Helper method per ottenere il testo descrittivo dello stato della richiesta
  getStatusText(status: StatoRichiesta): string {
    switch (status) {
      case StatoRichiesta.Diretta:
        return 'Inserimento Diretto';
      case StatoRichiesta.Immessa:
        return 'In Attesa';
      case StatoRichiesta.ApprovazioneInCorso:
        return 'Approvazione In Corso';
      case StatoRichiesta.ParzialmenteApprovata:
        return 'Parzialmente Approvata';
      case StatoRichiesta.Approvata:
        return 'Approvata';
      case StatoRichiesta.Rifiutata:
        return 'Rifiutata';
      case StatoRichiesta.Cancellata:
        return 'Cancellata';
      default:
        return 'Stato Sconosciuto';
    }
  }

  get_dip_GG_Giustificativi_backColor(ggJust: Dip_GG_GiustificativiModel): string {
    const just = this.sharedParameterGestionePresenzeService.Par_Giustificativi.find(x => x.id == ggJust.idPar_Giustificativi);
    if (just)
      return just.backgroundColor;
    else
      return null;
  }

  get_dip_GG_Giustificativi_txtColor(ggJust: Dip_GG_GiustificativiModel): string {
    const just = this.sharedParameterGestionePresenzeService.Par_Giustificativi.find(x => x.id == ggJust.idPar_Giustificativi);
    if (just)
      return just.textColor;
    else
      return null;
  }

  onPeriodChange(period: { year: number, month: number } | undefined): void {
    if (period) {
      this.currYear = period.year;
      this.currMonth = period.month - 1;
      this.loadMonth();
    }
  }

  onCurrentUserChanged(userId: string | undefined): void {
    this.currUserId = userId;
    this.loadMonth();
  }

  // Metodi per gestire altri eventi dall'app-sedi-reparto-user-navigation
  onSedeChanged(sediId: number | undefined): void { }
  onRepartiChanged(repartoIds: number[] | undefined): void { }
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void { }
}
