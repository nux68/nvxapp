import { ModelResult } from "../../../ModelsBase/model-result";

// Timbratura evoluta (AT-TIMB, ANALISI_ATTENDANCETRACKING_TIMBRATURA.md)

export enum RegolaGeofence { Nessuna = 0, Segnala = 1, Blocca = 2 }
export enum OrigineTimbratura { App = 0, Qr = 1, Kiosk = 2 }
export enum EsitoGeofence { NonRichiesto = 0, Dentro = 1, Fuori = 2, PosizioneMancante = 3, LavoroAgile = 4 }

export class Az_SediTimbraturaModel {
  public idAz_Sedi: number = 0;
  public sede: string | null = null;
  public latitudine: number | null = null;
  public longitudine: number | null = null;
  public raggioMetri: number = 200;
  public regolaGeofence: RegolaGeofence = RegolaGeofence.Nessuna;
  public qrAttivo: boolean = false;
  public qrObbligatorio: boolean = false;
  public qrDurataSecondi: number = 30;
  public kioskAttivo: boolean = false;
  public differitaMaxOre: number = 24;
  public kioskRegistrato: boolean = false;
  public rigeneraSegretoQr: boolean = false;
}

export class TimbraturaEvoluta_SediInModel { }
export class TimbraturaEvoluta_SediOutModel extends ModelResult {
  public sedi: Az_SediTimbraturaModel[] = [];
  public idAz_Sedi_Proposta: number | null = null;
}
export class TimbraturaEvoluta_ConfigGetInModel { public idAz_Sedi: number = 0; }
export class TimbraturaEvoluta_ConfigGetOutModel extends ModelResult { public config: Az_SediTimbraturaModel; }
export class TimbraturaEvoluta_ConfigPutInModel { public config: Az_SediTimbraturaModel; }
export class TimbraturaEvoluta_ConfigPutOutModel extends ModelResult { public config: Az_SediTimbraturaModel; }

export class TimbraturaEvoluta_QrCodeInModel { public idAz_Sedi: number = 0; public origine: string | null = null; }
export class TimbraturaEvoluta_QrCodeOutModel extends ModelResult {
  public codice: string = '';
  public url: string = '';
  public immaginePng: string = '';
  public scadeTraSecondi: number = 0;
  public durataSecondi: number = 0;
}

export class TimbraturaEvoluta_StampInModel {
  // reparto-attivita' scelti tra le opzioni della sede (0 = l'opzione di default)
  public idAz_SubCommessaAttivita: number = 0;
  public idAz_SediReparto: number | null = null;
  public codice: string | null = null;
  // ticket di Opzioni (codice del QR gia' verificato): sostituisce il codice, la scelta non fa scadere il QR
  public ticket: string | null = null;
  public idAz_Sedi: number | null = null;
  public latitudine: number | null = null;
  public longitudine: number | null = null;
  public precisioneMetri: number | null = null;
  public offline: boolean = false;
  public dataDispositivo: string | null = null;
}

export class Dip_GG_TimbraturaInfoModel {
  public idDip_GG_Timbratura: number = 0;
  public timbratura: string = '';
  public idAz_Sedi: number | null = null;
  public sede: string | null = null;
  public idAz_SediReparto: number | null = null;
  public reparto: string | null = null;
  public origine: OrigineTimbratura = OrigineTimbratura.App;
  public differita: boolean = false;
  public dataDispositivo: string | null = null;
  public dataRicezione: string = '';
  public distanzaMetri: number | null = null;
  public esitoGeofence: EsitoGeofence = EsitoGeofence.NonRichiesto;
  public operatore: string | null = null;
}
export class TimbraturaEvoluta_StampOutModel extends ModelResult { public info: Dip_GG_TimbraturaInfoModel | null = null; }

// reparto - attivita' della sede compatibili con il dipendente (AT-TIMB.7)
export class TimbraturaOpzioneModel {
  public idAz_SediReparto: number | null = null;
  public reparto: string | null = null;
  public idAz_SubCommessaAttivita: number = 0;
  public attivita: string = '';
  public commessa: string | null = null;
}
export class TimbraturaEvoluta_OpzioniInModel {
  public codice: string | null = null;
  public idAz_Sedi: number | null = null;
}
export class TimbraturaEvoluta_OpzioniOutModel extends ModelResult {
  public idAz_Sedi: number | null = null;
  public ticket: string | null = null;
  public opzioni: TimbraturaOpzioneModel[] = [];
  public indiceDefault: number = 0;
}

export class TimbraturaEvoluta_KioskRegistraInModel { public idAz_Sedi: number = 0; }
export class TimbraturaEvoluta_KioskRegistraOutModel extends ModelResult { public kioskToken: string = ''; }
// uscita dalla modalita' terminale con la password di un CompanyPowerAdmin dell'azienda
export class TimbraturaEvoluta_TerminaleSbloccaInModel { public password: string | null = null; }
export class TimbraturaEvoluta_TerminaleSbloccaOutModel extends ModelResult { public sbloccato: boolean = false; }
// kiosk: PIN o badge -> dipendente e opzioni reparto-attivita' della sede del kiosk
export class TimbraturaEvoluta_KioskIdentificaInModel {
  public idAz_Sedi: number = 0;
  public kioskToken: string | null = null;
  public pin: string | null = null;
  public badge: string | null = null;
}
export class TimbraturaEvoluta_KioskIdentificaOutModel extends ModelResult {
  public nominativo: string | null = null;
  public opzioni: TimbraturaOpzioneModel[] = [];
  public indiceDefault: number = 0;
}
export class TimbraturaEvoluta_KioskStampInModel extends TimbraturaEvoluta_KioskIdentificaInModel {
  // reparto-attivita' scelti (0 = l'opzione di default)
  public idAz_SubCommessaAttivita: number = 0;
  public idAz_SediReparto: number | null = null;
}
export class TimbraturaEvoluta_KioskStampOutModel extends ModelResult {
  public nominativo: string | null = null;
  public ora: string = '';
  public info: Dip_GG_TimbraturaInfoModel | null = null;
}

export class Dip_TimbraturaPinModel {
  public idAspNetUsers: string = '';
  public nominativo: string | null = null;
  public pinImpostato: boolean = false;
  public badge: string | null = null;
}
export class TimbraturaEvoluta_PinGetAllInModel { }
export class TimbraturaEvoluta_PinGetAllOutModel extends ModelResult { public dipendenti: Dip_TimbraturaPinModel[] = []; }
export class TimbraturaEvoluta_PinPutInModel {
  public idAspNetUsers: string | null = null;
  public pin: string | null = null;     // null = non cambia, '' = toglie
  public badge: string | null = null;
}
export class TimbraturaEvoluta_PinPutOutModel extends ModelResult { }
