import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_AttivitaDeleteInModel, Par_AttivitaModel, Par_Attivita_GetAll_InModel } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/Models/par-attivita-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParAttivitaService } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/par-attivita.service';
import { map, catchError } from 'rxjs';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';

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
  public btnDelete: ButtonItem;

  constructor(
    private navCtrl: NavController,
    public fabMenuService: FabMenuService,
    private azAttivitaService: ParAttivitaService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService,
    private refresherService: RefresherService
  ) {
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
  }

  private loadData() {
    let request: GenericRequest<Par_Attivita_GetAll_InModel> = new GenericRequest<Par_Attivita_GetAll_InModel>(Par_Attivita_GetAll_InModel);
    this.azAttivitaService.GetAll(request).subscribe(res => {
      this.par_AttivitaList = res.data.par_Attivita;
    });
  }

  ionViewWillEnter() {
    this.loadData();

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

  handleButtonDeleteClick = (item: any) => {
    let request: GenericRequest<Par_AttivitaDeleteInModel> = new GenericRequest<Par_AttivitaDeleteInModel>(Par_AttivitaDeleteInModel);
    request.data.id = item.id;
    return this.azAttivitaService.Par_AttivitaDelete(request).pipe(
      map(() => {
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh();
        this.loadData();
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false];
      })
    ).subscribe();
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  getAll() {
    return this.par_AttivitaList?.sort((a, b) => a.descrizione.localeCompare(b.descrizione));
  }
}
