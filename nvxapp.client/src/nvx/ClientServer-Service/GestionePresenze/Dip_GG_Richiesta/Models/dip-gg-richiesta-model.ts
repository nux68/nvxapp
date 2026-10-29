import { ModelResult } from "../../../ModelsBase/model-result";
import { Dip_GG_GiustificativiModel } from "../../Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model";
import { Dip_GG_TimbraturaModel } from "../../Dip_GG_Timbratura/Models/dip-gg-timbratura-model";

/************************************************************/
/******************** MODEL DATA ****************************/
/************************************************************/

export class Dip_GG_RichiestaModel {

  id!: number;
  idDip_RapportoLavoro!: number;

  data!: string;
  dataA!: string;

  richiestaTipo!: TipoRichiesta;
  
  // Campo per oggetto JSON
  dati?: string;  // contiene oggetti di tipo  Dip_GG_Richiesta_Body_Timbratura,Dip_GG_Richiesta_Body_Giustificativo, Dip_GG_Richiesta_Body_NotaSpesa

  richiestaStato!: StatoRichiesta;
  richiestaApprovazioneData: Dip_GG_Richiesta_Stato_Cronology[];

  revocaStato?: StatoRichiesta;
  revocaApprovazioneData: Dip_GG_Richiesta_Stato_Cronology[];

}
export class Dip_GG_Richiesta_Stato_Cronology {
  idAspNetUsers!: string;
  richiestaStato!: StatoRichiesta;
  data!: Date;
}
export class Dip_GG_Richiesta_Body_Timbratura {
  hhmm!: string;
  idAz_SubCommessaAttivita!: number;
}
export class Dip_GG_Richiesta_Body_Giustificativo {
  hhmm!: string;
  allDay!: boolean;
  idPar_Giustificativi!: number;
  /** giustificativo a importo: euro per ogni giorno lavorativo del periodo */
  importo?: number | null;
}
/*
 Nota spese: la richiesta contiene le spese (dettagli). All'invio si mandano spese e allegati;
 il server salva i dettagli e in "dati" lascia solo il riepilogo (Descrizione, Totale, NumeroSpese).
*/
export class Dip_GG_Richiesta_Body_NotaSpesa {
  descrizione: string = '';
  totale?: number;
  numeroSpese?: number;
  spese?: Dip_GG_Richiesta_Body_NotaSpesa_Spesa[];
}
export class Dip_GG_Richiesta_Body_NotaSpesa_Spesa {
  id?: number | null = null;     // solo in modifica: spesa gia' salvata
  data: string = '';             // "YYYY-MM-DD"
  idPar_TipoSpesa: number = 0;
  importo: number = 0;           // rimborso km: lo ricalcola il server (km x rimborso per km dell'autovettura)
  descrizione: string = '';
  km?: number | null = null;     // solo rimborso km
  idDip_RapportoAutovetture?: number | null = null;
  allegati: Dip_GG_Richiesta_Body_NotaSpesa_Allegato[] = [];
}
export class Dip_GG_Richiesta_Body_NotaSpesa_Allegato {
  id?: number | null = null;     // solo in modifica: allegato gia' salvato (senza contenuto)
  nomeFile: string = '';
  contentType: string = '';
  contenuto: string = '';        // base64
}

/** Riepilogo di una nota spese dal campo "dati" della richiesta (il server lo scrive con le maiuscole). */
export function riepilogoNotaSpesa(dati: string | null | undefined): { descrizione: string, totale: number, numeroSpese: number } {
  try {
    const d = JSON.parse(dati ?? '{}');
    return { descrizione: d.Descrizione ?? d.descrizione ?? '', totale: d.Totale ?? d.totale ?? 0, numeroSpese: d.NumeroSpese ?? d.numeroSpese ?? 0 };
  } catch {
    return { descrizione: '', totale: 0, numeroSpese: 0 };
  }
}


export enum TipoRichiesta {
  Timbratura,
  Giustificativo,
  NotaSpesa,
  ApprovazioneStraordinario
}
export enum StatoRichiesta {
  Diretta,

  Immessa,
  Cancellata,
  Rifiutata,
  ApprovazioneInCorso,
  ParzialmenteApprovata,

  Approvata
}


/************************************************************/
/******************** DATI INPUT X API **********************/
/************************************************************/

export class Dip_GG_Richiesta_GetAll4User_InModel {
  public idAspNetUsers?: string
  public year: number;
  public month: number;

  constructor() {}
}
export class Dip_GG_Richiesta_GetAll4User_OutModel extends ModelResult {

  public dip_GG_Richiesta: Dip_GG_RichiestaModel[];

}


// contatori del menu "Richieste"
export class Dip_GG_Richiesta_Count_InModel {
  // true = calcola anche le richieste da approvare (menu amministratore)
  public admin: boolean = false;
}
export class Dip_GG_Richiesta_Count_OutModel extends ModelResult {
  // richieste dell'azienda che l'utente puo' approvare adesso
  public daApprovare: number = 0;
  // richieste dell'utente in attesa di approvazione
  public inAttesa: number = 0;
}

// notifica push (evento SignalR "RichiestaNotifier")
export class RichiestaNotifierData {
  public idDip_GG_Richiesta: number[] = [];
}

// filtro della lista richieste dell'amministratore, sulla fase corrente (revoca se presente, altrimenti richiesta)
export enum Dip_GG_Richiesta_Filtro {
  Tutte = 0,
  DaApprovare = 1,  // ancora da decidere
  Approvate = 2     // approvata o diretta
}

export class Dip_GG_Richiesta_GetAll4Admin_InModel {
  public year: number;
  public month: number;
  public filtro: Dip_GG_Richiesta_Filtro = Dip_GG_Richiesta_Filtro.Tutte;

  constructor() { }
}
export class Dip_GG_Richiesta_GetAll4Admin_OutModel extends ModelResult {

  public dip_GG_Richiesta: Dip_GG_RichiestaModel[];

}


// modifica di una nota spese non ancora approvata (dipendente che l'ha inviata)
export class Dip_GG_Richiesta_NotaSpesaUpdate_InModel {
  public idDip_GG_Richiesta: number = 0;
  public dati: string = '';
}
export class Dip_GG_Richiesta_NotaSpesaUpdate_OutModel extends ModelResult {
  public dip_GG_Richiesta: Dip_GG_RichiestaModel | null = null;
}

/** Nota spese ancora modificabile dal dipendente: non approvata, non chiusa e senza revoca. */
export function notaSpesaModificabile(nota: Dip_GG_RichiestaModel | null | undefined): boolean {
  return !!nota && nota.revocaStato == null &&
    [StatoRichiesta.Immessa, StatoRichiesta.ApprovazioneInCorso, StatoRichiesta.ParzialmenteApprovata].includes(nota.richiestaStato);
}

export class Dip_GG_Richiesta_Send_InModel {
  public excludeRicalc: boolean;
  public idAspNetUsers?: string
  public dip_GG_Richiesta: Dip_GG_RichiestaModel;
  public fromHR: boolean;
  constructor() {
    this.dip_GG_Richiesta = new Dip_GG_RichiestaModel(); 
  }
}
export class Dip_GG_Richiesta_Send_OutModel extends ModelResult {

  public dip_GG_Richiesta: Dip_GG_RichiestaModel[];
  public dip_GG_Timbratura: Dip_GG_TimbraturaModel[];
  public dip_GG_Giustificativi: Dip_GG_GiustificativiModel[];

  
  constructor() {
    super();
  }

}


export class Dip_GG_Richiesta_SetState_InModel {

  public excludeRicalc: boolean;
  public richiestaStato!: StatoRichiesta;
  public idDip_GG_Richiesta: number[] = [];
  public fromHR: boolean;

  constructor() {
    
  }
}
export class Dip_GG_Richiesta_SetState_OutModel extends ModelResult {

  public dip_GG_Richiesta: Dip_GG_RichiestaModel[] = [];
  public dip_GG_Timbratura: Dip_GG_TimbraturaModel[] = [];
  public dip_GG_Giustificativi: Dip_GG_GiustificativiModel[] = [];


  constructor() {
    super();
    //this.dip_GG_Richiesta = new Dip_GG_RichiestaModel();
  }

}


export class Dip_GG_Richiesta_Get_4Calculation_InModel {
  public usersId: string[] = [];
  public dal!: string;
  public al!: string;
}
export class Dip_GG_Richiesta_Get_4Calculation_OutModel extends ModelResult {
  public dip_GG_Richiesta: Dip_GG_RichiestaModel[] = [];
}
