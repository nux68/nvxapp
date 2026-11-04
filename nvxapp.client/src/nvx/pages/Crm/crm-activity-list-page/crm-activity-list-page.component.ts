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
  selector: 'app-crm-activity-list-page',
  templateUrl: './crm-activity-list-page.component.html',
  styleUrls: ['./crm-activity-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Agenda: attivita' di oggi, scadute (da fare, gia' iniziate), prossime e fatte; gli amministratori possono vedere quelle di tutti
export class CrmActivityListPageComponent {

  public title: string;
  public searchText!: string;
  public sezione = 'oggi';
  public soloMie = true;
  public tutte: M.CrmAttivitaListModel[] = [];
  public items: M.CrmAttivitaListModel[] | null = null;
  public readonly columns: NvxListColumn<M.CrmAttivitaListModel>[];
  public readonly actions: NvxListAction<M.CrmAttivitaListModel>[];
  public readonly scaduta = (x: M.CrmAttivitaListModel) => x.stato === M.CrmStatoAttivita.DaFare && new Date(x.inizio) < new Date();

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: CrmService,
              public setupService: CrmSetupService,
              private alertCtrl: AlertController,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('crm.pages.activityList.title');
    this.columns = [
      { key: 'inizio', title: t('crm.fields.quando'), mobile: 'note', value: x => crmDataOra(x.inizio), sortValue: x => x.inizio },
      { key: 'oggetto', title: t('crm.fields.oggetto'), mobile: 'title', value: x => x.tipo + ': ' + x.oggetto },
      { key: 'riferimento', title: t('crm.fields.riferimento'), mobile: 'subtitle', value: x => [x.cliente ?? x.lead, x.opportunita].filter(v => !!v).join(' · ') },
      { key: 'stato', title: t('crm.fields.stato'), value: x => t('crm.enums.statoAttivita.' + x.stato) },
      { key: 'assegnatario', title: t('crm.fields.assegnatario'), mobile: 'hidden', value: x => x.assegnatario ?? '' },
    ];
    const bFatta = userInterfaceService.Btn_Approva;
    bFatta.text = t('crm.pages.activityList.done');
    bFatta.event = (x: M.CrmAttivitaListModel) => this.fatta(x);
    const bEdit = userInterfaceService.Btn_Modifica;
    bEdit.event = (x: M.CrmAttivitaListModel) => this.navCtrl.navigateForward('/crmactivityedit', { state: { id: x.id } });
    this.actions = [{ button: bFatta, visible: x => x.stato === M.CrmStatoAttivita.DaFare }, { button: bEdit }];
  }

  get amministratore(): boolean { return !!(this.setupService.setup?.permessi.powerAdmin || this.setupService.setup?.permessi.admin); }

  async ionViewWillEnter() {
    await this.setupService.load();
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('crm.pages.activityList.new'), 'add-circle-outline', () => this.navCtrl.navigateForward('/crmactivityedit', { state: { id: 0 } })),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    const oggi = new Date();
    const giorni = (n: number) => { const d = new Date(oggi); d.setDate(d.getDate() + n); return d.toISOString().substring(0, 10); };
    const request = new GenericRequest<M.CrmAttivitaListInModel>(M.CrmAttivitaListInModel);
    request.data = { dal: giorni(-60), al: giorni(180), soloMie: this.soloMie, soloDaFare: false };
    this.service.AttivitaList(request).subscribe(res => {
      this.tutte = res.success && res.data ? res.data.attivita : [];
      this.filtra();
    });
  }

  filtra() {
    const adesso = new Date();
    const oggi = adesso.toDateString();
    const daFare = this.tutte.filter(x => x.stato === M.CrmStatoAttivita.DaFare);
    switch (this.sezione) {
      case 'oggi': this.items = daFare.filter(x => new Date(x.inizio).toDateString() === oggi || new Date(x.inizio) < adesso); break;
      case 'scadute': this.items = daFare.filter(x => new Date(x.inizio) < adesso); break;
      case 'prossime': this.items = daFare.filter(x => new Date(x.inizio) >= adesso); break;
      default: this.items = this.tutte.filter(x => x.stato !== M.CrmStatoAttivita.DaFare).reverse(); break;
    }
  }

  conta(sezione: string): number {
    const adesso = new Date();
    const daFare = this.tutte.filter(x => x.stato === M.CrmStatoAttivita.DaFare);
    return sezione === 'scadute' ? daFare.filter(x => new Date(x.inizio) < adesso).length : 0;
  }

  async fatta(x: M.CrmAttivitaListModel) {
    const alert = await this.alertCtrl.create({
      header: this.transloco.translate('crm.pages.activityList.done'),
      subHeader: x.oggetto,
      inputs: [{ name: 'esito', type: 'textarea', placeholder: this.transloco.translate('crm.fields.esito') }],
      buttons: [{ text: this.transloco.translate('common.buttons.cancel'), role: 'cancel' }, { text: 'OK', role: 'confirm' }]
    });
    await alert.present();
    const { data, role } = await alert.onDidDismiss();
    if (role !== 'confirm')
      return;
    const request = new GenericRequest<M.CrmAttivitaCompletaInModel>(M.CrmAttivitaCompletaInModel);
    request.data = { id: x.id, esito: data?.values?.esito ?? null, annullata: false };
    this.service.AttivitaCompleta(request).subscribe(res => { if (res.success) this.load(); });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
