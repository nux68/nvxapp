import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_CompetenzaModel, Par_Competenza_GetAll_InModel } from '../../../ClientServer-Service/GestionePresenze/Par_Competenza/Models/par-competenza-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParCompetenzaService } from '../../../ClientServer-Service/GestionePresenze/Par_Competenza/par-competenza.service';

@Component({
  selector: 'app-competence-list-page',
  templateUrl: './competence-list-page.component.html',
  styleUrls: ['./competence-list-page.component.scss'],
  standalone: false
})
export class CompetenceListPageComponent implements OnInit {
  public title = 'Competenze';
  public searchText!: string;
  public par_CompetenzaList: Par_CompetenzaModel[] | null = null;
  public btnEdit: ButtonItem;

  constructor(
    private navCtrl: NavController,
    public fabMenuService: FabMenuService,
    private parCompetenzaService: ParCompetenzaService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService
  ) {
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {
    let request: GenericRequest<Par_Competenza_GetAll_InModel> = new GenericRequest<Par_Competenza_GetAll_InModel>(Par_Competenza_GetAll_InModel);
    this.parCompetenzaService.GetAll(request).subscribe(res => {
      this.par_CompetenzaList = res.data.par_Competenza;
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
    return this.par_CompetenzaList?.sort((a, b) => a.descrizione.localeCompare(b.descrizione));
  }
}
