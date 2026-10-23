import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ModalController, NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Subscription } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import {
  TicketAction, TicketActionInModel, TicketAttachGetInModel, TicketAttachModel, TicketCronologyModel, TicketDetailModel, TicketGetInModel, TicketState
} from '../../../ClientServer-Service/Infrastructure/SupportTicket/Models/ticket-model';
import { TicketService } from '../../../ClientServer-Service/Infrastructure/SupportTicket/ticket.service';
import { TicketActionDialogComponent } from '../../../shared/components/infrastructure/ticket-action-dialog/ticket-action-dialog.component';
import { TicketNotifierService } from '../../../Utility/infrastructure/ticket-notifier.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

/*
 Dettaglio di un ticket: dati, cronologia dei passaggi con testi e allegati, pulsanti delle azioni
 consentite in quel momento (calcolate dal server). Ogni azione apre il dialog per testo e allegati.
 Pagina senza form propria: i dati si inseriscono nel dialog (BaseDialogConfirmCancelComponent).
*/
@Component({
  selector: 'app-ticket-detail-page',
  templateUrl: './ticket-detail-page.component.html',
  styleUrls: ['./ticket-detail-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TicketDetailPageComponent {

  public readonly TicketAction = TicketAction;
  public readonly TicketState = TicketState;
  public ticket: TicketDetailModel | null = null;
  public buttonbar: ButtonItem[];

  private idTicket = 0;
  private changedSub: Subscription | null = null;

  constructor(private navCtrl: NavController,
              private modalCtrl: ModalController,
              private userInterfaceService: UserInterfaceService,
              private ticketService: TicketService,
              private ticketNotifierService: TicketNotifierService,
              private transloco: TranslocoService) {
    // il ritorno all'elenco c'e' anche prima che il ticket sia caricato (es. ticket non visibile)
    this.buttonbar = this.buttons(new TicketDetailModel());
  }

  get title(): string {
    return this.transloco.translate('pages.ticketDetail.title', { id: this.ticket?.id ?? this.idTicket });
  }

  ionViewWillEnter() {
    const state = history.state;
    this.idTicket = state && state.id ? state.id : 0;
    if (!this.idTicket) {
      this.navCtrl.navigateRoot('/ticketlist');
      return;
    }

    this.load();
    // un altro utente ha modificato questo ticket: si ricarica
    this.changedSub = this.ticketNotifierService.changed$.subscribe(data => {
      if (data.idTicket === this.idTicket)
        this.load();
    });
  }

  ionViewWillLeave() {
    this.changedSub?.unsubscribe();
    this.changedSub = null;
  }

  load() {
    const request = new GenericRequest<TicketGetInModel>(TicketGetInModel);
    request.data.id = this.idTicket;
    this.ticketService.TicketGet(request).subscribe(res => {
      if (res.success && res.data)
        this.show(res.data.ticket);
    });
  }

  private show(ticket: TicketDetailModel) {
    this.ticket = ticket;
    this.buttonbar = this.buttons(ticket);
  }

  // pulsanti delle azioni consentite, poi il ritorno all'elenco (sempre presente, non tocca il ticket)
  private buttons(ticket: TicketDetailModel): ButtonItem[] {
    const elenco = this.userInterfaceService.Btn_Elenco;
    elenco.event = () => this.navCtrl.navigateBack('/ticketlist');
    return [...this.actionButtons(ticket), elenco];
  }

  private actionButtons(ticket: TicketDetailModel): ButtonItem[] {
    const a = ticket.actions;
    const list: [boolean, ButtonItem, TicketAction][] = [
      [a.rispondi, this.userInterfaceService.Btn_Rispondi, TicketAction.RispostaInfo],
      [a.risolvi, this.userInterfaceService.Btn_Risolvi, TicketAction.Risoluzione],
      [a.assegna, this.userInterfaceService.Btn_Assegna, TicketAction.Assegnazione],
      [a.inoltra, this.userInterfaceService.Btn_Inoltra, TicketAction.Inoltro],
      [a.richiediInfo, this.userInterfaceService.Btn_RichiediInfo, TicketAction.RichiestaInfo],
      [a.nota, this.userInterfaceService.Btn_Nota, TicketAction.Nota],
      [a.riclassifica, this.userInterfaceService.Btn_Riclassifica, TicketAction.Riclassificazione],
      [a.chiudi, this.userInterfaceService.Btn_ChiudiTicket, TicketAction.Chiusura],
      [a.riapri, this.userInterfaceService.Btn_Riapri, TicketAction.Riapertura],
    ];

    return list.filter(([visible]) => visible).map(([, button, action]) => {
      button.event = () => this.openAction(action);
      return button;
    });
  }

  async openAction(action: TicketAction) {
    if (!this.ticket)
      return;

    const modal = await this.modalCtrl.create({
      component: TicketActionDialogComponent,
      componentProps: {
        action: action,
        actions: this.ticket.actions
      }
    });
    await modal.present();

    const { data, role } = await modal.onDidDismiss();
    if (role !== 'confirm' || !data)
      return;

    const request = new GenericRequest<TicketActionInModel>(TicketActionInModel);
    request.data = data as TicketActionInModel;
    request.data.id = this.ticket.id;
    request.data.version = this.ticket.version;

    this.ticketService.TicketExecute(request).subscribe(res => {
      if (res.success && res.data) {
        this.show(res.data.ticket);
        this.ticketNotifierService.refreshCount();
      }
      else {
        // versione superata o azione non piu' consentita: si ricarica lo stato attuale
        this.load();
      }
    });
  }

  download(item: TicketAttachModel) {
    const request = new GenericRequest<TicketAttachGetInModel>(TicketAttachGetInModel);
    request.data.id = item.id;
    this.ticketService.TicketAttachGet(request).subscribe(res => {
      const attach = res.data?.ticketAttach;
      if (!res.success || !attach || !attach.file)
        return;

      const bytes = Uint8Array.from(atob(attach.file), c => c.charCodeAt(0));
      const url = URL.createObjectURL(new Blob([bytes], { type: attach.contentType || 'application/octet-stream' }));
      const link = document.createElement('a');
      link.href = url;
      link.download = attach.fileName;
      link.click();
      setTimeout(() => URL.revokeObjectURL(url), 1000);
    });
  }

  // destinatario del passaggio: utente o livello
  target(row: TicketCronologyModel): string {
    const level = this.transloco.translate('ticket.level.' + row.level);
    return row.userName ? level + ' - ' + row.userName : level;
  }

  author(row: TicketCronologyModel): string {
    return row.idAspNetUsersAuthor ? (row.authorName ?? '') : this.transloco.translate('pages.ticketDetail.authorAi');
  }

  stateColor(state: TicketState): string {
    switch (state) {
      case TicketState.Aperto: return 'warning';
      case TicketState.InCarico: return 'primary';
      case TicketState.InAttesaInfo: return 'tertiary';
      case TicketState.Risolto: return 'success';
      default: return 'medium';
    }
  }
}
