import { ModelResult } from '../../ModelsBase/model-result';

export class HrEmptyInModel { }
export class HrIdInModel { public id = 0; }
export class HrIdOutModel extends ModelResult { public id = 0; }
export class HrJobOutModel extends ModelResult { public jobId = ''; }

export class HrPermessiModel { public powerAdmin = false; public admin = false; }

// ---------------------------------------------------------------- configurazione
export class HrCfgModel {
  public giorniPreavviso = 30;
  public notificheAttive = true;
  public note: string | null = null;
}

export class HrCfgGetOutModel extends ModelResult { public permessi = new HrPermessiModel(); public cfg = new HrCfgModel(); }
export class HrCfgPutInModel { public cfg = new HrCfgModel(); }
export class HrCfgPutOutModel extends ModelResult { public cfg = new HrCfgModel(); }

// ---------------------------------------------------------------- dipendenti (anagrafica comune)
export class HrDipendenteBreveModel { public id = 0; public cognome = ''; public nome = ''; public matricola: string | null = null; public codiceFiscale: string | null = null; public attivo = true; }
export class HrAccountModel { public presente = false; public userName: string | null = null; public mail: string | null = null; public disabilitato = false; }
export class HrDipendenteModel {
  public id = 0;
  public cognome: string | null = null;
  public nome: string | null = null;
  public matricola: string | null = null;
  public codiceFiscale: string | null = null;
  public email: string | null = null;
  public dataAssunzione: string | null = null;
  public dataCessazione: string | null = null;
  public attivo = true;
  public account = new HrAccountModel();
}
export class HrDipendenteListInModel { public soloAttivi = false; }
export class HrDipendenteListOutModel extends ModelResult { public dipendenti: HrDipendenteModel[] = []; }
export class HrDipendenteGetOutModel extends ModelResult { public dipendente = new HrDipendenteModel(); }
export class HrDipendentePutInModel { public dipendente = new HrDipendenteModel(); }
export class HrDipendentePutOutModel extends ModelResult { public dipendente = new HrDipendenteModel(); }
export class HrAccountCreaInModel { public idDip_Anagrafica = 0; public userName = ''; public mail: string | null = null; public password = ''; public idHr_ChecklistVoce: number | null = null; }
export class HrAccountDisattivaInModel { public idDip_Anagrafica = 0; public idHr_ChecklistVoce: number | null = null; }
export class HrAccountOutModel extends ModelResult { public account = new HrAccountModel(); }

// ---------------------------------------------------------------- cedolini e CU
export enum HrTipoDocumento { Cedolino = 1, Cu = 2 }
export enum HrStatoCaricamento { InElaborazione = 0, DaVerificare = 1, DaPubblicare = 2, Pubblicato = 3, Errore = 9 }

export class HrCedAziendaInModel { public idCompany: number | null = null; }
export class HrCedIdInModel extends HrCedAziendaInModel { public id = 0; }
export class HrStudioAziendaModel { public idCompany = 0; public descrizione = ''; }
export class HrStudioAziendeOutModel extends ModelResult { public aziende: HrStudioAziendaModel[] = []; }

export class HrCaricamentoModel {
  public id = 0;
  public tipo = HrTipoDocumento.Cedolino;
  public anno = 0;
  public mese: number | null = null;
  public origine = 1;
  public nomeFile = '';
  public stato = HrStatoCaricamento.InElaborazione;
  public numeroPagine = 0;
  public numeroDocumenti = 0;
  public numeroScarti = 0;
  public numeroPubblicati = 0;
  public messaggio: string | null = null;
  public dataCaricamento: string | null = null;
  public dataPubblicazione: string | null = null;
}
export class HrCaricamentoListInModel extends HrCedAziendaInModel { public anno: number | null = null; }
export class HrCaricamentoListOutModel extends ModelResult { public caricamenti: HrCaricamentoModel[] = []; }
export class HrCaricamentoUploadModel extends HrCedAziendaInModel { public tipo = HrTipoDocumento.Cedolino; public anno = new Date().getFullYear(); public mese: number | null = new Date().getMonth() + 1; }
export class HrCaricamentoUploadOutModel extends ModelResult { public caricamento = new HrCaricamentoModel(); public jobId = ''; }

export class HrDocDipendenteModel {
  public id = 0;
  public idDip_Anagrafica = 0;
  public dipendente = '';
  public codiceFiscale: string | null = null;
  public matricola: string | null = null;
  public pagine = '';
  public metodo = 1;
  public dataPubblicazione: string | null = null;
  public senzaUtente = false;
  public dataRichiesta: string | null = null;
  public dataConferma: string | null = null;
  public canale: string | null = null;
}
export class HrDocScartoModel {
  public id = 0;
  public pagina = 0;
  public testo: string | null = null;
  public motivo: string | null = null;
  public idDip_AnagraficaProposta: number | null = null;
  public proposta: string | null = null;
  public esito = 0;
  public idDip_Anagrafica: number | null = null;
  public dipendente: string | null = null;
}
export class HrConfermeModel { public pubblicati = 0; public confermati = 0; public daConfermare = 0; public senzaUtente = 0; }
export class HrCaricamentoGetOutModel extends ModelResult {
  public caricamento = new HrCaricamentoModel();
  public documenti: HrDocDipendenteModel[] = [];
  public scarti: HrDocScartoModel[] = [];
  public dipendenti: HrDipendenteBreveModel[] = [];
  public conferme = new HrConfermeModel();
}
export class HrScartoAssegnaInModel extends HrCedAziendaInModel { public idScarto = 0; public idDip_Anagrafica = 0; }

export class HrMioDocumentoModel {
  public id = 0;
  public tipo = HrTipoDocumento.Cedolino;
  public anno = 0;
  public mese: number | null = null;
  public dataPubblicazione: string | null = null;
  public idPresaVisione: number | null = null;
  public dataConferma: string | null = null;
  public confermabileQui = false;
}
export class HrMieiDocumentiOutModel extends ModelResult { public dipendente = false; public documenti: HrMioDocumentoModel[] = []; }

// ---------------------------------------------------------------- formazione
export enum HrEsitoCorso { Iscritto = 0, Superato = 1, NonSuperato = 2, Assente = 3 }

export class HrCorsoModel {
  public id = 0;
  public titolo = '';
  public ente: string | null = null;
  public durataOre: number | null = null;
  public validitaMesi: number | null = null;
  public idPar_Competenza: number | null = null;
  public competenza: string | null = null;
  public obbligatorio = false;
  public attivo = true;
  public note: string | null = null;
  public numeroEdizioni = 0;
}
export class HrEdizioneBreveModel { public id = 0; public dataInizio = ''; public dataFine: string | null = null; public sede: string | null = null; public docente: string | null = null; public numeroPartecipanti = 0; public numeroSuperati = 0; }
export class HrCorsoListOutModel extends ModelResult { public corsi: HrCorsoModel[] = []; }
export class HrCorsoGetOutModel extends ModelResult { public corso = new HrCorsoModel(); public edizioni: HrEdizioneBreveModel[] = []; }
export class HrCorsoPutInModel { public corso = new HrCorsoModel(); }
export class HrCorsoPutOutModel extends ModelResult { public corso = new HrCorsoModel(); }

export class HrPartecipanteModel {
  public id = 0;
  public idDip_Anagrafica = 0;
  public dipendente: string | null = null;
  public esito = HrEsitoCorso.Iscritto;
  public dataConseguimento: string | null = null;
  public dataScadenza: string | null = null;
  public attestatoNomeFile: string | null = null;
}
export class HrEdizioneModel {
  public id = 0;
  public idHr_Corso = 0;
  public dataInizio = '';
  public dataFine: string | null = null;
  public idAz_Sedi: number | null = null;
  public luogo: string | null = null;
  public docente: string | null = null;
  public note: string | null = null;
  public partecipanti: HrPartecipanteModel[] = [];
}
export class HrEdizioneGetInModel { public id = 0; public idHr_Corso = 0; }
export class HrEdizioneGetOutModel extends ModelResult { public corso = ''; public validitaMesi: number | null = null; public edizione = new HrEdizioneModel(); public dipendenti: HrDipendenteBreveModel[] = []; }
export class HrEdizionePutInModel { public edizione = new HrEdizioneModel(); }
export class HrEdizionePutOutModel extends ModelResult { public edizione = new HrEdizioneModel(); }
export class HrAttestatoUploadModel { public idPartecipante = 0; }
export class HrAttestatoUploadOutModel extends ModelResult { public partecipante = new HrPartecipanteModel(); }

export class HrPianoRigaModel {
  public id = 0;
  public idHr_Corso = 0;
  public corso: string | null = null;
  public idDip_Anagrafica = 0;
  public dipendente: string | null = null;
  public entroIl: string | null = null;
  public note: string | null = null;
  public svolto = false;
  public dataSvolto: string | null = null;
}
export class HrPianoModel {
  public id = 0;
  public anno = new Date().getFullYear();
  public descrizione = '';
  public note: string | null = null;
  public numeroRighe = 0;
  public numeroSvolte = 0;
  public righe: HrPianoRigaModel[] = [];
}
export class HrPianoListInModel { public anno: number | null = null; }
export class HrPianoListOutModel extends ModelResult { public piani: HrPianoModel[] = []; }
export class HrPianoGetOutModel extends ModelResult { public piano = new HrPianoModel(); public corsi: HrCorsoModel[] = []; public dipendenti: HrDipendenteBreveModel[] = []; }
export class HrPianoPutInModel { public piano = new HrPianoModel(); }
export class HrPianoPutOutModel extends ModelResult { public piano = new HrPianoModel(); }

// ---------------------------------------------------------------- onboarding e offboarding
export enum HrTipoChecklist { Assunzione = 1, Cessazione = 2 }
export enum HrStatoChecklist { Aperta = 0, Completata = 1, Annullata = 2 }
export enum HrAzioneVoce { Nessuna = 0, CreaAccount = 1, DisattivaAccount = 2 }

export class HrUtenteModel { public id = ''; public nome = ''; }
export class HrModelloVoceModel { public id = 0; public ordine = 0; public descrizione = ''; public idAspNetUsersResponsabile: string | null = null; public giorni = 0; public documento = false; public azione = HrAzioneVoce.Nessuna; }
export class HrModelloModel { public id = 0; public tipo = HrTipoChecklist.Assunzione; public descrizione = ''; public predefinito = false; public attivo = true; public numeroVoci = 0; public voci: HrModelloVoceModel[] = []; }
export class HrModelloListOutModel extends ModelResult { public modelli: HrModelloModel[] = []; }
export class HrModelloGetOutModel extends ModelResult { public modello = new HrModelloModel(); public utenti: HrUtenteModel[] = []; }
export class HrModelloPutInModel { public modello = new HrModelloModel(); }
export class HrModelloPutOutModel extends ModelResult { public modello = new HrModelloModel(); }

export class HrChecklistModel {
  public id = 0;
  public idDip_Anagrafica = 0;
  public dipendente = '';
  public tipo = HrTipoChecklist.Assunzione;
  public descrizione = '';
  public dataRiferimento = '';
  public stato = HrStatoChecklist.Aperta;
  public note: string | null = null;
  public numeroVoci = 0;
  public numeroCompletate = 0;
  public prossimaScadenza: string | null = null;
}
export class HrChecklistVoceModel {
  public id = 0;
  public ordine = 0;
  public descrizione = '';
  public idAspNetUsersResponsabile: string | null = null;
  public responsabile: string | null = null;
  public dataPrevista = '';
  public completata = false;
  public dataCompletamento: string | null = null;
  public documento = false;
  public documentoNomeFile: string | null = null;
  public dataRichiestaPresaVisione: string | null = null;
  public dataConferma: string | null = null;
  public azione = HrAzioneVoce.Nessuna;
  public note: string | null = null;
}
export class HrChecklistListInModel { public soloAperte = true; }
export class HrChecklistListOutModel extends ModelResult { public checklist: HrChecklistModel[] = []; public modelli: HrModelloModel[] = []; public dipendenti: HrDipendenteBreveModel[] = []; }
export class HrChecklistGetOutModel extends ModelResult { public checklist = new HrChecklistModel(); public voci: HrChecklistVoceModel[] = []; public account = new HrAccountModel(); public utenti: HrUtenteModel[] = []; }
export class HrChecklistCreaInModel { public idDip_Anagrafica = 0; public idHr_ChecklistModello = 0; public dataRiferimento: string | null = null; }
export class HrChecklistPutInModel { public id = 0; public stato = HrStatoChecklist.Aperta; public note: string | null = null; }
export class HrChecklistVocePutInModel { public voce = new HrChecklistVoceModel(); }
export class HrChecklistVoceOutModel extends ModelResult { public voce = new HrChecklistVoceModel(); }
export class HrVoceDocUploadModel { public idVoce = 0; }

// ---------------------------------------------------------------- widget della dashboard (ANALISI_HUMANRESOURCES_WIDGET.md)
export class HrWidgetVoceModel {
  public idHr_Checklist = 0;
  public dipendente = '';
  public tipo = 0;
  public descrizione = '';
  public dataPrevista = '';
}
export class HrWidgetOnboardingGetOutModel extends ModelResult {
  public checklistAperte = 0;
  public assunzioni = 0;
  public cessazioni = 0;
  public vociScadute = 0;
  public vociProssime = 0;
  public giorniProssime = 7;
  public voci: HrWidgetVoceModel[] = [];
}
export class HrWidgetAttestatoModel {
  public idHr_Corso = 0;
  public corso = '';
  public dipendente = '';
  public dataScadenza = '';
  public obbligatorio = false;
}
export class HrWidgetFormazioneGetOutModel extends ModelResult {
  public scaduti = 0;
  public entro30 = 0;
  public entro60 = 0;
  public giorniFinestra = 60;
  public attestati: HrWidgetAttestatoModel[] = [];
}
