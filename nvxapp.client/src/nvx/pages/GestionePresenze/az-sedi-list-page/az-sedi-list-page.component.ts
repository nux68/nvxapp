import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Az_Sedi_GetAll_InModel, Az_SediModel, Az_SediDeleteInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Sedi/Models/az-sedi-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AzSediService } from '../../../ClientServer-Service/GestionePresenze/Az_Sedi/az-sedi.service';
import { map, catchError } from 'rxjs';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

@Component({
  selector: 'app-az-sedi-list-page',
  templateUrl: './az-sedi-list-page.component.html',
  styleUrls: ['./az-sedi-list-page.component.scss'],
  standalone: false
})
export class AzSediListPageComponent implements OnInit {

  public title = 'Sedi';
  public searchText!: string;
  public azSediList: Az_SediModel[] | null = null;
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private azSediService: AzSediService,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService,
              private collectionDialogService: CollectionDialogService,
              private refresherService: RefresherService) {

    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
  }

  private loadData() {
    let request: GenericRequest<Az_Sedi_GetAll_InModel> = new GenericRequest<Az_Sedi_GetAll_InModel>(Az_Sedi_GetAll_InModel);
    this.azSediService.GetAll(request).subscribe(res => {
      this.azSediList = res.data.az_Sedi;
    });
  }

  ionViewWillEnter() {
    this.loadData();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem('Nuova Sede', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/azsediedit', {
          state: { id: 0 }
        });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  ngOnInit() { }

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/azsediedit', {
      state: { id: item.id }
    });
  }

  
  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione della sede?');
    if (result) {

        let request: GenericRequest<Az_SediDeleteInModel> = new GenericRequest<Az_SediDeleteInModel>(Az_SediDeleteInModel);
        request.data.id = item.id;
        return this.azSediService.AzSediDelete(request).pipe(
          map(() => {
            this.refresherService.SharedParameterGestionePresenze_triggerRefresh();
            this.loadData();
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

  isAdmin(item: any) {
    return false;
  }

  getAll() {
    if (!this.azSediList) return [];
    return this.azSediList.sort((a, b) => {
      // Prima ordina per default (true prima di false)
      if (a.default === b.default) {
        // Poi ordina per descrizione
        return a.descrizione.localeCompare(b.descrizione);
      }
      return a.default ? -1 : 1;
    });
  }

}
