import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_GiustificativiInModel, Par_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { AzSediRepartoService } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/az-sedi-reparto.service';
import { Az_SediReparto_GetAll_InModel, Az_SediRepartoModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';

@Component({
  selector: 'app-department-list-page',
  templateUrl: './department-list-page.component.html',
  styleUrls: ['./department-list-page.component.scss'],
  standalone: false
})
export class DepartmentListPageComponent  implements OnInit {

  public title!: string;
  public searchText!: string;
  public az_SediRepartoList: Az_SediRepartoModel[] | null = null;
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    public fabMenuService: FabMenuService,
    private azSediRepartoService: AzSediRepartoService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService) {

    this.title = 'Departments';
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    //TODO DISAB SHARED DATA
    //this.sharedParameterGestionePresenzeService.Par_Giustificativi$.subscribe(res => {
    //  this.par_GiustificativiList = this.sharedParameterGestionePresenzeService.Par_Giustificativi;
    //});

  }

  ionViewWillEnter() {

    //TODO DISAB SHARED DATA
    let request: GenericRequest<Az_SediReparto_GetAll_InModel> = new GenericRequest<Az_SediReparto_GetAll_InModel>(Az_SediReparto_GetAll_InModel);
    this.azSediRepartoService.GetAll(request).subscribe(res => {
      this.az_SediRepartoList = res.data.az_SediReparto;
    });


    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/departmentedit', {
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
    this.navCtrl.navigateForward('/departmentedit', {
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
    const sorted_GiustificativiList = this.az_SediRepartoList.sort((a, b) =>
      a.descrizione.localeCompare(b.descrizione)
    );
    return sorted_GiustificativiList;
  }



}
