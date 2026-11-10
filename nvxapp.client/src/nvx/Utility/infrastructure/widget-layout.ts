import { WidgetDefinition, WidgetSize } from './widget.model';

/*
 Disposizione dei widget nella dashboard (ANALISI_WIDGET.md §5.1), in funzione del numero e delle dimensioni:
   1. nell'ordine scelto dall'utente i widget riempiono righe da 12 colonne (small 4, medium 6, large 12);
   2. una riga con piu' widget che non arriva a 12 colonne si riempie allargandoli in proporzione
      (es. due small -> meta' e meta'; small + medium -> 40% e 60%), cosi' non restano buchi;
   3. un widget da solo nella riga (non large) e' largo almeno meta' pagina ed e' centrato: allargarlo a tutta
      pagina renderebbe dispersivi indicatori e grafici.
 La larghezza esce come frazione della riga (0-1): la usa il CSS, che sotto i 992px e i 768px la ridimensiona.
*/

export interface WidgetCella {
  widget: WidgetDefinition;
  /** frazione della larghezza della riga (0-1) */
  frazione: number;
}

export interface WidgetRiga {
  celle: WidgetCella[];
}

const COLONNE = 12;
const COLONNE_PER_DIMENSIONE: Record<WidgetSize, number> = { small: 4, medium: 6, large: 12 };
/** colonne minime di un widget da solo nella riga */
const MINIMO_DA_SOLO = 6;

export function disponiWidget(widgets: WidgetDefinition[]): WidgetRiga[] {
  // 1. righe da 12 colonne, nell'ordine
  const righe: { widget: WidgetDefinition, colonne: number }[][] = [];
  let corrente: { widget: WidgetDefinition, colonne: number }[] = [];
  let usate = 0;
  for (const w of widgets) {
    const colonne = COLONNE_PER_DIMENSIONE[w.size] ?? COLONNE;
    if (corrente.length > 0 && usate + colonne > COLONNE) {
      righe.push(corrente);
      corrente = [];
      usate = 0;
    }
    corrente.push({ widget: w, colonne });
    usate += colonne;
  }
  if (corrente.length > 0)
    righe.push(corrente);

  // 2-3. larghezze delle righe incomplete
  return righe.map(riga => {
    const totale = riga.reduce((s, c) => s + c.colonne, 0);
    if (riga.length === 1)
      return { celle: [{ widget: riga[0].widget, frazione: Math.max(riga[0].colonne, MINIMO_DA_SOLO) / COLONNE }] };
    return { celle: riga.map(c => ({ widget: c.widget, frazione: c.colonne / totale })) };
  });
}
