import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { StorageService } from '../../../ClientServer-Service/Infrastructure/Storage/storage.service';
import { CompanyStorageModel, CompanyStoragePutInModel, StoragePlanModel, bytesToMB } from '../../../ClientServer-Service/Infrastructure/Storage/Models/storage-model';

/*
 Spazio di un'azienda (SuperUser, DealerPowerAdmin del centro): pacchetto e, facoltativi, dimensione massima del file
 e soglia di avviso propri dell'azienda (vuoti = del pacchetto / predefiniti). Un pacchetto piu' piccolo dello spazio
 gia' usato e' consentito: l'azienda non carica finche' non scende sotto (avviso dal server).
*/
@Component({
  selector: 'app-company-storage-edit-page',
  templateUrl: './company-storage-edit-page.component.html',
  styleUrls: ['./company-storage-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CompanyStorageEditPageComponent extends BasePageConfirmCancelComponent<CompanyStorageModel> {

  public plans: StoragePlanModel[] = history.state?.plans ?? [];

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
    return c ? this.transloco.translate('pages.companyStorageEdit.used', { used: bytesToMB(c.usedBytes), quota: c.quotaMB.toLocaleString(), percent: c.usedPercent, files: c.fileCount }) : '';
  }

  get EditForm(): FormGroup {
    return this.fb.group({
      idStoragePlan: [null, [Validators.required]],
      maxFileMBAzienda: [null, [Validators.min(1)]],
      warningPercentAzienda: [null, [Validators.min(1), Validators.max(99)]],
    });
  }

  LoadData = (): Observable<CompanyStorageModel | null> => {
    const item: CompanyStorageModel | undefined = history.state?.companyStorage;
    // il pacchetto attuale resta selezionabile anche se disattivato
    if (item && !this.plans.some(p => p.id === item.idStoragePlan))
      this.plans = [...this.plans, { ...new StoragePlanModel(), id: item.idStoragePlan, descrizione: item.pacchetto, quotaMB: item.quotaMB, active: false }];
    return of(item ?? null);
  };

  SaveData = (editModel: CompanyStorageModel): Observable<boolean> => {
    const request = new GenericRequest<CompanyStoragePutInModel>(CompanyStoragePutInModel);
    request.data = {
      idCompany: editModel.idCompany,
      idStoragePlan: Number(editModel.idStoragePlan),
      maxFileMB: editModel.maxFileMBAzienda ? Number(editModel.maxFileMBAzienda) : null,
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
