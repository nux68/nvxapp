import { ModelResult } from "../../../ModelsBase/model-result";

// ── Calcolo contatori ─────────────────────────────────────────────────────────

export class Contatori_Calcolo_InModel {
  public idDip_RapportoLavoro: number = 0;
  public anno: number = 0;
  public mese: number = 0;
}

export class Contatori_Periodo {
  public maturato: string = '00:00:00';
  public goduto:   string = '00:00:00';
  public saldo:    string = '00:00:00';
}

export class Contatori_Giustificativo_Result {
  public idPar_Giustificativi: number = 0;
  public descrizione:          string = '';
  public codice:               string = '';
  public periodoPrecedente:    Contatori_Periodo = new Contatori_Periodo();
  public periodoCorrente:      Contatori_Periodo = new Contatori_Periodo();
  public periodoSuccessivo:    Contatori_Periodo = new Contatori_Periodo();
}

export class Contatori_Calcolo_OutModel extends ModelResult {
  public risultati: Contatori_Giustificativo_Result[] = [];
}

// ── Riporto (Mese 0) ──────────────────────────────────────────────────────────

export class Contatori_Riporto_Model {
  public id:                   number = 0;
  public idDip_RapportoLavoro: number = 0;
  public idPar_Giustificativi: number = 0;
  public anno:                 number = 0;
  public saldoRiporto:         string = '00:00:00';
  public isManuale:            boolean = false;
}

export class Contatori_Riporto_GetAll_InModel {
  public idDip_RapportoLavoro: number = 0;
  public anno:                 number = 0;
}

export class Contatori_Riporto_GetAll_OutModel extends ModelResult {
  public riporti: Contatori_Riporto_Model[] = [];
}

export class Contatori_Riporto_Upsert_InModel {
  public riporto: Contatori_Riporto_Model = new Contatori_Riporto_Model();
}

export class Contatori_Riporto_Upsert_OutModel extends ModelResult {
  public riporto: Contatori_Riporto_Model | null = null;
}

export class Contatori_Riporto_Delete_InModel {
  public id: number = 0;
}

export class Contatori_Riporto_Delete_OutModel extends ModelResult {}

// ── Vista annuale ─────────────────────────────────────────────────────────────

/** Maturazione mensile passata come override runtime (valori non ancora salvati). */
export class Contatori_Maturazione_Model {
  public idPar_Giustificativi: number = 0;
  public oreMaturazione:       string = '00:00';
}

export class Contatori_Anno_InModel {
  public idDip_RapportoLavoro: number = 0;
  public anno: number = 0;

  /**
   * Override runtime dei riporti (mese 0).
   * Se valorizzato sostituisce i valori DB per i giustificativi corrispondenti.
   * Usato per ricalcolare in tempo reale senza salvare.
   */
  public riportiOverride:     Contatori_Riporto_Model[]      | null = null;

  /** Override runtime della maturazione mensile. */
  public maturazioneOverride: Contatori_Maturazione_Model[]  | null = null;
}

export class Contatori_Anno_MeseResult {
  public mese: number = 0;
  public risultati: Contatori_Giustificativo_Result[] = [];
}

export class Contatori_Anno_OutModel extends ModelResult {
  public mesi: Contatori_Anno_MeseResult[] = [];
}
