import { ModelResult } from "../../../ModelsBase/model-result";
import { DestinazioneEccedenze, TipoContratto } from "../../Dip_RapportoLavoro/Models/dip-rapporto-lavoro-model";
import { StraoTipoConteggio } from "../../Par_ProfiloOrario/Models/par-profilo-orario-model";

export enum PeriodoStato {
  Provvisorio,   // mese (o la sua ultima settimana) in corso
  DaApprovare,   // periodo concluso, in attesa della validazione HR
  Approvato      // consolidato: giorni bloccati, pronto per le paghe
}

// cartellino di quadratura mensile, totali in minuti
export class Dip_PeriodoModel {
  public id: number;
  public idDip_RapportoLavoro: number;
  public anno: number;
  public mese: number;
  public stato: PeriodoStato;

  public min_Teo: number;
  public min_Lav: number;
  public min_Ord: number;
  public min_Assenze: number;
  public min_Ecc: number;
  public min_Compensate: number;
  public min_Provvisorie: number;
  public min_Suppl: number;
  public min_SupplEcc: number;
  public min_Strao: number;
  public min_StraoNott: number;
  public min_StraoFest: number;
  public min_StraoFestNott: number;
  public min_EccOrd: number;
  public min_NonRiconosciute: number;
  public min_BancaOre: number;
  public min_NonAutorizzate: number;
  public min_DaAutorizzare: number;
  public min_Riconosciute: number | null;

  public note: string | null;
  public dataApprovazione: string | null;
  public idAspNetUsers_Approvazione: string | null;
}

export class Dip_Periodo_Giorno {
  public data: string;
  public festivo: boolean;
  public bloccato: boolean;
  public eccProvvisoria: boolean;
  public bancaOre: boolean;
  public min_Teo: number;
  public min_Lav: number;
  public min_Ecc: number;
  public min_Suppl: number;
  public min_SupplEcc: number;
  public min_Strao: number;
  public min_StraoNott: number;
  public min_StraoFest: number;
  public min_StraoFestNott: number;
  public min_EccOrd: number;
  public min_NonRiconosciute: number;
  public min_NonAutorizzate: number;
  public min_DaAutorizzare: number;
}

export class Dip_Periodo_Settimana {
  public dal: string;
  public al: string;
  public provvisoria: boolean;
  public min_Teo: number;
  public min_Lav: number;
  public min_Ecc: number;
  public min_Qualificate: number;
}

export class Dip_Periodo_Get_InModel {
  public idAspNetUsers: string | null = null; // vuoto = utente corrente
  public anno: number;
  public mese: number;
}
export class Dip_Periodo_Get_OutModel extends ModelResult {
  public dip_Periodo: Dip_PeriodoModel | null;
  public tipoContratto: TipoContratto;
  public oreSettimanaliContratto: number | null;
  public destinazioneEccedenze: DestinazioneEccedenze;
  public tipoConteggio: StraoTipoConteggio;
  public eccedenzeAttive: boolean;
  public min_Saldo: number;
  public settimane: Dip_Periodo_Settimana[] = [];
  public giorni: Dip_Periodo_Giorno[] = [];
}

export class Dip_Periodo_GetAll_InModel {
  public anno: number;
  public mese: number;
  public usersId: string[] = [];
}
export class Dip_Periodo_Riga {
  public idAspNetUsers: string;
  public cognome: string | null;
  public nome: string | null;
  public userName: string | null;
  public tipoContratto: TipoContratto;
  public destinazioneEccedenze: DestinazioneEccedenze;
  public dip_Periodo: Dip_PeriodoModel | null;
}
export class Dip_Periodo_GetAll_OutModel extends ModelResult {
  public righe: Dip_Periodo_Riga[] = [];
}

export class Dip_Periodo_SetStato_InModel {
  public id: number[] = [];
  public stato: PeriodoStato;   // Approvato = approva, DaApprovare = riapri
}
export class Dip_Periodo_SetStato_OutModel extends ModelResult {
  public dip_Periodo: Dip_PeriodoModel[] = [];
}

export class Dip_Periodo_SetRiconosciute_InModel {
  public id: number;
  public min_Riconosciute: number | null;   // null = tutte, 0 = nessuna
  public note: string | null;
}
export class Dip_Periodo_SetRiconosciute_OutModel extends ModelResult {
  public dip_Periodo: Dip_PeriodoModel;
}

/** Minuti -> "h:mm" con segno (es. 270 -> "4:30", -90 -> "-1:30"). */
export function minutiToHHMM(minuti: number | null | undefined, conSegnoPiu: boolean = false): string {
  const m = minuti ?? 0;
  const segno = m < 0 ? '-' : (conSegnoPiu && m > 0 ? '+' : '');
  const abs = Math.abs(m);
  return `${segno}${Math.floor(abs / 60)}:${String(abs % 60).padStart(2, '0')}`;
}
