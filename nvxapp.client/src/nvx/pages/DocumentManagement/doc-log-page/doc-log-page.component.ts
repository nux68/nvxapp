import { Component, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { DocumentManagementService } from '../../../ClientServer-Service/DocumentManagement/document-management.service';
import { DocLogListInModel, DocLogModel } from '../../../ClientServer-Service/DocumentManagement/Models/doc-model';

// Registro degli accessi ai documenti (CompanyPowerAdmin abilitato ai documenti del personale): ultimi 2000 eventi
@Component({
  selector: 'app-doc-log-page',
  templateUrl: './doc-log-page.component.html',
  styleUrls: ['./doc-log-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DocLogPageComponent {

  public title: string;
  public searchText!: string;
  public items: DocLogModel[] | null = null;
  public readonly actions: any[] = [];
  public readonly columns: NvxListColumn<DocLogModel>[];

  constructor(private service: DocumentManagementService, private transloco: TranslocoService) {
    this.title = this.transloco.translate('documents.pages.log.title');
    this.columns = [
      { key: 'data', title: this.transloco.translate('documents.fields.date'), mobile: 'note', value: x => x.data.replace('T', ' ').substring(0, 19) },
      { key: 'userName', title: this.transloco.translate('documents.fields.user'), mobile: 'title' },
      { key: 'azione', title: this.transloco.translate('documents.fields.action'), value: x => this.transloco.translate('documents.action.' + x.azione) },
      { key: 'dettaglio', title: this.transloco.translate('documents.fields.detail'), value: x => (x.versione ? 'v' + x.versione + ' ' : '') + (x.dettaglio ?? '') },
    ];
  }

  ionViewWillEnter() {
    this.service.LogList(new GenericRequest<DocLogListInModel>(DocLogListInModel)).subscribe(res => this.items = res.success && res.data ? res.data.log : []);
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
