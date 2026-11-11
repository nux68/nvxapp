import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AlertController, ModalController, NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService, hrData } from '../../../ClientServer-Service/HumanResources/hr.service';
import * as M from '../../../ClientServer-Service/HumanResources/Models/hr-model';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { hrScegliDipendenti } from '../hr-ui';

@Component({
  selector: 'app-hr-checklist-list-page',
  templateUrl: './hr-checklist-list-page.component.html',
  styleUrls: ['./hr-checklist-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Checklist di assunzione e cessazione dei dipendenti (HR-ONB.1): il server genera all'apertura quelle mancanti dai modelli
 predefiniti sulle date di Common; il "+" crea a mano una checklist da un modello per un dipendente.
*/
export class HrChecklistListPageComponent {

  public title: string;
  public searchText!: string;
  public items: M.HrChecklistModel[] | null = null;
  public soloAperte = true;
  private modelli: M.HrModelloModel[] = [];
  private dipendenti: M.HrDipendenteBreveModel[] = [];
  public readonly columns: NvxListColumn<M.HrChecklistModel>[];
  public readonly actions: NvxListAction<M.HrChecklistModel>[];
  public readonly scaduta = (x: M.HrChecklistModel) => !!x.prossimaScadenza && x.prossimaScadenza.substring(0, 10) < new Date().toISOString().substring(0, 10);

  constructor(private navCtrl: NavController,
              private modalCtrl: ModalController,
              private alertCtrl: AlertController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: HrService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('hr.pages.checklistList.title');
    this.columns = [
      { key: 'dipendente', title: t('hr.fields.dipendente'), mobile: 'title' },
      { key: 'tipo', title: t('hr.fields.tipoChecklist'), mobile: 'subtitle', value: x => t('hr.enums.tipoChecklist.' + x.tipo) + ' ' + hrData(x.dataRiferimento) },
      { key: 'descrizione', title: t('hr.fields.descrizione'), mobile: 'hidden' },
      { key: 'avanzamento', title: t('hr.fields.avanzamento'), align: 'end', mobile: 'note', value: x => `${x.numeroCompletate}/${x.numeroVoci}` },
      { key: 'prossimaScadenza', title: t('hr.fields.prossimaScadenza'), mobile: 'hidden', value: x => hrData(x.prossimaScadenza), sortValue: x => x.prossimaScadenza ?? '9999' },
      { key: 'stato', title: t('hr.fields.stato'), mobile: 'hidden', value: x => t('hr.enums.statoChecklist.' + x.stato) },
    ];
    const bOpen = userInterfaceService.Btn_Apri;
    bOpen.event = (x: M.HrChecklistModel) => this.navCtrl.navigateForward('/hrchecklistedit', { state: { id: x.id } });
    this.actions = [{ button: bOpen }];
  }

  ionViewWillEnter() {
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('hr.pages.checklistList.new'), 'add-circle-outline', () => this.nuova()),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    const request = new GenericRequest<M.HrChecklistListInModel>(M.HrChecklistListInModel);
    request.data.soloAperte = this.soloAperte;
    this.service.ChecklistList(request).subscribe(res => {
      this.items = res.success && res.data ? res.data.checklist : [];
      this.modelli = res.success ? res.data.modelli : [];
      this.dipendenti = res.success ? res.data.dipendenti : [];
    });
  }

  // dipendente e modello: la data e' quella di assunzione o cessazione del dipendente
  async nuova() {
    const t = (k: string) => this.transloco.translate(k);
    if (this.modelli.length === 0) {
      const a = await this.alertCtrl.create({ message: t('hr.pages.checklistList.noTemplates'), buttons: [t('common.buttons.close')] });
      await a.present();
      return;
    }
    const scelti = await hrScegliDipendenti(this.modalCtrl, this.dipendenti.filter(d => d.attivo), 'single');
    if (!scelti || scelti.length === 0) return;
    const alert = await this.alertCtrl.create({
      header: t('hr.pages.checklistList.chooseTemplate'),
      inputs: this.modelli.map((m, i) => ({ type: 'radio' as const, label: t('hr.enums.tipoChecklist.' + m.tipo) + ': ' + m.descrizione, value: m.id, checked: i === 0 })),
      buttons: [{ text: t('common.buttons.cancel'), role: 'cancel' }, { text: t('common.buttons.confirm'), role: 'confirm' }]
    });
    await alert.present();
    const { data, role } = await alert.onDidDismiss();
    if (role !== 'confirm' || !data?.values) return;
    const request = new GenericRequest<M.HrChecklistCreaInModel>(M.HrChecklistCreaInModel);
    request.data = { idDip_Anagrafica: scelti[0], idHr_ChecklistModello: data.values, dataRiferimento: null };
    this.service.ChecklistCrea(request).subscribe(res => {
      if (res.success) this.navCtrl.navigateForward('/hrchecklistedit', { state: { id: res.data.checklist.id } });
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
