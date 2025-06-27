import { Component } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ModalController, NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { Az_CommessaGetInModel, Az_CommessaGetOutModel, Az_CommessaModel, Az_CommessaPutInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Commessa/Models/az-commessa-model';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { Az_ClienteModel } from '../../../ClientServer-Service/GestionePresenze/Az_Cliente/Models/az-cliente-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { AzCommessaService } from '../../../ClientServer-Service/GestionePresenze/Az_Commessa/az-commessa.service';
import { CheckObjOn_Id_Number, CheckObjOn_Id_Text } from '../../../ClientServer-Service/ModelsBase/check-obj';
import { Dip_AnagraficaModel } from '../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model';
import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { Az_SediRepartoModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { Par_AttivitaModel } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/Models/par-attivita-model';
import { FabMenuService, FabMenuItem } from '../../../Utility/infrastructure/fab-menu.service';
import { SeletionSediRepartoDialogComponent, SeletionSediRepartoDialogResult } from '../../../shared/components/GestionePresenze/seletion-sedi-reparto-dialog/seletion-sedi-reparto-dialog.component';
import { SeletionSediRepartoUserDialogComponent, SeletionSediRepartoUserDialogResult } from '../../../shared/components/GestionePresenze/seletion-sedi-reparto-user-dialog/seletion-sedi-reparto-user-dialog.component';

@Component({
  selector: 'app-commessa-edit-page',
  templateUrl: './commessa-edit-page.component.html',
  styleUrls: ['./commessa-edit-page.component.scss'],
  standalone: false
})
export class CommessaEditPageComponent extends BasePageConfirmCancelComponent<Az_CommessaGetOutModel> {

  public override _editForm: FormGroup;
  public _az_ClienteModelList: Az_ClienteModel[] = [];
  public _az_SediRepartoList: Az_SediRepartoModel[] = [];
  public _par_Attivita: Par_AttivitaModel[] = [];

  public searchText!: string;
  public currSection: string = "sez_1";
  public currSection_sub: string = "sez_1_sub_1";
  public idxCurrCommessa: number = -1;


  public btnDeleteReparto: ButtonItem;
  public btnDeleteUser: ButtonItem;


  //date x il backend
  public formattedStartDate: string;
  public formattedEndDate: string;
  //date per i controlli ionic
  public startDate: string;
  public endDate: string;

  ////
  public dip_Anagrafica: Dip_AnagraficaModel[];
  selected_Az_SediReparto: CheckObjOn_Id_Number[] = [];
  selected_User: CheckObjOn_Id_Text[] = [];
  ////


  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              protected override fb: FormBuilder,
              public fabMenuService: FabMenuService,
              private stringHelperService: StringHelperService,
              private azCommessaService: AzCommessaService,
              private refresherService: RefresherService,
              private modalCtrl: ModalController)
  {

    super(navCtrl, userInterfaceService, fb);

    this.btnDeleteReparto = userInterfaceService.Btn_Cancella;
    this.btnDeleteReparto.event = this.handleButtonDeleteRepartoClick;

    this.btnDeleteUser = userInterfaceService.Btn_Cancella;
    this.btnDeleteUser.event = this.handleButtonDeleteUserClick;

    

    this._editForm = this.fb.group({
        tmp_az_SubCommessa: [null, []],
            //SUB FORM PER OGGETTi DI OGGETTI
            az_Commessa: this.fb.group({
              descrizione: [null, [Validators.required, Validators.maxLength(50)]],
              idAz_Cliente: [null, [Validators.required]],
            })
        });
  }

  override get EditForm(): FormGroup {
    return this._editForm;
  }

  override ionViewWillEnter() {
    super.ionViewWillEnter();
    this._az_ClienteModelList = this.sharedParameterGestionePresenzeService.Az_Cliente;
    this._az_SediRepartoList = this.sharedParameterGestionePresenzeService.Az_SediReparto;
    this._par_Attivita = this.sharedParameterGestionePresenzeService.Par_Attivita;

    const now = new Date();

    // Set time to midnight for consistency when dealing with dates only
    now.setHours(0, 0, 0, 0);
    this.startDate = this.stringHelperService.DateCurr_To_ISOString();
    this.endDate = this.stringHelperService.DateCurr_To_ISOString();
    this.formattedStartDate = this.stringHelperService.Date_To_S_ddmmyyyy(now);
    this.formattedEndDate = this.stringHelperService.Date_To_S_ddmmyyyy(now);
    //
    this.dip_Anagrafica = this.sharedParameterGestionePresenzeService.Dip_Anagrafica;

    //this.fabMenuService.fabMenuItem = [

    //  new FabMenuItem('xxx', 'add-circle-outline', () => {
    //    //this.navCtrl.navigateForward('/seletionsedirepartopage', {
    //    //  state: { id: 0 }
    //    //});

    //    this.SeletionSediRepartoDialogOpen();

    //  }),

    //];

    this.setfabMenuService();

  }


  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  get Title(): string { return 'Commessa'; }

  LoadData = (): Observable<Az_CommessaGetOutModel | null> => {
    const state = history.state;
    if (state && state.id) {
      let request: GenericRequest<Az_CommessaGetInModel> = new GenericRequest<Az_CommessaGetInModel>(Az_CommessaGetInModel);
      request.data.id = state.id;
      return this.azCommessaService.Az_CommessaGet(request).pipe(
        map((res) => {

          //la classe base non gestisce questo tipo di dato DEVO assegnare i valori a manina
          this.startDate = this.stringHelperService.DateString_ddMMyyyy_To_ISOString(res.data.az_Commessa.data);
          this.endDate = this.stringHelperService.DateString_ddMMyyyy_To_ISOString(res.data.az_Commessa.dataA); 
          this.formattedStartDate = res.data.az_Commessa.data; 
          this.formattedEndDate = res.data.az_Commessa.dataA;
          //
          this.selected_Az_SediReparto = [];//res.data.selected_Az_SediReparto;
          this.selected_User = []; // res.data.selected_User;

          if (res.data.az_SubCommessa.length > 0) {
            res.data.tmp_az_SubCommessa = res.data.az_SubCommessa[0].id;
            this.idxCurrCommessa = 0;
          }
          else {
            this.idxCurrCommessa = -1;
          }
            

          return res.data;

        }), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null]; // Restituisce null in caso di errore
        })
      );
    } else {
      return new Observable<Az_CommessaGetOutModel | null>((subscriber) => {

        this.selected_Az_SediReparto = [];
        this.selected_User = [];
        ///
        subscriber.next(new  Az_CommessaGetOutModel());
        subscriber.complete();
      });
    }
  };

  public SaveData(editModel: Az_CommessaGetOutModel): Observable<boolean> {
    let request: GenericRequest<Az_CommessaPutInModel> = new GenericRequest<Az_CommessaPutInModel>(Az_CommessaPutInModel);
    request.data.az_Commessa = editModel.az_Commessa;

    //la classe base non gestisce questo tipo di dato DEVO assegnare i valori a manina
    request.data.az_Commessa.data = this.formattedStartDate;
    request.data.az_Commessa.dataA = this.formattedEndDate;
    

    return this.azCommessaService.Az_CommessaPut(request).pipe(
      map(() => {
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh();
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false];
      })
    );
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

  compareDates(date1: string, date2: string): number {
    // Converte da formato dd/mm/yyyy a Date objects per confronto
    const [day1, month1, year1] = date1.split('/').map(Number);
    const [day2, month2, year2] = date2.split('/').map(Number);

    const d1 = new Date(year1, month1 - 1, day1);
    const d2 = new Date(year2, month2 - 1, day2);

    // Ritorna -1 se d1 < d2, 0 se uguali, 1 se d1 > d2
    return d1 < d2 ? -1 : d1 > d2 ? 1 : 0;
  }

  segmentChanged(event: any) {
    console.log('Segment cambiato:', event.detail.value);
    this.currSection = event.detail.value;
    this.setfabMenuService();
  }
  segmentChanged_sub(event: any) {
    console.log('Segment cambiato:', event.detail.value);
    this.currSection_sub = event.detail.value;
    this.setfabMenuService();
  }

  setfabMenuService() {

    this.fabMenuService.fabMenuItem = [];

    switch (this.currSection) {
      case 'sez_1':
        break;
      case 'sez_2':

        switch (this.currSection_sub) {
          case 'sez_1_sub_1':
            break;
          case 'sez_1_sub_2':

            this.fabMenuService.fabMenuItem = [

              new FabMenuItem('xxx', 'add-circle-outline', () => {
                this.SeletionSediRepartoDialogOpen();
              }),

              //new FabMenuItem('xxx', 'add-circle-outline', () => {
              //  this.SeletionSediRepartoDialogOpen();
              //}),

            ];


            break;
          case 'sez_1_sub_3':

            this.fabMenuService.fabMenuItem = [

              new FabMenuItem('xxx', 'add-circle-outline', () => {
                this.SeletionSediRepartoUserDialogOpen();
              }),

            ];

            

            break;
        }

        break;
    }


  }
  

  /////

  onSubCommessaChange(event: any) {
    const selectedId = event.detail.value;
    this.idxCurrCommessa = this._editModel.az_SubCommessa.findIndex(x => x.id === selectedId);
  }




  public Az_SubCommessaUser_Get(): CheckObjOn_Id_Text[] {

    //let retVal: CheckObjOn_Id_Text[] = [];

    //this.dip_Anagrafica.filter(dip =>
    //  dip.roleCode.includes(RoleCode.User)
    //).forEach(item => {
    //  let appo = { id: item.idAspNetUsers, checked: false };
    //  retVal.push(appo);
    //});

    //return retVal;

    if (this.idxCurrCommessa === -1 || !this._editModel.az_SubCommessa?.[this.idxCurrCommessa]) {
      return [];
    }
    return this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaUser
      .filter(item => item.checked === true);

  }

  Az_SubCommessaUser_IsSelected(itemId: string): boolean {
    
    return this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaUser.find(entry => entry.id === itemId)?.checked ?? false;

  }

  Az_SubCommessaUser_Toggle(itemId: string, event: any) {


    const existingEntry = this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaUser.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.checked = event.detail.checked;
    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaUser.push({ id: itemId, checked: event.detail.checked });
    }

  }

  Az_SubCommessaUser_SetCheck(itemId: string, checked: boolean) {

    if (this.idxCurrCommessa == -1)
      return;


    const existingEntry = this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaUser.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.checked = checked;

    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaUser.push({ id: itemId, checked: checked });
    }

  }

  handleButtonDeleteUserClick = (item: any) => {

    this.Az_SubCommessaUser_SetCheck(item.id, false);

  }

  async SeletionSediRepartoUserDialogOpen() {
    // Crea l'istanza del modal
    const modal = await this.modalCtrl.create({
      component: SeletionSediRepartoUserDialogComponent, // Il componente da usare
      // Passa i dati al modal tramite componentProps
      // Questi dati saranno accessibili tramite @Input() nel DialogExampleComponent
      componentProps: {
        nomeUtente: 'Mario Rossi'
      },
      //cssClass:'nvx-modal'
    });

    // Presenta il modal all'utente
    await modal.present();


    const { data, role } = await modal.onWillDismiss<SeletionSediRepartoUserDialogResult | null>();


    if (role === 'confirm') {

      if (data.userIds) {
        data.userIds.forEach(item => {
          this.Az_SubCommessaUser_SetCheck(item, true);
        });
      }
    } else {}

  }





  public Az_SediReparto_Get(): CheckObjOn_Id_Number[] {
    if (this.idxCurrCommessa === -1 || !this._editModel.az_SubCommessa?.[this.idxCurrCommessa]) {
      return [];
    }
    return this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaSediReparto
      .filter(item => item.checked === true);
  }

  Az_SediReparto_IsSelected(itemId: number): boolean {

    if (this.idxCurrCommessa == -1)
      return false;

    return this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaSediReparto.find(entry => entry.id === itemId)?.checked ?? false;

  }

  Az_SediReparto_Toggle(itemId: number, event: any) {

    if (this.idxCurrCommessa == -1)
      return ;


    const existingEntry = this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaSediReparto.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.checked = event.detail.checked;
    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaSediReparto.push({ id: itemId, checked: event.detail.checked });
    }

  }

  Az_SediReparto_SetCheck(itemId: number, checked:boolean) {

    if (this.idxCurrCommessa == -1)
      return;


    const existingEntry = this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaSediReparto.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.checked = checked;
      
    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaSediReparto.push({ id: itemId, checked: checked });
    }

  }

  handleButtonDeleteRepartoClick = (item: any) => {

    this.Az_SediReparto_SetCheck(item.id,false);

  }

  async SeletionSediRepartoDialogOpen() {
    // Crea l'istanza del modal
    const modal = await this.modalCtrl.create({
      component: SeletionSediRepartoDialogComponent, // Il componente da usare
      // Passa i dati al modal tramite componentProps
      // Questi dati saranno accessibili tramite @Input() nel DialogExampleComponent
      componentProps: {
        nomeUtente: 'Mario Rossi'
      },
      //cssClass:'nvx-modal'
    });

    // Presenta il modal all'utente
    await modal.present();


    const { data, role } = await modal.onWillDismiss<SeletionSediRepartoDialogResult | null>();


    if (role === 'confirm') {
      this.Az_SediReparto_SetCheck(data.idReparto, true);
    } else {
      //this.risultatoDialog = `L'utente ha annullato l'operazione.`;
    }
  }




  public getPar_Attivita(): CheckObjOn_Id_Number[] {

    let retVal: CheckObjOn_Id_Number[] = [];

    this._par_Attivita.filter(rep =>
      rep.id > 0
    ).forEach(item => {
      let appo = { id: item.id, checked: false };
      retVal.push(appo);
    });


    return retVal;
  }

  isSelectedaz_SubCommessaAttivita(itemId: number): boolean {

    if (this.idxCurrCommessa == -1)
      return false;

    return this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaAttivita.find(entry => entry.id === itemId)?.checked ?? false;

  }

  toggleSelectionaz_SubCommessaAttivita(itemId: number, event: any) {

    if (this.idxCurrCommessa == -1)
      return;


    const existingEntry = this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaAttivita.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.checked = event.detail.checked;
    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaAttivita.push({ id: itemId, checked: event.detail.checked });
    }

  }



}
