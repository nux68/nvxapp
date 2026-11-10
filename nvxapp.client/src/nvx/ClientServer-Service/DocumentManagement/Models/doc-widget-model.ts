import { ModelResult } from "../../ModelsBase/model-result";

// spazio occupato per tipo documento (widget "Spazio per tipo documento", ANALISI_WIDGET_DOCUMENTALE.md)
export class DocSpazioPerTipoGetInModel { }

export class DocSpazioPerTipoModel {
  public idDoc_Par_Tipo = 0;
  public descrizione = '';
  public bytes = 0;
  public documenti = 0;
}

export class DocSpazioPerTipoGetOutModel extends ModelResult {
  public tipi: DocSpazioPerTipoModel[] = [];
  public cestinoBytes = 0;
  public totaleBytes = 0;
}
