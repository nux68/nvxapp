import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_GiustificativiInModel, Par_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { SharedParameterGestionePresenzeService, ParTable } from '../../../shared/shared-parameter-gestione-presenze.service';
import { AzSediRepartoService } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/az-sedi-reparto.service';
import { Az_SediReparto_GetAll_InModel, Az_SediRepartoModel, Az_SediRepartoDeleteInModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { map, catchError } from 'rxjs';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

@Component({
  selector: 'app-department-list-page',
  templateUrl: './department-list-page.component.html',
  styleUrls: ['./department-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DepartmentListPageComponent {

  public title!: string;
  public searchText!: string;
  public readonly columns: NvxListColumn<Az_SediRepartoModel>[] = [{ key: 'descrizione', title: 'Reparto' }];
  public actions: NvxListAction<Az_SediRepartoModel>[] = [];
  // prima le voci di default, poi per descrizione
  public readonly sortList = (a: Az_SediRepartoModel, b: Az_SediRepartoModel) =>
    a.default === b.default ? (a.descrizione ?? '').localeCompare(b.descrizione ?? '') : (a.default ? -1 : 1);

  // reparti della sede scelta, dalla cache caricata al login; ricalcolati solo se cambiano cache o sede
  private _src: Az_SediRepartoModel[] | null = null;
  private _srcSedi: number | undefined;
  private _list: Az_SediRepartoModel[] | null = null;
  public get az_SediRepartoList(): Az_SediRepartoModel[] | null {
    const src = this.sharedParameterGestionePresenzeService.Az_SediReparto;
    if (src !== this._src || this.curr_idAz_Sedi !== this._srcSedi) {
      this._src = src;
      this._srcSedi = this.curr_idAz_Sedi;
      this._list = src?.filter(x => x.idAz_Sedi === this.curr_idAz_Sedi) ?? null;
    }
    return this._list;
  }
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;
  public curr_idAz_Sedi: number;

  constructor(private navCtrl: NavController,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    public fabMenuService: FabMenuService,
    private azSediRepartoService: AzSediRepartoService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService,
    private collectionDialogService: CollectionDialogService,
    private refresherService: RefresherService) {

    this.title = 'Reparti';
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    this.actions = [{ button: this.btnDelete, visible: item => !item.default }, { button: this.btnEdit }];


  }


  ionViewWillEnter() {

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/departmentedit', {
          state: { id: 0, idAz_Sedi: this.curr_idAz_Sedi }
        });
      }),

    ];

  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }



  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/departmentedit', {
      state: { id: item.id, idAz_Sedi: this.curr_idAz_Sedi }
    });
  }

  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione del reparto');
    if (result) {

        const request: GenericRequest<Az_SediRepartoDeleteInModel> = new GenericRequest<Az_SediRepartoDeleteInModel>(Az_SediRepartoDeleteInModel);
        request.data.id = item.id;
        this.azSediRepartoService.Az_SediRepartoDelete(request).pipe(
          map(() => {
            this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Az_SediReparto);
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




  onPeriodChange(period: { year: number, month: number } | undefined): void { }
  onCurrentUserChanged(userId: string[] | undefined): void { }
  onSedeChanged(sediId: number | undefined): void {
    this.curr_idAz_Sedi = sediId;
  }
  onRepartiChanged(repartoIds: number[] | undefined): void { }
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void { }



}
