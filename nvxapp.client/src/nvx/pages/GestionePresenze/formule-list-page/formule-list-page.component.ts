import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { catchError, map } from 'rxjs';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParFormuleService } from '../../../ClientServer-Service/GestionePresenze/Par_Formule/par-formule.service';
import { Par_Formule_DeleteInModel, Par_FormuleInModel, Par_FormuleModel } from '../../../ClientServer-Service/GestionePresenze/Par_Formule/Models/par-formule-model';

// elenco delle formule personalizzate del calcolo giornata (Parametri, CompanyPowerAdmin, piano Pro)
@Component({
  selector: 'app-formule-list-page',
  templateUrl: './formule-list-page.component.html',
  styleUrls: ['./formule-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class FormuleListPageComponent {

  public title: string;
  public searchText!: string;
  public par_FormuleList: Par_FormuleModel[] | null = null;
  public readonly columns: NvxListColumn<Par_FormuleModel>[];
  public actions: NvxListAction<Par_FormuleModel>[] = [];
  public readonly sortList = (a: Par_FormuleModel, b: Par_FormuleModel) => (a.codice ?? '').localeCompare(b.codice ?? '');
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private parFormuleService: ParFormuleService,
              private userInterfaceService: UserInterfaceService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('attendance.pages.formuleList.title');
    this.columns = [
      { key: 'codice', title: this.transloco.translate('attendance.pages.formuleList.colCodice'), width: '160px', mobile: 'subtitle' },
      { key: 'titolo', title: this.transloco.translate('attendance.pages.formuleList.colTitolo'), mobile: 'title' },
      { key: 'descrizione', title: this.transloco.translate('attendance.pages.formuleList.colDescrizione'), mobile: 'note',
        value: (x: Par_FormuleModel) => (x.descrizione ?? '').length > 80 ? x.descrizione.substring(0, 80) + '…' : (x.descrizione ?? '') },
    ];
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
    this.actions = [{ button: this.btnDelete }, { button: this.btnEdit }];
  }

  private loadData() {
    const request = new GenericRequest<Par_FormuleInModel>(Par_FormuleInModel);
    this.parFormuleService.GetAll(request).subscribe(res => {
      this.par_FormuleList = res.data?.par_Formule ?? [];
    });
  }

  ionViewWillEnter() {
    this.loadData();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('attendance.pages.formuleList.nuovo'), 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/formuleedit', { state: { id: 0 } });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  handleButtonDeleteClick = async (item: Par_FormuleModel) => {
    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.formuleList.confermaCancellazione'));
    if (result) {
      const request = new GenericRequest<Par_Formule_DeleteInModel>(Par_Formule_DeleteInModel);
      request.data.id = item.id;
      this.parFormuleService.Par_FormuleDelete(request).pipe(
        map(() => { this.loadData(); return true; }),
        catchError((error: any) => { console.error('Errore durante la chiamata API:', error); return [false]; })
      ).subscribe();
    }
  }

  handleButtonEditClick = (item: Par_FormuleModel) => {
    this.navCtrl.navigateForward('/formuleedit', { state: { id: item.id } });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
