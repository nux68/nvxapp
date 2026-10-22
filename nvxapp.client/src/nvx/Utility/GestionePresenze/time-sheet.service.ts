import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { LanguageService } from '../infrastructure/language.service';
import { catchError, Observable, of, switchMap, throwError } from 'rxjs';
import { map } from 'rxjs/operators'; // Import map operator if you plan real sorting/processing
import { Dip_GG_Timbratura_GetAll_InModel, Dip_GG_TimbraturaModel, TipoTimbratura } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { HoverPopupData } from '../../shared/components/infrastructure/hover-popup/hover-popup.component';
import { Dip_GG_Richiesta_GetAll4User_InModel, Dip_GG_RichiestaModel, riepilogoNotaSpesa, StatoRichiesta, TipoRichiesta } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { Dip_GG_Giustificativi_GetAll_InModel, Dip_GG_GiustificativiModel } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';
import { DipGGGiustificativiService } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/dip-gg-giustificativi.service';
import { DipGGTimbraturaService } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/dip-gg-timbratura.service';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { forkJoin } from 'rxjs/internal/observable/forkJoin';
import { DipGGRichiestaService } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { FuoriRapporto, MonthData, PeriodoRapporto, SegmentoItem, SegmentoRichiesta, TimeSheetRemoteData } from './time-sheet-common-data';
import { SharedParameterGestionePresenzeService } from '../../shared/shared-parameter-gestione-presenze.service';
import { DateTimeUtilService } from '../infrastructure/date-time-util.service';
import { TipoRichiestaToShortTextPipe } from '../../shared/pipe/GestionePresenze/tipo-richiesta-to-short-text.pipe';
import { StatoRichiestaLongTextPipe } from '../../shared/pipe/GestionePresenze/stato-richiesta-long-text.pipe';
import { TipoTimbraturaToLongTextPipe } from '../../shared/pipe/GestionePresenze/tipo-timbratura-to-long-text.pipe';
import { OrariSchema_4User_OutModel, Timesheet_AllData_InModel } from '../../ClientServer-Service/GestionePresenze/TimeSheet_EngineService/Models/time-sheet-engine-model';
import { TimeSheetEngineService } from '../../ClientServer-Service/GestionePresenze/TimeSheet_EngineService/time-sheet-engine.service';
import { Dip_GG_ResultModel, GG_ResultStato } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Result/Models/dip-gg-result-model';
import { minutiToHHMM } from '../../ClientServer-Service/GestionePresenze/Dip_Periodo/Models/dip-periodo-model';
import { Dip_GG_CausaliModel } from '../../ClientServer-Service/GestionePresenze/Dip_GG_Causali/Models/dip-gg-causali-model';
import { Contatori_Anno_InModel, Contatori_Anno_MeseResult } from '../../ClientServer-Service/GestionePresenze/Contatori/Models/contatori-model';
import { ContatoriService } from '../../ClientServer-Service/GestionePresenze/Contatori/contatori.service';
import { TipoContatore } from '../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';





@Injectable({
  providedIn: 'root'
})
export class TimeSheetService {

  constructor(private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private dipGGGiustificativiService: DipGGGiustificativiService,
              private dipGGTimbraturaService: DipGGTimbraturaService,
              private dipGGRichiestaService: DipGGRichiestaService,
              private contatoriService: ContatoriService,
              public timeSheetEngineService: TimeSheetEngineService,
              public dateTimeUtilService: DateTimeUtilService,
              private transloco: TranslocoService,
              private languageService: LanguageService) {
  }

  private t(key: string, params?: Record<string, unknown>): string {
    return this.transloco.translate('attendance.services.timeSheet.' + key, params);
  }

  /** "giovedì 05 marzo" nella lingua dell'interfaccia */
  private giornoLungo(data: string | Date): string {
    return new Date(data).toLocaleDateString(this.languageService.locale, { weekday: 'long', day: '2-digit', month: 'long' });
  }

  /** "08:30" nella lingua dell'interfaccia */
  private oraBreve(data: string | Date): string {
    return new Date(data).toLocaleTimeString(this.languageService.locale, { hour: '2-digit', minute: '2-digit' });
  }


  getMonthData(year: number, month: number, idAspNetUsers: string|null,loadCounter:boolean): Observable<MonthData> {

    return this.getMonthDataFromServer(year, month, idAspNetUsers, loadCounter).pipe(
      map(remoteData => this.transformRemoteDataToMonthData(remoteData, year, month)),
      catchError(error => {
        console.warn(`Error fetching data from server: ${error}. Falling back to mock data.`);
          return of({
            year: year,
            month: month,
            days: {},
            dip_GG_Richiesta: [],
            daySlot: [],
            contatori_Anno_Mese:[] 
          });
      })
    );
  }

  getMonthDataFromServer(year: number, month: number, idAspNetUsers: string | null, loadCounter: boolean): Observable<TimeSheetRemoteData> {

    if (year == undefined || month == undefined) {
      const remoteData: TimeSheetRemoteData = {
        dip_GG_Giustificativi: [],
        dip_GG_Timbratura: [],
        dip_GG_Richiesta: [],
        dip_GG_Result: [],
        dip_GG_Causali: [],
        daySlot: [],
        contatori_Anno_Mese:[],
      };

      return of(remoteData);
    }

    let request: GenericRequest<Timesheet_AllData_InModel> = new GenericRequest<Timesheet_AllData_InModel>(Timesheet_AllData_InModel);
    request.data = new Timesheet_AllData_InModel();
    request.data.dal = new Date(Date.UTC(year, month, 1)).toISOString();
    request.data.al = new Date(Date.UTC(year, month + 1, 0)).toISOString();
    if (idAspNetUsers != null) {
      request.data.usersId = [idAspNetUsers];
    }



    
    return this.timeSheetEngineService.Get_Timesheet_AllData(request).pipe(
      switchMap(x => {
        // Prepariamo l'oggetto base
        let remoteData: TimeSheetRemoteData = {
          dip_GG_Giustificativi: x.data?.dip_GG_AllData_OutModel?.dip_GG_Giustificativi ?? [],
          dip_GG_Timbratura: x.data?.dip_GG_AllData_OutModel?.dip_GG_Timbratura ?? [],
          dip_GG_Richiesta: x.data?.dip_GG_AllData_OutModel?.dip_GG_Richiesta ?? [],
          dip_GG_Result: x.data?.dip_GG_AllData_OutModel?.dip_GG_Result ?? [],
          dip_GG_Causali: x.data?.dip_GG_AllData_OutModel?.dip_GG_Causali ?? [],
          daySlot: x.data?.orariSchema_4User_OutModel?.daySlots ?? [],
          contatori_Anno_Mese: [],
          periodiRapporto: this.periodiRapporto(x.data?.orariSchema_4User_OutModel, idAspNetUsers)
        };

        // Se non abbiamo l'ID utente, restituiamo subito i dati base avvolti in un Observable
        if (idAspNetUsers == null) {
          return of(remoteData);
        }


        // senza giorni coperti dal rapporto (es. mese prima dell'assunzione) non ci sono contatori da leggere
        if (loadCounter == false || remoteData.daySlot.length === 0) {
          return of(remoteData);
        }
        else {
          // Altrimenti, prepariamo la seconda chiamata
          const req = new GenericRequest<Contatori_Anno_InModel>(Contatori_Anno_InModel);
          req.data.idDip_RapportoLavoro = remoteData.daySlot[0].idDip_RapportoLavoro; // Verifica se questo deve essere dinamico
          req.data.anno = year;

          // Usiamo map per unire i risultati dei contatori a remoteData
          return this.contatoriService.CalcolaContatori_Anno(req).pipe(
            map(resContatori => {
              // Supponendo che i dati siano in resContatori.data
              remoteData.contatori_Anno_Mese = resContatori.data.mesi ?? [];
              return remoteData;
            })
          );
        }

       


      })
    );


  }
    
  /**
   * Periodi (assunzione..cessazione) dei rapporti dell'utente del cartellino.
   * Senza utente indicato vale l'unica anagrafica restituita (cartellino del dipendente stesso);
   * null se il cartellino è di più utenti.
   */
  private periodiRapporto(orari: OrariSchema_4User_OutModel | undefined, idAspNetUsers: string | null): PeriodoRapporto[] | null {
    if (!orari) return null;
    const anagrafiche = idAspNetUsers != null
      ? orari.dip_Anagrafica.filter(a => a.idAspNetUsers === idAspNetUsers)
      : orari.dip_Anagrafica;
    if (anagrafiche.length !== 1) return null;
    return (orari.dip_RapportoLavoro_Tutti ?? [])
      .filter(r => r.idDip_Anagrafica === anagrafiche[0].id && r.dataAss)
      .map(r => ({ dal: r.dataAss.substring(0, 10), al: r.dataLic ? r.dataLic.substring(0, 10) : null }))
      .sort((a, b) => a.dal.localeCompare(b.dal));
  }

  /** Dove cade il giorno rispetto ai rapporti di lavoro: null se è dentro un rapporto (o se non si sa). */
  fuoriRapporto(currentMonth: MonthData, data: Date | string): FuoriRapporto | null {
    const periodi = currentMonth?.periodiRapporto;
    if (!periodi) return null;
    const giorno = this.giornoIso(data);
    if (periodi.some(p => p.dal <= giorno && (p.al == null || giorno <= p.al))) return null;
    if (periodi.length === 0 || giorno < periodi[0].dal) return 'prima';
    if (periodi.every(p => p.al != null && p.al < giorno)) return 'dopo';
    return 'tra';
  }

  /**
   * Posizione di un giustificativo nella sua richiesta su più giorni ('inizio' | 'mezzo' | 'fine'),
   * null se la richiesta copre un solo giorno. `tutti` sono i giustificativi visibili nella pagina,
   * `inizioVista`/`fineVista` ("YYYY-MM-DD") il periodo mostrato: se la richiesta continua oltre,
   * il pezzo sul bordo resta aperto da quel lato.
   */
  segmentoRichiesta(g: SegmentoItem, tutti: SegmentoItem[], richiesta: Dip_GG_RichiestaModel | null | undefined,
                    inizioVista: string, fineVista: string): SegmentoRichiesta | null {
    if (g?.idDip_GG_Richiesta == null) return null;
    const giorno = this.giornoIso(g.data);
    const stessa = tutti.filter(x => x.idDip_GG_Richiesta === g.idDip_GG_Richiesta).map(x => this.giornoIso(x.data));
    const dal = richiesta?.data ? this.ddmmyyyyToIso(richiesta.data) : null;
    const al  = richiesta?.dataA ? this.ddmmyyyyToIso(richiesta.dataA) : null;
    const prima = stessa.some(x => x < giorno) || (dal != null && dal < inizioVista && giorno <= fineVista);
    const dopo  = stessa.some(x => x > giorno) || (al != null && al > fineVista && giorno >= inizioVista);
    if (prima && dopo) return 'mezzo';
    if (dopo) return 'inizio';
    if (prima) return 'fine';
    return null;
  }

  /**
   * Pezzi di richiesta di tutti i giustificativi indicati (chiave = l'oggetto giustificativo):
   * contiene solo quelli di richieste su più giorni. Va calcolato una volta al caricamento dei dati.
   */
  segmentiRichieste(tutti: SegmentoItem[], richieste: Dip_GG_RichiestaModel[], inizioVista: string, fineVista: string): Map<SegmentoItem, SegmentoRichiesta> {
    const retVal = new Map<SegmentoItem, SegmentoRichiesta>();
    for (const g of tutti) {
      const richiesta = richieste?.find(r => r.id === g.idDip_GG_Richiesta);
      const seg = this.segmentoRichiesta(g, tutti, richiesta, inizioVista, fineVista);
      if (seg) retVal.set(g, seg);
    }
    return retVal;
  }

  /** Pezzi di richiesta dei giustificativi del mese del cartellino. */
  segmentiDelMese(month: MonthData): Map<SegmentoItem, SegmentoRichiesta> {
    const tutti = Object.values(month?.days ?? {}).flatMap(d => d.dip_GG_Giustificativi ?? []);
    const inizio = this.giornoIso(new Date(month.year, month.month, 1));
    const fine = this.giornoIso(new Date(month.year, month.month + 1, 0));
    return this.segmentiRichieste(tutti, month.dip_GG_Richiesta, inizio, fine);
  }

  /**
   * Note spese del mese del cartellino per giorno (1-31): ognuna compare solo nel giorno iniziale.
   * Le note annullate dal dipendente non si mostrano.
   */
  noteSpeseDelMese(month: MonthData): Map<number, Dip_GG_RichiestaModel[]> {
    const retVal = new Map<number, Dip_GG_RichiestaModel[]>();
    for (const r of month?.dip_GG_Richiesta ?? []) {
      if (r.richiestaTipo !== TipoRichiesta.NotaSpesa || r.richiestaStato === StatoRichiesta.Cancellata || !r.data)
        continue;
      const [aaaa, mm, gg] = this.ddmmyyyyToIso(r.data).split('-').map(Number);
      if (aaaa !== month.year || mm !== month.month + 1)
        continue;
      retVal.set(gg, [...(retVal.get(gg) ?? []), r]);
    }
    return retVal;
  }

  /** Totale della nota spese (dal riepilogo della richiesta), es. "€ 123,45". */
  notaSpeseTotaleTesto(richiesta: Dip_GG_RichiestaModel | null | undefined): string {
    return '€ ' + (riepilogoNotaSpesa(richiesta?.dati).totale ?? 0).toFixed(2).replace('.', ',');
  }

  /** "Richiesta dal … al …" per i tooltip dei giustificativi di una richiesta su più giorni. */
  periodoRichiestaTesto(richiesta: Dip_GG_RichiestaModel | null | undefined): string {
    return richiesta?.data && richiesta.dataA && richiesta.data !== richiesta.dataA
      ? this.t('richiestaDalAl', { dal: richiesta.data, al: richiesta.dataA }) : '';
  }

  /** "YYYY-MM-DD" di una data (oggetto Date o stringa ISO), nel fuso locale. */
  giornoIso(data: Date | string): string {
    const d = new Date(data);
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  private ddmmyyyyToIso(s: string): string {
    const [gg, mm, aaaa] = s.substring(0, 10).split('/');
    return `${aaaa}-${mm}-${gg}`;
  }

  /** Testo breve per i giorni fuori rapporto. */
  fuoriRapportoTesto(stato: FuoriRapporto | null): string {
    switch (stato) {
      case 'prima': return this.t('fuoriRapporto.prima');
      case 'dopo':  return this.t('fuoriRapporto.dopo');
      case 'tra':   return this.t('fuoriRapporto.tra');
      default:      return '';
    }
  }

  private transformRemoteDataToMonthData(remoteData: TimeSheetRemoteData, year: number, month: number): MonthData {
    const monthData: MonthData = {
      year: year,
      month: month,
      days: {},
      dip_GG_Richiesta: [],
      daySlot: [],
      contatori_Anno_Mese: []
    };

    monthData.dip_GG_Richiesta = remoteData.dip_GG_Richiesta;
    monthData.daySlot = remoteData.daySlot;
    monthData.contatori_Anno_Mese = remoteData.contatori_Anno_Mese;
    monthData.periodiRapporto = remoteData.periodiRapporto ?? null;

    // Raggruppa le timbrature per giorno
    const timbratureByDay = new Map<number, Dip_GG_TimbraturaModel[]>();
    remoteData.dip_GG_Timbratura.forEach(timbratura => {
      const day = new Date(timbratura.timbratura).getDate();

      // Verifica se la timbratura appartiene al mese corretto
      const timbraMonth = new Date(timbratura.timbratura).getMonth();
      const timbraYear = new Date(timbratura.timbratura).getFullYear();

      if (timbraMonth === month && timbraYear === year) {
        if (!timbratureByDay.has(day)) {
          timbratureByDay.set(day, []);
        }
        timbratureByDay.get(day)?.push(timbratura);
      }
    });

    // Raggruppa i giustificativi per giorno
    const giustificativiByDay = new Map<number, Dip_GG_GiustificativiModel[]>();
    remoteData.dip_GG_Giustificativi.forEach(giustificativo => {
      const day = new Date(giustificativo.data).getDate();

      // Verifica se il giustificativo appartiene al mese corretto
      const giustMonth = new Date(giustificativo.data).getMonth();
      const giustYear = new Date(giustificativo.data).getFullYear();

      if (giustMonth === month && giustYear === year) {
        if (!giustificativiByDay.has(day)) {
          giustificativiByDay.set(day, []);
        }
        giustificativiByDay.get(day)?.push(giustificativo);
      }
    });


    // Raggruppa le causali per giorno
    const causaliByDay = new Map<number, Dip_GG_CausaliModel[]>();
    remoteData.dip_GG_Causali.forEach(causale => {
      const day = new Date(causale.data).getDate();

      // Verifica se la causale appartiene al mese corretto
      const cauMonth = new Date(causale.data).getMonth();
      const cauYear = new Date(causale.data).getFullYear();

      if (cauMonth === month && cauYear === year) {
        if (!causaliByDay.has(day)) {
          causaliByDay.set(day, []);
        }
        causaliByDay.get(day)?.push(causale);
      }
    });


    // Assegna il Result al giorno
    const resultByDay = new Map<number, Dip_GG_ResultModel[]>();
    remoteData.dip_GG_Result.forEach(result => {

      
      const day = parseInt(result.data.substring(8, 10));
      

      if (!resultByDay.has(day)) {
        resultByDay.set(day, []);
      }
      resultByDay.get(day)?.push(result);

    });


    // Unisci i dati per creare i record giornalieri
    const allDays = new Set<number>([
      ...Array.from(timbratureByDay.keys()),
      ...Array.from(giustificativiByDay.keys()),
      ...Array.from(resultByDay.keys()),
      ...Array.from(causaliByDay.keys())
    ]);

    allDays.forEach(day => {
      monthData.days[day] = {
        date: new Date(year, month, day),
        dip_GG_Timbratura: timbratureByDay.get(day) || [],
        dip_GG_Giustificativi: giustificativiByDay.get(day) || [],
        dip_GG_Result: this.get_dip_GG_Result(remoteData, day),
        dip_GG_Causali: causaliByDay.get(day) || [],
        idDip_RapportoLavoro:0
      };
    });

    return monthData;
  }

  private get_dip_GG_Result(remoteData: TimeSheetRemoteData, day:number): Dip_GG_ResultModel {
    var retVal = remoteData.dip_GG_Result.filter(t => parseInt(t.data.substring(8, 10))  === day)
    return retVal.length > 0 ? retVal[0] : null;
  }


  private sortTimestampsByTime(timestamps: Dip_GG_TimbraturaModel[] | undefined): Dip_GG_TimbraturaModel[] {
    if (!timestamps) return [];

    return [...timestamps].sort((a, b) => this.dateTimeUtilService.timeToMinutes(a.timbratura) - this.dateTimeUtilService.timeToMinutes(b.timbratura));

  }

  get_IdDip_RapportoLavoro(currentMonth: MonthData,idAspNetUsers: string, data: Date): number {
    const dataStr = this.dateTimeUtilService.DateTo_ggmmyyyy(data) // "yyyy-MM-dd"

    const slot = currentMonth.daySlot?.find(s => s.idAspNetUsers === idAspNetUsers &&
                                                 this.dateTimeUtilService.DateTo_ggmmyyyy(new Date(s.data))  === dataStr );

    return slot?.idDip_RapportoLavoro ?? 0;
  }

  get_StatoRichiesta_icon(dip_GG_Richiesta: Dip_GG_RichiestaModel): string {

    if (dip_GG_Richiesta == null || (dip_GG_Richiesta != null && (dip_GG_Richiesta.richiestaStato == undefined && dip_GG_Richiesta.revocaStato == undefined)))
      return null;

    if (dip_GG_Richiesta.revocaStato == null) {
      switch (dip_GG_Richiesta.richiestaStato) {
        case StatoRichiesta.Diretta:
          return 'checkmark-circle'; // Inserimento diretto
        case StatoRichiesta.Immessa:
          return 'time-outline'; // In attesa
        case StatoRichiesta.ApprovazioneInCorso:
          return 'hourglass-outline'; // In corso
        case StatoRichiesta.ParzialmenteApprovata:
          return 'alert-circle-outline'; // Parzialmente approvata
        case StatoRichiesta.Approvata:
          return 'checkmark-circle-outline'; // Approvata
        case StatoRichiesta.Rifiutata:
          return 'close-circle-outline'; // Rifiutata
        case StatoRichiesta.Cancellata:
          return 'trash-outline'; // Cancellata
        default:
          return 'help-circle-outline'; // Stato sconosciuto
      }
    }
    else {
      switch (dip_GG_Richiesta.revocaStato) {
        case StatoRichiesta.Diretta:
          return 'checkmark-circle'; // Inserimento diretto
        case StatoRichiesta.Immessa:
          return 'time-outline'; // In attesa
        case StatoRichiesta.ApprovazioneInCorso:
          return 'hourglass-outline'; // In corso
        case StatoRichiesta.ParzialmenteApprovata:
          return 'alert-circle-outline'; // Parzialmente approvata
        case StatoRichiesta.Approvata:
          return 'checkmark-circle-outline'; // Approvata
        case StatoRichiesta.Rifiutata:
          return 'close-circle-outline'; // Rifiutata
        case StatoRichiesta.Cancellata:
          return 'trash-outline'; // Cancellata
        default:
          return 'help-circle-outline'; // Stato sconosciuto
      }
    }

   

  }

  get_StatoRichiesta_text(status: StatoRichiesta): string {

    const statoRichiesta = new StatoRichiestaLongTextPipe();
    return statoRichiesta.transform(status);
    
  }

  get_Dip_GG_Giustificativi_backColor(ggJust: Dip_GG_GiustificativiModel): string {
    const just = this.sharedParameterGestionePresenzeService.Par_Giustificativi.find(x => x.id == ggJust.idPar_Giustificativi);
    //if (just)
    //  return just.backgroundColor;
    //else
    //  return null;

    switch (ggJust.richiestaStato) {

      case StatoRichiesta.Diretta:
      case StatoRichiesta.Approvata:
        return null;
        break;

      case StatoRichiesta.Immessa:
      if (just)
        return just.backgroundColor;
      else
        return null;

      default:
        return null;
        break;
    }


  }

  get_Dip_GG_Giustificativi_txtColor(ggJust: Dip_GG_GiustificativiModel): string {
    const just = this.sharedParameterGestionePresenzeService.Par_Giustificativi.find(x => x.id == ggJust.idPar_Giustificativi);
    //if (just)
    //  return just.textColor;
    //else
    //  return null;

    switch (ggJust.richiestaStato) {

      case StatoRichiesta.Diretta:
      case StatoRichiesta.Approvata:
        if (just)
          return just.backgroundColor;
        else
          return null;

      case StatoRichiesta.Immessa:
        if (just)
          return just.textColor;
        else
          return null;

      default:
        return null;
        break;
    }

  }


  /**
   * Ore in piu' (o mancanti) del giorno e loro qualifica, per la colonna "In piu'" dei calendari.
   * null se il giorno non e' calcolato o quadra con il lavorabile.
   * Con il conteggio settimanale/mensile, finche' il periodo e' in corso, le ore sono "in attesa di quadratura".
   */
  eccedenzaGiorno(r: Dip_GG_ResultModel | null | undefined): { minuti: number, testo: string, provvisoria: boolean, qualifica: string, dettaglio: string } | null {
    const nonAutorizzate = r?.min_NonAutorizzate ?? 0;
    if (!r || (!r.min_Ecc && !nonAutorizzate))
      return null;
    if (!(Number(r.stato) & ~GG_ResultStato.Init))
      return null;

    const parti: string[] = [];
    const aggiungi = (etichetta: string, minuti: number) => { if (minuti > 0) parti.push(`${etichetta} ${minutiToHHMM(minuti)}`); };
    aggiungi(this.t('eccedenza.suppl'), r.min_Suppl);
    aggiungi(this.t('eccedenza.supplOltre'), r.min_SupplEcc);
    aggiungi(this.t('eccedenza.strao'), r.min_Strao);
    aggiungi(this.t('eccedenza.straoNott'), r.min_StraoNott);
    aggiungi(this.t('eccedenza.straoFest'), r.min_StraoFest);
    aggiungi(this.t('eccedenza.straoFestNott'), r.min_StraoFestNott);
    aggiungi(this.t('eccedenza.eccedenti'), r.min_EccOrd);
    aggiungi(this.t('eccedenza.nonRic'), r.min_NonRiconosciute);
    if (r.min_DaAutorizzare > 0)
      parti.push(this.t('eccedenza.inAttesaAutorizzazione', { ore: minutiToHHMM(r.min_DaAutorizzare) }));
    if (nonAutorizzate - (r.min_DaAutorizzare ?? 0) > 0)
      parti.push(this.t('eccedenza.nonAutorizzate', { ore: minutiToHHMM(nonAutorizzate - (r.min_DaAutorizzare ?? 0)) }));

    const qualificate = r.min_Suppl + r.min_SupplEcc + r.min_Strao + r.min_StraoNott + r.min_StraoFest + r.min_StraoFestNott + r.min_EccOrd + r.min_NonRiconosciute;
    let qualifica = parti.join(' · ');
    if (r.min_Ecc > 0 && !r.eccProvvisoria && qualificate < r.min_Ecc)
      qualifica = (qualifica ? qualifica + ' · ' : '') + this.t('eccedenza.compensate', { ore: minutiToHHMM(r.min_Ecc - qualificate) });
    if (r.bancaOre && qualificate > 0)
      qualifica += ' ' + this.t('eccedenza.bancaOre');

    // le ore in piu' mostrate sono tutte quelle lavorate oltre il dovuto, anche quelle non autorizzate
    const totale = r.min_Ecc > 0 ? r.min_Ecc + nonAutorizzate : (r.min_Ecc < 0 ? r.min_Ecc : nonAutorizzate);
    return {
      minuti: totale,
      testo: minutiToHHMM(totale, true),
      provvisoria: (r.min_Ecc > 0 && r.eccProvvisoria) || r.min_DaAutorizzare > 0,
      qualifica,
      dettaglio: r.min_Ecc < 0
        ? this.t('eccedenza.oreMancanti')
        : (r.eccProvvisoria ? this.t('eccedenza.oreInPiuProvvisorie') : (qualifica || this.t('eccedenza.oreInPiu')))
    };
  }

  /**
   * Le ore in piu' non sono un errore: un giorno il cui unico errore e' Err_3 (ore lavorate oltre le previste,
   * segnale dei calcoli precedenti allo straordinario) si mostra come avviso, con il "!" giallo.
   * I nuovi calcoli segnalano gia' le ore in piu' con Warning_1.
   */
  private statoPerIcona(stato: number): number {
    const dettagliErrore = ((1 << 25) - 1) & ~((1 << 5) - 1);   // Err_TimbratureMancanti .. Err_20
    if ((stato & GG_ResultStato.Err) && (stato & dettagliErrore) === GG_ResultStato.Err_3)
      return (stato & ~GG_ResultStato.Err) | GG_ResultStato.Warning;
    return stato;
  }

  get_StatoDay_icon(dip_GG_ResultModel: Dip_GG_ResultModel): string {

    const stato = this.statoPerIcona(Number(dip_GG_ResultModel.stato));

    let main: GG_ResultStato;

    if (stato & GG_ResultStato.Err) {
      main = GG_ResultStato.Err;
    } else if (stato & GG_ResultStato.Warning) {
      main = GG_ResultStato.Warning;
    } else if (stato & GG_ResultStato.Locked) {
      main = GG_ResultStato.Locked;
    } else if (stato & GG_ResultStato.OK) {
      main = GG_ResultStato.OK;
    } else {
      main = GG_ResultStato.Init;
    }

    switch (main) {
      case GG_ResultStato.Init:
        return null;

      case GG_ResultStato.OK:
        return 'checkmark-done-outline';

      case GG_ResultStato.Locked:
        return 'lock-closed-outline';

      case GG_ResultStato.Warning:
        return 'alert-outline';

      case GG_ResultStato.Err:
        return 'close-outline';

      default:
        return null;
    }
  }

  get_StatoDay_color(dip_GG_ResultModel: Dip_GG_ResultModel): string {
    

    const stato = this.statoPerIcona(Number(dip_GG_ResultModel.stato));

    let main: GG_ResultStato;

    if (stato & GG_ResultStato.Err) {
      main = GG_ResultStato.Err;
    } else if (stato & GG_ResultStato.Warning) {
      main = GG_ResultStato.Warning;
    } else if (stato & GG_ResultStato.Locked) {
      main = GG_ResultStato.Locked;
    } else if (stato & GG_ResultStato.OK) {
      main = GG_ResultStato.OK;
    } else {
      main = GG_ResultStato.Init;
    }

    switch (main) {
      case GG_ResultStato.Init:
        return "var(--ion-color-medium)" //grigio
        

      case GG_ResultStato.OK:
        return "var(--ion-color-success)"

      case GG_ResultStato.Locked:
        return "var(--ion-color-medium)" // grigio

      case GG_ResultStato.Warning:
        return "var(--ion-color-warning)"

      case GG_ResultStato.Err:
        return "var(--ion-color-danger)"

      default:
        return "var(--ion-color-medium)" // grigio
    }


  }


  //get_Dip_GG_Timbratura_backColor(record: Dip_GG_TimbraturaModel): string {
  //  // Ottieni il valore della variabile CSS dal root
  //  const root = document.documentElement;

  //  let value = '';

  //  switch (record.timbraturaTipo) {
  //    case TipoTimbratura.Entrata:
  //      value = getComputedStyle(root).getPropertyValue('--ion-color-primary').trim();
  //      return value || '#3880ff';
  //      break;
  //    case TipoTimbratura.Uscita:
  //      value = getComputedStyle(root).getPropertyValue('--ion-color-medium').trim();
  //      return value || '#92949c';
  //      break;
  //  }


  //  return '#3880ff';
  //}

  //get_Dip_GG_Timbratura_txtColor(record: Dip_GG_TimbraturaModel): string {
  //  // Ottieni il valore della variabile CSS dal root
  //  const root = document.documentElement;

  //  let value = '';

  //  switch (record.timbraturaTipo) {
  //    case TipoTimbratura.Entrata:
  //      value = getComputedStyle(root).getPropertyValue('--ion-color-primary-contrast').trim();
  //      return value || '#ffffff';
  //      break;
  //    case TipoTimbratura.Uscita:
  //      value = getComputedStyle(root).getPropertyValue('--ion-color-medium-contrast').trim();
  //      return value || '#ffffff';
  //      break;
  //  }


  //  return '#ffffff';
  //}

  get_Dip_GG_Timbratura_backColor(record: Dip_GG_TimbraturaModel): string {
    const root = document.documentElement;

    


    let value = '';

    switch (record.richiestaStato) {

      case StatoRichiesta.Diretta:
      case StatoRichiesta.Approvata:

        switch (record.timbraturaTipo) {
          case TipoTimbratura.SenzaVerso:  // ancora da riconoscere
          case TipoTimbratura.Entrata:
          case TipoTimbratura.Uscita:
          case TipoTimbratura.Attivita:
            return null;
        }

        break;

      case StatoRichiesta.Immessa:
          value = getComputedStyle(root).getPropertyValue('--ion-color-secondary').trim();
          return value || '#ffffff';

      default:
        return null;
        break;

    }


    return '#3880ff';
  }

  get_Dip_GG_Timbratura_txtColor(record: Dip_GG_TimbraturaModel): string {
    const root = document.documentElement;

    
    let value = '';

    switch (record.richiestaStato) {
      case  StatoRichiesta.Diretta:
      case StatoRichiesta.Approvata:

        switch (record.timbraturaTipo)
        {
          case TipoTimbratura.SenzaVerso:  // ancora da riconoscere
            value = getComputedStyle(root).getPropertyValue('--ion-color-secondary').trim();  //(giallo)
            return value || '#ffffff';
            break;

          case TipoTimbratura.Entrata:
              value = getComputedStyle(root).getPropertyValue('--ion-color-primary').trim(); // (blue)
              return value || '#ffffff';

          case TipoTimbratura.Uscita:
              value = getComputedStyle(root).getPropertyValue('--ion-color-medium').trim(); // (grigio)
            return value || '#ffffff';

          case TipoTimbratura.Attivita:
            value = getComputedStyle(root).getPropertyValue('--ion-color-secondary').trim(); //(giallo) da gestire
            return value || '#ffffff';
            break;
        }

        break;

      case StatoRichiesta.Immessa:
        
        value = getComputedStyle(root).getPropertyValue('--ion-color-secondary-contrast').trim(); // grigio scuro - su fondo giallo
        return value || '#ffffff';
        break;

      default:
        return getComputedStyle(root).getPropertyValue('--ion-text-color').trim() || '#000000';
        break;

      }


    return '#ffffff';
  }

  get_Dip_GG_Causali_backColor(ggCau: Dip_GG_CausaliModel): string {
    //const just = this.sharedParameterGestionePresenzeService.Par_Causali.find(x => x.id == ggCau.idPar_Causali);
    //if (just)
    //  return just.backgroundColor;
    //else
    //  return null;

    const root = document.documentElement;

    let value = '';

    value = getComputedStyle(root).getPropertyValue('--ion-color-medium').trim();
    return value || '#92949c';

  }

  get_Dip_GG_Causali_txtColor(ggCau: Dip_GG_CausaliModel): string {
    //const just = this.sharedParameterGestionePresenzeService.Par_Causali.find(x => x.id == ggCau.idPar_Causali);
    //if (just)
    //  return just.textColor;
    //else
    //  return null;

    const root = document.documentElement;

    let value = '';
    
    value = getComputedStyle(root).getPropertyValue('--ion-color-medium-contrast').trim();
    return value || '#ffffff';

  }





  get_StatoRichiesta_Approval_text(dip_GG_Richiesta: Dip_GG_RichiestaModel): string {

    if (dip_GG_Richiesta == null || (dip_GG_Richiesta != null && (dip_GG_Richiesta.richiestaStato == undefined && dip_GG_Richiesta.revocaStato == undefined)))
      return '';

    if (dip_GG_Richiesta.revocaStato == null) {
      return this.get_StatoRichiesta_text(dip_GG_Richiesta.richiestaStato);
    }
    else {
      return this.t('statoConRevoca', { stato: this.get_StatoRichiesta_text(dip_GG_Richiesta.richiestaStato), revoca: this.get_StatoRichiesta_text(dip_GG_Richiesta.revocaStato) });
    }
    
  }


  public Dip_GG_Richiesta_User_Can_Delete(dip_GG_Richiesta: Dip_GG_RichiestaModel) {

    if (dip_GG_Richiesta == null || (dip_GG_Richiesta != null && (dip_GG_Richiesta.richiestaStato == undefined && dip_GG_Richiesta.revocaStato == undefined)))
      return false;

    if (dip_GG_Richiesta.revocaStato == null) {

      if (dip_GG_Richiesta.richiestaStato == StatoRichiesta.Immessa ||
        dip_GG_Richiesta.richiestaStato == StatoRichiesta.ApprovazioneInCorso ||
        dip_GG_Richiesta.richiestaStato == StatoRichiesta.Approvata)
        return true;
      return false;
    }
    else {
      if (dip_GG_Richiesta.revocaStato == StatoRichiesta.Immessa ||
        dip_GG_Richiesta.revocaStato == StatoRichiesta.ApprovazioneInCorso)
        return true;
      return false;
    }
  }

  public Dip_GG_Richiesta_Admin_Can_Approve(dip_GG_Richiesta: Dip_GG_RichiestaModel) {

    if (dip_GG_Richiesta == null || (dip_GG_Richiesta != null && (dip_GG_Richiesta.richiestaStato == undefined && dip_GG_Richiesta.revocaStato == undefined)))
      return false;

    if (dip_GG_Richiesta.revocaStato == null) {
      

      if (dip_GG_Richiesta.richiestaStato != StatoRichiesta.Immessa)
        return false;

      return true;
    }
    else {
      if (dip_GG_Richiesta.revocaStato != StatoRichiesta.Immessa)
        return false;

      return true;
    }

  }


  get_GiustificativoContatoriPopupData(record: Dip_GG_GiustificativiModel, month:number,contatori_Anno_Mese: Contatori_Anno_MeseResult[]): HoverPopupData| null {

    const extraInfo: Record<string, string> = {};

    const just = this.sharedParameterGestionePresenzeService.Par_Giustificativi.find(x => x.id == record.idPar_Giustificativi);
    if (just != null && just.tipoContatore != TipoContatore.NoContatore) {

      const tipoLabel = this.sharedParameterGestionePresenzeService.Par_Giustificativi.find(x => x.id == record.idPar_Giustificativi)?.descrizione ?? this.t('giustificativo');
      const content = this.t('contatori');


      const idx = contatori_Anno_Mese.findIndex(x => x.mese == month);

      const idxJust = idx >= 0 ? contatori_Anno_Mese[idx].risultati.findIndex(x => x.idPar_Giustificativi == record.idPar_Giustificativi) : -1;
      if (idxJust >= 0) {

        // periodi del mese del giustificativo: il precedente comprende il riporto dell'anno prima
        extraInfo[this.t('contatoriInfo.mesePrecMaturato')] = contatori_Anno_Mese[idx].risultati[idxJust].periodoPrecedente.maturato;
        extraInfo[this.t('contatoriInfo.mesePrecGoduto')] = contatori_Anno_Mese[idx].risultati[idxJust].periodoPrecedente.goduto;
        extraInfo[this.t('contatoriInfo.mesePrecSaldo')] = contatori_Anno_Mese[idx].risultati[idxJust].periodoPrecedente.saldo;


        extraInfo[this.t('contatoriInfo.meseCorrMaturato')] = contatori_Anno_Mese[idx].risultati[idxJust].periodoCorrente.maturato;
        extraInfo[this.t('contatoriInfo.meseCorrGoduto')] = contatori_Anno_Mese[idx].risultati[idxJust].periodoCorrente.goduto;
        extraInfo[this.t('contatoriInfo.meseCorrSaldo')] = contatori_Anno_Mese[idx].risultati[idxJust].periodoCorrente.saldo;

        extraInfo[this.t('contatoriInfo.mesiSuccMaturato')] = contatori_Anno_Mese[idx].risultati[idxJust].periodoSuccessivo.maturato;
        extraInfo[this.t('contatoriInfo.mesiSuccGoduto')] = contatori_Anno_Mese[idx].risultati[idxJust].periodoSuccessivo.goduto;
        extraInfo[this.t('contatoriInfo.mesiSuccSaldo')] = contatori_Anno_Mese[idx].risultati[idxJust].periodoSuccessivo.saldo;
      }
      


      return {
        title: tipoLabel,
        content: content,
        extraInfo
      };

    }


    return null;
    

  }



  get_TimbraturaPopupData(record: Dip_GG_TimbraturaModel, richiesta: Dip_GG_RichiestaModel | null): HoverPopupData {
    const tipoLabel = new TipoTimbraturaToLongTextPipe().transform(record.timbraturaTipo);

    const timbratura       = record.timbratura       ? this.oraBreve(record.timbratura) : '--';
    const timbraturaOrig   = record.timbraturaOriginale ? this.oraBreve(record.timbraturaOriginale) : null;
    const timbraturaArrot  = record.timbraturaArrotondata ? this.oraBreve(record.timbraturaArrotondata) : null;
    const giornoComp       = record.giornoCompetenza ? this.giornoLungo(record.giornoCompetenza) : '--';

    const extraInfo: Record<string, string> = {};

    extraInfo[this.t('popup.timbratura')] = timbratura;

    if (timbraturaOrig && timbraturaOrig !== timbratura)
      extraInfo[this.t('popup.originale')] = timbraturaOrig;

    if (timbraturaArrot &&
        (record.timbraturaTipo === TipoTimbratura.Entrata || record.timbraturaTipo === TipoTimbratura.Uscita))
      extraInfo[this.t('popup.arrotondata')] = timbraturaArrot;

    extraInfo[this.t('popup.giornoCompetenza')] = giornoComp;

    if (richiesta)
      extraInfo[this.t('popup.statoRichiesta')] = new StatoRichiestaLongTextPipe().transform(
        richiesta.revocaStato != null ? richiesta.revocaStato : richiesta.richiestaStato
      );

    const attivita = this.sharedParameterGestionePresenzeService.Az_SubCommessaAttivita_4Full
      ?.find(a => a.subCommessaAttivita_Id === record.idAz_SubCommessaAttivita);
    if (attivita)
      extraInfo[this.t('popup.attivita')] = `${attivita.cliente_Descrizione} › ${attivita.commessa_Decrizione} › ${attivita.subCommessa_Decrizione} › ${attivita.subCommessaAttivita_Par_Attivita_Descrizione}`;

    return {
      title: tipoLabel,
      content: giornoComp,
      extraInfo
    };
  }

  get_DayPopupStato(result: Dip_GG_ResultModel): HoverPopupData {
    if (!result) {
      return { title: this.t('giornata.statoGiornata'), content: this.t('giornata.nessunDato') };
    }

    const stato = Number(result.stato);
    const activeFlags: string[] = [];

    // --- Stato principale ---
    const statoLabels: Partial<Record<GG_ResultStato, string>> = {
      [GG_ResultStato.Init]:    this.t('giornata.stato.init'),
      [GG_ResultStato.OK]:      this.t('giornata.stato.ok'),
      [GG_ResultStato.Locked]:  this.t('giornata.stato.locked'),
      [GG_ResultStato.Warning]: this.t('giornata.stato.warning'),
      [GG_ResultStato.Err]:     this.t('giornata.stato.err'),
    };

    // --- Dettagli errori ---
    const errLabels: Partial<Record<GG_ResultStato, string>> = {
      [GG_ResultStato.Err_TimbratureMancanti]: this.t('giornata.err.timbratureMancanti'),
      [GG_ResultStato.Err_2]:  this.t('giornata.err.oreInferiori'),
      [GG_ResultStato.Err_3]:  this.t('giornata.oreSuperiori'),
      [GG_ResultStato.Err_4]:  this.t('giornata.erroreN', { n: 4 }),
      [GG_ResultStato.Err_5]:  this.t('giornata.erroreN', { n: 5 }),
      [GG_ResultStato.Err_6]:  this.t('giornata.erroreN', { n: 6 }),
      [GG_ResultStato.Err_7]:  this.t('giornata.erroreN', { n: 7 }),
      [GG_ResultStato.Err_8]:  this.t('giornata.erroreN', { n: 8 }),
      [GG_ResultStato.Err_9]:  this.t('giornata.erroreN', { n: 9 }),
      [GG_ResultStato.Err_10]: this.t('giornata.erroreN', { n: 10 }),
      [GG_ResultStato.Err_11]: this.t('giornata.erroreN', { n: 11 }),
      [GG_ResultStato.Err_12]: this.t('giornata.erroreN', { n: 12 }),
      [GG_ResultStato.Err_13]: this.t('giornata.erroreN', { n: 13 }),
      [GG_ResultStato.Err_14]: this.t('giornata.erroreN', { n: 14 }),
      [GG_ResultStato.Err_15]: this.t('giornata.erroreN', { n: 15 }),
      [GG_ResultStato.Err_16]: this.t('giornata.erroreN', { n: 16 }),
      [GG_ResultStato.Err_17]: this.t('giornata.erroreN', { n: 17 }),
      [GG_ResultStato.Err_18]: this.t('giornata.erroreN', { n: 18 }),
      [GG_ResultStato.Err_19]: this.t('giornata.erroreN', { n: 19 }),
      [GG_ResultStato.Err_20]: this.t('giornata.erroreN', { n: 20 }),
    };

    // --- Dettagli warning ---
    const warnLabels: Partial<Record<GG_ResultStato, string>> = {
      [GG_ResultStato.Warning_1]:  this.t('giornata.oreSuperiori'),
      [GG_ResultStato.Warning_2]:  this.t('giornata.avvertimentoN', { n: 2 }),
      [GG_ResultStato.Warning_3]:  this.t('giornata.avvertimentoN', { n: 3 }),
      [GG_ResultStato.Warning_4]:  this.t('giornata.avvertimentoN', { n: 4 }),
      [GG_ResultStato.Warning_5]:  this.t('giornata.avvertimentoN', { n: 5 }),
      [GG_ResultStato.Warning_6]:  this.t('giornata.avvertimentoN', { n: 6 }),
      [GG_ResultStato.Warning_7]:  this.t('giornata.avvertimentoN', { n: 7 }),
      [GG_ResultStato.Warning_8]:  this.t('giornata.avvertimentoN', { n: 8 }),
      [GG_ResultStato.Warning_9]:  this.t('giornata.avvertimentoN', { n: 9 }),
      [GG_ResultStato.Warning_10]: this.t('giornata.avvertimentoN', { n: 10 }),
      [GG_ResultStato.Warning_11]: this.t('giornata.avvertimentoN', { n: 11 }),
      [GG_ResultStato.Warning_12]: this.t('giornata.avvertimentoN', { n: 12 }),
      [GG_ResultStato.Warning_13]: this.t('giornata.avvertimentoN', { n: 13 }),
      [GG_ResultStato.Warning_14]: this.t('giornata.avvertimentoN', { n: 14 }),
      [GG_ResultStato.Warning_15]: this.t('giornata.avvertimentoN', { n: 15 }),
      [GG_ResultStato.Warning_16]: this.t('giornata.avvertimentoN', { n: 16 }),
      [GG_ResultStato.Warning_17]: this.t('giornata.avvertimentoN', { n: 17 }),
      [GG_ResultStato.Warning_18]: this.t('giornata.avvertimentoN', { n: 18 }),
      [GG_ResultStato.Warning_19]: this.t('giornata.avvertimentoN', { n: 19 }),
      [GG_ResultStato.Warning_20]: this.t('giornata.avvertimentoN', { n: 20 }),
    };

    // Stato principale (primo attivo tra Init/OK/Locked/Warning/Err)
    const statoPrincipale = Object.entries(statoLabels).find(([flag]) => stato & Number(flag));
    const titleText = statoPrincipale ? statoPrincipale[1] : this.t('giornata.statoSconosciuto');

    // Dettagli errori attivi
    for (const [flag, label] of Object.entries(errLabels)) {
      if (stato & Number(flag)) activeFlags.push(`• ${label}`);
    }

    // Dettagli warning attivi (bit > 31: usa confronto numerico)
    for (const [flag, label] of Object.entries(warnLabels)) {
      if (stato & Number(flag)) activeFlags.push(`• ${label}`);
    }

    const extraInfo: Record<string, string> = {};
    if (result.hH_Teo != null) extraInfo[this.t('popup.oreTeoriche')] = result.hH_Teo;
    if (result.hH_Lav != null) extraInfo[this.t('popup.oreLavorate')] = result.hH_Lav;

    return {
      title: result.data
        ? this.giornoLungo(result.data)
        : this.t('giornata.statoGiornata'),
      content: activeFlags.length > 0 ? activeFlags.join('\n') : titleText,
      extraInfo
    };
  }

  get_DayPopupData(
    result: Dip_GG_ResultModel,
    timbrature: Dip_GG_TimbraturaModel[],
    giustificativi: Dip_GG_GiustificativiModel[],
    causali: Dip_GG_CausaliModel[]
  ): HoverPopupData {

    const extraInfo: Record<string, string> = {};

    if (result?.hH_Teo != null)  extraInfo[this.t('popup.oreTeoriche')]  = result.hH_Teo;
    if (result?.hH_Lav != null)  extraInfo[this.t('popup.oreLavorate')]  = result.hH_Lav;
    if (timbrature?.length > 0)  extraInfo[this.t('popup.timbrature')]    = `${timbrature.length}`;
    if (giustificativi?.length > 0) extraInfo[this.t('popup.giustificativi')] = `${giustificativi.length}`;
    if (causali?.length > 0)     extraInfo[this.t('popup.causali')]       = `${causali.length}`;

    return {
      title: result?.data ? this.giornoLungo(result.data) : this.t('giornata.dettaglioGiorno'),
      content: this.get_StatoDay_icon(result) ? this.t('giornata.statoValore', { stato: this.get_StatoDay_color(result) }) : this.t('giornata.nessunRisultato'),
      extraInfo
    };
  }

  public Dip_GG_Richiesta_Admin_Can_Reject(dip_GG_Richiesta: Dip_GG_RichiestaModel) {


    if (dip_GG_Richiesta == null || (dip_GG_Richiesta != null  && (dip_GG_Richiesta.richiestaStato == undefined && dip_GG_Richiesta.revocaStato == undefined) ) )
      return false;

    if (dip_GG_Richiesta.revocaStato == null) {
      if (dip_GG_Richiesta.richiestaStato != StatoRichiesta.Immessa)
        return false;

      return true;
    }
    else {
      if (dip_GG_Richiesta.revocaStato != StatoRichiesta.Immessa)
        return false;

      return true;
    }

  }

  

}

