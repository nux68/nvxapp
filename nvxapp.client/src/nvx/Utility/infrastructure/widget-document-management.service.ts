import { Injectable } from '@angular/core';
import { iWidgetProvider, WidgetDefinition } from './widget.model';
import { DocStorageWidgetComponent } from '../../shared/components/DocumentManagement/widgets/doc-storage-widget/doc-storage-widget.component';
import { DocTypeSpaceWidgetComponent } from '../../shared/components/DocumentManagement/widgets/doc-type-space-widget/doc-type-space-widget.component';

// chi amministra l'archivio (come le voci di menu "Archivio" e "Spazio documenti")
const RUOLI_ARCHIVIO = ['CompanyPowerAdmin', 'CompanyAdmin'];

/*
 Widget della dashboard del Documentale (ANALISI_INFRASTRUCTURE_WIDGET.md, dettaglio in ANALISI_DOCUMENTMANAGEMENT_WIDGET.md).
 Compaiono solo se il documentale e' attivo per l'azienda (WidgetService).
*/
@Injectable({
  providedIn: 'root'
})
export class WidgetDocumentManagementService implements iWidgetProvider {
  public get Widgets(): WidgetDefinition[] {
    return [
      {
        key: 'documents.storage',
        title: 'documents.widgets.storage.title',
        description: 'documents.widgets.storage.description',
        icon: 'server-outline',
        roles: RUOLI_ARCHIVIO,
        size: 'small',
        default: true,
        zorder: 400100,
        component: DocStorageWidgetComponent
      },
      {
        key: 'documents.typeSpace',
        title: 'documents.widgets.typeSpace.title',
        description: 'documents.widgets.typeSpace.description',
        icon: 'pie-chart-outline',
        roles: RUOLI_ARCHIVIO,
        size: 'medium',
        default: true,
        zorder: 400200,
        component: DocTypeSpaceWidgetComponent
      }
    ];
  }
}
