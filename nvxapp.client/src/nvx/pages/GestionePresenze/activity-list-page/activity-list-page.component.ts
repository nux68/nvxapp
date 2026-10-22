import { TranslocoService } from '@jsverse/transloco';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_AttivitaDeleteInModel, Par_AttivitaModel, Par_Attivita_GetAll_InModel } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/Models/par-attivita-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParAttivitaService } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/par-attivita.service';
import { map, catchError } from 'rxjs';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { ParTable, SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';

@Component({
  selector: 'app-activity-list-page',
  templateUrl: './activity-list-page.component.html',
  styleUrls: ['./activity-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ActivityListPageComponent {
  public title: string;
  public searchText!: string;
  /** dalla cache caricata al login: si aggiorna da sola dopo salvataggi e cancellazioni (refresh mirato) */
  public get par_AttivitaList(): Par_AttivitaModel[] | null {
    return this.sharedParameterGestionePresenzeService.Par_Attivita;
  }
  public readonly columns: NvxListColumn<Par_AttivitaModel>[] = [
    { key: 'descrizione', title: this.transloco.translate('attendance.pages.activityList.columnActivity'), mobile: 'title' }
  ];
  public actions: NvxListAction<Par_AttivitaModel>[] = [];
  // prima le voci di default, poi per descrizione
  public readonly sortList = (a: Par_AttivitaModel, b: Par_AttivitaModel) =>
    a.default === b.default ? (a.descrizione ?? '').localeCompare(b.descrizione ?? '') : (a.default ? -1 : 1);
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private navCtrl: NavController,
    public fabMenuService: FabMenuService,
    private azAttivitaService: ParAttivitaService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService,
    private collectionDialogService: CollectionDialogService,
    private refresherService: RefresherService,
    private transloco: TranslocoService
  ) {
    this.title = this.transloco.translate('attendance.pages.activityList.title');
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    this.actions = [{ button: this.btnDelete, visible: item => !item.default }, { button: this.btnEdit }];
  }


  ionViewWillEnter() {

    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('attendance.pages.activityList.fabAdd'), 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/activityedit', { state: { id: 0 } });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }


  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/activityedit', { state: { id: item.id } });
  }

  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.activityList.confirmDelete'));
    if (result) {

        let request: GenericRequest<Par_AttivitaDeleteInModel> = new GenericRequest<Par_AttivitaDeleteInModel>(Par_AttivitaDeleteInModel);
        request.data.id = item.id;
        return this.azAttivitaService.Par_AttivitaDelete(request).pipe(
          map(() => {
            this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Par_Attivita);
            return true;
          }),
          catchError((error) => {
            console.error('Errore durante la chiamata API:', error);
            return [false];
          })
          ).subscribe();
    }

    return true;

  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

}
