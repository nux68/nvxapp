import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_CausaliInModel, Par_CausaliModel } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParCausaliService } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/par-causali.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

@Component({
  selector: 'app-causali-list-page',
  templateUrl: './causali-list-page.component.html',
  styleUrls: ['./causali-list-page.component.scss'],
  standalone: false
})
export class CausaliListPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public par_CausaliList: Par_CausaliModel[] | null = null;
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              public fabMenuService: FabMenuService,
              private parCausaliService: ParCausaliService,
              private userInterfaceService: UserInterfaceService,
              private collectionDialogService: CollectionDialogService,
              private userNavigationService: UserNavigationService) {

    this.title = 'Causali';
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {
    let request: GenericRequest<Par_CausaliInModel> = new GenericRequest<Par_CausaliInModel>(Par_CausaliInModel);
    this.parCausaliService.GetAll(request).subscribe(res => {
      this.par_CausaliList = res.data.par_Causali;
    });

    this.fabMenuService.fabMenuItem = [
      new FabMenuItem('Nuova Causale', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/causaliedit', {
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
    this.navCtrl.navigateForward('/causaliedit', {
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
    if (!this.par_CausaliList) {
      return [];
    }
    const sorted_CausaliList = this.par_CausaliList.sort((a, b) =>
      a.descrizione.localeCompare(b.descrizione)
    );
    return sorted_CausaliList;
  }
}
