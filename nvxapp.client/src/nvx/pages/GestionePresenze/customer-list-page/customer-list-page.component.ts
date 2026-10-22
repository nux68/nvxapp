import { TranslocoService } from '@jsverse/transloco';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Az_Cliente_GetAll_InModel, Az_ClienteModel, Az_ClienteDeleteInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Cliente/Models/az-cliente-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AzClienteService } from '../../../ClientServer-Service/GestionePresenze/Az_Cliente/az-cliente.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { map, catchError } from 'rxjs';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { ParTable, SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';

@Component({
  selector: 'app-customer-list-page',
  templateUrl: './customer-list-page.component.html',
  styleUrls: ['./customer-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CustomerListPageComponent {

  public title!: string;
  public searchText!: string;
  /** dalla cache caricata al login: si aggiorna da sola dopo salvataggi e cancellazioni (refresh mirato) */
  public get az_ClienteList(): Az_ClienteModel[] | null {
    return this.sharedParameterGestionePresenzeService.Az_Cliente;
  }
  public readonly columns: NvxListColumn<Az_ClienteModel>[] = [
    { key: 'descrizione', title: this.transloco.translate('attendance.pages.customerList.columnCustomer'), mobile: 'title' }
  ];
  public actions: NvxListAction<Az_ClienteModel>[] = [];
  public readonly sortList = (a: Az_ClienteModel, b: Az_ClienteModel) => (a.descrizione ?? '').localeCompare(b.descrizione ?? '');
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private azClienteService: AzClienteService,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService,
              private collectionDialogService: CollectionDialogService,
              private refresherService: RefresherService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('attendance.pages.customerList.title');
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    this.actions = [{ button: this.btnDelete, visible: item => !item.default }, { button: this.btnEdit }];
  }


  ionViewWillEnter() {

    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('attendance.pages.customerList.fabAdd'), 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/customeredit', {
          state: { id: 0 }
        });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }


  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/customeredit', {
      state: { id: item.id }
    });
  }

  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.customerList.confirmDelete'));
    if (result) {

      let request: GenericRequest<Az_ClienteDeleteInModel> = new GenericRequest<Az_ClienteDeleteInModel>(Az_ClienteDeleteInModel);
      request.data.id = item.id;
      this.azClienteService.Az_ClienteDelete(request).pipe(
        map(() => {
          this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Az_Cliente);
          return true;
        }),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [false];
        })
        ).subscribe();

    }

  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }


}
