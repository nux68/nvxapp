import { ModelResult } from '../../ModelsBase/model-result';

// Widget della dashboard di Compliance (ANALISI_COMPLIANCE_WIDGET.md)

/** scadenze aperte di un tipo: fonte senza il prefisso "cmp." (formazione, visita, nomina, documento, responsabile, violazione) */
export class CmpWidgetScadenzaAreaModel {
  public fonte = '';
  public scadute = 0;
  public inScadenza = 0;
}

export class CmpWidgetScadenzeGetOutModel extends ModelResult {
  public aree: CmpWidgetScadenzaAreaModel[] = [];
  public scadute = 0;
  public inScadenza = 0;
  public giorniPreavviso = 30;
}

export class CmpWidgetGdprGetOutModel extends ModelResult {
  public trattamentiAttivi = 0;
  public trattamentiDatiParticolari = 0;
  public responsabiliScaduti = 0;
  public responsabiliInScadenza = 0;
  public violazioniDaNotificare = 0;
  public violazioniOltreTermine = 0;
  public violazioni12Mesi = 0;
}
