import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AlertController, NavController, ToastController } from '@ionic/angular';
import { Subscription } from 'rxjs';
import { RequestService } from '../../../ClientServer-Service/Infrastructure/Request/request.service';
import { RequestActionInModel, RequestActionOutModel } from '../../../ClientServer-Service/Infrastructure/Request/Models/request-model';
import { GenericResult } from '../../../ClientServer-Service/ModelsBase/generic-result';
import { RequestNotifierService } from '../../../Utility/infrastructure/request-notifier.service';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../ClientServer-Service/Assets/ast.service';
import * as F from '../../../ClientServer-Service/Assets/Models/ast-flotta-model';
import * as P from '../../../ClientServer-Service/Assets/Models/ast-prenotazioni-model';
import { AstFormatService, astOggi } from '../../../Utility/Assets/ast-format.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

@Component({
  selector: 'app-ast-booking-page',
  templateUrl: './ast-booking-page.component.html',
  styleUrls: ['./ast-booking-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
/*
 Calendario delle prenotazioni (AST-PREN.2), per settimana: postazioni, sale e auto in pool. Ognuno vede quando le risorse
 sono occupate; le proprie prenotazioni (gli amministratori tutte) si modificano o annullano. Nuova prenotazione con il + del fab.
 Le proprie prenotazioni vanno anche nel calendario personale (pulsante Calendario: feed .ics, AST-PREN.3).
*/
export class AstBookingPageComponent {

  public title: string;
  public searchText!: string;
  public dal = AstBookingPageComponent.lunedi(astOggi());
  public idRisorsa: number | null = null;
  public soloMie = false;
  public dati: P.AstPrenotazioneListOutModel | null = null;
  public readonly columns: NvxListColumn<P.AstPrenotazioneModel>[];
  public readonly actions: NvxListAction<P.AstPrenotazioneModel>[];
  public readonly buttonbar: ButtonItem[];
  public readonly mia = (x: P.AstPrenotazioneModel) => x.mia;
  public readonly passata = (x: P.AstPrenotazioneModel) => (x.fine ?? '') < new Date().toISOString().substring(0, 16);
  public readonly sort = (a: P.AstPrenotazioneModel, b: P.AstPrenotazioneModel) => (a.inizio ?? '').localeCompare(b.inizio ?? '');
  public readonly iconOf = (x: P.AstPrenotazioneModel) => x.tipoRisorsa === P.AstTipoRisorsa.Sala ? 'people-outline'
                                                          : x.tipoRisorsa === P.AstTipoRisorsa.AutoPool ? 'car-outline' : 'desktop-outline';

  // prenotazioni da decidere evidenziate, come nella pagina delle richieste
  public readonly highlight = (x: P.AstPrenotazioneModel) => x.daDecidere || this.mia(x);
  // richieste cambiate (SignalR "RequestNotifier"): la pagina si ricarica
  private changedSub: Subscription | null = null;

  constructor(private navCtrl: NavController,
              private toastCtrl: ToastController,
              private alertController: AlertController,
              private requestService: RequestService,
              private requestNotifierService: RequestNotifierService,
              public fabMenuService: FabMenuService,
              userInterfaceService: UserInterfaceService,
              private service: AstService,
              public fmt: AstFormatService,
              private collectionDialogService: CollectionDialogService,
              private transloco: TranslocoService) {
    const t = (k: string) => this.transloco.translate(k);
    this.title = t('ast.pages.booking.title');
    this.columns = [
      { key: 'inizio', title: t('ast.fields.quando'), mobile: 'note', sortValue: x => x.inizio,
        value: x => this.fmt.dataOra(x.inizio) + ' - ' + (x.inizio?.substring(0, 10) === x.fine?.substring(0, 10) ? this.fmt.ora(x.fine) : this.fmt.dataOra(x.fine)) },
      { key: 'risorsa', title: t('ast.fields.risorsa'), mobile: 'title' },
      { key: 'utente', title: t('ast.fields.prenotatoDa'), mobile: 'subtitle', value: x => x.utente ?? t('ast.pages.booking.busy') },
      { key: 'titolo', title: t('ast.fields.titolo'), mobile: 'hidden' },
      // approvazione (risorse con RichiedeApprovazione): in attesa o rifiutata, con il motivo; vuoto se confermata
      { key: 'stato', title: t('ast.fields.statoPrenotazione'), mobile: 'note', value: x => this.stato(x) },
    ];
    const bEdit = userInterfaceService.Btn_Modifica;
    bEdit.event = (x: P.AstPrenotazioneModel) => this.apri(x);
    const bAnnulla = userInterfaceService.Btn_Cancella;
    bAnnulla.event = (x: P.AstPrenotazioneModel) => this.annulla(x);
    // amministratori: le prenotazioni da decidere (richiesta o annullamento in attesa) si approvano o si rifiutano,
    // con lo stesso ciclo della pagina delle richieste; le altre si modificano o si annullano
    const bApprova = userInterfaceService.Btn_Approva;
    bApprova.event = (x: P.AstPrenotazioneModel) => this.approva(x);
    const bRifiuta = userInterfaceService.Btn_Rifiuta;
    bRifiuta.event = (x: P.AstPrenotazioneModel) => this.rifiuta(x);
    // richiedente: ritira la richiesta in attesa
    const bRitira = new ButtonItem(t('pages.requestList.cancel'), 'close-circle-outline', 'danger', false,
                                   (x: P.AstPrenotazioneModel) => this.annulla(x, 'pages.requestList.confirmCancel'));
    // pulsanti decisi dal server per stato e utente (come la pagina di approvazione delle presenze):
    // in attesa: richiedente Annulla, amministratore Approva/Rifiuta; approvata: richiedente Cancella (richiesta di annullamento);
    // annullamento richiesto: amministratore Approva/Rifiuta; senza richiesta: Modifica/Cancella
    this.actions = [
      { button: bApprova, visible: x => x.daDecidere },
      { button: bRifiuta, visible: x => x.daDecidere },
      { button: bRitira, visible: x => x.ritirabile },
      { button: bAnnulla, visible: x => x.modificabile || x.annullamentoChiedibile },
      { button: bEdit, visible: x => x.modificabile },
    ];
    const bCal = userInterfaceService.Btn_Calendario;
    bCal.event = () => this.navCtrl.navigateForward('/calendarfeed');
    this.buttonbar = [bCal];
  }

  get al(): string { return AstBookingPageComponent.aggiungiGiorni(this.dal, 7); }

  get periodo(): string { return this.fmt.data(this.dal) + ' - ' + this.fmt.data(AstBookingPageComponent.aggiungiGiorni(this.dal, 6)); }

  ionViewWillEnter() {
    this.load();
    this.changedSub?.unsubscribe();
    this.changedSub = this.requestNotifierService.changed$.subscribe(() => this.load());
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('ast.pages.booking.new'), 'add-circle-outline', () => this.apri(null)),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
    this.changedSub?.unsubscribe();
    this.changedSub = null;
  }

  private async approva(x: P.AstPrenotazioneModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('pages.requestList.confirmApprove')))
      return;
    this.requestService.RequestApprove(this.azione(x)).subscribe(res => this.dopoAzione(res));
  }

  private async rifiuta(x: P.AstPrenotazioneModel) {
    const alert = await this.alertController.create({
      header: this.transloco.translate('pages.requestList.confirmRejectTitle'),
      message: this.transloco.translate('pages.requestList.confirmReject'),
      inputs: [{ name: 'motivo', type: 'textarea', placeholder: this.transloco.translate('pages.requestList.rejectReason'), attributes: { maxlength: 500 } }],
      buttons: [
        { text: this.transloco.translate('common.buttons.cancel'), role: 'cancel' },
        { text: this.transloco.translate('pages.requestList.reject'), role: 'confirm' },
      ],
    });
    await alert.present();
    const esito = await alert.onDidDismiss();
    if (esito.role !== 'confirm')
      return;
    const request = this.azione(x);
    request.data.motivo = (esito.data?.values?.motivo ?? '').trim() || null;
    this.requestService.RequestReject(request).subscribe(res => this.dopoAzione(res));
  }

  private azione(x: P.AstPrenotazioneModel): GenericRequest<RequestActionInModel> {
    const request = new GenericRequest<RequestActionInModel>(RequestActionInModel);
    request.data.key = String(x.idRichiesta ?? '');
    return request;
  }

  private dopoAzione(res: GenericResult<RequestActionOutModel>) {
    if (res.success && res.data)
      this.requestNotifierService.set(res.data.daApprovare, res.data.inAttesa, res.data.perApplicativo);
    this.load();
  }

  load() {
    const request = new GenericRequest<P.AstPrenotazioneListInModel>(P.AstPrenotazioneListInModel);
    request.data = { dal: this.dal, al: this.al, idAst_Risorsa: this.idRisorsa, soloMie: this.soloMie };
    this.service.PrenotazioneList(request).subscribe(res => this.dati = res.success ? res.data : null);
  }

  settimana(delta: number) {
    this.dal = AstBookingPageComponent.aggiungiGiorni(this.dal, delta * 7);
    this.load();
  }

  oggi() {
    this.dal = AstBookingPageComponent.lunedi(astOggi());
    this.load();
  }

  filtroRisorsa(v: any) {
    this.idRisorsa = v === null || v === undefined || v === '' ? null : Number(v);
    this.load();
  }

  filtroMie(v: boolean) {
    this.soloMie = v;
    this.load();
  }

  private apri(x: P.AstPrenotazioneModel | null) {
    this.navCtrl.navigateForward('/astbookingedit', {
      state: { item: x ? { ...x } : null, risorse: this.dati?.risorsa ?? [], idRisorsa: this.idRisorsa,
               durataMaxOre: this.dati?.durataMaxOre ?? 0, anticipoMaxGiorni: this.dati?.anticipoMaxGiorni ?? 0 }
    });
  }

  public stato(x: P.AstPrenotazioneModel): string {
    if (x.annullamentoRichiesto)
      return this.transloco.translate('ast.pages.booking.cancelRequested');
    if (x.annullamentoRifiutato)
      return this.transloco.translate('ast.pages.booking.cancelRejected');
    if (x.stato === P.AstStatoPrenotazione.InAttesa)
      return this.transloco.translate('ast.enums.statoPrenotazione.2');
    if (x.stato === P.AstStatoPrenotazione.Rifiutata)
      return this.transloco.translate('ast.enums.statoPrenotazione.3') + (x.motivoRifiuto ? ': ' + x.motivoRifiuto : '');
    return '';
  }

  private async annulla(x: P.AstPrenotazioneModel, conferma: string = 'ast.pages.booking.confirmCancel') {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate(conferma, { nome: x.risorsa ?? '' })))
      return;
    const request = new GenericRequest<F.AstIdInModel>(F.AstIdInModel);
    request.data.id = x.id;
    this.service.PrenotazioneAnnulla(request).subscribe(async res => {
      if (!res.success)
        return;
      // prenotazione approvata: l'annullamento e' una richiesta da approvare, la prenotazione resta confermata
      if (res.data?.prenotazione?.annullamentoRichiesto) {
        const toast = await this.toastCtrl.create({ message: this.transloco.translate('ast.pages.booking.cancelRequestSent'), duration: 4000, position: 'top', color: 'warning' });
        await toast.present();
      }
      this.load();
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  // date "yyyy-MM-dd" locali, senza fuso
  static aggiungiGiorni(data: string, giorni: number): string {
    const [y, m, d] = data.substring(0, 10).split('-').map(Number);
    const x = new Date(y, m - 1, d + giorni);
    const p = (n: number) => String(n).padStart(2, '0');
    return `${x.getFullYear()}-${p(x.getMonth() + 1)}-${p(x.getDate())}`;
  }

  static lunedi(data: string): string {
    const [y, m, d] = data.substring(0, 10).split('-').map(Number);
    const giorno = new Date(y, m - 1, d).getDay();
    return AstBookingPageComponent.aggiungiGiorni(data, -((giorno + 6) % 7));
  }
}
