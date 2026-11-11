import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { catchError, map } from 'rxjs';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParProfiloFormuleService } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloFormule/par-profilo-formule.service';
import { Par_ProfiloFormule_DeleteInModel, Par_ProfiloFormuleInModel, Par_ProfiloFormuleModel } from '../../../ClientServer-Service/GestionePresenze/Par_ProfiloFormule/Models/par-profilo-formule-model';

// elenco dei profili formule (Parametri, CompanyPowerAdmin, piano Pro)
@Component({
  selector: 'app-profilo-formule-list-page',
  templateUrl: './profilo-formule-list-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ProfiloFormuleListPageComponent {

  public title: string;
  public searchText!: string;
  public lista: Par_ProfiloFormuleModel[] | null = null;
  public readonly columns: NvxListColumn<Par_ProfiloFormuleModel>[];
  public actions: NvxListAction<Par_ProfiloFormuleModel>[] = [];
  public readonly sortList = (a: Par_ProfiloFormuleModel, b: Par_ProfiloFormuleModel) => (a.codice ?? '').localeCompare(b.codice ?? '');
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private parProfiloFormuleService: ParProfiloFormuleService,
              private userInterfaceService: UserInterfaceService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('attendance.pages.profiloFormuleList.title');
    this.columns = [
      { key: 'codice', title: this.transloco.translate('attendance.pages.profiloFormuleList.colCodice'), width: '160px', mobile: 'subtitle' },
      { key: 'descrizione', title: this.transloco.translate('attendance.pages.profiloFormuleList.colDescrizione'), mobile: 'title' },
    ];
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
    this.actions = [{ button: this.btnDelete }, { button: this.btnEdit }];
  }

  private loadData() {
    const request = new GenericRequest<Par_ProfiloFormuleInModel>(Par_ProfiloFormuleInModel);
    this.parProfiloFormuleService.GetAll(request).subscribe(res => {
      this.lista = res.data?.par_ProfiloFormule ?? [];
    });
  }

  ionViewWillEnter() {
    this.loadData();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('attendance.pages.profiloFormuleList.nuovo'), 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/profiloformuleedit', { state: { id: 0 } });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  handleButtonDeleteClick = async (item: Par_ProfiloFormuleModel) => {
    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.profiloFormuleList.confermaCancellazione'));
    if (result) {
      const request = new GenericRequest<Par_ProfiloFormule_DeleteInModel>(Par_ProfiloFormule_DeleteInModel);
      request.data.id = item.id;
      this.parProfiloFormuleService.Par_ProfiloFormuleDelete(request).pipe(
        map(() => { this.loadData(); return true; }),
        catchError((error: any) => { console.error('Errore durante la chiamata API:', error); return [false]; })
      ).subscribe();
    }
  }

  handleButtonEditClick = (item: Par_ProfiloFormuleModel) => {
    this.navCtrl.navigateForward('/profiloformuleedit', { state: { id: item.id } });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
