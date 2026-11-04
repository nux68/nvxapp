import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, ModalController, AlertController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
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
  selector: 'app-crm-opportunity-edit-page',
  templateUrl: './crm-opportunity-edit-page.component.html',
  styleUrls: ['./crm-opportunity-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Opportunita': dati (la probabilita' segue la fase; motivo di perdita per le fasi perse), attivita', offerte, note, allegati, storico
export class CrmOpportunityEditPageComponent extends BasePageConfirmCancelComponent<M.CrmOpportunitaModel> {

  public sezione = 'dati';
  public scheda: M.CrmOpportunitaGetOutModel | null = null;
  public clienti: M.CrmClienteListModel[] = [];
  public contatti: M.CrmContattoModel[] = [];
  private id = 0;
  public readonly data = crmData;
  public readonly dataOra = crmDataOra;
  public readonly colAttivita: NvxListColumn<M.CrmAttivitaListModel>[];
  public readonly azAttivita: NvxListAction<M.CrmAttivitaListModel>[];
  public readonly colOfferte: NvxListColumn<M.CrmOffertaListModel>[];
  public readonly azOfferte: NvxListAction<M.CrmOffertaListModel>[];

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: CrmService,
              public setupService: CrmSetupService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    const t = (k: string) => this.transloco.translate(k);
    this.colAttivita = [
      { key: 'inizio', title: t('crm.fields.quando'), mobile: 'note', value: x => crmDataOra(x.inizio), sortValue: x => x.inizio },
      { key: 'oggetto', title: t('crm.fields.oggetto'), mobile: 'title', value: x => x.tipo + ': ' + x.oggetto },
      { key: 'stato', title: t('crm.fields.stato'), mobile: 'subtitle', value: x => t('crm.enums.statoAttivita.' + x.stato) },
    ];
    const b1 = userInterfaceService.Btn_Apri; b1.event = (x: M.CrmAttivitaListModel) => this.navCtrl.navigateForward('/crmactivityedit', { state: { id: x.id } });
    this.azAttivita = [{ button: b1 }];
    this.colOfferte = [
      { key: 'numeroCompleto', title: t('crm.fields.numero'), mobile: 'title' },
      { key: 'data', title: t('crm.fields.data'), value: x => crmData(x.data), sortValue: x => x.data },
      { key: 'stato', title: t('crm.fields.stato'), mobile: 'subtitle', value: x => t('crm.enums.statoOfferta.' + x.stato) },
      { key: 'totale', title: t('crm.fields.totale'), align: 'end', mobile: 'note', value: x => this.importo(x.totale), sortValue: x => x.totale },
    ];
    const b2 = userInterfaceService.Btn_Apri; b2.event = (x: M.CrmOffertaListModel) => this.navCtrl.navigateForward('/crmofferedit', { state: { id: x.id } });
    this.azOfferte = [{ button: b2 }];
  }

  importo(v: number | null): string { return crmImporto(v ?? 0, this.setupService.setup?.cfg.valuta); }

  get Title(): string { return this.transloco.translate('crm.pages.opportunityEdit.title'); }

  get EditForm(): FormGroup {
    const f = this.fb.group({
      titolo: [null, [Validators.required, Validators.maxLength(200)]], idAz_Cliente: [null, [Validators.required]], idAz_ClienteContatto: [null],
      idCrm_Par_Fase: [null, [Validators.required]], valore: [0, [Validators.min(0)]], probabilita: [0, [Validators.min(0), Validators.max(100)]],
      dataChiusuraPrevista: [null], dataChiusura: [null], idResponsabile: [null], idCrm_Par_MotivoPerdita: [null], idCrm_Par_Fonte: [null], note: [null, [Validators.maxLength(2000)]],
    });
    // la probabilita' proposta segue la fase; i contatti seguono il cliente
    f.get('idCrm_Par_Fase')!.valueChanges.subscribe(id => {
      const fase = this.setupService.setup?.fasi.find(x => x.id === id);
      if (fase && f.dirty) f.get('probabilita')!.setValue(fase.probabilita);
    });
    f.get('idAz_Cliente')!.valueChanges.subscribe(id => this.caricaContatti(id));
    return f;
  }

  get faseTipo(): M.CrmFaseTipo {
    return this.setupService.setup?.fasi.find(x => x.id === this._editForm?.get('idCrm_Par_Fase')?.value)?.tipo ?? M.CrmFaseTipo.Aperta;
  }

  LoadData = (): Observable<M.CrmOpportunitaModel | null> => {
    this.id = this.id || history.state?.id || 0;
    const preset = history.state?.preset ?? {};
    return from(this.setupService.load()).pipe(switchMap(setup => {
      this.caricaClienti();
      if (!this.id) {
        this.scheda = null;
        const prima = (setup?.fasi ?? []).filter(f => f.attivo && f.tipo === M.CrmFaseTipo.Aperta).sort((a, b) => a.ordine - b.ordine)[0];
        const nuova = Object.assign(new M.CrmOpportunitaModel(), { idCrm_Par_Fase: prima?.id ?? null, probabilita: prima?.probabilita ?? 0 }, preset);
        if (setup && !setup.permessi.powerAdmin && !setup.permessi.admin)
          nuova.idResponsabile = setup.permessi.idUtente;
        return of(nuova);
      }
      return this.carica();
    }));
  };

  private carica(): Observable<M.CrmOpportunitaModel | null> {
    const request = new GenericRequest<M.CrmIdInModel>(M.CrmIdInModel);
    request.data.id = this.id;
    return this.service.OpportunitaGet(request).pipe(map(res => {
      this.scheda = res.success ? res.data : null;
      if (!this.scheda) return null;
      this.contatti = this.scheda.contatti;
      const o = this.scheda.opportunita;
      return { ...o, dataChiusuraPrevista: crmPerInputData(o.dataChiusuraPrevista), dataChiusura: crmPerInputData(o.dataChiusura) };
    }), catchError(() => of(null)));
  }

  ricarica() { this.carica().subscribe(); }

  private caricaClienti() {
    const r = new GenericRequest<M.CrmClienteListInModel>(M.CrmClienteListInModel);
    this.service.ClienteList(r).subscribe(res => this.clienti = res.success ? res.data.clienti : []);
  }

  // clienti proponibili: attivi (piu' quello gia' scelto)
  get clientiScelta(): M.CrmClienteListModel[] {
    const scelto = this._editForm?.get('idAz_Cliente')?.value;
    return this.clienti.filter(c => (c.attivo && c.tipo !== 1) || c.id === scelto);
  }

  private caricaContatti(idCliente: number | null) {
    if (!idCliente) { this.contatti = []; return; }
    const r = new GenericRequest<M.CrmIdInModel>(M.CrmIdInModel);
    r.data.id = idCliente;
    this.service.ClienteGet(r).subscribe(res => this.contatti = res.success ? res.data.contatti : []);
  }

  SaveData = (editModel: M.CrmOpportunitaModel): Observable<boolean> => {
    const request = new GenericRequest<M.CrmOpportunitaPutInModel>(M.CrmOpportunitaPutInModel);
    request.data.opportunita = { ...editModel, id: this.id, valore: Number(editModel.valore) || 0, probabilita: Number(editModel.probabilita) || 0,
                                 dataChiusuraPrevista: editModel.dataChiusuraPrevista || null, dataChiusura: editModel.dataChiusura || null };
    return this.service.OpportunitaPut(request).pipe(map(res => res.success), catchError(() => of(false)));
  };

  override AfterSave(saved: boolean): void {
    if (saved) this.navCtrl.back();
  }

  async elimina() {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('crm.pages.opportunityEdit.confirmDelete')))
      return;
    const r = new GenericRequest<M.CrmIdInModel>(M.CrmIdInModel);
    r.data.id = this.id;
    this.service.OpportunitaDelete(r).subscribe(res => { if (res.success) this.navCtrl.back(); });
  }

  nuova(pagina: string) {
    this.navCtrl.navigateForward(pagina, { state: { id: 0, preset: { idAz_Cliente: this.scheda?.opportunita.idAz_Cliente, idCrm_Opportunita: this.id } } });
  }
}
