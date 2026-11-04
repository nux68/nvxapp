import { Component, ChangeDetectionStrategy, EventEmitter, Input, Output } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { CrmService } from '../../../../ClientServer-Service/Crm/crm.service';
import { CrmIdInModel, CrmNotaModel, CrmNotaPutInModel } from '../../../../ClientServer-Service/Crm/Models/crm-model';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { CollectionDialogService } from '../../infrastructure/generic-dialog/collection-dialog.service';
import { crmDataOra } from '../../../../Utility/Crm/crm-setup.service';

// Note di un cliente, un'opportunita' o un lead: nuova nota in testa, fissate in alto, modifica dell'autore o di un amministratore
@Component({
  selector: 'crm-notes',
  templateUrl: './crm-notes.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CrmNotesComponent {

  @Input() note: CrmNotaModel[] = [];
  @Input() idAz_Cliente: number | null = null;
  @Input() idCrm_Opportunita: number | null = null;
  @Input() idCrm_Lead: number | null = null;
  @Output() changed = new EventEmitter<void>();

  public testo = '';
  public readonly dataOra = crmDataOra;

  constructor(private service: CrmService, private collectionDialogService: CollectionDialogService, private transloco: TranslocoService) { }

  aggiungi() {
    if (!this.testo.trim())
      return;
    const request = new GenericRequest<CrmNotaPutInModel>(CrmNotaPutInModel);
    request.data.nota = Object.assign(new CrmNotaModel(), { idAz_Cliente: this.idAz_Cliente, idCrm_Opportunita: this.idCrm_Opportunita, idCrm_Lead: this.idCrm_Lead, testo: this.testo });
    this.service.NotaPut(request).subscribe(res => {
      if (res.success) {
        this.testo = '';
        this.changed.emit();
      }
    });
  }

  fissa(n: CrmNotaModel) {
    const request = new GenericRequest<CrmNotaPutInModel>(CrmNotaPutInModel);
    request.data.nota = { ...n, fissata: !n.fissata };
    this.service.NotaPut(request).subscribe(res => { if (res.success) this.changed.emit(); });
  }

  async elimina(n: CrmNotaModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('crm.notes.confirmDelete')))
      return;
    const request = new GenericRequest<CrmIdInModel>(CrmIdInModel);
    request.data.id = n.id;
    this.service.NotaDelete(request).subscribe(res => { if (res.success) this.changed.emit(); });
  }
}
