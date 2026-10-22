import { TranslocoService } from '@jsverse/transloco';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Az_Sedi_GetAll_InModel, Az_SediModel, Az_SediDeleteInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Sedi/Models/az-sedi-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AzSediService } from '../../../ClientServer-Service/GestionePresenze/Az_Sedi/az-sedi.service';
import { map, catchError } from 'rxjs';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { ParTable, SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';

@Component({
  selector: 'app-az-sedi-list-page',
  templateUrl: './az-sedi-list-page.component.html',
  styleUrls: ['./az-sedi-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class AzSediListPageComponent {

  public title: string;
  public searchText!: string;
  /** dalla cache caricata al login: si aggiorna da sola dopo salvataggi e cancellazioni (refresh mirato) */
  public get azSediList(): Az_SediModel[] | null {
    return this.sharedParameterGestionePresenzeService.Az_Sedi;
  }
  public readonly columns: NvxListColumn<Az_SediModel>[] = [
    { key: 'descrizione', title: this.transloco.translate('attendance.pages.azSediList.columnSite'), mobile: 'title' }
  ];
  public actions: NvxListAction<Az_SediModel>[] = [];
  // prima le voci di default, poi per descrizione
  public readonly sortList = (a: Az_SediModel, b: Az_SediModel) =>
    a.default === b.default ? (a.descrizione ?? '').localeCompare(b.descrizione ?? '') : (a.default ? -1 : 1);
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private azSediService: AzSediService,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService,
              private collectionDialogService: CollectionDialogService,
              private refresherService: RefresherService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('attendance.pages.azSediList.title');

    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    this.actions = [{ button: this.btnDelete, visible: item => !item.default }, { button: this.btnEdit }];
  }


  ionViewWillEnter() {
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('attendance.pages.azSediList.fabAdd'), 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/azsediedit', {
          state: { id: 0 }
        });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }


  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/azsediedit', {
      state: { id: item.id }
    });
  }

  
  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.azSediList.confirmDelete'));
    if (result) {

        let request: GenericRequest<Az_SediDeleteInModel> = new GenericRequest<Az_SediDeleteInModel>(Az_SediDeleteInModel);
        request.data.id = item.id;
        return this.azSediService.AzSediDelete(request).pipe(
          map(() => {
            this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Az_Sedi);
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
