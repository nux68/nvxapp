import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Az_CompetenzaModel, Az_Competenza_GetAll_InModel } from '../../../ClientServer-Service/GestionePresenze/Az_Competenza/Models/az-competenza-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AzCompetenzaService } from '../../../ClientServer-Service/GestionePresenze/Az_Competenza/az-competenza.service';

@Component({
  selector: 'app-competence-list-page',
  templateUrl: './competence-list-page.component.html',
  styleUrls: ['./competence-list-page.component.scss'],
  standalone: false
})
export class CompetenceListPageComponent implements OnInit {
  public title = 'Competenze';
  public searchText!: string;
  public az_CompetenzaList: Az_CompetenzaModel[] | null = null;
  public btnEdit: ButtonItem;

  constructor(
    private navCtrl: NavController,
    public fabMenuService: FabMenuService,
    private azCompetenzaService: AzCompetenzaService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService
  ) {
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {
    let request: GenericRequest<Az_Competenza_GetAll_InModel> = new GenericRequest<Az_Competenza_GetAll_InModel>(Az_Competenza_GetAll_InModel);
    this.azCompetenzaService.GetAll(request).subscribe(res => {
      this.az_CompetenzaList = res.data.az_Competenza;
    });

    this.fabMenuService.fabMenuItem = [
      new FabMenuItem('Nuova Competenza', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/competenceedit', { state: { id: 0 } });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  ngOnInit() {}

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/competenceedit', { state: { id: item.id } });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  getAll() {
    return this.az_CompetenzaList?.sort((a, b) => a.descrizione.localeCompare(b.descrizione));
  }
}
