import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { LicenseService } from '../../../ClientServer-Service/Infrastructure/License/license.service';
import {
  LicenseDealerModel, LicenseSettingsGetInModel, LicenseSettingsModel, LicenseSettingsPutInModel
} from '../../../ClientServer-Service/Infrastructure/License/Models/license-model';

/*
 Impostazioni delle licenze (SuperUser, ANALISI_INFRASTRUCTURE_LICENZE.md §3.3, §5.1, §8.1.1):
    - sistema licenze attivo (spento = nessun blocco, tutto come Pro)
    - utenti predefiniti dei piani Free, Standard e Pro dei nuovi listini (vuoto = illimitati)
    - giorni di grace dopo la scadenza e giorni di preavviso
    - centri abilitati a definire i propri listini
*/
@Component({
  selector: 'app-license-settings-page',
  templateUrl: './license-settings-page.component.html',
  styleUrls: ['./license-settings-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class LicenseSettingsPageComponent extends BasePageConfirmCancelComponent<LicenseSettingsModel> {

  public dealer: LicenseDealerModel[] = [];
  public debugPagamenti = false;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private licenseService: LicenseService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('pages.licenseSettings.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      attive: [false],
      // 0 o vuoto = illimitati
      maxUtentiFree: [null, [Validators.min(0)]],
      maxUtentiStandard: [null, [Validators.min(0)]],
      maxUtentiPro: [null, [Validators.min(0)]],
      maxClientiFree: [null, [Validators.min(0)]],
      maxClientiStandard: [null, [Validators.min(0)]],
      maxClientiPro: [null, [Validators.min(0)]],
      maxSpazioMBFree: [null, [Validators.min(0)]],
      maxSpazioMBStandard: [null, [Validators.min(0)]],
      maxSpazioMBPro: [null, [Validators.min(0)]],
      graceGiorni: [7, [Validators.required, Validators.min(0), Validators.max(365)]],
      avvisoGiorni: [10, [Validators.required, Validators.min(0), Validators.max(365)]],
    });
  }

  LoadData = (): Observable<LicenseSettingsModel | null> =>
    this.licenseService.LicenseSettingsGet(new GenericRequest<LicenseSettingsGetInModel>(LicenseSettingsGetInModel)).pipe(
      map(res => {
        if (!res.success || !res.data)
          return null;
        this.dealer = res.data.dealer ?? [];
        this.debugPagamenti = res.data.debugPagamenti;
        return res.data.settings;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(null);
      })
    );

  SaveData = (editModel: LicenseSettingsModel): Observable<boolean> => {
    // 0 o vuoto = illimitati
    const numero = (v: any) => (v === null || v === undefined || v === '' || Number(v) <= 0) ? null : Number(v);
    const request = new GenericRequest<LicenseSettingsPutInModel>(LicenseSettingsPutInModel);
    request.data.settings = {
      attive: !!editModel.attive,
      maxUtentiFree: numero(editModel.maxUtentiFree),
      maxUtentiStandard: numero(editModel.maxUtentiStandard),
      maxUtentiPro: numero(editModel.maxUtentiPro),
      maxClientiFree: numero(editModel.maxClientiFree),
      maxClientiStandard: numero(editModel.maxClientiStandard),
      maxClientiPro: numero(editModel.maxClientiPro),
      maxSpazioMBFree: numero(editModel.maxSpazioMBFree),
      maxSpazioMBStandard: numero(editModel.maxSpazioMBStandard),
      maxSpazioMBPro: numero(editModel.maxSpazioMBPro),
      graceGiorni: Number(editModel.graceGiorni) || 0,
      avvisoGiorni: Number(editModel.avvisoGiorni) || 0
    };
    request.data.dealer = this.dealer;
    return this.licenseService.LicenseSettingsPut(request).pipe(
      map(res => res.success),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.back();
  }
}
