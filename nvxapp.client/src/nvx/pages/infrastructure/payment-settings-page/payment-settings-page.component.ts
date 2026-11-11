import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { MonthNavigatorService } from '../../../Utility/infrastructure/month-navigator.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { PaymentService } from '../../../ClientServer-Service/Infrastructure/Payment/payment.service';
import {
  PaymentSettingsGetInModel, PaymentSettingsModel, PaymentSettingsPutInModel
} from '../../../ClientServer-Service/Infrastructure/Payment/Models/payment-model';

/*
 Impostazioni dei pagamenti ai centri (SuperUser, ANALISI_INFRASTRUCTURE_PAGAMENTI_CENTRI.md §3, §5):
    - trattenuta generale della piattaforma, per i centri senza una propria
    - invio automatico: dal giorno indicato le bozze dei rendiconti del mese precedente, nel mese indicato
      il riepilogo annuale dell'anno precedente
*/
@Component({
  selector: 'app-payment-settings-page',
  templateUrl: './payment-settings-page.component.html',
  styleUrls: ['./payment-settings-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class PaymentSettingsPageComponent extends BasePageConfirmCancelComponent<PaymentSettingsModel> {

  public emailConfigurata = true;
  public debugPagamenti = false;
  public readonly giorni = Array.from({ length: 28 }, (_, i) => i + 1);

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              public monthNavigator: MonthNavigatorService,
              private paymentService: PaymentService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('pages.paymentSettings.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      percentualePiattaforma: [0, [Validators.required, Validators.min(0), Validators.max(100)]],
      riepilogoAutomatico: [false],
      giornoInvioMensile: [5, [Validators.required, Validators.min(1), Validators.max(28)]],
      meseInvioAnnuale: [1, [Validators.required, Validators.min(1), Validators.max(12)]],
    });
  }

  LoadData = (): Observable<PaymentSettingsModel | null> =>
    this.paymentService.PaymentSettingsGet(new GenericRequest<PaymentSettingsGetInModel>(PaymentSettingsGetInModel)).pipe(
      map(res => {
        if (!res.success || !res.data)
          return null;
        this.emailConfigurata = res.data.emailConfigurata;
        this.debugPagamenti = res.data.debugPagamenti;
        return res.data.settings;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(null);
      })
    );

  SaveData = (editModel: PaymentSettingsModel): Observable<boolean> => {
    const request = new GenericRequest<PaymentSettingsPutInModel>(PaymentSettingsPutInModel);
    request.data.settings = {
      percentualePiattaforma: Number(editModel.percentualePiattaforma) || 0,
      riepilogoAutomatico: !!editModel.riepilogoAutomatico,
      giornoInvioMensile: Number(editModel.giornoInvioMensile) || 5,
      meseInvioAnnuale: Number(editModel.meseInvioAnnuale) || 1
    };
    return this.paymentService.PaymentSettingsPut(request).pipe(
      map(res => res.success),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(false);
      })
    );
  };

  override AfterSave(saved: boolean): void {
    if (saved)
      this.navCtrl.back();
  }
}
