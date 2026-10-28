import { ModelResult } from "../../../ModelsBase/model-result";
import { StatoRichiesta } from "../../Dip_GG_Richiesta/Models/dip-gg-richiesta-model";

// report PDF delle pagine HR (pulsante Report del FAB): stessi valori dell'enum ReportTipo del server
export enum ReportTipo {
  AttivitaTotali = 0,
  AttivitaPerDipendente = 1,
  PianoFerie = 2,
  QuadraturaMensile = 3,
  NotaSpesePerTipo = 4,
  NotaSpesePerDipendente = 5
}

export class ReportModel {
  public tipo: ReportTipo;
  public year: number;
  public month: number;                    // 1-12
  // dipendenti selezionati nella pagina (il server tiene solo quelli che l'utente puo' vedere)
  public selectedUserId: string[] = [];
  // quadratura mensile: reparti selezionati
  public repartiId: number[] = [];
  // note spese: stessi filtri della pagina
  public idPar_TipoSpesa: number | null = null;
  public stati: StatoRichiesta[] | null = null;
}

export class Report_InModel {
  report: ReportModel;
  constructor() {
    this.report = new ReportModel();
  }
}

export class Report_OutModel extends ModelResult {
  public jobId: string;
}
