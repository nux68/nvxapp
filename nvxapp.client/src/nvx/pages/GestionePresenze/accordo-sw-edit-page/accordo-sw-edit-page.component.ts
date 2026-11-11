import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { TranslocoService } from '@jsverse/transloco';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { SmartWorkingService } from '../../../ClientServer-Service/GestionePresenze/SmartWorking/smart-working.service';
import { Dip_AccordoSWModel, Dip_AccordoSW_GetInModel, Dip_AccordoSW_PutInModel } from '../../../ClientServer-Service/GestionePresenze/SmartWorking/Models/smart-working-model';
import { TurnoDipendenteModel } from '../../../ClientServer-Service/GestionePresenze/Turni/Models/turni-model';

/*
 Accordo individuale di lavoro agile (AT-SW.1): dipendente, periodo, giorni massimi a settimana e giorni consentiti.
 Al salvataggio il dipendente riceve la richiesta di presa visione/firma (verbale di accordo con il Documentale attivo);
 se cambiano le condizioni la richiesta si ripete.
*/
@Component({
  selector: 'app-accordo-sw-edit-page',
  templateUrl: './accordo-sw-edit-page.component.html',
  styleUrls: ['./accordo-sw-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class AccordoSwEditPageComponent extends BasePageConfirmCancelComponent<Dip_AccordoSWModel> {

  public dipendenti: TurnoDipendenteModel[] = [];
  public giorni: boolean[] = [true, true, true, true, true, false, false];
  public readonly giorniSettimana = [0, 1, 2, 3, 4, 5, 6];

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private smartWorkingService: SmartWorkingService,
    private transloco: TranslocoService
  ) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('attendance.pages.accordoSwEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      idAspNetUsers: [null as string | null, [Validators.required]],
      dal: [null as string | null, [Validators.required]],
      al: [null as string | null, [Validators.required]],
      maxGiorniSettimana: [2, [Validators.required, Validators.min(1), Validators.max(7)]],
      note: [null as string | null, [Validators.maxLength(500)]],
    });
  }

  nomeGiorno(i: number): string {
    return this.transloco.translate('attendance.pages.turni.wd' + i);
  }

  LoadData = (): Observable<Dip_AccordoSWModel | null> => {
    const request = new GenericRequest<Dip_AccordoSW_GetInModel>(Dip_AccordoSW_GetInModel);
    request.data.id = history.state?.id ?? 0;
    return this.smartWorkingService.Dip_AccordoSWGet(request).pipe(
      map(res => {
        this.dipendenti = res.data?.dipendenti ?? [];
        const a = res.data?.dip_AccordoSW ?? null;
        if (a)
          this.giorni = (a.giorniConsentiti || '1111100').split('').map(c => c === '1');
        return a;
      }),
      catchError(() => of(null))
    );
  };

  SaveData = (editModel: Dip_AccordoSWModel): Observable<boolean> => {
    const request = new GenericRequest<Dip_AccordoSW_PutInModel>(Dip_AccordoSW_PutInModel);
    request.data.dip_AccordoSW = {
      ...editModel,
      dal: (editModel.dal ?? '').substring(0, 10) + 'T00:00:00',
      al: (editModel.al ?? '').substring(0, 10) + 'T00:00:00',
      maxGiorniSettimana: Number(editModel.maxGiorniSettimana),
      giorniConsentiti: this.giorni.map(g => g ? '1' : '0').join('')
    };
    return this.smartWorkingService.Dip_AccordoSWPut(request).pipe(
      map(res => res.success),
      catchError(() => of(false))
    );
  };

  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.back();
  }
}
