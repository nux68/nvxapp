import { TranslocoService } from '@jsverse/transloco';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { Az_Cliente_GetAll_InModel, Az_ClienteDeleteInModel, Az_ClienteModel, TipoCliente } from '../../../ClientServer-Service/Infrastructure/Az_Cliente/Models/az-cliente-model';
import { AzClienteService } from '../../../ClientServer-Service/Infrastructure/Az_Cliente/az-cliente.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { CommonDataEventsService } from '../../../Utility/infrastructure/common-data-events.service';

/*
 Clienti dell'azienda (anagrafica comune, schema Common): una sola anagrafica per presenze, CRM e documentale.
 I clienti disattivati restano nello storico e si vedono con "Mostra disabilitati"; un cliente usato da un
 applicativo (commesse, opportunita'...) non si cancella: lo blocca il server con un messaggio.
*/
@Component({
  selector: 'app-customer-list-page',
  templateUrl: './customer-list-page.component.html',
  styleUrls: ['./customer-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CustomerListPageComponent {

  public title: string;
  public searchText!: string;
  public items: Az_ClienteModel[] | null = null;
  public showDisabled = false;
  public readonly columns: NvxListColumn<Az_ClienteModel>[];
  public readonly actions: NvxListAction<Az_ClienteModel>[];
  public readonly sortList = (a: Az_ClienteModel, b: Az_ClienteModel) => (a.descrizione ?? '').localeCompare(b.descrizione ?? '');
  public readonly isDisabled = (x: Az_ClienteModel) => !x.attivo;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private azClienteService: AzClienteService,
              private userInterfaceService: UserInterfaceService,
              private collectionDialogService: CollectionDialogService,
              private commonDataEventsService: CommonDataEventsService,
              private transloco: TranslocoService) {

    const t = (k: string) => this.transloco.translate(k);
    this.title = t('pages.customerList.title');
    this.columns = [
      { key: 'descrizione', title: t('pages.customerList.columnCustomer'), mobile: 'title' },
      { key: 'tipo', title: t('pages.customerEdit.tipo'), mobile: 'note', value: x => t('pages.customerEdit.tipi.' + (x.tipo ?? TipoCliente.Cliente)) },
      { key: 'partitaIva', title: t('pages.customerEdit.partitaIva') },
      { key: 'citta', title: t('pages.customerEdit.citta'), mobile: 'subtitle' },
    ];
    const btnEdit = userInterfaceService.Btn_Modifica;
    btnEdit.event = (x: Az_ClienteModel) => this.navCtrl.navigateForward('/customeredit', { state: { id: x.id } });
    const btnDelete = userInterfaceService.Btn_Cancella;
    btnDelete.event = (x: Az_ClienteModel) => this.delete(x);
    this.actions = [{ button: btnDelete }, { button: btnEdit }];
  }

  ionViewWillEnter() {
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('pages.customerList.fabAdd'), 'add-circle-outline', () =>
        this.navCtrl.navigateForward('/customeredit', { state: { id: 0 } })),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    this.azClienteService.GetAll(new GenericRequest<Az_Cliente_GetAll_InModel>(Az_Cliente_GetAll_InModel))
      .subscribe(res => this.items = res.success && res.data ? res.data.az_Cliente : []);
  }

  private async delete(item: Az_ClienteModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('pages.customerList.confirmDelete')))
      return;
    const request = new GenericRequest<Az_ClienteDeleteInModel>(Az_ClienteDeleteInModel);
    request.data.id = item.id;
    this.azClienteService.Az_ClienteDelete(request).subscribe(res => {
      if (res.success) {
        this.commonDataEventsService.ClientiModificati();
        this.load();
      }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
