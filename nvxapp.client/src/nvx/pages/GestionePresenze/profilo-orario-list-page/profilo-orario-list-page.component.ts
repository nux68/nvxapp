import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';

import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Par_Orario_DeleteInModel, Par_Orario_GetAllInModel, Par_OrarioModel } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/Models/par-orario-model';
import { map, catchError } from 'rxjs';
import { Az_SediRepartoDeleteInModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { ParProfiloOrarioService } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/par-profilo-orario.service';
import { Par_ProfiloOrario_DeleteInModel, Par_ProfiloOrario_GetAllInModel, Par_ProfiloOrarioModel } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/Models/par-profilo-orario-model';
import { ParTable, SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';

@Component({
  selector: 'app-profilo-orario-list-page',
  templateUrl: './profilo-orario-list-page.component.html',
  styleUrls: ['./profilo-orario-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ProfiloOrarioListPageComponent {

  public title: string;
  public searchText: string = '';
  /** dalla cache caricata al login: si aggiorna da sola dopo salvataggi e cancellazioni (refresh mirato) */
  public get par_ProfiloOrarioList(): Par_ProfiloOrarioModel[] | null {
    return this.sharedParameterGestionePresenzeService.Par_ProfiloOrario;
  }
  public readonly columns: NvxListColumn<Par_ProfiloOrarioModel>[] = [
    { key: 'codice', title: 'Codice', width: '140px', mobile: 'subtitle' },
    { key: 'descrizione', title: 'Descrizione', mobile: 'title' }
  ];
  public actions: NvxListAction<Par_ProfiloOrarioModel>[] = [];
  public readonly sortList = (a: Par_ProfiloOrarioModel, b: Par_ProfiloOrarioModel) => (a.descrizione ?? '').localeCompare(b.descrizione ?? '');
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
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

    this.actions = [{ button: this.btnDelete, visible: item => !(item as any).default }, { button: this.btnEdit }];
  }




  ionViewWillEnter() {


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
          this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Par_ProfiloOrario);
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


}

