import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ActionSheetController, NavController, ToastController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../ClientServer-Service/Assets/ast.service';
import { AstEmptyInModel } from '../../../ClientServer-Service/Assets/Models/ast-model';
import * as F from '../../../ClientServer-Service/Assets/Models/ast-flotta-model';
import * as B from '../../../ClientServer-Service/Assets/Models/ast-beni-model';
import { AstFormatService } from '../../../Utility/Assets/ast-format.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

export type AstGoodsSection = 'beni' | 'restituire' | 'categorie';

@Component({
  selector: 'app-ast-goods-list-page',
  templateUrl: './ast-goods-list-page.component.html',
  styleUrls: ['./ast-goods-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Beni assegnati (amministratori, AST-BENI): elenco dei beni, beni e veicoli da restituire alla cessazione dei dipendenti
 (AST-BENI.3, con la verifica immediata) e categorie. Il + del fab apre il menu: nuovo bene o nuova categoria.
*/
export class AstGoodsListPageComponent {

  public title: string;
  public searchText!: string;
  public sezione: AstGoodsSection = 'beni';
  public readonly sezioni: AstGoodsSection[] = ['beni', 'restituire', 'categorie'];
  public showDisabled = false;
  public beni: B.AstBeneListModel[] | null = null;
  public daRestituire: B.AstDaRestituireModel[] | null = null;
  public categorie: B.AstCategoriaModel[] | null = null;
  public buttonbar: ButtonItem[] = [];
  public readonly colBene: NvxListColumn<B.AstBeneListModel>[];
  public readonly colRestituire: NvxListColumn<B.AstDaRestituireModel>[];
  public readonly colCategoria: NvxListColumn<B.AstCategoriaModel>[];
  public readonly actBene: NvxListAction<B.AstBeneListModel>[];
  public readonly actRestituire: NvxListAction<B.AstDaRestituireModel>[];
  public readonly actCategoria: NvxListAction<B.AstCategoriaModel>[];
  public readonly beneDismesso = (x: B.AstBeneListModel) => x.stato === B.AstStatoBene.Dismesso;
  public readonly categoriaDisattiva = (x: B.AstCategoriaModel) => !x.attivo;
  public readonly restituzioneScaduta = (x: B.AstDaRestituireModel) => (x.dataCessazione ?? '') <= new Date().toISOString();
  private readonly btnVerifica: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              userInterfaceService: UserInterfaceService,
              private actionSheetCtrl: ActionSheetController,
              private toastCtrl: ToastController,
              private service: AstService,
              private fmt: AstFormatService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('ast.pages.goodsList.title');
    this.colBene = [
      { key: 'codiceInventario', title: t('ast.fields.codiceInventario'), mobile: 'note' },
      { key: 'descrizione', title: t('ast.fields.descrizione'), mobile: 'title' },
      { key: 'categoria', title: t('ast.fields.categoria'), mobile: 'hidden' },
      { key: 'stato', title: t('ast.fields.stato'), value: x => t('ast.enums.statoBene.' + x.stato), mobile: 'hidden' },
      { key: 'assegnatario', title: t('ast.fields.assegnatario'), mobile: 'subtitle',
        value: x => x.assegnatario ? x.assegnatario + ' (' + this.fmt.data(x.dataConsegna) + ')' : '' },
      { key: 'sede', title: t('ast.fields.sede'), mobile: 'hidden' },
    ];
    this.colRestituire = [
      { key: 'dipendente', title: t('ast.fields.dipendente'), mobile: 'title' },
      { key: 'dataCessazione', title: t('ast.fields.dataCessazione'), mobile: 'note', value: x => this.fmt.data(x.dataCessazione), sortValue: x => x.dataCessazione },
      { key: 'tipo', title: t('ast.fields.tipo'), value: x => t('ast.enums.tipoRestituzione.' + x.tipo), mobile: 'hidden' },
      { key: 'oggetto', title: t('ast.fields.oggetto'), mobile: 'subtitle' },
      { key: 'dataAvviso', title: t('ast.fields.dataAvviso'), value: x => this.fmt.data(x.dataAvviso), sortValue: x => x.dataAvviso ?? '', mobile: 'hidden' },
    ];
    this.colCategoria = [
      { key: 'descrizione', title: t('ast.fields.descrizione'), mobile: 'title' },
      { key: 'numeroBeni', title: t('ast.fields.numeroBeni'), align: 'end', mobile: 'note' },
      { key: 'ordine', title: t('ast.fields.ordine'), align: 'end', mobile: 'hidden' },
    ];

    const bEditBene = userInterfaceService.Btn_Modifica;
    bEditBene.event = (x: B.AstBeneListModel) => this.navCtrl.navigateForward('/astgoodedit', { state: { id: x.id } });
    const bDelBene = userInterfaceService.Btn_Cancella;
    bDelBene.event = (x: B.AstBeneListModel) => this.eliminaBene(x);
    this.actBene = [{ button: bDelBene, visible: x => x.stato !== B.AstStatoBene.Assegnato }, { button: bEditBene }];

    const bApri = userInterfaceService.Btn_Apri;
    bApri.event = (x: B.AstDaRestituireModel) => this.navCtrl.navigateForward(x.tipo === 'veicolo' ? '/astvehicleedit' : '/astgoodedit', { state: { id: x.idOggetto } });
    this.actRestituire = [{ button: bApri }];

    const bEditCat = userInterfaceService.Btn_Modifica;
    bEditCat.event = (x: B.AstCategoriaModel) => this.navCtrl.navigateForward('/astgooditemedit', { state: { kind: 'categoria', item: { ...x } } });
    const bDelCat = userInterfaceService.Btn_Cancella;
    bDelCat.event = (x: B.AstCategoriaModel) => this.eliminaCategoria(x);
    this.actCategoria = [{ button: bDelCat, visible: x => x.numeroBeni === 0 }, { button: bEditCat }];

    this.btnVerifica = userInterfaceService.Btn_Esegui;
    this.btnVerifica.text = t('ast.pages.goodsList.checkNow');
    this.btnVerifica.event = () => this.verifica();
  }

  ionViewWillEnter() {
    const s = history.state?.sezione as AstGoodsSection | undefined;
    if (s && this.sezioni.includes(s)) this.sezione = s;
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('ast.pages.goodsList.add'), 'add-circle-outline', () => this.menuAggiungi()),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  sezioneChanged() {
    this.buttonbar = this.sezione === 'restituire' ? [this.btnVerifica] : [];
  }

  load() {
    this.sezioneChanged();
    const rb = new GenericRequest<B.AstBeneListInModel>(B.AstBeneListInModel);
    rb.data.dismessi = true;
    this.service.BeneList(rb).subscribe(res => this.beni = res.success && res.data ? res.data.bene : []);
    const re = new GenericRequest<AstEmptyInModel>(AstEmptyInModel);
    this.service.DaRestituireList(re).subscribe(res => this.daRestituire = res.success && res.data ? res.data.daRestituire : []);
    this.service.CategoriaList(re).subscribe(res => this.categorie = res.success && res.data ? res.data.categoria : []);
  }

  private async menuAggiungi() {
    const t = (k: string) => this.transloco.translate(k);
    const sheet = await this.actionSheetCtrl.create({
      header: t('ast.pages.goodsList.add'),
      buttons: [
        { text: t('ast.pages.goodsList.newGood'), icon: 'cube-outline', handler: () => { this.navCtrl.navigateForward('/astgoodedit', { state: { id: 0 } }); } },
        { text: t('ast.pages.goodsList.newCategory'), icon: 'pricetag-outline', handler: () => {
          this.navCtrl.navigateForward('/astgooditemedit', { state: { kind: 'categoria', item: null } }); } },
        { text: t('common.buttons.cancel'), role: 'cancel' },
      ]
    });
    await sheet.present();
  }

  private verifica() {
    const request = new GenericRequest<AstEmptyInModel>(AstEmptyInModel);
    this.service.DaRestituireVerifica(request).subscribe(async res => {
      if (!res.success || !res.data) return;
      this.daRestituire = res.data.daRestituire;
      const toast = await this.toastCtrl.create({ message: this.transloco.translate('ast.pages.goodsList.checkDone', { n: res.data.avvisate }), duration: 3000, position: 'bottom' });
      await toast.present();
    });
  }

  private async eliminaBene(x: B.AstBeneListModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('ast.pages.goodsList.confirmDelete', { nome: x.codiceInventario + ' ' + x.descrizione })))
      return;
    const request = new GenericRequest<F.AstIdInModel>(F.AstIdInModel);
    request.data.id = x.id;
    this.service.BeneDelete(request).subscribe(res => { if (res.success) this.load(); });
  }

  private async eliminaCategoria(x: B.AstCategoriaModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('ast.pages.goodsList.confirmDeleteCategory', { nome: x.descrizione })))
      return;
    const request = new GenericRequest<F.AstIdInModel>(F.AstIdInModel);
    request.data.id = x.id;
    this.service.CategoriaDelete(request).subscribe(res => { if (res.success) this.load(); });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
