import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Az_Commessa_GetAll_InModel, Az_CommessaModel, Az_CommessaDeleteInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Commessa/Models/az-commessa-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AzCommessaService } from '../../../ClientServer-Service/GestionePresenze/Az_Commessa/az-commessa.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { map, catchError } from 'rxjs';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

@Component({
  selector: 'app-commessa-list-page',
  templateUrl: './commessa-list-page.component.html',
  styleUrls: ['./commessa-list-page.component.scss'],
  standalone: false
})
export class CommessaListPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public commessaList: Az_CommessaModel[] | null = null;
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private azCommessaService: AzCommessaService,
              private userInterfaceService: UserInterfaceService,
              private userNavigationService: UserNavigationService,
              private collectionDialogService: CollectionDialogService,
              private refresherService: RefresherService) {

    this.title = 'Commesse';
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
  }

  private loadData() {
    let request: GenericRequest<Az_Commessa_GetAll_InModel> = new GenericRequest<Az_Commessa_GetAll_InModel>(Az_Commessa_GetAll_InModel);
    this.azCommessaService.GetAll(request).subscribe(res => {
      this.commessaList = res.data.az_Commessa;
    });
  }

  ionViewWillEnter() {
    this.loadData();

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Nuova Commessa', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/commessaedit', {
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
    this.navCtrl.navigateForward('/commessaedit', {
      state: { id: item.id }
    });
  }

  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione della commessa?');
    if (result) {

      let request: GenericRequest<Az_CommessaDeleteInModel> = new GenericRequest<Az_CommessaDeleteInModel>(Az_CommessaDeleteInModel);
      request.data.id = item.id;
      this.azCommessaService.Az_CommessaDelete(request).pipe(
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

  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  isAdmin(item: any) {
    return false;
  }

  getAll() {
    if (!this.commessaList) return [];
    const sortedList = this.commessaList.sort((a, b) =>
      a.descrizione.localeCompare(b.descrizione)
    );
    return sortedList;
  }
}
