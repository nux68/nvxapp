import { ModelResult } from "../../../ModelsBase/model-result";

// dipendenti presenti e assenti oggi (widget "Presenti oggi", ANALISI_ATTENDANCETRACKING_WIDGET.md)
export class Widget_PresenzeOggiGetInModel { }

export class Widget_PresenzeOggiGetOutModel extends ModelResult {
  public data = '';
  public totale = 0;
  public presenti = 0;
  public assenti = 0;
  public assentiGiustificati = 0;
}

