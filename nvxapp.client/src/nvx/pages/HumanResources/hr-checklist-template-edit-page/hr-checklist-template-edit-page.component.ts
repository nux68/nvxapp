import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService } from '../../../ClientServer-Service/HumanResources/hr.service';
import * as M from '../../../ClientServer-Service/HumanResources/Models/hr-model';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

@Component({
  selector: 'app-hr-checklist-template-edit-page',
  templateUrl: './hr-checklist-template-edit-page.component.html',
  styleUrls: ['./hr-checklist-template-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Modello di checklist (HR-ONB.1-3): tipo (assunzione / cessazione), predefinito (genera le checklist dalle date dei dipendenti)
 e voci: giorni rispetto alla data, responsabile, documento da consegnare o firmare, azione sull'account del dipendente.
*/
export class HrChecklistTemplateEditPageComponent extends BasePageConfirmCancelComponent<M.HrModelloModel> {

  public voci: M.HrModelloVoceModel[] = [];
  public utenti: M.HrUtenteModel[] = [];
  public readonly azioni = [0, 1, 2];
  private id = 0;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: HrService,
              public fabMenuService: FabMenuService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('hr.pages.templateEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      tipo: [M.HrTipoChecklist.Assunzione, [Validators.required]],
      descrizione: [null, [Validators.required, Validators.maxLength(200)]],
      predefinito: [false],
      attivo: [true],
    });
  }

  override ionViewWillEnter() {
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('hr.pages.templateEdit.addItem'), 'add-circle-outline', () => this.aggiungi()),
    ];
    super.ionViewWillEnter();
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  LoadData = (): Observable<M.HrModelloModel | null> => {
    this.id = this.id || history.state?.id || 0;
    const request = new GenericRequest<M.HrIdInModel>(M.HrIdInModel);
    request.data.id = this.id;
    return this.service.ModelloGet(request).pipe(map(res => {
      if (!res.success) return null;
      this.utenti = res.data.utenti;
      this.voci = res.data.modello.voci.map(v => ({ ...v }));
      return res.data.modello;
    }), catchError(() => of(null)));
  };

  aggiungi() {
    this.voci = [...this.voci, new M.HrModelloVoceModel()];
  }

  togli(v: M.HrModelloVoceModel) {
    this.voci = this.voci.filter(x => x !== v);
  }

  sposta(i: number, d: number) {
    const j = i + d;
    if (j < 0 || j >= this.voci.length) return;
    const v = [...this.voci];
    [v[i], v[j]] = [v[j], v[i]];
    this.voci = v;
  }

  get vociValide(): boolean { return this.voci.every(v => !!v.descrizione?.trim()); }

  SaveData = (editModel: M.HrModelloModel): Observable<boolean> => {
    if (!this.vociValide)
      return of(false);
    const request = new GenericRequest<M.HrModelloPutInModel>(M.HrModelloPutInModel);
    request.data.modello = { ...editModel, id: this.id, tipo: Number(editModel.tipo), voci: this.voci.map(v => ({ ...v, giorni: Number(v.giorni) || 0, azione: Number(v.azione) })) };
    return this.service.ModelloPut(request).pipe(map(res => res.success), catchError(() => of(false)));
  };
}
