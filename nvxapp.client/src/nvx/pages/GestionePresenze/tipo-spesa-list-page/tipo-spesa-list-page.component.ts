import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_TipoSpesa_DeleteInModel, Par_TipoSpesaInModel, Par_TipoSpesaModel } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/Models/par-tipo-spesa-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParTipoSpesaService } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/par-tipo-spesa.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { map, catchError } from 'rxjs';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';

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
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              public fabMenuService: FabMenuService,
              private parTipoSpesaService: ParTipoSpesaService,
              private userInterfaceService: UserInterfaceService,
              private refresherService: RefresherService,
              private collectionDialogService: CollectionDialogService,
              private userNavigationService: UserNavigationService) {

    this.title = 'Tipi spesa';
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
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
      new FabMenuItem('Nuovo tipo spesa', 'add-circle-outline', () => {
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

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione del tipo di spesa?');
    if (result) {

      const request: GenericRequest<Par_TipoSpesa_DeleteInModel> = new GenericRequest<Par_TipoSpesa_DeleteInModel>(Par_TipoSpesa_DeleteInModel);
      request.data.id = item.id;
      this.parTipoSpesaService.Par_TipoSpesaDelete(request).pipe(
        map(() => {
          this.refresherService.SharedParameterGestionePresenze_triggerRefresh();
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

  isAdmin(item: any) {
    return false;
  }

  getAll() {
    if (!this.par_TipoSpesaList) {
      return [];
    }
    const sorted_TipoSpesaList = this.par_TipoSpesaList.sort((a, b) =>
      a.descrizione.localeCompare(b.descrizione)
    );
    return sorted_TipoSpesaList;
  }
}
