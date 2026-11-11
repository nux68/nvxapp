import { Component, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DocumentManagementService } from '../../../ClientServer-Service/DocumentManagement/document-management.service';
import { DocAbilitazioneModel, DocAbilitazionePutInModel, DocEmptyInModel } from '../../../ClientServer-Service/DocumentManagement/Models/doc-model';

/*
 Abilitazioni ai documenti del personale (ANALISI_DOCUMENTMANAGEMENT_APPLICATIVO.md §4): per ogni CompanyAdmin "vede" e "carica" (chi
 carica vede anche); senza abilitazione non vede i documenti del personale. Il CompanyPowerAdmin e' sempre abilitato
 (riga fissa). Le gestiscono il CompanyPowerAdmin, il SuperUser e il DealerPowerAdmin del centro.
 Ogni interruttore si salva subito (come la pagina delle impostazioni AI dei tipi di ticket).
*/
@Component({
  selector: 'app-doc-permission-page',
  templateUrl: './doc-permission-page.component.html',
  styleUrls: ['./doc-permission-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DocPermissionPageComponent {

  public items: DocAbilitazioneModel[] | null = null;

  constructor(private service: DocumentManagementService, private transloco: TranslocoService) { }

  get Title(): string { return this.transloco.translate('documents.pages.permissions.title'); }

  ionViewWillEnter() {
    this.service.AbilitazioneList(new GenericRequest<DocEmptyInModel>(DocEmptyInModel)).subscribe(res => this.items = res.success && res.data ? res.data.abilitazioni : []);
  }

  set(item: DocAbilitazioneModel, campo: 'vedePersonale' | 'caricaPersonale', valore: boolean) {
    const request = new GenericRequest<DocAbilitazionePutInModel>(DocAbilitazionePutInModel);
    request.data.abilitazione = { ...item, [campo]: valore };
    if (campo === 'vedePersonale' && !valore) request.data.abilitazione.caricaPersonale = false;
    this.service.AbilitazionePut(request).subscribe(res => {
      if (res.success && res.data) Object.assign(item, res.data.abilitazione);
      else this.ionViewWillEnter();
    });
  }
}
