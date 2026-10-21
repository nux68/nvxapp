import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
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
import { catchError, map, Observable, of, forkJoin } from 'rxjs';
import { Par_AttivitaModel } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/Models/par-attivita-model';
import { Az_SubCommessaAttivita_4FullListModel, Az_SubCommessaAttivita_GetAll_4FullList_4User_InModel } from '../../../ClientServer-Service/GestionePresenze/Az_SubCommessaAttivita/Models/az-subcommessa-attivita-model';
import { AzSubCommessaAttivitaService } from '../../../ClientServer-Service/GestionePresenze/Az_SubCommessaAttivita/az-subcommessa-attivita.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';

@Component({
  selector: 'app-request-clocking-user-page',
  templateUrl: './request-clocking-user-page.component.html',
  styleUrls: ['./request-clocking-user-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class RequestClockingUserPageComponent extends BasePageConfirmCancelComponent<Dip_GG_Timbratura_StampPrepare_OutModel> {
  public title: string;

  
  
  public dateTime: string;
  public formattedDate: string;
  public formattedTime: string;
  
  public notes: string;
  public par_AttivitaList: Par_AttivitaModel[] = []
  public az_SubCommessaAttivita_4FullList: Az_SubCommessaAttivita_4FullListModel[] = [];

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private dipGGTimbraturaService: DipGGTimbraturaService,
    private stringHelperService: StringHelperService,
    private dipGGRichiestaService: DipGGRichiestaService,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private refresherService: RefresherService,
    private azSubCommessaAttivitaService: AzSubCommessaAttivitaService
  ) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return 'Richiedi timbratura'; }

  get EditForm(): FormGroup {
    return this.fb.group({
      currentDate: [null, [Validators.required]],
      idPar_Attivita: [null, [Validators.required, Validators.min(1)]],
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

    // solo le attività consentite al dipendente (quello passato dall'HR, altrimenti l'utente corrente)
    const request_att = new GenericRequest<Az_SubCommessaAttivita_GetAll_4FullList_4User_InModel>(Az_SubCommessaAttivita_GetAll_4FullList_4User_InModel);
    request_att.data.idAspNetUsers = history.state?.currUserId ?? null;

    const request = new GenericRequest<Dip_GG_Timbratura_StampPrepare_InModel>(Dip_GG_Timbratura_StampPrepare_InModel);
    return forkJoin({
      prepare: this.dipGGTimbraturaService.PrepareStamp(request),
      attivita: this.azSubCommessaAttivitaService.GetAll_4FullList_4User(request_att)
    }).pipe(
      map(({ prepare, attivita }) => {

        this.par_AttivitaList = prepare.data.par_Attivita;
        this.az_SubCommessaAttivita_4FullList = attivita.data?.az_SubCommessaAttivita ?? [];

        // preselezione: attività dell'ultima timbratura se ancora consentita, altrimenti la prima consentita
        if (attivita.data?.idAz_SubCommessaAttivita_Proposta)
          prepare.data.idPar_Attivita = attivita.data.idAz_SubCommessaAttivita_Proposta;

        return prepare.data;
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
    dip_GG_Richiesta_Body_Timbratura.idAz_SubCommessaAttivita = this._editForm.get('idPar_Attivita')?.value;
    

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
