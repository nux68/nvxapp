import { ModelResult } from "../../../ModelsBase/model-result";


// riepiloghi periodici dell'utente corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-AI.3)
export enum DigestType {
  Azienda = 0,
  Studio = 1,
}

export class DigestModel {
  public id = 0;
  public tipo: DigestType = DigestType.Azienda;
  public idCompany: number | null = null;
  public idFinancialAdvisor: number | null = null;
  /** azienda o studio */
  public soggetto: string | null = null;
  public dal: string | null = null;
  public al: string | null = null;
  public titolo = '';
  /** inizio del testo (elenco) o testo completo (dettaglio) */
  public testo = '';
  public generatoConAI = false;
  public manuale = false;
  public data: string | null = null;
}

/** fatto di una sezione: frase, numero, oggetti a cui si riferisce */
export class DigestFact {
  public testo = '';
  public numero: number | null = null;
  public critico = false;
  public dettagli: string[] = [];
}

/** sezione fornita da un modulo */
export class DigestSection {
  public codice = '';
  public application = 0;
  public titolo = '';
  public critica = false;
  public ordine = 0;
  public righe: DigestFact[] = [];
}

export class DigestCompanyFacts {
  public idCompany = 0;
  public azienda = '';
  public sezioni: DigestSection[] = [];
}

/** fatti da cui e' stato scritto il testo: sezioni dell'azienda oppure aziende dello studio con criticita' */
export class DigestFacts {
  public dal: string | null = null;
  public al: string | null = null;
  public soggetto = '';
  public sezioni: DigestSection[] | null = null;
  public aziende: DigestCompanyFacts[] | null = null;
}

export class DigestDetailModel extends DigestModel {
  public fatti: DigestFacts | null = null;
}


export class DigestListInModel {
}

export class DigestListOutModel extends ModelResult {
  public digest: DigestModel[] = [];
  /** "Genera ora": CompanyPowerAdmin nell'azienda corrente o FinancialAdvisorPowerAdmin nello studio corrente */
  public puoGenerare = false;
}


export class DigestGetInModel {
  public id = 0;
}

export class DigestGetOutModel extends ModelResult {
  public digest: DigestDetailModel = new DigestDetailModel();
}


export class DigestGenerateInModel {
}

export class DigestGenerateOutModel extends ModelResult {
  /** riepiloghi creati (vuoto = nulla da riepilogare) */
  public id: number[] = [];
}
