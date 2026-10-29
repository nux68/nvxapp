import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { TipoValore } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';
import { NavController } from '@ionic/angular';
import { Observable, map, of } from 'rxjs';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Dip_GG_Richiesta_Body_Giustificativo, Dip_GG_Richiesta_Send_InModel, StatoRichiesta, TipoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { Par_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { TranslocoService } from '@jsverse/transloco';

/** richiesta di giustificativo in compilazione (date nel formato di ion-datetime) */
export class RequestJustificationModel {
  justificationType: string | null = null;
  startDate: string | null = null;
  endDate: string | null = null;
  fullDay: boolean = true;
  notes: string = '';
  /** giustificativo a importo: euro per ogni giorno lavorativo del periodo */
  importo: number | null = null;
}

@Component({
  selector: 'app-request-justification-user-page',
  templateUrl: './request-justification-user-page.component.html',
  styleUrls: ['./request-justification-user-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class RequestJustificationUserPageComponent extends BasePageConfirmCancelComponent<RequestJustificationModel> {

  // ore e minuti per i giustificativi non a giornata intera (pulsanti +/-)
  public hours: number = 8;
  public minutes: number = 0;
  public hoursFormatted: string = '08:00';

  // Proprietà per accedere ai giustificativi
  public giustificativi: Par_GiustificativiModel[] = [];

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private refresherService: RefresherService,
              private dipGGRichiestaService: DipGGRichiestaService,
              private stringHelperService: StringHelperService,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private transloco: TranslocoService)
  {
    super(navCtrl, userInterfaceService, fb);

    const btnInvia = userInterfaceService.Btn_Invia;
    this.btnConfirm.text = btnInvia.text;
    this.btnConfirm.image = btnInvia.image;
  }

  get Title(): string { return this.transloco.translate('attendance.pages.requestJustificationUser.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      justificationType: [null, [Validators.required]],
      startDate: [null, [Validators.required]],
      endDate: [null, [Validators.required]],
      fullDay: [true],
      notes: [''],
      importo: [null, [Validators.min(0.01), Validators.max(99999999.99)]],
    }, { validators: (g: AbstractControl) => this.importoValidator(g) });
  }

  /** Il giustificativo scelto e' a importo (es. rimborso): si chiede l'importo al posto delle ore. */
  get aImporto(): boolean {
    return this.giustificativi.find(g => g.codice === this.justificationType)?.tipo === TipoValore.Importo;
  }

  private importoValidator(g: AbstractControl): ValidationErrors | null {
    const codice = g.get('justificationType')?.value;
    const aImporto = (this.giustificativi ?? []).find(x => x.codice === codice)?.tipo === TipoValore.Importo;
    return aImporto && !g.get('importo')?.value ? { importoRequired: true } : null;
  }

  get justificationType(): string | null { return this._editForm?.get('justificationType')?.value ?? null; }
  get fullDay(): boolean { return this._editForm?.get('fullDay')?.value === true; }

  LoadData = (): Observable<RequestJustificationModel> => {
    // Caricamento dei giustificativi: il primo e' quello proposto
    this.giustificativi = this.sharedParameterGestionePresenzeService.Par_Giustificativi ?? [];

    const model = new RequestJustificationModel();
    model.justificationType = this.giustificativi.length > 0 ? this.giustificativi[0].codice : null;
    // oggi, nel formato ISO 8601 locale
    model.startDate = this.stringHelperService.DateCurr_To_ISOString();
    model.endDate = model.startDate;

    this.hours = 8; // Impostato a 8 ore per giorno intero
    this.minutes = 0;
    this.updateHoursFormatted();
    return of(model);
  };

  SaveData = (editModel: RequestJustificationModel): Observable<boolean> => {

    let request_rich = new GenericRequest<Dip_GG_Richiesta_Send_InModel>(Dip_GG_Richiesta_Send_InModel);

    const state = history.state;
    if (state && state.currUserId) {
      request_rich.data.idAspNetUsers = state.currUserId;
      if (state.fromHR) {
        request_rich.data.fromHR = state.fromHR;
      }
    }

    let dip_GG_Richiesta_Body_Giustificativo: Dip_GG_Richiesta_Body_Giustificativo = new Dip_GG_Richiesta_Body_Giustificativo();
    dip_GG_Richiesta_Body_Giustificativo.hhmm = this.hoursFormatted;
    dip_GG_Richiesta_Body_Giustificativo.allDay = editModel.fullDay;
    // a importo: solo l'importo (per giorno), niente ore
    if (this.aImporto) {
      dip_GG_Richiesta_Body_Giustificativo.allDay = false;
      dip_GG_Richiesta_Body_Giustificativo.hhmm = '00:00';
      dip_GG_Richiesta_Body_Giustificativo.importo = editModel.importo != null ? +editModel.importo : null;
    }

    // Trova l'ID del giustificativo selezionato
    const giustificativoSelezionato = this.giustificativi.find(g => g.codice === editModel.justificationType);
    dip_GG_Richiesta_Body_Giustificativo.idPar_Giustificativi = giustificativoSelezionato ? giustificativoSelezionato.id : 1;

    request_rich.data.dip_GG_Richiesta.id = 0;
    request_rich.data.dip_GG_Richiesta.idDip_RapportoLavoro = 0;
    request_rich.data.dip_GG_Richiesta.richiestaStato = StatoRichiesta.Immessa;
    request_rich.data.dip_GG_Richiesta.richiestaTipo = TipoRichiesta.Giustificativo;
    request_rich.data.dip_GG_Richiesta.data = this.stringHelperService.Date_To_S_ddmmyyyy(new Date(editModel.startDate));
    request_rich.data.dip_GG_Richiesta.dataA = this.stringHelperService.Date_To_S_ddmmyyyy(new Date(editModel.endDate));
    request_rich.data.dip_GG_Richiesta.dati = this.stringHelperService.toJSONString(dip_GG_Richiesta_Body_Giustificativo);

    return this.dipGGRichiestaService.Send(request_rich).pipe(map(() => {
      this.refresherService.Dip_GG_Richiesta_triggerRefresh();
      return true;
    }));
  };

  // la data di fine non puo' precedere quella di inizio (si confronta il giorno "YYYY-MM-DD")
  updateStartDate(event: any) {
    const start: string = event.detail.value;
    if (start && start.substring(0, 10) > (this._editForm.get('endDate')?.value ?? '').substring(0, 10))
      this._editForm.patchValue({ endDate: start });
  }

  updateEndDate(event: any) {
    const end: string = event.detail.value;
    if (end && end.substring(0, 10) < (this._editForm.get('startDate')?.value ?? '').substring(0, 10))
      this._editForm.patchValue({ startDate: end });
  }

  /** Il giustificativo scelto e' un'autorizzazione alle ore in piu': ore libere, mai giornata intera. */
  get autorizzazione(): boolean {
    return this.giustificativi.find(g => g.codice === this.justificationType)?.autorizzaStraordinario === true;
  }

  /** Ore massime per giorno: 8 per ferie e permessi, 16 per un'autorizzazione alle ore in piu'. */
  get maxOre(): number {
    return this.autorizzazione ? 16 : 8;
  }

  onGiustificativoChange(codice: string) {
    const autorizzazione = this.giustificativi.find(g => g.codice === codice)?.autorizzaStraordinario === true;
    if (autorizzazione && this.fullDay) {
      this._editForm.patchValue({ fullDay: false });
      this.hours = 2;
      this.minutes = 0;
      this.updateHoursFormatted();
    }
  }

  increaseHours() {
    if (!this.fullDay && this.hours < this.maxOre) {
      this.hours++;
      this.updateHoursFormatted();
    }
  }

  decreaseHours() {
    if (!this.fullDay && this.hours > 0) {
      this.hours--;
      this.updateHoursFormatted();
    }
  }

  increaseMinutes() {
    if (!this.fullDay) {
      if (this.minutes === 45) {
        this.minutes = 0;
        if (this.hours < this.maxOre) {
          this.hours++;
        }
      } else {
        this.minutes += 15;
      }
      this.updateHoursFormatted();
    }
  }

  decreaseMinutes() {
    if (!this.fullDay) {
      if (this.minutes === 0) {
        this.minutes = 45;
        if (this.hours > 0) {
          this.hours--;
        }
      } else {
        this.minutes -= 15;
      }
      this.updateHoursFormatted();
    }
  }

  updateHoursFormatted() {
    const h = this.hours || 0;
    const m = this.minutes || 0;
    this.hoursFormatted = `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}`;
  }

  toggleFullDay(fullDay: boolean) {
    if (fullDay) {
      this.hours = 8;
      this.minutes = 0;
    } else {
      this.hours = 1;
      this.minutes = 0;
    }
    this.updateHoursFormatted();
  }
}
