import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, IonSpinner } from '@ionic/angular';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { HrService } from '../../../../../ClientServer-Service/HumanResources/hr.service';
import { HrEmptyInModel, HrMieiDocumentiOutModel, HrMioDocumentoModel, HrTipoDocumento } from '../../../../../ClientServer-Service/HumanResources/Models/hr-model';

/*
 Widget "I miei cedolini" (Risorse Umane, ANALISI_HUMANRESOURCES_WIDGET.md §2): ultimo cedolino pubblicato, documenti di cui
 confermare la presa visione e totale dei documenti; dati dell'area del dipendente (Hr_Cedolino/Hr_MieiDocumenti, solo i propri).
 Se l'utente non e' collegato a un dipendente lo dice e non mostra altro.
*/
@Component({
  selector: 'app-hr-my-payslips-widget',
  templateUrl: './hr-my-payslips-widget.component.html',
  styleUrls: ['./hr-my-payslips-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe]
})
export class HrMyPayslipsWidgetComponent {

  public caricamento = true;
  public dati: HrMieiDocumentiOutModel | null = null;
  /** ultimo cedolino pubblicato (i documenti arrivano dal piu' recente) */
  public ultimo: HrMioDocumentoModel | null = null;
  /** documenti con la presa visione chiesta e non ancora confermata */
  public daConfermare = 0;

  constructor(private hrService: HrService, private transloco: TranslocoService, private navCtrl: NavController) {
    this.hrService.MieiDocumenti(new GenericRequest<HrEmptyInModel>(HrEmptyInModel)).subscribe({
      next: res => {
        this.dati = res.success && res.data ? res.data : null;
        const documenti = this.dati?.documenti ?? [];
        this.ultimo = documenti.find(d => d.tipo === HrTipoDocumento.Cedolino) ?? null;
        this.daConfermare = documenti.filter(d => d.idPresaVisione != null && !d.dataConferma).length;
        this.caricamento = false;
      },
      error: () => this.caricamento = false
    });
  }

  public periodo(d: HrMioDocumentoModel): string {
    return d.tipo === HrTipoDocumento.Cu ? this.transloco.translate('hr.enums.tipoDocumento.2') + ' ' + d.anno
                                         : this.transloco.translate('hr.enums.mese.' + (d.mese ?? 1)) + ' ' + d.anno;
  }

  public apri(): void {
    this.navCtrl.navigateForward('/hrmypayslips');
  }
}
