import { Component, ChangeDetectionStrategy } from '@angular/core';
import { formatDate } from '@angular/common';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Subscription } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { TicketListInModel, TicketListModel, TicketOpenInfoInModel, TicketState, TicketView } from '../../../ClientServer-Service/Infrastructure/SupportTicket/Models/ticket-model';
import { TicketService } from '../../../ClientServer-Service/Infrastructure/SupportTicket/ticket.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { LanguageService } from '../../../Utility/infrastructure/language.service';
import { TicketNotifierService } from '../../../Utility/infrastructure/ticket-notifier.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

/*
 Elenco dei ticket di assistenza, in tre viste:
   - In carico a me (iniziale): assegnati a me o al livello di un nodo di cui sono PowerAdmin
   - Aperti da me
   - Altri: livelli figli (PowerAdmin) e ticket assegnati in passato; per il SuperUser tutti
 Si ricarica all'arrivo di una notifica.
*/
@Component({
  selector: 'app-ticket-list-page',
  templateUrl: './ticket-list-page.component.html',
  styleUrls: ['./ticket-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TicketListPageComponent {

  public title: string;
  public searchText!: string;
  public view: TicketView = TicketView.InCarico;
  public state: TicketState | null = null;
  public ticketList: TicketListModel[] | null = null;
  public readonly views = [TicketView.InCarico, TicketView.Aperti, TicketView.Altri];
  public readonly states = [TicketState.Aperto, TicketState.InCarico, TicketState.InAttesaInfo, TicketState.Risolto, TicketState.Chiuso];
  public readonly columns: NvxListColumn<TicketListModel>[];
  public readonly actions: NvxListAction<TicketListModel>[];
  public readonly sortByModified = (a: TicketListModel, b: TicketListModel) => (b.modifiedDate ?? '').localeCompare(a.modifiedDate ?? '');
  public readonly highlight = (item: TicketListModel) => item.state === TicketState.Aperto || item.state === TicketState.Risolto;
  public btnApri: ButtonItem;

  private changedSub: Subscription | null = null;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private ticketService: TicketService,
              private ticketNotifierService: TicketNotifierService,
              private languageService: LanguageService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.ticketList.title');

    this.columns = [
      { key: 'id', title: '#', width: '70px', value: t => t.id, mobile: 'note' },
      { key: 'oggetto', title: this.transloco.translate('pages.ticketList.columnSubject'), mobile: 'title' },
      { key: 'tipoDescrizione', title: this.transloco.translate('pages.ticketList.columnType') },
      { key: 'state', title: this.transloco.translate('pages.ticketList.columnState'), value: t => this.transloco.translate('ticket.state.' + t.state) },
      { key: 'currentLevel', title: this.transloco.translate('pages.ticketList.columnLevel'),
        value: t => this.transloco.translate('ticket.level.' + t.currentLevel) + (t.currentUserName ? ' - ' + t.currentUserName : '') },
      { key: 'openerUserName', title: this.transloco.translate('pages.ticketList.columnOpener') },
      { key: 'modifiedDate', title: this.transloco.translate('pages.ticketList.columnModified'), mobile: 'note',
        value: t => t.modifiedDate ? formatDate(t.modifiedDate, 'short', this.languageService.locale) : '',
        sortValue: t => t.modifiedDate ? new Date(t.modifiedDate) : null },
    ];

    this.btnApri = userInterfaceService.Btn_Apri;
    this.btnApri.event = this.handleButtonApriClick;
    this.actions = [{ button: this.btnApri }];
  }

  ionViewWillEnter() {
    this.load();

    // nuovo ticket solo se l'utente ha almeno un ruolo con cui aprire
    this.fabMenuService.fabMenuItem = [];
    this.ticketService.TicketOpenInfo(new GenericRequest<TicketOpenInfoInModel>(TicketOpenInfoInModel)).subscribe(res => {
      if (res.success && res.data && res.data.openerRole.length > 0) {
        this.fabMenuService.fabMenuItem = [
          new FabMenuItem(this.transloco.translate('pages.ticketList.newTicket'), 'add-circle-outline', () => {
            this.navCtrl.navigateForward('/ticketnew');
          }),
        ];
      }
    });

    this.changedSub = this.ticketNotifierService.changed$.subscribe(() => this.load());
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
    this.changedSub?.unsubscribe();
    this.changedSub = null;
  }

  load() {
    const request = new GenericRequest<TicketListInModel>(TicketListInModel);
    request.data.view = this.view;
    request.data.state = this.state;
    this.ticketService.TicketList(request).subscribe(res => {
      this.ticketList = res.success && res.data ? res.data.ticket : [];
    });
    this.ticketNotifierService.refreshCount();
  }

  viewChanged(value: any) {
    this.view = Number(value) as TicketView;
    this.ticketList = null;
    this.load();
  }

  stateChanged(value: any) {
    this.state = value === null || value === undefined || value === '' ? null : Number(value) as TicketState;
    this.load();
  }

  handleButtonApriClick = (item: TicketListModel) => {
    this.navCtrl.navigateForward('/ticketdetail', {
      state: { id: item.id }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
