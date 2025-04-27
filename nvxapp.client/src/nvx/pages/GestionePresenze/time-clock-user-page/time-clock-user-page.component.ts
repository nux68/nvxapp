import { Component, OnInit, OnDestroy } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { DipGGTimbraturaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/dip-gg-timbratura.service';
import { Dip_GG_Timbratura_Stamp_InModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { NavController } from '@ionic/angular';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

@Component({
  selector: 'app-time-clock-user-page',
  templateUrl: './time-clock-user-page.component.html',
  styleUrls: ['./time-clock-user-page.component.scss'],
  standalone: false
})
export class TimeClockUserPageComponent implements OnInit, OnDestroy {
  public title: string;
  //////
  public buttonbar: ButtonItem[] = [];
  public btnAnnulla: ButtonItem;
  public btnInvia: ButtonItem;
  //////

  public location: string;
  public currentDate: Date;
  public currentTime: string;
  public formattedDate: string;
  public lastAction: string;

  public startDate: string;
  public startDateBtn: string | undefined = undefined;

  // Traccia lo stato dell'ultima timbratura (entrata o uscita)
  private isLastActionCheckIn: boolean = false;

  private timeInterval: any;

  constructor(private navCtrl: NavController,
              private userInterfaceService: UserInterfaceService,
              public userNavigationService: UserNavigationService,
              private dipGGTimbraturaService: DipGGTimbraturaService) {

    this.title = 'Terminale di timbratura';

    

    this.btnInvia = userInterfaceService.Btn_Invia;
    this.btnInvia.event = this._handleButtonConfirmClick;
    this.buttonbar.push(this.btnInvia);
    this.btnAnnulla = userInterfaceService.Btn_Annulla;
    this.btnAnnulla.event = this._handleButtonCancelClick;
    this.buttonbar.push(this.btnAnnulla);

  }

  ngOnInit() {
    this.startClock();
  }

  ngOnDestroy() {
    this.stopClock();
  }

  ionViewWillEnter() {

    this.location = 'Via Vesuvio';
    this.currentDate = new Date();
    this.currentTime = this.formatTime(this.currentDate);
    this.formattedDate = this.formatDate(this.currentDate);
    this.lastAction = 'xxxxx';

    // Imposta lo stato iniziale in base all'ultima azione (qui assumiamo che fosse un'uscita)
    this.isLastActionCheckIn = false;
    this.startDate = new Date().toLocaleDateString();

    this.startClock();
  }


  ionViewWillLeave() {
    this.stopClock();
  }

  startClock() {
    this.timeInterval = setInterval(() => {
      this.currentDate = new Date();
      this.currentTime = this.formatTime(this.currentDate);
      this.formattedDate = this.formatDate(this.currentDate);
    }, 1000);
  }

  stopClock() {
    if (this.timeInterval) {
      clearInterval(this.timeInterval);
    }
  }

  formatTime(date: Date): string {
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;
  }

  formatDate(date: Date): string {
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
  }

  // Nuovo metodo che alterna tra entrata e uscita
  clockInOut() {
    const now = new Date();
    const timeStr = this.formatTime(now);
    const dateStr = this.formatDate(now);

    // Alterna tra entrata e uscita ad ogni click
    //this.isLastActionCheckIn = !this.isLastActionCheckIn;

    //// Aggiorna il messaggio dell'ultima azione in base allo stato attuale
    //if (this.isLastActionCheckIn) {
    //  this.lastAction = `entrata ${timeStr} (${this.startDate})`;
    //  // Logica per inviare i dati di entrata al server
    //} else {
    //  this.lastAction = `uscita ${timeStr} (${this.startDate})`;
    //  // Logica per inviare i dati di uscita al server
    //}

    if (this.startDateBtn) {
      this.startDate = new Date(this.startDateBtn).toLocaleDateString()
    }


    this.lastAction = ` ${timeStr} (${this.startDate})`;
    

    let request_stamp = new GenericRequest<Dip_GG_Timbratura_Stamp_InModel>(Dip_GG_Timbratura_Stamp_InModel);
    request_stamp.data.dateStamp = this.startDate;
    this.dipGGTimbraturaService.Stamp(request_stamp).subscribe(res => {
      this.navCtrl.navigateForward('/usertimesheet');
    });

  }

  private _handleButtonConfirmClick = (param: object) => {

    this.clockInOut();

  }

  private _handleButtonCancelClick = (param: object) => {

    this.navCtrl.navigateForward('/home');

  }

}
