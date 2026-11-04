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
  selector: 'app-crm-pipeline-page',
  templateUrl: './crm-pipeline-page.component.html',
  styleUrls: ['./crm-pipeline-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Pipeline delle opportunita': colonne per fase (aperte, poi vinte e perse degli ultimi 30 giorni). Si cambia fase
 trascinando la scheda (o con la scelta della fase su mobile); passando a una fase "persa" si chiede il motivo.
 Vista elenco in alternativa.
*/
export class CrmPipelinePageComponent {

  public title: string;
  public searchText = '';
  public vista: 'kanban' | 'elenco' = 'kanban';
  public idResponsabile: string | null = null;
  public opportunita: M.CrmOpportunitaListModel[] = [];
  public fasi: M.CrmParModel[] = [];
  private trascinata: M.CrmOpportunitaListModel | null = null;
  public readonly columns: NvxListColumn<M.CrmOpportunitaListModel>[];
  public readonly actions: NvxListAction<M.CrmOpportunitaListModel>[];
  public readonly data = crmData;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: CrmService,
              public setupService: CrmSetupService,
              private alertCtrl: AlertController,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('crm.pages.pipeline.title');
    this.columns = [
      { key: 'titolo', title: t('crm.fields.titolo'), mobile: 'title' },
      { key: 'cliente', title: t('crm.fields.cliente'), mobile: 'subtitle' },
      { key: 'fase', title: t('crm.fields.fase'), mobile: 'note' },
      { key: 'valore', title: t('crm.fields.valore'), align: 'end', value: x => this.importo(x.valore), sortValue: x => x.valore },
      { key: 'probabilita', title: t('crm.fields.probabilita'), align: 'end', value: x => x.probabilita + '%', sortValue: x => x.probabilita },
      { key: 'dataChiusuraPrevista', title: t('crm.fields.chiusuraPrevista'), value: x => crmData(x.dataChiusura ?? x.dataChiusuraPrevista), sortValue: x => x.dataChiusura ?? x.dataChiusuraPrevista ?? '' },
      { key: 'responsabile', title: t('crm.fields.responsabile'), value: x => x.responsabile ?? '' },
    ];
    const b = userInterfaceService.Btn_Apri; b.event = (x: M.CrmOpportunitaListModel) => this.apri(x);
    this.actions = [{ button: b }];
  }

  importo(v: number): string { return crmImporto(v, this.setupService.setup?.cfg.valuta); }

  async ionViewWillEnter() {
    const setup = await this.setupService.load();
    // fasi attive: prima le aperte nell'ordine, poi vinte e perse
    const ordineTipo = (f: M.CrmParModel) => f.tipo === M.CrmFaseTipo.Aperta ? 0 : f.tipo === M.CrmFaseTipo.Vinta ? 1 : 2;
    this.fasi = (setup?.fasi ?? []).filter(f => f.attivo).sort((a, b) => ordineTipo(a) - ordineTipo(b) || a.ordine - b.ordine);
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('crm.pages.pipeline.new'), 'add-circle-outline', () => this.navCtrl.navigateForward('/crmopportunityedit', { state: { id: 0 } })),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    const request = new GenericRequest<M.CrmOpportunitaListInModel>(M.CrmOpportunitaListInModel);
    request.data.idResponsabile = this.idResponsabile;
    this.service.OpportunitaList(request).subscribe(res => this.opportunita = res.success && res.data ? res.data.opportunita : []);
  }

  private filtrate(): M.CrmOpportunitaListModel[] {
    const s = (this.searchText ?? '').toLowerCase();
    return s ? this.opportunita.filter(o => (o.titolo + ' ' + o.cliente).toLowerCase().includes(s)) : this.opportunita;
  }

  colonna(f: M.CrmParModel): M.CrmOpportunitaListModel[] {
    return this.filtrate().filter(o => o.idCrm_Par_Fase === f.id);
  }

  totale(f: M.CrmParModel): number {
    return this.colonna(f).reduce((t, o) => t + o.valore, 0);
  }

  apri(o: M.CrmOpportunitaListModel) {
    this.navCtrl.navigateForward('/crmopportunityedit', { state: { id: o.id } });
  }

  // trascinamento (desktop)
  inizio(ev: DragEvent, o: M.CrmOpportunitaListModel) {
    this.trascinata = o;
    ev.dataTransfer?.setData('text/plain', String(o.id));
  }

  sopra(ev: DragEvent) {
    ev.preventDefault();
  }

  rilascia(ev: DragEvent, f: M.CrmParModel) {
    ev.preventDefault();
    const o = this.trascinata;
    this.trascinata = null;
    if (o && o.idCrm_Par_Fase !== f.id)
      this.cambiaFase(o, f);
  }

  // scelta della fase (mobile o tastiera)
  async scegliFase(o: M.CrmOpportunitaListModel) {
    const alert = await this.alertCtrl.create({
      header: this.transloco.translate('crm.pages.pipeline.movePhase'),
      inputs: this.fasi.map(f => ({ type: 'radio' as const, label: f.descrizione, value: f.id, checked: f.id === o.idCrm_Par_Fase })),
      buttons: [{ text: this.transloco.translate('common.buttons.cancel'), role: 'cancel' }, { text: 'OK', role: 'confirm' }]
    });
    await alert.present();
    const { data, role } = await alert.onDidDismiss();
    const f = this.fasi.find(x => x.id === data?.values);
    if (role === 'confirm' && f && f.id !== o.idCrm_Par_Fase)
      this.cambiaFase(o, f);
  }

  private async cambiaFase(o: M.CrmOpportunitaListModel, f: M.CrmParModel) {
    let motivo: number | null = null;
    if (f.tipo === M.CrmFaseTipo.Persa) {
      const motivi = (this.setupService.setup?.motiviPerdita ?? []).filter(m => m.attivo);
      const alert = await this.alertCtrl.create({
        header: this.transloco.translate('crm.pages.pipeline.lossReason'),
        inputs: motivi.map(m => ({ type: 'radio' as const, label: m.descrizione, value: m.id })),
        buttons: [{ text: this.transloco.translate('common.buttons.cancel'), role: 'cancel' }, { text: 'OK', role: 'confirm' }]
      });
      await alert.present();
      const { data, role } = await alert.onDidDismiss();
      if (role !== 'confirm')
        return;
      motivo = data?.values ?? null;
    }
    const request = new GenericRequest<M.CrmCambiaFaseInModel>(M.CrmCambiaFaseInModel);
    request.data = { id: o.id, idCrm_Par_Fase: f.id, idCrm_Par_MotivoPerdita: motivo };
    this.service.OpportunitaCambiaFase(request).subscribe(res => { if (res.success) this.load(); });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
