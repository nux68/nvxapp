import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { AccountService } from '../../../ClientServer-Service/Infrastructure/Account/account.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_GiustificativiInModel, Par_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParGiustificativiService } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/par-giustificativi.service';


@Component({
  selector: 'app-justification-list-page',
  templateUrl: './justification-list-page.component.html',
  styleUrls: ['./justification-list-page.component.scss'],
  standalone: false
}) 
export class JustificationListPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public par_GiustificativiList: Par_GiustificativiModel[] | null = null;
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
              private parGiustificativiService: ParGiustificativiService,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService) {

    this.title = 'Justifications';
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {

    let request: GenericRequest<Par_GiustificativiInModel> = new GenericRequest<Par_GiustificativiInModel>(Par_GiustificativiInModel);
    this.parGiustificativiService.GetAll(request).subscribe(res => {
      this.par_GiustificativiList = res.data.par_Giustificativi;
    });

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/justificationedit', {
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
    this.navCtrl.navigateForward('/justificationedit', {
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
    const sorted_GiustificativiList = this.par_GiustificativiList.sort((a, b) =>
      a.descrizione.localeCompare(b.descrizione)
    );
    return sorted_GiustificativiList;
  }


}
