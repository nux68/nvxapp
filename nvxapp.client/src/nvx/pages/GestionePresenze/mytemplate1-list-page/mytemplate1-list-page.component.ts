import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { My_template1_GetAllInModel, My_Template1Model } from '../../../ClientServer-Service/GestionePresenze/My_Template1Service/Models/my-template1-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { MyTemplate1Service } from '../../../ClientServer-Service/GestionePresenze/My_Template1Service/my-template1.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';

@Component({
  selector: 'app-mytemplate1-list-page',
  templateUrl: './mytemplate1-list-page.component.html',
  styleUrls: ['./mytemplate1-list-page.component.scss'],
  standalone: false
})
export class MyTemplate1ListPageComponent implements OnInit {
  public title!: string;
  public searchText!: string;
  public myTemplate1List: My_Template1Model[] | null = null;
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              public fabMenuService: FabMenuService,
              private myTemplate1Service: MyTemplate1Service,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService) {
    this.title = 'MyTemplate1';
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {
    let request: GenericRequest<My_template1_GetAllInModel> = new GenericRequest<My_template1_GetAllInModel>(My_template1_GetAllInModel);
    this.myTemplate1Service.GetAll(request).subscribe(res => {
      this.myTemplate1List = res.data.my_Template1;
    });
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem('Nuovo', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/mytemplate1edit', {
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
    this.navCtrl.navigateForward('/mytemplate1edit', {
      state: { id: item.id }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  isAdmin(item: any) {
    // Personalizza la logica se necessario
    return false;
  }

  getAll() {
    if (!this.myTemplate1List) return [];
    // Ordina per 'id' come fallback
    return this.myTemplate1List.sort((a, b) => a.id - b.id);
  }
}
