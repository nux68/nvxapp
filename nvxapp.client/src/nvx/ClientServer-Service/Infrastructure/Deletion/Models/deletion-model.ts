import { ModelResult } from '../../../ModelsBase/model-result';

// elemento da eliminare (stessi valori di DeletionNodeType sul server)
export enum DeletionNodeType {
  Dealer = 0,
  FinancialAdvisor = 1,
  Company = 2,
  UserDealer = 3,            // utente tolto da un centro (eliminato se non ha altri collegamenti)
  UserFinancialAdvisor = 4,  // utente tolto da uno studio
  UserCompany = 5,           // utente tolto da un'azienda
  User = 6,                  // utente con tutti i collegamenti (solo SuperUser)
}

// id del nodo o del collegamento; per DeletionNodeType.User l'id dell'utente
export class DeletionTargetModel {
  public nodeType: DeletionNodeType = DeletionNodeType.Company;
  public id: number | null = null;
  public idAspNetUsers: string | null = null;
}

export class DeletionCountModel {
  public label: string = '';
  public count: number = 0;
}


// anteprima: cosa verra' eliminato, senza eliminare nulla
export class DeletionPreviewInModel {
  public target: DeletionTargetModel = new DeletionTargetModel();
}

export class DeletionPreviewOutModel extends ModelResult {
  public descrizione: string = '';          // nome da digitare per confermare
  public counts: DeletionCountModel[] = [];
  public usersKept: string[] = [];          // utenti che restano perche' collegati altrove
  public warnings: string[] = [];
  public canDelete: boolean = false;
  public blockReason: string | null = null;
}


// esecuzione: confirmText deve coincidere con il nome dell'anteprima
export class DeletionExecuteInModel {
  public target: DeletionTargetModel = new DeletionTargetModel();
  public confirmText: string = '';
}

export class DeletionExecuteOutModel extends ModelResult {
  public counts: DeletionCountModel[] = [];
}
