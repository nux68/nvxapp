import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { StorageService } from '../../../ClientServer-Service/Infrastructure/Storage/storage.service';
import { StorageSettingsGetInModel, StorageSettingsModel, StorageSettingsPutInModel } from '../../../ClientServer-Service/Infrastructure/Storage/Models/storage-model';

// Valori predefiniti dello spazio (SuperUser): dimensione massima del file e soglia di avviso
@Component({
  selector: 'app-storage-settings-page',
  templateUrl: './storage-settings-page.component.html',
  styleUrls: ['./storage-settings-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class StorageSettingsPageComponent extends BasePageConfirmCancelComponent<StorageSettingsModel> {

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private storageService: StorageService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('pages.storageSettings.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      maxFileMB: [50, [Validators.required, Validators.min(1)]],
      warningPercent: [80, [Validators.required, Validators.min(1), Validators.max(99)]],
    });
  }

  LoadData = (): Observable<StorageSettingsModel | null> =>
    this.storageService.StorageSettingsGet(new GenericRequest<StorageSettingsGetInModel>(StorageSettingsGetInModel)).pipe(
      map(res => res.data?.settings ?? null),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(null);
      })
    );

  SaveData = (editModel: StorageSettingsModel): Observable<boolean> => {
    const request = new GenericRequest<StorageSettingsPutInModel>(StorageSettingsPutInModel);
    request.data.settings = { maxFileMB: Number(editModel.maxFileMB) || 0, warningPercent: Number(editModel.warningPercent) || 0 };
    return this.storageService.StorageSettingsPut(request).pipe(
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
