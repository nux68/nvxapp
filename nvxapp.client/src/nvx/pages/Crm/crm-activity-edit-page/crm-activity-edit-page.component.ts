import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, ModalController, AlertController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, from, map, of, switchMap } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CrmService } from '../../../ClientServer-Service/Crm/crm.service';
import * as M from '../../../ClientServer-Service/Crm/Models/crm-model';
import { CrmSetupService, crmAdesso, crmData, crmDataOra, crmImporto, crmPerInputData, crmPerInputDataOra } from '../../../Utility/Crm/crm-setup.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

@Component({
  selector: 'app-crm-activity-edit-page',
  templateUrl: './crm-activity-edit-page.component.html',
  styleUrls: ['./crm-activity-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Attivita': collegata a un cliente (contatto, opportunita') o a un lead; assegnatario e promemoria
export class CrmActivityEditPageComponent extends BasePageConfirmCancelComponent<M.CrmAttivitaModel> {

  private id = 0;
  public clienti: M.CrmClienteListModel[] = [];
  public contatti: M.CrmContattoModel[] = [];
  public opportunitaCliente: M.CrmOpportunitaListModel[] = [];
  public readonly stati = [0, 1, 2];

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: CrmService,
              public setupService: CrmSetupService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService,
              private route: ActivatedRoute) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('crm.pages.activityEdit.title'); }

  get EditForm(): FormGroup {
    const f = this.fb.group({
      idCrm_Par_TipoAttivita: [null, [Validators.required]], oggetto: [null, [Validators.required, Validators.maxLength(200)]], descrizione: [null, [Validators.maxLength(4000)]],
      inizio: [null, [Validators.required]], fine: [null], idAssegnatario: [null], stato: [0], esito: [null, [Validators.maxLength(2000)]],
      idAz_Cliente: [null], idAz_ClienteContatto: [null], idCrm_Opportunita: [null], idCrm_Lead: [null],
    });
    f.get('idAz_Cliente')!.valueChanges.subscribe(id => this.caricaCliente(id));
    // fine proposta con la durata del tipo
    f.get('idCrm_Par_TipoAttivita')!.valueChanges.subscribe(id => {
      const tipo = this.setupService.setup?.tipiAttivita.find(x => x.id === id);
      const inizio = f.get('inizio')!.value;
      if (tipo && inizio && !f.get('fine')!.value && tipo.durataMinuti > 0) {
        const d = new Date(new Date(inizio).getTime() + tipo.durataMinuti * 60000);
        const p = (n: number) => String(n).padStart(2, '0');
        f.get('fine')!.setValue(`${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`);
      }
    });
    return f;
  }

  LoadData = (): Observable<M.CrmAttivitaModel | null> => {
    // id dallo stato della navigazione o dall'indirizzo (link degli eventi del calendario, CRM-CAL.1)
    this.id = this.id || history.state?.id || Number(this.route.snapshot.queryParamMap.get('id')) || 0;
    const preset = history.state?.preset ?? {};
    return from(this.setupService.load()).pipe(switchMap(setup => {
      const r = new GenericRequest<M.CrmClienteListInModel>(M.CrmClienteListInModel);
      this.service.ClienteList(r).subscribe(res => this.clienti = res.success ? res.data.clienti : []);
      if (!this.id) {
        const tipo = (setup?.tipiAttivita ?? []).filter(t => t.attivo).sort((a, b) => a.ordine - b.ordine)[0];
        return of(Object.assign(new M.CrmAttivitaModel(), { inizio: crmAdesso(60 - new Date().getMinutes()), idCrm_Par_TipoAttivita: tipo?.id ?? null,
                                                            idAssegnatario: setup?.permessi.idUtente ?? null }, preset));
      }
      const request = new GenericRequest<M.CrmIdInModel>(M.CrmIdInModel);
      request.data.id = this.id;
      return this.service.AttivitaGet(request).pipe(map(res => {
        if (!res.success) return null;
        this.contatti = res.data.contatti;
        this.opportunitaCliente = res.data.opportunitaCliente;
        const a = res.data.attivita;
        return { ...a, inizio: crmPerInputDataOra(a.inizio)!, fine: crmPerInputDataOra(a.fine) };
      }), catchError(() => of(null)));
    }));
  };

  get clientiScelta(): M.CrmClienteListModel[] {
    const scelto = this._editForm?.get('idAz_Cliente')?.value;
    return this.clienti.filter(c => c.attivo || c.id === scelto);
  }

  get daLead(): boolean { return !!this._editForm?.get('idCrm_Lead')?.value; }

  private caricaCliente(idCliente: number | null) {
    if (!idCliente) { this.contatti = []; this.opportunitaCliente = []; return; }
    const r = new GenericRequest<M.CrmIdInModel>(M.CrmIdInModel);
    r.data.id = idCliente;
    this.service.ClienteGet(r).subscribe(res => {
      this.contatti = res.success ? res.data.contatti : [];
      this.opportunitaCliente = res.success ? res.data.opportunita.filter(o => o.faseTipo === M.CrmFaseTipo.Aperta || o.id === this._editForm.get('idCrm_Opportunita')?.value) : [];
    });
  }

  SaveData = (editModel: M.CrmAttivitaModel): Observable<boolean> => {
    const request = new GenericRequest<M.CrmAttivitaPutInModel>(M.CrmAttivitaPutInModel);
    request.data.attivita = { ...editModel, id: this.id, fine: editModel.fine || null };
    return this.service.AttivitaPut(request).pipe(map(res => res.success), catchError(() => of(false)));
  };

  override AfterSave(saved: boolean): void {
    if (saved) this.navCtrl.back();
  }

  async elimina() {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('crm.pages.activityEdit.confirmDelete')))
      return;
    const r = new GenericRequest<M.CrmIdInModel>(M.CrmIdInModel);
    r.data.id = this.id;
    this.service.AttivitaDelete(r).subscribe(res => { if (res.success) this.navCtrl.back(); });
  }

  get esistente(): boolean { return this.id > 0; }
}
