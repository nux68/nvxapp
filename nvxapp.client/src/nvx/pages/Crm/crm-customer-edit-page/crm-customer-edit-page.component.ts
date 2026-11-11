import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, ModalController, AlertController, ActionSheetController } from '@ionic/angular';
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
import { CrmContactDialogComponent } from '../../../shared/components/Crm/crm-contact-dialog/crm-contact-dialog.component';

@Component({
  selector: 'app-crm-customer-edit-page',
  templateUrl: './crm-customer-edit-page.component.html',
  styleUrls: ['./crm-customer-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Scheda del cliente nel CRM: anagrafica comune + dati del CRM (salvati con il pulsante Conferma) e, per un cliente gia'
 salvato, contatti, opportunita', attivita', offerte (preventivi), richieste di assistenza, note e allegati (ognuno si salva da se').
*/
export class CrmCustomerEditPageComponent extends BasePageConfirmCancelComponent<M.CrmClienteModel> {

  public sezione = 'dati';
  public scheda: M.CrmClienteGetOutModel | null = null;
  public readonly stati = [0, 1, 2, 3];
  public readonly tipi = [0, 1, 2];
  private id = 0;
  private nuovo = false;
  // pagina visibile: il "+" del FAB si registra solo mentre la pagina e' attiva (il caricamento e' asincrono)
  private attiva = false;
  public readonly dataOra = crmDataOra;
  public readonly data = crmData;

  public readonly colContatti: NvxListColumn<M.CrmContattoModel>[];
  public readonly azContatti: NvxListAction<M.CrmContattoModel>[];
  public readonly colOpportunita: NvxListColumn<M.CrmOpportunitaListModel>[];
  public readonly azOpportunita: NvxListAction<M.CrmOpportunitaListModel>[];
  public readonly colAttivita: NvxListColumn<M.CrmAttivitaListModel>[];
  public readonly azAttivita: NvxListAction<M.CrmAttivitaListModel>[];
  public readonly colOfferte: NvxListColumn<M.CrmOffertaListModel>[];
  public readonly azOfferte: NvxListAction<M.CrmOffertaListModel>[];
  public readonly colRichieste: NvxListColumn<M.CrmRichiestaListModel>[];
  public readonly azRichieste: NvxListAction<M.CrmRichiestaListModel>[];
  public readonly richiestaAperta = (x: M.CrmRichiestaListModel) => x.stato <= 2;
  public readonly principale = (x: M.CrmContattoModel) => x.principale;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private service: CrmService,
              public setupService: CrmSetupService,
              private modalCtrl: ModalController,
              private collectionDialogService: CollectionDialogService,
              private actionSheetCtrl: ActionSheetController,
              public fabMenuService: FabMenuService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    const t = (k: string) => this.transloco.translate(k);
    const valuta = () => this.setupService.setup?.cfg.valuta ?? 'EUR';
    this.colContatti = [
      { key: 'nominativo', title: t('crm.contact.nominativo'), mobile: 'title', value: x => x.nominativo + (x.principale ? ' (' + t('crm.contact.principaleShort') + ')' : '') },
      { key: 'ruolo', title: t('crm.contact.ruolo'), mobile: 'subtitle' },
      { key: 'telefono', title: t('crm.contact.telefono'), value: x => x.cellulare || x.telefono || '' },
      { key: 'email', title: t('crm.contact.email'), mobile: 'note' },
    ];
    const b1 = userInterfaceService.Btn_Modifica; b1.event = (x: M.CrmContattoModel) => this.contatto(x);
    const b2 = userInterfaceService.Btn_Cancella; b2.event = (x: M.CrmContattoModel) => this.eliminaContatto(x);
    this.azContatti = [{ button: b2 }, { button: b1 }];
    this.colOpportunita = [
      { key: 'titolo', title: t('crm.fields.titolo'), mobile: 'title' },
      { key: 'fase', title: t('crm.fields.fase'), mobile: 'subtitle' },
      { key: 'valore', title: t('crm.fields.valore'), align: 'end', mobile: 'note', value: x => crmImporto(x.valore, valuta()), sortValue: x => x.valore },
      { key: 'dataChiusuraPrevista', title: t('crm.fields.chiusuraPrevista'), value: x => crmData(x.dataChiusura ?? x.dataChiusuraPrevista), sortValue: x => x.dataChiusura ?? x.dataChiusuraPrevista ?? '' },
      { key: 'responsabile', title: t('crm.fields.responsabile'), mobile: 'hidden', value: x => x.responsabile ?? '' },
    ];
    const b3 = userInterfaceService.Btn_Apri; b3.event = (x: M.CrmOpportunitaListModel) => this.navCtrl.navigateForward('/crmopportunityedit', { state: { id: x.id } });
    this.azOpportunita = [{ button: b3 }];
    this.colAttivita = [
      { key: 'inizio', title: t('crm.fields.quando'), mobile: 'note', value: x => crmDataOra(x.inizio), sortValue: x => x.inizio },
      { key: 'oggetto', title: t('crm.fields.oggetto'), mobile: 'title', value: x => x.tipo + ': ' + x.oggetto },
      { key: 'stato', title: t('crm.fields.stato'), mobile: 'subtitle', value: x => t('crm.enums.statoAttivita.' + x.stato) },
      { key: 'assegnatario', title: t('crm.fields.assegnatario'), mobile: 'hidden', value: x => x.assegnatario ?? '' },
    ];
    const b4 = userInterfaceService.Btn_Apri; b4.event = (x: M.CrmAttivitaListModel) => this.navCtrl.navigateForward('/crmactivityedit', { state: { id: x.id } });
    this.azAttivita = [{ button: b4 }];
    this.colOfferte = [
      { key: 'numeroCompleto', title: t('crm.fields.numero'), mobile: 'title' },
      { key: 'data', title: t('crm.fields.data'), value: x => crmData(x.data), sortValue: x => x.data },
      { key: 'oggetto', title: t('crm.fields.oggetto'), mobile: 'subtitle', value: x => x.oggetto ?? '' },
      { key: 'stato', title: t('crm.fields.stato'), value: x => t('crm.enums.statoOfferta.' + x.stato) },
      { key: 'totale', title: t('crm.fields.totale'), align: 'end', mobile: 'note', value: x => crmImporto(x.totale, valuta()), sortValue: x => x.totale },
    ];
    const b5 = userInterfaceService.Btn_Apri; b5.event = (x: M.CrmOffertaListModel) => this.navCtrl.navigateForward('/crmofferedit', { state: { id: x.id } });
    this.azOfferte = [{ button: b5 }];
    this.colRichieste = [
      { key: 'numeroCompleto', title: t('crm.fields.numero'), mobile: 'note' },
      { key: 'dataApertura', title: t('crm.fields.apertura'), value: x => crmDataOra(x.dataApertura), sortValue: x => x.dataApertura },
      { key: 'oggetto', title: t('crm.fields.oggetto'), mobile: 'title' },
      { key: 'priorita', title: t('crm.fields.priorita'), value: x => t('crm.enums.prioritaRichiesta.' + x.priorita), sortValue: x => x.priorita },
      { key: 'stato', title: t('crm.fields.stato'), mobile: 'subtitle', value: x => t('crm.enums.statoRichiesta.' + x.stato) },
      { key: 'assegnatario', title: t('crm.fields.assegnatario'), mobile: 'hidden', value: x => x.assegnatario ?? '' },
      { key: 'oreRisoluzione', title: t('crm.fields.oreRisoluzione'), align: 'end', mobile: 'hidden', value: x => x.oreRisoluzione != null ? String(x.oreRisoluzione) : '', sortValue: x => x.oreRisoluzione ?? -1 },
    ];
    const b6 = userInterfaceService.Btn_Apri; b6.event = (x: M.CrmRichiestaListModel) => this.navCtrl.navigateForward('/crmrequestedit', { state: { id: x.id } });
    this.azRichieste = [{ button: b6 }];
  }

  get Title(): string { return this.transloco.translate('crm.pages.customerEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(100)]],
      tipo: [0], attivo: [true], stato: [0], idResponsabile: [null], idCrm_Par_Fonte: [null], tag: [[]],
      settore: [null, [Validators.maxLength(100)]], sito: [null, [Validators.maxLength(200)]],
      fatturatoStimato: [null, [Validators.min(0)]], numeroDipendenti: [null, [Validators.min(0)]], consensoMarketing: [false],
      partitaIva: [null, [Validators.pattern(/^\d{11}$/)]], codiceFiscale: [null, [Validators.pattern(/^([A-Za-z0-9]{16}|\d{11})$/)]],
      indirizzo: [null, [Validators.maxLength(100)]], cap: [null, [Validators.maxLength(10)]], citta: [null, [Validators.maxLength(50)]],
      provincia: [null, [Validators.pattern(/^[A-Za-z]{2}$/)]], nazione: [null, [Validators.pattern(/^[A-Za-z]{2}$/)]],
      telefono: [null, [Validators.maxLength(30)]], email: [null, [Validators.email, Validators.maxLength(100)]], pec: [null, [Validators.email, Validators.maxLength(100)]],
      referente: [null, [Validators.maxLength(100)]], codiceDestinatario: [null, [Validators.pattern(/^[A-Za-z0-9]{6,7}$/)]], note: [null, [Validators.maxLength(500)]],
    });
  }

  LoadData = (): Observable<M.CrmClienteModel | null> => {
    this.id = this.id || history.state?.id || 0;
    return from(this.setupService.load()).pipe(switchMap(setup => {
      if (!this.id) {
        this.scheda = null;
        const nuovo = new M.CrmClienteModel();
        if (setup && !setup.permessi.powerAdmin && !setup.permessi.admin)
          nuovo.idResponsabile = setup.permessi.idUtente;
        return of(nuovo);
      }
      return this.carica();
    }));
  };

  private carica(): Observable<M.CrmClienteModel | null> {
    const request = new GenericRequest<M.CrmIdInModel>(M.CrmIdInModel);
    request.data.id = this.id;
    return this.service.ClienteGet(request).pipe(
      map(res => { this.scheda = res.success ? res.data : null; this.aggiornaFab(); return this.scheda?.cliente ?? null; }),
      catchError(() => of(null)));
  }

  override ionViewWillEnter() {
    this.attiva = true;
    this.aggiornaFab();
    super.ionViewWillEnter();
  }

  ionViewWillLeave() {
    this.attiva = false;
    this.fabMenuService.fabMenuItem = [];
  }

  // "+" nel FAB solo per un cliente gia' salvato (contatti, opportunita', attivita' e offerte richiedono il cliente)
  private aggiornaFab() {
    if (!this.attiva)
      return;
    this.fabMenuService.fabMenuItem = this.scheda
      ? [new FabMenuItem(this.transloco.translate('crm.pages.customerEdit.fabAdd'), 'add-circle-outline', () => { this.scegliInserimento(); })]
      : [];
  }

  /** Pulsante "+": action sheet con il tipo di elemento da aggiungere al cliente. */
  async scegliInserimento() {
    const t = (k: string) => this.transloco.translate(k);
    const sheet = await this.actionSheetCtrl.create({
      header: t('crm.pages.customerEdit.fabAdd'),
      buttons: [
        { text: t('crm.pages.customerEdit.addContatto'), icon: 'person-outline', handler: () => { this.contatto(null); } },
        { text: t('crm.pages.customerEdit.addOpportunita'), icon: 'git-network-outline', handler: () => { this.nuova('/crmopportunityedit'); } },
        { text: t('crm.pages.customerEdit.addAttivita'), icon: 'calendar-outline', handler: () => { this.nuova('/crmactivityedit'); } },
        { text: t('crm.pages.customerEdit.addOfferta'), icon: 'document-text-outline', handler: () => { this.nuova('/crmofferedit'); } },
        { text: t('crm.pages.customerEdit.addRichiesta'), icon: 'help-buoy-outline', handler: () => { this.nuova('/crmrequestedit'); } },
        { text: t('common.buttons.cancel'), role: 'cancel' }
      ]
    });
    await sheet.present();
  }

  ricarica() {
    this.carica().subscribe();
  }

  SaveData = (editModel: M.CrmClienteModel): Observable<boolean> => {
    const request = new GenericRequest<M.CrmClientePutInModel>(M.CrmClientePutInModel);
    request.data.cliente = { ...editModel, id: this.id, fatturatoStimato: editModel.fatturatoStimato != null && editModel.fatturatoStimato !== ('' as any) ? Number(editModel.fatturatoStimato) : null,
                             numeroDipendenti: editModel.numeroDipendenti != null && editModel.numeroDipendenti !== ('' as any) ? Number(editModel.numeroDipendenti) : null };
    return this.service.ClientePut(request).pipe(map(res => {
      if (res.success) {
        this.nuovo = !this.id;
        this.id = res.data.cliente.id;
      }
      return res.success;
    }), catchError(() => of(false)));
  };

  // un cliente nuovo resta aperto (per aggiungere contatti, opportunita'...), uno esistente torna all'elenco
  override AfterSave(saved: boolean): void {
    if (!saved)
      return;
    if (this.nuovo)
      this.ionViewWillEnter();
    else
      this.navCtrl.back();
  }

  async contatto(item: M.CrmContattoModel | null) {
    const contatto = item ? { ...item } : Object.assign(new M.CrmContattoModel(), { idAz_Cliente: this.id });
    const modal = await this.modalCtrl.create({ component: CrmContactDialogComponent, componentProps: { contatto } });
    await modal.present();
    const { role } = await modal.onDidDismiss();
    if (role === 'confirm')
      this.ricarica();
  }

  async eliminaContatto(item: M.CrmContattoModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('crm.contact.confirmDelete')))
      return;
    const request = new GenericRequest<M.CrmIdInModel>(M.CrmIdInModel);
    request.data.id = item.id;
    this.service.ContattoDelete(request).subscribe(res => { if (res.success) this.ricarica(); });
  }

  nuova(pagina: string) {
    this.navCtrl.navigateForward(pagina, { state: { id: 0, preset: { idAz_Cliente: this.id } } });
  }
}
