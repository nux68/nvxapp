import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ModalController, NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { DbUtilService } from '../../../Utility/infrastructure/db-util.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { EditParProfiloFormuleDettDialogComponent } from '../../../shared/components/GestionePresenze/edit-par-profilo-formule-dett-dialog/edit-par-profilo-formule-dett-dialog.component';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParFormuleService } from '../../../ClientServer-Service/GestionePresenze/Par_Formule/par-formule.service';
import { ParProfiloFormuleService } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloFormule/par-profilo-formule.service';
import { FORMULA_FASE_TESTO, FormulaFase, Par_FormuleInModel, Par_FormuleModel } from '../../../ClientServer-Service/GestionePresenze/Par_Formule/Models/par-formule-model';
import {
  Par_ProfiloFormule_DuplicaInModel, Par_ProfiloFormuleDettModel, Par_ProfiloFormuleGetInModel, Par_ProfiloFormuleModel, Par_ProfiloFormulePutInModel
} from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloFormule/Models/par-profilo-formule-model';

// modifica di un profilo formule: testata + righe (formula, ordine, fase, Dal/Al, sospesa). Le righe si salvano con la testata.
@Component({
  selector: 'app-profilo-formule-edit-page',
  templateUrl: './profilo-formule-edit-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ProfiloFormuleEditPageComponent extends BasePageConfirmCancelComponent<Par_ProfiloFormuleModel> {

  public righe: Par_ProfiloFormuleDettModel[] = [];
  public formule: Par_FormuleModel[] = [];
  public duplicaCodice = '';

  public readonly columns: NvxListColumn<Par_ProfiloFormuleDettModel>[];
  public actions: NvxListAction<Par_ProfiloFormuleDettModel>[] = [];
  public readonly sortRighe = (a: Par_ProfiloFormuleDettModel, b: Par_ProfiloFormuleDettModel) => a.ordine - b.ordine;
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    public fabMenuService: FabMenuService,
    private modalCtrl: ModalController,
    private dbUtilService: DbUtilService,
    private collectionDialogService: CollectionDialogService,
    private parFormuleService: ParFormuleService,
    private parProfiloFormuleService: ParProfiloFormuleService,
    private transloco: TranslocoService
  ) {
    super(navCtrl, userInterfaceService, fb);
    const t = (k: string) => this.transloco.translate('attendance.pages.profiloFormuleEdit.' + k);
    const data = (s: string) => s ? s.substring(8, 10) + '/' + s.substring(5, 7) + '/' + s.substring(0, 4) : '';
    this.columns = [
      { key: 'ordine', title: t('colOrdine'), width: '90px', mobile: 'subtitle' },
      { key: 'idPar_Formule', title: t('colFormula'), mobile: 'title', value: (r: Par_ProfiloFormuleDettModel) => this.nomeFormula(r.idPar_Formule) },
      { key: 'fase', title: t('colFase'), width: '130px', mobile: 'note', value: (r: Par_ProfiloFormuleDettModel) => this.transloco.translate(FORMULA_FASE_TESTO[r.fase as FormulaFase] ?? '') },
      { key: 'dal', title: t('colDal'), width: '120px', mobile: 'note', value: (r: Par_ProfiloFormuleDettModel) => data(r.dal), sortValue: (r: Par_ProfiloFormuleDettModel) => r.dal },
      { key: 'al', title: t('colAl'), width: '120px', mobile: 'note', value: (r: Par_ProfiloFormuleDettModel) => data(r.al), sortValue: (r: Par_ProfiloFormuleDettModel) => r.al },
      { key: 'sospesa', title: t('colSospesa'), width: '110px', mobile: 'note', value: (r: Par_ProfiloFormuleDettModel) => r.sospesa ? t('sospesa') : '' },
    ];
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = (r: Par_ProfiloFormuleDettModel) => this.apriRiga(r);
    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.cancellaRiga;
    this.actions = [{ button: this.btnDelete }, { button: this.btnEdit }];
  }

  get Title(): string { return this.transloco.translate('attendance.pages.profiloFormuleEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required, Validators.maxLength(20)]],
      descrizione: [null, [Validators.required, Validators.maxLength(200)]],
    });
  }

  override ionViewWillEnter() {
    super.ionViewWillEnter();
    const request = new GenericRequest<Par_FormuleInModel>(Par_FormuleInModel);
    this.parFormuleService.GetAll(request).subscribe(res => this.formule = res.data?.par_Formule ?? []);
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('attendance.pages.profiloFormuleEdit.nuovaRiga'), 'add-circle-outline', () => this.apriRiga(null)),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  LoadData = (): Observable<Par_ProfiloFormuleModel | null> => {
    const request = new GenericRequest<Par_ProfiloFormuleGetInModel>(Par_ProfiloFormuleGetInModel);
    request.data.id = history.state?.id ?? 0;
    return this.parProfiloFormuleService.Par_ProfiloFormuleGet(request).pipe(
      map(res => {
        this.righe = res.data?.par_ProfiloFormuleDett ?? [];
        return res.data?.par_ProfiloFormule ?? null;
      }),
      catchError(error => { console.error('Errore durante la chiamata API:', error); return of(null); })
    );
  };

  SaveData = (editModel: Par_ProfiloFormuleModel): Observable<boolean> => {
    const request = new GenericRequest<Par_ProfiloFormulePutInModel>(Par_ProfiloFormulePutInModel);
    request.data.par_ProfiloFormule = editModel;
    request.data.par_ProfiloFormuleDett = this.righe;
    return this.parProfiloFormuleService.Par_ProfiloFormulePut(request).pipe(
      map(res => res.success),
      catchError(error => { console.error('Errore durante la chiamata API:', error); return of(false); })
    );
  };

  override AfterSave(saved: boolean): void {
    if (saved) this.navCtrl.back();
  }

  nomeFormula(id: number): string {
    const f = this.formule.find(x => x.id === id);
    return f ? `${f.codice} - ${f.titolo}` : String(id);
  }

  async apriRiga(riga: Par_ProfiloFormuleDettModel | null) {
    let modello: Par_ProfiloFormuleDettModel;
    if (riga) {
      modello = { ...riga };
    } else {
      modello = new Par_ProfiloFormuleDettModel();
      modello.id = this.dbUtilService.GenerateCounterKey();
      modello.idPar_ProfiloFormule = this._editModel?.id ?? 0;
      modello.ordine = (this.righe.length ? Math.max(...this.righe.map(r => r.ordine)) : 0) + 10;
      modello.dal = '2000-01-01';
      modello.al = '2100-12-31';
      modello.fase = FormulaFase.Finale;
    }

    const modal = await this.modalCtrl.create({
      component: EditParProfiloFormuleDettDialogComponent,
      componentProps: { riga: modello, formule: this.formule },
    });
    await modal.present();
    const { data, role } = await modal.onWillDismiss<Par_ProfiloFormuleDettModel | null>();
    if (role === 'confirm' && data) {
      const index = this.righe.findIndex(r => r.id === data.id);
      if (index > -1) this.righe[index] = data;
      else this.righe.push(data);
      this.righe = [...this.righe];
      this._editForm.markAsDirty();
    }
  }

  cancellaRiga = async (riga: Par_ProfiloFormuleDettModel) => {
    const ok = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.profiloFormuleEdit.confermaCancellazioneRiga'));
    if (ok) {
      this.righe = this.righe.filter(r => r.id !== riga.id);
      this._editForm.markAsDirty();
    }
  }

  duplica() {
    if (!this._editModel?.id || !this.duplicaCodice.trim()) return;
    const request = new GenericRequest<Par_ProfiloFormule_DuplicaInModel>(Par_ProfiloFormule_DuplicaInModel);
    request.data.id = this._editModel.id;
    request.data.codice = this.duplicaCodice.trim();
    this.parProfiloFormuleService.Par_ProfiloFormuleDuplica(request).subscribe(res => {
      if (res.success && res.data?.par_ProfiloFormule?.id) {
        this.navCtrl.navigateRoot('/profiloformulelist').then(() =>
          this.navCtrl.navigateForward('/profiloformuleedit', { state: { id: res.data.par_ProfiloFormule.id } }));
      }
    });
  }
}
