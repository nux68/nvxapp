import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ApplicationType } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { TicketLevel } from '../../../ClientServer-Service/Infrastructure/SupportTicket/Models/ticket-model';
import { TicketTypeEditModel, TicketTypeGetInModel, TicketTypePutInModel } from '../../../ClientServer-Service/Infrastructure/SupportTicket/Models/ticket-type-model';
import { TicketTypeService } from '../../../ClientServer-Service/Infrastructure/SupportTicket/ticket-type.service';

/*
 Tipo di richiesta dei ticket (SuperUser): applicativo, descrizione (una sola, come gli altri dati), livelli abilitati
 (almeno due: chi apre e chi riceve), risoluzione automatica AI (predisposizione), ordine, attivo.
*/
@Component({
  selector: 'app-ticket-type-edit-page',
  templateUrl: './ticket-type-edit-page.component.html',
  styleUrls: ['./ticket-type-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TicketTypeEditPageComponent extends BasePageConfirmCancelComponent<TicketTypeEditModel> {

  public readonly levels = [TicketLevel.SuperUser, TicketLevel.Dev, TicketLevel.Dealer, TicketLevel.FinancialAdvisor, TicketLevel.Company, TicketLevel.User];
  public readonly applications = Object.values(ApplicationType).filter((v): v is ApplicationType => typeof v === 'number');

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private ticketTypeService: TicketTypeService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('pages.ticketTypeEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required, Validators.maxLength(20)]],
      descrizione: [null, [Validators.required, Validators.maxLength(100)]],
      applicationType: [null],
      level: [[], [this.minLevels]],
      aiResolution: [false],
      ordine: [0],
      active: [true],
    });
  }

  // almeno un livello che apre e uno che riceve
  private minLevels(control: AbstractControl): ValidationErrors | null {
    return control.value && control.value.length >= 2 ? null : { minLevels: true };
  }

  LoadData = (): Observable<TicketTypeEditModel | null> => {
    const state = history.state;

    if (state && state.id) {
      const request = new GenericRequest<TicketTypeGetInModel>(TicketTypeGetInModel);
      request.data.id = state.id;
      return this.ticketTypeService.TicketTypeGet(request).pipe(
        map(res => res.data?.ticketType ?? null),
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return of(null);
        })
      );
    }

    return of(new TicketTypeEditModel());
  };

  SaveData = (editModel: TicketTypeEditModel): Observable<boolean> => {
    const request = new GenericRequest<TicketTypePutInModel>(TicketTypePutInModel);
    request.data.ticketType = { ...editModel, ordine: Number(editModel.ordine) || 0 };

    return this.ticketTypeService.TicketTypePut(request).pipe(
      map(res => res.success),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  // in caso di errore (es. codice gia' usato) si resta sulla pagina
  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.back();
  }
}
