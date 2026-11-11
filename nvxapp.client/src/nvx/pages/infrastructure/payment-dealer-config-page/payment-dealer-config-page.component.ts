import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, catchError, map, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { PaymentService } from '../../../ClientServer-Service/Infrastructure/Payment/payment.service';
import {
  PaymentDealerConfigGetInModel, PaymentDealerConfigModel, PaymentDealerConfigPutInModel
} from '../../../ClientServer-Service/Infrastructure/Payment/Models/payment-model';

/*
 Dati di liquidazione di un centro (ANALISI_PAGAMENTI_CENTRI.md §3): trattenuta della piattaforma (solo SuperUser;
 vuota = quella generale), IBAN e intestatario per il bonifico, email dell'amministrazione che riceve rendiconti e riepiloghi
 (vuota = i DealerPowerAdmin del centro). Li modificano il SuperUser e il DealerPowerAdmin del centro.
*/
@Component({
  selector: 'app-payment-dealer-config-page',
  templateUrl: './payment-dealer-config-page.component.html',
  styleUrls: ['./payment-dealer-config-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class PaymentDealerConfigPageComponent extends BasePageConfirmCancelComponent<PaymentDealerConfigModel> {

  public canEditPercentuale = false;
  public dealer = '';
  public percentualeGenerale = 0;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private paymentService: PaymentService,
              private transloco: TranslocoService) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('pages.paymentDealerConfig.title') + (this.dealer ? ' - ' + this.dealer : ''); }

  get EditForm(): FormGroup {
    return this.fb.group({
      percentualePiattaforma: [null, [Validators.min(0), Validators.max(100)]],
      iban: [null, [Validators.maxLength(34), Validators.pattern(/^[A-Za-z0-9 ]*$/)]],
      intestatario: [null, [Validators.maxLength(100)]],
      emailAmministrazione: [null, [Validators.maxLength(256)]],
    });
  }

  LoadData = (): Observable<PaymentDealerConfigModel | null> => {
    const request = new GenericRequest<PaymentDealerConfigGetInModel>(PaymentDealerConfigGetInModel);
    request.data.idDealer = history.state?.idDealer ?? null;
    return this.paymentService.PaymentDealerConfigGet(request).pipe(
      map(res => {
        if (!res.success || !res.data)
          return null;
        this.canEditPercentuale = res.data.canEditPercentuale;
        this.dealer = res.data.config.dealer ?? '';
        this.percentualeGenerale = res.data.config.percentualeGenerale;
        if (!this.canEditPercentuale)
          this._editForm.get('percentualePiattaforma')?.disable();
        return res.data.config;
      }),
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return of(null);
      })
    );
  };

  SaveData = (editModel: PaymentDealerConfigModel): Observable<boolean> => {
    const request = new GenericRequest<PaymentDealerConfigPutInModel>(PaymentDealerConfigPutInModel);
    const perc = editModel.percentualePiattaforma as any;
    request.data.config = {
      ...editModel,
      percentualePiattaforma: perc === null || perc === undefined || perc === '' ? null : Number(perc),
      iban: editModel.iban || null,
      intestatario: editModel.intestatario || null,
      emailAmministrazione: editModel.emailAmministrazione || null
    };
    return this.paymentService.PaymentDealerConfigPut(request).pipe(
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
