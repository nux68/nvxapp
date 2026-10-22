import { TranslocoService } from '@jsverse/transloco';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_CompetenzaDeleteInModel, Par_CompetenzaModel, Par_Competenza_GetAll_InModel } from '../../../ClientServer-Service/GestionePresenze/Par_Competenza/Models/par-competenza-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParCompetenzaService } from '../../../ClientServer-Service/GestionePresenze/Par_Competenza/par-competenza.service';
import { map, catchError } from 'rxjs';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { ParTable, SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
//import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';

@Component({
  selector: 'app-competence-list-page',
  templateUrl: './competence-list-page.component.html',
  styleUrls: ['./competence-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CompetenceListPageComponent {
  public title: string;
  public searchText!: string;
  /** dalla cache caricata al login: si aggiorna da sola dopo salvataggi e cancellazioni (refresh mirato) */
  public get par_CompetenzaList(): Par_CompetenzaModel[] | null {
    return this.sharedParameterGestionePresenzeService.Par_Competenza;
  }
  public readonly columns: NvxListColumn<Par_CompetenzaModel>[] = [
    { key: 'descrizione', title: this.transloco.translate('attendance.pages.competenceList.columnCompetence'), mobile: 'title' }
  ];
  public actions: NvxListAction<Par_CompetenzaModel>[] = [];
  // prima le voci di default, poi per descrizione
  public readonly sortList = (a: Par_CompetenzaModel, b: Par_CompetenzaModel) =>
    a.default === b.default ? (a.descrizione ?? '').localeCompare(b.descrizione ?? '') : (a.default ? -1 : 1);
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private navCtrl: NavController,
    public fabMenuService: FabMenuService,
    private refresherService: RefresherService,
    private parCompetenzaService: ParCompetenzaService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService,
    private collectionDialogService: CollectionDialogService,
    private transloco: TranslocoService
  ) {
    this.title = this.transloco.translate('attendance.pages.competenceList.title');
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    this.actions = [{ button: this.btnDelete, visible: item => !item.default }, { button: this.btnEdit }];
  }


  ionViewWillEnter() {

    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('attendance.pages.competenceList.fabAdd'), 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/competenceedit', { state: { id: 0 } });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }
  

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/competenceedit', { state: { id: item.id } });
  }

  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.competenceList.confirmDelete'));
    if (result) {

        let request: GenericRequest<Par_CompetenzaDeleteInModel> = new GenericRequest<Par_CompetenzaDeleteInModel>(Par_CompetenzaDeleteInModel);
        request.data.id = item.id ;
        return this.parCompetenzaService.Par_CompetenzaDelete(request).pipe(
          map(() => {
            this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Par_Competenza);
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
