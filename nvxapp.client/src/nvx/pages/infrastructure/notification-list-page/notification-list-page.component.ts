import { Component, ChangeDetectionStrategy } from '@angular/core';
import { formatDate } from '@angular/common';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Subscription } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { NotificationDeleteInModel, NotificationListInModel, NotificationModel, NotificationReadInModel } from '../../../ClientServer-Service/Infrastructure/Notification/Models/notification-model';
import { NotificationService } from '../../../ClientServer-Service/Infrastructure/Notification/notification.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { LanguageService } from '../../../Utility/infrastructure/language.service';
import { NotificationNotifierService } from '../../../Utility/infrastructure/notification-notifier.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

/** filtro della lista: tutte o solo le non lette */
type NotificationFilter = 'all' | 'unread';

/*
 Notifiche dell'utente collegato (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.4), dalla piu' recente, a pagine di 50.
 "Apri" segna la notifica come letta e, se ha un indirizzo, apre la pagina dell'oggetto (url + stato).
 In fondo: segna tutte come lette, cancella le lette, preferenze (nell'app / per email, per categoria).
 Si ricarica all'arrivo di una nuova notifica (evento SignalR "NotificationNew").
*/
@Component({
  selector: 'app-notification-list-page',
  templateUrl: './notification-list-page.component.html',
  styleUrls: ['./notification-list-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class NotificationListPageComponent {

  public title: string;
  public searchText!: string;
  public notificationList: NotificationModel[] | null = null;
  public filtro: NotificationFilter = 'all';
  public readonly filtri: NotificationFilter[] = ['all', 'unread'];
  public altreDisponibili = false;
  private pagina = 0;

  public readonly columns: NvxListColumn<NotificationModel>[];
  public readonly actions: NvxListAction<NotificationModel>[];
  public readonly highlight = (item: NotificationModel) => !item.letta;
  public readonly iconOf = (item: NotificationModel) => item.letta ? 'mail-open-outline' : 'mail-unread-outline';
  public readonly sortByData = (a: NotificationModel, b: NotificationModel) => (b.data ?? '').localeCompare(a.data ?? '') || b.id - a.id;

  public buttonbar: ButtonItem[] = [];
  public readonly btnCaricaAltri: ButtonItem;

  private changedSub: Subscription | null = null;

  constructor(private navCtrl: NavController,
              private userInterfaceService: UserInterfaceService,
              private notificationService: NotificationService,
              private notificationNotifierService: NotificationNotifierService,
              private collectionDialogService: CollectionDialogService,
              private languageService: LanguageService,
              private transloco: TranslocoService) {

    this.title = this.transloco.translate('pages.notificationList.title');

    this.columns = [
      { key: 'titolo', title: this.transloco.translate('pages.notificationList.columnTitle'), mobile: 'title' },
      { key: 'messaggio', title: this.transloco.translate('pages.notificationList.columnMessage') },
      { key: 'data', title: this.transloco.translate('pages.notificationList.columnDate'), width: '160px', mobile: 'note',
        value: n => n.data ? formatDate(n.data, 'short', this.languageService.locale) : '',
        sortValue: n => n.data ? new Date(n.data) : null },
    ];

    const btnApri = userInterfaceService.Btn_Apri;
    btnApri.event = (item: NotificationModel) => this.apri(item);
    const btnCancella = userInterfaceService.Btn_Cancella;
    btnCancella.event = (item: NotificationModel) => this.cancella([item.id], false);
    this.actions = [{ button: btnApri }, { button: btnCancella }];

    const btnSegnaTutte = userInterfaceService.Btn_SegnaTutteLette;
    btnSegnaTutte.event = () => this.segnaTutteLette();
    const btnCancellaLette = userInterfaceService.Btn_CancellaLette;
    btnCancellaLette.event = () => this.cancellaLette();
    const btnPreferenze = userInterfaceService.Btn_Preferenze;
    btnPreferenze.event = () => this.navCtrl.navigateForward('/notificationpreference');
    this.buttonbar = [btnSegnaTutte, btnCancellaLette, btnPreferenze];

    this.btnCaricaAltri = userInterfaceService.Btn_CaricaAltri;
    this.btnCaricaAltri.event = () => this.caricaAltre();
  }

  ionViewWillEnter() {
    this.load();
    this.changedSub = this.notificationNotifierService.changed$.subscribe(() => this.load());
  }

  ionViewWillLeave() {
    this.changedSub?.unsubscribe();
    this.changedSub = null;
  }

  // prima pagina (all'ingresso, al cambio del filtro, dopo le modifiche)
  load() {
    this.pagina = 0;
    this.carica(false);
  }

  caricaAltre() {
    this.pagina++;
    this.carica(true);
  }

  private carica(aggiungi: boolean) {
    const request = new GenericRequest<NotificationListInModel>(NotificationListInModel);
    request.data.soloNonLette = this.filtro === 'unread';
    request.data.pagina = this.pagina;
    this.notificationService.NotificationList(request).subscribe(res => {
      const righe = res.success && res.data ? res.data.notification : [];
      this.notificationList = aggiungi && this.notificationList ? [...this.notificationList, ...righe] : righe;
      this.altreDisponibili = !!(res.success && res.data && res.data.altreDisponibili);
      if (res.success && res.data)
        this.notificationNotifierService.setNonLette(res.data.nonLette);
    });
  }

  filtroChanged(value: any) {
    this.filtro = (value ?? 'all') as NotificationFilter;
    this.notificationList = null;
    this.load();
  }

  // segna come letta e apre la pagina dell'oggetto, se c'e'
  private apri(item: NotificationModel) {
    if (!item.letta) {
      const request = new GenericRequest<NotificationReadInModel>(NotificationReadInModel);
      request.data.id = [item.id];
      this.notificationService.NotificationRead(request).subscribe(res => {
        if (res.success && res.data) {
          item.letta = true;
          this.notificationNotifierService.setNonLette(res.data.nonLette);
          // nel filtro "non lette" la notifica letta esce dalla lista
          if (this.filtro === 'unread' && this.notificationList)
            this.notificationList = this.notificationList.filter(n => n.id !== item.id);
          else if (this.notificationList)
            this.notificationList = [...this.notificationList];
        }
      });
    }
    this.notificationNotifierService.openUrl(item.url, item.urlState);
  }

  private segnaTutteLette() {
    const request = new GenericRequest<NotificationReadInModel>(NotificationReadInModel);
    request.data.tutte = true;
    this.notificationService.NotificationRead(request).subscribe(res => {
      if (res.success && res.data) {
        this.notificationNotifierService.setNonLette(res.data.nonLette);
        this.load();
      }
    });
  }

  private async cancellaLette() {
    if (!await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('pages.notificationList.confirmDeleteRead')))
      return;
    this.cancella([], true);
  }

  private cancella(id: number[], lette: boolean) {
    const request = new GenericRequest<NotificationDeleteInModel>(NotificationDeleteInModel);
    request.data.id = id;
    request.data.lette = lette;
    this.notificationService.NotificationDelete(request).subscribe(res => {
      if (res.success && res.data) {
        this.notificationNotifierService.setNonLette(res.data.nonLette);
        this.load();
      }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
