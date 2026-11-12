import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, ToastController } from '@ionic/angular';
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

  constructor(private navCtrl: NavController,
              private toastCtrl: ToastController,
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
    this.actions = [{ button: bAnnulla, visible: x => x.modificabile }, { button: bEdit, visible: x => x.modificabile }];
    const bCal = userInterfaceService.Btn_Calendario;
    bCal.event = () => this.navCtrl.navigateForward('/calendarfeed');
    this.buttonbar = [bCal];
  }

  get al(): string { return AstBookingPageComponent.aggiungiGiorni(this.dal, 7); }

  get periodo(): string { return this.fmt.data(this.dal) + ' - ' + this.fmt.data(AstBookingPageComponent.aggiungiGiorni(this.dal, 6)); }

  ionViewWillEnter() {
    this.load();
    this.fabMenuService.fabMenuItem = [
      new FabMenuItem(this.transloco.translate('ast.pages.booking.new'), 'add-circle-outline', () => this.apri(null)),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
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
    if (x.stato === P.AstStatoPrenotazione.InAttesa)
      return this.transloco.translate('ast.enums.statoPrenotazione.2');
    if (x.stato === P.AstStatoPrenotazione.Rifiutata)
      return this.transloco.translate('ast.enums.statoPrenotazione.3') + (x.motivoRifiuto ? ': ' + x.motivoRifiuto : '');
    return '';
  }

  private async annulla(x: P.AstPrenotazioneModel) {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('ast.pages.booking.confirmCancel', { nome: x.risorsa ?? '' })))
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
