import { ChangeDetectorRef, Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { ModalController, NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError, of } from 'rxjs';
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
import { DestinazioneEccedenze, Dip_RapportoLavoroModel, TipoContratto } from '../../../ClientServer-Service/GestionePresenze/Dip_RapportoLavoro/Models/dip-rapporto-lavoro-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { ContatoriService } from '../../../ClientServer-Service/GestionePresenze/Contatori/contatori.service';
import { Contatori_Anno_MeseResult, Contatori_Anno_InModel, Contatori_Riporto_Model, Contatori_Maturazione_Model } from '../../../ClientServer-Service/GestionePresenze/Contatori/Models/contatori-model';
import { Par_GiustificativiModel, TipoContatore } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { Dip_Rapporto_Giustificativi_MaturazioneModel } from '../../../ClientServer-Service/GestionePresenze/Dip_Rapporto_Giustificativi_Maturazione/Models/dip-rapporto-giustificativi-maturazione-model';
import { Dip_CompetenzaModel } from '../../../ClientServer-Service/GestionePresenze/Dip_Competenza/Models/dip-competenza-model';
import { SeletionParCompetenzaDialogComponent, SeletionParCompetenzaDialogResult } from '../../../shared/components/GestionePresenze/seletion-par-competenza-dialog/seletion-par-competenza-dialog.component';
import { Subject } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { TranslocoService } from '@jsverse/transloco';
import { LanguageService } from '../../../Utility/infrastructure/language.service';
import { Dip_RapportoAutovettureModel } from '../../../ClientServer-Service/GestionePresenze/Dip_RapportoAutovetture/Models/dip-rapporto-autovetture-model';
import { EditDipRapportoAutovettureDialogComponent } from '../../../shared/components/GestionePresenze/edit-dip-rapporto-autovetture-dialog/edit-dip-rapporto-autovetture-dialog.component';
import { FunzionalitaGestionePresenzeService } from '../../../Utility/GestionePresenze/funzionalita-gestione-presenze.service';

@Component({
  selector: 'app-user-department-edit-page',
  templateUrl: './user-department-edit-page.component.html',
  styleUrls: ['./user-department-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class UserDepartmentEditPageComponent extends BasePageConfirmCancelComponent<Dip_Anagrafica4EditModel> implements OnDestroy {

  public readonly SEGMENT_MAIN_ANAGRAFICA = 'MAIN_ANA_0';
  public readonly SEGMENT_MAIN_VARIE = 'MAIN_VAR_0';
  public readonly SEGMENT_MAIN_COMPETENZE = 'MAIN_COMP_0';
  public readonly SEGMENT_MAIN_RAPP_LAV = 'MAIN_RAPP_LAV_';

  public readonly SEGMENT_DIPRAPP_GENERALE  = 'DIPRAPP_DETT_GENERALE';
  public readonly tipoContrattoEnum = TipoContratto;
  public readonly destinazioneEccedenzeEnum = DestinazioneEccedenze;
  public readonly SEGMENT_DIPRAPP_PROF_HH   = 'DIPRAPP_DETT_PROF_HH';
  public readonly SEGMENT_DIPRAPP_VARIE     = 'DIPRAPP_DETT_VARIE';
  public readonly SEGMENT_DIPRAPP_CONTATORI = 'DIPRAPP_DETT_CONTATORI';
  public readonly SEGMENT_DIPRAPP_SPESA     = 'DIPRAPP_DETT_SPESA';

  // ── Contatori ─────────────────────────────────────────────────────────────
  public annoSelezionato: number = new Date().getFullYear();
  public anniDisponibili: number[] = Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - i);
  public contatoriMesi: Contatori_Anno_MeseResult[] = [];
  public contatoriLoading = false;
  /** Mappa idJust → Contatori_Riporto_Model: usata per il binding diretto con ngModel. */
  public currentRiporti: { [idJust: number]: Contatori_Riporto_Model } = {};
  /** Mappa idJust → Dip_Rapporto_Giustificativi_MaturazioneModel: NON sensibile all'anno selezionato. */
  public currentMaturazioni: { [idJust: number]: Dip_Rapporto_Giustificativi_MaturazioneModel } = {};

  /** Id del rapporto di lavoro attivo nella tab Contatori (usato per il debounce). */
  private _currentContatoriRappLavId: number = 0;
  private _contatoriRichiesta: number = 0;
  public riportoLoading: boolean = false;
  /** Trigger debounced per ricalcolare i contatori dopo la modifica di un riporto. */
  private _riportoChangedSubject = new Subject<void>();
  private _destroy$ = new Subject<void>();
  /** Nomi dei mesi nella lingua dell'interfaccia, con l'iniziale maiuscola. */
  public readonly nomiMesi = Array.from({ length: 12 }, (_, i) => {
    const nome = new Intl.DateTimeFormat(this.languageService.locale, { month: 'long' }).format(new Date(2000, i, 1));
    return nome.charAt(0).toUpperCase() + nome.slice(1);
  });


  public btnEdit_Dip_ProfiloOrario: ButtonItem;
  public btnDelete_Dip_ProfiloOrario: ButtonItem;

  public btnDelete_Dip_Competenza: ButtonItem;

  public btnEdit_Dip_RapportoAutovetture: ButtonItem;
  public btnDelete_Dip_RapportoAutovetture: ButtonItem;


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
    private contatoriService: ContatoriService,
    private dbUtilService: DbUtilService,
    private collectionDialogService: CollectionDialogService,
    private cdr: ChangeDetectorRef,
    private transloco: TranslocoService,
    private languageService: LanguageService,
    public funzionalitaGestionePresenzeService: FunzionalitaGestionePresenzeService) {

    super(navCtrl, userInterfaceService, fb);

    this.btnEdit_Dip_ProfiloOrario = this.userInterfaceService.Btn_Modifica;
    this.btnEdit_Dip_ProfiloOrario.event = this.handleButton_Dip_ProfiloOrario_EditClick;

    this.btnDelete_Dip_ProfiloOrario = userInterfaceService.Btn_Cancella;
    this.btnDelete_Dip_ProfiloOrario.event = this.handleButton_Dip_ProfiloOrario_DeleteClick;

    this.btnDelete_Dip_Competenza = userInterfaceService.Btn_Cancella;
    this.btnDelete_Dip_Competenza.event = this.handleButton_Dip_Competenza_DeleteClick;

    this.btnEdit_Dip_RapportoAutovetture = userInterfaceService.Btn_Modifica;
    this.btnEdit_Dip_RapportoAutovetture.event = this.handleButton_Dip_RapportoAutovetture_EditClick;

    this.btnDelete_Dip_RapportoAutovetture = userInterfaceService.Btn_Cancella;
    this.btnDelete_Dip_RapportoAutovetture.event = this.handleButton_Dip_RapportoAutovetture_DeleteClick;

    // Ricalcola i contatori 800ms dopo l'ultima modifica al riporto, passando i valori non ancora salvati
    this._riportoChangedSubject.pipe(
      debounceTime(800),
      takeUntil(this._destroy$)
    ).subscribe(() => {
      if (this._currentContatoriRappLavId > 0)
        this.loadContatoriAnno(this._currentContatoriRappLavId, true);
    });
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
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
    } else if (this.currSection_segment_main == this.SEGMENT_MAIN_COMPETENZE) {

      this.fabMenuService.fabMenuItem = [
        new FabMenuItem('xxx', 'add-circle-outline', () => {
          this.Dip_Competenza_DialogOpen();
        }),
      ];

    } else if (this.currSection_segment_main.startsWith(this.SEGMENT_MAIN_RAPP_LAV)) {

      const curr_RappLav = this.getCurr_RappLav_Id();
      const currKey = this.currSection_segment_dip_rapp[curr_RappLav];
      

      if (currKey.includes(this.SEGMENT_DIPRAPP_VARIE)) {

      } else if (currKey.includes(this.SEGMENT_DIPRAPP_CONTATORI)) {

      } else if (currKey.includes(this.SEGMENT_DIPRAPP_SPESA)) {

        this.fabMenuService.fabMenuItem = [
          new FabMenuItem('xxx', 'add-circle-outline', () => {
            this.EditDipRapportoAutovettureDialog_Open(0);
          }),
        ];

      } else if (currKey.includes(this.SEGMENT_DIPRAPP_PROF_HH)) {

        

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
      this.currSection_segment_dip_rapp[rappLavId] = this.SEGMENT_MAIN_RAPP_LAV + rappLavId + '_' + this.SEGMENT_DIPRAPP_GENERALE;
    }

    // Carica contatori se la scheda attiva è CONTATORI
    this.loadContatoriIfNeeded(rappLavId);

    this.setfabMenuService();
  }

  segment_dip_rapp_segmentChanged(event: any, rappLavId: number) {
    console.log(`Sotto-segment per Rapporto ID ${rappLavId} cambiato:`, event.detail.value);
    this.currSection_segment_dip_rapp[rappLavId] = event.detail.value;
    this.loadContatoriIfNeeded(rappLavId);
    this.setfabMenuService();
  }


  get Title(): string { return this.transloco.translate('attendance.pages.userDepartmentEdit.title'); }
  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      roles: [[], this.minArrayLength(1)],
      disabled: [false],
      // email: modificabile anche per gli utenti esistenti (obbligatoria solo per i nuovi;
      // vuota su un utente esistente = non modificata)
      mail: [null, [Validators.email]],
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
        catchError((error): Observable<Dip_Anagrafica4EditModel | null> => {
          console.error('Errore durante la chiamata API:', error);
          return of(null);
        })
      );
    } else {
      return new Observable<Dip_Anagrafica4EditModel | null>((subscriber) => {

        this._editForm.controls['mail'].addValidators(Validators.required);
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
        Dip_RapportoLavoro.dataAss = this.oggiIso();
        dip_Anagrafica4Edit.dip_RapportoLavoro.push(Dip_RapportoLavoro);
        

        if (this.sharedParameterGestionePresenzeService.Par_ProfiloOrario.length > 0) {
          let dip_ProfiloOrarioModel: Dip_ProfiloOrarioModel = new Dip_ProfiloOrarioModel();

          dip_ProfiloOrarioModel.id = this.dbUtilService.GenerateCounterKey();
          dip_ProfiloOrarioModel.idDip_RapportoLavoro = Dip_RapportoLavoro.id;
          dip_ProfiloOrarioModel.numGiornoPartenzaCiclo = 1;
          dip_ProfiloOrarioModel.idPar_ProfiloOrario = this.sharedParameterGestionePresenzeService.Par_ProfiloOrario[0].id;

          dip_Anagrafica4Edit.dip_ProfiloOrario.push(dip_ProfiloOrarioModel);

        }

        // nuovo dipendente: competenza di default dell'azienda
        const par_CompetenzaDefault = (this.sharedParameterGestionePresenzeService.Par_Competenza ?? []).find(x => x.default);
        if (par_CompetenzaDefault) {
          let dip_Competenza = new Dip_CompetenzaModel();
          dip_Competenza.id = this.dbUtilService.GenerateCounterKey();
          dip_Competenza.idDip_Anagrafica = dip_Anagrafica4Edit.id;
          dip_Competenza.idPar_Competenza = par_CompetenzaDefault.id;
          dip_Anagrafica4Edit.dip_Competenza.push(dip_Competenza);
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
    // sezione Spesa non gestita: senza autovetture il server le lascia come sono
    if (!this.funzionalitaGestionePresenzeService.sezioneSpesaRapporto)
      request.data.dip_Anagrafica = Object.assign(new Dip_Anagrafica4EditModel(), editModel, { dip_RapportoAutovetture: null });
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

    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.userDepartmentEdit.confirmDeleteWorkSchedule'));
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






  // ── Competenze ────────────────────────────────────────────────────────────

  getAll_Dip_Competenza(): Dip_CompetenzaModel[] {
    if (!this._editModel) return [];
    return this._editModel.dip_Competenza;
  }

  handleButton_Dip_Competenza_DeleteClick = async (item: Dip_CompetenzaModel) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.userDepartmentEdit.confirmDeleteSkill'));
    if (result) {
      const index = this._editModel.dip_Competenza.findIndex(p => p.idPar_Competenza === item.idPar_Competenza);
      if (index > -1) {
        this._editModel.dip_Competenza.splice(index, 1);
      }
    }

  }

  async Dip_Competenza_DialogOpen() {

    const modal = await this.modalCtrl.create({
      component: SeletionParCompetenzaDialogComponent,
      componentProps: {
        idPar_Competenza_Escluse: this._editModel.dip_Competenza.map(x => x.idPar_Competenza)
      },
    });

    await modal.present();

    const { data, role } = await modal.onWillDismiss<SeletionParCompetenzaDialogResult | null>();

    if (role === 'confirm' && data && data.id) {
      data.id.forEach(idPar_Competenza => {
        if (!this._editModel.dip_Competenza.some(x => x.idPar_Competenza === idPar_Competenza)) {
          let dip_Competenza = new Dip_CompetenzaModel();
          dip_Competenza.id = this.dbUtilService.GenerateCounterKey();
          dip_Competenza.idDip_Anagrafica = this._editModel.id;
          dip_Competenza.idPar_Competenza = idPar_Competenza;
          this._editModel.dip_Competenza.push(dip_Competenza);
        }
      });
    }
  }



  // ── Spesa: autovetture del rapporto (rimborsi km) ─────────────────────────

  getAll_Dip_RapportoAutovetture(): Dip_RapportoAutovettureModel[] {
    if (!this._editModel?.dip_RapportoAutovetture) return [];
    const curr_RappLav = this.getCurr_RappLav_Id();
    return this._editModel.dip_RapportoAutovetture.filter(x => x.idDip_RapportoLavoro == curr_RappLav);
  }

  handleButton_Dip_RapportoAutovetture_EditClick = (item: Dip_RapportoAutovettureModel) => {
    this.EditDipRapportoAutovettureDialog_Open(item.id);
  }

  handleButton_Dip_RapportoAutovetture_DeleteClick = async (item: Dip_RapportoAutovettureModel) => {
    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.userDepartmentEdit.confirmDeleteCar'));
    if (result && this._editModel.dip_RapportoAutovetture) {
      const index = this._editModel.dip_RapportoAutovetture.findIndex(p => p.id === item.id);
      if (index > -1)
        this._editModel.dip_RapportoAutovetture.splice(index, 1);
    }
  }

  async EditDipRapportoAutovettureDialog_Open(id: number) {
    this._editModel.dip_RapportoAutovetture ??= [];

    // si modifica una copia: annullando il dialog la riga resta com'era
    let autovettura: Dip_RapportoAutovettureModel;
    if (id === 0) {
      autovettura = new Dip_RapportoAutovettureModel();
      autovettura.id = this.dbUtilService.GenerateCounterKey();
      autovettura.idDip_RapportoLavoro = this.getCurr_RappLav_Id();
    } else {
      autovettura = Object.assign(new Dip_RapportoAutovettureModel(), this._editModel.dip_RapportoAutovetture.find(p => p.id === id));
    }

    const modal = await this.modalCtrl.create({
      component: EditDipRapportoAutovettureDialogComponent,
      componentProps: {
        dip_RapportoAutovetture: autovettura
      },
    });

    await modal.present();

    const { data, role } = await modal.onWillDismiss<Dip_RapportoAutovettureModel | null>();

    if (role === 'confirm' && data) {
      data.rimborsoKM = Number(data.rimborsoKM) || 0;
      data.targa = data.targa ? String(data.targa).trim().toUpperCase() : null;
      const index = this._editModel.dip_RapportoAutovetture.findIndex(p => p.id === data.id);
      if (index > -1) {
        this._editModel.dip_RapportoAutovetture[index] = data;
      } else {
        this._editModel.dip_RapportoAutovetture.push(data);
      }
    }
  }

  rimborsoKMTesto(valore: number): string {
    return '€ ' + (Number(valore) || 0).toFixed(4).replace('.', ',');
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

  // ── Generale: date del rapporto ──────────────────────────────────────────

  /** Oggi nel formato delle date del rapporto ("YYYY-MM-DDT00:00:00", senza fuso orario). */
  oggiIso(): string {
    const d = new Date();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    return `${d.getFullYear()}-${mm}-${dd}T00:00:00`;
  }

  /** Salva solo il giorno scelto: senza ora e fuso orario il server non lo sposta al giorno prima. */
  onDataRapportoChange(rapporto: Dip_RapportoLavoroModel, campo: 'dataAss' | 'dataLic', event: any): void {
    const value = event?.detail?.value;
    if (typeof value !== 'string' || value.length < 10) return;
    const nuova = value.substring(0, 10) + 'T00:00:00';
    if (campo === 'dataAss') this.allineaPrimoProfiloOrario(rapporto, rapporto.dataAss, nuova);
    rapporto[campo] = nuova;
  }

  /**
   * Il calcolo considera solo i giorni coperti da un profilo orario: se l'assunzione si sposta,
   * il primo profilo del rapporto la segue quando partiva dalla vecchia data o dopo la nuova.
   */
  private allineaPrimoProfiloOrario(rapporto: Dip_RapportoLavoroModel, vecchia: string | null, nuova: string): void {
    const primo = this._editModel.dip_ProfiloOrario
      .filter(p => p.idDip_RapportoLavoro === rapporto.id && p.dal)
      .sort((a, b) => a.dal.localeCompare(b.dal))[0];
    if (!primo) return;
    const dal = primo.dal.substring(0, 10);
    if (dal === vecchia?.substring(0, 10) || dal > nuova.substring(0, 10))
      primo.dal = nuova;
  }

  // ── Contatori ─────────────────────────────────────────────────────────────

  /**
   * Cache degli array usati nel template.
   * Sono proprietà calcolate UNA SOLA VOLTA quando i dati cambiano, non ad ogni
   * ciclo di change-detection. Usarli direttamente in *ngFor evita che Angular
   * confronti riferimenti diversi ad ogni CD e ricostruisca il DOM
   * (che causa il reset della posizione di scroll di IonContent).
   */
  public justContatori: Par_GiustificativiModel[] = [];
  public contatoriRighe: ContatoreRiga[] = [];

  /** Aggiorna la cache dei giustificativi-contatore (chiamato dopo il caricamento parametri). */
  private refreshJustContatori(): void {
    this.justContatori = this.sharedParameterGestionePresenzeService.Par_Giustificativi
      .filter(j => j.tipoContatore !== TipoContatore.NoContatore);
  }

  /** Aggiorna la cache delle righe griglia (chiamato dopo loadContatoriAnno). */
  private refreshContatoriRighe(): void {
    const righe: ContatoreRiga[] = [];
    // solo i mesi del rapporto: dal mese di assunzione a quello di licenziamento (AAAAMM)
    const rapporto = this._editModel?.dip_RapportoLavoro.find(x => x.id === this._currentContatoriRappLavId);
    const meseDa = rapporto?.dataAss ? Number(rapporto.dataAss.substring(0, 7).replace('-', '')) : 0;
    const meseA  = rapporto?.dataLic ? Number(rapporto.dataLic.substring(0, 7).replace('-', '')) : 999999;
    for (const meseItem of this.contatoriMesi) {
      const annoMese = this.annoSelezionato * 100 + meseItem.mese;
      if (annoMese < meseDa || annoMese > meseA) continue;
      this.justContatori.forEach((just, idx) => {
        const r = meseItem.risultati.find(x => x.idPar_Giustificativi === just.id) ?? null;
        righe.push({
          mese:      meseItem.mese,
          nomeMese:  this.nomiMesi[meseItem.mese - 1],
          showMese:  idx === 0,
          just,
          risultato: r
        });
      });
    }
    this.contatoriRighe = righe;
  }

  /** Saldo negativo: i saldi arrivano come "[-][Xg ]HH:mm", il segno è il primo carattere. */
  isSaldoNegativo(saldo: string | null | undefined): boolean {
    return !!saldo?.startsWith('-');
  }

  /** @deprecated Usare la proprietà `justContatori` nel template. */
  getJustContatori(): Par_GiustificativiModel[] { return this.justContatori; }

  /** Carica il dettaglio mensile dei contatori se la scheda attiva è CONTATORI. */
  loadContatoriIfNeeded(rappLavId: number): void {
    const currKey = this.currSection_segment_dip_rapp[rappLavId];
    if (!currKey?.includes(this.SEGMENT_DIPRAPP_CONTATORI)) return;
    if (rappLavId <= 0) return;
    this._currentContatoriRappLavId = rappLavId;
    this.refreshJustContatori();
    this.refreshCurrentRiporti(rappLavId);
    this.refreshCurrentMaturazioni(rappLavId);
    this.loadContatoriAnno(rappLavId);
  }

  /**
   * Carica il calcolo annuale dei contatori.
   * @param withOverride se true, passa i valori correnti (non salvati) di riporti
   *   e maturazioni come override runtime, così il calcolo riflette i valori
   *   che l'utente ha editato senza ancora salvare.
   */
  loadContatoriAnno(rappLavId: number, withOverride = false): void {
    this.contatoriLoading = true;
    this.contatoriMesi    = [];
    this.contatoriRighe   = [];
    // vale solo l'ultima richiesta: una risposta arrivata in ritardo (es. dell'anno lasciato) si scarta
    const richiesta = ++this._contatoriRichiesta;
    const req = new GenericRequest<Contatori_Anno_InModel>(Contatori_Anno_InModel);
    req.data.idDip_RapportoLavoro = rappLavId;
    req.data.anno                 = this.annoSelezionato;

    if (withOverride) {
      // Solo i riporti inseriti: quelli non inseriti valgono zero
      req.data.riportiOverride = Object.values(this.currentRiporti)
        .filter(r => this.isRiportoNelModello(r))
        .map(r => ({
        ...r,
        anno: this.annoSelezionato
      }));
      // Maturazioni correnti
      req.data.maturazioneOverride = Object.values(this.currentMaturazioni).map(m => ({
        idPar_Giustificativi: m.idPar_Giustificativi,
        oreMaturazione:       m.oreMaturazione
      }));
    }

    this.contatoriService.CalcolaContatori_Anno(req).subscribe({
      next: res => {
        if (richiesta !== this._contatoriRichiesta) return;
        if (res.success && res.data) {
          this.contatoriMesi = res.data.mesi;
          this.refreshContatoriRighe();
        }
        this.contatoriLoading = false;
      },
      error: () => { if (richiesta === this._contatoriRichiesta) this.contatoriLoading = false; }
    });
  }

  /** Il riporto si può leggere solo se l'anno precedente rientra nel rapporto di lavoro. */
  get riportoDaAnnoPrecedenteAbilitato(): boolean {
    const rapporto = this._editModel?.dip_RapportoLavoro.find(x => x.id === this._currentContatoriRappLavId);
    const annoAss = rapporto?.dataAss ? Number(rapporto.dataAss.substring(0, 4)) : 0;
    return !this.riportoLoading && annoAss > 0 && this.annoSelezionato - 1 >= annoAss;
  }

  /**
   * Legge i contatori dell'anno precedente e mette il saldo di fine dicembre
   * nelle caselle del riporto, come se l'utente li avesse inseriti (vengono salvati con Conferma).
   */
  async riportoDaAnnoPrecedente(rappLavId: number): Promise<void> {
    const conferma = await this.collectionDialogService.ConfirmCancelDialog(
      this.transloco.translate('attendance.pages.userDepartmentEdit.replaceCarryOverConfirm', { year: this.annoSelezionato, prevYear: this.annoSelezionato - 1 }));
    if (!conferma) return;

    const req = new GenericRequest<Contatori_Anno_InModel>(Contatori_Anno_InModel);
    req.data.idDip_RapportoLavoro = rappLavId;
    req.data.anno                 = this.annoSelezionato - 1;
    // riporti dell'anno precedente inseriti e non ancora salvati
    req.data.riportiOverride = this._editModel.dip_Contatori_Riporto
      .filter(r => r.idDip_RapportoLavoro === rappLavId && r.anno === req.data.anno);
    // la maturazione vale per tutti gli anni: si usano anche i valori non ancora salvati
    req.data.maturazioneOverride = Object.values(this.currentMaturazioni).map(m => ({
      idPar_Giustificativi: m.idPar_Giustificativi,
      oreMaturazione:       m.oreMaturazione
    }));

    this.riportoLoading = true;
    this.contatoriService.CalcolaContatori_Anno(req).subscribe({
      next: res => {
        const dicembre = res.success ? res.data?.mesi.find(m => m.mese === 12) : null;
        for (const just of this.justContatori) {
          const saldo = dicembre?.risultati.find(x => x.idPar_Giustificativi === just.id)?.periodoFineMese?.saldo;
          const r = this.currentRiporti[just.id];
          if (saldo == null || !r) continue;
          r.saldoRiporto = this.saldoToRiporto(saldo);
          this.onRiportoChange(just.id);
        }
        this.riportoLoading = false;
      },
      error: () => { this.riportoLoading = false; }
    });
  }

  /** Saldo visualizzato ("[-][Xg ]HH:mm") nel formato del riporto ("[-]HHH:MM"). */
  private saldoToRiporto(saldo: string): string {
    const negativo = saldo.trim().startsWith('-');
    const testo = saldo.replace('-', '').trim();
    const g = testo.indexOf('g');
    const giorni = g > 0 ? Number(testo.substring(0, g)) || 0 : 0;
    const [ore, minuti] = (g > 0 ? testo.substring(g + 1) : testo).trim().split(':').map(x => Number(x) || 0);
    const totale = giorni * 24 * 60 + (ore ?? 0) * 60 + (minuti ?? 0);
    const hh = String(Math.floor(totale / 60)).padStart(2, '0');
    const mm = String(totale % 60).padStart(2, '0');
    return `${negativo && totale > 0 ? '-' : ''}${hh}:${mm}`;
  }

  /**
   * Ricalcola currentRiporti per il rappLav e annoSelezionato correnti.
   * Se il riporto non esiste in dip_Contatori_Riporto NON lo crea subito:
   * viene creato solo al momento della modifica (onRiportoChange).
   * Questo evita la proliferazione di record con id=0 e salvataggi duplicati.
   */
  refreshCurrentRiporti(rappLavId: number): void {
    if (!this._editModel) return;
    this.currentRiporti = {};
    for (const just of this.justContatori) {
      const existing = this._editModel.dip_Contatori_Riporto
        .find(x => x.idDip_RapportoLavoro === rappLavId
                && x.idPar_Giustificativi  === just.id
                && x.anno                  === this.annoSelezionato);
      if (existing) {
        this.currentRiporti[just.id] = existing;
      } else {
        // Placeholder locale: non ancora nell'array principale
        const placeholder = new Contatori_Riporto_Model();
        placeholder.idDip_RapportoLavoro = rappLavId;
        placeholder.idPar_Giustificativi  = just.id;
        placeholder.anno                  = this.annoSelezionato;
        placeholder.saldoRiporto          = '00:00';
        this.currentRiporti[just.id] = placeholder;
      }
    }
  }

  private isRiportoNelModello(r: Contatori_Riporto_Model): boolean {
    return this._editModel.dip_Contatori_Riporto
      .some(x => x.idDip_RapportoLavoro === r.idDip_RapportoLavoro
              && x.idPar_Giustificativi  === r.idPar_Giustificativi
              && x.anno                  === r.anno);
  }

  /** Chiamato dal template quando l'utente modifica un valore di riporto. */
  onRiportoChange(justId: number): void {
    const r = this.currentRiporti[justId];
    if (!r || !this._editModel) return;
    // Aggiunge al modello principale solo se non già presente
    const exists = this._editModel.dip_Contatori_Riporto
      .find(x => x.idDip_RapportoLavoro === r.idDip_RapportoLavoro
              && x.idPar_Giustificativi  === r.idPar_Giustificativi
              && x.anno                  === r.anno);
    if (!exists) {
      this._editModel.dip_Contatori_Riporto.push(r);
    }
    // Ricalcola i contatori con debounce (passa i valori non ancora salvati)
    this._riportoChangedSubject.next();
  }

  onAnnoChange(rappLavId: number, anno?: number): void {
    // l'anno arriva dall'evento: non dipende dall'ordine in cui ngModel aggiorna annoSelezionato
    if (anno) this.annoSelezionato = anno;
    this.refreshCurrentRiporti(rappLavId);
    this.loadContatoriAnno(rappLavId);
    // la maturazione NON dipende dall'anno: non serve refreshCurrentMaturazioni
  }

  /**
   * Popola currentMaturazioni per il rappLav dato.
   * NON è sensibile all'anno: la maturazione è per rapporto+giustificativo, non per anno.
   */
  refreshCurrentMaturazioni(rappLavId: number): void {
    if (!this._editModel) return;
    this.currentMaturazioni = {};
    for (const just of this.justContatori) {
      const existing = this._editModel.dip_Maturazione
        .find(x => x.idDip_RapportoLavoro === rappLavId
                && x.idPar_Giustificativi  === just.id);
      if (existing) {
        this.currentMaturazioni[just.id] = existing;
      } else {
        // Placeholder locale: non aggiunto all'array principale finché non modificato
        const placeholder = new Dip_Rapporto_Giustificativi_MaturazioneModel();
        placeholder.idDip_RapportoLavoro = rappLavId;
        placeholder.idPar_Giustificativi  = just.id;
        placeholder.oreMaturazione        = '00:00';
        this.currentMaturazioni[just.id] = placeholder;
      }
    }
  }

  /** Chiamato dal template quando l'utente modifica un valore di maturazione. */
  onMaturazioneChange(justId: number): void {
    const m = this.currentMaturazioni[justId];
    if (!m || !this._editModel) return;
    const exists = this._editModel.dip_Maturazione
      .find(x => x.idDip_RapportoLavoro === m.idDip_RapportoLavoro
              && x.idPar_Giustificativi  === m.idPar_Giustificativi);
    if (!exists) {
      this._editModel.dip_Maturazione.push(m);
    }
    // Ricalcola i contatori con debounce (passa i valori non ancora salvati)
    this._riportoChangedSubject.next();
  }

}

const matchPasswords: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('pw')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;
  return password === confirmPassword ? null : { notMatching: true };
};

interface ContatoreRiga {
  mese:      number;
  nomeMese:  string;
  showMese:  boolean;
  just:      Par_GiustificativiModel;
  risultato: import('../../../ClientServer-Service/GestionePresenze/Contatori/Models/contatori-model').Contatori_Giustificativo_Result | null;
}
