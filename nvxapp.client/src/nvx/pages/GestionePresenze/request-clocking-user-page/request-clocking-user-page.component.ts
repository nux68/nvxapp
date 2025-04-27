// request-clocking-user-page.component.ts
import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { NavController } from '@ionic/angular';
import { Dip_GG_Richiesta_Body_Timbratura, Dip_GG_Richiesta_Send_InModel, StatoRichiesta, TipoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Dip_GG_TimbraturaModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';

@Component({
  selector: 'app-request-clocking-user-page',
  templateUrl: './request-clocking-user-page.component.html',
  styleUrls: ['./request-clocking-user-page.component.scss'],
  standalone: false
})
export class RequestClockingUserPageComponent implements OnInit {
  public title: string;
  public requestType: string;

  public dateTime: string;
  public formattedDate: string;
  public formattedTime: string;

  public supervisors: string[];
  public notes: string;

  constructor(private navCtrl: NavController,
              public userNavigationService: UserNavigationService,
              private dipGGRichiestaService: DipGGRichiestaService,
              private stringHelperService: StringHelperService) {

    this.title = 'Richiedi timbratura';
    this.requestType = 'ENTRATA';

    // Initialize with current date and time
    const now = new Date();
    // Format date for ion-datetime (ISO format)
    //this.dateTime = now.toISOString();

    this.dateTime = this.stringHelperService.DateTimeCurr_To_ISOString();

    //DateTime parsedDate = DateTime.ParseExact(dateString, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);

    this.formattedDate = this.stringHelperService.DateTime_To_ddmmyyyy(now)
    this.formattedTime = this.stringHelperService.DateTime_To_hhmm(now);

    this.supervisors = ['manzo.admin'];
    this.notes = '';
  }

  ionViewWillEnter() {
  }

  ngOnInit() { }

  formatDateTime(date: Date): string {
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${day}/${month}/${year} ${hours}:${minutes}`;
  }

  updateDateTime(event: any) {
    const selectedDate = new Date(event.detail.value);
    this.formattedDate = this.stringHelperService.DateTime_To_ddmmyyyy(selectedDate)
    this.formattedTime = this.stringHelperService.DateTime_To_hhmm(selectedDate);
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

    request_rich.data.dip_GG_RichiestaModel.id = 0;
    request_rich.data.dip_GG_RichiestaModel.idDip_RapportoLavoro = 0;
    request_rich.data.dip_GG_RichiestaModel.richiestaStato = StatoRichiesta.Immessa;
    request_rich.data.dip_GG_RichiestaModel.richiestaTipo = TipoRichiesta.Timbratura;
    request_rich.data.dip_GG_RichiestaModel.data = this.formattedDate;
    request_rich.data.dip_GG_RichiestaModel.dataA = this.formattedDate;
    request_rich.data.dip_GG_RichiestaModel.dati = this.stringHelperService.toJSONString(dip_GG_Richiesta_Body_Timbratura);

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
}
