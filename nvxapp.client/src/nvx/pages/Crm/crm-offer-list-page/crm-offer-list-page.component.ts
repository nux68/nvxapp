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
  selector: 'app-crm-offer-list-page',
  templateUrl: './crm-offer-list-page.component.html',
  styleUrls: ['./crm-offer-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Offerte visibili all'utente, con PDF e apertura
export class CrmOfferListPageComponent {

  public title: string;
  public searchText!: string;
  public items: M.CrmOffertaListModel[] | null = null;
  public readonly columns: NvxListColumn<M.CrmOffertaListModel>[];
  public readonly actions: NvxListAction<M.CrmOffertaListModel>[];

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: CrmService,
              private setupService: CrmSetupService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('crm.pages.offerList.title');
    this.columns = [
      { key: 'numeroCompleto', title: t('crm.fields.numero'), mobile: 'note' },
      { key: 'cliente', title: t('crm.fields.cliente'), mobile: 'title' },
      { key: 'oggetto', title: t('crm.fields.oggetto'), mobile: 'subtitle', value: x => x.oggetto ?? '' },
      { key: 'data', title: t('crm.fields.data'), value: x => crmData(x.data), sortValue: x => x.data },
      { key: 'stato', title: t('crm.fields.stato'), value: x => t('crm.enums.statoOfferta.' + x.stato) },
      { key: 'totale', title: t('crm.fields.totale'), align: 'end', value: x => crmImporto(x.totale, this.setupService.setup?.cfg.valuta), sortValue: x => x.totale },
    ];
    const bPdf = userInterfaceService.Btn_Pdf;
    bPdf.event = (x: M.CrmOffertaListModel) => this.setupService.pdfOfferta(x.id);
    const bEdit = userInterfaceService.Btn_Modifica;
    bEdit.event = (x: M.CrmOffertaListModel) => this.navCtrl.navigateForward('/crmofferedit', { state: { id: x.id } });
    this.actions = [{ button: bPdf }, { button: bEdit }];
  }

  async ionViewWillEnter() {
    await this.setupService.load();
    const request = new GenericRequest<M.CrmOffertaListInModel>(M.CrmOffertaListInModel);
    this.service.OffertaList(request).subscribe(res => this.items = res.success && res.data ? res.data.offerte : []);
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('crm.pages.offerList.new'), 'add-circle-outline', () => this.navCtrl.navigateForward('/crmofferedit', { state: { id: 0 } })),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
