import { ModelResult } from "../../../ModelsBase/model-result";

// anomalie delle timbrature, giornate da sistemare e previsione delle assenze (ANALISI_ATTENDANCETRACKING_ANOMALIE_AI.md)
// stessi valori degli enum AnomaliaTipo e AnomaliaGravita del server
export enum AnomaliaTipo {
  TimbratureDispari = 0,
  GiornataVuota = 1,
  ErroreCalcolo = 2,
  NonCalcolata = 3,
  RichiestaInAttesa = 4,
  FuoriOrario = 5,
  GiornoNonLavorativo = 6,
  PausaMancante = 7,
  PausaEccessiva = 8,
  Ritardo = 9
}

export enum AnomaliaGravita {
  Avviso = 0,
  Errore = 1
}

export class AnomaliaModel {
  public tipo: AnomaliaTipo;
  public gravita: AnomaliaGravita;
  public descrizione = '';
}

export class AnomaliaGiornoModel {
  public idDip_RapportoLavoro = 0;
  public idDip_Anagrafica = 0;
  public idAspNetUsers = '';
  public nomeDipendente = '';
  public data = '';
  public minTeo = 0;
  public timbrature: string[] = [];
  public anomalie: AnomaliaModel[] = [];
  public daSistemare = false;
}

export class AnomaliaRitardiModel {
  public idDip_Anagrafica = 0;
  public idAspNetUsers = '';
  public nomeDipendente = '';
  public ritardi = 0;
  public ritardoMedioMin = 0;
  public giorni: string[] = [];
}

export class AnomaliaConteggioModel {
  public tipo: AnomaliaTipo;
  public gravita: AnomaliaGravita;
  public numero = 0;
  public descrizione = '';
}

export class AnomalieRisultato {
  public dal = '';
  public al = '';
  public dipendentiAnalizzati = 0;
  public giornateAnalizzate = 0;
  public giornateDaSistemare = 0;
  public giornateDaVerificare = 0;
  public dipendentiCoinvolti = 0;
  public conteggi: AnomaliaConteggioModel[] = [];
  public giornate: AnomaliaGiornoModel[] = [];
  public ritardiRicorrenti: AnomaliaRitardiModel[] = [];
}

export class Dip_Anomalie_GetInModel {
  public dal = '';
  public al = '';
  public usersId: string[] = [];
  public soloDaSistemare = false;
}

export class Dip_Anomalie_GetOutModel extends ModelResult {
  public risultato: AnomalieRisultato;
}

export class Dip_Anomalie_SpiegaOutModel extends ModelResult {
  public testo = '';
  public generatoConAI = false;
  public providerAI: string | null = null;
}

// previsione delle assenze
export class PrevisioneTassoModel {
  public valore = 0;
  public giornate = 0;
  public tasso = 0;
  public daStorico = false;
}

export class PrevisioneGiornoModel {
  public data = '';
  public inServizio = 0;
  public pianificate = 0;
  public statistiche = 0;
  public previste = 0;
  public picco = false;
  public pianificati: string[] = [];
}

export class PrevisioneRisultato {
  public dal = '';
  public al = '';
  public storicoDal = '';
  public storicoAl = '';
  public giornateStorico = 0;
  public minimoGiornate = 0;
  public tassoMedio = 0;
  public sogliaPiccoPerc = 0;
  public mediaPreviste = 0;
  public tassiSettimana: PrevisioneTassoModel[] = [];
  public fattoriMese: PrevisioneTassoModel[] = [];
  public giorni: PrevisioneGiornoModel[] = [];
}

export class Dip_PrevisioneAssenze_GetInModel {
  public usersId: string[] = [];
  public giorni = 30;
}

export class Dip_PrevisioneAssenze_GetOutModel extends ModelResult {
  public risultato: PrevisioneRisultato;
}

// widget
export class Widget_GiornateDaSistemareGetInModel { }

export class Widget_GiornateDaSistemareGetOutModel extends ModelResult {
  public dal = '';
  public al = '';
  public daSistemare = 0;
  public daVerificare = 0;
  public dipendenti = 0;
  public conteggi: AnomaliaConteggioModel[] = [];
}

export class Widget_PrevisioneAssenzeGetInModel { }

export class Widget_PrevisioneAssenzeGetOutModel extends ModelResult {
  public mediaPreviste = 0;
  public pianificate = 0;
  public giorni: PrevisioneGiornoModel[] = [];
}
