import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ValidatorFn, AbstractControl, ValidationErrors } from '@angular/forms';
import { NavController, ModalController } from '@ionic/angular';
import { Observable, map, catchError, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { ParExportCauService } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau/par-export-cau.service';
import { Par_ExportCau_Causali_Get_InModel, Par_ExportCau_CausaliModel } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau_Causali/Models/par-export-cau-causali-model';
import { Par_Export_TipoFile, Par_ExportCau_Get_InModel, Par_ExportCau_Put_InModel, Par_ExportCauModel } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau/Models/par-export-cau-model';
import { FabMenuService, FabMenuItem } from '../../../Utility/infrastructure/fab-menu.service';
import { EditParExportCauCausaliComponentComponent } from '../../../shared/components/GestionePresenze/edit-par-export-cau-causali-component/edit-par-export-cau-causali-component.component';
import { ParExportCauCausaliService } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau_Causali/par-export-cau-causali.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

@Component({
  selector: 'app-export-cau-edit-page',
  templateUrl: './export-cau-edit-page.component.html',
  styleUrls: ['./export-cau-edit-page.component.scss'],
  standalone: false
})
export class ExportCauEditPageComponent extends BasePageConfirmCancelComponent<Par_ExportCauModel> implements OnInit {

  public readonly SEGMENT_sez1 = 'sez1';
  public readonly SEGMENT_sez2 = 'sez2';

  public searchText: string = '';
  public par_ExportCau_Causali: Par_ExportCau_CausaliModel[];
  public currSection: string = this.SEGMENT_sez1;
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;
  public TMP_counter: number = 0;
  public par_Export_TipoFile = Par_Export_TipoFile;

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private refresherService: RefresherService,
    private parExportCauService: ParExportCauService,
    private parExportCauCausaliService: ParExportCauCausaliService,
    private modalCtrl: ModalController,
    private collectionDialogService: CollectionDialogService,
    public fabMenuService: FabMenuService,
  ) {
    super(navCtrl, userInterfaceService, fb);

    this.btnEdit = this.userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
    this.currSection = this.SEGMENT_sez1;
    this.btnEdit = this.userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
  }

  override ionViewWillEnter() {
    super.ionViewWillEnter();
    this.setfabMenuService();
  }

  setfabMenuService() {
    this.fabMenuService.fabMenuItem = [];

    if (this.currSection == this.SEGMENT_sez1) {
      // non faccio nulla
    }
    else if (this.currSection == this.SEGMENT_sez2) {
        this.fabMenuService.fabMenuItem = [
          new FabMenuItem('xxx', 'add-circle-outline', () => {

            this.Load_Par_ExportCau_Causali(0).subscribe(res => {
              this.EdiProfiloOrarioDettDialog_Open(res);
            })
          }),
      ];

    }
  }



  get Title(): string {return "Modelli export";}

  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required, Validators.maxLength(10)]],
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      tipoFile: [null, [Validators.required]],
    });
  }

  LoadData = (): Observable<Par_ExportCauModel | null> => {
    const state = history.state;

    if (state) {

      let request = new GenericRequest<Par_ExportCau_Get_InModel>(Par_ExportCau_Get_InModel);
      request.data.id = state.id;

      return this.parExportCauService.Par_ExportCauGet(request).pipe(
        map((res) => {
          this.par_ExportCau_Causali = res.data.par_ExportCau_Causali;
          return res.data.par_ExportCau;
        }
        ),
        catchError((error) => {
          console.error('Errore durante il caricamento dei dati:', error);
          return [null];
        })
      );
    }
    else {
      return null;
    }

  };

  SaveData = (editModel: Par_ExportCauModel): Observable<boolean> => {

    let request: GenericRequest<Par_ExportCau_Put_InModel> = new GenericRequest<Par_ExportCau_Put_InModel>(Par_ExportCau_Put_InModel);
    request.data.par_ExportCau = editModel;
    request.data.par_ExportCau_Causali = this.par_ExportCau_Causali;

    return this.parExportCauService.Par_ExportCauPut(request).pipe(
      map(() => {
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh();
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante il salvataggio:', error);
        return [false];
      })
    );
  };

  segmentChanged(event: any) {
    console.log('Segment cambiato:', event.detail.value);
    this.currSection = event.detail.value;
    this.setfabMenuService();
  }

  public get_par_ExportCau_Causali(): Par_ExportCau_CausaliModel[] {
    if (!this.par_ExportCau_Causali) {
      return [];
    }

    //let retVal = this.par_ExportCau_Causali.sort((a, b) => a.numCoppia - b.numCoppia);

    let retVal = this.par_ExportCau_Causali;
    

    return retVal;
  }

  handleButtonEditClick = (item: Par_ExportCau_CausaliModel) => {

    this.Load_Par_ExportCau_Causali(item.id).subscribe(res => {
      this.EdiProfiloOrarioDettDialog_Open(res);
    })

  }

  handleButtonDeleteClick = async (item: Par_ExportCau_CausaliModel) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione della causale in export');
    if (result) {

      var idx = this.par_ExportCau_Causali.findIndex(p => p.id === item.id);
      if (idx > -1) {
        this.par_ExportCau_Causali.splice(idx, 1);
      }
    }

  }

  /* Legge i dati locali o inizilizza con il server */
  Load_Par_ExportCau_Causali = (id: number): Observable<Par_ExportCau_CausaliModel | null> => {
    const state = history.state;

    if (id==0) {  // se nuovo, faccio inizializazre dal server

      let request: GenericRequest<Par_ExportCau_Causali_Get_InModel> = new GenericRequest<Par_ExportCau_Causali_Get_InModel>(Par_ExportCau_Causali_Get_InModel);
      request.data.id = id;

      return this.parExportCauCausaliService.Par_ExportCau_Causali_Get(request).pipe(
        map((res) => {
          
          return res.data.par_ExportCau_Causali;
        }
        ),
        catchError((error) => {
          console.error('Errore durante il caricamento dei dati:', error);
          return [null];
        })
      );
    }
    else {
      var idx = this.par_ExportCau_Causali.findIndex(p => p.id === id);
      if (idx > -1) {
        return of(this.par_ExportCau_Causali[idx]);
      }
      else {
        return of(null);
      }
    }

  };



  async EdiProfiloOrarioDettDialog_Open(par_ExportCau_Causali: Par_ExportCau_CausaliModel) {

      const modal = await this.modalCtrl.create({
        component: EditParExportCauCausaliComponentComponent,
        componentProps: {
          par_ExportCau_Causali: par_ExportCau_Causali
        },
      });

      await modal.present();

      const { data, role } = await modal.onWillDismiss<Par_ExportCau_CausaliModel | null>();

      if (role === 'confirm' && data) {

        const index = this.par_ExportCau_Causali.findIndex(p => p.id === data.id);

        if (index > -1) {
          this.par_ExportCau_Causali[index] = data;
        } else {
          this.par_ExportCau_Causali.push(data);
        }

      }

  }




}


const matchData: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  //const password = control.get('pw')?.value;
  //const confirmPassword = control.get('confirmPassword')?.value;

  //return password === confirmPassword ? null : { notMatching: true };

  return null;
};
