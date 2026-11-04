import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, ModalController, AlertController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, from, map, of, switchMap } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CrmService } from '../../../ClientServer-Service/Crm/crm.service';
import * as M from '../../../ClientServer-Service/Crm/Models/crm-model';
import { CrmSetupService, crmAdesso, crmData, crmDataOra, crmImporto, crmPerInputData, crmPerInputDataOra } from '../../../Utility/Crm/crm-setup.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';

@Component({
  selector: 'app-crm-par-list-page',
  templateUrl: './crm-par-list-page.component.html',
  styleUrls: ['./crm-par-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Tabelle del CRM (CompanyPowerAdmin): fasi, tipi di attivita', motivi di perdita, fonti, prodotti, tag
export class CrmParListPageComponent {

  public title: string;
  public searchText!: string;
  public tabella = M.CrmParTabella.Fase;
  public readonly tabelle = [0, 1, 2, 3, 4, 5];
  public showDisabled = false;
  public items: M.CrmParModel[] | null = null;
  public columns: NvxListColumn<M.CrmParModel>[] = [];
  public readonly actions: NvxListAction<M.CrmParModel>[];
  public readonly isDisabled = (x: M.CrmParModel) => !x.attivo;
  public readonly sort = (a: M.CrmParModel, b: M.CrmParModel) => a.ordine - b.ordine || a.descrizione.localeCompare(b.descrizione);

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: CrmService,
              private setupService: CrmSetupService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('crm.pages.parList.title');
    const bEdit = userInterfaceService.Btn_Modifica;
    bEdit.event = (x: M.CrmParModel) => this.navCtrl.navigateForward('/crmparedit', { state: { item: { ...x } } });
    const bDel = userInterfaceService.Btn_Cancella;
    bDel.event = (x: M.CrmParModel) => this.elimina(x);
    this.actions = [{ button: bDel }, { button: bEdit }];
  }

  async ionViewWillEnter() {
    await this.setupService.load();
    this.mostra();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('crm.pages.parList.new'), 'add-circle-outline', () =>
        this.navCtrl.navigateForward('/crmparedit', { state: { item: Object.assign(new M.CrmParModel(), { tabella: this.tabella }) } })),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  mostra() {
    const s = this.setupService.setup;
    const t = (k: string) => this.transloco.translate(k);
    const liste: Record<number, M.CrmParModel[] | undefined> = { 0: s?.fasi, 1: s?.tipiAttivita, 2: s?.motiviPerdita, 3: s?.fonti, 4: s?.prodotti, 5: s?.tag };
    this.items = liste[this.tabella] ?? [];
    const cols: NvxListColumn<M.CrmParModel>[] = [{ key: 'descrizione', title: t('crm.fields.descrizione'), mobile: 'title' }];
    if (this.tabella === M.CrmParTabella.Fase)
      cols.push({ key: 'tipo', title: t('crm.fields.tipoFase'), mobile: 'subtitle', value: x => t('crm.enums.faseTipo.' + x.tipo) },
                { key: 'probabilita', title: t('crm.fields.probabilita'), align: 'end', mobile: 'note', value: x => x.probabilita + '%' });
    if (this.tabella === M.CrmParTabella.TipoAttivita)
      cols.push({ key: 'durataMinuti', title: t('crm.fields.durata'), align: 'end', mobile: 'note' });
    if (this.tabella === M.CrmParTabella.Prodotto)
      cols.push({ key: 'codice', title: t('crm.fields.codice'), mobile: 'subtitle' },
                { key: 'prezzo', title: t('crm.fields.prezzo'), align: 'end', mobile: 'note', value: x => crmImporto(x.prezzo, s?.cfg.valuta), sortValue: x => x.prezzo });
    cols.push({ key: 'ordine', title: t('crm.fields.ordine'), align: 'end', mobile: 'hidden' });
    this.columns = cols;
  }

  private async elimina(x: M.CrmParModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('crm.pages.parList.confirmDelete', { nome: x.descrizione })))
      return;
    const request = new GenericRequest<M.CrmParDeleteInModel>(M.CrmParDeleteInModel);
    request.data = { tabella: x.tabella, id: x.id };
    this.service.ParDelete(request).subscribe(async res => { if (res.success) { await this.setupService.load(); this.mostra(); } });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
