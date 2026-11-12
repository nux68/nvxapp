import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, IonSpinner } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../../../ClientServer-Service/Assets/ast.service';
import { AstEmptyInModel } from '../../../../../ClientServer-Service/Assets/Models/ast-model';
import { AstDaRestituireModel } from '../../../../../ClientServer-Service/Assets/Models/ast-beni-model';
import { AstFormatService } from '../../../../../Utility/Assets/ast-format.service';

/*
 Widget "Da restituire" (Risorse aziendali, ANALISI_ASSETS_WIDGET.md, AST-BENI.3): beni e veicoli ancora assegnati a dipendenti
 cessati (Ast_DaRestituireList, solo amministratori), quanti per tipo e i primi 4 con la data di cessazione.
 Una riga apre il bene o il veicolo (come la pagina dei beni), il collegamento l'elenco dei beni con la sezione "Da restituire".
*/
@Component({
  selector: 'app-ast-to-return-widget',
  templateUrl: './ast-to-return-widget.component.html',
  styleUrls: ['./ast-to-return-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe]
})
export class AstToReturnWidgetComponent {

  private readonly massimo = 4;

  public caricamento = true;
  public errore = false;
  public beni = 0;
  public veicoli = 0;
  public prime: AstDaRestituireModel[] = [];

  constructor(private astService: AstService, private fmt: AstFormatService, private navCtrl: NavController) {
    this.astService.DaRestituireList(new GenericRequest<AstEmptyInModel>(AstEmptyInModel)).subscribe({
      next: res => {
        if (res.success && res.data) {
          const elenco = [...res.data.daRestituire].sort((a, b) => (a.dataCessazione ?? '').localeCompare(b.dataCessazione ?? ''));
          this.beni = elenco.filter(x => x.tipo === 'bene').length;
          this.veicoli = elenco.filter(x => x.tipo === 'veicolo').length;
          this.prime = elenco.slice(0, this.massimo);
        }
        else
          this.errore = true;
        this.caricamento = false;
      },
      error: () => { this.errore = true; this.caricamento = false; }
    });
  }

  public data(x: AstDaRestituireModel): string {
    return this.fmt.data(x.dataCessazione);
  }

  public apriOggetto(x: AstDaRestituireModel): void {
    this.navCtrl.navigateForward(x.tipo === 'veicolo' ? '/astvehicleedit' : '/astgoodedit', { state: { id: x.idOggetto } });
  }

  public apri(): void {
    this.navCtrl.navigateForward('/astgoods');
  }
}
