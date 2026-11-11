import { Component, ChangeDetectionStrategy, TemplateRef, ViewChild } from '@angular/core';
import { NavController, ToastController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { MonthNavigatorService } from '../../../Utility/infrastructure/month-navigator.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { PaymentService } from '../../../ClientServer-Service/Infrastructure/Payment/payment.service';
import {
  PaymentDealerGetInModel, PaymentDealerGetOutModel, PaymentDealerSummaryModel, PaymentEmailLogModel, PaymentExportInModel, PaymentRowModel,
  PaymentSettlementCancelInModel, PaymentSettlementConfirmInModel, PaymentSettlementGenerateInModel, PaymentSettlementInvoicePutInModel,
  PaymentSettlementModel, PaymentSettlementPayInModel, PaymentSummarySendInModel, StatoRendiconto
} from '../../../ClientServer-Service/Infrastructure/Payment/Models/payment-model';

/*
 Pagamenti di un centro (ANALISI_INFRASTRUCTURE_PAGAMENTI_CENTRI.md §4, §6): SuperUser (centro scelto dal riepilogo o dal filtro) e utenti
 del centro (sempre il proprio, lo decide il server; le bozze non si vedono).
    - totali del periodo, rendiconti dell'anno, pagamenti del periodo, email inviate
    - SuperUser: genera la bozza del mese, conferma (parte l'email al centro), liquida (data e riferimento del bonifico), annulla
    - DealerPowerAdmin e SuperUser: fattura del centro sui rendiconti confermati o liquidati, dati di liquidazione
    - tutti: riepilogo via email, PDF ed Excel del periodo
*/
@Component({
  selector: 'app-payment-dealer-page',
  templateUrl: './payment-dealer-page.component.html',
  styleUrls: ['./payment-dealer-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class PaymentDealerPageComponent {

  @ViewChild('payDialog', { static: true }) payDialog!: TemplateRef<any>;
  @ViewChild('invoiceDialog', { static: true }) invoiceDialog!: TemplateRef<any>;

  public readonly StatoRendiconto = StatoRendiconto;
  public title: string;
  public anni: number[] = [];
  public anno = new Date().getFullYear();
  /** 0 = tutto l'anno */
  public mese = 0;
  public idDealer: number | null = null;
  public data: PaymentDealerGetOutModel | null = null;
  public searchPayments!: string;

  public readonly settlementColumns: NvxListColumn<PaymentSettlementModel>[];
  public readonly settlementActions: NvxListAction<PaymentSettlementModel>[];
  public readonly paymentColumns: NvxListColumn<PaymentRowModel>[];
  // incassati: Completato (2) e Debug (5), entrambi nel cumulo; nascosti finche' non si chiede i rimborsati
  public readonly isPaymentDisabled = (p: PaymentRowModel) => p.stato !== 2 && p.stato !== 5;

  // campi dei dialoghi di liquidazione e fattura
  public payDate = '';
  public payReference = '';
  public payNote = '';
  public invoiceNumber = '';
  public invoiceDate = '';

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              public monthNavigator: MonthNavigatorService,
              private paymentService: PaymentService,
              private collectionDialogService: CollectionDialogService,
              private toastController: ToastController,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.paymentDealer.title');
    const oggi = new Date().getFullYear();
    for (let a = oggi; a >= oggi - 5; a--)
      this.anni.push(a);

    const t = (k: string) => this.transloco.translate('pages.paymentDealer.' + k);
    const euro = (v: number) => v.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    const data = (v: string | null) => v ? new Date(v).toLocaleDateString() : '';

    this.settlementColumns = [
      { key: 'mese', title: t('columnMonth'), mobile: 'title', value: r => this.monthNavigator.monthNames[r.mese - 1] + ' ' + r.anno, sortValue: r => r.anno * 100 + r.mese },
      { key: 'stato', title: t('columnState'), mobile: 'subtitle', value: r => this.transloco.translate('pages.payment.settlementState' + r.stato) },
      { key: 'numeroPagamenti', title: t('columnPayments'), align: 'end' },
      { key: 'totaleIncassato', title: t('columnCollected'), align: 'end', value: r => euro(r.totaleIncassato - r.totaleRettifiche), sortValue: r => r.totaleIncassato },
      { key: 'quotaPiattaforma', title: t('columnFee'), align: 'end', value: r => euro(r.quotaPiattaforma) + ' (' + r.percentualePiattaforma + '%)', sortValue: r => r.quotaPiattaforma },
      { key: 'quotaCentro', title: t('columnDealerShare'), align: 'end', mobile: 'note', value: r => euro(r.quotaCentro), sortValue: r => r.quotaCentro },
      { key: 'dataLiquidazione', title: t('columnPaidOn'), value: r => data(r.dataLiquidazione) + (r.riferimentoBonifico ? ' · ' + r.riferimentoBonifico : ''), sortValue: r => r.dataLiquidazione ?? '' },
      { key: 'numeroFatturaCentro', title: t('columnInvoice'), value: r => (r.numeroFatturaCentro ?? '') + (r.dataFatturaCentro ? ' · ' + data(r.dataFatturaCentro) : '') },
    ];

    const btnConfirm = this.button(userInterfaceService.Btn_Approva, t('confirm'), this.confirm);
    const btnPay = this.button(userInterfaceService.Btn_Esegui, t('pay'), this.pay);
    btnPay.image = 'cash-outline';
    const btnInvoice = this.button(userInterfaceService.Btn_Modifica, t('invoice'), this.invoice);
    btnInvoice.image = 'receipt-outline';
    const btnCancel = this.button(userInterfaceService.Btn_Cancella, t('cancel'), this.cancel);
    this.settlementActions = [
      { button: btnCancel, visible: r => !!this.data?.isSuperUser && (r.stato === StatoRendiconto.Bozza || r.stato === StatoRendiconto.Confermato) },
      { button: btnInvoice, visible: r => !!this.data?.canEditDealerData && (r.stato === StatoRendiconto.Confermato || r.stato === StatoRendiconto.Liquidato) },
      { button: btnPay, visible: r => !!this.data?.isSuperUser && r.stato === StatoRendiconto.Confermato },
      { button: btnConfirm, visible: r => !!this.data?.isSuperUser && r.stato === StatoRendiconto.Bozza },
    ];

    this.paymentColumns = [
      { key: 'dataPagamento', title: t('columnDate'), value: p => data(p.dataPagamento), sortValue: p => p.dataPagamento ?? '' },
      { key: 'soggetto', title: t('columnCustomer'), mobile: 'title',
        value: p => (p.soggetto ?? '') + ' (' + this.transloco.translate('pages.license.subject' + p.tipoSoggetto) + ')' },
      { key: 'ambito', title: t('columnFunction'), mobile: 'subtitle',
        value: p => this.transloco.translate('pages.license.function' + p.ambito) + ' ' + this.transloco.translate('pages.license.level' + p.livello) },
      { key: 'periodo', title: t('columnPeriod'), value: p => data(p.dataInizioCoperta) + ' - ' + data(p.dataFineCoperta), sortValue: p => p.dataInizioCoperta },
      { key: 'importo', title: t('columnAmount'), align: 'end', mobile: 'note', value: p => euro(p.importo) + ' ' + p.valuta, sortValue: p => p.importo },
      { key: 'stato', title: t('columnPaymentState'), value: p => this.transloco.translate('pages.license.paymentState' + p.stato) + (p.debug ? ' (debug)' : '') },
      { key: 'statoRendiconto', title: t('columnSettlement'),
        value: p => p.statoRendiconto === null ? t('notSettled') : this.transloco.translate('pages.payment.settlementState' + p.statoRendiconto) },
    ];
  }

  private button(base: ButtonItem, label: string, handler: (r: PaymentSettlementModel) => void): ButtonItem {
    base.text = label;
    base.event = handler;
    return base;
  }

  ionViewWillEnter() {
    const state = history.state ?? {};
    if (state.idDealer)
      this.idDealer = state.idDealer;
    if (state.anno)
      this.anno = state.anno;
    if (state.mese !== undefined && state.mese !== null)
      this.mese = state.mese;
    this.load();
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  get riepilogo(): PaymentDealerSummaryModel | null { return this.data?.riepilogo ?? null; }

  get periodo(): string {
    return this.mese > 0 ? this.monthNavigator.monthNames[this.mese - 1] + ' ' + this.anno : String(this.anno);
  }

  /** il mese scelto e' concluso: si puo' generare il rendiconto */
  get meseConcluso(): boolean {
    if (this.mese <= 0)
      return false;
    const oggi = new Date();
    return this.anno < oggi.getFullYear() || (this.anno === oggi.getFullYear() && this.mese < oggi.getMonth() + 1);
  }

  load() {
    const request = new GenericRequest<PaymentDealerGetInModel>(PaymentDealerGetInModel);
    request.data.idDealer = this.idDealer;
    request.data.anno = this.anno;
    request.data.mese = this.mese > 0 ? this.mese : null;
    this.paymentService.PaymentDealerGet(request).subscribe(res => {
      this.data = res.success && res.data ? res.data : null;
      if (this.data && this.idDealer === null && this.data.riepilogo.idDealer !== null)
        this.idDealer = this.data.riepilogo.idDealer;
      this.title = this.transloco.translate('pages.paymentDealer.title') + (this.data?.riepilogo.dealer ? ' - ' + this.data.riepilogo.dealer : '');
      this.fab();
    });
  }

  private fab() {
    const fab: FabMenuItem[] = [];
    if (!this.data) {
      this.fabMenuService.fabMenuItem = fab;
      return;
    }
    if (this.data.isSuperUser && this.meseConcluso && (!this.riepilogo?.rendiconto || this.riepilogo.rendiconto.stato === StatoRendiconto.Bozza))
      fab.push(new FabMenuItem(this.transloco.translate('pages.paymentDealer.generate'), 'document-text-outline', () => this.generate()));
    if (this.data.emailConfigurata)
      fab.push(new FabMenuItem(this.transloco.translate('pages.paymentDealer.sendSummary'), 'mail-outline', () => this.sendSummary()));
    fab.push(new FabMenuItem(this.transloco.translate('pages.payment.pdf'), 'document-outline', () => this.export('pdf')));
    fab.push(new FabMenuItem(this.transloco.translate('pages.payment.excel'), 'grid-outline', () => this.export('xlsx')));
    fab.push(new FabMenuItem(this.transloco.translate('pages.paymentDealer.dealerData'), 'card-outline', () =>
      this.navCtrl.navigateForward('/paymentdealerconfig', { state: { idDealer: this.idDealer } })));
    this.fabMenuService.fabMenuItem = fab;
  }

  private async toast(key: string, params: any = {}) {
    const toast = await this.toastController.create({ message: this.transloco.translate(key, params), duration: 3000, position: 'bottom' });
    await toast.present();
  }

  private generate() {
    const request = new GenericRequest<PaymentSettlementGenerateInModel>(PaymentSettlementGenerateInModel);
    request.data.anno = this.anno;
    request.data.mese = this.mese;
    request.data.idDealer = this.idDealer;
    this.paymentService.PaymentSettlementGenerate(request).subscribe(res => {
      if (res.success)
        this.toast('pages.paymentDealer.generated');
      this.load();
    });
  }

  private async sendSummary() {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('pages.paymentDealer.confirmSend', { periodo: this.periodo })))
      return;
    const request = new GenericRequest<PaymentSummarySendInModel>(PaymentSummarySendInModel);
    request.data.idDealer = this.idDealer;
    request.data.anno = this.anno;
    request.data.mese = this.mese > 0 ? this.mese : null;
    this.paymentService.PaymentSummarySend(request).subscribe(res => {
      if (res.success)
        this.toast('pages.paymentDealer.sent', { destinatari: res.data?.destinatari ?? '' });
      this.load();
    });
  }

  private export(formato: 'pdf' | 'xlsx') {
    const request = new GenericRequest<PaymentExportInModel>(PaymentExportInModel);
    request.data.idDealer = this.idDealer;
    request.data.anno = this.anno;
    request.data.mese = this.mese > 0 ? this.mese : null;
    request.data.formato = formato;
    // il file si prepara con un job: lo si scarica dalla barra dei job quando e' pronto
    this.paymentService.PaymentExport(request).subscribe(res => {
      if (res.success)
        this.toast('pages.payment.exportStarted');
    });
  }

  confirm = async (r: PaymentSettlementModel) => {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('pages.paymentDealer.confirmConfirm')))
      return;
    const request = new GenericRequest<PaymentSettlementConfirmInModel>(PaymentSettlementConfirmInModel);
    request.data.id = r.id;
    this.paymentService.PaymentSettlementConfirm(request).subscribe(res => {
      if (res.success)
        this.toast(res.data?.emailInviata ? 'pages.paymentDealer.confirmedSent' : 'pages.paymentDealer.confirmedNotSent');
      this.load();
    });
  }

  pay = async (r: PaymentSettlementModel) => {
    this.payDate = new Date().toISOString().substring(0, 10);
    this.payReference = '';
    this.payNote = '';
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.payDialog))
      return;
    const request = new GenericRequest<PaymentSettlementPayInModel>(PaymentSettlementPayInModel);
    request.data.id = r.id;
    request.data.dataLiquidazione = this.payDate;
    request.data.riferimentoBonifico = this.payReference || null;
    request.data.note = this.payNote || null;
    this.paymentService.PaymentSettlementPay(request).subscribe(res => {
      if (res.success)
        this.toast(res.data?.emailInviata ? 'pages.paymentDealer.paidSent' : 'pages.paymentDealer.paidNotSent');
      this.load();
    });
  }

  invoice = async (r: PaymentSettlementModel) => {
    this.invoiceNumber = r.numeroFatturaCentro ?? '';
    this.invoiceDate = r.dataFatturaCentro ? r.dataFatturaCentro.substring(0, 10) : new Date().toISOString().substring(0, 10);
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.invoiceDialog))
      return;
    const request = new GenericRequest<PaymentSettlementInvoicePutInModel>(PaymentSettlementInvoicePutInModel);
    request.data.id = r.id;
    request.data.numeroFatturaCentro = this.invoiceNumber || null;
    request.data.dataFatturaCentro = this.invoiceNumber && this.invoiceDate ? this.invoiceDate : null;
    this.paymentService.PaymentSettlementInvoicePut(request).subscribe(() => this.load());
  }

  cancel = async (r: PaymentSettlementModel) => {
    const key = r.stato === StatoRendiconto.Bozza ? 'pages.paymentDealer.confirmDeleteDraft' : 'pages.paymentDealer.confirmCancel';
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate(key)))
      return;
    const request = new GenericRequest<PaymentSettlementCancelInModel>(PaymentSettlementCancelInModel);
    request.data.id = r.id;
    this.paymentService.PaymentSettlementCancel(request).subscribe(() => this.load());
  }

  emailLabel(i: PaymentEmailLogModel): string {
    const periodo = i.mese ? this.monthNavigator.monthNames[i.mese - 1] + ' ' + i.anno : String(i.anno);
    return this.transloco.translate('pages.payment.emailType' + i.tipo) + ' ' + periodo;
  }

  FilterPayments(CurrFilter: any) {
    this.searchPayments = CurrFilter;
  }
}
