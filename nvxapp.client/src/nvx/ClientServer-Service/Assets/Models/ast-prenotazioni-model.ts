import { ModelResult } from '../../ModelsBase/model-result';
import { AstPermessiModel } from './ast-model';
import { AstLookupModel } from './ast-flotta-model';

// ---------------------------------------------------------------- prenotazioni (AST-PREN)
export enum AstTipoRisorsa { Postazione = 0, Sala = 1, AutoPool = 2 }

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
