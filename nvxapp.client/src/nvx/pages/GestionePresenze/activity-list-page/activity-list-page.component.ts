import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Az_AttivitaModel, Az_Attivita_GetAll_InModel } from '../../../ClientServer-Service/GestionePresenze/Az_Attivita/Models/az-attivita-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AzAttivitaService } from '../../../ClientServer-Service/GestionePresenze/Az_Attivita/az-attivita.service';

@Component({
  selector: 'app-activity-list-page',
  templateUrl: './activity-list-page.component.html',
  styleUrls: ['./activity-list-page.component.scss'],
  standalone: false
})
export class ActivityListPageComponent implements OnInit {
  public title = 'Attività';
  public searchText!: string;
  public az_AttivitaList: Az_AttivitaModel[] | null = null;
  public btnEdit: ButtonItem;

  constructor(
    private navCtrl: NavController,
    public fabMenuService: FabMenuService,
    private azAttivitaService: AzAttivitaService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService
  ) {
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {
    let request: GenericRequest<Az_Attivita_GetAll_InModel> = new GenericRequest<Az_Attivita_GetAll_InModel>(Az_Attivita_GetAll_InModel);
    this.azAttivitaService.GetAll(request).subscribe(res => {
      this.az_AttivitaList = res.data.az_Attivita;
    });

    this.fabMenuService.fabMenuItem = [
      new FabMenuItem('Nuova Attività', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/activityedit', { state: { id: 0 } });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  ngOnInit() {}

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/activityedit', { state: { id: item.id } });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  getAll() {
    return this.az_AttivitaList?.sort((a, b) => a.descrizione.localeCompare(b.descrizione));
  }
}
