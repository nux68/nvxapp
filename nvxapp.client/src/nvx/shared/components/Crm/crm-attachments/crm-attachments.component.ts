import { Component, ChangeDetectionStrategy, EventEmitter, Input, Output } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { CrmService } from '../../../../ClientServer-Service/Crm/crm.service';
import { CrmAllegatoModel, CrmAllegatoUploadModel, CrmIdInModel } from '../../../../ClientServer-Service/Crm/Models/crm-model';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { CollectionDialogService } from '../../infrastructure/generic-dialog/collection-dialog.service';
import { crmDataOra } from '../../../../Utility/Crm/crm-setup.service';

// Allegati di un cliente, un'opportunita' o un'offerta (max 20 MB, lo ricontrolla il server)
@Component({
  selector: 'crm-attachments',
  templateUrl: './crm-attachments.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CrmAttachmentsComponent {

  @Input() allegati: CrmAllegatoModel[] = [];
  @Input() idAz_Cliente: number | null = null;
  @Input() idCrm_Opportunita: number | null = null;
  @Input() idCrm_Offerta: number | null = null;
  @Output() changed = new EventEmitter<void>();

  public caricamento = false;
  public readonly dataOra = crmDataOra;

  constructor(private service: CrmService, private collectionDialogService: CollectionDialogService, private transloco: TranslocoService) { }

  dimensione(b: number): string {
    return b >= 1048576 ? (b / 1048576).toFixed(1) + ' MB' : Math.max(1, Math.round(b / 1024)) + ' KB';
  }

  carica(ev: Event) {
    const input = ev.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file)
      return;
    const data = Object.assign(new CrmAllegatoUploadModel(), { idAz_Cliente: this.idAz_Cliente, idCrm_Opportunita: this.idCrm_Opportunita, idCrm_Offerta: this.idCrm_Offerta });
    this.caricamento = true;
    this.service.AllegatoUpload(data, file).subscribe({
      next: res => { this.caricamento = false; if (res.success) this.changed.emit(); },
      error: () => this.caricamento = false
    });
  }

  scarica(a: CrmAllegatoModel) {
    this.service.AllegatoDownload(a.id);
  }

  async elimina(a: CrmAllegatoModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('crm.attachments.confirmDelete', { nome: a.nomeFile })))
      return;
    const request = new GenericRequest<CrmIdInModel>(CrmIdInModel);
    request.data.id = a.id;
    this.service.AllegatoDelete(request).subscribe(res => { if (res.success) this.changed.emit(); });
  }
}
