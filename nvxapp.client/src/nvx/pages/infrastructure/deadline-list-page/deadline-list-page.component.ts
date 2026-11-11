import { Component, ChangeDetectionStrategy } from '@angular/core';
import { formatDate } from '@angular/common';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DeadlineCloseInModel, DeadlineListInModel, DeadlineModel, DeadlineState } from '../../../ClientServer-Service/Infrastructure/Deadline/Models/deadline-model';
import { DeadlineService } from '../../../ClientServer-Service/Infrastructure/Deadline/deadline.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { LanguageService } from '../../../Utility/infrastructure/language.service';
import { NotificationNotifierService } from '../../../Utility/infrastructure/notification-notifier.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

/** filtro sullo stato delle scadenze */
type DeadlineStateFilter = 'open' | 'closed' | 'all';

/*
 Scadenzario dell'azienda corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.3); e' anche la pagina dei promemoria.
   - amministratori (CompanyPowerAdmin, CompanyAdmin): tutte le scadenze (o "solo mie"), nuova scadenza manuale con il + del fab,
     modifica di quelle manuali
   - altri utenti: solo le scadenze di cui sono responsabili
 Tutti possono chiudere e riaprire le scadenze che vedono e aprire l'oggetto collegato (url + stato).
 Filtri: stato, entro n giorni (server); testo nel filtro della barra (sulla lista caricata).
*/
@Component({
  selector: 'app-deadline-list-page',
  templateUrl: './deadline-list-page.component.html',
  styleUrls: ['./deadline-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DeadlineListPageComponent {

  public title: string;
  public searchText!: string;
  public deadlineList: DeadlineModel[] | null = null;
  public amministratore = false;

  public stato: DeadlineStateFilter = 'open';
  public readonly stati: DeadlineStateFilter[] = ['open', 'closed', 'all'];
  /** null = nessun limite */
  public entroGiorni: number | null = null;
  public readonly giorni = [7, 30, 90, 365];
  public soloMie = false;

  public readonly columns: NvxListColumn<DeadlineModel>[];
  public readonly actions: NvxListAction<DeadlineModel>[];
  /** calendario personale (INF-CAL.3): le scadenze aperte anche in Google / Outlook / Apple */
  public readonly buttonbar: ButtonItem[];
  public readonly sortByData = (a: DeadlineModel, b: DeadlineModel) => (a.dataScadenza ?? '').localeCompare(b.dataScadenza ?? '');
  public readonly highlight = (item: DeadlineModel) => item.scaduta;
  public readonly isClosed = (item: DeadlineModel) => item.stato === DeadlineState.Chiusa;
  public readonly iconOf = (item: DeadlineModel) => item.stato === DeadlineState.Chiusa ? 'checkmark-circle-outline'
                                                    : item.scaduta ? 'alert-circle-outline'
                                                    : item.inScadenza ? 'time-outline' : 'calendar-outline';

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private deadlineService: DeadlineService,
              private notificationNotifierService: NotificationNotifierService,
              private languageService: LanguageService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.deadlineList.title');

    this.columns = [
      { key: 'dataScadenza', title: this.transloco.translate('pages.deadlineList.columnDate'), width: '150px', mobile: 'note',
        value: d => this.formatData(d.dataScadenza),
        sortValue: d => d.dataScadenza ? new Date(d.dataScadenza) : null },
      { key: 'descrizione', title: this.transloco.translate('pages.deadlineList.columnDescription'), mobile: 'title' },
      { key: 'responsabile', title: this.transloco.translate('pages.deadlineList.columnResponsible') },
      { key: 'stato', title: this.transloco.translate('pages.deadlineList.columnState'),
        value: d => this.descrizioneStato(d) },
      { key: 'fonte', title: this.transloco.translate('pages.deadlineList.columnSource'),
        value: d => d.manuale ? this.transloco.translate('pages.deadlineList.manual') : d.fonte },
    ];

    const btnApri = userInterfaceService.Btn_Apri;
    btnApri.event = (item: DeadlineModel) => this.notificationNotifierService.openUrl(item.url, item.urlState);
    const btnCompleta = userInterfaceService.Btn_Completa;
    btnCompleta.event = (item: DeadlineModel) => this.chiudi(item, true);
    const btnRiapri = userInterfaceService.Btn_Riapri;
    btnRiapri.event = (item: DeadlineModel) => this.chiudi(item, false);
    const btnModifica = userInterfaceService.Btn_Modifica;
    btnModifica.event = (item: DeadlineModel) => this.navCtrl.navigateForward('/deadlineedit', { state: { id: item.id } });

    this.actions = [
      { button: btnApri, visible: d => !!d.url },
      { button: btnCompleta, visible: d => d.stato === DeadlineState.Aperta },
      { button: btnRiapri, visible: d => d.stato === DeadlineState.Chiusa },
      { button: btnModifica, visible: d => d.manuale && this.amministratore },
    ];

    const btnCalendario = userInterfaceService.Btn_Calendario;
    btnCalendario.event = () => this.navCtrl.navigateForward('/calendarfeed');
    this.buttonbar = [btnCalendario];
  }

  ionViewWillEnter() {
    this.fabMenuService.fabMenuItem = [];
    this.load();
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    const request = new GenericRequest<DeadlineListInModel>(DeadlineListInModel);
    request.data.stato = this.stato === 'open' ? DeadlineState.Aperta : this.stato === 'closed' ? DeadlineState.Chiusa : null;
    request.data.entroGiorni = this.entroGiorni;
    request.data.soloMie = this.soloMie;
    this.deadlineService.DeadlineList(request).subscribe(res => {
      this.deadlineList = res.success && res.data ? res.data.deadline : [];
      this.amministratore = !!(res.success && res.data && res.data.amministratore);

      // nuova scadenza manuale: solo per gli amministratori
      this.fabMenuService.fabMenuItem = this.amministratore ? [
        new FabMenuItem(this.transloco.translate('pages.deadlineList.newDeadline'), 'add-circle-outline', () => {
          this.navCtrl.navigateForward('/deadlineedit', { state: { id: 0 } });
        }),
      ] : [];
    });
  }

  statoChanged(value: any) {
    this.stato = (value ?? 'open') as DeadlineStateFilter;
    this.deadlineList = null;
    this.load();
  }

  entroGiorniChanged(value: any) {
    this.entroGiorni = value === null || value === undefined || value === '' ? null : Number(value);
    this.load();
  }

  soloMieChanged(value: boolean) {
    this.soloMie = value;
    this.load();
  }

  private chiudi(item: DeadlineModel, chiudi: boolean) {
    const request = new GenericRequest<DeadlineCloseInModel>(DeadlineCloseInModel);
    request.data.id = item.id;
    request.data.chiudi = chiudi;
    this.deadlineService.DeadlineClose(request).subscribe(res => {
      if (res.success)
        this.load();
    });
  }

  private formatData(value: string | null): string {
    return value ? formatDate(value, 'shortDate', this.languageService.locale) : '';
  }

  public descrizioneStato(d: DeadlineModel): string {
    if (d.stato === DeadlineState.Chiusa)
      return this.transloco.translate('pages.deadlineList.stateClosed') + (d.dataChiusura ? ' (' + this.formatData(d.dataChiusura) + ')' : '');
    if (d.scaduta)
      return this.transloco.translate('pages.deadlineList.stateExpired');
    if (d.inScadenza)
      return this.transloco.translate('pages.deadlineList.stateExpiring');
    return this.transloco.translate('pages.deadlineList.stateOpen');
  }

  public coloreStato(d: DeadlineModel): string {
    if (d.stato === DeadlineState.Chiusa)
      return 'medium';
    return d.scaduta ? 'danger' : d.inScadenza ? 'warning' : 'success';
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
