import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CrmService } from '../../../ClientServer-Service/Crm/crm.service';
import * as M from '../../../ClientServer-Service/Crm/Models/crm-model';
import { CrmSetupService, crmDataOra } from '../../../Utility/Crm/crm-setup.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

@Component({
  selector: 'app-crm-request-list-page',
  templateUrl: './crm-request-list-page.component.html',
  styleUrls: ['./crm-request-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Richieste di assistenza dei clienti dell'azienda (CRM-ASS, ANALISI_CRM_ASSISTENZA.md): aperte / tutte / mie, con i tempi
 di presa in carico e di risoluzione. Cosa si vede lo decide il server (visibilita' del CRM + richieste assegnate).
*/
export class CrmRequestListPageComponent {

  public title: string;
  public searchText!: string;
  public sezione = 'aperte';
  public items: M.CrmRichiestaListModel[] | null = null;
  public readonly columns: NvxListColumn<M.CrmRichiestaListModel>[];
  public readonly actions: NvxListAction<M.CrmRichiestaListModel>[];
  // urgenti e alte ancora aperte in evidenza
  public readonly evidenza = (x: M.CrmRichiestaListModel) => x.stato <= M.CrmStatoRichiesta.InAttesaCliente && x.priorita >= M.CrmPrioritaRichiesta.Alta;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: CrmService,
              private setupService: CrmSetupService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    const ore = (v: number | null) => v != null ? String(v) : '';
    this.title = t('crm.pages.requestList.title');
    this.columns = [
      { key: 'numeroCompleto', title: t('crm.fields.numero'), mobile: 'note' },
      { key: 'cliente', title: t('crm.fields.cliente'), mobile: 'title' },
      { key: 'oggetto', title: t('crm.fields.oggetto'), mobile: 'subtitle' },
      { key: 'priorita', title: t('crm.fields.priorita'), value: x => t('crm.enums.prioritaRichiesta.' + x.priorita), sortValue: x => x.priorita },
      { key: 'stato', title: t('crm.fields.stato'), value: x => t('crm.enums.statoRichiesta.' + x.stato), sortValue: x => x.stato },
      { key: 'assegnatario', title: t('crm.fields.assegnatario'), mobile: 'hidden', value: x => x.assegnatario ?? '' },
      { key: 'dataApertura', title: t('crm.fields.apertura'), mobile: 'hidden', value: x => crmDataOra(x.dataApertura), sortValue: x => x.dataApertura },
      { key: 'orePresaInCarico', title: t('crm.fields.orePresaInCarico'), align: 'end', mobile: 'hidden', value: x => ore(x.orePresaInCarico), sortValue: x => x.orePresaInCarico ?? -1 },
      { key: 'oreRisoluzione', title: t('crm.fields.oreRisoluzione'), align: 'end', mobile: 'hidden', value: x => ore(x.oreRisoluzione), sortValue: x => x.oreRisoluzione ?? -1 },
    ];
    const bEdit = userInterfaceService.Btn_Modifica;
    bEdit.event = (x: M.CrmRichiestaListModel) => this.navCtrl.navigateForward('/crmrequestedit', { state: { id: x.id } });
    this.actions = [{ button: bEdit }];
  }

  async ionViewWillEnter() {
    await this.setupService.load();
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('crm.pages.requestList.new'), 'add-circle-outline', () => this.navCtrl.navigateForward('/crmrequestedit', { state: { id: 0 } })),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    const request = new GenericRequest<M.CrmRichiestaListInModel>(M.CrmRichiestaListInModel);
    request.data = { idAz_Cliente: null, soloAperte: this.sezione === 'aperte', soloMie: this.sezione === 'mie' };
    this.service.RichiestaList(request).subscribe(res => this.items = res.success && res.data ? res.data.richieste : []);
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
