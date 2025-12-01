import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_Giustificativi_DeleteInModel, Par_GiustificativiInModel, Par_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParGiustificativiService } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/par-giustificativi.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { map, catchError } from 'rxjs';
import { Par_Orario_DeleteInModel } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/Models/par-orario-model';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';


@Component({
  selector: 'app-justification-list-page',
  templateUrl: './justification-list-page.component.html',
  styleUrls: ['./justification-list-page.component.scss'],
  standalone: false
}) 
export class JustificationListPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public par_GiustificativiList: Par_GiustificativiModel[] | null = null;
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              public fabMenuService: FabMenuService,
              private parGiustificativiService: ParGiustificativiService,
              private userInterfaceService: UserInterfaceService,
              private collectionDialogService: CollectionDialogService,
              private refresherService: RefresherService,
              private userNavigationService: UserNavigationService) {

    this.title = 'Giustificativi';
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    //TODO DISAB SHARED DATA
    //this.sharedParameterGestionePresenzeService.Par_Giustificativi$.subscribe(res => {
    //  this.par_GiustificativiList = this.sharedParameterGestionePresenzeService.Par_Giustificativi;
    //});

  }


  private loadData() {
    //TODO DISAB SHARED DATA
    let request: GenericRequest<Par_GiustificativiInModel> = new GenericRequest<Par_GiustificativiInModel>(Par_GiustificativiInModel);
    this.parGiustificativiService.GetAll(request).subscribe(res => {
      this.par_GiustificativiList = res.data.par_Giustificativi;
    });
  }

  ionViewWillEnter() {

    this.loadData();


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

  ngOnInit() { }


  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/justificationedit', {
      state: { id: item.id }
    });
  }

  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione del giustificativo');
    if (result) {

      const request: GenericRequest<Par_Giustificativi_DeleteInModel> = new GenericRequest<Par_Giustificativi_DeleteInModel>(Par_Giustificativi_DeleteInModel);
      request.data.id = item.id;
      this.parGiustificativiService.Par_GiustificativiDelete(request).pipe(
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
    const sorted_GiustificativiList = this.par_GiustificativiList.sort((a, b) =>
      a.descrizione.localeCompare(b.descrizione)
    );
    return sorted_GiustificativiList;
  }


}
