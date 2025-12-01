import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';

import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Par_Orario_DeleteInModel, Par_Orario_GetAllInModel, Par_OrarioModel } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/Models/par-orario-model';
import { ParOrarioService } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/par-orario.service';
import { map, catchError } from 'rxjs';
import { Az_SediRepartoDeleteInModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';

@Component({
  selector: 'app-orari-list',
  templateUrl: './orari-list.component.html',
  styleUrls: ['./orari-list.component.scss'],
  standalone: false
})
export class OrariListPageComponent implements OnInit {

  public title: string;
  public searchText: string = '';
  public par_OrarioList: Par_OrarioModel[] = [];
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(
    private navCtrl: NavController,
    public fabMenuService: FabMenuService,
    private parOrarioService: ParOrarioService,
    private collectionDialogService: CollectionDialogService,
    private refresherService: RefresherService,
    private userInterfaceService: UserInterfaceService
  ) {
    this.title = 'Orari';
    this.btnEdit = this.userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
  }

  ngOnInit() { }


  private loadData() {
    let request = new GenericRequest<Par_Orario_GetAllInModel>(Par_Orario_GetAllInModel);
    this.parOrarioService.GetAll(request).subscribe(res => {
      this.par_OrarioList = res.data.par_Orario;
    });
  }

  ionViewWillEnter() {
    
    this.loadData();

    this.fabMenuService.fabMenuItem = [

        new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
          this.navCtrl.navigateForward('/orariedit', {
            state: { id: 0 }
          });
        }),

    ];


  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  handleButtonEditClick = (item: Par_OrarioModel) => {
    this.navCtrl.navigateForward('/orariedit', {
      state: { id: item.id }
    });
  }

  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione dell orario');
    if (result) {

      const request: GenericRequest<Par_Orario_DeleteInModel> = new GenericRequest<Par_Orario_DeleteInModel>(Par_Orario_DeleteInModel);
      request.data.id = item.id;
      this.parOrarioService.Par_OrarioDelete(request).pipe(
        map(() => {
          this.refresherService.SharedParameterGestionePresenze_triggerRefresh();
          this.loadData();
          return true;
        }),
        catchError((error: any) => {
          console.error('Errore durante la chiamata API:', error);
          return [false];
        })
      ).subscribe();

    }

  }


  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  isAdmin(item: any) {
    return false;
  }

  getAll() {
    if (!this.par_OrarioList) return [];
    return this.par_OrarioList.sort((a, b) =>
      a.descrizione.localeCompare(b.descrizione)
    );
  }
}

