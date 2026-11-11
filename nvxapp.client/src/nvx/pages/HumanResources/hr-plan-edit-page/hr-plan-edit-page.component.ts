import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ModalController, NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService, hrData, hrPerInput } from '../../../ClientServer-Service/HumanResources/hr.service';
import * as M from '../../../ClientServer-Service/HumanResources/Models/hr-model';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { hrScegliDipendenti } from '../hr-ui';

@Component({
  selector: 'app-hr-plan-edit-page',
  templateUrl: './hr-plan-edit-page.component.html',
  styleUrls: ['./hr-plan-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Piano di formazione (HR-FOR.1): per ogni dipendente il corso da seguire entro una data; "svolto" se superato nell'anno del piano
export class HrPlanEditPageComponent extends BasePageConfirmCancelComponent<M.HrPianoModel> {

  public righe: M.HrPianoRigaModel[] = [];
  public corsi: M.HrCorsoModel[] = [];
  public readonly data = hrData;
  private dipendenti: M.HrDipendenteBreveModel[] = [];
  private id = 0;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: HrService,
              private modalCtrl: ModalController,
              public fabMenuService: FabMenuService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('hr.pages.planEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      anno: [new Date().getFullYear(), [Validators.required, Validators.min(2000), Validators.max(2100)]],
      descrizione: [null, [Validators.required, Validators.maxLength(200)]],
      note: [null, [Validators.maxLength(2000)]],
    });
  }

  override ionViewWillEnter() {
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('hr.pages.planEdit.addRows'), 'person-add-outline', () => this.aggiungi()),
    ];
    super.ionViewWillEnter();
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  LoadData = (): Observable<M.HrPianoModel | null> => {
    this.id = this.id || history.state?.id || 0;
    const request = new GenericRequest<M.HrIdInModel>(M.HrIdInModel);
    request.data.id = this.id;
    return this.service.PianoGet(request).pipe(map(res => {
      if (!res.success) return null;
      this.corsi = res.data.corsi.filter(c => c.attivo || res.data.piano.righe.some(r => r.idHr_Corso === c.id));
      this.dipendenti = res.data.dipendenti;
      this.righe = res.data.piano.righe.map(r => ({ ...r, entroIl: hrPerInput(r.entroIl) }));
      return res.data.piano;
    }), catchError(() => of(null)));
  };

  async aggiungi() {
    const scelti = await hrScegliDipendenti(this.modalCtrl, this.dipendenti.filter(d => d.attivo), 'multi');
    const corso = this.corsi.length === 1 ? this.corsi[0].id : 0;
    for (const id of scelti ?? []) {
      const d = this.dipendenti.find(x => x.id === id);
      this.righe = [...this.righe, Object.assign(new M.HrPianoRigaModel(), { idDip_Anagrafica: id, idHr_Corso: corso, dipendente: `${d?.cognome ?? ''} ${d?.nome ?? ''}`.trim() })];
    }
  }

  togli(r: M.HrPianoRigaModel) {
    this.righe = this.righe.filter(x => x !== r);
  }

  get righeValide(): boolean { return this.righe.every(r => !!r.idHr_Corso); }

  SaveData = (editModel: M.HrPianoModel): Observable<boolean> => {
    if (!this.righeValide)
      return of(false);
    const request = new GenericRequest<M.HrPianoPutInModel>(M.HrPianoPutInModel);
    request.data.piano = { ...editModel, id: this.id, anno: Number(editModel.anno), righe: this.righe.map(r => ({ ...r, entroIl: r.entroIl || null })) };
    return this.service.PianoPut(request).pipe(map(res => res.success), catchError(() => of(false)));
  };
}
