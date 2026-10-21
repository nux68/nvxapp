import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_Causali_DeleteInModel, Par_CausaliInModel, Par_CausaliModel } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParCausaliService } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/par-causali.service';
import { SharedParameterGestionePresenzeService, ParTable } from '../../../shared/shared-parameter-gestione-presenze.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { map, catchError } from 'rxjs';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';

@Component({
  selector: 'app-causali-list-page',
  templateUrl: './causali-list-page.component.html',
  styleUrls: ['./causali-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CausaliListPageComponent {

  public title!: string;
  public searchText!: string;
  /** dalla cache caricata al login: si aggiorna da sola dopo salvataggi e cancellazioni (refresh mirato) */
  public get par_CausaliList(): Par_CausaliModel[] | null {
    return this.sharedParameterGestionePresenzeService.Par_Causali;
  }
  public readonly columns: NvxListColumn<Par_CausaliModel>[] = [
    { key: 'codice', title: 'Codice', width: '140px', mobile: 'subtitle' },
    { key: 'descrizione', title: 'Descrizione', mobile: 'title' }
  ];
  public actions: NvxListAction<Par_CausaliModel>[] = [];
  public readonly sortList = (a: Par_CausaliModel, b: Par_CausaliModel) => (a.descrizione ?? '').localeCompare(b.descrizione ?? '');
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              public fabMenuService: FabMenuService,
              private parCausaliService: ParCausaliService,
              private userInterfaceService: UserInterfaceService,
              private refresherService: RefresherService,
              private collectionDialogService: CollectionDialogService,
              private userNavigationService: UserNavigationService) {

    this.title = 'Causali';
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    this.actions = [{ button: this.btnDelete, visible: item => !(item as any).default }, { button: this.btnEdit }];
  }


  ionViewWillEnter() {


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



  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione della causale');
    if (result) {

      const request: GenericRequest<Par_Causali_DeleteInModel> = new GenericRequest<Par_Causali_DeleteInModel>(Par_Causali_DeleteInModel);
      request.data.id = item.id;
      this.parCausaliService.Par_CausaliDelete(request).pipe(
        map(() => {
          this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Par_Causali);
          return true;
        }),
        catchError((error: any) => {
          console.error('Errore durante la chiamata API:', error);
          return [false];
        })
      ).subscribe();

    }

  }

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/causaliedit', {
      state: { id: item.id }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }


}
