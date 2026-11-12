import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, IonSpinner } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../../../ClientServer-Service/Assets/ast.service';
import { AstEmptyInModel } from '../../../../../ClientServer-Service/Assets/Models/ast-model';
import { AstWidgetFlottaOutModel, AstWidgetScadenzaModel } from '../../../../../ClientServer-Service/Assets/Models/ast-widget-model';
import { AstFormatService } from '../../../../../Utility/Assets/ast-format.service';

/*
 Widget "Scadenze della flotta" (Risorse aziendali, ANALISI_ASSETS_WIDGET.md): scadenze dei veicoli (bollo, assicurazione,
 revisione, tagliando...) scadute e in arrivo entro il preavviso del modulo, contratti di noleggio/leasing in scadenza,
 multe da notificare ai conducenti e le prime 6 scadenze (Ast_Widget/Ast_WidgetFlottaGet, solo amministratori).
 Una scadenza apre il veicolo, il resto l'elenco dei veicoli.
*/
@Component({
  selector: 'app-ast-fleet-widget',
  templateUrl: './ast-fleet-widget.component.html',
  styleUrls: ['./ast-fleet-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe]
})
export class AstFleetWidgetComponent {

  public caricamento = true;
  public dati: AstWidgetFlottaOutModel | null = null;

  constructor(private astService: AstService, private fmt: AstFormatService, private navCtrl: NavController) {
    this.astService.WidgetFlottaGet(new GenericRequest<AstEmptyInModel>(AstEmptyInModel)).subscribe({
      next: res => {
        this.dati = res.success && res.data ? res.data : null;
        this.caricamento = false;
      },
      error: () => this.caricamento = false
    });
  }

  public data(s: AstWidgetScadenzaModel): string {
    return this.fmt.data(s.dataScadenza);
  }

  public apriVeicolo(s: AstWidgetScadenzaModel): void {
    this.navCtrl.navigateForward('/astvehicleedit', { state: { id: s.idAst_Veicolo } });
  }

  public apri(): void {
    this.navCtrl.navigateForward('/astvehicles');
  }
}
