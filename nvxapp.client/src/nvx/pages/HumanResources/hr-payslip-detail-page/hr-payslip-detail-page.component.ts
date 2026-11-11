import { Component, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { ModalController, NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Subscription, filter } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService, hrData } from '../../../ClientServer-Service/HumanResources/hr.service';
import * as M from '../../../ClientServer-Service/HumanResources/Models/hr-model';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { LongJobNotifierService } from '../../../Utility/infrastructure/long-job-notifier.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { hrPeriodo } from '../hr-payslip-list-page/hr-payslip-list-page.component';
import { hrScegliDipendenti } from '../hr-ui';

@Component({
  selector: 'app-hr-payslip-detail-page',
  templateUrl: './hr-payslip-detail-page.component.html',
  styleUrls: ['./hr-payslip-detail-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Dettaglio di un caricamento (HR-CED.2-4): documenti divisi per dipendente con lo stato della presa visione (report delle
 conferme), pagine da verificare (assegnazione a un dipendente o scarto), pubblicazione nell'area dei dipendenti (job).
*/
export class HrPayslipDetailPageComponent {

  public dati: M.HrCaricamentoGetOutModel | null = null;
  public sezione = 'documenti';
  public buttonbar: ButtonItem[] = [];
  public readonly data = hrData;
  public readonly colDocumenti: NvxListColumn<M.HrDocDipendenteModel>[];
  public readonly azDocumenti: NvxListAction<M.HrDocDipendenteModel>[];
  public readonly colScarti: NvxListColumn<M.HrDocScartoModel>[];
  public readonly azScarti: NvxListAction<M.HrDocScartoModel>[];
  public readonly scartoChiuso = (x: M.HrDocScartoModel) => x.esito !== 0;
  private id = 0;
  private idCompany: number | null = null;
  private studio = false;
  private jobs?: Subscription;
  private readonly btnPubblica: ButtonItem;
  private readonly btnCancella: ButtonItem;
  private readonly btnElenco: ButtonItem;

  constructor(private navCtrl: NavController,
              private router: Router,
              private modalCtrl: ModalController,
              private userInterfaceService: UserInterfaceService,
              private service: HrService,
              private collectionDialogService: CollectionDialogService,
              private longJobNotifierService: LongJobNotifierService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.colDocumenti = [
      { key: 'dipendente', title: t('hr.fields.dipendente'), mobile: 'title' },
      { key: 'codiceFiscale', title: t('hr.fields.codiceFiscale'), mobile: 'hidden', value: x => x.codiceFiscale ?? '' },
      { key: 'pagine', title: t('hr.fields.pagine'), mobile: 'note' },
      { key: 'metodo', title: t('hr.fields.metodo'), mobile: 'hidden', value: x => t('hr.enums.metodo.' + x.metodo) },
      { key: 'presaVisione', title: t('hr.fields.presaVisione'), mobile: 'subtitle', value: x => this.statoPresaVisione(x) },
    ];
    const bPdf = userInterfaceService.Btn_Pdf;
    bPdf.event = (x: M.HrDocDipendenteModel) => this.service.DocDownload(x.id, this.idCompany);
    this.azDocumenti = [{ button: bPdf }];
    this.colScarti = [
      { key: 'pagina', title: t('hr.fields.pagina'), mobile: 'title', value: x => t('hr.fields.pagina') + ' ' + x.pagina, sortValue: x => x.pagina },
      { key: 'motivo', title: t('hr.fields.motivo'), mobile: 'subtitle', value: x => x.esito === 0 ? (x.motivo ?? '') : t('hr.enums.esitoScarto.' + x.esito) + (x.dipendente ? ': ' + x.dipendente : '') },
      { key: 'proposta', title: t('hr.fields.proposta'), mobile: 'note', value: x => x.esito === 0 ? (x.proposta ?? '') : '' },
      { key: 'testo', title: t('hr.fields.testo'), mobile: 'hidden', value: x => (x.testo ?? '').substring(0, 120) },
    ];
    const bAssegna = userInterfaceService.Btn_Assegna;
    bAssegna.event = (x: M.HrDocScartoModel) => this.assegna(x);
    const bIgnora = userInterfaceService.Btn_Rifiuta;
    bIgnora.text = t('hr.pages.payslipDetail.ignore');
    bIgnora.event = (x: M.HrDocScartoModel) => this.ignora(x);
    this.azScarti = [{ button: bIgnora, visible: x => x.esito === 0 }, { button: bAssegna, visible: x => x.esito === 0 }];

    this.btnPubblica = userInterfaceService.Btn_Invia;
    this.btnPubblica.text = t('hr.pages.payslipDetail.publish');
    this.btnPubblica.image = 'paper-plane-outline';
    this.btnPubblica.event = () => this.pubblica();
    this.btnCancella = userInterfaceService.Btn_Cancella;
    this.btnCancella.event = () => this.elimina();
    this.btnElenco = userInterfaceService.Btn_Elenco;
    this.btnElenco.event = () => this.navCtrl.back();
  }

  get titolo(): string {
    const c = this.dati?.caricamento;
    return c ? hrPeriodo(this.transloco, c.tipo, c.anno, c.mese) : this.transloco.translate('hr.pages.payslipDetail.title');
  }

  get daVerificare(): number { return this.dati?.scarti.filter(x => x.esito === 0).length ?? 0; }

  ionViewWillEnter() {
    this.id = history.state?.id ?? this.id;
    this.idCompany = history.state?.idCompany ?? this.idCompany;
    this.studio = this.router.url.startsWith('/hrstudio');
    this.load();
    this.jobs = this.longJobNotifierService.jobFinished$.pipe(filter(j => j.jobType?.startsWith('Hr_Cedolini') && j.payload?.id === this.id)).subscribe(() => this.load());
  }

  ionViewWillLeave() {
    this.jobs?.unsubscribe();
  }

  load() {
    const request = new GenericRequest<M.HrCedIdInModel>(M.HrCedIdInModel);
    request.data = { id: this.id, idCompany: this.idCompany };
    this.service.CaricamentoGet(request).subscribe(res => this.mostra(res.success ? res.data : null));
  }

  private mostra(d: M.HrCaricamentoGetOutModel | null) {
    this.dati = d;
    const stato = d?.caricamento.stato;
    const bar: ButtonItem[] = [];
    if (stato === M.HrStatoCaricamento.DaPubblicare || stato === M.HrStatoCaricamento.DaVerificare) bar.push(this.btnPubblica);
    if (stato === M.HrStatoCaricamento.DaPubblicare || stato === M.HrStatoCaricamento.DaVerificare || stato === M.HrStatoCaricamento.Errore) bar.push(this.btnCancella);
    bar.push(this.btnElenco);
    this.buttonbar = bar;
    if (d && this.sezione === 'scarti' && d.scarti.length === 0) this.sezione = 'documenti';
  }

  private statoPresaVisione(x: M.HrDocDipendenteModel): string {
    const t = (k: string) => this.transloco.translate(k);
    if (!x.dataPubblicazione) return t('hr.pages.payslipDetail.notPublished');
    if (x.senzaUtente) return t('hr.pages.payslipDetail.noUser');
    if (x.dataConferma) return t('hr.pages.payslipDetail.confirmed') + ' ' + hrData(x.dataConferma);
    return t('hr.pages.payslipDetail.waiting');
  }

  async assegna(x: M.HrDocScartoModel) {
    const scelti = await hrScegliDipendenti(this.modalCtrl, this.dati?.dipendenti ?? [], 'single', x.idDip_AnagraficaProposta ? [x.idDip_AnagraficaProposta] : []);
    if (!scelti || scelti.length === 0) return;
    const request = new GenericRequest<M.HrScartoAssegnaInModel>(M.HrScartoAssegnaInModel);
    request.data = { idCompany: this.idCompany, idScarto: x.id, idDip_Anagrafica: scelti[0] };
    this.service.ScartoAssegna(request).subscribe(res => { if (res.success) this.mostra(res.data); });
  }

  ignora(x: M.HrDocScartoModel) {
    const request = new GenericRequest<M.HrCedIdInModel>(M.HrCedIdInModel);
    request.data = { idCompany: this.idCompany, id: x.id };
    this.service.ScartoIgnora(request).subscribe(res => { if (res.success) this.mostra(res.data); });
  }

  async pubblica() {
    const messaggio = this.daVerificare > 0 ? 'hr.pages.payslipDetail.confirmPublishPending' : 'hr.pages.payslipDetail.confirmPublish';
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate(messaggio, { n: this.daVerificare })))
      return;
    const request = new GenericRequest<M.HrCedIdInModel>(M.HrCedIdInModel);
    request.data = { idCompany: this.idCompany, id: this.id };
    this.service.CaricamentoPubblica(request).subscribe();
  }

  async elimina() {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('hr.pages.payslipDetail.confirmDelete')))
      return;
    const request = new GenericRequest<M.HrCedIdInModel>(M.HrCedIdInModel);
    request.data = { idCompany: this.idCompany, id: this.id };
    this.service.CaricamentoDelete(request).subscribe(res => { if (res.success) this.navCtrl.back(); });
  }
}
