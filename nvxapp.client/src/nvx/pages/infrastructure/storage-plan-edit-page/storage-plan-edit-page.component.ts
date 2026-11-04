import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { StorageService } from '../../../ClientServer-Service/Infrastructure/Storage/storage.service';
import { StoragePlanModel, StoragePlanPutInModel } from '../../../ClientServer-Service/Infrastructure/Storage/Models/storage-model';

// Pacchetto di spazio (SuperUser): descrizione, spazio, dimensione massima del file (vuoto = predefinita), ordine, attivo, predefinito
@Component({
  selector: 'app-storage-plan-edit-page',
  templateUrl: './storage-plan-edit-page.component.html',
  styleUrls: ['./storage-plan-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class StoragePlanEditPageComponent extends BasePageConfirmCancelComponent<StoragePlanModel> {

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private storageService: StorageService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('pages.storagePlanEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      quotaMB: [1024, [Validators.required, Validators.min(1)]],
      maxFileMB: [null, [Validators.min(1)]],
      ordine: [0],
      active: [true],
      predefinito: [false],
    });
  }

  // il pacchetto arriva dall'elenco (nuovo: valori iniziali)
  LoadData = (): Observable<StoragePlanModel | null> => of(history.state?.plan ?? new StoragePlanModel());

  SaveData = (editModel: StoragePlanModel): Observable<boolean> => {
    const request = new GenericRequest<StoragePlanPutInModel>(StoragePlanPutInModel);
    request.data.plan = {
      ...editModel,
      quotaMB: Number(editModel.quotaMB) || 0,
      maxFileMB: editModel.maxFileMB ? Number(editModel.maxFileMB) : null,
      ordine: Number(editModel.ordine) || 0
    };
    return this.storageService.StoragePlanPut(request).pipe(
      map(res => res.success),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  // in caso di errore (es. descrizione gia' usata) si resta sulla pagina
  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.back();
  }
}
