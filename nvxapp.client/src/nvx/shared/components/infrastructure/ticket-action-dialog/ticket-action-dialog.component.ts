import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, of } from 'rxjs';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import {
  TicketAction, TicketActionInModel, TicketActionsModel, TicketAttachModel, TicketInfoTargetModel
} from '../../../../ClientServer-Service/Infrastructure/SupportTicket/Models/ticket-model';

// valori del form del dialog
export class TicketActionDialogModel {
  public text: string | null = null;
  public idAspNetUsers: string | null = null;
  public infoTarget: string | null = null;     // 'opener' oppure il livello intermedio
  public infoToLevel: boolean = false;
  public idTicketType: number | null = null;
}

const TEXT_MAX = 4000;

/*
 Dialog di un passaggio sul ticket: testo e allegati, piu' l'assegnatario (Assegnazione),
 il destinatario (RichiestaInfo) o il nuovo tipo (Riclassificazione).
 Alla conferma restituisce (ruolo 'confirm') un TicketActionInModel senza id/versione: la pagina lo invia.
*/
@Component({
  selector: 'app-ticket-action-dialog',
  templateUrl: './ticket-action-dialog.component.html',
  styleUrls: ['./ticket-action-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TicketActionDialogComponent extends BaseDialogConfirmCancelComponent<TicketActionDialogModel> {

  @Input() action: TicketAction = TicketAction.Nota;
  @Input() actions: TicketActionsModel = new TicketActionsModel();

  public readonly TicketAction = TicketAction;
  public attach: TicketAttachModel[] = [];
  public attachRequired = false;

  constructor(protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              protected override modalCtrl: ModalController,
              private transloco: TranslocoService) {
    super(userInterfaceService, fb, modalCtrl);
  }

  override ngOnInit() {
    super.ngOnInit();
    this.btnConfirm.text = this.transloco.translate('ticket.actionButton.' + this.action);
    // la conferma restituisce il passaggio da inviare (con gli allegati, che non sono nel form)
    this.buttonbar[0].event = () => this.handleConfirm();
  }

  get Title(): string { return this.transloco.translate('ticket.actionButton.' + this.action); }

  get EditForm(): FormGroup {
    const textRequired = [TicketAction.RichiestaInfo, TicketAction.RispostaInfo, TicketAction.Risoluzione, TicketAction.Riapertura].includes(this.action);
    const required = (on: boolean): ValidatorFn[] => on ? [Validators.required] : [];

    return this.fb.group({
      text: [null, [...required(textRequired), Validators.maxLength(TEXT_MAX)]],
      idAspNetUsers: [null, required(this.action === TicketAction.Assegnazione)],
      infoTarget: [null, required(this.action === TicketAction.RichiestaInfo)],
      infoToLevel: [false],
      idTicketType: [null, required(this.action === TicketAction.Riclassificazione)],
    });
  }

  LoadData = (): Observable<TicketActionDialogModel> => of(this._editModel ?? new TicketActionDialogModel());

  SaveData = (editModel: TicketActionDialogModel): Observable<TicketActionDialogModel> => of(editModel);

  // destinatario scelto per la richiesta di informazioni
  get selectedTarget(): TicketInfoTargetModel | undefined {
    const value = this._editForm?.get('infoTarget')?.value;
    if (value === 'opener')
      return this.actions.infoTarget.find(t => t.isOpener);
    return this.actions.infoTarget.find(t => !t.isOpener && String(t.level) === String(value));
  }

  targetKey(target: TicketInfoTargetModel): string {
    return target.isOpener ? 'opener' : String(target.level);
  }

  private handleConfirm() {
    Object.keys(this._editForm.controls).forEach((key) => {
      const control = this._editForm.get(key);
      control?.markAsTouched();
      control?.updateValueAndValidity();
    });

    const value = this._editForm.value as TicketActionDialogModel;

    // la nota richiede un testo o almeno un allegato
    this.attachRequired = this.action === TicketAction.Nota && !value.text && this.attach.length === 0;
    if (!this._editForm.valid || this.attachRequired)
      return;

    const result = new TicketActionInModel();
    result.action = this.action;
    result.text = value.text;
    result.ticketAttach = this.attach;

    switch (this.action) {
      case TicketAction.Assegnazione:
        result.idAspNetUsers = value.idAspNetUsers;
        break;
      case TicketAction.RichiestaInfo: {
        const target = this.selectedTarget;
        result.infoToOpener = target?.isOpener ?? false;
        result.infoLevel = target && !target.isOpener ? target.level : null;
        // senza un utente che aveva in carico il ticket, la richiesta va al livello
        result.infoToLevel = !!target && !target.isOpener && (value.infoToLevel || !target.idAspNetUsers);
        break;
      }
      case TicketAction.Riclassificazione:
        result.idTicketType = value.idTicketType;
        break;
    }

    this.modalCtrl.dismiss(result, 'confirm');
  }
}
