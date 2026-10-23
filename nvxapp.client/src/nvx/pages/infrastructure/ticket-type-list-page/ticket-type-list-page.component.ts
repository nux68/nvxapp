import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { TicketTypeListInModel, TicketTypeListModel } from '../../../ClientServer-Service/Infrastructure/SupportTicket/Models/ticket-type-model';
import { TicketTypeService } from '../../../ClientServer-Service/Infrastructure/SupportTicket/ticket-type.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

// Tipi di richiesta dei ticket (SuperUser): i disattivati si vedono con "mostra anche i disattivati"
@Component({
  selector: 'app-ticket-type-list-page',
  templateUrl: './ticket-type-list-page.component.html',
  styleUrls: ['./ticket-type-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TicketTypeListPageComponent {

  public title: string;
  public searchText!: string;
  public ticketTypeList: TicketTypeListModel[] | null = null;
  public readonly columns: NvxListColumn<TicketTypeListModel>[];
  public readonly actions: NvxListAction<TicketTypeListModel>[];
  public readonly isDisabled = (item: TicketTypeListModel) => !item.active;
  public readonly sortByOrdine = (a: TicketTypeListModel, b: TicketTypeListModel) => a.ordine - b.ordine || a.codice.localeCompare(b.codice);
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private ticketTypeService: TicketTypeService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.ticketTypeList.title');
    this.columns = [
      { key: 'codice', title: this.transloco.translate('pages.ticketTypeList.columnCode'), mobile: 'note' },
      { key: 'descrizione', title: this.transloco.translate('pages.ticketTypeList.columnDescription'), mobile: 'title' },
      { key: 'applicationType', title: this.transloco.translate('pages.ticketTypeList.columnApplication'),
        value: t => this.transloco.translate('ticket.application.' + (t.applicationType ?? 'general')) },
      { key: 'level', title: this.transloco.translate('pages.ticketTypeList.columnLevels'), sortable: false,
        value: t => t.level.map(l => this.transloco.translate('ticket.level.' + l)).join(', ') },
      { key: 'aiResolution', title: this.transloco.translate('pages.ticketTypeList.columnAi'), width: '70px',
        value: t => this.transloco.translate(t.aiResolution ? 'pages.ticketTypeList.yes' : 'pages.ticketTypeList.no') },
    ];

    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
    this.actions = [{ button: this.btnEdit }];
  }

  ionViewWillEnter() {
    this.ticketTypeService.TicketTypeList(new GenericRequest<TicketTypeListInModel>(TicketTypeListInModel)).subscribe(res => {
      this.ticketTypeList = res.success && res.data ? res.data.ticketType : [];
    });

    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('pages.ticketTypeList.newType'), 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/tickettypeedit', { state: { id: 0 } });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  handleButtonEditClick = (item: TicketTypeListModel) => {
    this.navCtrl.navigateForward('/tickettypeedit', {
      state: { id: item.id }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
