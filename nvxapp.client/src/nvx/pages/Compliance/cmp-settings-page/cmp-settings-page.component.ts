import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CmpService } from '../../../ClientServer-Service/Compliance/cmp.service';
import * as M from '../../../ClientServer-Service/Compliance/Models/cmp-model';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

@Component({
  selector: 'app-cmp-settings-page',
  templateUrl: './cmp-settings-page.component.html',
  styleUrls: ['./cmp-settings-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Impostazioni di Compliance (CompanyPowerAdmin): preavviso delle scadenze, notifiche, note
export class CmpSettingsPageComponent extends BasePageConfirmCancelComponent<M.CmpCfgModel> {

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: CmpService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('cmp.pages.settings.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      giorniPreavviso: [30, [Validators.required, Validators.min(0), Validators.max(365)]],
      notificheAttive: [true],
      note: [null, [Validators.maxLength(2000)]],
    });
  }

  LoadData = (): Observable<M.CmpCfgModel> => {
    const request = new GenericRequest<M.CmpEmptyInModel>(M.CmpEmptyInModel);
    return this.service.CfgGet(request).pipe(map(res => res.data?.cfg ?? new M.CmpCfgModel()), catchError(() => of(new M.CmpCfgModel())));
  };

  SaveData = (editModel: M.CmpCfgModel): Observable<boolean> => {
    const request = new GenericRequest<M.CmpCfgPutInModel>(M.CmpCfgPutInModel);
    request.data.cfg = { ...editModel, giorniPreavviso: Number(editModel.giorniPreavviso) || 0 };
    return this.service.CfgPut(request).pipe(map(res => res.success), catchError(() => of(false)));
  };

  override AfterSave(saved: boolean): void {
    if (saved) this.navCtrl.back();
  }
}
