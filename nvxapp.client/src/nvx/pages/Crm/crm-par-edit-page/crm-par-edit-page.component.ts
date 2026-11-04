import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, ModalController, AlertController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, from, map, of, switchMap } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CrmService } from '../../../ClientServer-Service/Crm/crm.service';
import * as M from '../../../ClientServer-Service/Crm/Models/crm-model';
import { CrmSetupService, crmAdesso, crmData, crmDataOra, crmImporto, crmPerInputData, crmPerInputDataOra } from '../../../Utility/Crm/crm-setup.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

@Component({
  selector: 'app-crm-par-edit-page',
  templateUrl: './crm-par-edit-page.component.html',
  styleUrls: ['./crm-par-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Valore di una tabella del CRM: i campi dipendono dalla tabella
export class CrmParEditPageComponent extends BasePageConfirmCancelComponent<M.CrmParModel> {

  public tabella = M.CrmParTabella.Fase;
  public readonly tipiFase = [0, 1, 2];
  public readonly colori = ['primary', 'secondary', 'tertiary', 'success', 'warning', 'danger', 'medium'];

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: CrmService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('crm.enums.tabella.' + this.tabella); }

  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(100)]], ordine: [0], attivo: [true],
      probabilita: [0, [Validators.min(0), Validators.max(100)]], tipo: [0], icona: [null, [Validators.maxLength(50)]], colore: [null],
      durataMinuti: [30, [Validators.min(0)]], codice: [null, [Validators.maxLength(30)]], prezzo: [0], um: [null, [Validators.maxLength(10)]], aliquotaIva: [22, [Validators.min(0), Validators.max(100)]],
    });
  }

  LoadData = (): Observable<M.CrmParModel | null> => {
    const item: M.CrmParModel = history.state?.item ?? new M.CrmParModel();
    this.tabella = item.tabella;
    return of(item);
  };

  SaveData = (editModel: M.CrmParModel): Observable<boolean> => {
    const request = new GenericRequest<M.CrmParPutInModel>(M.CrmParPutInModel);
    request.data.par = { ...editModel, ordine: Number(editModel.ordine) || 0, probabilita: Number(editModel.probabilita) || 0, durataMinuti: Number(editModel.durataMinuti) || 0,
                         prezzo: Number(editModel.prezzo) || 0, aliquotaIva: Number(editModel.aliquotaIva) || 0 };
    return this.service.ParPut(request).pipe(map(res => res.success), catchError(() => of(false)));
  };

  override AfterSave(saved: boolean): void {
    if (saved) this.navCtrl.back();
  }
}
