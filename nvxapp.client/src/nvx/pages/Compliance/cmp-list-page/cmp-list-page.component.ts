import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NavController, ToastController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { GenericResult } from '../../../ClientServer-Service/ModelsBase/generic-result';
import { CmpService } from '../../../ClientServer-Service/Compliance/cmp.service';
import * as M from '../../../ClientServer-Service/Compliance/Models/cmp-model';
import { CMP_AREE, CMP_SEZIONI, CmpArea, CmpSezione, cmpOrdine, cmpValore } from '../../../Utility/Compliance/cmp-sezioni';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { LanguageService } from '../../../Utility/infrastructure/language.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

/*
 Elenchi di Compliance (ANALISI_COMPLIANCE_APPLICATIVO.md), una sezione alla volta (segmenti), area dalla rotta (data.area):
    - /cmp81     : sicurezza sul lavoro (CMP-81): corsi, situazione della formazione, visite, consegne DPI, nomine, documenti e tabelle.
                   Gli abilitati ai dati sanitari che non sono amministratori vedono solo le visite
    - /cmpgdpr   : registro GDPR (CMP-GDPR) dell'azienda, con l'export in Excel (job)
    - /cmpstudio : lato studio (CMP-GDPR.4): azienda scelta tra quelle dello studio con Compliance, registro GDPR e scadenze del modulo
 Nuovo elemento con il + del fab (sezione corrente), modifica e cancellazione dalle azioni di riga; la pagina di modifica e'
 unica (cmp-edit-page) e prende i campi dalla descrizione della sezione (cmp-sezioni.ts). I permessi veri li controlla il server.
 Lo stato di navigazione { sezione, id } (scadenze, notifiche, prese visione) apre la sezione indicata.
*/
@Component({
  selector: 'app-cmp-list-page',
  templateUrl: './cmp-list-page.component.html',
  styleUrls: ['./cmp-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CmpListPageComponent {

  public area: CmpArea = '81';
  public title = '';
  public searchText!: string;
  public sezioni: CmpSezione[] = [];
  public sezione: CmpSezione | null = null;
  public items: M.CmpRiga[] | null = null;
  public columns: NvxListColumn<M.CmpRiga>[] = [];
  public actions: NvxListAction<M.CmpRiga>[] = [];
  public buttonbar: ButtonItem[] = [];
  public showDisabled = false;

  // lato studio: aziende dello studio con Compliance attivo e azienda scelta
  public aziende: M.CmpStudioAziendaModel[] = [];
  public idCompany: number | null = null;
  private permessi: M.CmpPermessiAreeModel | null = null;

  // in evidenza: scadute, corsi obbligatori mancanti o scaduti, violazioni non notificate oltre le 72 ore
  public readonly highlight = (x: M.CmpRiga) => !!x.scaduta
                                                || (this.sezione?.speciale === 'stato' && (x['stato'] === 0 || x['stato'] === 3))
                                                || (!!x['notificaDovuta'] && !!x['notificaInRitardo']);
  // disattivate: corsi e visite rinnovati, voci non attive, scadenze chiuse
  public readonly isDisabled = (x: M.CmpRiga) => !!x.rinnovata || x['attivo'] === false || !!x['chiusa'];
  public readonly iconOf = (x: M.CmpRiga) => this.highlight(x) ? 'alert-circle-outline'
                                             : (x.inScadenza || x['notificaDovuta'] || x['stato'] === 2) ? 'time-outline' : (this.sezione?.icona ?? 'list-outline');

  constructor(private route: ActivatedRoute,
              private navCtrl: NavController,
              public fabMenuService: FabMenuService,
              private userInterfaceService: UserInterfaceService,
              private service: CmpService,
              private collectionDialogService: CollectionDialogService,
              private languageService: LanguageService,
              private toastController: ToastController,
              private transloco: TranslocoService) { }

  ionViewWillEnter() {
    this.area = (this.route.snapshot.data['area'] ?? '81') as CmpArea;
    this.title = this.transloco.translate('cmp.pages.list.title' + this.area);
    const state = history.state ?? {};
    if (state.idCompany)
      this.idCompany = state.idCompany;
    const richiesta: string | null = state.sezione ?? this.sezione?.key ?? null;
    this.fabMenuService.fabMenuItem = [];

    if (this.area === '81') {
      // permessi dalla tabella di scelta: gli abilitati ai dati sanitari non amministratori vedono solo le visite
      this.service.Lookup(new GenericRequest<M.CmpListInModel>(M.CmpListInModel)).subscribe(res => {
        this.permessi = res.success && res.data ? res.data.permessi : null;
        const admin = !!this.permessi && (this.permessi.powerAdmin || this.permessi.admin);
        this.imposta(CMP_AREE['81'].map(k => CMP_SEZIONI[k]).filter(s => admin || (s.sanitario && !!this.permessi?.sanitario)), richiesta);
      });
    }
    else if (this.area === 'studio') {
      this.service.StudioAziende(new GenericRequest<M.CmpListInModel>(M.CmpListInModel)).subscribe(res => {
        this.aziende = res.success && res.data ? res.data.items : [];
        if (this.idCompany && !this.aziende.some(a => a.idCompany === this.idCompany))
          this.idCompany = null;
        if (!this.idCompany && this.aziende.length === 1)
          this.idCompany = this.aziende[0].idCompany;
        this.imposta(CMP_AREE['studio'].map(k => CMP_SEZIONI[k]), richiesta);
      });
    }
    else
      this.imposta(CMP_AREE['gdpr'].map(k => CMP_SEZIONI[k]), richiesta);

    // export del registro GDPR (job): il file si scarica dalla barra dei job quando e' pronto
    if (this.area !== '81') {
      const btnExcel = this.userInterfaceService.Btn_Excel;
      btnExcel.event = () => this.export();
      this.buttonbar = [btnExcel];
    }
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  private imposta(sezioni: CmpSezione[], richiesta: string | null) {
    this.sezioni = sezioni;
    this.sezione = sezioni.find(s => s.key === richiesta) ?? sezioni[0] ?? null;
    this.mostra();
  }

  sezioneChanged(key: any) {
    this.sezione = this.sezioni.find(s => s.key === key) ?? this.sezione;
    this.mostra();
  }

  aziendaChanged(id: any) {
    this.idCompany = id ? Number(id) : null;
    this.mostra();
  }

  /** colonne, azioni, + del fab e dati della sezione corrente */
  mostra() {
    const s = this.sezione;
    this.items = null;
    if (!s) {
      this.items = [];
      return;
    }
    const t = (k: string) => this.transloco.translate(k);
    const locale = this.languageService.locale;
    this.columns = s.colonne.map(c => ({
      key: c.key, title: t('cmp.fields.' + c.key), mobile: c.mobile, align: c.align, width: c.width,
      value: (x: M.CmpRiga) => cmpValore(c, x, this.transloco, locale),
      sortValue: (x: M.CmpRiga) => cmpOrdine(c, x),
    }));

    const azioni: NvxListAction<M.CmpRiga>[] = [];
    if (s.campi.length > 0) {
      const bDel = this.userInterfaceService.Btn_Cancella;
      bDel.event = (x: M.CmpRiga) => this.elimina(x);
      const bEdit = this.userInterfaceService.Btn_Modifica;
      bEdit.event = (x: M.CmpRiga) => this.apri(s.key, { ...x });
      azioni.push({ button: bDel }, { button: bEdit });
    }
    if (s.speciale === 'stato') {
      // corso mancante, in scadenza o scaduto: si registra il corso svolto
      const bAdd = this.userInterfaceService.Btn_Aggiungi;
      bAdd.event = (x: M.CmpRiga) => this.apri('formazione', { ...CMP_SEZIONI['formazione'].nuovo!(), idDip_Anagrafica: x['idDip_Anagrafica'], tipoCorso: x['tipoCorso'],
                                                                 descrizione: x['corso'] ?? t('cmp.enums.corso.' + x['tipoCorso']) });
      azioni.push({ button: bAdd, visible: x => x['stato'] !== 1 });
    }
    this.actions = azioni;

    const studioSenzaAzienda = this.area === 'studio' && !this.idCompany;
    this.fabMenuService.fabMenuItem = s.campi.length > 0 && s.nuovo && !studioSenzaAzienda ? [
      new FabMenuItem(t('cmp.pages.list.new') + ': ' + t('cmp.sections.' + s.key), 'add-circle-outline', () => this.apri(s.key, s.nuovo!())),
    ] : [];

    if (studioSenzaAzienda) {
      this.items = [];
      return;
    }
    this.carica(s).subscribe(res => {
      if (this.sezione === s)
        this.items = res.success && res.data ? res.data.items : [];
    });
  }

  private carica(s: CmpSezione): Observable<GenericResult<M.CmpListOutModel<M.CmpRiga>>> {
    const request = new GenericRequest<M.CmpListInModel>(M.CmpListInModel);
    request.data.idCompany = this.idCompany;
    if (s.speciale === 'stato')
      return this.service.FormazioneStato(request);
    if (s.speciale === 'scadenze')
      return this.service.StudioScadenze(request);
    return this.service.List(s.api, s.server, request);
  }

  private apri(sezione: string, item: any) {
    this.navCtrl.navigateForward(this.area === 'studio' ? '/cmpstudioedit' : '/cmpedit', { state: { sezione, item, idCompany: this.idCompany } });
  }

  private async elimina(x: M.CmpRiga) {
    const s = this.sezione;
    if (!s || !await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('cmp.pages.list.confirmDelete')))
      return;
    const request = new GenericRequest<M.CmpDeleteInModel>(M.CmpDeleteInModel);
    request.data.id = x.id;
    request.data.idCompany = this.idCompany;
    this.service.Delete(s.api, s.server, request).subscribe(res => { if (res.success) this.mostra(); });
  }

  private export() {
    if (this.area === 'studio' && !this.idCompany)
      return;
    const request = new GenericRequest<M.CmpListInModel>(M.CmpListInModel);
    request.data.idCompany = this.idCompany;
    this.service.GdprExport(request).subscribe(async res => {
      if (!res.success)
        return;
      const toast = await this.toastController.create({ message: this.transloco.translate('cmp.pages.list.exportStarted'), duration: 3000, position: 'bottom' });
      await toast.present();
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
