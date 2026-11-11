import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ActionSheetController, NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { TurniService } from '../../../ClientServer-Service/GestionePresenze/Turni/turni.service';
import { Par_TurnoModel, Par_Turno_DeleteInModel, Par_Turno_GetAllInModel } from '../../../ClientServer-Service/GestionePresenze/Turni/Models/turni-model';

// Modelli di turno (AT-TURNI.1): elenco con modifica, cancellazione e "+" (nuovo turno o coperture dei reparti)
@Component({
  selector: 'app-turno-list-page',
  templateUrl: './turno-list-page.component.html',
  styleUrls: ['./turno-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TurnoListPageComponent {

  public title: string;
  public searchText!: string;
  public lista: Par_TurnoModel[] | null = null;
  public readonly columns: NvxListColumn<Par_TurnoModel>[];
  public actions: NvxListAction<Par_TurnoModel>[] = [];
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private turniService: TurniService,
              private userInterfaceService: UserInterfaceService,
              private collectionDialogService: CollectionDialogService,
              private actionSheetCtrl: ActionSheetController,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('attendance.pages.turnoList.title');
    this.columns = [
      { key: 'codice', title: this.transloco.translate('attendance.pages.turnoList.columnCode') },
      { key: 'descrizione', title: this.transloco.translate('attendance.pages.turnoList.columnDescription') },
      {
        key: 'orarioTesto', title: this.transloco.translate('attendance.pages.turnoList.columnHours'),
        value: x => x.idPar_Orario ? (x.orarioTesto ?? '') : this.transloco.translate('attendance.pages.turnoList.rest')
      },
    ];

    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = (item: Par_TurnoModel) => this.navCtrl.navigateForward('/turnoedit', { state: { id: item.id } });
    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
    this.actions = [{ button: this.btnDelete }, { button: this.btnEdit }];
  }

  private loadData() {
    this.turniService.GetAll(new GenericRequest<Par_Turno_GetAllInModel>(Par_Turno_GetAllInModel)).subscribe(res => {
      this.lista = res.data?.par_Turno ?? [];
    });
  }

  ionViewWillEnter() {
    this.loadData();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('attendance.pages.turnoList.fabAdd'), 'add-circle-outline', () => this.scegliInserimento()),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  /** "+": nuovo modello di turno o turni e coperture di un reparto */
  async scegliInserimento() {
    const sheet = await this.actionSheetCtrl.create({
      header: this.transloco.translate('attendance.pages.turnoList.fabAdd'),
      buttons: [
        { text: this.transloco.translate('attendance.pages.turnoList.fabNew'), icon: 'add-circle-outline', handler: () => { this.navCtrl.navigateForward('/turnoedit', { state: { id: 0 } }); } },
        { text: this.transloco.translate('attendance.pages.turnoList.fabCoverage'), icon: 'people-outline', handler: () => { this.navCtrl.navigateForward('/turnocoperture'); } },
        { text: this.transloco.translate('common.buttons.cancel'), role: 'cancel' }
      ]
    });
    await sheet.present();
  }

  handleButtonDeleteClick = async (item: Par_TurnoModel) => {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.turnoList.deleteConfirm')))
      return;
    const request = new GenericRequest<Par_Turno_DeleteInModel>(Par_Turno_DeleteInModel);
    request.data.id = item.id;
    this.turniService.Par_TurnoDelete(request).subscribe(() => this.loadData());
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
