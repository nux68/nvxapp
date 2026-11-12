import { Component, ChangeDetectionStrategy } from '@angular/core';
import { formatDate } from '@angular/common';
import { NavController, IonSpinner } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService } from '../../../../../ClientServer-Service/HumanResources/hr.service';
import { HrEmptyInModel, HrWidgetAttestatoModel, HrWidgetFormazioneGetOutModel } from '../../../../../ClientServer-Service/HumanResources/Models/hr-model';
import { LanguageService } from '../../../../../Utility/infrastructure/language.service';

/*
 Widget "Attestati in scadenza" (Risorse Umane, ANALISI_HUMANRESOURCES_WIDGET.md §4): attestati dei corsi scaduti, in scadenza
 entro 30 e 60 giorni e i primi 5 (l'ultimo per dipendente e corso, dipendenti in forza; Hr_Widget/Hr_WidgetFormazioneGet,
 solo amministratori dell'azienda).
*/
@Component({
  selector: 'app-hr-training-expiring-widget',
  templateUrl: './hr-training-expiring-widget.component.html',
  styleUrls: ['./hr-training-expiring-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe]
})
export class HrTrainingExpiringWidgetComponent {

  public caricamento = true;
  public dati: HrWidgetFormazioneGetOutModel | null = null;
  private readonly oggi = new Date(new Date().setHours(0, 0, 0, 0));
  private readonly tra30 = new Date(new Date(this.oggi).setDate(this.oggi.getDate() + 30));

  constructor(private hrService: HrService, private languageService: LanguageService, private navCtrl: NavController) {
    this.hrService.WidgetFormazioneGet(new GenericRequest<HrEmptyInModel>(HrEmptyInModel)).subscribe({
      next: res => {
        this.dati = res.success && res.data ? res.data : null;
        this.caricamento = false;
      },
      error: () => this.caricamento = false
    });
  }

  public data(a: HrWidgetAttestatoModel): string {
    return formatDate(a.dataScadenza, 'shortDate', this.languageService.locale);
  }

  public scaduto(a: HrWidgetAttestatoModel): boolean {
    return new Date(a.dataScadenza) < this.oggi;
  }

  public vicino(a: HrWidgetAttestatoModel): boolean {
    return !this.scaduto(a) && new Date(a.dataScadenza) <= this.tra30;
  }

  public apriCorso(a: HrWidgetAttestatoModel): void {
    this.navCtrl.navigateForward('/hrcourseedit', { state: { id: a.idHr_Corso } });
  }

  public apri(): void {
    this.navCtrl.navigateForward('/hrcourselist');
  }
}
