import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DocumentManagementService } from '../../../ClientServer-Service/DocumentManagement/document-management.service';
import { DocCfgModel, DocCfgPutInModel, DocEmptyInModel } from '../../../ClientServer-Service/DocumentManagement/Models/doc-model';

// Impostazioni del documentale (CompanyPowerAdmin): cestino, versioni da conservare, avvisi email
@Component({
  selector: 'app-doc-settings-page',
  templateUrl: './doc-settings-page.component.html',
  styleUrls: ['./doc-settings-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DocSettingsPageComponent extends BasePageConfirmCancelComponent<DocCfgModel> {

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: DocumentManagementService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('documents.pages.settings.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      giorniCestino: [30, [Validators.required, Validators.min(0)]],
      versioniDaConservare: [0, [Validators.required, Validators.min(0)]],
      notificheEmail: [true],
    });
  }

  LoadData = (): Observable<DocCfgModel | null> =>
    this.service.SetupGet(new GenericRequest<DocEmptyInModel>(DocEmptyInModel)).pipe(map(res => res.data?.cfg ?? null), catchError(() => of(null)));

  SaveData = (editModel: DocCfgModel): Observable<boolean> => {
    const request = new GenericRequest<DocCfgPutInModel>(DocCfgPutInModel);
    request.data.cfg = { giorniCestino: Number(editModel.giorniCestino) || 0, versioniDaConservare: Number(editModel.versioniDaConservare) || 0, notificheEmail: !!editModel.notificheEmail };
    return this.service.CfgPut(request).pipe(
      map(res => res.success),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  // in caso di errore si resta sulla pagina
  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.back();
  }
}
