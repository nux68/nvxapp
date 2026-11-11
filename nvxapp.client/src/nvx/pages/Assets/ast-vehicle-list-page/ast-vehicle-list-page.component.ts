import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../ClientServer-Service/Assets/ast.service';
import * as F from '../../../ClientServer-Service/Assets/Models/ast-flotta-model';
import { AstFormatService } from '../../../Utility/Assets/ast-format.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

@Component({
  selector: 'app-ast-vehicle-list-page',
  templateUrl: './ast-vehicle-list-page.component.html',
  styleUrls: ['./ast-vehicle-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Flotta aziendale (amministratori, AST-FLO): veicoli con assegnatario e prossima scadenza; nuovo veicolo con il + del fab
export class AstVehicleListPageComponent {

  public title: string;
  public searchText!: string;
  public showDisabled = false;
  public items: F.AstVeicoloListModel[] | null = null;
  public readonly columns: NvxListColumn<F.AstVeicoloListModel>[];
  public readonly actions: NvxListAction<F.AstVeicoloListModel>[];
  public readonly isDisabled = (x: F.AstVeicoloListModel) => x.stato === F.AstStatoVeicolo.Dismesso;
  public readonly sort = (a: F.AstVeicoloListModel, b: F.AstVeicoloListModel) => a.targa.localeCompare(b.targa);

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              userInterfaceService: UserInterfaceService,
              private service: AstService,
              private fmt: AstFormatService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('ast.pages.vehicleList.title');
    this.columns = [
      { key: 'targa', title: t('ast.fields.targa'), mobile: 'title' },
      { key: 'descrizione', title: t('ast.fields.veicolo'), mobile: 'subtitle' },
      { key: 'uso', title: t('ast.fields.uso'), value: x => t('ast.enums.uso.' + x.uso), mobile: 'hidden' },
      { key: 'assegnatario', title: t('ast.fields.assegnatario'), mobile: 'subtitle' },
      { key: 'sede', title: t('ast.fields.sede'), mobile: 'hidden' },
      { key: 'prossimaScadenza', title: t('ast.fields.prossimaScadenza'), mobile: 'note',
        value: x => x.prossimaScadenza ? this.fmt.data(x.prossimaScadenza) + ' ' + (x.prossimaScadenzaDescrizione ?? '') : '',
        sortValue: x => x.prossimaScadenza ?? '9999' },
    ];
    const bEdit = userInterfaceService.Btn_Modifica;
    bEdit.event = (x: F.AstVeicoloListModel) => this.navCtrl.navigateForward('/astvehicleedit', { state: { id: x.id } });
    const bDel = userInterfaceService.Btn_Cancella;
    bDel.event = (x: F.AstVeicoloListModel) => this.elimina(x);
    this.actions = [{ button: bDel }, { button: bEdit }];
  }

  ionViewWillEnter() {
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('ast.pages.vehicleList.new'), 'add-circle-outline', () =>
        this.navCtrl.navigateForward('/astvehicleedit', { state: { id: 0 } })),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    const request = new GenericRequest<F.AstVeicoloListInModel>(F.AstVeicoloListInModel);
    request.data.dismessi = true;
    this.service.VeicoloList(request).subscribe(res => this.items = res.success && res.data ? res.data.veicolo : []);
  }

  private async elimina(x: F.AstVeicoloListModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('ast.pages.vehicleList.confirmDelete', { targa: x.targa })))
      return;
    const request = new GenericRequest<F.AstIdInModel>(F.AstIdInModel);
    request.data.id = x.id;
    this.service.VeicoloDelete(request).subscribe(res => { if (res.success) this.load(); });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
