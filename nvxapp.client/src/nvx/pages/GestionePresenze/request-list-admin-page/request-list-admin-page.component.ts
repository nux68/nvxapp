import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_GiustificativiInModel, Par_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParGiustificativiService } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/par-giustificativi.service';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { Dip_GG_Richiesta_GetAll4Admin_InModel, Dip_GG_Richiesta_GetAll4User_InModel, Dip_GG_RichiestaModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { MonthNavigatorService } from '../../../Utility/infrastructure/month-navigator.service';



@Component({
  selector: 'app-request-list-admin-page',
  templateUrl: './request-list-admin-page.component.html',
  styleUrls: ['./request-list-admin-page.component.scss'],
  standalone: false
}) 
export class RequestListAdminPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public dip_GG_RichiestaList: Dip_GG_RichiestaModel[] | null = null;
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
    private dipGGRichiestaService: DipGGRichiestaService,
    public fabMenuService: FabMenuService,
    private monthNavigatorService: MonthNavigatorService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService) {

    this.title = 'Request List Admin';
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  //const record = _dip_Anagrafica?.find(dip =>
  //  dip.dip_RapportoLavoro.some(rapporto => rapporto.id === 1)
  //);

  ionViewWillEnter() {

    let request: GenericRequest<Dip_GG_Richiesta_GetAll4Admin_InModel> = new GenericRequest<Dip_GG_Richiesta_GetAll4Admin_InModel>(Dip_GG_Richiesta_GetAll4Admin_InModel);
    request.data.month = this.monthNavigatorService.currentMonth + 1;
    request.data.year = this.monthNavigatorService.currentYear;

    this.dipGGRichiestaService.GetAll4Admin(request).subscribe(res => {
      this.dip_GG_RichiestaList = res.data.dip_GG_Richiesta;
    });

    //this.fabMenuService.fabMenuItem = [

    //  new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
    //    this.navCtrl.navigateForward('/justificationedit', {
    //      state: { id: 0 }
    //    });
    //  }),

    //];

  }

  ionViewWillLeave() {
    //this.fabMenuService.fabMenuItem = [];
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
    //const sorted_GiustificativiList = this.par_GiustificativiList.sort((a, b) =>
    //  a.descrizione.localeCompare(b.descrizione)
    //);
    //return sorted_GiustificativiList;

    return this.dip_GG_RichiestaList;
  }


}
