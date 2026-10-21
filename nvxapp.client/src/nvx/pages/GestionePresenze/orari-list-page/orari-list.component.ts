import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';

import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Par_Orario_DeleteInModel, Par_Orario_GetAllInModel, Par_OrarioModel } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/Models/par-orario-model';
import { ParOrarioService } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/par-orario.service';
import { map, catchError } from 'rxjs';
import { Az_SediRepartoDeleteInModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { ParTable, SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';

@Component({
  selector: 'app-orari-list',
  templateUrl: './orari-list.component.html',
  styleUrls: ['./orari-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class OrariListPageComponent {

  public title: string;
  public searchText: string = '';
  /** dalla cache caricata al login: si aggiorna da sola dopo salvataggi e cancellazioni (refresh mirato) */
  public get par_OrarioList(): Par_OrarioModel[] | null {
    return this.sharedParameterGestionePresenzeService.Par_Orario;
  }
  public readonly columns: NvxListColumn<Par_OrarioModel>[] = [
    { key: 'codice', title: 'Codice', width: '140px', mobile: 'subtitle' },
    { key: 'descrizione', title: 'Descrizione', mobile: 'title' }
  ];
  public actions: NvxListAction<Par_OrarioModel>[] = [];
  public readonly sortList = (a: Par_OrarioModel, b: Par_OrarioModel) => (a.descrizione ?? '').localeCompare(b.descrizione ?? '');
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
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

    this.actions = [{ button: this.btnDelete, visible: item => !(item as any).default }, { button: this.btnEdit }];
  }




  ionViewWillEnter() {
    

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
          this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Par_Orario);
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

