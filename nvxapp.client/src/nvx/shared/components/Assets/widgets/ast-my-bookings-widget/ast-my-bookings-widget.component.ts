import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, IonSpinner } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { AstService } from '../../../../../ClientServer-Service/Assets/ast.service';
import { AstPrenotazioneListInModel, AstPrenotazioneModel, AstStatoPrenotazione } from '../../../../../ClientServer-Service/Assets/Models/ast-prenotazioni-model';
import { AstFormatService, astDataOraLocale, astOggi } from '../../../../../Utility/Assets/ast-format.service';

/*
 Widget "Le mie prenotazioni" (Risorse aziendali, ANALISI_ASSETS_WIDGET.md): prenotazioni confermate dell'utente da adesso
 ai prossimi 14 giorni (Ast_PrenotazioneList con SoloMie), quante oggi e le prime 4; collegamento alla pagina delle prenotazioni.
*/
@Component({
  selector: 'app-ast-my-bookings-widget',
  templateUrl: './ast-my-bookings-widget.component.html',
  styleUrls: ['./ast-my-bookings-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe]
})
export class AstMyBookingsWidgetComponent {

  public readonly giorni = 14;
  private readonly massimo = 4;

  public caricamento = true;
  public errore = false;
  public oggi = 0;
  public totale = 0;
  public prossime: AstPrenotazioneModel[] = [];

  constructor(private astService: AstService, private fmt: AstFormatService, private navCtrl: NavController) {
    const request = new GenericRequest<AstPrenotazioneListInModel>(AstPrenotazioneListInModel);
    request.data.dal = astDataOraLocale(new Date());
    request.data.al = astOggi(this.giorni + 1);
    request.data.soloMie = true;
    this.astService.PrenotazioneList(request).subscribe({
      next: res => {
        if (res.success && res.data) {
          // confermate e in attesa di approvazione (indicate); le rifiutate restano nella pagina delle prenotazioni
          const mie = res.data.prenotazione.filter(p => p.mia && p.stato !== AstStatoPrenotazione.Rifiutata && p.stato !== AstStatoPrenotazione.Annullata);
          const oggi = astOggi();
          this.oggi = mie.filter(p => p.inizio?.substring(0, 10) === oggi).length;
          this.totale = mie.length;
          this.prossime = mie.slice(0, this.massimo);
        }
        else
          this.errore = true;
        this.caricamento = false;
      },
      error: () => { this.errore = true; this.caricamento = false; }
    });
  }

  /** inizio e fine: la fine con la sola ora se nello stesso giorno */
  public periodo(p: AstPrenotazioneModel): string {
    const stessoGiorno = p.inizio?.substring(0, 10) === p.fine?.substring(0, 10);
    return this.fmt.dataOra(p.inizio) + ' - ' + (stessoGiorno ? this.fmt.ora(p.fine) : this.fmt.dataOra(p.fine));
  }

  public inAttesa(p: AstPrenotazioneModel): boolean {
    return p.stato === AstStatoPrenotazione.InAttesa;
  }

  public apri(): void {
    this.navCtrl.navigateForward('/astbookings');
  }
}
