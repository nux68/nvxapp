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
import { CrmOfferSendDialogComponent } from '../../../shared/components/Crm/crm-offer-send-dialog/crm-offer-send-dialog.component';

@Component({
  selector: 'app-crm-offer-edit-page',
  templateUrl: './crm-offer-edit-page.component.html',
  styleUrls: ['./crm-offer-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Offerta (preventivo): testata (cliente, contatto, opportunita', date, stato, oggetto, note) e righe (prodotto, quantita', prezzo,
 sconto, IVA). I totali mostrati sono un'anteprima: li ricalcola il server. Salvando si resta sulla pagina (per PDF e invio).
 PDF con un job (barra dei job); l'esito "accettata" chiude la scadenza e porta l'opportunita' collegata in una fase vinta (server).
*/
export class CrmOfferEditPageComponent extends BasePageConfirmCancelComponent<M.CrmOffertaModel> {

  private id = 0;
  public sezione = 'dati';
  public scheda: M.CrmOffertaGetOutModel | null = null;
  public clienti: M.CrmClienteListModel[] = [];
  public contatti: M.CrmContattoModel[] = [];
  public opportunitaCliente: M.CrmOpportunitaListModel[] = [];
  public righe: M.CrmOffertaRigaModel[] = [];
  public readonly stati = [0, 1, 2, 3, 4];
  private btnPdf: ButtonItem;
  private btnInvia: ButtonItem;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: CrmService,
              public setupService: CrmSetupService,
              private modalCtrl: ModalController,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    this.btnPdf = userInterfaceService.Btn_Pdf;
    this.btnPdf.event = () => this.setupService.pdfOfferta(this.id);
    this.btnInvia = userInterfaceService.Btn_Invia;
    this.btnInvia.event = () => this.invia();
  }

  get Title(): string {
    return this.transloco.translate('crm.pages.offerEdit.title') + (this.scheda ? ' ' + this.scheda.offerta.numeroCompleto : '');
  }

  importo(v: number): string { return crmImporto(v, this.setupService.setup?.cfg.valuta); }

  get EditForm(): FormGroup {
    const f = this.fb.group({
      idAz_Cliente: [null, [Validators.required]], idAz_ClienteContatto: [null], idCrm_Opportunita: [null],
      data: [null, [Validators.required]], validaFinoAl: [null], stato: [0], oggetto: [null, [Validators.maxLength(200)]], note: [null, [Validators.maxLength(4000)]],
    });
    f.get('idAz_Cliente')!.valueChanges.subscribe(id => this.caricaCliente(id));
    return f;
  }

  LoadData = (): Observable<M.CrmOffertaModel | null> => {
    this.id = this.id || history.state?.id || 0;
    const preset = history.state?.preset ?? {};
    return from(this.setupService.load()).pipe(switchMap(() => {
      const r = new GenericRequest<M.CrmClienteListInModel>(M.CrmClienteListInModel);
      this.service.ClienteList(r).subscribe(res => this.clienti = res.success ? res.data.clienti : []);
      if (!this.id) {
        this.scheda = null;
        this.righe = [new M.CrmOffertaRigaModel()];
        this.pulsanti();
        return of(Object.assign(new M.CrmOffertaModel(), { data: new Date().toISOString().substring(0, 10) }, preset));
      }
      return this.carica();
    }));
  };

  private carica(): Observable<M.CrmOffertaModel | null> {
    const request = new GenericRequest<M.CrmIdInModel>(M.CrmIdInModel);
    request.data.id = this.id;
    return this.service.OffertaGet(request).pipe(map(res => {
      this.scheda = res.success ? res.data : null;
      this.pulsanti();
      if (!this.scheda) return null;
      this.contatti = this.scheda.contatti;
      this.opportunitaCliente = this.scheda.opportunitaCliente;
      this.righe = this.scheda.offerta.righe.map(r => ({ ...r }));
      const o = this.scheda.offerta;
      return { ...o, data: crmPerInputData(o.data)!, validaFinoAl: crmPerInputData(o.validaFinoAl) };
    }), catchError(() => of(null)));
  }

  ricarica() { this.carica().subscribe(); }

  // PDF e invio solo per un'offerta salvata
  private pulsanti() {
    const base = this.buttonbar.filter(b => b !== this.btnPdf && b !== this.btnInvia);
    this.buttonbar = this.id ? [this.btnPdf, this.btnInvia, ...base] : base;
  }

  get clientiScelta(): M.CrmClienteListModel[] {
    const scelto = this._editForm?.get('idAz_Cliente')?.value;
    return this.clienti.filter(c => (c.attivo && c.tipo !== 1) || c.id === scelto);
  }

  private caricaCliente(idCliente: number | null) {
    if (!idCliente) { this.contatti = []; this.opportunitaCliente = []; return; }
    const r = new GenericRequest<M.CrmIdInModel>(M.CrmIdInModel);
    r.data.id = idCliente;
    this.service.ClienteGet(r).subscribe(res => {
      this.contatti = res.success ? res.data.contatti : [];
      this.opportunitaCliente = res.success ? res.data.opportunita : [];
    });
  }

  // ---------------------------------------------------------------- righe
  prodotto(r: M.CrmOffertaRigaModel) {
    const p = this.setupService.setup?.prodotti.find(x => x.id === r.idCrm_Par_Prodotto);
    if (p) {
      r.descrizione = p.descrizione;
      r.prezzo = p.prezzo;
      r.um = p.um;
      r.aliquotaIva = p.aliquotaIva;
    }
  }

  aggiungiRiga() { this.righe.push(new M.CrmOffertaRigaModel()); }

  eliminaRiga(i: number) { this.righe.splice(i, 1); }

  importoRiga(r: M.CrmOffertaRigaModel): number {
    return Math.round((Number(r.quantita) || 0) * (Number(r.prezzo) || 0) * (1 - (Number(r.scontoPerc) || 0) / 100) * 100) / 100;
  }

  get imponibile(): number { return this.righe.reduce((t, r) => t + this.importoRiga(r), 0); }
  get iva(): number { return this.righe.reduce((t, r) => t + Math.round(this.importoRiga(r) * (Number(r.aliquotaIva) || 0)) / 100, 0); }

  SaveData = (editModel: M.CrmOffertaModel): Observable<boolean> => {
    const request = new GenericRequest<M.CrmOffertaPutInModel>(M.CrmOffertaPutInModel);
    request.data.offerta = {
      ...editModel, id: this.id, validaFinoAl: editModel.validaFinoAl || null,
      righe: this.righe.map(r => ({ ...r, quantita: Number(r.quantita) || 0, prezzo: Number(r.prezzo) || 0, scontoPerc: Number(r.scontoPerc) || 0, aliquotaIva: Number(r.aliquotaIva) || 0 }))
    };
    return this.service.OffertaPut(request).pipe(map(res => {
      if (res.success) this.id = res.data.offerta.id;
      return res.success;
    }), catchError(() => of(false)));
  };

  override AfterSave(saved: boolean): void {
    if (saved) this.ricarica();
  }

  async invia() {
    const contatto = this.contatti.find(c => c.id === this._editForm.get('idAz_ClienteContatto')?.value);
    const cliente = this.clienti.find(c => c.id === this._editForm.get('idAz_Cliente')?.value);
    const modal = await this.modalCtrl.create({ component: CrmOfferSendDialogComponent, componentProps: { idOfferta: this.id, email: contatto?.email || cliente?.email || '' } });
    await modal.present();
    const { role } = await modal.onDidDismiss();
    if (role === 'confirm') this.ricarica();
  }

  async elimina() {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('crm.pages.offerEdit.confirmDelete')))
      return;
    const r = new GenericRequest<M.CrmIdInModel>(M.CrmIdInModel);
    r.data.id = this.id;
    this.service.OffertaDelete(r).subscribe(res => { if (res.success) this.navCtrl.back(); });
  }
}
