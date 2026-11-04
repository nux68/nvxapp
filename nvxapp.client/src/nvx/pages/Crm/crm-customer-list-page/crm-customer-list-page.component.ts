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
  selector: 'app-crm-customer-list-page',
  templateUrl: './crm-customer-list-page.component.html',
  styleUrls: ['./crm-customer-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Clienti del CRM visibili all'utente (regola di visibilita' del CRM), con stato, responsabile e attivita'
export class CrmCustomerListPageComponent {

  public title: string;
  public searchText!: string;
  public items: M.CrmClienteListModel[] | null = null;
  public showDisabled = false;
  public filtro = new M.CrmClienteListInModel();
  public readonly stati = [0, 1, 2, 3];
  public readonly columns: NvxListColumn<M.CrmClienteListModel>[];
  public readonly actions: NvxListAction<M.CrmClienteListModel>[];
  public readonly isDisabled = (x: M.CrmClienteListModel) => !x.attivo;
  public readonly sort = (a: M.CrmClienteListModel, b: M.CrmClienteListModel) => a.descrizione.localeCompare(b.descrizione);

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: CrmService,
              public setupService: CrmSetupService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('crm.pages.customerList.title');
    const valuta = () => this.setupService.setup?.cfg.valuta ?? 'EUR';
    this.columns = [
      { key: 'descrizione', title: t('crm.fields.cliente'), mobile: 'title' },
      { key: 'stato', title: t('crm.fields.stato'), mobile: 'note', value: x => t('crm.enums.statoCliente.' + x.stato) },
      { key: 'responsabile', title: t('crm.fields.responsabile'), mobile: 'subtitle', value: x => x.responsabile ?? '' },
      { key: 'citta', title: t('crm.fields.citta'), mobile: 'hidden' },
      { key: 'opportunitaAperte', title: t('crm.fields.opportunitaAperte'), align: 'end', mobile: 'hidden' },
      { key: 'valoreAperto', title: t('crm.fields.valoreAperto'), align: 'end', mobile: 'hidden', value: x => x.valoreAperto ? crmImporto(x.valoreAperto, valuta()) : '', sortValue: x => x.valoreAperto },
      { key: 'prossimaAttivita', title: t('crm.fields.prossimaAttivita'), mobile: 'hidden', value: x => crmDataOra(x.prossimaAttivita), sortValue: x => x.prossimaAttivita ?? '9999' },
    ];
    const btnEdit = userInterfaceService.Btn_Modifica;
    btnEdit.event = (x: M.CrmClienteListModel) => this.navCtrl.navigateForward('/crmcustomeredit', { state: { id: x.id } });
    this.actions = [{ button: btnEdit }];
  }

  async ionViewWillEnter() {
    await this.setupService.load();
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('crm.pages.customerList.new'), 'add-circle-outline', () => this.navCtrl.navigateForward('/crmcustomeredit', { state: { id: 0 } })),
      new FabMenuItem(this.transloco.translate('crm.pages.customerList.export'), 'download-outline', () => this.esporta()),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    const request = new GenericRequest<M.CrmClienteListInModel>(M.CrmClienteListInModel);
    request.data = { ...this.filtro };
    this.service.ClienteList(request).subscribe(res => this.items = res.success && res.data ? res.data.clienti : []);
  }

  esporta() {
    const request = new GenericRequest<M.CrmClienteListInModel>(M.CrmClienteListInModel);
    request.data = { ...this.filtro };
    this.service.ClienteExport(request);
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
