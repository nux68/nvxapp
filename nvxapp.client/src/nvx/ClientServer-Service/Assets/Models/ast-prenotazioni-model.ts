import { ModelResult } from '../../ModelsBase/model-result';
import { AstPermessiModel } from './ast-model';
import { AstLookupModel } from './ast-flotta-model';

// ---------------------------------------------------------------- prenotazioni (AST-PREN)
export enum AstTipoRisorsa { Postazione = 0, Sala = 1, AutoPool = 2 }
/** stato della prenotazione: InAttesa e Rifiutata solo sulle risorse con approvazione (ANALISI_INFRASTRUCTURE_RICHIESTE.md) */
export enum AstStatoPrenotazione { Confermata = 0, Annullata = 1, InAttesa = 2, Rifiutata = 3 }

export class AstRisorsaModel {
  public id = 0;
  public tipo = 0;
  public descrizione = '';
  public idAz_Sedi: number | null = null;
  public sede: string | null = null;
  public capienza: number | null = null;
  public idAst_Veicolo: number | null = null;
  public veicolo: string | null = null;
  public attiva = true;
  public note: string | null = null;
  /** le prenotazioni degli utenti vanno approvate da un amministratore */
  public richiedeApprovazione = false;
}

export class AstPrenotazioneModel {
  public id = 0;
  public idAst_Risorsa = 0;
  public risorsa: string | null = null;
  public tipoRisorsa = 0;
  public inizio: string | null = null;
  public fine: string | null = null;
  public titolo: string | null = null;
  public note: string | null = null;
  public stato = 0;
  /** motivo del rifiuto (proprie e amministratori) */
  public motivoRifiuto: string | null = null;
  /** confermata con la richiesta di annullamento in attesa di approvazione */
  public annullamentoRichiesto = false;
  /** l'amministratore puo' approvarla o rifiutarla adesso (richiesta o annullamento in attesa) */
  public daDecidere = false;
  /** id della richiesta comune (pagine /requests) da approvare o rifiutare */
  public idRichiesta: number | null = null;
  /** richiedente: ritira la richiesta o l'annullamento ancora in attesa ("Annulla"), come la revoca delle presenze */
  public ritirabile = false;
  /** richiedente: chiede l'annullamento della prenotazione approvata ("Cancella", va approvato) */
  public annullamentoChiedibile = false;
  /** annullamento rifiutato: pratica conclusa, resta approvata */
  public annullamentoRifiutato = false;
  /** annullamento ritirato dal richiedente: pratica conclusa, resta approvata */
  public annullamentoRitirato = false;
  /** annullata perche' l'annullamento (revoca) e' stato approvato */
  public revocata = false;
  /** soggetta ad approvazione (richiesta comune): confermata = "Approvata" */
  public conRichiesta = false;
  public mia = false;
  public utente: string | null = null;
  public modificabile = false;
}

export class AstRisorsaListInModel { public tutte = false; }
export class AstRisorsaListOutModel extends ModelResult {
  public permessi = new AstPermessiModel();
  public risorsa: AstRisorsaModel[] = [];
  public sede: AstLookupModel[] = [];
  public veicoloPool: AstLookupModel[] = [];
}
export class AstRisorsaPutInModel { public risorsa = new AstRisorsaModel(); }
export class AstRisorsaPutOutModel extends ModelResult { public risorsa = new AstRisorsaModel(); }

export class AstPrenotazioneListInModel {
  public dal: string | null = null;
  public al: string | null = null;
  public idAst_Risorsa: number | null = null;
  public soloMie = false;
}
export class AstPrenotazioneListOutModel extends ModelResult {
  public permessi = new AstPermessiModel();
  public prenotazione: AstPrenotazioneModel[] = [];
  public risorsa: AstRisorsaModel[] = [];
  public durataMaxOre = 0;
  public anticipoMaxGiorni = 0;
}
export class AstPrenotazionePutInModel { public prenotazione = new AstPrenotazioneModel(); }
export class AstPrenotazionePutOutModel extends ModelResult { public prenotazione = new AstPrenotazioneModel(); }
