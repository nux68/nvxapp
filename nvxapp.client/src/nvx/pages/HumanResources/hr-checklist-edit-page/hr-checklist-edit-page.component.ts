import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AlertController, NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService, hrData } from '../../../ClientServer-Service/HumanResources/hr.service';
import * as M from '../../../ClientServer-Service/HumanResources/Models/hr-model';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { hrChiediAccount, hrScegliFile } from '../hr-ui';

class HrChecklistForm { public note: string | null = null; public annullata = false; }

@Component({
  selector: 'app-hr-checklist-edit-page',
  templateUrl: './hr-checklist-edit-page.component.html',
  styleUrls: ['./hr-checklist-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Checklist di un dipendente (HR-ONB.1-3). Le voci si aggiornano subito (completamento, responsabile), con il documento da
 consegnare e la presa visione chiesta al dipendente (con il documentale attivo diventa una firma); le voci con un'azione
 creano o disattivano l'account del dipendente e si completano da sole. Note e annullamento si salvano con Conferma.
*/
export class HrChecklistEditPageComponent extends BasePageConfirmCancelComponent<HrChecklistForm> {

  public dati: M.HrChecklistGetOutModel | null = null;
  public readonly data = hrData;
  private id = 0;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: HrService,
              private alertCtrl: AlertController,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('hr.pages.checklistEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      note: [null, [Validators.maxLength(2000)]],
      annullata: [false],
    });
  }

  get annullata(): boolean { return this.dati?.checklist.stato === M.HrStatoChecklist.Annullata; }

  LoadData = (): Observable<HrChecklistForm | null> => {
    this.id = history.state?.id || this.id;
    const request = new GenericRequest<M.HrIdInModel>(M.HrIdInModel);
    request.data.id = this.id;
    return this.service.ChecklistGet(request).pipe(map(res => this.mostra(res.success ? res.data : null)), catchError(() => of(null)));
  };

  private mostra(d: M.HrChecklistGetOutModel | null): HrChecklistForm | null {
    this.dati = d;
    return d ? { note: d.checklist.note, annullata: d.checklist.stato === M.HrStatoChecklist.Annullata } : null;
  }

  private ricarica() {
    this.LoadData().subscribe();
  }

  private aggiornaVoce(v: M.HrChecklistVoceModel) {
    if (!this.dati) return;
    this.dati.voci = this.dati.voci.map(x => x.id === v.id ? v : x);
    this.ricarica();
  }

  salvaVoce(v: M.HrChecklistVoceModel, modifiche: Partial<M.HrChecklistVoceModel>) {
    const request = new GenericRequest<M.HrChecklistVocePutInModel>(M.HrChecklistVocePutInModel);
    request.data.voce = { ...v, ...modifiche };
    this.service.ChecklistVocePut(request).subscribe(res => res.success ? this.aggiornaVoce(res.data.voce) : this.ricarica());
  }

  async caricaDocumento(v: M.HrChecklistVoceModel) {
    const file = await hrScegliFile('*/*');
    if (!file) return;
    this.service.ChecklistVoceDocUpload({ idVoce: v.id }, file).subscribe(res => { if (res.success) this.aggiornaVoce(res.data.voce); });
  }

  scaricaDocumento(v: M.HrChecklistVoceModel) {
    this.service.ChecklistVoceDocDownload(v.id);
  }

  async presaVisione(v: M.HrChecklistVoceModel) {
    if (v.dataRichiestaPresaVisione && !await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('hr.pages.checklistEdit.confirmRequestAgain')))
      return;
    const request = new GenericRequest<M.HrIdInModel>(M.HrIdInModel);
    request.data.id = v.id;
    this.service.ChecklistVocePresaVisione(request).subscribe(res => { if (res.success) this.aggiornaVoce(res.data.voce); });
  }

  async creaAccount(v: M.HrChecklistVoceModel) {
    const dati = await hrChiediAccount(this.alertCtrl, this.transloco, null);
    if (!dati || !this.dati) return;
    const request = new GenericRequest<M.HrAccountCreaInModel>(M.HrAccountCreaInModel);
    request.data = { ...request.data, idDip_Anagrafica: this.dati.checklist.idDip_Anagrafica, idHr_ChecklistVoce: v.id, ...dati };
    this.service.AccountCrea(request).subscribe(res => { if (res.success) this.ricarica(); });
  }

  async disattivaAccount(v: M.HrChecklistVoceModel) {
    if (!this.dati || !await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('hr.account.confirmDisable', { nome: this.dati.account.userName })))
      return;
    const request = new GenericRequest<M.HrAccountDisattivaInModel>(M.HrAccountDisattivaInModel);
    request.data = { idDip_Anagrafica: this.dati.checklist.idDip_Anagrafica, idHr_ChecklistVoce: v.id };
    this.service.AccountDisattiva(request).subscribe(res => { if (res.success) this.ricarica(); });
  }

  SaveData = (editModel: HrChecklistForm): Observable<boolean> => {
    const request = new GenericRequest<M.HrChecklistPutInModel>(M.HrChecklistPutInModel);
    request.data = { id: this.id, note: editModel.note, stato: editModel.annullata ? M.HrStatoChecklist.Annullata : M.HrStatoChecklist.Aperta };
    return this.service.ChecklistPut(request).pipe(map(res => res.success), catchError(() => of(false)));
  };
}
