import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { AccountService } from '../../ClientServer-Service/Infrastructure/Account/account.service';
import { UserCompanyEditModel, UserCompanyPutInModel } from '../../ClientServer-Service/Infrastructure/Account/Models/user-company-model';
import { DipAnagraficaService } from '../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/dip-anagrafica.service';
import { Dip_Anagrafica_Get_InModel, Dip_Anagrafica_Put_InModel } from '../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model';
import { DestinazioneEccedenze, TipoContratto } from '../../ClientServer-Service/GestionePresenze/Dip_RapportoLavoro/Models/dip-rapporto-lavoro-model';
import { Dip_ProfiloOrarioModel } from '../../ClientServer-Service/GestionePresenze/Dip_ProfiloOrario/Models/dip-profilo-orario-model';
import { Dip_CompetenzaModel } from '../../ClientServer-Service/GestionePresenze/Dip_Competenza/Models/dip-competenza-model';
import { Dip_Rapporto_Giustificativi_MaturazioneModel } from '../../ClientServer-Service/GestionePresenze/Dip_Rapporto_Giustificativi_Maturazione/Models/dip-rapporto-giustificativi-maturazione-model';
import { SharedParameterGestionePresenzeService } from '../../shared/shared-parameter-gestione-presenze.service';
import { normalizzaCodiceFiscale } from './codice-fiscale';

/** dati del primo step del wizard: l'utente */
export interface NuovoDipendenteUtente {
  cognome: string;
  nome: string;
  /** facoltativi */
  matricola?: string | null;
  codiceFiscale?: string | null;
  /** username */
  descrizione: string;
  pw: string;
  mail: string;
  roles: string[];
}

/** dati degli step 2-5 del wizard */
export interface NuovoDipendenteRapporto {
  rapporto: { dataAss: string; tipoContratto: TipoContratto; oreSettimanaliContratto: number | string | null; ccnl: string | null; destinazioneEccedenze: DestinazioneEccedenze };
  profilo: { idPar_ProfiloOrario: number | null; numGiornoPartenzaCiclo: number | string };
  competenze: number[];
  /** ore di maturazione mensile per giustificativo-contatore, "HHH:MM" (vuote o zero = nessuna) */
  maturazioni: { [idPar_Giustificativi: number]: string };
}

export interface NuovoDipendenteEsito {
  /** utente creato (valorizzato anche se poi non si sono salvati i dati delle presenze) */
  idAspNetUsers: string | null;
  /** inserimento completo */
  ok: boolean;
  errore: string | null;
}

/**
 * Inserimento di un nuovo dipendente: crea l'utente nell'azienda (il server prepara anagrafica, rapporto e
 * profilo di default) e poi gli applica i dati delle presenze. Lo usano il wizard di inserimento e
 * l'importazione da file (che usa i valori di default per tutto tranne l'utente).
 */
@Injectable({
  providedIn: 'root'
})
export class NuovoDipendenteService {

  constructor(private accountService: AccountService,
              private dipAnagraficaService: DipAnagraficaService,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private transloco: TranslocoService) { }

  /** valori proposti per gli step 2-5: assunzione oggi, full time, profilo DAY7H8, competenze di default, nessuna maturazione */
  Default(): NuovoDipendenteRapporto {
    const profili = this.sharedParameterGestionePresenzeService.Par_ProfiloOrario ?? [];
    const profiloDefault = profili.find(p => p.codice === 'DAY7H8') ?? profili[0];
    return {
      rapporto: { dataAss: oggiIso(), tipoContratto: TipoContratto.FullTime, oreSettimanaliContratto: null, ccnl: null, destinazioneEccedenze: DestinazioneEccedenze.Pagamento },
      profilo: { idPar_ProfiloOrario: profiloDefault?.id ?? null, numGiornoPartenzaCiclo: 1 },
      competenze: (this.sharedParameterGestionePresenzeService.Par_Competenza ?? []).filter(c => c.default).map(c => c.id),
      maturazioni: {},
    };
  }

  async Inserisci(utente: NuovoDipendenteUtente, dati: NuovoDipendenteRapporto): Promise<NuovoDipendenteEsito> {
    const esito: NuovoDipendenteEsito = { idAspNetUsers: null, ok: false, errore: null };
    try {
      const request = new GenericRequest<UserCompanyPutInModel>(UserCompanyPutInModel);
      request.data.userCompanyEdit = new UserCompanyEditModel(utente.descrizione, 0, '', false, utente.mail, utente.pw, utente.roles);

      const res = await firstValueFrom(this.accountService.UserCompanyPut(request));
      esito.idAspNetUsers = res?.success ? res.data?.userCompanyEdit?.idAspNetUsers ?? null : null;
      if (!esito.idAspNetUsers) {
        esito.errore = res?.messages?.map(m => m.text).join(' ') || this.t('creazioneUtenteNonRiuscita');
        return esito;
      }

      esito.errore = await this.SalvaDatiPresenze(esito.idAspNetUsers, utente, dati);
      esito.ok = esito.errore === null;
    } catch (error) {
      console.error('Errore durante la chiamata API:', error);
      esito.errore = this.t('erroreSalvataggio');
    }
    return esito;
  }

  /** Applica ai dati creati dal server quelli degli step 2-5 e li salva; restituisce l'errore (null se riuscito). */
  private async SalvaDatiPresenze(idAspNetUsers: string, utente: NuovoDipendenteUtente, dati: NuovoDipendenteRapporto): Promise<string | null> {
    const reqGet = new GenericRequest<Dip_Anagrafica_Get_InModel>(Dip_Anagrafica_Get_InModel);
    reqGet.data.id = idAspNetUsers;
    const resGet = await firstValueFrom(this.dipAnagraficaService.Dip_AnagraficaGet(reqGet));
    const dip = resGet?.data?.dip_Anagrafica;
    const rapporto = dip?.dip_RapportoLavoro?.[0];
    if (!dip || !rapporto)
      return this.t('datiDipendenteNonDisponibili');

    // step 1
    dip.cognome = utente.cognome;
    dip.nome = utente.nome;
    dip.matricola = utente.matricola?.trim() || null;
    dip.codiceFiscale = normalizzaCodiceFiscale(utente.codiceFiscale) || null;
    dip.pw = '';

    // step 2: rapporto di lavoro (la data di licenziamento resta vuota)
    const r = dati.rapporto;
    rapporto.dataAss = r.dataAss;
    rapporto.tipoContratto = r.tipoContratto;
    rapporto.oreSettimanaliContratto = r.tipoContratto === TipoContratto.PartTime && r.oreSettimanaliContratto !== null && r.oreSettimanaliContratto !== ''
      ? +r.oreSettimanaliContratto : null;
    rapporto.ccnl = r.ccnl || null;
    rapporto.destinazioneEccedenze = r.destinazioneEccedenze;

    // step 3: profilo orario dalla data di assunzione (si aggiorna quello di default creato dal server)
    const p = dati.profilo;
    let profilo = dip.dip_ProfiloOrario.find(x => x.idDip_RapportoLavoro === rapporto.id);
    if (!profilo) {
      profilo = new Dip_ProfiloOrarioModel();
      profilo.id = -1;
      profilo.idDip_RapportoLavoro = rapporto.id;
      profilo.al = '2099-12-31T00:00:00';
      dip.dip_ProfiloOrario.push(profilo);
    }
    profilo.idPar_ProfiloOrario = p.idPar_ProfiloOrario;
    profilo.numGiornoPartenzaCiclo = +p.numGiornoPartenzaCiclo;
    profilo.dal = r.dataAss;
    dip.dip_ProfiloOrario = dip.dip_ProfiloOrario.filter(x => x.idDip_RapportoLavoro !== rapporto.id || x === profilo);

    // step 4: competenze (senza competenze il server assegna quella di default)
    dip.dip_Competenza = dati.competenze.map(idPar_Competenza => {
      const c = new Dip_CompetenzaModel();
      c.id = 0;
      c.idDip_Anagrafica = dip.id;
      c.idPar_Competenza = idPar_Competenza;
      return c;
    });

    // step 5: maturazione mensile, solo i giustificativi valorizzati
    dip.dip_Maturazione = Object.entries(dati.maturazioni)
      .filter(([, ore]) => ore && ore !== '00:00' && ore !== '0:00')
      .map(([idPar_Giustificativi, ore]) => {
        const m = new Dip_Rapporto_Giustificativi_MaturazioneModel();
        m.idDip_RapportoLavoro = rapporto.id;
        m.idPar_Giustificativi = +idPar_Giustificativi;
        m.oreMaturazione = ore;
        return m;
      });

    const reqPut = new GenericRequest<Dip_Anagrafica_Put_InModel>(Dip_Anagrafica_Put_InModel);
    reqPut.data.dip_Anagrafica = dip;
    reqPut.data.id = dip.idAspNetUsers;
    const resPut = await firstValueFrom(this.dipAnagraficaService.Dip_AnagraficaPut(reqPut));
    if (!resPut?.success)
      return resPut?.messages?.map(m => m.text).join(' ') || this.t('salvataggioDipendenteNonRiuscito');
    return null;
  }

  private t(key: string): string {
    return this.transloco.translate('attendance.components.userAttendanceWizardDialog.' + key);
  }
}

/** Oggi nel formato delle date del rapporto ("YYYY-MM-DDT00:00:00", senza fuso orario). */
export function oggiIso(): string {
  const d = new Date();
  const mm = String(d.getMonth() + 1).padStart(2, '0');
  const dd = String(d.getDate()).padStart(2, '0');
  return `${d.getFullYear()}-${mm}-${dd}T00:00:00`;
}
