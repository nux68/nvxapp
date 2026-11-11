import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService } from '../../../ClientServer-Service/HumanResources/hr.service';
import * as M from '../../../ClientServer-Service/HumanResources/Models/hr-model';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

@Component({
  selector: 'app-hr-settings-page',
  templateUrl: './hr-settings-page.component.html',
  styleUrls: ['./hr-settings-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Impostazioni di Risorse Umane (CompanyPowerAdmin): preavviso delle scadenze, notifiche, note
export class HrSettingsPageComponent extends BasePageConfirmCancelComponent<M.HrCfgModel> {

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: HrService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('hr.pages.settings.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      giorniPreavviso: [30, [Validators.required, Validators.min(0), Validators.max(365)]],
      notificheAttive: [true],
      note: [null, [Validators.maxLength(2000)]],
    });
  }

  LoadData = (): Observable<M.HrCfgModel> => {
    const request = new GenericRequest<M.HrEmptyInModel>(M.HrEmptyInModel);
    return this.service.CfgGet(request).pipe(map(res => res.data?.cfg ?? new M.HrCfgModel()), catchError(() => of(new M.HrCfgModel())));
  };

  SaveData = (editModel: M.HrCfgModel): Observable<boolean> => {
    const request = new GenericRequest<M.HrCfgPutInModel>(M.HrCfgPutInModel);
    request.data.cfg = { ...editModel, giorniPreavviso: Number(editModel.giorniPreavviso) || 0 };
    return this.service.CfgPut(request).pipe(map(res => res.success), catchError(() => of(false)));
  };

  override AfterSave(saved: boolean): void {
    if (saved) this.navCtrl.back();
  }
}
