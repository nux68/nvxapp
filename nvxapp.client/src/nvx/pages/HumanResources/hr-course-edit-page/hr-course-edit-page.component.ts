import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService, hrData } from '../../../ClientServer-Service/HumanResources/hr.service';
import * as M from '../../../ClientServer-Service/HumanResources/Models/hr-model';
import { CommonOrganizzazioneService } from '../../../ClientServer-Service/Infrastructure/Common/common-organizzazione.service';
import { CommonOrganizzazione_CompetenzeGetAll_InModel, Par_CompetenzaCommonModel } from '../../../ClientServer-Service/Infrastructure/Common/Models/common-organizzazione-model';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

@Component({
  selector: 'app-hr-course-edit-page',
  templateUrl: './hr-course-edit-page.component.html',
  styleUrls: ['./hr-course-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Corso del catalogo (HR-FOR.1): titolo, ente, durata, validita' (scadenza dell'attestato), competenza comune acquisita.
 Per un corso salvato: edizioni (il "+" del FAB aggiunge un'edizione).
*/
export class HrCourseEditPageComponent extends BasePageConfirmCancelComponent<M.HrCorsoModel> {

  public id = 0;
  public competenze: Par_CompetenzaCommonModel[] = [];
  public edizioni: M.HrEdizioneBreveModel[] = [];
  public readonly colEdizioni: NvxListColumn<M.HrEdizioneBreveModel>[];
  public readonly azEdizioni: NvxListAction<M.HrEdizioneBreveModel>[];
  private nuovo = false;
  private attiva = false;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: HrService,
              private organizzazione: CommonOrganizzazioneService,
              public fabMenuService: FabMenuService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    const t = (k: string) => this.transloco.translate(k);
    this.colEdizioni = [
      { key: 'dataInizio', title: t('hr.fields.dataInizio'), mobile: 'title', value: x => hrData(x.dataInizio) + (x.dataFine && x.dataFine !== x.dataInizio ? ' - ' + hrData(x.dataFine) : ''), sortValue: x => x.dataInizio },
      { key: 'sede', title: t('hr.fields.sede'), mobile: 'subtitle', value: x => x.sede ?? '' },
      { key: 'docente', title: t('hr.fields.docente'), mobile: 'hidden', value: x => x.docente ?? '' },
      { key: 'partecipanti', title: t('hr.fields.partecipanti'), align: 'end', mobile: 'note', value: x => `${x.numeroSuperati}/${x.numeroPartecipanti}` },
    ];
    const bOpen = userInterfaceService.Btn_Apri;
    bOpen.event = (x: M.HrEdizioneBreveModel) => this.navCtrl.navigateForward('/hrcourseedition', { state: { id: x.id, idHr_Corso: this.id } });
    const bDel = userInterfaceService.Btn_Cancella;
    bDel.event = (x: M.HrEdizioneBreveModel) => this.eliminaEdizione(x);
    this.azEdizioni = [{ button: bDel }, { button: bOpen }];
  }

  get Title(): string { return this.transloco.translate('hr.pages.courseEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      titolo: [null, [Validators.required, Validators.maxLength(200)]],
      ente: [null, [Validators.maxLength(200)]],
      durataOre: [null, [Validators.min(0)]],
      validitaMesi: [null, [Validators.min(0), Validators.max(600)]],
      idPar_Competenza: [null],
      obbligatorio: [false],
      attivo: [true],
      note: [null, [Validators.maxLength(2000)]],
    });
  }

  override ionViewWillEnter() {
    this.attiva = true;
    this.organizzazione.CompetenzeGetAll(new GenericRequest(CommonOrganizzazione_CompetenzeGetAll_InModel)).subscribe(res => this.competenze = res.success ? res.data.par_Competenza : []);
    super.ionViewWillEnter();
  }

  ionViewWillLeave() {
    this.attiva = false;
    this.fabMenuService.fabMenuItem = [];
  }

  LoadData = (): Observable<M.HrCorsoModel | null> => {
    this.id = this.id || history.state?.id || 0;
    this.aggiornaFab();
    if (!this.id) {
      this.edizioni = [];
      return of(new M.HrCorsoModel());
    }
    const request = new GenericRequest<M.HrIdInModel>(M.HrIdInModel);
    request.data.id = this.id;
    return this.service.CorsoGet(request).pipe(map(res => {
      this.edizioni = res.success ? res.data.edizioni : [];
      return res.success ? res.data.corso : null;
    }), catchError(() => of(null)));
  };

  private aggiornaFab() {
    if (!this.attiva) return;
    this.fabMenuService.fabMenuItem = this.id
      ? [new FabMenuItem(this.transloco.translate('hr.pages.courseEdit.newEdition'), 'add-circle-outline',
          () => this.navCtrl.navigateForward('/hrcourseedition', { state: { id: 0, idHr_Corso: this.id } }))]
      : [];
  }

  SaveData = (editModel: M.HrCorsoModel): Observable<boolean> => {
    const request = new GenericRequest<M.HrCorsoPutInModel>(M.HrCorsoPutInModel);
    const num = (v: any) => v === null || v === undefined || v === '' ? null : Number(v);
    request.data.corso = { ...editModel, id: this.id, durataOre: num(editModel.durataOre), validitaMesi: num(editModel.validitaMesi) };
    return this.service.CorsoPut(request).pipe(map(res => {
      if (res.success) {
        this.nuovo = !this.id;
        this.id = res.data.corso.id;
      }
      return res.success;
    }), catchError(() => of(false)));
  };

  // un corso nuovo resta aperto (per aggiungere le edizioni), uno esistente torna all'elenco
  override AfterSave(saved: boolean): void {
    if (!saved) return;
    if (this.nuovo) this.ionViewWillEnter();
    else this.navCtrl.back();
  }

  private async eliminaEdizione(x: M.HrEdizioneBreveModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('hr.pages.courseEdit.confirmDeleteEdition', { data: hrData(x.dataInizio) })))
      return;
    const request = new GenericRequest<M.HrIdInModel>(M.HrIdInModel);
    request.data.id = x.id;
    this.service.EdizioneDelete(request).subscribe(res => { if (res.success) this.edizioni = this.edizioni.filter(e => e.id !== x.id); });
  }
}
