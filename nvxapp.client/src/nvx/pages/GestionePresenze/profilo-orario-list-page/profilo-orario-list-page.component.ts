import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';

import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Par_Orario_DeleteInModel, Par_Orario_GetAllInModel, Par_OrarioModel } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/Models/par-orario-model';
import { map, catchError } from 'rxjs';
import { Az_SediRepartoDeleteInModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { ParProfiloOrarioService } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/par-profilo-orario.service';
import { Par_ProfiloOrario_DeleteInModel, Par_ProfiloOrario_GetAllInModel, Par_ProfiloOrarioModel } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/Models/par-profilo-orario-model';

@Component({
  selector: 'app-profilo-orario-list-page',
  templateUrl: './profilo-orario-list-page.component.html',
  styleUrls: ['./profilo-orario-list-page.component.scss'],
  standalone: false
})
export class ProfiloOrarioListPageComponent implements OnInit {

  public title: string;
  public searchText: string = '';
  public par_ProfiloOrarioList: Par_ProfiloOrarioModel[] = [];
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(
    private navCtrl: NavController,
    public fabMenuService: FabMenuService,
    private parProfiloOrarioService: ParProfiloOrarioService,
    private collectionDialogService: CollectionDialogService,
    private refresherService: RefresherService,
    private userInterfaceService: UserInterfaceService
  ) {
    this.title = 'Profili Orari';
    this.btnEdit = this.userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
  }

  ngOnInit() { }


  private loadData() {
    let request = new GenericRequest<Par_ProfiloOrario_GetAllInModel>(Par_ProfiloOrario_GetAllInModel);
    this.parProfiloOrarioService.GetAll(request).subscribe(res => {
      this.par_ProfiloOrarioList = res.data.par_ProfiloOrario;
    });
  }

  ionViewWillEnter() {

    this.loadData();

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/profiliorariedit', {
          state: { id: 0 }
        });
      }),

    ];


  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  handleButtonEditClick = (item: Par_ProfiloOrarioModel) => {
    this.navCtrl.navigateForward('/profiliorariedit', {
      state: { id: item.id }
    });
  }

  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione dell Profilo Orario');
    if (result) {

      const request: GenericRequest<Par_ProfiloOrario_DeleteInModel> = new GenericRequest<Par_ProfiloOrario_DeleteInModel>(Par_ProfiloOrario_DeleteInModel);
      request.data.id = item.id;
      this.parProfiloOrarioService.Par_ProfiloOrarioDelete(request).pipe(
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
    if (!this.par_ProfiloOrarioList) return [];
    return this.par_ProfiloOrarioList.sort((a, b) =>
      a.descrizione.localeCompare(b.descrizione)
    );
  }
}

