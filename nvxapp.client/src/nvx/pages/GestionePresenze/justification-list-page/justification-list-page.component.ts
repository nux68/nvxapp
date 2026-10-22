import { TranslocoService } from '@jsverse/transloco';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_Giustificativi_DeleteInModel, Par_GiustificativiInModel, Par_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParGiustificativiService } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/par-giustificativi.service';
import { SharedParameterGestionePresenzeService, ParTable } from '../../../shared/shared-parameter-gestione-presenze.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { map, catchError } from 'rxjs';
import { Par_Orario_DeleteInModel } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/Models/par-orario-model';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';


@Component({
  selector: 'app-justification-list-page',
  templateUrl: './justification-list-page.component.html',
  styleUrls: ['./justification-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
}) 
export class JustificationListPageComponent {

  public title!: string;
  public searchText!: string;
  /** dalla cache caricata al login: si aggiorna da sola dopo salvataggi e cancellazioni (refresh mirato) */
  public get par_GiustificativiList(): Par_GiustificativiModel[] | null {
    return this.sharedParameterGestionePresenzeService.Par_Giustificativi;
  }
  public readonly columns: NvxListColumn<Par_GiustificativiModel>[] = [
    { key: 'codice', title: this.transloco.translate('attendance.pages.justificationList.columnCode'), width: '140px', mobile: 'subtitle' },
    { key: 'descrizione', title: this.transloco.translate('attendance.pages.justificationList.columnDescription'), mobile: 'title' },
    { key: 'tipoContatore', title: this.transloco.translate('attendance.pages.justificationList.columnCounter'), mobile: 'subtitle' }
  ];
  public actions: NvxListAction<Par_GiustificativiModel>[] = [];
  public readonly sortList = (a: Par_GiustificativiModel, b: Par_GiustificativiModel) => (a.descrizione ?? '').localeCompare(b.descrizione ?? '');
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              public fabMenuService: FabMenuService,
              private parGiustificativiService: ParGiustificativiService,
              private userInterfaceService: UserInterfaceService,
              private collectionDialogService: CollectionDialogService,
              private refresherService: RefresherService,
              private userNavigationService: UserNavigationService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('attendance.pages.justificationList.title');
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    this.actions = [{ button: this.btnDelete, visible: item => !(item as any).default }, { button: this.btnEdit }];

    //TODO DISAB SHARED DATA
    //this.sharedParameterGestionePresenzeService.Par_Giustificativi$.subscribe(res => {
    //  this.par_GiustificativiList = this.sharedParameterGestionePresenzeService.Par_Giustificativi;
    //});

  }



  ionViewWillEnter() {



    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/justificationedit', {
          state: { id: 0 }
        });
      }),

    ];

  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }



  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/justificationedit', {
      state: { id: item.id }
    });
  }

  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.justificationList.confirmDelete'));
    if (result) {

      const request: GenericRequest<Par_Giustificativi_DeleteInModel> = new GenericRequest<Par_Giustificativi_DeleteInModel>(Par_Giustificativi_DeleteInModel);
      request.data.id = item.id;
      this.parGiustificativiService.Par_GiustificativiDelete(request).pipe(
        map(() => {
          this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Par_Giustificativi);
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
