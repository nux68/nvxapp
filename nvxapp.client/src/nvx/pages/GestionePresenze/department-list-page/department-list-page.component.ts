import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_GiustificativiInModel, Par_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { AzSediRepartoService } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/az-sedi-reparto.service';
import { Az_SediReparto_GetAll_InModel, Az_SediRepartoModel, Az_SediRepartoDeleteInModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { map, catchError } from 'rxjs';

@Component({
  selector: 'app-department-list-page',
  templateUrl: './department-list-page.component.html',
  styleUrls: ['./department-list-page.component.scss'],
  standalone: false
})
export class DepartmentListPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public az_SediRepartoList: Az_SediRepartoModel[] | null = null;
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;
  public curr_idAz_Sedi: number;

  constructor(private navCtrl: NavController,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    public fabMenuService: FabMenuService,
    private azSediRepartoService: AzSediRepartoService,
    private userInterfaceService: UserInterfaceService,
    private userNavigationService: UserNavigationService,
    private refresherService: RefresherService) {

    this.title = 'Departments';
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
    let request: GenericRequest<Az_SediReparto_GetAll_InModel> = new GenericRequest<Az_SediReparto_GetAll_InModel>(Az_SediReparto_GetAll_InModel);
    this.azSediRepartoService.GetAll(request).subscribe(res => {
      this.az_SediRepartoList = res.data.az_SediReparto;
    });
  }

  ionViewWillEnter() {
    this.loadData();

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

  ngOnInit() { }


  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/departmentedit', {
      state: { id: item.id, idAz_Sedi: this.curr_idAz_Sedi }
    });
  }

  handleButtonDeleteClick = (item: any) => {
    // Model per la cancellazione
    const request: GenericRequest<Az_SediRepartoDeleteInModel> = new GenericRequest<Az_SediRepartoDeleteInModel>(Az_SediRepartoDeleteInModel);
    request.data.id = item.id;
    this.azSediRepartoService.Az_SediRepartoDelete(request).pipe(
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

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  isAdmin(item: any) {

    return false;
  }

  getAll() {
    if (!this.az_SediRepartoList) return [];
    const filteredList = this.az_SediRepartoList.filter(x => x.idAz_Sedi === this.curr_idAz_Sedi);
    const sorted_SediList = filteredList.sort((a, b) => {
      // Prima ordina per default (true prima di false)
      if (a.default === b.default) {
        // Poi ordina per descrizione
        return a.descrizione.localeCompare(b.descrizione);
      }
      return a.default ? -1 : 1;
    });
    return sorted_SediList;
  }


  onPeriodChange(period: { year: number, month: number } | undefined): void { }
  onCurrentUserChanged(userId: string[] | undefined): void { }
  onSedeChanged(sediId: number | undefined): void {
    this.curr_idAz_Sedi = sediId;
  }
  onRepartiChanged(repartoIds: number[] | undefined): void { }
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void { }



}
