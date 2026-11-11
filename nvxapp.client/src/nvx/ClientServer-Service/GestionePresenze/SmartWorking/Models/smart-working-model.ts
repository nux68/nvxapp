import { ModelResult } from "../../../ModelsBase/model-result";
import { StatoRichiesta } from "../../Dip_GG_Richiesta/Models/dip-gg-richiesta-model";
import { TurnoDipendenteModel } from "../../Turni/Models/turni-model";

// Smart working / lavoro agile (AT-SW, ANALISI_ATTENDANCETRACKING_SMARTWORKING.md)

export class Dip_AccordoSWModel {
  public id: number = 0;
  public idDip_RapportoLavoro: number = 0;
  public idAspNetUsers: string | null = null;
  public nominativo: string | null = null;
  public dal: string = '';
  public al: string = '';
  public maxGiorniSettimana: number = 2;
  public giorniConsentiti: string = '1111100';
  public note: string | null = null;
  public dataRichiestaFirma: string | null = null;
  public dataFirma: string | null = null;
  public firmato: boolean = false;
}

export class Dip_AccordoSW_GetAllInModel { public idAspNetUsers: string | null = null; }
export class Dip_AccordoSW_GetAllOutModel extends ModelResult {
  public dip_AccordoSW: Dip_AccordoSWModel[] = [];
  public dipendenti: TurnoDipendenteModel[] = [];
}
export class Dip_AccordoSW_GetInModel { public id: number = 0; }
export class Dip_AccordoSW_GetOutModel extends ModelResult {
  public dip_AccordoSW: Dip_AccordoSWModel;
  public dipendenti: TurnoDipendenteModel[] = [];
}
export class Dip_AccordoSW_PutInModel { public dip_AccordoSW: Dip_AccordoSWModel; }
export class Dip_AccordoSW_PutOutModel extends ModelResult { public dip_AccordoSW: Dip_AccordoSWModel; }
export class Dip_AccordoSW_DeleteInModel { public id: number = 0; }
export class Dip_AccordoSW_DeleteOutModel extends ModelResult { }

export class LavoroAgileGiornoModel {
  public data: string = '';
  public consentito: boolean = false;
  public lavorativo: boolean = false;
  public stato: StatoRichiesta | null = null;
  public idDip_GG_Richiesta: number | null = null;
}
export class LavoroAgileSettimanaModel {
  public lunedi: string = '';
  public usati: number = 0;
  public max: number = 0;
}
export class Dip_AccordoSW_CalendarioInModel {
  public idAspNetUsers: string | null = null;
  public year: number = 0;
  public month: number = 0;
}
export class Dip_AccordoSW_CalendarioOutModel extends ModelResult {
  public idPar_Giustificativi: number = 0;
  public accordi: Dip_AccordoSWModel[] = [];
  public giorni: LavoroAgileGiornoModel[] = [];
  public settimane: LavoroAgileSettimanaModel[] = [];
}
