import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of, switchMap } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { TicketLevel } from '../../../ClientServer-Service/Infrastructure/SupportTicket/Models/ticket-model';
import {
  TicketTypeAiSettingListInModel, TicketTypeAiSettingModel, TicketTypeAiSettingPutInModel, TicketTypeAiSettingScopeInModel, TicketTypeAiSettingScopeModel
} from '../../../ClientServer-Service/Infrastructure/SupportTicket/Models/ticket-type-ai-setting-model';
import { TicketTypeAiSettingService } from '../../../ClientServer-Service/Infrastructure/SupportTicket/ticket-type-ai-setting.service';

/*
 Risposta automatica AI dei tipi di richiesta, per il livello che riceve il ticket:
   - SuperUser: valori di default dei livelli DEV, centro, studio e azienda
   - PowerAdmin: personalizzazione del proprio nodo ("personalizza"), altrimenti valgono i default; l'Admin legge
 Per ogni tipo abilitato al livello: AI attiva e numero di tentativi prima di passare all'operatore.
 L'AI risponde solo se il livello ha dei documenti (Documentazione AI) per il tipo.
*/
@Component({
  selector: 'app-ticket-type-ai-setting-page',
  templateUrl: './ticket-type-ai-setting-page.component.html',
  styleUrls: ['./ticket-type-ai-setting-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TicketTypeAiSettingPageComponent extends BasePageConfirmCancelComponent<TicketTypeAiSettingPutInModel> {

  public scopes: TicketTypeAiSettingScopeModel[] = [];
  public level: TicketLevel | null = null;
  public maxAttempts = 10;
  public loaded = false;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private ticketTypeAiSettingService: TicketTypeAiSettingService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('pages.ticketTypeAiSetting.title'); }

  // i valori sono nelle righe (ngModel): il form resta vuoto e sempre valido
  get EditForm(): FormGroup {
    return this.fb.group({});
  }

  get scope(): TicketTypeAiSettingScopeModel | null {
    return this.scopes.find(s => s.level === this.level) ?? null;
  }

  get canEdit(): boolean { return this.scope?.canEdit ?? false; }

  get isDefault(): boolean { return this.scope?.isDefault ?? false; }

  LoadData = (): Observable<TicketTypeAiSettingPutInModel | null> => {
    return this.ticketTypeAiSettingService.TicketTypeAiSettingScope(new GenericRequest<TicketTypeAiSettingScopeInModel>(TicketTypeAiSettingScopeInModel)).pipe(
      switchMap(res => {
        this.scopes = res.success && res.data ? res.data.scope : [];
        this.maxAttempts = res.data?.maxAttempts ?? 10;
        this.loaded = true;
        if (this.level === null || !this.scopes.some(s => s.level === this.level))
          this.level = this.scopes.length > 0 ? this.scopes[0].level : null;
        return this.loadLevel();
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(null);
      })
    );
  };

  private loadLevel(): Observable<TicketTypeAiSettingPutInModel | null> {
    this.buttonbar = this.canEdit ? [this.btnConfirm, this.btnCancel] : [this.btnCancel];
    if (this.level === null)
      return of(null);

    const level = this.level;
    const request = new GenericRequest<TicketTypeAiSettingListInModel>(TicketTypeAiSettingListInModel);
    request.data.level = level;
    return this.ticketTypeAiSettingService.TicketTypeAiSettingList(request).pipe(
      map(res => {
        const model = new TicketTypeAiSettingPutInModel();
        model.level = level;
        model.ticketTypeAiSetting = res.data?.ticketTypeAiSetting ?? [];
        return model;
      })
    );
  }

  // cambio di livello: si rileggono i tipi (le modifiche non salvate si perdono)
  changeLevel() {
    this.loadLevel().subscribe(model => this._editModel = model);
  }

  // personalizza / torna ai default: si parte dai valori di default
  toggleCustom(row: TicketTypeAiSettingModel) {
    if (!row.custom) {
      row.aiEnabled = row.defaultAiEnabled;
      row.aiMaxAttempts = row.defaultAiMaxAttempts;
    }
  }

  editable(row: TicketTypeAiSettingModel): boolean {
    return this.canEdit && (this.isDefault || row.custom);
  }

  defaultText(row: TicketTypeAiSettingModel): string {
    return row.defaultAiEnabled
      ? this.transloco.translate('pages.ticketTypeAiSetting.defaultOn', { n: row.defaultAiMaxAttempts })
      : this.transloco.translate('pages.ticketTypeAiSetting.defaultOff');
  }

  SaveData = (editModel: TicketTypeAiSettingPutInModel): Observable<boolean> => {
    const request = new GenericRequest<TicketTypeAiSettingPutInModel>(TicketTypeAiSettingPutInModel);
    request.data.level = editModel.level;
    request.data.ticketTypeAiSetting = editModel.ticketTypeAiSetting.map(r => ({
      ...r,
      aiMaxAttempts: Math.min(this.maxAttempts, Math.max(1, Math.round(Number(r.aiMaxAttempts) || 1)))
    }));

    return this.ticketTypeAiSettingService.TicketTypeAiSettingPut(request).pipe(
      map(res => {
        if (res.success && res.data && this._editModel)
          this._editModel.ticketTypeAiSetting = res.data.ticketTypeAiSetting;
        return res.success;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  // si resta sulla pagina con i valori salvati
  override AfterSave(saved: boolean): void {
  }
}
