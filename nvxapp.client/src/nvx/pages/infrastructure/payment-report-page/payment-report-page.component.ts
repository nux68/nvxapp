import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, ToastController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { MonthNavigatorService } from '../../../Utility/infrastructure/month-navigator.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { PaymentService } from '../../../ClientServer-Service/Infrastructure/Payment/payment.service';
import {
  PaymentDealerSummaryModel, PaymentExportInModel, PaymentReportGetInModel, PaymentSettlementGenerateInModel, PaymentTotalsModel
} from '../../../ClientServer-Service/Infrastructure/Payment/Models/payment-model';

/*
 Pagamenti per centro (SuperUser, ANALISI_INFRASTRUCTURE_PAGAMENTI_CENTRI.md §4, §6): per anno o mese, incassato, trattenuta della piattaforma,
 quota dei centri, liquidato e da liquidare; dal dettaglio di un centro rendiconti, pagamenti ed email.
 Dal pulsante rotondo: bozze dei rendiconti del mese per tutti i centri, Excel di tutti i centri, impostazioni.
*/
@Component({
  selector: 'app-payment-report-page',
  templateUrl: './payment-report-page.component.html',
  styleUrls: ['./payment-report-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class PaymentReportPageComponent {

  public title: string;
  public searchText!: string;
  public anni: number[] = [];
  public anno = new Date().getFullYear();
  /** 0 = tutto l'anno */
  public mese = 0;
  public centri: PaymentDealerSummaryModel[] | null = null;
  public totali = new PaymentTotalsModel();
  public debugPagamenti = false;
  public columns: NvxListColumn<PaymentDealerSummaryModel>[] = [];
  public readonly actions: NvxListAction<PaymentDealerSummaryModel>[];
  public readonly isDisabled = (item: PaymentDealerSummaryModel) => item.numeroPagamenti === 0 && item.liquidato === 0 && item.daLiquidare === 0;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              public monthNavigator: MonthNavigatorService,
              private paymentService: PaymentService,
              private collectionDialogService: CollectionDialogService,
              private toastController: ToastController,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.paymentReport.title');
    const oggi = new Date().getFullYear();
    for (let a = oggi; a >= oggi - 5; a--)
      this.anni.push(a);

    const btnDetail = userInterfaceService.Btn_Dettaglio;
    btnDetail.event = this.handleButtonDetailClick;
    this.actions = [{ button: btnDetail, visible: c => c.idDealer !== null }];
    this.columns = this.colonne();
  }

  ionViewWillEnter() {
    this.load();
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  private colonne(): NvxListColumn<PaymentDealerSummaryModel>[] {
    const t = (k: string) => this.transloco.translate('pages.paymentReport.' + k);
    const euro = (v: number) => v.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    const cols: NvxListColumn<PaymentDealerSummaryModel>[] = [
      { key: 'dealer', title: t('columnDealer'), mobile: 'title', value: c => c.dealer ?? this.transloco.translate('pages.payment.noDealer') },
      { key: 'numeroPagamenti', title: t('columnPayments'), align: 'end' },
      { key: 'incassato', title: t('columnCollected'), align: 'end', value: c => euro(c.incassato), sortValue: c => c.incassato, mobile: 'subtitle' },
      { key: 'percentualePiattaforma', title: t('columnFee'), align: 'end', value: c => c.percentualePiattaforma + '%', sortValue: c => c.percentualePiattaforma },
      { key: 'quotaCentro', title: t('columnDealerShare'), align: 'end', value: c => euro(c.quotaCentro), sortValue: c => c.quotaCentro, mobile: 'note' },
    ];
    if (this.mese > 0)
      cols.push({ key: 'rendiconto', title: t('columnSettlement'), sortValue: c => c.rendiconto?.stato ?? -1,
                  value: c => c.rendiconto ? this.transloco.translate('pages.payment.settlementState' + c.rendiconto.stato) : '-' });
    cols.push({ key: 'liquidato', title: t('columnPaid'), align: 'end', value: c => euro(c.liquidato), sortValue: c => c.liquidato });
    cols.push({ key: 'daLiquidare', title: t('columnToPay'), align: 'end', value: c => euro(c.daLiquidare), sortValue: c => c.daLiquidare });
    return cols;
  }

  periodChanged() {
    this.columns = this.colonne();
    this.load();
  }

  private load() {
    const request = new GenericRequest<PaymentReportGetInModel>(PaymentReportGetInModel);
    request.data.anno = this.anno;
    request.data.mese = this.mese > 0 ? this.mese : null;
    this.paymentService.PaymentReportGet(request).subscribe(res => {
      this.centri = res.success && res.data ? res.data.centro : [];
      this.totali = res.data?.totali ?? new PaymentTotalsModel();
      this.debugPagamenti = !!res.data?.debugPagamenti;
      this.fab();
    });
  }

  private fab() {
    const fab: FabMenuItem[] = [];
    if (this.mese > 0)
      fab.push(new FabMenuItem(this.transloco.translate('pages.paymentReport.generateAll'), 'document-text-outline', () => this.generateAll()));
    fab.push(new FabMenuItem(this.transloco.translate('pages.payment.excel'), 'grid-outline', () => this.excel()));
    fab.push(new FabMenuItem(this.transloco.translate('pages.paymentReport.settings'), 'options-outline', () => this.navCtrl.navigateForward('/paymentsettings')));
    this.fabMenuService.fabMenuItem = fab;
  }

  private async generateAll() {
    if (!await this.collectionDialogService.ConfirmCancelDialog(
          this.transloco.translate('pages.paymentReport.confirmGenerateAll', { periodo: this.monthNavigator.monthNames[this.mese - 1] + ' ' + this.anno })))
      return;
    const request = new GenericRequest<PaymentSettlementGenerateInModel>(PaymentSettlementGenerateInModel);
    request.data.anno = this.anno;
    request.data.mese = this.mese;
    this.paymentService.PaymentSettlementGenerate(request).subscribe(() => this.load());
  }

  private excel() {
    const request = new GenericRequest<PaymentExportInModel>(PaymentExportInModel);
    request.data.anno = this.anno;
    request.data.mese = this.mese > 0 ? this.mese : null;
    request.data.formato = 'xlsx';
    // il file si prepara con un job: lo si scarica dalla barra dei job quando e' pronto
    this.paymentService.PaymentExport(request).subscribe(async res => {
      if (!res.success)
        return;
      const toast = await this.toastController.create({ message: this.transloco.translate('pages.payment.exportStarted'), duration: 3000, position: 'bottom' });
      await toast.present();
    });
  }

  handleButtonDetailClick = (item: PaymentDealerSummaryModel) => {
    this.navCtrl.navigateForward('/paymentdealer', { state: { idDealer: item.idDealer, anno: this.anno, mese: this.mese } });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
