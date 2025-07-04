import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_CompetenzaDeleteInModel, Par_CompetenzaModel, Par_Competenza_GetAll_InModel } from '../../../ClientServer-Service/GestionePresenze/Par_Competenza/Models/par-competenza-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParCompetenzaService } from '../../../ClientServer-Service/GestionePresenze/Par_Competenza/par-competenza.service';
import { map, catchError } from 'rxjs';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
//import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';

@Component({
  selector: 'app-competence-list-page',
  templateUrl: './competence-list-page.component.html',
  styleUrls: ['./competence-list-page.component.scss'],
  standalone: false
})
export class CompetenceListPageComponent implements OnInit {
  public title = 'Competenze';
  public searchText!: string;
  public par_CompetenzaList: Par_CompetenzaModel[] | null = null;
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(
    private navCtrl: NavController,
    public fabMenuService: FabMenuService,
    private refresherService: RefresherService,
    private parCompetenzaService: ParCompetenzaService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService,
    private collectionDialogService: CollectionDialogService
  ) {
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
  }

  private loadData() {
    let request: GenericRequest<Par_Competenza_GetAll_InModel> = new GenericRequest<Par_Competenza_GetAll_InModel>(Par_Competenza_GetAll_InModel);
    this.parCompetenzaService.GetAll(request).subscribe(res => {
      this.par_CompetenzaList = res.data.par_Competenza;
    });
  }

  ionViewWillEnter() {
    this.loadData();

    this.fabMenuService.fabMenuItem = [
      new FabMenuItem('Nuova Competenza', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/competenceedit', { state: { id: 0 } });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }
  
  ngOnInit() {}

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/competenceedit', { state: { id: item.id } });
  }

  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione della competenza?');
    if (result) {

        let request: GenericRequest<Par_CompetenzaDeleteInModel> = new GenericRequest<Par_CompetenzaDeleteInModel>(Par_CompetenzaDeleteInModel);
        request.data.id = item.id ;
        return this.parCompetenzaService.Par_CompetenzaDelete(request).pipe(
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
  getAll() {
    return this.par_CompetenzaList?.sort((a, b) => {
      // Prima ordina per default (true prima di false)
      if (a.default === b.default) {
        // Poi ordina per descrizione
        return a.descrizione.localeCompare(b.descrizione);
      }
      return a.default ? -1 : 1;
    });
  }
}
