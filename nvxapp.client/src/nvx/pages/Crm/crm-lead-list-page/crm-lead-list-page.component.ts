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
  selector: 'app-crm-lead-list-page',
  templateUrl: './crm-lead-list-page.component.html',
  styleUrls: ['./crm-lead-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Lead (contatti non ancora qualificati): con "Mostra anche chiusi" compaiono i convertiti e gli scartati
export class CrmLeadListPageComponent {

  public title: string;
  public searchText!: string;
  public items: M.CrmLeadModel[] | null = null;
  public showDisabled = false;
  public readonly columns: NvxListColumn<M.CrmLeadModel>[];
  public readonly actions: NvxListAction<M.CrmLeadModel>[];
  public readonly isDisabled = (x: M.CrmLeadModel) => x.stato === M.CrmStatoLead.Convertito || x.stato === M.CrmStatoLead.Scartato;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: CrmService,
              private setupService: CrmSetupService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('crm.pages.leadList.title');
    this.columns = [
      { key: 'nominativo', title: t('crm.fields.nominativo'), mobile: 'title', value: x => [x.cognome, x.nome].filter(v => !!v).join(' ') || (x.azienda ?? '') },
      { key: 'azienda', title: t('crm.fields.azienda'), mobile: 'subtitle', value: x => x.azienda ?? '' },
      { key: 'stato', title: t('crm.fields.stato'), mobile: 'note', value: x => t('crm.enums.statoLead.' + x.stato) },
      { key: 'fonte', title: t('crm.fields.fonte'), mobile: 'hidden', value: x => this.setupService.descrizione(this.setupService.setup?.fonti, x.idCrm_Par_Fonte) },
      { key: 'responsabile', title: t('crm.fields.responsabile'), mobile: 'hidden', value: x => x.responsabile ?? '' },
      { key: 'creationDate', title: t('crm.fields.creato'), mobile: 'hidden', value: x => crmData(x.creationDate), sortValue: x => x.creationDate ?? '' },
    ];
    const btnEdit = userInterfaceService.Btn_Modifica;
    btnEdit.event = (x: M.CrmLeadModel) => this.navCtrl.navigateForward('/crmleadedit', { state: { id: x.id } });
    this.actions = [{ button: btnEdit }];
  }

  async ionViewWillEnter() {
    await this.setupService.load();
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('crm.pages.leadList.new'), 'add-circle-outline', () => this.navCtrl.navigateForward('/crmleadedit', { state: { id: 0 } })),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    const request = new GenericRequest<M.CrmLeadListInModel>(M.CrmLeadListInModel);
    request.data.ancheChiusi = true;
    this.service.LeadList(request).subscribe(res => this.items = res.success && res.data ? res.data.lead : []);
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
