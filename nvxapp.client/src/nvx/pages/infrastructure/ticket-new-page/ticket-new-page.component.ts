import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { TicketNotifierService } from '../../../Utility/infrastructure/ticket-notifier.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import {
  TicketAttachModel, TicketLevel, TicketOpenerRoleModel, TicketOpenInfoInModel, TicketOpenInModel, TicketOpenModel, TicketTypeOptionModel, TicketUserModel
} from '../../../ClientServer-Service/Infrastructure/SupportTicket/Models/ticket-model';
import { TicketService } from '../../../ClientServer-Service/Infrastructure/SupportTicket/ticket.service';

/*
 Apertura di un ticket.
   - Ruolo di apertura: solo se l'utente ne ha piu' di uno (es. dipendente e amministratore aziendale);
     decide il destinatario. Proposto il piu' alto.
   - Tipi di richiesta: quelli disponibili per il ruolo scelto.
   - SuperUser: al posto del destinatario automatico si sceglie l'assegnatario (PowerAdmin o Admin).
 Dopo l'invio si apre il dettaglio del ticket.
*/
@Component({
  selector: 'app-ticket-new-page',
  templateUrl: './ticket-new-page.component.html',
  styleUrls: ['./ticket-new-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TicketNewPageComponent extends BasePageConfirmCancelComponent<TicketOpenModel> {

  public readonly TicketLevel = TicketLevel;
  public openerRole: TicketOpenerRoleModel[] = [];
  public assegnatario: TicketUserModel[] = [];
  public attach: TicketAttachModel[] = [];

  private idTicket = 0;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private ticketService: TicketService,
              private ticketNotifierService: TicketNotifierService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
    this.btnConfirm.text = this.transloco.translate('pages.ticketNew.send');
  }

  get Title(): string { return this.transloco.translate('pages.ticketNew.title'); }

  get EditForm(): FormGroup {
    const form = this.fb.group({
      openerLevel: [null as TicketLevel | null, [Validators.required]],
      idTicketType: [null as number | null, [Validators.required]],
      idAspNetUsersAssignee: [null as string | null],
      oggetto: [null as string | null, [Validators.required, Validators.maxLength(200)]],
      text: [null as string | null, [Validators.required, Validators.maxLength(4000)]],
    });

    // cambiando ruolo cambiano i tipi disponibili e, per il SuperUser, serve l'assegnatario
    form.get('openerLevel')!.valueChanges.subscribe(level => {
      const types = this.typesOf(level);
      const idType = form.get('idTicketType')!.value;
      if (idType !== null && !types.some(t => t.id === idType))
        form.get('idTicketType')!.setValue(null);

      const assignee = form.get('idAspNetUsersAssignee')!;
      assignee.setValidators(level === TicketLevel.SuperUser ? [Validators.required] : []);
      assignee.updateValueAndValidity();
    });

    return form;
  }

  LoadData = (): Observable<TicketOpenModel | null> => {
    return this.ticketService.TicketOpenInfo(new GenericRequest<TicketOpenInfoInModel>(TicketOpenInfoInModel)).pipe(
      map(res => {
        this.openerRole = res.data?.openerRole ?? [];
        this.assegnatario = res.data?.assegnatario ?? [];

        const model = new TicketOpenModel();
        // ruolo proposto: il piu' alto (valore piu' basso)
        model.openerLevel = this.openerRole.length > 0 ? Math.min(...this.openerRole.map(r => r.level)) : null;
        return model;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(null);
      })
    );
  };

  SaveData = (editModel: TicketOpenModel): Observable<boolean> => {
    const request = new GenericRequest<TicketOpenInModel>(TicketOpenInModel);
    request.data.ticket = { ...editModel, ticketAttach: this.attach };

    return this.ticketService.TicketOpen(request).pipe(
      map(res => {
        this.idTicket = res.success && res.data ? res.data.id : 0;
        return this.idTicket > 0;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  // inviato il ticket si apre il dettaglio; in caso di errore si resta sulla pagina
  override AfterSave(saved: boolean): void {
    if (saved) {
      this.ticketNotifierService.refreshCount();
      this.navCtrl.navigateForward('/ticketdetail', { state: { id: this.idTicket }, replaceUrl: true });
    }
  }

  get currentLevel(): TicketLevel | null {
    return this._editForm?.get('openerLevel')?.value ?? null;
  }

  get currentTypes(): TicketTypeOptionModel[] {
    return this.typesOf(this.currentLevel);
  }

  private typesOf(level: TicketLevel | null): TicketTypeOptionModel[] {
    return this.openerRole.find(r => r.level === level)?.ticketType ?? [];
  }
}
