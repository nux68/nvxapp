import { ModelResult } from '../../ModelsBase/model-result';

// ---------------------------------------------------------------- widget della dashboard (ANALISI_ASSETS_WIDGET.md)
export class AstWidgetScadenzaModel {
  public idAst_Veicolo = 0;
  public targa = '';
  public veicolo = '';
  public descrizione = '';
  public dataScadenza: string | null = null;
  public scaduta = false;
  public contratto = false;
}

export class AstWidgetFlottaOutModel extends ModelResult {
  public giorniPreavviso = 30;
  public veicoliAttivi = 0;
  public scadute = 0;
  public inArrivo = 0;
  public multeDaNotificare = 0;
  public scadenze: AstWidgetScadenzaModel[] = [];
}
