import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Az_Sedi_GetAll_InModel, Az_SediModel } from '../../../ClientServer-Service/GestionePresenze/Az_Sedi/Models/az-sedi-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AzSediService } from '../../../ClientServer-Service/GestionePresenze/Az_Sedi/az-sedi.service';

@Component({
  selector: 'app-az-sedi-list-page',
  templateUrl: './az-sedi-list-page.component.html',
  styleUrls: ['./az-sedi-list-page.component.scss'],
  standalone: false
})
export class AzSediListPageComponent implements OnInit {

  public title = 'Sedi';
  public searchText!: string;
  public azSediList: Az_SediModel[] | null = null;
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private azSediService: AzSediService,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService) {

    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {
    let request: GenericRequest<Az_Sedi_GetAll_InModel> = new GenericRequest<Az_Sedi_GetAll_InModel>(Az_Sedi_GetAll_InModel);
    this.azSediService.GetAll(request).subscribe(res => {
      this.azSediList = res.data.az_Sedi;
    });

    this.fabMenuService.fabMenuItem = [
      new FabMenuItem('Nuova Sede', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/azsediedit', {
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
    this.navCtrl.navigateForward('/azsediedit', {
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
    if (!this.azSediList) return [];
    return this.azSediList.sort((a, b) => a.descrizione.localeCompare(b.descrizione));
  }
}
