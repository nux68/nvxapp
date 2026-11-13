import { ModelResult } from "../../../ModelsBase/model-result";

// Turni e pianificazione (AT-TURNI, ANALISI_ATTENDANCETRACKING_TURNI.md)

export enum TurnoScambioStato {
  InAttesaCollega = 0,
  RifiutatoCollega = 1,
  InApprovazione = 2,
  Approvato = 3,
  Rifiutato = 4,
  Annullato = 5
}

export class Par_TurnoModel {
  public id: number = 0;
  public idAz_Anagrafica: number = 0;
  public codice: string = '';
  public descrizione: string = '';
  public idPar_Orario: number | null = null;   // null = riposo
  public colore: string | null = null;
  public orarioTesto?: string | null;
  public minutiTeorici?: number;
}

export class Az_SediRepartoTurnoModel {
  public id: number = 0;
  public idAz_SediReparto: number = 0;
  public idPar_Turno: number = 0;
  public minPersone: number = 0;
  public giorniSettimana: string = '1111111';
}

export class Dip_TurnoModel {
  public id: number = 0;
  public idDip_RapportoLavoro: number = 0;
  public data: string = '';
  public idPar_Turno: number = 0;
  public idAz_SediReparto: number | null = null;
  public note: string | null = null;
}

export class Par_Turno_GetAllInModel { }
export class Par_Turno_GetAllOutModel extends ModelResult { public par_Turno: Par_TurnoModel[] = []; }
export class Par_Turno_GetInModel { public id: number = 0; }
export class Par_Turno_GetOutModel extends ModelResult { public par_Turno: Par_TurnoModel; }
export class Par_Turno_PutInModel { public par_Turno: Par_TurnoModel; }
export class Par_Turno_PutOutModel extends ModelResult { public par_Turno: Par_TurnoModel; }
export class Par_Turno_DeleteInModel { public id: number = 0; }
export class Par_Turno_DeleteOutModel extends ModelResult { }

export class Az_SediRepartoTurno_GetInModel { public idAz_SediReparto: number = 0; }
export class Az_SediRepartoTurno_GetOutModel extends ModelResult {
  public idAz_SediReparto: number = 0;
  public az_SediRepartoTurno: Az_SediRepartoTurnoModel[] = [];
}
export class Az_SediRepartoTurno_PutInModel {
  public idAz_SediReparto: number = 0;
  public az_SediRepartoTurno: Az_SediRepartoTurnoModel[] = [];
}
export class Az_SediRepartoTurno_PutOutModel extends ModelResult { public az_SediRepartoTurno: Az_SediRepartoTurnoModel[] = []; }

export class TurnoDipendenteModel {
  public idAspNetUsers: string = '';
  public idDip_Anagrafica: number = 0;
  public idDip_RapportoLavoro: number = 0;
  public nominativo: string = '';
}

export class TurnoAssenzaModel {
  public idDip_RapportoLavoro: number = 0;
  public data: string = '';
  public codice: string | null = null;
  public descrizione: string | null = null;
  public backgroundColor: string | null = null;
  public textColor: string | null = null;
  public approvata: boolean = false;
  public giornataIntera: boolean = false;
}

export class TurnoCoperturaGiornoModel {
  public data: string = '';
  public idPar_Turno: number = 0;
  public minPersone: number = 0;
  public assegnati: number = 0;
  public insufficiente: boolean = false;
}

export class TurnoConflittoModel {
  public idDip_RapportoLavoro: number = 0;
  public data: string = '';
  public messaggio: string = '';
  public grave: boolean = false;
}

export class TurnoConsuntivoModel {
  public idDip_RapportoLavoro: number = 0;
  public data: string = '';
  public min_Teo: number = 0;
  public min_Lav: number = 0;
  public stato: number = 0;
  public anomalia: boolean = false;
}

export class Dip_Turno_PianificazioneInModel {
  public idAz_SediReparto: number = 0;
  public dal: string = '';
  public al: string = '';
}
export class Dip_Turno_PianificazioneOutModel extends ModelResult {
  public idAz_SediReparto: number = 0;
  public reparto: string | null = null;
  public dal: string = '';
  public al: string = '';
  public dipendenti: TurnoDipendenteModel[] = [];
  public turni: Par_TurnoModel[] = [];
  public turniReparto: Az_SediRepartoTurnoModel[] = [];
  public assegnazioni: Dip_TurnoModel[] = [];
  public assenze: TurnoAssenzaModel[] = [];
  public coperture: TurnoCoperturaGiornoModel[] = [];
  public conflitti: TurnoConflittoModel[] = [];
  public consuntivo: TurnoConsuntivoModel[] = [];
  public copertureInsufficienti: number = 0;
}

export class Dip_Turno_PutRiga {
  /** turni del giorno (piu' turni compatibili): sostituiscono quelli presenti; vuoto = nessun turno. Se null vale idPar_Turno */
  public idPar_Turni: number[] | null = null;
  public idDip_RapportoLavoro: number = 0;
  public data: string = '';
  public idPar_Turno: number = 0;   // 0 = togli il turno
  public note: string | null = null;
}
export class Dip_Turno_PutInModel {
  public idAz_SediReparto: number = 0;
  public assegnazioni: Dip_Turno_PutRiga[] = [];
}
export class Dip_Turno_PutOutModel extends ModelResult {
  public salvati: number = 0;
  public tolti: number = 0;
}

export class Dip_TurnoUserModel extends Dip_TurnoModel {
  public codice: string | null = null;
  public descrizione: string | null = null;
  public colore: string | null = null;
  public orarioTesto: string | null = null;
  public reparto: string | null = null;
  public idAspNetUsers: string | null = null;
  public nominativo: string | null = null;
}

export class Dip_TurnoScambioModel {
  public id: number = 0;
  public stato: TurnoScambioStato = TurnoScambioStato.InAttesaCollega;
  public idDip_Turno: number = 0;
  public idDip_Turno_Collega: number | null = null;
  public richiedente: string | null = null;
  public collega: string | null = null;
  public testo: string | null = null;
  public note: string | null = null;
  public idDip_GG_Richiesta: number | null = null;
  public daRispondere: boolean = false;
  public annullabile: boolean = false;
}

/** report PDF dei turni (job): pianificazione del reparto (amministratori) o i miei turni */
export enum TurniReportTipo { Reparto = 0, Miei = 1 }

export class Dip_Turno_ReportInModel {
  public tipo: TurniReportTipo = TurniReportTipo.Miei;
  /** solo per il report del reparto */
  public idAz_SediReparto: number = 0;
  public dal: string = '';
  public al: string = '';
}
export class Dip_Turno_ReportOutModel extends ModelResult {
  public jobId: string = '';
}

export class Dip_Turno_GetAll4UserInModel {
  public dal: string = '';
  public al: string = '';
}
export class Dip_Turno_GetAll4UserOutModel extends ModelResult {
  public mieiTurni: Dip_TurnoUserModel[] = [];
  public turniColleghi: Dip_TurnoUserModel[] = [];
  public colleghi: TurnoDipendenteModel[] = [];
  public scambi: Dip_TurnoScambioModel[] = [];
}

export class Dip_TurnoScambio_ProponiInModel {
  public idDip_Turno: number = 0;
  public idDip_Turno_Collega: number | null = null;
  public idAspNetUsers_Collega: string | null = null;
  public note: string | null = null;
}
export class Dip_TurnoScambio_ProponiOutModel extends ModelResult { public dip_TurnoScambio: Dip_TurnoScambioModel | null = null; }
export class Dip_TurnoScambio_RispondiInModel { public id: number = 0; public accetta: boolean = false; }
export class Dip_TurnoScambio_RispondiOutModel extends ModelResult { public dip_TurnoScambio: Dip_TurnoScambioModel | null = null; }
export class Dip_TurnoScambio_AnnullaInModel { public id: number = 0; }
export class Dip_TurnoScambio_AnnullaOutModel extends ModelResult { }

/** data locale "YYYY-MM-DDT00:00:00" (senza fuso orario) */
export function dataLocale(d: Date): string {
  const p = (n: number) => n.toString().padStart(2, '0');
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T00:00:00`;
}
