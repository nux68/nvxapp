import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { NavController } from '@ionic/angular';
import { Dip_GG_Richiesta_Body_Timbratura, Dip_GG_Richiesta_Send_InModel, StatoRichiesta, TipoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { Dip_GG_Timbratura_StampPrepare_InModel, Dip_GG_Timbratura_StampPrepare_OutModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DipGGTimbraturaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/dip-gg-timbratura.service';
import { catchError, map, Observable, of } from 'rxjs';
import { Par_AttivitaModel } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/Models/par-attivita-model';

@Component({
  selector: 'app-request-clocking-user-page',
  templateUrl: './request-clocking-user-page.component.html',
  styleUrls: ['./request-clocking-user-page.component.scss'],
  standalone: false
})
export class RequestClockingUserPageComponent extends BasePageConfirmCancelComponent<Dip_GG_Timbratura_StampPrepare_OutModel> {
  public title: string;

  
  
  public dateTime: string;
  public formattedDate: string;
  public formattedTime: string;
  
  public notes: string;
  public par_AttivitaList: Par_AttivitaModel[] = []

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private dipGGTimbraturaService: DipGGTimbraturaService,
    private stringHelperService: StringHelperService,
    private dipGGRichiestaService: DipGGRichiestaService,
    private refresherService: RefresherService
  ) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return 'Richiedi timbratura'; }

  get EditForm(): FormGroup {
    return this.fb.group({
      currentDate: [null, [Validators.required]],
      idPar_Attivita: [null, [Validators.required]],
    });
  }

  override ionViewWillEnter() {
      const now = new Date();
      this.formattedDate = this.stringHelperService.Date_To_S_ddmmyyyy(now)
      this.formattedTime = this.stringHelperService.Date_To_S_hhmm(now);

    super.ionViewWillEnter();
  }

  updateDateTime(event: any) {
    const selectedDate = new Date(event.detail.value);
    this.formattedDate = this.stringHelperService.Date_To_S_ddmmyyyy(selectedDate)
    this.formattedTime = this.stringHelperService.Date_To_S_hhmm(selectedDate);
  }

  LoadData = (): Observable<Dip_GG_Timbratura_StampPrepare_OutModel | null> => {
    const request = new GenericRequest<Dip_GG_Timbratura_StampPrepare_InModel>(Dip_GG_Timbratura_StampPrepare_InModel);
    return this.dipGGTimbraturaService.PrepareStamp(request).pipe(
      map(res => {

        this.par_AttivitaList = res.data.par_Attivita;

        return res.data;
      }),
      catchError(error => {
        console.error('Errore durante PrepareStamp:', error);
        return of(null);
      })
    );
  };

  SaveData = (editModel: Dip_GG_Timbratura_StampPrepare_OutModel): Observable<boolean> => {

    let request_rich = new GenericRequest<Dip_GG_Richiesta_Send_InModel>(Dip_GG_Richiesta_Send_InModel);

    const state = history.state;
    if (state && state.currUserId) {
      request_rich.data.idAspNetUsers = state.currUserId;
      if (state.fromHR) {
        request_rich.data.fromHR = state.fromHR;
      }

    }

    let dip_GG_Richiesta_Body_Timbratura: Dip_GG_Richiesta_Body_Timbratura = new Dip_GG_Richiesta_Body_Timbratura();
    dip_GG_Richiesta_Body_Timbratura.hhmm = this.formattedTime;

    request_rich.data.dip_GG_Richiesta.id = 0;
    request_rich.data.dip_GG_Richiesta.idDip_RapportoLavoro = 0;
    request_rich.data.dip_GG_Richiesta.richiestaStato = StatoRichiesta.Immessa;
    request_rich.data.dip_GG_Richiesta.richiestaTipo = TipoRichiesta.Timbratura;
    request_rich.data.dip_GG_Richiesta.data = this.formattedDate;
    request_rich.data.dip_GG_Richiesta.dataA = this.formattedDate;
    request_rich.data.dip_GG_Richiesta.dati = this.stringHelperService.toJSONString(dip_GG_Richiesta_Body_Timbratura);

    return this.dipGGRichiestaService.Send(request_rich).pipe(map(() => {
      this.refresherService.Dip_GG_Richiesta_triggerRefresh();
      return true;
    } ));
    


  };

  onStartDatetimeChange(event: any) {
    const value = event?.detail?.value;
    if (value) {
      const selected = new Date(value);
      
      this.formattedDate = this.formatDate(selected);
      
    }
  }

  formatDate(date: Date): string {
    return `${date.getDate().toString().padStart(2, '0')}/${(date.getMonth() + 1).toString().padStart(2, '0')}/${date.getFullYear()}`;
  }


}
