import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AlertController, NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService, hrPerInput } from '../../../ClientServer-Service/HumanResources/hr.service';
import * as M from '../../../ClientServer-Service/HumanResources/Models/hr-model';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { hrChiediAccount } from '../hr-ui';

@Component({
  selector: 'app-hr-employee-edit-page',
  templateUrl: './hr-employee-edit-page.component.html',
  styleUrls: ['./hr-employee-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Dipendente in Risorse Umane (anagrafica comune Dip_Anagrafica): dati per cedolini e checklist (le date di assunzione e cessazione
 generano le checklist dai modelli predefiniti) e account del dipendente (creazione e disattivazione con i servizi degli utenti).
*/
export class HrEmployeeEditPageComponent extends BasePageConfirmCancelComponent<M.HrDipendenteModel> {

  public id = 0;
  public account = new M.HrAccountModel();

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: HrService,
              private alertCtrl: AlertController,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('hr.pages.employeeEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      cognome: [null, [Validators.required, Validators.maxLength(100)]],
      nome: [null, [Validators.maxLength(100)]],
      codiceFiscale: [null, [Validators.pattern(/^\s*([A-Za-z0-9]{16}|\d{11})\s*$/)]],
      matricola: [null, [Validators.maxLength(20)]],
      email: [null, [Validators.email, Validators.maxLength(100)]],
      dataAssunzione: [null],
      dataCessazione: [null],
      attivo: [true],
    });
  }

  LoadData = (): Observable<M.HrDipendenteModel | null> => {
    this.id = this.id || history.state?.id || 0;
    const request = new GenericRequest<M.HrIdInModel>(M.HrIdInModel);
    request.data.id = this.id;
    return this.service.DipendenteGet(request).pipe(map(res => {
      const d = res.success ? res.data.dipendente : null;
      if (d) {
        this.account = d.account;
        d.dataAssunzione = hrPerInput(d.dataAssunzione);
        d.dataCessazione = hrPerInput(d.dataCessazione);
      }
      return d;
    }), catchError(() => of(null)));
  };

  SaveData = (editModel: M.HrDipendenteModel): Observable<boolean> => {
    const request = new GenericRequest<M.HrDipendentePutInModel>(M.HrDipendentePutInModel);
    request.data.dipendente = { ...editModel, id: this.id, dataAssunzione: editModel.dataAssunzione || null, dataCessazione: editModel.dataCessazione || null };
    return this.service.DipendentePut(request).pipe(map(res => {
      if (res.success) this.id = res.data.dipendente.id;
      return res.success;
    }), catchError(() => of(false)));
  };

  async creaAccount() {
    const dati = await hrChiediAccount(this.alertCtrl, this.transloco, this._editForm.value.email);
    if (!dati) return;
    const request = new GenericRequest<M.HrAccountCreaInModel>(M.HrAccountCreaInModel);
    request.data = { ...request.data, idDip_Anagrafica: this.id, ...dati };
    this.service.AccountCrea(request).subscribe(res => { if (res.success) this.account = res.data.account; });
  }

  async disattivaAccount() {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('hr.account.confirmDisable', { nome: this.account.userName })))
      return;
    const request = new GenericRequest<M.HrAccountDisattivaInModel>(M.HrAccountDisattivaInModel);
    request.data.idDip_Anagrafica = this.id;
    this.service.AccountDisattiva(request).subscribe(res => { if (res.success) this.account = res.data.account; });
  }
}
