import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../ClientServer-Service/Assets/ast.service';
import * as M from '../../../ClientServer-Service/Assets/Models/ast-model';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

@Component({
  selector: 'app-ast-settings-page',
  templateUrl: './ast-settings-page.component.html',
  styleUrls: ['./ast-settings-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Impostazioni di Risorse aziendali (CompanyPowerAdmin): preavviso delle scadenze, notifiche, limiti delle prenotazioni, note
export class AstSettingsPageComponent extends BasePageConfirmCancelComponent<M.AstCfgModel> {

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: AstService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('ast.pages.settings.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      giorniPreavviso: [30, [Validators.required, Validators.min(0), Validators.max(365)]],
      notificheAttive: [true],
      prenotazioneDurataMaxOre: [10, [Validators.required, Validators.min(0), Validators.max(744)]],
      prenotazioneAnticipoMaxGiorni: [60, [Validators.required, Validators.min(0), Validators.max(730)]],
      note: [null, [Validators.maxLength(2000)]],
    });
  }

  LoadData = (): Observable<M.AstCfgModel> => {
    const request = new GenericRequest<M.AstEmptyInModel>(M.AstEmptyInModel);
    return this.service.CfgGet(request).pipe(map(res => res.data?.cfg ?? new M.AstCfgModel()), catchError(() => of(new M.AstCfgModel())));
  };

  SaveData = (editModel: M.AstCfgModel): Observable<boolean> => {
    const request = new GenericRequest<M.AstCfgPutInModel>(M.AstCfgPutInModel);
    request.data.cfg = { ...editModel, giorniPreavviso: Number(editModel.giorniPreavviso) || 0,
                         prenotazioneDurataMaxOre: Number(editModel.prenotazioneDurataMaxOre) || 0,
                         prenotazioneAnticipoMaxGiorni: Number(editModel.prenotazioneAnticipoMaxGiorni) || 0 };
    return this.service.CfgPut(request).pipe(map(res => res.success), catchError(() => of(false)));
  };

  override AfterSave(saved: boolean): void {
    if (saved) this.navCtrl.back();
  }
}
