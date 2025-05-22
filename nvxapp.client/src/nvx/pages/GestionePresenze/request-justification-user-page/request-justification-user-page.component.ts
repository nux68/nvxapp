import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { Dip_GG_Richiesta_Body_Giustificativo, Dip_GG_Richiesta_Send_InModel, StatoRichiesta, TipoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { Par_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';

@Component({
  selector: 'app-request-justification-user-page',
  templateUrl: './request-justification-user-page.component.html',
  styleUrls: ['./request-justification-user-page.component.scss'],
  standalone: false
})
export class RequestJustificationUserPageComponent implements OnInit {
  public title: string;

  //////
  public buttonbar: ButtonItem[] = [];
  public btnAnnulla: ButtonItem;
  public btnInvia: ButtonItem;
  //////

  public startDate: string;
  public formattedStartDate: string;
  public endDate: string;
  public formattedEndDate: string;

  public justificationType: string = '';
  public hours: number = 1;
  public minutes: number = 0;
  public hoursFormatted: string = '01:00';
  public fullDay: boolean = true;  // Modificato a true come richiesto
  public supervisors: string[] = ['manzo.admin'];
  public notes: string = '';

  // Proprietà per accedere ai giustificativi
  public giustificativi: Par_GiustificativiModel[] = [];

  constructor(public userNavigationService: UserNavigationService,
    private navCtrl: NavController,
    private dipGGRichiestaService: DipGGRichiestaService,
    private stringHelperService: StringHelperService,
    private userInterfaceService: UserInterfaceService,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService) {
    this.title = 'Richiesta ferie e permessi';

    this.btnInvia = userInterfaceService.Btn_Invia;
    this.btnInvia.event = this._handleButtonConfirmClick;
    this.buttonbar.push(this.btnInvia);
    this.btnAnnulla = userInterfaceService.Btn_Annulla;
    this.btnAnnulla.event = this._handleButtonCancelClick;
    this.buttonbar.push(this.btnAnnulla);

    // Initialize variables immediately to prevent undefined errors
    this.initializeValues();
  }

  ngOnInit() {
    // Caricamento dei giustificativi dall'apposito servizio
    if (this.sharedParameterGestionePresenzeService.Par_Giustificativi) {
      this.giustificativi = this.sharedParameterGestionePresenzeService.Par_Giustificativi;
      // Se ci sono giustificativi disponibili, impostiamo il primo come default
      if (this.giustificativi.length > 0) {
        this.justificationType = this.giustificativi[0].codice;
      }
    }
  }

  // Method to explicitly initialize values
  initializeValues() {
    this.hours = 8; // Impostiamo a 8 ore per giorno intero
    this.minutes = 0;
    this.hoursFormatted = '08:00';
    this.fullDay = true; // Modificato a true come richiesto
    this.updateHoursFormatted();
  }

  ionViewWillEnter() {
    // Caricamento dei giustificativi se non già caricati
    if (this.sharedParameterGestionePresenzeService.Par_Giustificativi) {
      this.giustificativi = this.sharedParameterGestionePresenzeService.Par_Giustificativi;
      // Se ci sono giustificativi disponibili, impostiamo il primo come default
      if (this.giustificativi.length > 0) {
        this.justificationType = this.giustificativi[0].codice;
      }
    }

    // Initialize with current date in ISO 8601 format
    const now = new Date();

    // Set time to midnight for consistency when dealing with dates only
    now.setHours(0, 0, 0, 0);

    this.startDate = this.stringHelperService.DateCurr_To_ISOString();
    this.endDate = this.stringHelperService.DateCurr_To_ISOString();

    this.formattedStartDate = this.stringHelperService.Date_To_S_ddmmyyyy(now);
    this.formattedEndDate = this.stringHelperService.Date_To_S_ddmmyyyy(now);

    this.hours = 8; // Impostato a 8 ore per giorno intero
    this.minutes = 0;
    this.hoursFormatted = '08:00';
    this.fullDay = true; // Modificato a true come richiesto
    this.supervisors = ['manzo.admin'];
    this.notes = '';
    this.updateHoursFormatted();
  }

  // Funzione per confrontare due date in formato dd/mm/yyyy
  compareDates(date1: string, date2: string): number {
    // Converte da formato dd/mm/yyyy a Date objects per confronto
    const [day1, month1, year1] = date1.split('/').map(Number);
    const [day2, month2, year2] = date2.split('/').map(Number);

    const d1 = new Date(year1, month1 - 1, day1);
    const d2 = new Date(year2, month2 - 1, day2);

    // Ritorna -1 se d1 < d2, 0 se uguali, 1 se d1 > d2
    return d1 < d2 ? -1 : d1 > d2 ? 1 : 0;
  }

  updateStartDate(event: any) {
    const selectedDate = new Date(event.detail.value);
    this.formattedStartDate = this.stringHelperService.Date_To_S_ddmmyyyy(selectedDate);

    // Controlla se la data di inizio è successiva alla data di fine
    if (this.compareDates(this.formattedStartDate, this.formattedEndDate) > 0) {
      // Aggiorna la data di fine per farla coincidere con la data di inizio
      this.endDate = event.detail.value;
      this.formattedEndDate = this.formattedStartDate;
    }
  }

  updateEndDate(event: any) {
    const selectedDate = new Date(event.detail.value);
    this.formattedEndDate = this.stringHelperService.Date_To_S_ddmmyyyy(selectedDate);

    // Controlla se la data di fine è precedente alla data di inizio
    if (this.compareDates(this.formattedEndDate, this.formattedStartDate) < 0) {
      // Aggiorna la data di inizio per farla coincidere con la data di fine
      this.startDate = event.detail.value;
      this.formattedStartDate = this.formattedEndDate;
    }
  }

  increaseHours() {
    if (!this.fullDay && this.hours < 8) {
      this.hours++;
      this.updateHoursFormatted();
    }
  }

  decreaseHours() {
    if (!this.fullDay && this.hours > 0) {
      this.hours--;
      this.updateHoursFormatted();
    }
  }

  increaseMinutes() {
    if (!this.fullDay) {
      if (this.minutes === 45) {
        this.minutes = 0;
        if (this.hours < 8) {
          this.hours++;
        }
      } else {
        this.minutes += 15;
      }
      this.updateHoursFormatted();
    }
  }

  decreaseMinutes() {
    if (!this.fullDay) {
      if (this.minutes === 0) {
        this.minutes = 45;
        if (this.hours > 0) {
          this.hours--;
        }
      } else {
        this.minutes -= 15;
      }
      this.updateHoursFormatted();
    }
  }

  updateHoursFormatted() {
    const h = this.hours || 0;
    const m = this.minutes || 0;
    this.hoursFormatted = `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}`;
  }

  toggleFullDay() {
    if (this.fullDay) {
      this.hours = 8;
      this.minutes = 0;
    } else {
      this.hours = 1;
      this.minutes = 0;
    }
    this.updateHoursFormatted();
  }

  addSupervisor() {
    const newSupervisor = 'new.supervisor.' + (this.supervisors.length + 1);
    if (!this.supervisors.includes(newSupervisor)) {
      this.supervisors.push(newSupervisor);
    }
  }

  removeSupervisor(index: number) {
    if (index >= 0 && index < this.supervisors.length) {
      this.supervisors.splice(index, 1);
    }
  }

  submitRequest() {

  

    let request_rich = new GenericRequest<Dip_GG_Richiesta_Send_InModel>(Dip_GG_Richiesta_Send_InModel);

    const state = history.state;
    if (state && state.currUserId) {
      request_rich.data.idAspNetUsers = state.currUserId;
    }

    let dip_GG_Richiesta_Body_Giustificativo: Dip_GG_Richiesta_Body_Giustificativo = new Dip_GG_Richiesta_Body_Giustificativo();
    dip_GG_Richiesta_Body_Giustificativo.hhmm = this.hoursFormatted;
    dip_GG_Richiesta_Body_Giustificativo.allDay = this.fullDay;

    // Trova l'ID del giustificativo selezionato
    const giustificativoSelezionato = this.giustificativi.find(g => g.codice === this.justificationType);
    dip_GG_Richiesta_Body_Giustificativo.idPar_Giustificativi = giustificativoSelezionato ? giustificativoSelezionato.id : 1;

    request_rich.data.dip_GG_Richiesta.id = 0;
    request_rich.data.dip_GG_Richiesta.idDip_RapportoLavoro = 0;
    request_rich.data.dip_GG_Richiesta.richiestaStato = StatoRichiesta.Immessa;
    request_rich.data.dip_GG_Richiesta.richiestaTipo = TipoRichiesta.Giustificativo;
    request_rich.data.dip_GG_Richiesta.data = this.formattedStartDate;
    request_rich.data.dip_GG_Richiesta.dataA = this.formattedEndDate;
    request_rich.data.dip_GG_Richiesta.dati = this.stringHelperService.toJSONString(dip_GG_Richiesta_Body_Giustificativo);

    this.dipGGRichiestaService.Send(request_rich).subscribe(res => {
      //this.navCtrl.navigateForward('/usertimesheet');
      this.navCtrl.back();
    });
  }

  private _handleButtonConfirmClick = (param: object) => {
    this.submitRequest();
  }

  private _handleButtonCancelClick = (param: object) => {
    //this.navCtrl.navigateForward('/home');
    this.navCtrl.back();
  }
}
