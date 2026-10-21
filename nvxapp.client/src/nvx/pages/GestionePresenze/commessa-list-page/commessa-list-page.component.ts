import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Az_Commessa_GetAll_InModel, Az_CommessaModel, Az_CommessaDeleteInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Commessa/Models/az-commessa-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AzCommessaService } from '../../../ClientServer-Service/GestionePresenze/Az_Commessa/az-commessa.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { map, catchError } from 'rxjs';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { ParTable, SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';

@Component({
  selector: 'app-commessa-list-page',
  templateUrl: './commessa-list-page.component.html',
  styleUrls: ['./commessa-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CommessaListPageComponent {

  public title!: string;
  public searchText!: string;
  /** dalla cache caricata al login: si aggiorna da sola dopo salvataggi e cancellazioni (refresh mirato) */
  public get commessaList(): Az_CommessaModel[] | null {
    return this.sharedParameterGestionePresenzeService.Az_Commessa;
  }
  public readonly columns: NvxListColumn<Az_CommessaModel>[] = [
    { key: 'descrizione', title: 'Commessa', mobile: 'title' }
  ];
  public actions: NvxListAction<Az_CommessaModel>[] = [];
  public readonly sortList = (a: Az_CommessaModel, b: Az_CommessaModel) => (a.descrizione ?? '').localeCompare(b.descrizione ?? '');
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private azCommessaService: AzCommessaService,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService,
              private collectionDialogService: CollectionDialogService,
              private refresherService: RefresherService) {

    this.title = 'Commesse';
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    this.actions = [{ button: this.btnDelete, visible: item => !item.default }, { button: this.btnEdit }];
  }


  ionViewWillEnter() {

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Nuova Commessa', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/commessaedit', {
          state: { id: 0 }
        });
      }),



    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }


  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/commessaedit', {
      state: { id: item.id }
    });
  }

  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione della commessa?');
    if (result) {

      let request: GenericRequest<Az_CommessaDeleteInModel> = new GenericRequest<Az_CommessaDeleteInModel>(Az_CommessaDeleteInModel);
      request.data.id = item.id;
      this.azCommessaService.Az_CommessaDelete(request).pipe(
        map(() => {
          this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Az_Commessa);
          return true;
        }),
        catchError((error) => {
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
