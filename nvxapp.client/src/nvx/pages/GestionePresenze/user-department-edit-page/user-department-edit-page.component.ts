import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ModalController, NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { ParameterService } from '../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { RolesModel } from '../../../ClientServer-Service/Infrastructure/Parameter/Models/roles-model';


import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { Dip_Anagrafica4EditModel, Dip_Anagrafica_Get_InModel, Dip_Anagrafica_Put_InModel } from '../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model';
import { DipAnagraficaService } from '../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/dip-anagrafica.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { EditDipProfiloOrarioDialogComponent } from '../../../shared/components/GestionePresenze/edit-dip-profilo-orario-dialog/edit-dip-profilo-orario-dialog.component';
import { Dip_ProfiloOrarioModel } from '../../../ClientServer-Service/GestionePresenze/Dip_ProfiloOrario/Models/dip-profilo-orario-model';
import { DbUtilService } from '../../../Utility/infrastructure/db-util.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { Dip_RapportoLavoroModel } from '../../../ClientServer-Service/GestionePresenze/Dip_RapportoLavoro/Models/dip-rapporto-lavoro-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';

@Component({
  selector: 'app-user-department-edit-page',
  templateUrl: './user-department-edit-page.component.html',
  styleUrls: ['./user-department-edit-page.component.scss'],
  standalone: false
})
export class UserDepartmentEditPageComponent extends BasePageConfirmCancelComponent<Dip_Anagrafica4EditModel> {

  public readonly SEGMENT_MAIN_ANAGRAFICA = 'MAIN_ANA_0';
  public readonly SEGMENT_MAIN_VARIE = 'MAIN_VAR_0';
  public readonly SEGMENT_MAIN_RAPP_LAV = 'MAIN_RAPP_LAV_';

  public readonly SEGMENT_DIPRAPP_PROF_HH = 'DIPRAPP_DETT_PROF_HH';
  public readonly SEGMENT_DIPRAPP_VARIE = 'DIPRAPP_DETT_VARIE';


  public btnEdit_Dip_ProfiloOrario: ButtonItem;
  public btnDelete_Dip_ProfiloOrario: ButtonItem;


  public currSection_segment_main: string = this.SEGMENT_MAIN_ANAGRAFICA;
  //public currSection_segment_dip_rapp: string = this.SEGMENT_DIPRAPP_PROF_HH;
  public currSection_segment_dip_rapp: { [key: number]: string } = {};
  
  public searchText: string = '';
  modifiedDescription: string | null = null;

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parameterService: ParameterService,
    private stringHelperService: StringHelperService,
    private modalCtrl: ModalController,
    public fabMenuService: FabMenuService,
    private dipAnagraficaService: DipAnagraficaService,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private dbUtilService: DbUtilService,
    private collectionDialogService: CollectionDialogService,
    private cdr: ChangeDetectorRef) {

    super(navCtrl, userInterfaceService, fb);


    this.btnEdit_Dip_ProfiloOrario = this.userInterfaceService.Btn_Modifica;
    this.btnEdit_Dip_ProfiloOrario.event = this.handleButton_Dip_ProfiloOrario_EditClick;

    this.btnDelete_Dip_ProfiloOrario = userInterfaceService.Btn_Cancella;
    this.btnDelete_Dip_ProfiloOrario.event = this.handleButton_Dip_ProfiloOrario_DeleteClick;

  }






  override ionViewWillEnter() {
    super.ionViewWillEnter();
    this.setfabMenuService();
  }


  setfabMenuService() {
    this.fabMenuService.fabMenuItem = [];


    
    

    if (this.currSection_segment_main == this.SEGMENT_MAIN_ANAGRAFICA) {
      // non faccio nulla
    } else if (this.currSection_segment_main == this.SEGMENT_MAIN_VARIE) {
      // non faccio nulla
    } else if (this.currSection_segment_main.startsWith(this.SEGMENT_MAIN_RAPP_LAV)) {

      const curr_RappLav = this.getCurr_RappLav_Id();
      const currKey = this.currSection_segment_dip_rapp[curr_RappLav];
      

      if (currKey.includes(this.SEGMENT_DIPRAPP_VARIE)) {

      }else  if (currKey.includes(this.SEGMENT_DIPRAPP_PROF_HH)) {

        

        this.fabMenuService.fabMenuItem = [
            new FabMenuItem('xxx', 'add-circle-outline', () => {
              this.EditDipProfiloOrarioDialog_Open( 0 );
            }),
          ];
      }

    }

  }


  segment_main_segmentChanged(event: any) {
    console.log('Segment principale cambiato:', event.detail.value);
    this.currSection_segment_main = event.detail.value;
    
    const rappLavId = this.getCurr_RappLav_Id();
    if (rappLavId !== -1 && !this.currSection_segment_dip_rapp[rappLavId]) {
      this.currSection_segment_dip_rapp[rappLavId] = this.SEGMENT_MAIN_RAPP_LAV + rappLavId + '_' + this.SEGMENT_DIPRAPP_PROF_HH;
    }

    this.setfabMenuService();
  }

  segment_dip_rapp_segmentChanged(event: any, rappLavId: number) {
    console.log(`Sotto-segment per Rapporto ID ${rappLavId} cambiato:`, event.detail.value);
    this.currSection_segment_dip_rapp[rappLavId] = event.detail.value;

    this.setfabMenuService();
  }


  get Title(): string { return "User Department Edit"; }
  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      roles: [[], this.minArrayLength(1)],
      cognome: [null, [Validators.required, Validators.maxLength(50)]],
      nome: [null, [Validators.required, Validators.maxLength(50)]],
    });
  }

  minArrayLength(min: number) {
    return (control: AbstractControl): ValidationErrors | null => {
      return control.value && control.value.length >= min ? null : { minArrayLength: true };
    };
  }

  LoadData = (): Observable<Dip_Anagrafica4EditModel | null> => {

    this._editModel = null;
    this.cdr.detectChanges();

    const state = history.state;
    if (state && state.id) {
      let request: GenericRequest<Dip_Anagrafica_Get_InModel> = new GenericRequest<Dip_Anagrafica_Get_InModel>(Dip_Anagrafica_Get_InModel);
      request.data.id = state.id;
      return this.dipAnagraficaService.Dip_AnagraficaGet(request).pipe(
        map((res) => res.data.dip_Anagrafica),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null];
        })
      );
    } else {
      return new Observable<Dip_Anagrafica4EditModel | null>((subscriber) => {

        this._editForm.addControl('mail', this.fb.control(null, [Validators.required, Validators.email]));
        this._editForm.addControl('pw', this.fb.control(null, [Validators.required]));
        this._editForm.addControl('confirmPassword', this.fb.control(null, [Validators.required]));
        this._editForm.setValidators(matchPasswords);
        this._editForm.updateValueAndValidity();

        let dip_Anagrafica4Edit = new Dip_Anagrafica4EditModel();
        dip_Anagrafica4Edit.dip_RapportoLavoro = [];
        dip_Anagrafica4Edit.id = this.dbUtilService.GenerateCounterKey();

        let Dip_RapportoLavoro = new Dip_RapportoLavoroModel();
        Dip_RapportoLavoro.idDip_Anagrafica = dip_Anagrafica4Edit.id;
        Dip_RapportoLavoro.id = this.dbUtilService.GenerateCounterKey();
        dip_Anagrafica4Edit.dip_RapportoLavoro.push(Dip_RapportoLavoro);
        

        if (this.sharedParameterGestionePresenzeService.Par_ProfiloOrario.length > 0) {
          let dip_ProfiloOrarioModel: Dip_ProfiloOrarioModel = new Dip_ProfiloOrarioModel();

          dip_ProfiloOrarioModel.id = this.dbUtilService.GenerateCounterKey();
          dip_ProfiloOrarioModel.idDip_RapportoLavoro = Dip_RapportoLavoro.id;
          dip_ProfiloOrarioModel.numGiornoPartenzaCiclo = 1;
          dip_ProfiloOrarioModel.idPar_ProfiloOrario = this.sharedParameterGestionePresenzeService.Par_ProfiloOrario[0].id;

          dip_Anagrafica4Edit.dip_ProfiloOrario.push(dip_ProfiloOrarioModel);

        }


        subscriber.next(dip_Anagrafica4Edit);
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Dip_Anagrafica4EditModel): Observable<boolean> => {
    let request: GenericRequest<Dip_Anagrafica_Put_InModel> =
      new GenericRequest<Dip_Anagrafica_Put_InModel>(Dip_Anagrafica_Put_InModel);
    request.data.dip_Anagrafica = editModel;
    request.data.id = editModel.idAspNetUsers;
    return this.dipAnagraficaService.Dip_AnagraficaPut(request).pipe(
      map(() => true),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false];
      })
    );
  };

  getRoler(): RolesModel[] {
    if (this._editModel && this._editModel.idUserCompany == 0) {
      return this.parameterService.Roles.filter(role => role.code == RoleCode.User);
    }
    return this.parameterService.Roles;
  }

  getRoles(): string[] {
    if (this._editModel) {
      const rolesPowerAdmin = this.parameterService.Roles.find(role => role.code == RoleCode.CompanyPowerAdmin);
      if (this._editModel.roles.includes(rolesPowerAdmin.name)) {
        return this.parameterService.Roles.filter(role => role.code == RoleCode.CompanyPowerAdmin )
          .map(role => role.name); 
      } else {
        return this.parameterService.Roles.filter(role => role.code == RoleCode.User ||
          role.code == RoleCode.CompanyAdmin)
          .map(role => role.name); 
      }
    }
    return [];
  }



  getAll_Dip_ProfiloOrario() {

    !this._editModel

    if (!this._editModel) return [];

    const curr_RappLav = this.getCurr_RappLav_Id();
    return this._editModel.dip_ProfiloOrario.filter(x => x.idDip_RapportoLavoro == curr_RappLav);
    
  }


    
  handleButton_Dip_ProfiloOrario_EditClick = (item: Dip_ProfiloOrarioModel) => {
    this.EditDipProfiloOrarioDialog_Open(item.id);
  }

  handleButton_Dip_ProfiloOrario_DeleteClick = async (item: any) => {

    const index = this._editModel.dip_ProfiloOrario.findIndex(p => p.id === item.id);

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione del profilo orario');
    if (result) {

      if (index > -1) {
        this._editModel.dip_ProfiloOrario.splice(index, 1);
      }  

    }

  }

  async EditDipProfiloOrarioDialog_Open(id: number) {

    const curr_RappLav = this.getCurr_RappLav_Id();
    const currKey = this.currSection_segment_dip_rapp[curr_RappLav];

    let dip_ProfiloOrario: Dip_ProfiloOrarioModel = null;

    if (id === 0) { //new
      dip_ProfiloOrario = new Dip_ProfiloOrarioModel();
      dip_ProfiloOrario.idDip_RapportoLavoro = curr_RappLav;
      dip_ProfiloOrario.id = this.dbUtilService.GenerateCounterKey();
      dip_ProfiloOrario.numGiornoPartenzaCiclo = 1;
      dip_ProfiloOrario.dal = new Date().toISOString();;
      dip_ProfiloOrario.al = new Date().toISOString();;
    }
    else {
      dip_ProfiloOrario = this._editModel.dip_ProfiloOrario.find(p => p.id === id);
    }



    const modal = await this.modalCtrl.create({
      component: EditDipProfiloOrarioDialogComponent,
      componentProps: {
        dip_ProfiloOrario: dip_ProfiloOrario
      },
    });

    await modal.present();

    const { data, role } = await modal.onWillDismiss<Dip_ProfiloOrarioModel | null>();

    if (role === 'confirm' && data) {

      const index = this._editModel.dip_ProfiloOrario.findIndex(p => p.id === data.id);

      if (index > -1) {
        this._editModel.dip_ProfiloOrario[index] = data;
      } else {
        this._editModel.dip_ProfiloOrario.push(data);
      }

    }
  }






  getCurr_RappLav_Id(): number {
    if (this.currSection_segment_main && this.currSection_segment_main.startsWith(this.SEGMENT_MAIN_RAPP_LAV)) {
      const idString = this.currSection_segment_main.replace(this.SEGMENT_MAIN_RAPP_LAV, '');
      const id = parseInt(idString, 10);
      return !isNaN(id) ? id : 0;
    }
    return 0;
  }

  getCurr_RappLav_Idx(): number {

    let currId: number = this.getCurr_RappLav_Id();

    if (currId != 0) {
      return this._editModel.dip_RapportoLavoro.findIndex(x => x.id == currId);
    }

    return -1;

  }

  isAdmin(item: any) {

    return false;
  }


}

const matchPasswords: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('pw')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;
  return password === confirmPassword ? null : { notMatching: true };
};
