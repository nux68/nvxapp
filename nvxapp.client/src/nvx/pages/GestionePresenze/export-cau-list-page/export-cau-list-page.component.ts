import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { ParOrarioService } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/par-orario.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { map, catchError } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParExportCauService } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau/par-export-cau.service';
import { Par_ExportCau_Delete_InModel, Par_ExportCau_GetAll_InModel, Par_ExportCauModel } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau/Models/par-export-cau-model';

@Component({
  selector: 'app-export-cau-list-page',
  templateUrl: './export-cau-list-page.component.html',
  styleUrls: ['./export-cau-list-page.component.scss'],
  standalone: false
})
export class ExportCauListPageComponent  implements OnInit {


  public title: string;
  public searchText: string = '';
  public par_ExportCauList: Par_ExportCauModel[] = [];
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(
    private navCtrl: NavController,
    public fabMenuService: FabMenuService,
    private parExportCauService: ParExportCauService,
    private collectionDialogService: CollectionDialogService,
    private refresherService: RefresherService,
    private userInterfaceService: UserInterfaceService
  ) {
    this.title = 'Modelli export';
    this.btnEdit = this.userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
  }

  ngOnInit() { }


  private loadData() {
    let request = new GenericRequest<Par_ExportCau_GetAll_InModel>(Par_ExportCau_GetAll_InModel);
    this.parExportCauService.GetAll(request).subscribe(res => {
      this.par_ExportCauList = res.data.par_ExportCau;
    });
  }

  ionViewWillEnter() {

    this.loadData();

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/exportcauedit', {
          state: { id: 0 }
        });
      }),

    ];


  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  handleButtonEditClick = (item: Par_ExportCauModel) => {
    this.navCtrl.navigateForward('/exportcauedit', {
      state: { id: item.id }
    });
  }

  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione della tabella');
    if (result) {

      const request: GenericRequest<Par_ExportCau_Delete_InModel> = new GenericRequest<Par_ExportCau_Delete_InModel>(Par_ExportCau_Delete_InModel);
      request.data.id = item.id;
      this.parExportCauService.Par_ExportCauDelete(request).pipe(
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


  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  isAdmin(item: any) {
    return false;
  }

  getAll() {
    if (!this.par_ExportCauList) return [];
    return this.par_ExportCauList.sort((a, b) =>
      a.descrizione.localeCompare(b.descrizione)
    );
  }


}
