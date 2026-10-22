import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { catchError, map } from 'rxjs';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { ParFestivitaService } from '../../../ClientServer-Service/GestionePresenze/Par_Festivita/par-festivita.service';
import { TranslocoService } from '@jsverse/transloco';
import { Par_Festivita_DeleteInModel, Par_Festivita_GetAllInModel, Par_FestivitaModel, TipoFestivita } from '../../../ClientServer-Service/GestionePresenze/Par_Festivita/Models/par-festivita-model';

@Component({
  selector: 'app-festivita-list-page',
  templateUrl: './festivita-list-page.component.html',
  styleUrls: ['./festivita-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class FestivitaListPageComponent {

  public title: string;
  public searchText!: string;
  public par_FestivitaList: Par_FestivitaModel[] | null = null;
  public readonly columns: NvxListColumn<Par_FestivitaModel>[];
  public actions: NvxListAction<Par_FestivitaModel>[] = [];
  public readonly sortList = (a: Par_FestivitaModel, b: Par_FestivitaModel) => this.chiaveOrdine(a).localeCompare(this.chiaveOrdine(b));
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private parFestivitaService: ParFestivitaService,
              private userInterfaceService: UserInterfaceService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('attendance.pages.festivitaList.title');
    this.columns = [
      { key: 'descrizione', title: this.transloco.translate('attendance.pages.festivitaList.columnHoliday') },
      { key: 'quando', title: this.transloco.translate('attendance.pages.festivitaList.columnWhen'), value: x => this.quando(x), sortValue: x => this.chiaveOrdine(x) },
    ];

    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    this.actions = [{ button: this.btnDelete }, { button: this.btnEdit }];
  }

  private loadData() {
    const request = new GenericRequest<Par_Festivita_GetAllInModel>(Par_Festivita_GetAllInModel);
    this.parFestivitaService.GetAll(request).subscribe(res => {
      this.par_FestivitaList = res.data.par_Festivita;
    });
  }

  ionViewWillEnter() {
    this.loadData();

    this.fabMenuService.fabMenuItem = [
      new FabMenuItem('Nuova festività', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/festivitaedit', { state: { id: 0 } });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  handleButtonDeleteClick = async (item: Par_FestivitaModel) => {
    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.festivitaList.deleteConfirm'));
    if (result) {
      const request = new GenericRequest<Par_Festivita_DeleteInModel>(Par_Festivita_DeleteInModel);
      request.data.id = item.id;
      this.parFestivitaService.Par_FestivitaDelete(request).pipe(
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

  handleButtonEditClick = (item: Par_FestivitaModel) => {
    this.navCtrl.navigateForward('/festivitaedit', { state: { id: item.id } });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  /** Testo della data: giorno e mese per le ricorrenti, data intera per le singole, "Pasqua + 1" per Pasquetta. */
  quando(item: Par_FestivitaModel): string {
    if (item.tipoFestivita === TipoFestivita.Pasquetta)
      return this.transloco.translate('attendance.pages.festivitaList.easterMonday');
    if (!item.data)
      return '';
    const [anno, mese, giorno] = item.data.substring(0, 10).split('-');
    return item.tipoFestivita === TipoFestivita.DataRicorrente
      ? this.transloco.translate('attendance.pages.festivitaList.everyYear', { date: `${giorno}/${mese}` })
      : `${giorno}/${mese}/${anno}`;
  }

  /** Ordine: ricorrenti per mese/giorno, poi Pasquetta, poi le date singole in ordine cronologico. */
  chiaveOrdine(f: Par_FestivitaModel): string {
    return f.tipoFestivita === TipoFestivita.DataRicorrente ? '0' + (f.data ?? '').substring(5, 10)
      : f.tipoFestivita === TipoFestivita.Pasquetta ? '1'
        : '2' + (f.data ?? '');
  }
}
