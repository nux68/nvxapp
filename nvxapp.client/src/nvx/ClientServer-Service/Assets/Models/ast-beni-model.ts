import { ModelResult } from '../../ModelsBase/model-result';
import { AstPermessiModel } from './ast-model';
import { AstDipendenteModel, AstLookupModel } from './ast-flotta-model';

// ---------------------------------------------------------------- beni assegnati (AST-BENI)
export enum AstStatoBene { Disponibile = 0, Assegnato = 1, InRiparazione = 2, Dismesso = 3 }

export class AstCategoriaModel { public id = 0; public descrizione = ''; public attivo = true; public ordine = 0; public numeroBeni = 0; }

export class AstBeneModel {
  public id = 0;
  public codiceInventario = '';
  public idAst_Categoria = 0;
  public descrizione = '';
  public marcaModello: string | null = null;
  public numeroSerie: string | null = null;
  public dataAcquisto: string | null = null;
  public valore: number | null = null;
  public scadenzaGaranzia: string | null = null;
  public idAz_Sedi: number | null = null;
  public stato = 0;
  public note: string | null = null;
}

export class AstBeneListModel {
  public id = 0;
  public codiceInventario = '';
  public descrizione = '';
  public categoria: string | null = null;
  public numeroSerie: string | null = null;
  public sede: string | null = null;
  public stato = 0;
  public assegnatario: string | null = null;
  public dataConsegna: string | null = null;
  public scadenzaGaranzia: string | null = null;
}

export class AstVerbaleModel { public richiesto = false; public dataConferma: string | null = null; public canale: string | null = null; }

export class AstBeneAssegnazioneModel {
  public id = 0;
  public idAst_Bene = 0;
  public bene: string | null = null;
  public codiceInventario: string | null = null;
  public idDip_Anagrafica = 0;
  public dipendente: string | null = null;
  public dataConsegna: string | null = null;
  public condizioniConsegna: string | null = null;
  public dataRestituzione: string | null = null;
  public condizioniRestituzione: string | null = null;
  public note: string | null = null;
  public verbaleConsegna = new AstVerbaleModel();
  public verbaleRestituzione = new AstVerbaleModel();
}

export class AstCategoriaListOutModel extends ModelResult { public categoria: AstCategoriaModel[] = []; }
export class AstCategoriaPutInModel { public categoria = new AstCategoriaModel(); }
export class AstCategoriaPutOutModel extends ModelResult { public categoria = new AstCategoriaModel(); }

export class AstBeneListInModel { public dismessi = false; }
export class AstBeneListOutModel extends ModelResult { public permessi = new AstPermessiModel(); public bene: AstBeneListModel[] = []; }

export class AstBeneGetOutModel extends ModelResult {
  public bene = new AstBeneModel();
  public assegnazione: AstBeneAssegnazioneModel[] = [];
  public categoria: AstCategoriaModel[] = [];
  public sede: AstLookupModel[] = [];
  public dipendente: AstDipendenteModel[] = [];
}

export class AstBenePutInModel { public bene = new AstBeneModel(); }
export class AstBenePutOutModel extends ModelResult { public bene = new AstBeneModel(); }

export class AstConsegnaPutInModel {
  public idAst_Bene = 0;
  public idDip_Anagrafica = 0;
  public dataConsegna: string | null = null;
  public condizioniConsegna: string | null = null;
  public note: string | null = null;
}

export class AstRestituzionePutInModel {
  public idAst_BeneAssegnazione = 0;
  public dataRestituzione: string | null = null;
  public condizioniRestituzione: string | null = null;
  public statoBene = 0;
}

export class AstBeneAssegnazionePutOutModel extends ModelResult { public assegnazione = new AstBeneAssegnazioneModel(); }

// restituzioni alla cessazione (AST-BENI.3)
export class AstDaRestituireModel {
  public tipo: 'bene' | 'veicolo' = 'bene';
  public idAssegnazione = 0;
  public idOggetto = 0;
  public oggetto = '';
  public idDip_Anagrafica = 0;
  public dipendente = '';
  public dataCessazione: string | null = null;
  public dataConsegna: string | null = null;
  public dataAvviso: string | null = null;
  public chiave = '';
}

export class AstDaRestituireListOutModel extends ModelResult { public daRestituire: AstDaRestituireModel[] = []; }
export class AstDaRestituireVerificaOutModel extends ModelResult { public avvisate = 0; public daRestituire: AstDaRestituireModel[] = []; }

export class AstMieiBeniOutModel extends ModelResult { public assegnazione: AstBeneAssegnazioneModel[] = []; }
