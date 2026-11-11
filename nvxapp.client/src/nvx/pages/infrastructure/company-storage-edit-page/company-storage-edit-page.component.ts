import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { StorageService } from '../../../ClientServer-Service/Infrastructure/Storage/storage.service';
import { CompanyStorageModel, CompanyStoragePutInModel, bytesToMB } from '../../../ClientServer-Service/Infrastructure/Storage/Models/storage-model';

/*
 Spazio di un'azienda (SuperUser, DealerPowerAdmin del centro): soglia di avviso propria dell'azienda, facoltativa
 (vuota = predefinita). La quota non si cambia qui: e' quella della licenza Documentale (illimitata con le licenze spente,
 zero e caricamenti bloccati senza licenza).
*/
@Component({
  selector: 'app-company-storage-edit-page',
  templateUrl: './company-storage-edit-page.component.html',
  styleUrls: ['./company-storage-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CompanyStorageEditPageComponent extends BasePageConfirmCancelComponent<CompanyStorageModel> {

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private storageService: StorageService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('pages.companyStorageEdit.title'); }

  get Used(): string {
    const c = this._editModel;
    if (!c)
      return '';
    if (c.senzaLicenza)
      return this.transloco.translate('pages.companyStorageEdit.usedNoLicense', { used: bytesToMB(c.usedBytes), files: c.fileCount });
    return c.illimitato
      ? this.transloco.translate('pages.companyStorageEdit.usedUnlimited', { used: bytesToMB(c.usedBytes), files: c.fileCount })
      : this.transloco.translate('pages.companyStorageEdit.used', { used: bytesToMB(c.usedBytes), quota: c.quotaMB.toLocaleString(), percent: c.usedPercent, files: c.fileCount });
  }

  get EditForm(): FormGroup {
    return this.fb.group({
      warningPercentAzienda: [null, [Validators.min(1), Validators.max(99)]],
    });
  }

  LoadData = (): Observable<CompanyStorageModel | null> => {
    const item: CompanyStorageModel | undefined = history.state?.companyStorage;
    return of(item ?? null);
  };

  SaveData = (editModel: CompanyStorageModel): Observable<boolean> => {
    const request = new GenericRequest<CompanyStoragePutInModel>(CompanyStoragePutInModel);
    request.data = {
      idCompany: editModel.idCompany,
      warningPercent: editModel.warningPercentAzienda ? Number(editModel.warningPercentAzienda) : null
    };
    return this.storageService.CompanyStoragePut(request).pipe(
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
