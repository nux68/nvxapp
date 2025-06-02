import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Az_Cliente_GetAll_InModel, Az_ClienteModel } from '../../../ClientServer-Service/GestionePresenze/Az_Cliente/Models/az-cliente-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AzClienteService } from '../../../ClientServer-Service/GestionePresenze/Az_Cliente/az-cliente.service';

@Component({
  selector: 'app-customer-list-page',
  templateUrl: './customer-list-page.component.html',
  styleUrls: ['./customer-list-page.component.scss'],
  standalone: false
})
export class CustomerListPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public az_ClienteList: Az_ClienteModel[] | null = null;
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private azClienteService: AzClienteService,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService) {

    this.title = 'Customers';
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {
    let request: GenericRequest<Az_Cliente_GetAll_InModel> = new GenericRequest<Az_Cliente_GetAll_InModel>(Az_Cliente_GetAll_InModel);
    this.azClienteService.GetAll(request).subscribe(res => {
      this.az_ClienteList = res.data.az_Cliente;
    });

    this.fabMenuService.fabMenuItem = [
      new FabMenuItem('Nuovo', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/customeredit', {
          state: { id: 0 }
        });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  ngOnInit() { }

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/customeredit', {
      state: { id: item.id }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  isAdmin(item: any) {
    return false;
  }

  getAll() {
    if (!this.az_ClienteList) return [];
    const sorted_ClienteList = this.az_ClienteList.sort((a, b) =>
      a.descrizione.localeCompare(b.descrizione)
    );
    return sorted_ClienteList;
  }
}
