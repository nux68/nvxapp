import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, IonSpinner } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { StorageService } from '../../../../../ClientServer-Service/Infrastructure/Storage/storage.service';
import { CompanyStorageGetInModel, CompanyStorageModel } from '../../../../../ClientServer-Service/Infrastructure/Storage/Models/storage-model';
import { formatBytes, QUOTA_ILLIMITATA_MB } from '../doc-widget-format';

/*
 Widget "Spazio documenti" (Documentale, ANALISI_WIDGET_DOCUMENTALE.md): spazio occupato e disponibile dell'azienda,
 dalla quota della licenza Documentale, illimitata senza licenza (CompanyStorageGet, Infrastructure).
*/
@Component({
  selector: 'app-doc-storage-widget',
  templateUrl: './doc-storage-widget.component.html',
  styleUrls: ['./doc-storage-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe]
})
export class DocStorageWidgetComponent {

  public caricamento = true;
  public storage: CompanyStorageModel | null = null;
  public readonly bytes = formatBytes;

  constructor(private storageService: StorageService, private navCtrl: NavController) {
    this.storageService.CompanyStorageGet(new GenericRequest<CompanyStorageGetInModel>(CompanyStorageGetInModel)).subscribe({
      next: res => {
        this.storage = res.success && res.data ? res.data.companyStorage : null;
        this.caricamento = false;
      },
      error: () => this.caricamento = false
    });
  }

  public get illimitato(): boolean {
    return (this.storage?.quotaMB ?? 0) >= QUOTA_ILLIMITATA_MB;
  }

  public get quotaBytes(): number {
    return (this.storage?.quotaMB ?? 0) * 1024 * 1024;
  }

  public get disponibiliBytes(): number {
    return Math.max(0, this.quotaBytes - (this.storage?.usedBytes ?? 0));
  }

  /** percentuale per la barra (0-100) */
  public get percentuale(): number {
    if (!this.storage || this.illimitato || this.quotaBytes <= 0)
      return 0;
    return Math.min(100, Math.round(this.storage.usedBytes * 100 / this.quotaBytes));
  }

  public dettagli(): void {
    this.navCtrl.navigateForward('/docstorage');
  }
}
