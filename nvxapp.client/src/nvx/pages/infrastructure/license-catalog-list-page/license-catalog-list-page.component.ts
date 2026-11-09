import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { LicenseService } from '../../../ClientServer-Service/Infrastructure/License/license.service';
import {
  LicenseCatalogDeleteInModel, LicenseCatalogListInModel, LicenseCatalogModel
} from '../../../ClientServer-Service/Infrastructure/License/Models/license-model';

/*
 Listini delle licenze (ANALISI_LICENZE.md §4, §10.2): SuperUser (listini globali e dei centri) e utenti del centro
 (listini globali in sola lettura, propri modificabili dal DealerPowerAdmin se il centro e' abilitato).
 Un listino usato da licenze non si elimina: il server lo disattiva. Dal menu della pagina (SuperUser): impostazioni.
*/
@Component({
  selector: 'app-license-catalog-list-page',
  templateUrl: './license-catalog-list-page.component.html',
  styleUrls: ['./license-catalog-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class LicenseCatalogListPageComponent {

  public title: string;
  public searchText!: string;
  public listini: LicenseCatalogModel[] | null = null;
  public canCreate = false;
  public isSuperUser = false;
  public readonly columns: NvxListColumn<LicenseCatalogModel>[];
  public readonly actions: NvxListAction<LicenseCatalogModel>[];
  public readonly isDisabled = (item: LicenseCatalogModel) => !item.attivo;
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private licenseService: LicenseService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.licenseCatalogList.title');
    this.columns = [
      { key: 'descrizione', title: this.transloco.translate('pages.licenseCatalogList.columnDescription'), mobile: 'title' },
      { key: 'dealer', title: this.transloco.translate('pages.licenseCatalogList.columnOwner'), mobile: 'subtitle',
        value: l => l.dealer ?? this.transloco.translate('pages.licenseCatalogList.global') },
      { key: 'tipoSoggetto', title: this.transloco.translate('pages.licenseCatalogList.columnSubject'),
        value: l => this.transloco.translate('pages.license.subject' + l.tipoSoggetto) },
      { key: 'ambito', title: this.transloco.translate('pages.licenseCatalogList.columnFunction'),
        value: l => this.transloco.translate('pages.license.function' + l.ambito) },
      { key: 'prezzi', title: this.transloco.translate('pages.licenseCatalogList.columnPrices'), searchable: false, sortable: false,
        value: l => l.piano.map(p => p.codice + ' ' + p.prezzoMensile.toFixed(2)).join(' · ') },
      { key: 'abbonamenti', title: this.transloco.translate('pages.licenseCatalogList.columnSubscriptions'), align: 'end', mobile: 'note' },
    ];

    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
    this.actions = [{ button: this.btnDelete, visible: l => l.canEdit && l.attivo }, { button: this.btnEdit }];
  }

  ionViewWillEnter() {
    this.load();
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  private load() {
    this.licenseService.LicenseCatalogList(new GenericRequest<LicenseCatalogListInModel>(LicenseCatalogListInModel)).subscribe(res => {
      this.listini = res.success && res.data ? res.data.listino : [];
      this.canCreate = !!res.data?.canCreate;
      this.isSuperUser = !!res.data?.isSuperUser;

      const fab: FabMenuItem[] = [];
      if (this.canCreate)
        fab.push(new FabMenuItem(this.transloco.translate('pages.licenseCatalogList.newCatalog'), 'add-circle-outline', () => {
          this.navCtrl.navigateForward('/licensecatalogedit', { state: { id: 0 } });
        }));
      if (this.isSuperUser)
        fab.push(new FabMenuItem(this.transloco.translate('pages.licenseCatalogList.settings'), 'options-outline', () => {
          this.navCtrl.navigateForward('/licensesettings');
        }));
      this.fabMenuService.fabMenuItem = fab;
    });
  }

  handleButtonEditClick = (item: LicenseCatalogModel) => {
    this.navCtrl.navigateForward('/licensecatalogedit', { state: { id: item.id } });
  }

  handleButtonDeleteClick = async (item: LicenseCatalogModel) => {
    const message = item.abbonamenti > 0
      ? this.transloco.translate('pages.licenseCatalogList.confirmDeactivate', { count: item.abbonamenti })
      : this.transloco.translate('pages.licenseCatalogList.confirmDelete');
    if (!await this.collectionDialogService.ConfirmCancelDialog(message))
      return;
    const request = new GenericRequest<LicenseCatalogDeleteInModel>(LicenseCatalogDeleteInModel);
    request.data.id = item.id;
    this.licenseService.LicenseCatalogDelete(request).subscribe(() => this.load());
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
