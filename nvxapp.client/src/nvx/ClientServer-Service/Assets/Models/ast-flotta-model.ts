import { ModelResult } from '../../ModelsBase/model-result';
import { AstPermessiModel } from './ast-model';

// ---------------------------------------------------------------- comuni del modulo
export class AstIdInModel { public id = 0; }

export class AstLookupModel { public id = 0; public descrizione = ''; public attivo = true; }

export class AstDipendenteModel {
  public id = 0;
  public nominativo = '';
  public haUtente = false;
  public attivo = true;
  public dataCessazione: string | null = null;
}

export class AstDeleteOutModel extends ModelResult { public eliminato = false; }

// ---------------------------------------------------------------- flotta (AST-FLO)
export enum AstAlimentazione { Benzina = 0, Diesel = 1, Gpl = 2, Metano = 3, Ibrida = 4, Elettrica = 5, Altro = 6 }
export enum AstPossesso { Proprieta = 0, Noleggio = 1, Leasing = 2 }
export enum AstUsoVeicolo { Assegnato = 0, Pool = 1 }
export enum AstStatoVeicolo { Attivo = 0, Dismesso = 1 }
export enum AstStatoMulta { DaNotificare = 0, Notificata = 1, Pagata = 2, Contestata = 3 }

export class AstVeicoloModel {
  public id = 0;
  public targa = '';
  public marca: string | null = null;
  public modello: string | null = null;
  public alimentazione = 0;
  public dataImmatricolazione: string | null = null;
  public possesso = 0;
  public fornitore: string | null = null;
  public numeroContratto: string | null = null;
  public scadenzaContratto: string | null = null;
  public kmInclusi: number | null = null;
  public canoneMensile: number | null = null;
  public idAz_Sedi: number | null = null;
  public uso = 0;
  public costoKmAci = 0;
  public emissioniCO2: number | null = null;
  public kmAttuali: number | null = null;
  public stato = 0;
  public note: string | null = null;
}

export class AstVeicoloListModel {
  public id = 0;
  public targa = '';
  public descrizione = '';
  public uso = 0;
  public stato = 0;
  public sede: string | null = null;
  public assegnatario: string | null = null;
  public prossimaScadenza: string | null = null;
  public prossimaScadenzaDescrizione: string | null = null;
}

export class AstVeicoloAssegnazioneModel {
  public id = 0;
  public idAst_Veicolo = 0;
  public idDip_Anagrafica = 0;
  public dipendente: string | null = null;
  public dataInizio: string | null = null;
  public dataFine: string | null = null;
  public usoPromiscuo = true;
  public percentualeFringe = 0;
  public rimborsoKm = 0;
  public idDip_RapportoAutovetture: number | null = null;
  public note: string | null = null;
  public fringeAnnuo = 0;
  public fringeAnnoCorrente = 0;
}

export class AstVeicoloScadenzaModel {
  public id = 0;
  public idAst_Veicolo = 0;
  public tipo = 0;
  public descrizione: string | null = null;
  public dataScadenza: string | null = null;
  public km: number | null = null;
  public importo: number | null = null;
  public dataEsecuzione: string | null = null;
  public note: string | null = null;
}

export class AstVeicoloCostoModel {
  public id = 0;
  public idAst_Veicolo = 0;
  public data: string | null = null;
  public tipo = 0;
  public importo = 0;
  public km: number | null = null;
  public descrizione: string | null = null;
  public documento: string | null = null;
}

export class AstMultaModel {
  public id = 0;
  public idAst_Veicolo = 0;
  public targa: string | null = null;
  public dataInfrazione: string | null = null;
  public numeroVerbale: string | null = null;
  public luogo: string | null = null;
  public descrizione: string | null = null;
  public importo = 0;
  public idDip_Anagrafica: number | null = null;
  public conducente: string | null = null;
  public stato = 0;
  public dataNotifica: string | null = null;
  public dataPagamento: string | null = null;
  public note: string | null = null;
}

export class AstVeicoloListInModel { public dismessi = false; }
export class AstVeicoloListOutModel extends ModelResult { public permessi = new AstPermessiModel(); public veicolo: AstVeicoloListModel[] = []; }

export class AstVeicoloGetOutModel extends ModelResult {
  public veicolo = new AstVeicoloModel();
  public assegnazione: AstVeicoloAssegnazioneModel[] = [];
  public scadenza: AstVeicoloScadenzaModel[] = [];
  public costo: AstVeicoloCostoModel[] = [];
  public multa: AstMultaModel[] = [];
  public totaleCostiAnno = 0;
  public sede: AstLookupModel[] = [];
  public dipendente: AstDipendenteModel[] = [];
}

export class AstVeicoloPutInModel { public veicolo = new AstVeicoloModel(); }
export class AstVeicoloPutOutModel extends ModelResult { public veicolo = new AstVeicoloModel(); }
export class AstAssegnazionePutInModel { public assegnazione = new AstVeicoloAssegnazioneModel(); }
export class AstAssegnazionePutOutModel extends ModelResult { public assegnazione = new AstVeicoloAssegnazioneModel(); }
export class AstVeicoloScadenzaPutInModel { public scadenza = new AstVeicoloScadenzaModel(); }
export class AstVeicoloScadenzaPutOutModel extends ModelResult { public scadenza = new AstVeicoloScadenzaModel(); }
export class AstVeicoloCostoPutInModel { public costo = new AstVeicoloCostoModel(); }
export class AstVeicoloCostoPutOutModel extends ModelResult { public costo = new AstVeicoloCostoModel(); }
export class AstMultaPutInModel { public multa = new AstMultaModel(); }
export class AstMultaPutOutModel extends ModelResult { public multa = new AstMultaModel(); }

export class AstMioVeicoloModel {
  public id = 0;
  public targa = '';
  public descrizione = '';
  public dataInizio: string | null = null;
  public dataFine: string | null = null;
  public usoPromiscuo = false;
  public fringeAnnuo = 0;
  public fringeAnnoCorrente = 0;
}

export class AstMieiVeicoliOutModel extends ModelResult { public veicolo: AstMioVeicoloModel[] = []; public multa: AstMultaModel[] = []; }
