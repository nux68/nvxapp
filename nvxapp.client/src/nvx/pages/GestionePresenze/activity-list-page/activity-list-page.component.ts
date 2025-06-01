import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_AttivitaModel, Par_Attivita_GetAll_InModel } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/Models/par-attivita-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParAttivitaService } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/par-attivita.service';

@Component({
  selector: 'app-activity-list-page',
  templateUrl: './activity-list-page.component.html',
  styleUrls: ['./activity-list-page.component.scss'],
  standalone: false
})
export class ActivityListPageComponent implements OnInit {
  public title = 'Attività';
  public searchText!: string;
  public par_AttivitaList: Par_AttivitaModel[] | null = null;
  public btnEdit: ButtonItem;

  constructor(
    private navCtrl: NavController,
    public fabMenuService: FabMenuService,
    private azAttivitaService: ParAttivitaService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService
  ) {
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {
    let request: GenericRequest<Par_Attivita_GetAll_InModel> = new GenericRequest<Par_Attivita_GetAll_InModel>(Par_Attivita_GetAll_InModel);
    this.azAttivitaService.GetAll(request).subscribe(res => {
      this.par_AttivitaList = res.data.par_Attivita;
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
    return this.par_AttivitaList?.sort((a, b) => a.descrizione.localeCompare(b.descrizione));
  }
}
