import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { NavController } from '@ionic/angular';
import { Dip_GG_Richiesta_Body_Timbratura, Dip_GG_Richiesta_Send_InModel, StatoRichiesta, TipoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

@Component({
  selector: 'app-request-clocking-user-page',
  templateUrl: './request-clocking-user-page.component.html',
  styleUrls: ['./request-clocking-user-page.component.scss'],
  standalone: false
})
export class RequestClockingUserPageComponent implements OnInit {
  public title: string;

  //////
  public buttonbar: ButtonItem[] = [];
  public btnAnnulla: ButtonItem;
  public btnInvia: ButtonItem;
  //////
  public dateTime: string;
  public formattedDate: string;
  public formattedTime: string;
  public supervisors: string[];
  public notes: string;

  constructor(private navCtrl: NavController,
              public userNavigationService: UserNavigationService,
              private userInterfaceService: UserInterfaceService,
              private dipGGRichiestaService: DipGGRichiestaService,
              private stringHelperService: StringHelperService) {

    this.title = 'Richiedi timbratura';

    this.btnInvia = userInterfaceService.Btn_Invia;
    this.btnInvia.event = this._handleButtonConfirmClick;
    this.buttonbar.push(this.btnInvia);
    this.btnAnnulla = userInterfaceService.Btn_Annulla;
    this.btnAnnulla.event = this._handleButtonCancelClick;
    this.buttonbar.push(this.btnAnnulla);
 
  }

  ngOnInit() { }

  ionViewWillEnter() {

    // Initialize with current date and time
    const now = new Date();
    // Format date for ion-datetime (ISO format)
    //this.dateTime = now.toISOString();

    this.dateTime = this.stringHelperService.DateCurr_To_ISOString();


    this.formattedDate = this.stringHelperService.Date_To_S_ddmmyyyy(now)
    this.formattedTime = this.stringHelperService.Date_To_S_hhmm(now);

    this.supervisors = ['manzo.admin'];
    this.notes = '';
  }
   
  updateDateTime(event: any) {
    const selectedDate = new Date(event.detail.value);
    this.formattedDate = this.stringHelperService.Date_To_S_ddmmyyyy(selectedDate)
    this.formattedTime = this.stringHelperService.Date_To_S_hhmm(selectedDate);
  }

  addSupervisor() {
    // In a real app, this would open a modal or dropdown to select from available supervisors
    // For demo purposes, we'll just add a mock supervisor
    if (!this.supervisors.includes('new.supervisor')) {
      this.supervisors.push('new.supervisor');
    }
  }

  removeSupervisor(index: number) {
    this.supervisors.splice(index, 1);
  }

  submitRequest() {

    let request_rich = new GenericRequest<Dip_GG_Richiesta_Send_InModel>(Dip_GG_Richiesta_Send_InModel);

  
    let dip_GG_Richiesta_Body_Timbratura: Dip_GG_Richiesta_Body_Timbratura = new Dip_GG_Richiesta_Body_Timbratura();
    dip_GG_Richiesta_Body_Timbratura.hhmm = this.formattedTime;

    request_rich.data.dip_GG_Richiesta.id = 0;
    request_rich.data.dip_GG_Richiesta.idDip_RapportoLavoro = 0;
    request_rich.data.dip_GG_Richiesta.richiestaStato = StatoRichiesta.Immessa;
    request_rich.data.dip_GG_Richiesta.richiestaTipo = TipoRichiesta.Timbratura;
    request_rich.data.dip_GG_Richiesta.data = this.formattedDate;
    request_rich.data.dip_GG_Richiesta.dataA = this.formattedDate;
    request_rich.data.dip_GG_Richiesta.dati = this.stringHelperService.toJSONString(dip_GG_Richiesta_Body_Timbratura);

    this.dipGGRichiestaService.Send(request_rich).subscribe(res => {
      this.navCtrl.navigateForward('/usertimesheet');
    });

   

    //console.log('Request submitted', {
    //  type: this.requestType,
    //  dateTime: this.dateTime,
    //  //formattedDateTime: this.formattedDateTime,
    //  supervisors: this.supervisors,
    //  notes: this.notes
    //});

    //// In a real app, this would send the data to a service
    //alert('Richiesta inviata con successo!');

  }

  private _handleButtonConfirmClick = (param: object) => {

    this.submitRequest();

  }

  private _handleButtonCancelClick = (param: object) => {

    this.navCtrl.navigateForward('/home');

  }

}
