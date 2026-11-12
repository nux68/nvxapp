import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, IonSpinner } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { forkJoin } from 'rxjs';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../../../ClientServer-Service/Assets/ast.service';
import { AstEmptyInModel } from '../../../../../ClientServer-Service/Assets/Models/ast-model';
import { AstBeneAssegnazioneModel } from '../../../../../ClientServer-Service/Assets/Models/ast-beni-model';
import { AstMioVeicoloModel } from '../../../../../ClientServer-Service/Assets/Models/ast-flotta-model';
import { astOggi } from '../../../../../Utility/Assets/ast-format.service';

/** voce dell'elenco del widget (veicolo o bene in carico) */
export interface AstMineWidgetVoce {
  chiave: string;
  veicolo: boolean;
  testo: string;
}

/*
 Widget "Le mie risorse" (Risorse aziendali, ANALISI_ASSETS_WIDGET.md): beni in carico, veicolo assegnato, verbali di consegna o
 restituzione da confermare e multe notificate non ancora pagate (Ast_MieiBeniGet + Ast_MieiVeicoliGet: solo i dati del
 dipendente collegato all'utente); collegamento a "Le mie risorse", i verbali si confermano nelle prese visione.
*/
@Component({
  selector: 'app-ast-mine-widget',
  templateUrl: './ast-mine-widget.component.html',
  styleUrls: ['./ast-mine-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe]
})
export class AstMineWidgetComponent {

  private readonly massimo = 4;

  public caricamento = true;
  public errore = false;
  public beni: AstBeneAssegnazioneModel[] = [];
  public veicoli: AstMioVeicoloModel[] = [];
  public verbali = 0;
  public multe = 0;
  public elenco: AstMineWidgetVoce[] = [];

  constructor(private astService: AstService, private navCtrl: NavController) {
    const req = () => new GenericRequest<AstEmptyInModel>(AstEmptyInModel);
    forkJoin([this.astService.MieiBeniGet(req()), this.astService.MieiVeicoliGet(req())]).subscribe({
      next: ([b, v]) => {
        if (b.success && b.data && v.success && v.data) {
          const oggi = astOggi();
          const daConfermare = (x: AstBeneAssegnazioneModel) => (x.verbaleConsegna.richiesto && !x.verbaleConsegna.dataConferma)
                                                              || (x.verbaleRestituzione.richiesto && !x.verbaleRestituzione.dataConferma);
          this.beni = b.data.assegnazione.filter(x => !x.dataRestituzione);
          this.verbali = b.data.assegnazione.filter(daConfermare).length;
          // assegnazioni in corso oggi (il servizio restituisce anche quelle chiuse nell'anno, per il fringe benefit)
          this.veicoli = v.data.veicolo.filter(x => (x.dataInizio ?? '').substring(0, 10) <= oggi && (!x.dataFine || x.dataFine.substring(0, 10) >= oggi));
          // 1 = notificata al conducente (non ancora pagata ne' contestata)
          this.multe = v.data.multa.filter(m => m.stato === 1).length;
          this.elenco = [
            ...this.veicoli.map(x => ({ chiave: 'v' + x.id, veicolo: true, testo: x.descrizione })),
            ...this.beni.map(x => ({ chiave: 'b' + x.id, veicolo: false, testo: (x.codiceInventario ? x.codiceInventario + ' - ' : '') + (x.bene ?? '') })),
          ].slice(0, this.massimo);
        }
        else
          this.errore = true;
        this.caricamento = false;
      },
      error: () => { this.errore = true; this.caricamento = false; }
    });
  }

  public apri(): void {
    this.navCtrl.navigateForward('/astmine');
  }

  public prese(): void {
    this.navCtrl.navigateForward('/acknowledgements');
  }
}
