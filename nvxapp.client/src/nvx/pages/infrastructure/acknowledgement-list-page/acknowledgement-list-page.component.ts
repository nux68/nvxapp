import { Component, ChangeDetectionStrategy } from '@angular/core';
import { formatDate } from '@angular/common';
import { TranslocoService } from '@jsverse/transloco';
import { Subscription } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AcknowledgementConfirmInModel, AcknowledgementListInModel, AcknowledgementModel } from '../../../ClientServer-Service/Infrastructure/Acknowledgement/Models/acknowledgement-model';
import { AcknowledgementService } from '../../../ClientServer-Service/Infrastructure/Acknowledgement/acknowledgement.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { LanguageService } from '../../../Utility/infrastructure/language.service';
import { NotificationNotifierService } from '../../../Utility/infrastructure/notification-notifier.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

/** filtro della lista: da confermare o tutte */
type AcknowledgementFilter = 'pending' | 'all';

/*
 Prese visione dell'utente collegato (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.6); e' la pagina delle notifiche di presa visione.
   - confermabileQui (canale base): titolo, testo, link all'oggetto e "Confermo di aver letto"
   - altri canali (es. firma del documento): si conferma aprendo l'oggetto (url + stato) e seguendo il canale
 Si ricarica all'arrivo di una notifica (le richieste di presa visione arrivano come notifica).
*/
@Component({
  selector: 'app-acknowledgement-list-page',
  templateUrl: './acknowledgement-list-page.component.html',
  styleUrls: ['./acknowledgement-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class AcknowledgementListPageComponent {

  public title: string;
  public searchText!: string;
  public acknowledgementList: AcknowledgementModel[] | null = null;
  public filtro: AcknowledgementFilter = 'pending';
  public readonly filtri: AcknowledgementFilter[] = ['pending', 'all'];

  public readonly columns: NvxListColumn<AcknowledgementModel>[];
  public readonly actions: NvxListAction<AcknowledgementModel>[];
  public readonly highlight = (item: AcknowledgementModel) => !item.dataConferma;
  public readonly iconOf = (item: AcknowledgementModel) => item.dataConferma ? 'checkmark-circle-outline' : 'eye-outline';
  public readonly sortByData = (a: AcknowledgementModel, b: AcknowledgementModel) => (b.dataRichiesta ?? '').localeCompare(a.dataRichiesta ?? '');

  private changedSub: Subscription | null = null;

  constructor(private userInterfaceService: UserInterfaceService,
              private acknowledgementService: AcknowledgementService,
              private notificationNotifierService: NotificationNotifierService,
              private languageService: LanguageService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.acknowledgementList.title');

    this.columns = [
      { key: 'titolo', title: this.transloco.translate('pages.acknowledgementList.columnTitle'), mobile: 'title' },
      { key: 'testo', title: this.transloco.translate('pages.acknowledgementList.columnText') },
      { key: 'azienda', title: this.transloco.translate('pages.acknowledgementList.columnCompany') },
      { key: 'dataRichiesta', title: this.transloco.translate('pages.acknowledgementList.columnRequested'), width: '150px', mobile: 'note',
        value: a => this.formatData(a.dataRichiesta),
        sortValue: a => a.dataRichiesta ? new Date(a.dataRichiesta) : null },
      { key: 'stato', title: this.transloco.translate('pages.acknowledgementList.columnState'),
        value: a => this.descrizioneStato(a),
        sortValue: a => a.dataConferma ? new Date(a.dataConferma) : null },
    ];

    // apre l'oggetto: per gli altri canali e' il modo di confermare
    const btnApri = userInterfaceService.Btn_Apri;
    btnApri.event = (item: AcknowledgementModel) => this.notificationNotifierService.openUrl(item.url, item.urlState);
    const btnConferma = userInterfaceService.Btn_ConfermaLettura;
    btnConferma.event = (item: AcknowledgementModel) => this.conferma(item);
    this.actions = [
      { button: btnApri, visible: a => !!a.url },
      { button: btnConferma, visible: a => a.confermabileQui && !a.dataConferma },
    ];
  }

  ionViewWillEnter() {
    this.load();
    this.changedSub = this.notificationNotifierService.changed$.subscribe(() => this.load());
  }

  ionViewWillLeave() {
    this.changedSub?.unsubscribe();
    this.changedSub = null;
  }

  load() {
    const request = new GenericRequest<AcknowledgementListInModel>(AcknowledgementListInModel);
    request.data.daConfermare = this.filtro === 'pending';
    this.acknowledgementService.AcknowledgementList(request).subscribe(res => {
      this.acknowledgementList = res.success && res.data ? res.data.acknowledgement : [];
      if (res.success && res.data)
        this.notificationNotifierService.setDaConfermare(res.data.daConfermare);
    });
  }

  filtroChanged(value: any) {
    this.filtro = (value ?? 'pending') as AcknowledgementFilter;
    this.acknowledgementList = null;
    this.load();
  }

  private conferma(item: AcknowledgementModel) {
    const request = new GenericRequest<AcknowledgementConfirmInModel>(AcknowledgementConfirmInModel);
    request.data.id = item.id;
    this.acknowledgementService.AcknowledgementConfirm(request).subscribe(res => {
      if (res.success && res.data) {
        this.notificationNotifierService.setDaConfermare(res.data.daConfermare);
        this.load();
      }
    });
  }

  private formatData(value: string | null): string {
    return value ? formatDate(value, 'short', this.languageService.locale) : '';
  }

  public descrizioneStato(a: AcknowledgementModel): string {
    if (a.dataConferma)
      return this.transloco.translate('pages.acknowledgementList.confirmedOn', { date: this.formatData(a.dataConferma) });
    return this.transloco.translate(a.confermabileQui ? 'pages.acknowledgementList.pending' : 'pages.acknowledgementList.pendingOther');
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
