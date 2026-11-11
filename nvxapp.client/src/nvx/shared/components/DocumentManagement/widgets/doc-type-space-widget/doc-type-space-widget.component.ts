import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController, IonSpinner } from '@ionic/angular';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../../../ClientServer-Service/ModelsBase/generic-request';
import { DocWidgetService } from '../../../../../ClientServer-Service/DocumentManagement/doc-widget.service';
import { DocSpazioPerTipoGetInModel } from '../../../../../ClientServer-Service/DocumentManagement/Models/doc-widget-model';
import { formatBytes } from '../doc-widget-format';

/** fetta del grafico */
interface Fetta {
  etichetta: string;
  bytes: number;
  percentuale: number;
  colore: string;
  /** inizio della fetta sulla circonferenza (0-100) */
  inizio: number;
}

// colori delle fette: quelli del tema, cosi' vanno anche nel tema scuro; il cestino e "altri" in grigio.
// Ordine scelto perche' fette vicine abbiano toni lontani (primary e secondary sono entrambi blu).
const COLORI = ['--ion-color-primary', '--ion-color-warning', '--ion-color-success', '--ion-color-tertiary',
                '--ion-color-danger', '--ion-color-secondary'];
const MAX_FETTE = COLORI.length;

/*
 Widget "Spazio per tipo documento" (Documentale, ANALISI_DOCUMENTMANAGEMENT_WIDGET.md): grafico a torta (ciambella SVG)
 dello spazio occupato per tipologia; i tipi oltre il sesto confluiscono in "Altri", il cestino e' una fetta a parte.
 Dati da Doc_Widget/Doc_SpazioPerTipoGet (solo per chi amministra l'archivio).
*/
@Component({
  selector: 'app-doc-type-space-widget',
  templateUrl: './doc-type-space-widget.component.html',
  styleUrls: ['./doc-type-space-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonSpinner, TranslocoPipe]
})
export class DocTypeSpaceWidgetComponent {

  public caricamento = true;
  public errore = false;
  public fette: Fetta[] = [];
  public totale = 0;
  public readonly bytes = formatBytes;

  constructor(private docWidgetService: DocWidgetService,
              private transloco: TranslocoService,
              private navCtrl: NavController) {
    this.docWidgetService.SpazioPerTipoGet(new GenericRequest<DocSpazioPerTipoGetInModel>(DocSpazioPerTipoGetInModel)).subscribe({
      next: res => {
        if (res.success && res.data)
          this.costruisci(res.data.tipi.map(t => ({ etichetta: t.descrizione, bytes: t.bytes })), res.data.cestinoBytes);
        else
          this.errore = true;
        this.caricamento = false;
      },
      error: () => { this.errore = true; this.caricamento = false; }
    });
  }

  private costruisci(tipi: { etichetta: string, bytes: number }[], cestino: number): void {
    const ordinati = [...tipi].sort((a, b) => b.bytes - a.bytes);
    const voci: { etichetta: string, bytes: number, colore: string }[] = [];
    const visibili = ordinati.length > MAX_FETTE ? ordinati.slice(0, MAX_FETTE - 1) : ordinati;
    visibili.forEach((t, i) => voci.push({ ...t, colore: `var(${COLORI[i]})` }));
    if (ordinati.length > MAX_FETTE)
      voci.push({ etichetta: this.transloco.translate('documents.widgets.typeSpace.others'),
                  bytes: ordinati.slice(MAX_FETTE - 1).reduce((s, t) => s + t.bytes, 0), colore: 'var(--ion-color-medium)' });
    if (cestino > 0)
      voci.push({ etichetta: this.transloco.translate('documents.widgets.typeSpace.trash'), bytes: cestino, colore: 'var(--ion-color-medium-tint)' });

    this.totale = voci.reduce((s, v) => s + v.bytes, 0);
    let inizio = 0;
    this.fette = voci.map(v => {
      const percentuale = this.totale > 0 ? v.bytes * 100 / this.totale : 0;
      const f: Fetta = { ...v, percentuale, inizio };
      inizio += percentuale;
      return f;
    });
  }

  /** stroke-dasharray della fetta su una circonferenza lunga 100 */
  public dash(f: Fetta): string {
    return `${f.percentuale} ${100 - f.percentuale}`;
  }

  /** stroke-dashoffset: le fette partono da ore 12 e girano in senso orario */
  public offset(f: Fetta): number {
    return 25 - f.inizio;
  }

  public pct(f: Fetta): string {
    return f.percentuale.toLocaleString(undefined, { maximumFractionDigits: f.percentuale < 10 ? 1 : 0 }) + '%';
  }

  public dettagli(): void {
    this.navCtrl.navigateForward('/doclist');
  }
}
