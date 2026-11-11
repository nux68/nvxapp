import { Component, ChangeDetectionStrategy } from '@angular/core';
import { formatDate } from '@angular/common';
import { ToastController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { CalendarFeedDeleteInModel, CalendarFeedGetInModel, CalendarFeedModel, CalendarFeedRegenerateInModel } from '../../../ClientServer-Service/Infrastructure/Calendar/Models/calendar-feed-model';
import { CalendarFeedService } from '../../../ClientServer-Service/Infrastructure/Calendar/calendar-feed.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { LanguageService } from '../../../Utility/infrastructure/language.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

/*
 Calendario personale (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-CAL.3, decisione D9): indirizzo di un feed iCalendar (.ics)
 in sola lettura con gli impegni dell'utente nell'azienda corrente (per ora le scadenze dello scadenzario), da sottoscrivere
 in Google Calendar, Outlook o Apple Calendar.
   - il feed si crea solo su richiesta ("Crea indirizzo"): chi ha l'indirizzo vede gli impegni senza credenziali
   - "Nuovo indirizzo" cambia il token (il vecchio smette di funzionare), "Cancella" elimina il feed
*/
@Component({
  selector: 'app-calendar-feed-page',
  templateUrl: './calendar-feed-page.component.html',
  styleUrls: ['./calendar-feed-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CalendarFeedPageComponent {

  public title: string;
  /** undefined = in caricamento, null = nessun feed */
  public feed: CalendarFeedModel | null | undefined = undefined;

  public buttonbar: ButtonItem[] = [];
  public readonly btnCrea: ButtonItem;
  public readonly btnCopia: ButtonItem;
  private readonly btnRigenera: ButtonItem;
  private readonly btnCancella: ButtonItem;

  constructor(private userInterfaceService: UserInterfaceService,
              private calendarFeedService: CalendarFeedService,
              private collectionDialogService: CollectionDialogService,
              private languageService: LanguageService,
              private toastCtrl: ToastController,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.calendarFeed.title');

    this.btnCrea = userInterfaceService.Btn_Aggiungi;
    this.btnCrea.text = this.transloco.translate('pages.calendarFeed.create');
    this.btnCrea.event = () => this.load(true);

    this.btnCopia = userInterfaceService.Btn_Copia;
    this.btnCopia.event = () => this.copia();

    this.btnRigenera = userInterfaceService.Btn_Rigenera;
    this.btnRigenera.event = () => this.rigenera();

    this.btnCancella = userInterfaceService.Btn_Cancella;
    this.btnCancella.event = () => this.cancella();
  }

  ionViewWillEnter() {
    this.load(false);
  }

  private load(crea: boolean) {
    const request = new GenericRequest<CalendarFeedGetInModel>(CalendarFeedGetInModel);
    request.data.crea = crea;
    this.calendarFeedService.CalendarFeedGet(request).subscribe(res => {
      this.mostra(res.success && res.data ? res.data.feed : null);
    });
  }

  private mostra(feed: CalendarFeedModel | null) {
    this.feed = feed;
    this.buttonbar = feed ? [this.btnRigenera, this.btnCancella] : [];
  }

  private async rigenera() {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('pages.calendarFeed.confirmRegenerate')))
      return;
    this.calendarFeedService.CalendarFeedRegenerate(new GenericRequest<CalendarFeedRegenerateInModel>(CalendarFeedRegenerateInModel)).subscribe(res => {
      if (res.success && res.data)
        this.mostra(res.data.feed);
    });
  }

  private async cancella() {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('pages.calendarFeed.confirmDelete')))
      return;
    this.calendarFeedService.CalendarFeedDelete(new GenericRequest<CalendarFeedDeleteInModel>(CalendarFeedDeleteInModel)).subscribe(res => {
      if (res.success)
        this.mostra(null);
    });
  }

  private async copia() {
    if (!this.feed)
      return;
    let copiato = false;
    try {
      await navigator.clipboard.writeText(this.feed.url);
      copiato = true;
    } catch {
      // appunti non disponibili (es. pagina non sicura): l'utente seleziona il testo a mano
    }
    const toast = await this.toastCtrl.create({
      message: this.transloco.translate(copiato ? 'pages.calendarFeed.copied' : 'pages.calendarFeed.copyFailed'),
      duration: 2500,
      position: 'top',
      color: copiato ? 'success' : 'warning'
    });
    await toast.present();
  }

  // seleziona tutto l'indirizzo al clic, per copiarlo a mano
  seleziona(event: Event) {
    const input = event.target as HTMLInputElement | null;
    input?.select?.();
  }

  formatData(value: string | null): string {
    return value ? formatDate(value, 'short', this.languageService.locale) : this.transloco.translate('pages.calendarFeed.never');
  }
}
