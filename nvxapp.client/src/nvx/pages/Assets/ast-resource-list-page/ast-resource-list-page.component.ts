import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../ClientServer-Service/Assets/ast.service';
import * as F from '../../../ClientServer-Service/Assets/Models/ast-flotta-model';
import * as P from '../../../ClientServer-Service/Assets/Models/ast-prenotazioni-model';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

@Component({
  selector: 'app-ast-resource-list-page',
  templateUrl: './ast-resource-list-page.component.html',
  styleUrls: ['./ast-resource-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Risorse prenotabili (CompanyPowerAdmin, AST-PREN.1): postazioni, sale e auto in pool; nuova risorsa con il + del fab
export class AstResourceListPageComponent {

  public title: string;
  public searchText!: string;
  public showDisabled = false;
  public dati: P.AstRisorsaListOutModel | null = null;
  public readonly columns: NvxListColumn<P.AstRisorsaModel>[];
  public readonly actions: NvxListAction<P.AstRisorsaModel>[];
  public readonly disattiva = (x: P.AstRisorsaModel) => !x.attiva;
  public readonly iconOf = (x: P.AstRisorsaModel) => x.tipo === P.AstTipoRisorsa.Sala ? 'people-outline' : x.tipo === P.AstTipoRisorsa.AutoPool ? 'car-outline' : 'desktop-outline';

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              userInterfaceService: UserInterfaceService,
              private service: AstService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('ast.pages.resourceList.title');
    this.columns = [
      { key: 'descrizione', title: t('ast.fields.descrizione'), mobile: 'title' },
      { key: 'tipo', title: t('ast.fields.tipo'), value: x => t('ast.enums.tipoRisorsa.' + x.tipo), mobile: 'subtitle' },
      { key: 'sede', title: t('ast.fields.sede'), mobile: 'hidden' },
      { key: 'dettaglio', title: t('ast.fields.dettaglio'), mobile: 'note',
        value: x => x.tipo === P.AstTipoRisorsa.Sala && x.capienza ? this.transloco.translate('ast.pages.resourceList.seats', { n: x.capienza }) : (x.veicolo ?? '') },
    ];
    const bEdit = userInterfaceService.Btn_Modifica;
    bEdit.event = (x: P.AstRisorsaModel) => this.apri(x);
    const bDel = userInterfaceService.Btn_Cancella;
    bDel.event = (x: P.AstRisorsaModel) => this.elimina(x);
    this.actions = [{ button: bDel }, { button: bEdit }];
  }

  ionViewWillEnter() {
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('ast.pages.resourceList.new'), 'add-circle-outline', () => this.apri(null)),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    const request = new GenericRequest<P.AstRisorsaListInModel>(P.AstRisorsaListInModel);
    request.data.tutte = true;
    this.service.RisorsaList(request).subscribe(res => this.dati = res.success ? res.data : null);
  }

  private apri(x: P.AstRisorsaModel | null) {
    this.navCtrl.navigateForward('/astresourceedit', { state: { item: x ? { ...x } : null, sedi: this.dati?.sede ?? [], veicoli: this.dati?.veicoloPool ?? [] } });
  }

  private async elimina(x: P.AstRisorsaModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('ast.pages.resourceList.confirmDelete', { nome: x.descrizione })))
      return;
    const request = new GenericRequest<F.AstIdInModel>(F.AstIdInModel);
    request.data.id = x.id;
    this.service.RisorsaDelete(request).subscribe(res => { if (res.success) this.load(); });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
