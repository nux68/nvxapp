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
  selector: 'app-crm-settings-page',
  templateUrl: './crm-settings-page.component.html',
  styleUrls: ['./crm-settings-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Impostazioni del CRM (CompanyPowerAdmin): visibilita' dei commerciali, valuta, promemoria, offerte
export class CrmSettingsPageComponent extends BasePageConfirmCancelComponent<M.CrmCfgModel> {

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: CrmService,
              private setupService: CrmSetupService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('crm.pages.settings.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      visibilita: [0], valuta: ['EUR', [Validators.required, Validators.maxLength(3)]], minutiPreavviso: [30, [Validators.min(0)]],
      offertaPrefisso: [null, [Validators.maxLength(10)]], offertaGiorniValidita: [30, [Validators.min(0)]], offertaNote: [null, [Validators.maxLength(2000)]],
    });
  }

  LoadData = (): Observable<M.CrmCfgModel | null> => from(this.setupService.load()).pipe(map(s => s ? { ...s.cfg } : null));

  SaveData = (editModel: M.CrmCfgModel): Observable<boolean> => {
    const request = new GenericRequest<M.CrmCfgPutInModel>(M.CrmCfgPutInModel);
    request.data.cfg = { ...editModel, minutiPreavviso: Number(editModel.minutiPreavviso) || 0, offertaGiorniValidita: Number(editModel.offertaGiorniValidita) || 0 };
    return this.service.CfgPut(request).pipe(map(res => res.success), catchError(() => of(false)));
  };

  override AfterSave(saved: boolean): void {
    if (saved) this.navCtrl.back();
  }
}
