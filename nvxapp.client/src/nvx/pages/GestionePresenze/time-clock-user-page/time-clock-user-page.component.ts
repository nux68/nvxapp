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
  public buttonbar: ButtonItem[] = [];
  public btnAnnulla: ButtonItem;
  public btnInvia: ButtonItem;

  public location: string;
  public currentDate: Date;
  public currentTime: string;
  public formattedDate: string;
  public lastAction: string;

  public startDate: string;
  public startDateBtn: string | undefined = undefined;

  private isLastActionCheckIn: boolean = false;
  private timeInterval: any;

  // true quando l'utente ha modificato manualmente il picker → il timer NON sovrascrive
  private userHasEdited: boolean = false;

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

    this.isLastActionCheckIn = false;
    this.userHasEdited = false;  // reset ad ogni apertura della pagina
    this.startDate = new Date().toLocaleDateString();

    this.startClock();
  }

  ionViewWillLeave() {
    this.stopClock();
  }

  startClock() {
    const tick = () => {
      this.currentDate = new Date();

      // aggiorna currentTime, formattedDate e startDateBtn
      // SOLO se l'utente non ha modificato manualmente il picker
      if (!this.userHasEdited) {
        this.currentTime = this.formatTime(this.currentDate);
        this.formattedDate = this.formatDate(this.currentDate);
        this.startDateBtn = this.toIsoLocal(this.currentDate);
      }

      const msToNextMinute = (60 - this.currentDate.getSeconds()) * 1000
                             - this.currentDate.getMilliseconds();

      this.timeInterval = setTimeout(tick, msToNextMinute);
    };

    tick();
  }

  stopClock() {
    if (this.timeInterval) {
      clearTimeout(this.timeInterval);
    }
  }

  private toIsoLocal(date: Date): string {
    const pad = (n: number) => n.toString().padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}` +
           `T${pad(date.getHours())}:${pad(date.getMinutes())}:00`;
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

  clockInOut() {
    const now = new Date();
    const stampDate: Date = this.startDateBtn ? new Date(this.startDateBtn) : now;

    this.lastAction = ` ${this.formatTime(stampDate)} (${this.formatDate(stampDate)})`;

    let request_stamp = new GenericRequest<Dip_GG_Timbratura_Stamp_InModel>(Dip_GG_Timbratura_Stamp_InModel);
    request_stamp.data.dateStamp = this.toIsoLocal(stampDate);
    this.dipGGTimbraturaService.Stamp(request_stamp).subscribe(res => {
      this.navCtrl.navigateForward('/usertimesheet');
    });
  }

  onStartDatetimeChange(event: any) {
    const value = event?.detail?.value;
    if (value) {
      const selected = new Date(value);
      this.startDateBtn = this.toIsoLocal(selected);
      this.currentTime = this.formatTime(selected);
      this.formattedDate = this.formatDate(selected);

      // l'utente ha modificato manualmente → sospendi l'aggiornamento automatico del picker
      this.userHasEdited = true;
    }
  }

  private _handleButtonConfirmClick = (param: object) => {
    this.clockInOut();
  }

  private _handleButtonCancelClick = (param: object) => {
    this.navCtrl.navigateForward('/home');
  }
}
