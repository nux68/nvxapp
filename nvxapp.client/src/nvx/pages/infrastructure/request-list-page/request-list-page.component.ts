import { Component, ChangeDetectionStrategy } from '@angular/core';
import { formatDate } from '@angular/common';
import { Router } from '@angular/router';
import { AlertController, NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Subscription } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { RequestActionInModel, RequestActionOutModel, RequestFilter, RequestItemModel, RequestListInModel, RequestState } from '../../../ClientServer-Service/Infrastructure/Request/Models/request-model';
import { RequestService } from '../../../ClientServer-Service/Infrastructure/Request/request.service';
import { GenericResult } from '../../../ClientServer-Service/ModelsBase/generic-result';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { LanguageService } from '../../../Utility/infrastructure/language.service';
import { RequestNotifierService } from '../../../Utility/infrastructure/request-notifier.service';
import { RequestUiService } from '../../../Utility/infrastructure/request-ui.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

/*
 Richieste comuni a tutti gli applicativi (ANALISI_INFRASTRUCTURE_RICHIESTE.md, RICH-INF.3):
   - /requests: "Le mie richieste" (richieste dell'utente: in attesa / tutte), con le voci "Nuova richiesta" dei moduli
   - /requestsadmin: "Richieste da approvare" (amministratori dell'azienda: da approvare / tutte / approvate)
 Le richieste arrivano dai moduli attivi (Presenze, Risorse aziendali...): approva, rifiuta e annulla li decide il modulo;
 i rami possono aggiungere azioni sulle righe (RequestUiService). Si ricarica all'evento SignalR "RequestNotifier".
*/
@Component({
  selector: 'app-request-list-page',
  templateUrl: './request-list-page.component.html',
  styleUrls: ['./request-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class RequestListPageComponent {

  public readonly admin: boolean;
  public title: string;
  public searchText!: string;
  public richieste: RequestItemModel[] | null = null;
  public applicativi: number[] = [];
  /** applicativo scelto nel filtro (null = tutti) */
  public applicativo: number | null = null;

  public filtro: RequestFilter = RequestFilter.DaApprovare;
  public readonly filtri: { value: RequestFilter, key: string }[];

  public columns: NvxListColumn<RequestItemModel>[] = [];
  public actions: NvxListAction<RequestItemModel>[] = [];
  public readonly highlight = (item: RequestItemModel) => item.canApprove || item.canReject;
  public readonly iconOf = (item: RequestItemModel) => RequestListPageComponent.icona(item.stato);
  public readonly sortByDal = (a: RequestItemModel, b: RequestItemModel) => (b.dal ?? '').localeCompare(a.dal ?? '');

  private changedSub: Subscription | null = null;

  constructor(router: Router,
              private navCtrl: NavController,
              private alertController: AlertController,
              private requestService: RequestService,
              private requestNotifierService: RequestNotifierService,
              private requestUiService: RequestUiService,
              private collectionDialogService: CollectionDialogService,
              private userInterfaceService: UserInterfaceService,
              private fabMenuService: FabMenuService,
              private languageService: LanguageService,
              private transloco: TranslocoService) {

    this.admin = router.url.split('?')[0].toLowerCase().startsWith('/requestsadmin');
    this.title = this.transloco.translate(this.admin ? 'pages.requestList.titleAdmin' : 'pages.requestList.title');
    this.filtri = this.admin
      ? [
          { value: RequestFilter.DaApprovare, key: 'pages.requestList.filter.toApprove' },
          { value: RequestFilter.Tutte, key: 'pages.requestList.filter.all' },
          { value: RequestFilter.Approvate, key: 'pages.requestList.filter.approved' },
        ]
      : [
          { value: RequestFilter.DaApprovare, key: 'pages.requestList.filter.pending' },
          { value: RequestFilter.Tutte, key: 'pages.requestList.filter.all' },
        ];
  }

  ionViewWillEnter() {
    this.preparaColonne();
    this.preparaAzioni();
    this.preparaNuove();
    this.load();
    this.changedSub?.unsubscribe();
    this.changedSub = this.requestNotifierService.changed$.subscribe(() => this.load());
  }

  ionViewWillLeave() {
    this.changedSub?.unsubscribe();
    this.changedSub = null;
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    const request = new GenericRequest<RequestListInModel>(RequestListInModel);
    request.data.admin = this.admin;
    request.data.filtro = this.filtro;
    this.requestService.RequestList(request).subscribe(res => {
      this.richieste = res.success && res.data ? res.data.richieste : [];
      this.applicativi = res.success && res.data ? res.data.applicativi : [];
      if (this.applicativo !== null && !this.applicativi.includes(this.applicativo))
        this.applicativo = null;
      this.preparaColonne();
      if (res.success && res.data)
        this.requestNotifierService.set(res.data.daApprovare, res.data.inAttesa);
    });
  }

  /** richieste dell'applicativo scelto */
  public get richiesteFiltrate(): RequestItemModel[] | null {
    if (this.richieste === null || this.applicativo === null)
      return this.richieste;
    return this.richieste.filter(x => x.application === this.applicativo);
  }

  onFiltroChange(value: any) {
    this.filtro = Number(value ?? RequestFilter.DaApprovare) as RequestFilter;
    this.richieste = null;
    this.load();
  }

  onApplicativoChange(value: any) {
    this.applicativo = value === null || value === undefined || value === '' || value === 'all' ? null : Number(value);
  }

  nomeApplicativo(application: number): string {
    return this.transloco.translate('pages.requestList.application.' + application);
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  // ---------------------------------------------------------------------------

  private preparaColonne() {
    const t = (k: string) => this.transloco.translate('pages.requestList.' + k);
    // con richieste di piu' applicativi il tipo dice anche da quale arriva
    const conApplicativo = this.applicativi.length > 1;
    this.columns = [
      ...(this.admin ? [{ key: 'richiedente', title: t('columnRequester'), mobile: 'title' } as NvxListColumn<RequestItemModel>] : []),
      { key: 'tipoTesto', title: t('columnType'), mobile: this.admin ? 'subtitle' : 'title',
        value: x => conApplicativo ? `${x.tipoTesto} (${this.nomeApplicativo(x.application)})` : x.tipoTesto },
      { key: 'dal', title: t('columnPeriod'), width: '220px', mobile: 'note',
        value: x => this.periodo(x), sortValue: x => x.dal ? new Date(x.dal) : null },
      { key: 'dettaglio', title: t('columnDetail'), sortable: false },
      { key: 'statoTesto', title: t('columnState'), sortValue: x => x.stato },
    ];
  }

  private preparaAzioni() {
    const extra: NvxListAction<RequestItemModel>[] = this.requestUiService.Actions.map(a => {
      const btn = a.button();
      btn.event = (item: RequestItemModel) => a.run(item, this.admin);
      return { button: btn, visible: (item: RequestItemModel) => item.extra === a.extra };
    });

    const btnApri = this.userInterfaceService.Btn_Apri;
    btnApri.event = (item: RequestItemModel) => this.navCtrl.navigateForward(item.url!, { state: item.urlState ?? {} });

    const btnApprova = this.userInterfaceService.Btn_Approva;
    btnApprova.event = (item: RequestItemModel) => this.approva(item);
    const btnRifiuta = this.userInterfaceService.Btn_Rifiuta;
    btnRifiuta.event = (item: RequestItemModel) => this.rifiuta(item);
    const btnAnnulla = new ButtonItem(this.transloco.translate('pages.requestList.cancel'), 'close-circle-outline', 'danger', false,
                                      (item: RequestItemModel) => this.annulla(item));

    this.actions = [
      ...extra,
      { button: btnApri, visible: item => !!item.url },
      ...(this.admin
        ? [
            { button: btnApprova, visible: (item: RequestItemModel) => item.canApprove },
            { button: btnRifiuta, visible: (item: RequestItemModel) => item.canReject },
          ]
        : [
            { button: btnAnnulla, visible: (item: RequestItemModel) => item.canCancel },
          ]),
    ];
  }

  // voci "Nuova richiesta" dei moduli attivi (solo nella pagina dell'utente)
  private preparaNuove() {
    this.fabMenuService.fabMenuItem = this.admin
      ? []
      : this.requestUiService.NewRequests.map(n => new FabMenuItem(this.transloco.translate(n.title), 'add-circle-outline',
                                                                   () => this.navCtrl.navigateForward(n.url)));
  }

  private async approva(item: RequestItemModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('pages.requestList.confirmApprove')))
      return;
    const request = this.azione(item);
    this.requestService.RequestApprove(request).subscribe(res => this.dopoAzione(res));
  }

  private async rifiuta(item: RequestItemModel) {
    const alert = await this.alertController.create({
      header: this.transloco.translate('pages.requestList.confirmRejectTitle'),
      message: this.transloco.translate('pages.requestList.confirmReject'),
      inputs: [{ name: 'motivo', type: 'textarea', placeholder: this.transloco.translate('pages.requestList.rejectReason'), attributes: { maxlength: 500 } }],
      buttons: [
        { text: this.transloco.translate('common.buttons.cancel'), role: 'cancel' },
        { text: this.transloco.translate('pages.requestList.reject'), role: 'confirm' },
      ],
    });
    await alert.present();
    const esito = await alert.onDidDismiss();
    if (esito.role !== 'confirm')
      return;
    const request = this.azione(item);
    request.data.motivo = (esito.data?.values?.motivo ?? '').trim() || null;
    this.requestService.RequestReject(request).subscribe(res => this.dopoAzione(res));
  }

  private async annulla(item: RequestItemModel) {
    const key = item.stato === RequestState.Approvata ? 'pages.requestList.confirmRevoke' : 'pages.requestList.confirmCancel';
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate(key)))
      return;
    this.requestService.RequestCancel(this.azione(item)).subscribe(res => this.dopoAzione(res));
  }

  private azione(item: RequestItemModel): GenericRequest<RequestActionInModel> {
    const request = new GenericRequest<RequestActionInModel>(RequestActionInModel);
    request.data.key = item.key;
    return request;
  }

  private dopoAzione(res: GenericResult<RequestActionOutModel>) {
    if (res.success && res.data)
      this.requestNotifierService.set(res.data.daApprovare, res.data.inAttesa);
    this.load();
  }

  private periodo(x: RequestItemModel): string {
    const f = (v: string) => formatDate(v, x.soloData ? 'shortDate' : 'short', this.languageService.locale);
    if (!x.dal)
      return '';
    if (!x.al || x.al === x.dal)
      return f(x.dal);
    // stesso giorno con orari: "12/11/26, 10:00 - 11:00"
    if (!x.soloData && x.al.substring(0, 10) === x.dal.substring(0, 10))
      return `${f(x.dal)} - ${formatDate(x.al, 'shortTime', this.languageService.locale)}`;
    return `${f(x.dal)} - ${f(x.al)}`;
  }

  private static icona(stato: RequestState): string {
    switch (stato) {
      case RequestState.Approvata: return 'checkmark-circle-outline';
      case RequestState.Rifiutata: return 'close-circle-outline';
      case RequestState.Annullata: return 'remove-circle-outline';
      default: return 'hourglass-outline';
    }
  }
}
