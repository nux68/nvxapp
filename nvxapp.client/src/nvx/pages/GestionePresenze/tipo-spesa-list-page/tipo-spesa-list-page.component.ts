import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_TipoSpesa_DeleteInModel, Par_TipoSpesaInModel, Par_TipoSpesaModel } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/Models/par-tipo-spesa-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParTipoSpesaService } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/par-tipo-spesa.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { map, catchError } from 'rxjs';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { TranslocoService } from '@jsverse/transloco';

@Component({
  selector: 'app-tipo-spesa-list-page',
  templateUrl: './tipo-spesa-list-page.component.html',
  styleUrls: ['./tipo-spesa-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TipoSpesaListPageComponent {

  public title!: string;
  public searchText!: string;
  public par_TipoSpesaList: Par_TipoSpesaModel[] | null = null;
  public readonly columns: NvxListColumn<Par_TipoSpesaModel>[];
  public actions: NvxListAction<Par_TipoSpesaModel>[] = [];
  public readonly sortList = (a: Par_TipoSpesaModel, b: Par_TipoSpesaModel) => (a.descrizione ?? '').localeCompare(b.descrizione ?? '');
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              public fabMenuService: FabMenuService,
              private parTipoSpesaService: ParTipoSpesaService,
              private userInterfaceService: UserInterfaceService,
              private refresherService: RefresherService,
              private collectionDialogService: CollectionDialogService,
              private userNavigationService: UserNavigationService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('attendance.pages.tipoSpesaList.title');
    this.columns = [
      { key: 'codice', title: this.transloco.translate('attendance.pages.tipoSpesaList.colCodice'), width: '140px', mobile: 'subtitle' },
      { key: 'descrizione', title: this.transloco.translate('attendance.pages.tipoSpesaList.colDescrizione'), mobile: 'title' },
    ];
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    this.actions = [{ button: this.btnDelete, visible: item => !(item as any).default }, { button: this.btnEdit }];
  }

  private loadData() {
    let request: GenericRequest<Par_TipoSpesaInModel> = new GenericRequest<Par_TipoSpesaInModel>(Par_TipoSpesaInModel);
    this.parTipoSpesaService.GetAll(request).subscribe(res => {
      this.par_TipoSpesaList = res.data.par_TipoSpesa;
    });

  }

  ionViewWillEnter() {

    this.loadData();

    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('attendance.pages.tipoSpesaList.nuovo'), 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/tipospesaedit', {
          state: { id: 0 }
        });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }



  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.tipoSpesaList.confermaCancellazione'));
    if (result) {

      const request: GenericRequest<Par_TipoSpesa_DeleteInModel> = new GenericRequest<Par_TipoSpesa_DeleteInModel>(Par_TipoSpesa_DeleteInModel);
      request.data.id = item.id;
      this.parTipoSpesaService.Par_TipoSpesaDelete(request).pipe(
        map(() => {
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

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/tipospesaedit', {
      state: { id: item.id }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }


}
