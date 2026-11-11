import { Component, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { StorageService } from '../../../ClientServer-Service/Infrastructure/Storage/storage.service';
import { CompanyStorageGetInModel, CompanyStorageModel, bytesToMB } from '../../../ClientServer-Service/Infrastructure/Storage/Models/storage-model';
import { QUOTA_ILLIMITATA_MB } from '../../../shared/components/DocumentManagement/widgets/doc-widget-format';

// Spazio dei documenti dell'azienda (sola lettura): licenza Documentale da cui viene la quota (senza licenza illimitata),
// spazio usato, soglia di avviso.
@Component({
  selector: 'app-doc-storage-page',
  templateUrl: './doc-storage-page.component.html',
  styleUrls: ['./doc-storage-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DocStoragePageComponent {

  public storage: CompanyStorageModel | null = null;
  public readonly mb = bytesToMB;

  constructor(private storageService: StorageService, private transloco: TranslocoService) { }

  get Title(): string { return this.transloco.translate('documents.pages.storage.title'); }

  get Illimitato(): boolean { return (this.storage?.quotaMB ?? 0) >= QUOTA_ILLIMITATA_MB; }

  ionViewWillEnter() {
    this.storageService.CompanyStorageGet(new GenericRequest<CompanyStorageGetInModel>(CompanyStorageGetInModel))
      .subscribe(res => this.storage = res.success && res.data ? res.data.companyStorage : null);
  }
}
