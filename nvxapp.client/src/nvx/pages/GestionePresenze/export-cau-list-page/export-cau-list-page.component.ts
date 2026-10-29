import { TranslocoService } from '@jsverse/transloco';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ActionSheetController, ModalController, NavController } from '@ionic/angular';
import { ParOrarioService } from '../../../ClientServer-Service/GestionePresenze/Par_Orario/par-orario.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { map, catchError } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParExportCauService } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau/par-export-cau.service';
import { Par_ExportCau_Delete_InModel, Par_ExportCau_GetAll_InModel, Par_ExportCauModel } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau/Models/par-export-cau-model';
import { ParTable, SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { SeletionParExportCauDialogComponent, SeletionParExportCauDialogResult } from '../../../shared/components/GestionePresenze/seletion-par-export-cau-dialog/seletion-par-export-cau-dialog.component';

@Component({
  selector: 'app-export-cau-list-page',
  templateUrl: './export-cau-list-page.component.html',
  styleUrls: ['./export-cau-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ExportCauListPageComponent {


  public title: string;
  public searchText: string = '';
  /** dalla cache caricata al login: si aggiorna da sola dopo salvataggi e cancellazioni (refresh mirato) */
  public get par_ExportCauList(): Par_ExportCauModel[] | null {
    return this.sharedParameterGestionePresenzeService.Par_ExportCau;
  }
  public readonly columns: NvxListColumn<Par_ExportCauModel>[] = [
    { key: 'codice', title: this.transloco.translate('attendance.pages.exportCauList.columnCode'), width: '140px', mobile: 'subtitle' },
    { key: 'descrizione', title: this.transloco.translate('attendance.pages.exportCauList.columnDescription'), mobile: 'title' }
  ];
  public actions: NvxListAction<Par_ExportCauModel>[] = [];
  public readonly sortList = (a: Par_ExportCauModel, b: Par_ExportCauModel) => (a.descrizione ?? '').localeCompare(b.descrizione ?? '');
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private navCtrl: NavController,
    public fabMenuService: FabMenuService,
    private parExportCauService: ParExportCauService,
    private collectionDialogService: CollectionDialogService,
    private refresherService: RefresherService,
    private userInterfaceService: UserInterfaceService,
    private transloco: TranslocoService,
    private actionSheetCtrl: ActionSheetController,
    private modalCtrl: ModalController
  ) {
    this.title = this.transloco.translate('attendance.pages.exportCauList.title');
    this.btnEdit = this.userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;

    this.actions = [{ button: this.btnDelete }, { button: this.btnEdit }];
  }




  ionViewWillEnter() {


    this.fabMenuService.fabMenuItem = [

      new FabMenuItem(this.transloco.translate('attendance.pages.exportCauList.fabAdd'), 'add-circle-outline', () => {
        this.scegliInserimento();
      }),

    ];


  }

  /** Pulsante "+": action sheet con nuovo modello o duplicazione di un modello esistente (come i profili orari). */
  async scegliInserimento() {
    const sheet = await this.actionSheetCtrl.create({
      header: this.transloco.translate('attendance.pages.exportCauList.fabAdd'),
      buttons: [
        { text: this.transloco.translate('attendance.pages.exportCauList.fabNuovo'), icon: 'add-circle-outline', handler: () => { this.nuovoExport(); } },
        { text: this.transloco.translate('attendance.pages.exportCauList.fabDuplica'), icon: 'copy-outline', handler: () => { this.duplicaExport(); } },
        { text: this.transloco.translate('common.buttons.cancel'), role: 'cancel' }
      ]
    });
    await sheet.present();
  }

  private nuovoExport() {
    this.navCtrl.navigateForward('/exportcauedit', {
      state: { id: 0 }
    });
  }

  /** Si sceglie il modello da duplicare; la modifica si apre con una copia da salvare come modello nuovo. */
  private async duplicaExport() {
    const modal = await this.modalCtrl.create({
      component: SeletionParExportCauDialogComponent,
      componentProps: { title: this.transloco.translate('attendance.pages.exportCauList.duplicaTitolo') }
    });
    await modal.present();

    const { data, role } = await modal.onDidDismiss<SeletionParExportCauDialogResult>();
    if (role === 'confirm' && data?.id)
      this.navCtrl.navigateForward('/exportcauedit', {
        state: { id: 0, duplicaDa: data.id }
      });
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  handleButtonEditClick = (item: Par_ExportCauModel) => {
    this.navCtrl.navigateForward('/exportcauedit', {
      state: { id: item.id }
    });
  }

  handleButtonDeleteClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.exportCauList.confirmDelete'));
    if (result) {

      const request: GenericRequest<Par_ExportCau_Delete_InModel> = new GenericRequest<Par_ExportCau_Delete_InModel>(Par_ExportCau_Delete_InModel);
      request.data.id = item.id;
      this.parExportCauService.Par_ExportCauDelete(request).pipe(
        map(() => {
          this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Par_ExportCau);
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




}
