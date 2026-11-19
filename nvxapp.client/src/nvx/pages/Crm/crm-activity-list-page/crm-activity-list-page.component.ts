import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, ModalController, AlertController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, Subscription, catchError, from, map, of, switchMap } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CrmService } from '../../../ClientServer-Service/Crm/crm.service';
import * as M from '../../../ClientServer-Service/Crm/Models/crm-model';
import { CrmSetupService, crmAdesso, crmData, crmDataOra, crmImporto, crmPerInputData, crmPerInputDataOra } from '../../../Utility/Crm/crm-setup.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { MonthNavigatorService } from '../../../Utility/infrastructure/month-navigator.service';
import { LanguageService } from '../../../Utility/infrastructure/language.service';
import { ApplicationType } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { DataChangedNotifierService } from '../../../Utility/infrastructure/data-changed-notifier.service';

export type CrmVistaAgenda = 'lista' | 'settimana' | 'mese';

// una cella del calendario (settimana o mese)
export interface CrmGiornoAgenda {
  data: Date;
  chiave: string;          // yyyy-MM-dd locale
  numero: number;
  nome: string;            // giorno della settimana abbreviato
  oggi: boolean;
  fuoriMese: boolean;
  attivita: M.CrmAttivitaListModel[];
}

const VISTA_KEY = 'nvx.crm.agenda.vista';
// data locale yyyy-MM-dd (toISOString userebbe UTC e sposterebbe il giorno)
const chiaveGiorno = (d: Date) => `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
// lunedi' della settimana che contiene d
const lunedi = (d: Date) => { const r = new Date(d.getFullYear(), d.getMonth(), d.getDate()); r.setDate(r.getDate() - (r.getDay() + 6) % 7); return r; };
const piuGiorni = (d: Date, n: number) => { const r = new Date(d); r.setDate(r.getDate() + n); return r; };

@Component({
  selector: 'app-crm-activity-list-page',
  templateUrl: './crm-activity-list-page.component.html',
  styleUrls: ['./crm-activity-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
// Agenda: attivita' di oggi, scadute (da fare, gia' iniziate), prossime e fatte; gli amministratori possono vedere quelle di tutti.
// Oltre alla lista, vista calendario per settimana e per mese
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

  // vista calendario
  public vista: CrmVistaAgenda = 'lista';
  public riferimento = new Date();
  public giorni: CrmGiornoAgenda[] = [];
  public periodo = '';
  public readonly maxPerGiornoMese = 3;
  private calendario: M.CrmAttivitaListModel[] = [];
  // dati modificati da un comando eseguito fuori dalla pagina (assistente AI via MCP): ANALISI_INFRASTRUCTURE_MCP.md, MCP-43
  private dataChangedSub: Subscription | null = null;

  constructor(private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: CrmService,
              public setupService: CrmSetupService,
              private alertCtrl: AlertController,
              private transloco: TranslocoService,
              private monthNavigator: MonthNavigatorService,
              private languageService: LanguageService,
              private dataChangedNotifierService: DataChangedNotifierService) {
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
    try {
      const v = localStorage.getItem(VISTA_KEY) as CrmVistaAgenda | null;
      if (v === 'lista' || v === 'settimana' || v === 'mese') this.vista = v;
    } catch { }
  }

  get amministratore(): boolean { return !!(this.setupService.setup?.permessi.powerAdmin || this.setupService.setup?.permessi.admin); }

  async ionViewWillEnter() {
    await this.setupService.load();
    this.load();
    // ricarica con vista, sezione e periodo correnti
    this.dataChangedSub?.unsubscribe();
    this.dataChangedSub = this.dataChangedNotifierService.of(ApplicationType.Crm).subscribe(() => this.load());
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('crm.pages.activityList.new'), 'add-circle-outline', () => this.navCtrl.navigateForward('/crmactivityedit', { state: { id: 0 } })),
    ];
  }

  ionViewWillLeave() {
    this.dataChangedSub?.unsubscribe();
    this.dataChangedSub = null;
    this.fabMenuService.fabMenuItem = [];
  }

  load() {
    if (this.vista !== 'lista') {
      this.loadCalendario();
      return;
    }
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
    if (this.vista !== 'lista') this.componiCalendario();
  }

  // ---------- vista settimana / mese ----------

  cambiaVista() {
    try { localStorage.setItem(VISTA_KEY, this.vista); } catch { }
    this.load();
  }

  // primo e ultimo giorno mostrati (il mese parte dal lunedi' e finisce la domenica)
  private intervallo(): { dal: Date; al: Date } {
    const r = this.riferimento;
    if (this.vista === 'settimana') {
      const dal = lunedi(r);
      return { dal, al: piuGiorni(dal, 6) };
    }
    const dal = lunedi(new Date(r.getFullYear(), r.getMonth(), 1));
    const fineMese = new Date(r.getFullYear(), r.getMonth() + 1, 0);
    return { dal, al: piuGiorni(lunedi(fineMese), 6) };
  }

  loadCalendario() {
    const { dal, al } = this.intervallo();
    this.componiCalendario();
    const request = new GenericRequest<M.CrmAttivitaListInModel>(M.CrmAttivitaListInModel);
    request.data = { dal: chiaveGiorno(dal), al: chiaveGiorno(al), soloMie: this.soloMie, soloDaFare: false };
    this.service.AttivitaList(request).subscribe(res => {
      this.calendario = res.success && res.data ? res.data.attivita : [];
      this.componiCalendario();
    });
  }

  componiCalendario() {
    const { dal, al } = this.intervallo();
    const locale = this.languageService.locale;
    const testo = (this.searchText ?? '').trim().toLowerCase();
    const visibili = !testo ? this.calendario : this.calendario.filter(x =>
      [x.tipo, x.oggetto, x.cliente, x.lead, x.opportunita, x.assegnatario].some(v => !!v && v.toLowerCase().includes(testo)));
    const perGiorno = new Map<string, M.CrmAttivitaListModel[]>();
    for (const x of [...visibili].sort((a, b) => a.inizio.localeCompare(b.inizio))) {
      const k = chiaveGiorno(new Date(x.inizio));
      if (!perGiorno.has(k)) perGiorno.set(k, []);
      perGiorno.get(k)!.push(x);
    }
    const oggi = chiaveGiorno(new Date());
    const mese = this.riferimento.getMonth();
    const giorni: CrmGiornoAgenda[] = [];
    for (let d = new Date(dal), i = 0; d <= al; d = piuGiorni(d, 1), i++) {
      const k = chiaveGiorno(d);
      giorni.push({ data: d, chiave: k, numero: d.getDate(), nome: this.monthNavigator.weekDays[i % 7], oggi: k === oggi,
                    fuoriMese: this.vista === 'mese' && d.getMonth() !== mese, attivita: perGiorno.get(k) ?? [] });
    }
    this.giorni = giorni;
    if (this.vista === 'mese')
      this.periodo = new Intl.DateTimeFormat(locale, { month: 'long', year: 'numeric' }).format(this.riferimento);
    else {
      const f: any = new Intl.DateTimeFormat(locale, { day: 'numeric', month: 'short', year: 'numeric' });
      this.periodo = f.formatRange ? f.formatRange(dal, al) : f.format(dal) + ' - ' + f.format(al);
    }
  }

  sposta(passo: number) {
    const r = this.riferimento;
    this.riferimento = this.vista === 'settimana'
      ? piuGiorni(r, 7 * passo)
      : new Date(r.getFullYear(), r.getMonth() + passo, 1);
    this.loadCalendario();
  }

  vaiAOggi() {
    this.riferimento = new Date();
    this.loadCalendario();
  }

  // dal mese, un giorno con troppe attivita' apre la sua settimana
  apriSettimana(g: CrmGiornoAgenda, ev?: Event) {
    ev?.stopPropagation();
    this.riferimento = g.data;
    this.vista = 'settimana';
    this.cambiaVista();
  }

  apri(x: M.CrmAttivitaListModel, ev?: Event) {
    ev?.stopPropagation();
    this.navCtrl.navigateForward('/crmactivityedit', { state: { id: x.id } });
  }

  // nuova attivita' nel giorno scelto, alle 9 (o all'ora successiva se e' oggi)
  nuova(g: CrmGiornoAgenda) {
    const ora = g.oggi ? Math.min(new Date().getHours() + 1, 23) : 9;
    this.navCtrl.navigateForward('/crmactivityedit', { state: { id: 0, preset: { inizio: `${g.chiave}T${String(ora).padStart(2, '0')}:00` } } });
  }

  ora(x: M.CrmAttivitaListModel): string {
    return new Date(x.inizio).toLocaleTimeString(this.languageService.locale, { hour: '2-digit', minute: '2-digit' });
  }

  riferimentoDi(x: M.CrmAttivitaListModel): string {
    return [x.cliente ?? x.lead, x.opportunita].filter(v => !!v).join(' · ');
  }

  chiusa(x: M.CrmAttivitaListModel): boolean {
    return x.stato !== M.CrmStatoAttivita.DaFare;
  }
}
