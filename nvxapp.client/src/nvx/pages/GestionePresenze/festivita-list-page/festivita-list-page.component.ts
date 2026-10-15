import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular/lazy';
import { catchError, map } from 'rxjs';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { ParFestivitaService } from '../../../ClientServer-Service/GestionePresenze/Par_Festivita/par-festivita.service';
import { Par_Festivita_DeleteInModel, Par_Festivita_GetAllInModel, Par_FestivitaModel, TipoFestivita } from '../../../ClientServer-Service/GestionePresenze/Par_Festivita/Models/par-festivita-model';

@Component({
  selector: 'app-festivita-list-page',
  templateUrl: './festivita-list-page.component.html',
  styleUrls: ['./festivita-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class FestivitaListPageComponent {

  public title = 'Calendario festività';
  public searchText!: string;
  public par_FestivitaList: Par_FestivitaModel[] | null = null;
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private parFestivitaService: ParFestivitaService,
              private userInterfaceService: UserInterfaceService,
              private collectionDialogService: CollectionDialogService) {

    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
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
    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione della festività');
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
      return 'lunedì dopo Pasqua';
    if (!item.data)
      return '';
    const [anno, mese, giorno] = item.data.substring(0, 10).split('-');
    return item.tipoFestivita === TipoFestivita.DataRicorrente
      ? `${giorno}/${mese} ogni anno`
      : `${giorno}/${mese}/${anno}`;
  }

  /** Ordine: ricorrenti per mese/giorno, poi Pasquetta, poi le date singole in ordine cronologico. */
  getAll(): Par_FestivitaModel[] {
    if (!this.par_FestivitaList)
      return [];
    const chiave = (f: Par_FestivitaModel) =>
      f.tipoFestivita === TipoFestivita.DataRicorrente ? '0' + (f.data ?? '').substring(5, 10)
        : f.tipoFestivita === TipoFestivita.Pasquetta ? '1'
          : '2' + (f.data ?? '');
    return [...this.par_FestivitaList].sort((a, b) => chiave(a).localeCompare(chiave(b)));
  }
}
