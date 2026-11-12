import { Component, ChangeDetectionStrategy } from '@angular/core';
import { formatDate } from '@angular/common';
import { NavController, IonSpinner } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService } from '../../../../../ClientServer-Service/HumanResources/hr.service';
import { HrEmptyInModel, HrWidgetOnboardingGetOutModel, HrWidgetVoceModel } from '../../../../../ClientServer-Service/HumanResources/Models/hr-model';
import { LanguageService } from '../../../../../Utility/infrastructure/language.service';

/*
 Widget "Assunzioni e cessazioni" (Risorse Umane, ANALISI_HUMANRESOURCES_WIDGET.md §3): checklist aperte, voci scadute e dei
 prossimi giorni e le prime 5 voci da fare (Hr_Widget/Hr_WidgetOnboardingGet, solo amministratori dell'azienda).
*/
@Component({
  selector: 'app-hr-onboarding-widget',
  templateUrl: './hr-onboarding-widget.component.html',
  styleUrls: ['./hr-onboarding-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe]
})
export class HrOnboardingWidgetComponent {

  public caricamento = true;
  public dati: HrWidgetOnboardingGetOutModel | null = null;
  private readonly oggi = new Date(new Date().setHours(0, 0, 0, 0));

  constructor(private hrService: HrService, private languageService: LanguageService, private navCtrl: NavController) {
    this.hrService.WidgetOnboardingGet(new GenericRequest<HrEmptyInModel>(HrEmptyInModel)).subscribe({
      next: res => {
        this.dati = res.success && res.data ? res.data : null;
        this.caricamento = false;
      },
      error: () => this.caricamento = false
    });
  }

  public data(v: HrWidgetVoceModel): string {
    return formatDate(v.dataPrevista, 'shortDate', this.languageService.locale);
  }

  public scaduta(v: HrWidgetVoceModel): boolean {
    return new Date(v.dataPrevista) < this.oggi;
  }

  public apriVoce(v: HrWidgetVoceModel): void {
    this.navCtrl.navigateForward('/hrchecklistedit', { state: { id: v.idHr_Checklist } });
  }

  public apri(): void {
    this.navCtrl.navigateForward('/hrchecklistlist');
  }
}
