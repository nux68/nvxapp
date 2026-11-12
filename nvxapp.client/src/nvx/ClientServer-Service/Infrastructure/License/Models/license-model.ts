import { ModelResult } from '../../../ModelsBase/model-result';

/*
 Licenze d'uso (ANALISI_INFRASTRUCTURE_LICENZE.md): modelli speculari a quelli del server (License/Models/LicenseModel.cs).
 Gli enum viaggiano come interi.
*/

export enum TipoSoggettoLicenza { Company = 1, FinancialAdvisor = 2 }
export enum AmbitoLicenza { AttendanceTracking = 2, Crm = 3, DocumentManagement = 4, HumanResources = 5, Compliance = 6, Assets = 7, Cumulativo = 100 }
export enum PeriodicitaLicenza { Mensile = 1, Annuale = 2 }
export enum MetodoPagamento { Nessuno = 0, CartaCredito = 1, PayPal = 2 }
export enum StatoAbbonamento { InAttesa = 0, Attivo = 1, Scaduto = 2, Sospeso = 3, Annullato = 4 }
export enum StatoPagamento { Creato = 0, InAttesa = 1, Completato = 2, Fallito = 3, Rimborsato = 4, Debug = 5 }
export enum FonteCopertura { Nessuna = 0, Propria = 1, Studio = 2 }

export const AMBITI_LICENZA: AmbitoLicenza[] = [AmbitoLicenza.AttendanceTracking, AmbitoLicenza.Crm, AmbitoLicenza.DocumentManagement,
  AmbitoLicenza.HumanResources, AmbitoLicenza.Compliance, AmbitoLicenza.Assets, AmbitoLicenza.Cumulativo];

// ---------------------------------------------------------------------------
// Impostazioni (SuperUser)

export class LicenseSettingsModel {
  public attive: boolean = false;
  public maxUtentiFree: number | null = 10;
  public maxUtentiStandard: number | null = 50;
  public maxUtentiPro: number | null = null;
  public maxClientiFree: number | null = 100;
  public maxClientiStandard: number | null = 1000;
  public maxClientiPro: number | null = null;
  public maxSpazioMBFree: number | null = 1024;
  public maxSpazioMBStandard: number | null = 5120;
  public maxSpazioMBPro: number | null = 20480;
  public graceGiorni: number = 7;
  public avvisoGiorni: number = 10;
}

export class LicenseDealerModel {
  public idDealer: number = 0;
  public descrizione: string = '';
  public puoGestireListini: boolean = false;
}

export class LicenseSettingsGetInModel { }
export class LicenseSettingsGetOutModel extends ModelResult {
  public settings: LicenseSettingsModel = new LicenseSettingsModel();
  public dealer: LicenseDealerModel[] = [];
  public debugPagamenti: boolean = false;
}

export class LicenseSettingsPutInModel {
  public settings: LicenseSettingsModel = new LicenseSettingsModel();
  public dealer: LicenseDealerModel[] = [];
}
export class LicenseSettingsPutOutModel extends LicenseSettingsGetOutModel { }

// ---------------------------------------------------------------------------
// Listini

export class LicensePlanModel {
  public id: number = 0;
  public livello: number = 1;
  public codice: string = '';
  public descrizione: string | null = null;
  public ordine: number = 0;
  public prezzoMensile: number = 0;
  public prezzoAnnuale: number = 0;
  // limiti (null = illimitati): utenti (Presenze), clienti (CRM), spazio in MB (Documentale); il Cumulativo li usa tutti
  public maxUtenti: number | null = null;
  public maxClienti: number | null = null;
  public maxSpazioMB: number | null = null;
  public attivo: boolean = true;
}

export class LicenseCatalogModel {
  public id: number = 0;
  public idDealer: number | null = null;
  public dealer: string | null = null;
  public tipoSoggetto: TipoSoggettoLicenza = TipoSoggettoLicenza.Company;
  public ambito: AmbitoLicenza = AmbitoLicenza.AttendanceTracking;
  public descrizione: string = '';
  public valuta: string = 'EUR';
  public disponibileSuCompany: boolean = false;
  public attivo: boolean = true;
  public piano: LicensePlanModel[] = [];
  public abbonamenti: number = 0;
  public canEdit: boolean = false;
}

export class LicenseCatalogListInModel { }
export class LicenseCatalogListOutModel extends ModelResult {
  public listino: LicenseCatalogModel[] = [];
  public canCreate: boolean = false;
  public isSuperUser: boolean = false;
}

export class LicenseCatalogGetInModel {
  public id: number = 0;
}
export class LicenseCatalogGetOutModel extends ModelResult {
  public listino: LicenseCatalogModel = new LicenseCatalogModel();
  public isSuperUser: boolean = false;
  public dealer: LicenseDealerModel[] = [];
}

export class LicenseCatalogPutInModel {
  public listino: LicenseCatalogModel = new LicenseCatalogModel();
}
export class LicenseCatalogPutOutModel extends ModelResult {
  public listino: LicenseCatalogModel = new LicenseCatalogModel();
}

export class LicenseCatalogDeleteInModel {
  public id: number = 0;
}
export class LicenseCatalogDeleteOutModel extends ModelResult {
  public deleted: boolean = false;
}

// ---------------------------------------------------------------------------
// Listini pubblici (pagina prezzi dello showcase, senza login): solo i listini globali del SuperUser

export class LicensePublicPriceListInModel {
  /** funzioni da mostrare; vuoto = tutte */
  public ambito: AmbitoLicenza[] = [];
}
export class LicensePublicCatalogModel {
  public tipoSoggetto: TipoSoggettoLicenza = TipoSoggettoLicenza.Company;
  public ambito: AmbitoLicenza = AmbitoLicenza.Cumulativo;
  public descrizione: string = '';
  public valuta: string = 'EUR';
  /** listino dello studio valido anche per le sue aziende */
  public disponibileSuCompany: boolean = false;
  /** solo i piani attivi, in ordine */
  public piano: LicensePlanModel[] = [];
}
export class LicensePublicPriceListOutModel extends ModelResult {
  public listino: LicensePublicCatalogModel[] = [];
}

// ---------------------------------------------------------------------------
// Licenze del soggetto (azienda o studio)

export class LicenseSubscriptionModel {
  public id: number = 0;
  public ambito: AmbitoLicenza = AmbitoLicenza.AttendanceTracking;
  public livello: number = 1;
  public descrizionePiano: string | null = null;
  public periodicita: PeriodicitaLicenza = PeriodicitaLicenza.Mensile;
  public prezzo: number = 0;
  public valuta: string = 'EUR';
  public maxUtenti: number | null = null;
  public maxClienti: number | null = null;
  public maxSpazioMB: number | null = null;
  public dataInizio: string = '';
  public dataScadenza: string = '';
  public stato: StatoAbbonamento = StatoAbbonamento.Attivo;
  public rinnovoAutomatico: boolean = false;
  public metodo: MetodoPagamento = MetodoPagamento.Nessuno;
  public disponibileSuCompany: boolean = false;
  public inGrace: boolean = false;
  public inScadenza: boolean = false;
}

export class LicenseFunctionStatusModel {
  public ambito: AmbitoLicenza = AmbitoLicenza.AttendanceTracking;
  public stato: number = 0;
  public livello: number | null = null;
  public descrizionePiano: string | null = null;
  public fonte: FonteCopertura = FonteCopertura.Nessuna;
  public studio: string | null = null;
  public dataScadenza: string | null = null;
  // limite della funzione e uso attuale: utenti (Presenze), clienti attivi (CRM), spazio in MB (Documentale)
  public maxUtenti: number | null = null;
  public maxClienti: number | null = null;
  public maxSpazioMB: number | null = null;
  public clientiAttivi: number | null = null;
  public spazioUsatoMB: number | null = null;
  public oltreLimite: boolean = false;
}

export class LicenseOfferModel {
  public ambito: AmbitoLicenza = AmbitoLicenza.AttendanceTracking;
  public idListino: number = 0;
  public descrizione: string = '';
  public valuta: string = 'EUR';
  public disponibileSuCompany: boolean = false;
  public piano: LicensePlanModel[] = [];
}

export class LicensePaymentModel {
  public id: number = 0;
  public data: string = '';
  public ambito: AmbitoLicenza = AmbitoLicenza.AttendanceTracking;
  public livello: number = 1;
  public importo: number = 0;
  public valuta: string = 'EUR';
  public periodicita: PeriodicitaLicenza = PeriodicitaLicenza.Mensile;
  public dataInizioCoperta: string = '';
  public dataFineCoperta: string = '';
  public metodo: MetodoPagamento = MetodoPagamento.Nessuno;
  public stato: StatoPagamento = StatoPagamento.Creato;
  public debug: boolean = false;
}

export class LicenseCoveredCompanyModel {
  public idCompany: number = 0;
  public azienda: string = '';
  public ambitoSuperiore: number[] = [];
}

export class LicenseStatusGetInModel { }
export class LicenseStatusGetOutModel extends ModelResult {
  public tipoSoggetto: TipoSoggettoLicenza = TipoSoggettoLicenza.Company;
  public soggetto: string = '';
  public studio: string | null = null;
  public attive: boolean = false;
  public canManage: boolean = false;
  public debugPagamenti: boolean = false;
  public metodo: MetodoPagamento[] = [];
  public graceGiorni: number = 0;
  public utentiAttivi: number = 0;
  public limiteUtenti: number | null = null;
  public funzione: LicenseFunctionStatusModel[] = [];
  // stato della licenza Cumulativo (propria o dello studio)
  public cumulativo: LicenseFunctionStatusModel = new LicenseFunctionStatusModel();
  public abbonamento: LicenseSubscriptionModel[] = [];
  public offerta: LicenseOfferModel[] = [];
  public pagamento: LicensePaymentModel[] = [];
  public aziendaCoperta: LicenseCoveredCompanyModel[] = [];
}

export class LicenseSubscribeInModel {
  public ambito: AmbitoLicenza = AmbitoLicenza.AttendanceTracking;
  public livello: number = 1;
  public periodicita: PeriodicitaLicenza = PeriodicitaLicenza.Mensile;
  public metodo: MetodoPagamento = MetodoPagamento.Nessuno;
  public rinnovoAutomatico: boolean = false;
  public returnUrl: string | null = null;
}
export class LicenseSubscribeOutModel extends ModelResult {
  public abbonamento: LicenseSubscriptionModel = new LicenseSubscriptionModel();
  public redirectUrl: string | null = null;
  public debug: boolean = false;
}

export class LicenseRenewInModel {
  public idAbbonamento: number = 0;
  public returnUrl: string | null = null;
}
export class LicenseRenewOutModel extends ModelResult {
  public abbonamento: LicenseSubscriptionModel = new LicenseSubscriptionModel();
  public redirectUrl: string | null = null;
  public debug: boolean = false;
}

export class LicenseAutoRenewPutInModel {
  public idAbbonamento: number = 0;
  public rinnovoAutomatico: boolean = false;
}
export class LicenseAutoRenewPutOutModel extends ModelResult {
  public abbonamento: LicenseSubscriptionModel = new LicenseSubscriptionModel();
}

export class LicenseCancelInModel {
  public idAbbonamento: number = 0;
}
export class LicenseCancelOutModel extends ModelResult {
  public abbonamento: LicenseSubscriptionModel = new LicenseSubscriptionModel();
}
