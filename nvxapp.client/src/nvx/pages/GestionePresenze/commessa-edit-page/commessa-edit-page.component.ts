import { Component } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ModalController, NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { Az_CommessaGetInModel, Az_CommessaGetOutModel, Az_CommessaPutInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Commessa/Models/az-commessa-model';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { Az_ClienteModel } from '../../../ClientServer-Service/GestionePresenze/Az_Cliente/Models/az-cliente-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { AzCommessaService } from '../../../ClientServer-Service/GestionePresenze/Az_Commessa/az-commessa.service';
import { CheckObjOn_Id_Number, CheckObjOn_Id_Text } from '../../../ClientServer-Service/ModelsBase/check-obj';
import { Az_SediRepartoModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { Par_AttivitaModel } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/Models/par-attivita-model';
import { FabMenuService, FabMenuItem } from '../../../Utility/infrastructure/fab-menu.service';
import { SeletionSediRepartoDialogComponent, SeletionSediRepartoDialogResult } from '../../../shared/components/GestionePresenze/seletion-sedi-reparto-dialog/seletion-sedi-reparto-dialog.component';
import { SeletionSediRepartoUserDialogComponent, SeletionSediRepartoUserDialogResult } from '../../../shared/components/GestionePresenze/seletion-sedi-reparto-user-dialog/seletion-sedi-reparto-user-dialog.component';
import { SeletionParAttivitaDialogComponent, SeletionParAttivitaDialogResult } from '../../../shared/components/GestionePresenze/seletion-par-attivita-dialog/seletion-par-attivita-dialog.component';
import { Az_SubCommessaUser4EditModel } from '../../../ClientServer-Service/GestionePresenze/Az_SubCommessaUser/Models/az-subcommessa-user-model';
import { Az_SubCommessaAttivita4EditModel } from '../../../ClientServer-Service/GestionePresenze/Az_SubCommessaAttivita/Models/az-subcommessa-attivita-model';
import { Az_SubCommessaSediRepartoModel4EditModel } from '../../../ClientServer-Service/GestionePresenze/Az_SubCommessaSediReparto/Models/az-subcommessa-sedi-reparto-model';

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
  public currSection_sub: string = "sez_1_sub_4";
  public idxCurrCommessa: number = -1;

  public btnDeleteReparto: ButtonItem;
  public btnDeleteUser: ButtonItem;
  public btnDeleteAttivita: ButtonItem;

  //date x il backend
  public formattedStartDate: string | null = null;
  public formattedEndDate: string | null = null;

  // --- MODIFICA: Tipizzate come string | null e inizializzate a null per evitare l'errore di parsing di ion-datetime ---
  public startDate: string | null = null;
  public endDate: string | null = null;

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    protected override fb: FormBuilder,
    public fabMenuService: FabMenuService,
    private stringHelperService: StringHelperService,
    private azCommessaService: AzCommessaService,
    private refresherService: RefresherService,
    private modalCtrl: ModalController) {
    super(navCtrl, userInterfaceService, fb);

    this.btnDeleteReparto = userInterfaceService.Btn_Cancella;
    this.btnDeleteReparto.event = this.Az_SediReparto_HandleButtonDelete;

    this.btnDeleteUser = userInterfaceService.Btn_Cancella;
    this.btnDeleteUser.event = this.Az_SubCommessaUser_HandleButtonDelete;

    this.btnDeleteAttivita = userInterfaceService.Btn_Cancella;
    this.btnDeleteAttivita.event = this.Az_SubCommessaAttivita_HandleButtonDelete;

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
          if (res.data.az_SubCommessa.length > 0) {
            res.data.tmp_az_SubCommessa = res.data.az_SubCommessa[0].id;
            this.idxCurrCommessa = 0;

            this._editModel = res.data;

            this.setSubCommessaDates(this.idxCurrCommessa);
          } else {
            this.idxCurrCommessa = -1;
            // --- MODIFICA: Assegna null invece di stringa vuota
            this.startDate = null;
            this.endDate = null;
            this.formattedStartDate = null;
            this.formattedEndDate = null;
          }
          return res.data;
        }),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null];
        })
      );
    } else {
      return new Observable<Az_CommessaGetOutModel | null>((subscriber) => {
        subscriber.next(new Az_CommessaGetOutModel());
        subscriber.complete();
      });
    }
  };

  public SaveData(editModel: Az_CommessaGetOutModel): Observable<boolean> {
    let request: GenericRequest<Az_CommessaPutInModel> = new GenericRequest<Az_CommessaPutInModel>(Az_CommessaPutInModel);
    request.data.az_Commessa = editModel.az_Commessa;
    request.data.az_SubCommessa = editModel.az_SubCommessa;
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
    const selectedISODate = event.detail.value;
    if (!selectedISODate) return;

    this.startDate = selectedISODate; // Mantiene il formato ISO per il controllo
    const selectedDate = new Date(selectedISODate);
    this.formattedStartDate = this.stringHelperService.Date_To_S_ddmmyyyy(selectedDate);

    if (this.idxCurrCommessa !== -1) {
      this._editModel.az_SubCommessa[this.idxCurrCommessa].data = this.formattedStartDate;
    }
    if (!this.endDate || this.compareDates(this.formattedStartDate, this.formattedEndDate) > 0) {
      this.endDate = this.startDate;
      this.formattedEndDate = this.formattedStartDate;
      if (this.idxCurrCommessa !== -1) {
        this._editModel.az_SubCommessa[this.idxCurrCommessa].dataA = this.formattedEndDate;
      }
    }
  }

  updateEndDate(event: any) {
    const selectedISODate = event.detail.value;
    if (!selectedISODate) return;

    this.endDate = selectedISODate; // Mantiene il formato ISO per il controllo
    const selectedDate = new Date(selectedISODate);
    this.formattedEndDate = this.stringHelperService.Date_To_S_ddmmyyyy(selectedDate);

    if (this.idxCurrCommessa !== -1) {
      this._editModel.az_SubCommessa[this.idxCurrCommessa].dataA = this.formattedEndDate;
    }
    if (!this.startDate || this.compareDates(this.formattedEndDate, this.formattedStartDate) < 0) {
      this.startDate = this.endDate;
      this.formattedStartDate = this.formattedEndDate;
      if (this.idxCurrCommessa !== -1) {
        this._editModel.az_SubCommessa[this.idxCurrCommessa].data = this.formattedStartDate;
      }
    }
  }

  compareDates(date1: string, date2: string): number {
    // Aggiunto un controllo per date nulle o non valide
    if (!date1 || !date2) return 0;
    const [day1, month1, year1] = date1.split('/').map(Number);
    const [day2, month2, year2] = date2.split('/').map(Number);

    const d1 = new Date(year1, month1 - 1, day1);
    const d2 = new Date(year2, month2 - 1, day2);

    return d1 < d2 ? -1 : d1 > d2 ? 1 : 0;
  }

  onSubCommessaChange(event: any) {
    const selectedId = event.detail.value;
    this.idxCurrCommessa = this._editModel.az_SubCommessa.findIndex(x => x.id === selectedId);
    this.setSubCommessaDates(this.idxCurrCommessa);
    this.setfabMenuService();
  }

  

  updateSubCommessaDescrizione(event: any) {
    const value = (event.target as HTMLInputElement).value;
    if (this.idxCurrCommessa !== -1) {
      this._editModel.az_SubCommessa[this.idxCurrCommessa].descrizione = value;
      const ctrl = this._editForm.get('tmp_az_SubCommessa');
      if (ctrl) {
        const currentId = this._editModel.az_SubCommessa[this.idxCurrCommessa].id;
        // Forza il cambio valore per triggerare il change detection
        ctrl.setValue(null, { emitEvent: false });
        ctrl.setValue(currentId, { emitEvent: true });
      }
    }
  }

  setSubCommessaDates(idx: number) {
    if (idx === -1 || !this._editModel?.az_SubCommessa?.[idx]) {
      // --- MODIFICA: Assegna null invece di stringa vuota
      this.startDate = null;
      this.endDate = null;
      this.formattedStartDate = null;
      this.formattedEndDate = null;
      return;
    }
    const sub = this._editModel.az_SubCommessa[idx];
    // Assicurarsi che la funzione di conversione restituisca null se la data in input è nulla o vuota
    this.startDate = this.stringHelperService.DateString_ddMMyyyy_To_ISOString(sub.data);
    this.endDate = this.stringHelperService.DateString_ddMMyyyy_To_ISOString(sub.dataA);
    this.formattedStartDate = sub.data || null;
    this.formattedEndDate = sub.dataA || null;
  }

  segmentChanged(event: any) {
    this.currSection = event.detail.value;
    this.setfabMenuService();
  }

  segmentChanged_sub(event: any) {
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

          case 'sez_1_sub_4':
            this.fabMenuService.fabMenuItem = [
              new FabMenuItem('xxx', 'add-circle-outline', () => {
                this.Az_SubCommessaAdd();
              }),
            ];
            

            if (this.idxCurrCommessa !== -1) {
              if (!this._editModel.az_SubCommessa[this.idxCurrCommessa].default) {
                this.fabMenuService.fabMenuItem.push(
                  new FabMenuItem('xxx', 'remove-circle-outline', () => {
                    this.Az_SubCommessaDelete();
                  }),
                );
              }
            }


            break;


          case 'sez_1_sub_1':
            this.fabMenuService.fabMenuItem = [
              new FabMenuItem('xxx', 'add-circle-outline', () => {
                this.Az_SubCommessaAttivita_DialogOpen();
              }),
            ];
            break;
          case 'sez_1_sub_2':
            this.fabMenuService.fabMenuItem = [
              new FabMenuItem('xxx', 'add-circle-outline', () => {
                this.Az_SediReparto_DialogOpen();
              }),
            ];
            break;
          case 'sez_1_sub_3':
            this.fabMenuService.fabMenuItem = [
              new FabMenuItem('xxx', 'add-circle-outline', () => {
                this.Az_SubCommessaUser_DialogOpen();
              }),
            ];
            break;
        }
        break;
    }
  }

  /*SCHEDA USER*/
  public Az_SubCommessaUser_Get(): Az_SubCommessaUser4EditModel[] {
    if (this.idxCurrCommessa === -1 || !this._editModel.az_SubCommessa?.[this.idxCurrCommessa]) {
      return [];
    }
    return this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaUser
      .filter(item => item.checked === true);
  }

  Az_SubCommessaUser_IsSelected(idAspNetUsers: string): boolean {
    return this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaUser.find(entry => entry.idAspNetUsers === idAspNetUsers)?.checked ?? false;
  }

  Az_SubCommessaUser_Toggle(idAspNetUsers: string, event: any) {
    const existingEntry = this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaUser.find(entry => entry.idAspNetUsers === idAspNetUsers);

    if (existingEntry) {
      existingEntry.checked = event.detail.checked;
    } else {
      this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaUser.push({
        id: 0,
        idAz_SubCommessa: 0,
        idAspNetUsers: idAspNetUsers,
        checked: event.detail.checked
      });
    }
  }

  Az_SubCommessaUser_SetCheck(idAspNetUsers: string, checked: boolean) {
    if (this.idxCurrCommessa == -1) return;

    const existingEntry = this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaUser.find(entry => entry.idAspNetUsers === idAspNetUsers);

    if (existingEntry) {
      existingEntry.checked = checked;
    } else {
      this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaUser.push({
        id: 0,
        idAz_SubCommessa: 0,
        idAspNetUsers: idAspNetUsers,
        checked: checked
      });
    }
  }

  Az_SubCommessaUser_HandleButtonDelete = (item: any) => {
    this.Az_SubCommessaUser_SetCheck(item.idAspNetUsers, false);
  }

  async Az_SubCommessaUser_DialogOpen() {
    const modal = await this.modalCtrl.create({
      component: SeletionSediRepartoUserDialogComponent,
      componentProps: {
        nomeUtente: 'Mario Rossi'
      },
    });

    await modal.present();

    const { data, role } = await modal.onWillDismiss<SeletionSediRepartoUserDialogResult | null>();

    if (role === 'confirm' && data) {
      if (data.userIds) {
        data.userIds.forEach(item => {
          this.Az_SubCommessaUser_SetCheck(item, true);
        });
      }
    }
  }

  /*SCHEDA REPARTI*/
  public Az_SediReparto_Get(): Az_SubCommessaSediRepartoModel4EditModel[] {
    if (this.idxCurrCommessa === -1 || !this._editModel.az_SubCommessa?.[this.idxCurrCommessa]) {
      return [];
    }
    return this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaSediReparto
      .filter(item => item.checked === true);
  }

  Az_SediReparto_IsSelected(idAz_SediReparto: number): boolean {
    if (this.idxCurrCommessa == -1) return false;

    return this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaSediReparto.find(entry => entry.idAz_SediReparto === idAz_SediReparto)?.checked ?? false;
  }

  Az_SediReparto_Toggle(itemId: number, event: any) {
    if (this.idxCurrCommessa == -1) return;

    const existingEntry = this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaSediReparto.find(entry => entry.idAz_SediReparto === itemId);

    if (existingEntry) {
      existingEntry.checked = event.detail.checked;
    } else {
      this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaSediReparto.push({
        id: 0,
        checked: event.detail.checked,
        idAz_SubCommessa: 0,
        idAz_SediReparto: itemId
      });
    }
  }

  Az_SediReparto_SetCheck(itemId: number, checked: boolean) {
    if (this.idxCurrCommessa == -1) return;

    const existingEntry = this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaSediReparto.find(entry => entry.idAz_SediReparto === itemId);

    if (existingEntry) {
      existingEntry.checked = checked;
    } else {
      this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaSediReparto.push({
        id: 0,
        checked: checked,
        idAz_SediReparto: itemId,
        idAz_SubCommessa: 0
      });
    }
  }

  Az_SediReparto_HandleButtonDelete = (item: any) => {
    // --- MODIFICA: L'item passato è l'oggetto stesso, quindi l'id da usare è item.idAz_SediReparto
    this.Az_SediReparto_SetCheck(item.idAz_SediReparto, false);
  }

  async Az_SediReparto_DialogOpen() {
    const modal = await this.modalCtrl.create({
      component: SeletionSediRepartoDialogComponent,
      componentProps: {
        nomeUtente: 'Mario Rossi'
      },
    });

    await modal.present();

    const { data, role } = await modal.onWillDismiss<SeletionSediRepartoDialogResult | null>();

    if (role === 'confirm' && data) {
      this.Az_SediReparto_SetCheck(data.idReparto, true);
    }
  }

  /*SCHEDA ATTIVITA*/
  public Az_SubCommessaAttivita_Get(): Az_SubCommessaAttivita4EditModel[] {
    if (this.idxCurrCommessa === -1 || !this._editModel.az_SubCommessa?.[this.idxCurrCommessa]) {
      return [];
    }
    return this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaAttivita
      .filter(item => item.checked === true);
  }

  Az_SubCommessaAttivita_IsSelected(idPar_Attivita: number): boolean {
    if (this.idxCurrCommessa == -1) return false;

    return this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaAttivita.find(entry => entry.idPar_Attivita === idPar_Attivita)?.checked ?? false;
  }

  Az_SubCommessaAttivita_Toggle(idPar_Attivita: number, event: any) {
    if (this.idxCurrCommessa == -1) return;

    const existingEntry = this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaAttivita.find(entry => entry.idPar_Attivita === idPar_Attivita);

    if (existingEntry) {
      existingEntry.checked = event.detail.checked;
    } else {
      this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaAttivita.push({
        id: 0,
        idAz_SubCommessa: 0,
        idPar_Attivita: idPar_Attivita,
        checked: event.detail.checked,
        default: false
      });
    }
  }

  Az_SubCommessaAttivita_SetCheck(idPar_Attivita: number, checked: boolean) {
    if (this.idxCurrCommessa == -1) return;

    const existingEntry = this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaAttivita.find(entry => entry.idPar_Attivita === idPar_Attivita);

    if (existingEntry) {
      existingEntry.checked = checked;
    } else {
      this._editModel.az_SubCommessa[this.idxCurrCommessa].az_SubCommessaAttivita.push({
        idAz_SubCommessa: 0,
        idPar_Attivita: idPar_Attivita,
        id: 0,
        checked: checked,
        default: false
      });
    }
  }

  Az_SubCommessaAttivita_HandleButtonDelete = (item: any) => {
    this.Az_SubCommessaAttivita_SetCheck(item.idPar_Attivita, false);
  }

  async Az_SubCommessaAttivita_DialogOpen() {
    const modal = await this.modalCtrl.create({
      component: SeletionParAttivitaDialogComponent,
      componentProps: {
        nomeUtente: 'Mario Rossi'
      },
    });

    await modal.present();

    const { data, role } = await modal.onWillDismiss<SeletionParAttivitaDialogResult | null>();

    if (role === 'confirm' && data) {
      if (data.id) {
        data.id.forEach(item => {
          this.Az_SubCommessaAttivita_SetCheck(item, true);
        });
      }
    }
  }

  async Az_SubCommessaAdd() {
    // Crea un nuovo id temporaneo negativo (per evitare collisioni con quelli del backend)
    const minId = Math.min(0, ...this._editModel.az_SubCommessa.map(x => x.id || 0));
    const newId = minId - 1;


    const now = new Date();
    

    // Crea la nuova subcommessa con valori di default
    const nuovaSub: any = {
      id: newId,
      descrizione: 'Nuova sub commessa',
      data: this.stringHelperService.Date_To_S_ddmmyyyy(now),
      dataA: this.stringHelperService.Date_To_S_ddmmyyyy(new Date(now.getFullYear(), 11, 31)),
      default: false,
      az_SubCommessaUser: [],
      az_SubCommessaAttivita: [],
      az_SubCommessaSediReparto: []
    };

    // Aggiungi la nuova subcommessa all'array
    this._editModel.az_SubCommessa.push(nuovaSub);

    // Seleziona la nuova subcommessa
    this.idxCurrCommessa = this._editModel.az_SubCommessa.length - 1;
    this._editForm.get('tmp_az_SubCommessa')?.setValue(nuovaSub.id);

    // Aggiorna le date mostrate
    this.setSubCommessaDates(this.idxCurrCommessa);
  }

  async Az_SubCommessaDelete() {
    if (this.idxCurrCommessa !== -1) {
      if (!this._editModel.az_SubCommessa[this.idxCurrCommessa].default) {
        // Rimuovi l'elemento dall'array
        this._editModel.az_SubCommessa.splice(this.idxCurrCommessa, 1);

        // Aggiorna l'indice corrente
        if (this._editModel.az_SubCommessa.length > 0) {
          this.idxCurrCommessa = 0;
          // Aggiorna il controllo del form con il nuovo id selezionato
          this._editForm.get('tmp_az_SubCommessa')?.setValue(this._editModel.az_SubCommessa[0].id);
          this.setSubCommessaDates(this.idxCurrCommessa);
        } else {
          this.idxCurrCommessa = -1;
          this._editForm.get('tmp_az_SubCommessa')?.setValue(null);
          this.setSubCommessaDates(-1);
        }
      }
    }
  }


}
