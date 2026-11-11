import { ModelResult } from '../../../ModelsBase/model-result';

/*
 Pagamenti delle licenze e ripartizione ai centri (ANALISI_PAGAMENTI_CENTRI.md): modelli speculari a quelli del server
 (Payments/Models/PaymentModel.cs). Gli enum viaggiano come interi; mese null = tutto l'anno.
*/

export enum StatoRendiconto { Bozza = 0, Confermato = 1, Liquidato = 2, Annullato = 3 }
export enum TipoRiepilogoPagamenti { Mensile = 1, Annuale = 2, Liquidazione = 3 }

export class PaymentSettlementModel {
  public id: number = 0;
  public idDealer: number = 0;
  public dealer: string | null = null;
  public anno: number = 0;
  public mese: number = 0;
  public numeroPagamenti: number = 0;
  public totaleIncassato: number = 0;
  public totaleRettifiche: number = 0;
  public percentualePiattaforma: number = 0;
  public quotaPiattaforma: number = 0;
  public quotaCentro: number = 0;
  public valuta: string = 'EUR';
  public stato: StatoRendiconto = StatoRendiconto.Bozza;
  public dataConferma: string | null = null;
  public dataLiquidazione: string | null = null;
  public riferimentoBonifico: string | null = null;
  public numeroFatturaCentro: string | null = null;
  public dataFatturaCentro: string | null = null;
  public note: string | null = null;
  public ultimoInvio: string | null = null;
}

export class PaymentDealerSummaryModel {
  public idDealer: number | null = null;
  public dealer: string | null = null;
  public numeroPagamenti: number = 0;
  public incassato: number = 0;
  public rimborsato: number = 0;
  public numeroDebug: number = 0;
  public importoDebug: number = 0;
  public percentualePiattaforma: number = 0;
  public quotaPiattaforma: number = 0;
  public quotaCentro: number = 0;
  public liquidato: number = 0;
  public daLiquidare: number = 0;
  public rendiconto: PaymentSettlementModel | null = null;
}

export class PaymentTotalsModel {
  public numeroPagamenti: number = 0;
  public incassato: number = 0;
  public quotaPiattaforma: number = 0;
  public quotaCentri: number = 0;
  public liquidato: number = 0;
  public daLiquidare: number = 0;
}

export class PaymentRowModel {
  public id: number = 0;
  public dataPagamento: string | null = null;
  public idDealer: number | null = null;
  public dealer: string | null = null;
  public soggetto: string | null = null;
  public tipoSoggetto: number = 1;
  public ambito: number = 0;
  public livello: number = 0;
  public periodicita: number = 1;
  public dataInizioCoperta: string = '';
  public dataFineCoperta: string = '';
  public importo: number = 0;
  public valuta: string = 'EUR';
  public stato: number = 0;
  public debug: boolean = false;
  public idRendiconto: number | null = null;
  public statoRendiconto: StatoRendiconto | null = null;
  public idRendicontoRettifica: number | null = null;
}

export class PaymentDealerItemModel {
  public idDealer: number = 0;
  public descrizione: string = '';
}

export class PaymentEmailLogModel {
  public data: string | null = null;
  public anno: number = 0;
  public mese: number | null = null;
  public tipo: TipoRiepilogoPagamenti = TipoRiepilogoPagamenti.Mensile;
  public automatico: boolean = false;
  public destinatari: string | null = null;
  public esito: boolean = false;
  public errore: string | null = null;
}

export class PaymentDealerConfigModel {
  public idDealer: number = 0;
  public dealer: string | null = null;
  public percentualePiattaforma: number | null = null;
  public percentualeGenerale: number = 0;
  public iban: string | null = null;
  public intestatario: string | null = null;
  public emailAmministrazione: string | null = null;
}

export class PaymentSettingsModel {
  public percentualePiattaforma: number = 0;
  public riepilogoAutomatico: boolean = false;
  public giornoInvioMensile: number = 5;
  public meseInvioAnnuale: number = 1;
}

// ---------------------------------------------------------------------------

export class PaymentReportGetInModel { public anno: number = new Date().getFullYear(); public mese: number | null = null; }
export class PaymentReportGetOutModel extends ModelResult {
  public isSuperUser: boolean = false;
  public centro: PaymentDealerSummaryModel[] = [];
  public totali: PaymentTotalsModel = new PaymentTotalsModel();
  public debugPagamenti: boolean = false;
}

export class PaymentDealerGetInModel { public idDealer: number | null = null; public anno: number = new Date().getFullYear(); public mese: number | null = null; }
export class PaymentDealerGetOutModel extends ModelResult {
  public isSuperUser: boolean = false;
  public canEditDealerData: boolean = false;
  public emailConfigurata: boolean = false;
  public riepilogo: PaymentDealerSummaryModel = new PaymentDealerSummaryModel();
  public pagamento: PaymentRowModel[] = [];
  public rendiconto: PaymentSettlementModel[] = [];
  public invio: PaymentEmailLogModel[] = [];
  public dealer: PaymentDealerItemModel[] = [];
}

export class PaymentSettlementGenerateInModel { public anno: number = 0; public mese: number = 0; public idDealer: number | null = null; }
export class PaymentSettlementGenerateOutModel extends ModelResult { public generati: number = 0; }

export class PaymentSettlementConfirmInModel { public id: number = 0; }
export class PaymentSettlementConfirmOutModel extends ModelResult { public rendiconto: PaymentSettlementModel = new PaymentSettlementModel(); public emailInviata: boolean = false; }

export class PaymentSettlementCancelInModel { public id: number = 0; }
export class PaymentSettlementCancelOutModel extends ModelResult { }

export class PaymentSettlementPayInModel { public id: number = 0; public dataLiquidazione: string = ''; public riferimentoBonifico: string | null = null; public note: string | null = null; }
export class PaymentSettlementPayOutModel extends ModelResult { public rendiconto: PaymentSettlementModel = new PaymentSettlementModel(); public emailInviata: boolean = false; }

export class PaymentSettlementInvoicePutInModel { public id: number = 0; public numeroFatturaCentro: string | null = null; public dataFatturaCentro: string | null = null; }
export class PaymentSettlementInvoicePutOutModel extends ModelResult { public rendiconto: PaymentSettlementModel = new PaymentSettlementModel(); }

export class PaymentSummarySendInModel { public idDealer: number | null = null; public anno: number = 0; public mese: number | null = null; }
// l'email parte con un job: l'esito arriva nella barra dei job
export class PaymentSummarySendOutModel extends ModelResult { public jobId: string = ''; public destinatari: string | null = null; }

export class PaymentExportInModel { public idDealer: number | null = null; public anno: number = 0; public mese: number | null = null; public formato: 'pdf' | 'xlsx' = 'pdf'; }
// il file si prepara con un job: arriva nella barra dei job con il link di download
export class PaymentExportOutModel extends ModelResult { public jobId: string = ''; }

export class PaymentDealerConfigGetInModel { public idDealer: number | null = null; }
export class PaymentDealerConfigGetOutModel extends ModelResult { public config: PaymentDealerConfigModel = new PaymentDealerConfigModel(); public canEditPercentuale: boolean = false; }

export class PaymentDealerConfigPutInModel { public config: PaymentDealerConfigModel = new PaymentDealerConfigModel(); }
export class PaymentDealerConfigPutOutModel extends ModelResult { public config: PaymentDealerConfigModel = new PaymentDealerConfigModel(); }

export class PaymentSettingsGetInModel { }
export class PaymentSettingsGetOutModel extends ModelResult { public settings: PaymentSettingsModel = new PaymentSettingsModel(); public emailConfigurata: boolean = false; public debugPagamenti: boolean = false; }

export class PaymentSettingsPutInModel { public settings: PaymentSettingsModel = new PaymentSettingsModel(); }
export class PaymentSettingsPutOutModel extends ModelResult { public settings: PaymentSettingsModel = new PaymentSettingsModel(); }
