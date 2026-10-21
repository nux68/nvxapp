import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, forkJoin, map, Observable, of, retry, tap, timer } from 'rxjs';
import { GenericRequest } from '../ClientServer-Service/ModelsBase/generic-request';
import { Par_GiustificativiInModel, Par_GiustificativiModel } from '../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { ParGiustificativiService } from '../ClientServer-Service/GestionePresenze/Par_Giustificativi/par-giustificativi.service';
import { Par_Attivita_GetAll_InModel, Par_AttivitaModel } from '../ClientServer-Service/GestionePresenze/Par_Attivita/Models/par-attivita-model';
import { ParAttivitaService } from '../ClientServer-Service/GestionePresenze/Par_Attivita/par-attivita.service';
import { Par_Competenza_GetAll_InModel, Par_CompetenzaModel } from '../ClientServer-Service/GestionePresenze/Par_Competenza/Models/par-competenza-model';
import { ParCompetenzaService } from '../ClientServer-Service/GestionePresenze/Par_Competenza/par-competenza.service';
import { Az_Commessa_GetAll_InModel, Az_CommessaModel } from '../ClientServer-Service/GestionePresenze/Az_Commessa/Models/az-commessa-model';
import { AzCommessaService } from '../ClientServer-Service/GestionePresenze/Az_Commessa/az-commessa.service';
import { Az_Cliente_GetAll_InModel, Az_ClienteModel } from '../ClientServer-Service/GestionePresenze/Az_Cliente/Models/az-cliente-model';
import { AzClienteService } from '../ClientServer-Service/GestionePresenze/Az_Cliente/az-cliente.service';
import { RolesListInModel, RolesModel } from '../ClientServer-Service/Infrastructure/Parameter/Models/roles-model';
import { Dip_Anagrafica_GetAll_InModel, Dip_AnagraficaModel } from '../ClientServer-Service/GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model';
import { DipAnagraficaService } from '../ClientServer-Service/GestionePresenze/Dip_Anagrafica/dip-anagrafica.service';
import { AzCfgService } from '../ClientServer-Service/GestionePresenze/Az_Cfg/az-cfg.service';
import { Az_Cfg_Get_InModel, Az_Cfg_GetAll_InModel, Az_CfgModel } from '../ClientServer-Service/GestionePresenze/Az_Cfg/Models/az-cfg-model';
import { RoleCode } from '../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { Az_SediReparto_GetAll_InModel, Az_SediRepartoModel } from '../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { AzSediRepartoService } from '../ClientServer-Service/GestionePresenze/Az_SediReparto/az-sedi-reparto.service';
import { AzSediService } from '../ClientServer-Service/GestionePresenze/Az_Sedi/az-sedi.service';
import { Az_Sedi_GetAll_InModel, Az_SediModel } from '../ClientServer-Service/GestionePresenze/Az_Sedi/Models/az-sedi-model';
import { Par_ProfiloOrario_GetAllInModel, Par_ProfiloOrarioModel } from '../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/Models/par-profilo-orario-model';
import { ParProfiloOrarioService } from '../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/par-profilo-orario.service';
import { ParOrarioService } from '../ClientServer-Service/GestionePresenze/Par_Orario/par-orario.service';
import { Par_Orario_GetAllInModel, Par_OrarioModel } from '../ClientServer-Service/GestionePresenze/Par_Orario/Models/par-orario-model';
import { ParOrarioIntervalloHHService } from '../ClientServer-Service/GestionePresenze/Par_OrarioIntervalloHH/par-orario-intervallo-hh.service';
import { Par_OrarioIntervalloHHInModel, Par_OrarioIntervalloHHModel } from '../ClientServer-Service/GestionePresenze/Par_OrarioIntervalloHH/Models/par-orario-intervallo-hh-model';
import { Par_CausaliInModel, Par_CausaliModel } from '../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';
import { ParCausaliService } from '../ClientServer-Service/GestionePresenze/Par_Causali/par-causali.service';
import { Par_ExportCau_GetAll_InModel, Par_ExportCauModel } from '../ClientServer-Service/GestionePresenze/Par_ExportCau/Models/par-export-cau-model';
import { ParExportCauService } from '../ClientServer-Service/GestionePresenze/Par_ExportCau/par-export-cau.service';
import { Az_SubCommessaAttivita_4FullListModel, Az_SubCommessaAttivita_GetAll_4FullList_InModel } from '../ClientServer-Service/GestionePresenze/Az_SubCommessaAttivita/Models/az-subcommessa-attivita-model';
import { AzSubCommessaAttivitaService } from '../ClientServer-Service/GestionePresenze/Az_SubCommessaAttivita/az-subcommessa-attivita.service';

/** tabelle dei parametri tenute in cache dal login */
export enum ParTable {
  Par_Giustificativi = 'Par_Giustificativi',
  Par_Attivita = 'Par_Attivita',
  Par_Competenza = 'Par_Competenza',
  Az_Commessa = 'Az_Commessa',
  Az_Cliente = 'Az_Cliente',
  Dip_Anagrafica = 'Dip_Anagrafica',
  Az_SediReparto = 'Az_SediReparto',
  Az_Sedi = 'Az_Sedi',
  Az_Cfg = 'Az_Cfg',
  Par_ProfiloOrario = 'Par_ProfiloOrario',
  Par_Orario = 'Par_Orario',
  Par_OrarioIntervalloHH = 'Par_OrarioIntervalloHH',
  Par_Causali = 'Par_Causali',
  Par_ExportCau = 'Par_ExportCau',
  Az_SubCommessaAttivita_4Full = 'Az_SubCommessaAttivita_4Full',
}

@Injectable({
  providedIn: 'root'
})
export class SharedParameterGestionePresenzeService {

  private _isLoad = false;
  get IsLoad() { return this._isLoad; }
  set IsLoad(newValue) { this._isLoad = newValue; }

  constructor(
    private parGiustificativiService: ParGiustificativiService,
    private parAttivitaService: ParAttivitaService,
    private parCompetenzaService: ParCompetenzaService,
    private azCommessaService: AzCommessaService,
    private azClienteService: AzClienteService,
    private dipAnagraficaService: DipAnagraficaService,
    private azSediRepartoService: AzSediRepartoService,
    private azSediService: AzSediService,
    private azCfgService: AzCfgService,
    private parProfiloOrarioService: ParProfiloOrarioService,
    private parOrarioService: ParOrarioService,
    private parCausaliService: ParCausaliService,
    private parExportCauService: ParExportCauService,
    private parOrarioIntervalloHHService: ParOrarioIntervalloHHService,
    private azSubCommessaAttivitaService: AzSubCommessaAttivitaService,
    
  ) { }

  // un caricatore per ogni tabella in cache: usati tutti insieme al login (InitCall) o singolarmente (Reload)
  private readonly loaders: Record<ParTable, { label: string, load: () => Observable<any> }> = {
    [ParTable.Par_Giustificativi]: {
      label: 'dei giustificativi',
      load: () => this.parGiustificativiService.GetAll(new GenericRequest<Par_GiustificativiInModel>(Par_GiustificativiInModel)).pipe(tap(r => this.Par_Giustificativi = r.data.par_Giustificativi))
    },
    [ParTable.Par_Attivita]: {
      label: 'delle attività',
      load: () => this.parAttivitaService.GetAll(new GenericRequest<Par_Attivita_GetAll_InModel>(Par_Attivita_GetAll_InModel)).pipe(tap(r => this.Par_Attivita = r.data.par_Attivita))
    },
    [ParTable.Par_Competenza]: {
      label: 'delle competenze',
      load: () => this.parCompetenzaService.GetAll(new GenericRequest<Par_Competenza_GetAll_InModel>(Par_Competenza_GetAll_InModel)).pipe(tap(r => this.Par_Competenza = r.data.par_Competenza))
    },
    [ParTable.Az_Commessa]: {
      label: 'delle commesse',
      load: () => this.azCommessaService.GetAll(new GenericRequest<Az_Commessa_GetAll_InModel>(Az_Commessa_GetAll_InModel)).pipe(tap(r => this.Az_Commessa = r.data.az_Commessa))
    },
    [ParTable.Az_Cliente]: {
      label: 'dei clienti',
      load: () => this.azClienteService.GetAll(new GenericRequest<Az_Cliente_GetAll_InModel>(Az_Cliente_GetAll_InModel)).pipe(tap(r => this.Az_Cliente = r.data.az_Cliente))
    },
    [ParTable.Dip_Anagrafica]: {
      label: 'delle anagrafiche',
      load: () => this.dipAnagraficaService.GetAll(new GenericRequest<Dip_Anagrafica_GetAll_InModel>(Dip_Anagrafica_GetAll_InModel)).pipe(tap(r => this.Dip_Anagrafica = r.data.dip_Anagrafica))
    },
    [ParTable.Az_SediReparto]: {
      label: 'dei reparti',
      load: () => this.azSediRepartoService.GetAll(new GenericRequest<Az_SediReparto_GetAll_InModel>(Az_SediReparto_GetAll_InModel)).pipe(tap(r => this.Az_SediReparto = r.data.az_SediReparto))
    },
    [ParTable.Az_Sedi]: {
      label: 'delle sedi',
      load: () => this.azSediService.GetAll(new GenericRequest<Az_Sedi_GetAll_InModel>(Az_Sedi_GetAll_InModel)).pipe(tap(r => this.Az_Sedi = r.data.az_Sedi))
    },
    [ParTable.Az_Cfg]: {
      label: 'della configurazione azienda',
      load: () => this.azCfgService.Az_CfgGet(new GenericRequest<Az_Cfg_Get_InModel>(Az_Cfg_Get_InModel)).pipe(tap(r => this.Az_Cfg = r.data.az_Cfg))
    },
    [ParTable.Par_ProfiloOrario]: {
      label: 'dei profili orari',
      load: () => this.parProfiloOrarioService.GetAll(new GenericRequest<Par_ProfiloOrario_GetAllInModel>(Par_ProfiloOrario_GetAllInModel)).pipe(tap(r => this.Par_ProfiloOrario = r.data.par_ProfiloOrario))
    },
    [ParTable.Par_Orario]: {
      label: 'degli orari',
      load: () => this.parOrarioService.GetAll(new GenericRequest<Par_Orario_GetAllInModel>(Par_Orario_GetAllInModel)).pipe(tap(r => this.Par_Orario = r.data.par_Orario))
    },
    [ParTable.Par_OrarioIntervalloHH]: {
      label: 'degli intervalli orari',
      load: () => this.parOrarioIntervalloHHService.GetAll(new GenericRequest<Par_OrarioIntervalloHHInModel>(Par_OrarioIntervalloHHInModel)).pipe(tap(r => this.Par_OrarioIntervalloHH = r.data.par_OrarioIntervalloHH))
    },
    [ParTable.Par_Causali]: {
      label: 'delle causali',
      load: () => this.parCausaliService.GetAll(new GenericRequest<Par_CausaliInModel>(Par_CausaliInModel)).pipe(tap(r => this.Par_Causali = r.data.par_Causali))
    },
    [ParTable.Par_ExportCau]: {
      label: 'del export causali',
      load: () => this.parExportCauService.GetAll(new GenericRequest<Par_ExportCau_GetAll_InModel>(Par_ExportCau_GetAll_InModel)).pipe(tap(r => this.Par_ExportCau = r.data.par_ExportCau))
    },
    [ParTable.Az_SubCommessaAttivita_4Full]: {
      label: 'di Az_SubCommessaAttivita_4Full',
      load: () => this.azSubCommessaAttivitaService.GetAll_4FullList(new GenericRequest<Az_SubCommessaAttivita_GetAll_4FullList_InModel>(Az_SubCommessaAttivita_GetAll_4FullList_InModel)).pipe(tap(r => this.Az_SubCommessaAttivita_4Full = r.data.az_SubCommessaAttivita))
    },
  };

  // tabelle che contengono dati di altre tabelle: vanno ricaricate insieme
  private static readonly dependencies: Partial<Record<ParTable, ParTable[]>> = {
    [ParTable.Az_Commessa]: [ParTable.Az_SubCommessaAttivita_4Full],
    [ParTable.Az_Cliente]: [ParTable.Az_SubCommessaAttivita_4Full],
    [ParTable.Par_Attivita]: [ParTable.Az_SubCommessaAttivita_4Full],
    [ParTable.Az_Sedi]: [ParTable.Az_SediReparto],
    [ParTable.Par_Orario]: [ParTable.Par_OrarioIntervalloHH],
  };

  private withRetry(call: Observable<any>, label: string): Observable<any> {
    return call.pipe(
      retry({
        count: 20,
        delay: (error, retryCount) => {
          console.error(`Errore rilevato, ritento dopo ${retryCount} secondi:`, error);
          return timer(500);
        }
      }),
      catchError((error) => {
        console.error(`Errore durante il caricamento ${label}:`, error);
        return of(null);
      }));
  }

  /** tutte le tabelle, al login (con la barra di avanzamento del parameter-loader) */
  public InitCall(updateProgress: (calls: any[]) => void): any[] {
    let calls: any[] = [];
    calls = Object.values(ParTable).map(t =>
      this.withRetry(this.loaders[t].load().pipe(tap(() => updateProgress(calls))), this.loaders[t].label));
    return calls;
  }

  /** ricarica solo le tabelle indicate (e quelle che ne dipendono), senza riquadro di caricamento */
  public Reload(...tables: ParTable[]): Observable<void> {
    const all = new Set<ParTable>();
    for (const t of tables) {
      all.add(t);
      (SharedParameterGestionePresenzeService.dependencies[t] ?? []).forEach(d => all.add(d));
    }
    if (all.size == 0) return of(undefined);
    return forkJoin([...all].map(t => this.withRetry(this.loaders[t].load(), this.loaders[t].label))).pipe(map((): void => undefined));
  }

  private _par_Giustificativi: Par_GiustificativiModel[] | null = [];
  public get Par_Giustificativi(): Par_GiustificativiModel[] | null {
    return this._par_Giustificativi;
  }
  public set Par_Giustificativi(value: Par_GiustificativiModel[] | null) {
    this._par_Giustificativi = value;
    this._par_GiustificativiSubject.next(value);
  }
  private _par_GiustificativiSubject = new BehaviorSubject<Par_GiustificativiModel[]>([]);
  public get Par_Giustificativi$(): Observable<Par_GiustificativiModel[] | []> {
    return this._par_GiustificativiSubject.asObservable();
  }

  private _par_Attivita: Par_AttivitaModel[] | null = [];
  public get Par_Attivita(): Par_AttivitaModel[] | null {
    return this._par_Attivita;
  }
  public set Par_Attivita(value: Par_AttivitaModel[] | null) {
    this._par_Attivita = value;
    this._par_AttivitaSubject.next(value);
  }
  private _par_AttivitaSubject = new BehaviorSubject<Par_AttivitaModel[]>([]);
  public get Par_Attivita$(): Observable<Par_AttivitaModel[] | []> {
    return this._par_AttivitaSubject.asObservable();
  }

  private _par_Competenza: Par_CompetenzaModel[] | null = [];
  public get Par_Competenza(): Par_CompetenzaModel[] | null {
    return this._par_Competenza;
  }
  public set Par_Competenza(value: Par_CompetenzaModel[] | null) {
    this._par_Competenza = value;
    this._par_CompetenzaSubject.next(value);
  }
  private _par_CompetenzaSubject = new BehaviorSubject<Par_CompetenzaModel[]>([]);
  public get Par_Competenza$(): Observable<Par_CompetenzaModel[] | []> {
    return this._par_CompetenzaSubject.asObservable();
  }

  private _az_Commessa: Az_CommessaModel[] | null = [];
  public get Az_Commessa(): Az_CommessaModel[] | null {
    return this._az_Commessa;
  }
  public set Az_Commessa(value: Az_CommessaModel[] | null) {
    this._az_Commessa = value;
    this._az_CommessaSubject.next(value);
  }
  private _az_CommessaSubject = new BehaviorSubject<Az_CommessaModel[]>([]);
  public get Az_Commessa$(): Observable<Az_CommessaModel[] | []> {
    return this._az_CommessaSubject.asObservable();
  }

  private _az_Cliente: Az_ClienteModel[] | null = [];
  public get Az_Cliente(): Az_ClienteModel[] | null {
    return this._az_Cliente;
  }
  public set Az_Cliente(value: Az_ClienteModel[] | null) {
    this._az_Cliente = value;
    this._az_ClienteSubject.next(value);
  }
  private _az_ClienteSubject = new BehaviorSubject<Az_ClienteModel[]>([]);
  public get Az_Cliente$(): Observable<Az_ClienteModel[] | []> {
    return this._az_ClienteSubject.asObservable();
  }

  private _dip_Anagrafica: Dip_AnagraficaModel[] | null = [];
  public get Dip_Anagrafica(): Dip_AnagraficaModel[] | null {
    return this._dip_Anagrafica;
  }
  public set Dip_Anagrafica(value: Dip_AnagraficaModel[] | null) {
    this._dip_Anagrafica = value;
    this._dip_AnagraficaSubject.next(value);
  }
  private _dip_AnagraficaSubject = new BehaviorSubject<Dip_AnagraficaModel[]>([]);
  public get Dip_Anagrafica$(): Observable<Dip_AnagraficaModel[] | []> {
    return this._dip_AnagraficaSubject.asObservable();
  }

  public Dip_Anagrafica_OnRoles(roles: RoleCode[]): Dip_AnagraficaModel[]  {
    if (this._dip_Anagrafica === null) {
      return [];
    }
    if (!roles || roles.length === 0 || this._dip_Anagrafica.length === 0) {
      return [];
    }
    return this._dip_Anagrafica.filter(anagrafica => {
      if (!anagrafica.roleCode || anagrafica.roleCode.length === 0) {
        return false;
      }
      return anagrafica.roleCode.some(userRole => roles.includes(userRole));
    });
  }

  private _az_Cfg: Az_CfgModel | null = null;
  public get Az_Cfg(): Az_CfgModel | null {
    return this._az_Cfg;
  }
  public set Az_Cfg(value: Az_CfgModel | null) {
    this._az_Cfg = value;
    this._az_CfgSubject.next(value);
  }
  private _az_CfgSubject = new BehaviorSubject<Az_CfgModel>(null);
  public get Az_Cfg$(): Observable<Az_CfgModel | {}> {
    return this._az_CfgSubject.asObservable();
  }


  private _az_SediReparto: Az_SediRepartoModel[] | null = [];
  public get Az_SediReparto(): Az_SediRepartoModel[] | null {
    return this._az_SediReparto;
  }
  public set Az_SediReparto(value: Az_SediRepartoModel[] | null) {
    this._az_SediReparto = value;
    this._az_SediRepartoSubject.next(value);
  }
  private _az_SediRepartoSubject = new BehaviorSubject<Az_SediRepartoModel[]>([]);
  public get Az_SediReparto$(): Observable<Az_SediRepartoModel[] | []> {
    return this._az_SediRepartoSubject.asObservable();
  }


  private _az_Sedi: Az_SediModel[] | null = [];
  public get Az_Sedi(): Az_SediModel[] | null {
    return this._az_Sedi;
  }
  public set Az_Sedi(value: Az_SediModel[] | null) {
    this._az_Sedi = value;
    this._az_SediSubject.next(value);
  }
  private _az_SediSubject = new BehaviorSubject<Az_SediModel[]>([]);
  public get Az_Sedi$(): Observable<Az_SediModel[] | []> {
    return this._az_SediSubject.asObservable();
  }


  

  private _par_ProfiloOrario: Par_ProfiloOrarioModel[] | null = [];
  public get Par_ProfiloOrario(): Par_ProfiloOrarioModel[] | null {
    return this._par_ProfiloOrario;
  }
  public set Par_ProfiloOrario(value: Par_ProfiloOrarioModel[] | null) {
    this._par_ProfiloOrario = value;
    this._par_ProfiloOrarioSubject.next(value);
  }
  private _par_ProfiloOrarioSubject = new BehaviorSubject<Par_ProfiloOrarioModel[]>([]);
  public get Par_ProfiloOrario$(): Observable<Par_ProfiloOrarioModel[] | []> {
    return this._par_ProfiloOrarioSubject.asObservable();
  }


  private _par_Orario: Par_OrarioModel[] | null = [];
  public get Par_Orario(): Par_OrarioModel[] | null {
    return this._par_Orario;
  }
  public set Par_Orario(value: Par_OrarioModel[] | null) {
    this._par_Orario = value;
    this._par_OrarioSubject.next(value);
  }
  private _par_OrarioSubject = new BehaviorSubject<Par_OrarioModel[]>([]);
  public get Par_Orario$(): Observable<Par_OrarioModel[] | []> {
    return this._par_OrarioSubject.asObservable();
  }



  private _par_OrarioIntervalloHH: Par_OrarioIntervalloHHModel[] | null = [];
  public get Par_OrarioIntervalloHH(): Par_OrarioIntervalloHHModel[] | null {
    return this._par_OrarioIntervalloHH;
  }
  public set Par_OrarioIntervalloHH(value: Par_OrarioIntervalloHHModel[] | null) {
    this._par_OrarioIntervalloHH = value;
    this._par_OrarioIntervalloHHSubject.next(value);
  }
  private _par_OrarioIntervalloHHSubject = new BehaviorSubject<Par_OrarioIntervalloHHModel[]>([]);
  public get Par_OrarioIntervalloHH$(): Observable<Par_OrarioIntervalloHHModel[] | []> {
    return this._par_OrarioIntervalloHHSubject.asObservable();
  }


  private _par_Causali: Par_CausaliModel[] | null = [];
  public get Par_Causali(): Par_CausaliModel[] | null {
    return this._par_Causali;
  }
  public set Par_Causali(value: Par_CausaliModel[] | null) {
    this._par_Causali = value;
    this._par_CausaliSubject.next(value);
  }
  private _par_CausaliSubject = new BehaviorSubject<Par_CausaliModel[]>([]);
  public get Par_Causali$(): Observable<Par_CausaliModel[] | []> {
    return this._par_CausaliSubject.asObservable();
  }


  private _par_ExportCau: Par_ExportCauModel[] | null = [];
  public get Par_ExportCau(): Par_ExportCauModel[] | null {
    return this._par_ExportCau;
  }
  public set Par_ExportCau(value: Par_ExportCauModel[] | null) {
    this._par_ExportCau = value;
    this._par_ExportCauSubject.next(value);
  }
  private _par_ExportCauSubject = new BehaviorSubject<Par_ExportCauModel[]>([]);
  public get Par_ExportCau$(): Observable<Par_ExportCauModel[] | []> {
    return this._par_ExportCauSubject.asObservable();
  }

  
  
  private _az_SubCommessaAttivita_4Full: Az_SubCommessaAttivita_4FullListModel[] | null = [];
  public get Az_SubCommessaAttivita_4Full(): Az_SubCommessaAttivita_4FullListModel[] | null {
    return this._az_SubCommessaAttivita_4Full;
  }
  public set Az_SubCommessaAttivita_4Full(value: Az_SubCommessaAttivita_4FullListModel[] | null) {
    this._az_SubCommessaAttivita_4Full = value;
    this._az_SubCommessaAttivita_4FullSubject.next(value);
  }
  private _az_SubCommessaAttivita_4FullSubject = new BehaviorSubject<Az_SubCommessaAttivita_4FullListModel[]>([]);
  public get Az_SubCommessaAttivita_4Full$(): Observable<Az_SubCommessaAttivita_4FullListModel[] | []> {
    return this._az_SubCommessaAttivita_4FullSubject.asObservable();
  }
  

}
