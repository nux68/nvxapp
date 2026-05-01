import { ModelResult } from "../../../ModelsBase/model-result";
import { GG_ResultStato } from "../../Dip_GG_Result/Models/dip-gg-result-model";

// ---------------------------------------------------------------------------
// Input
// ---------------------------------------------------------------------------

export class ActivityStatisticsModel {
  public year: number             = 0;
  public month: number            = 0;
  public selectedUserId: string[] = [];

  // Filtri opzionali (non ancora forniti dall'interfaccia)
  public idsCommessa:    number[] | null = null;
  public idsSubCommessa: number[] | null = null;
  public idsCliente:     number[] | null = null;
  public idsAttivita:    number[] | null = null;
}

export class ActivityStatistics_GetInModel {
  public activityStatistics: ActivityStatisticsModel = new ActivityStatisticsModel();
}

// ---------------------------------------------------------------------------
// Output
// ---------------------------------------------------------------------------

/** Riga ore per dipendente × attività */
export class ActivityStatistics_RowModel {
  public userId:                   string = '';
  public nomeDipendente:           string = '';
  public idAz_SubCommessaAttivita: number = 0;
  public nomeAttivita:             string = '';
  public idAz_SubCommessa:         number = 0;
  public nomeSubCommessa:          string = '';
  public idAz_Commessa:            number = 0;
  public nomeCommessa:             string = '';
  public idCliente:                number = 0;
  public nomeCliente:              string = '';
  /** Totale ore in minuti interi */
  public totaleMinuti:             number = 0;
}

/** Riga aggregata per attività (tutti i dipendenti selezionati) */
export class ActivityStatistics_TotaleAttivitaModel {
  public idAz_SubCommessaAttivita: number = 0;
  public nomeAttivita:             string = '';
  public idAz_SubCommessa:         number = 0;
  public nomeSubCommessa:          string = '';
  public idAz_Commessa:            number = 0;
  public nomeCommessa:             string = '';
  public idCliente:                number = 0;
  public nomeCliente:              string = '';
  public totaleMinuti:             number = 0;
}

/** Giornata esclusa dal conteggio (stato Init o Err) */
export class ActivityStatistics_GiornataEsclusaModel {
  public userId:          string          = '';
  public nomeDipendente:  string          = '';
  public data:            string          = '';
  public stato:           GG_ResultStato  = GG_ResultStato.Init;
}

export class ActivityStatistics_GetOutModel extends ModelResult {
  public righePerDipendente: ActivityStatistics_RowModel[]             = [];
  public totaliPerAttivita:  ActivityStatistics_TotaleAttivitaModel[]  = [];
  public giornateEscluse:    ActivityStatistics_GiornataEsclusaModel[] = [];
}
