import { ModelResult } from "../../../ModelsBase/model-result";
import { FormulaFase } from "../../Par_Formule/Models/par-formule-model";

// profilo formule: elenco ordinato di formule assegnato ai rapporti di lavoro
export class Par_ProfiloFormuleModel {
  id: number = 0;
  idAz_Anagrafica: number = 0;
  codice: string = '';
  descrizione: string = '';
}

// riga del profilo (id negativo = nuova)
export class Par_ProfiloFormuleDettModel {
  id: number = 0;
  idPar_ProfiloFormule: number = 0;
  idPar_Formule: number = 0;
  ordine: number = 1;
  dal: string = '';
  al: string = '';
  sospesa: boolean = false;
  fase: FormulaFase = FormulaFase.Finale;
}

// profilo formule del rapporto di lavoro, storicizzato Dal/Al (si salva con l'anagrafica del dipendente)
export class Dip_ProfiloFormuleModel {
  id: number = 0;
  idDip_RapportoLavoro: number = 0;
  idPar_ProfiloFormule: number = 0;
  dal: string = '';
  al: string = '';
}

export class Par_ProfiloFormuleInModel { }
export class Par_ProfiloFormuleOutModel extends ModelResult {
  public par_ProfiloFormule: Par_ProfiloFormuleModel[] = [];
}

export class Par_ProfiloFormuleGetInModel {
  public id: number = 0;
}
export class Par_ProfiloFormuleGetOutModel extends ModelResult {
  public par_ProfiloFormule: Par_ProfiloFormuleModel = new Par_ProfiloFormuleModel();
  public par_ProfiloFormuleDett: Par_ProfiloFormuleDettModel[] = [];
}

export class Par_ProfiloFormulePutInModel {
  public par_ProfiloFormule: Par_ProfiloFormuleModel = new Par_ProfiloFormuleModel();
  public par_ProfiloFormuleDett: Par_ProfiloFormuleDettModel[] = [];
}
export class Par_ProfiloFormulePutOutModel extends ModelResult {
  public par_ProfiloFormule: Par_ProfiloFormuleModel = new Par_ProfiloFormuleModel();
  public par_ProfiloFormuleDett: Par_ProfiloFormuleDettModel[] = [];
}

export class Par_ProfiloFormule_DeleteInModel {
  public id: number = 0;
}
export class Par_ProfiloFormule_DeleteOutModel extends ModelResult { }

export class Par_ProfiloFormule_DuplicaInModel {
  public id: number = 0;
  public codice: string = '';
}
export class Par_ProfiloFormule_DuplicaOutModel extends ModelResult {
  public par_ProfiloFormule: Par_ProfiloFormuleModel = new Par_ProfiloFormuleModel();
}
