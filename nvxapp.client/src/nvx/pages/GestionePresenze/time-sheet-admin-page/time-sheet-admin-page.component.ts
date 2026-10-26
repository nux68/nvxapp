import { Component, OnInit, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { TimeSheetService } from '../../../Utility/GestionePresenze/time-sheet.service';
import { MonthData, SegmentoItem, SegmentoRichiesta } from '../../../Utility/GestionePresenze/time-sheet-common-data';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { Dip_GG_TimbraturaModel, TipoTimbratura } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { Dip_GG_GiustificativiModel, JustificationInputType } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';
import { Dip_GG_Richiesta_SetState_InModel, Dip_GG_RichiestaModel, StatoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { MonthNavigatorService } from '../../../Utility/infrastructure/month-navigator.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParGiustificativiToLongTextPipe } from '../../../shared/pipe/GestionePresenze/par-giustificativi-to-long-text.pipe';
import { TipoTimbraturaToLongTextPipe } from '../../../shared/pipe/GestionePresenze/tipo-timbratura-to-long-text.pipe';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { DateTimeUtilService } from '../../../Utility/infrastructure/date-time-util.service';
import { ModalController, NavController, Platform } from '@ionic/angular';
import { Dip_GG_ResultModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Result/Models/dip-gg-result-model';
import { FabMenuService, FabMenuItem } from '../../../Utility/infrastructure/fab-menu.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { Subscription } from 'rxjs';
import { JobNotifierService } from '../../../Utility/infrastructure/job-notifier.service';
import { GestionePresenze_JobType } from '../../../Utility/GestionePresenze/GestionePresenze_JobType';
import { NotaSpesaDettaglioDialogService } from '../../../shared/components/GestionePresenze/nota-spesa-dettaglio-dialog/nota-spesa-dettaglio-dialog.service';
import { TranslocoService } from '@jsverse/transloco';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { DipSelectorModalComponent, DipSelectorResult } from '../../../shared/components/GestionePresenze/dip-selector-modal/dip-selector-modal.component';
import { Az_SediRepartoUserModel } from '../../../ClientServer-Service/GestionePresenze/Az_SediRepartoUser/Models/az-reparto-user-model';
import { TimeSheetExportService } from '../../../ClientServer-Service/GestionePresenze/TimeSheet_ExportService/time-sheet-export.service';
import { TimeSheet_ExportPdf_4Admin_InModel } from '../../../ClientServer-Service/GestionePresenze/TimeSheet_ExportService/Models/time-sheet-export-model';



@Component({
  selector: 'app-time-sheet-admin-page',
  templateUrl: './time-sheet-admin-page.component.html',
  styleUrls: ['./time-sheet-admin-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TimeSheetAdminPageComponent implements OnInit, OnDestroy {
  public title!: string;

  TipoTimbratura = TipoTimbratura;
  StatoRichiesta = StatoRichiesta;

  private Dip_GG_Richiesta_refreshSub: Subscription;
  private commandExecutedSub: Subscription;

  currentMonth: MonthData; // Usa l'interfaccia importata
  // Usa le interfacce importate nella definizione di 'weeks'
  weeks: Array<Array<{
    day: number,
    isCurrentMonth: boolean,
    dip_GG_Timbratura: Dip_GG_TimbraturaModel[],
    dip_GG_Giustificativi: Dip_GG_GiustificativiModel[],
    dip_GG_Result?: Dip_GG_ResultModel
  }>>;


  public currYear: number
  // aggiornamento del mese quando un altro utente ricalcola il cartellino (notifica JobNotifierAsync)
  private jobSub: Subscription | null = null;
  public currMonth: number
  public currUserId: string | undefined;
  // dipendenti della sede/reparto selezionati: tra questi si scelgono quelli da stampare
  private allUsersId: string[] = [];

  /** Pezzi delle richieste su più giorni (inizio/mezzo/fine) e richiesta evidenziata al passaggio del mouse (come in usertimesheet). */
  public segmenti = new Map<SegmentoItem, SegmentoRichiesta>();
  public richiestaEvidenziata: number | null = null;
  /** Note spese del mese per giorno: compaiono solo nel giorno iniziale, con il totale. */
  public noteSpese = new Map<number, Dip_GG_RichiestaModel[]>();

  constructor(private navCtrl: NavController,
              private refresherService: RefresherService,
              public fabMenuService: FabMenuService,
              public monthNavigatorService: MonthNavigatorService,
              public timeSheetService: TimeSheetService,
              private dipGGRichiestaService: DipGGRichiestaService,
              public dateTimeUtilService: DateTimeUtilService,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private platform: Platform,
              private notaSpesaDettaglioDialogService: NotaSpesaDettaglioDialogService,
              private userInterfaceService: UserInterfaceService,
              private jobNotifierService: JobNotifierService,
              private transloco: TranslocoService,
              private modalCtrl: ModalController,
              private timeSheetExportService: TimeSheetExportService
  ) {
    this.title = this.transloco.translate('attendance.pages.timeSheetAdmin.title');
    this.weeks = [];
    // Inizializza con una struttura valida ma vuota
    this.currentMonth = { year: 0, month: 0, days: {}, dip_GG_Richiesta: [], daySlot: [], contatori_Anno_Mese :[]};

  }

  ionViewWillEnter() {

    // rientrando nella pagina (che Ionic tiene in memoria) il mese si ricarica: richieste approvate,
    // rifiutate o annullate da altre pagine (es. Richieste) non devono restare come prima.
    // Al primo ingresso il dipendente non c'e' ancora: il mese lo carica la sua selezione.
    if (this.currUserId)
      this.loadMonth();

    // ricalcolo fatto da altri (es. il dipendente fa o cancella una richiesta): si ricarica il mese
    // se e' quello del dipendente visualizzato. Una sola sottoscrizione: Ionic tiene la pagina in memoria
    if (!this.jobSub) {
      this.jobSub = this.jobNotifierService.jobFinished$.subscribe(jobUpdate => {
        if (jobUpdate.jobType === GestionePresenze_JobType.TimeSheet_Engine_Calculate &&
            this.currUserId &&
            jobUpdate.payload?.selectedUserId?.includes(this.currUserId) &&
            jobUpdate.payload?.year == this.currYear &&
            jobUpdate.payload?.month == (this.currMonth + 1))
          this.loadMonth();
      });
    }

    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'calendar-number-outline', () => {
        this.navCtrl.navigateForward('/requestjustificationuser', {
          state:  { currUserId: this.currUserId }
        });
      }),

      new FabMenuItem('Elemento 1', 'time-outline', () => {
        this.navCtrl.navigateForward('/requestclockinguser', {
          state:  { currUserId: this.currUserId }
        });
      }),

      // cartellini del mese in PDF: prima si scelgono i dipendenti, poi parte il job
      new FabMenuItem(this.userInterfaceService.Btn_Pdf.text, this.userInterfaceService.Btn_Pdf.image, () => {
        this.exportPdf();
      }),

    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  async exportPdf() {
    if (this.currYear === undefined || this.currMonth === undefined || this.allUsersId.length === 0)
      return;

    const modal = await this.modalCtrl.create({
      component: DipSelectorModalComponent,
      componentProps: {
        mode: 'multi',
        userList: this.allUsersId.map(id => ({ idAspNetUsers: id }) as Az_SediRepartoUserModel),
        preselected: this.currUserId ? [this.currUserId] : []
      }
    });
    await modal.present();

    const { data, role } = await modal.onWillDismiss<DipSelectorResult>();
    if (role !== 'confirm' || !data || data.selected.length === 0)
      return;

    const request = new GenericRequest<TimeSheet_ExportPdf_4Admin_InModel>(TimeSheet_ExportPdf_4Admin_InModel);
    request.data.timeSheet_ExportPdf.year = this.currYear;
    request.data.timeSheet_ExportPdf.month = this.currMonth + 1;   // currMonth e' 0-11
    request.data.timeSheet_ExportPdf.selectedUserId = data.selected;

    this.timeSheetExportService.ExportPdf_4Admin(request).subscribe();
  }

  ngOnInit() {

    // Registrazione all'observable per ricevere notifiche di ref
    this.Dip_GG_Richiesta_refreshSub = this.refresherService.Dip_GG_Richiesta_refresh$.subscribe(() => {
      this.loadMonth();
    });

    // comando dell'assistente (chat o voce) eseguito: il mese si rilegge subito, senza aspettare
    // la notifica SignalR di fine ricalcolo (che aggiorna poi anche le ore)
    this.commandExecutedSub = this.fabMenuService.commandExecuted$.subscribe(() => {
      if (this.currUserId)
        this.loadMonth();
    });

  }

  ngOnDestroy() {
    // Deregistrazione per evitare memory leak
    if (this.Dip_GG_Richiesta_refreshSub) {
      this.Dip_GG_Richiesta_refreshSub.unsubscribe();
    }
    this.commandExecutedSub?.unsubscribe();

  }



  loadMonth() {

    console.log(`UserPageComponent: Loading data for: ${this.currYear}-${this.currMonth + 1} via CalendarDataService`);

    // Chiama il metodo del servizio dati
    this.timeSheetService.getMonthData(this.currYear, this.currMonth, this.currUserId,false).subscribe(monthData => {
      this.currentMonth = monthData; // monthData è già del tipo corretto MonthData
      this.buildCalendarWeeks();     // Costruisce la UI dopo aver ricevuto i dati
      this.segmenti = this.timeSheetService.segmentiDelMese(this.currentMonth);
      this.noteSpese = this.timeSheetService.noteSpeseDelMese(this.currentMonth);
    });
  }

  buildCalendarWeeks() {
    this.weeks = [];


    const firstDay = new Date(this.currYear, this.currMonth, 1);
    let dayOfWeek = firstDay.getDay() || 7;
    dayOfWeek = dayOfWeek - 1;

    const lastDay = new Date(this.currYear, this.currMonth + 1, 0).getDate();
    const prevMonthLastDay = new Date(this.currYear, this.currMonth, 0).getDate();

    let currentWeek: Array<{
      day: number,
      isCurrentMonth: boolean,
      dip_GG_Timbratura: Dip_GG_TimbraturaModel[],
      dip_GG_Giustificativi: Dip_GG_GiustificativiModel[],
      dip_GG_Result?: Dip_GG_ResultModel
    }> = [];

    // Giorni mese precedente
    for (let i = 0; i < dayOfWeek; i++) {
      const day = prevMonthLastDay - dayOfWeek + i + 1;
      currentWeek.push({
        day: day,
        isCurrentMonth: false,
        dip_GG_Timbratura: [],
        dip_GG_Giustificativi: []
      });
    }

    // Giorni mese corrente
    for (let i = 1; i <= lastDay; i++) {
      // Usa i dati da this.currentMonth popolato dal servizio
      const dayData = this.currentMonth?.days?.[i]; // Può essere undefined se non ci sono dati per quel giorno

      currentWeek.push({
        day: i,
        // Usa fallback se dayData non esiste o se le proprietà sono vuote
        // Il servizio dati dovrebbe già fornire array vuoti dove appropriato
        isCurrentMonth: true,
        dip_GG_Timbratura: dayData?.dip_GG_Timbratura || [],
        dip_GG_Giustificativi: dayData?.dip_GG_Giustificativi || [],
        dip_GG_Result: dayData?.dip_GG_Result
      });

      if (currentWeek.length === 7) {
        this.weeks.push(currentWeek);
        currentWeek = [];
      }
    }

    // Giorni mese successivo
    if (currentWeek.length > 0) {
      let nextMonthDay = 1;
      while (currentWeek.length < 7) {
        currentWeek.push({
          day: nextMonthDay,
          isCurrentMonth: false,
          dip_GG_Timbratura: [],
          dip_GG_Giustificativi: []
        });
        nextMonthDay++;
      }
      this.weeks.push(currentWeek);
    }
  }

  // Restituisce timbrature filtrate per tipo (utile per UI specifiche?)
  getTimestampsByType(records: Dip_GG_TimbraturaModel[] | undefined, type: TipoTimbratura): Dip_GG_TimbraturaModel[] {
    return records?.filter(r => r.timbraturaTipo === type) || [];
  }

  hasFullDayJustification(justifications: Dip_GG_GiustificativiModel[] | undefined): boolean {
    return justifications?.some(j => j.inputType == JustificationInputType.AllDay,) || false;
  }

  getDayClass(day: any, index: number): { [key: string]: boolean } {
    return {
      'non-current-month': !day.isCurrentMonth,
      'weekend': index > 4,
      'has-content': (day.dip_GG_Timbratura && day.dip_GG_Timbratura.length > 0) ||
        (day.dip_GG_Giustificativi && day.dip_GG_Giustificativi.length > 0),
      'full-day-justification': this.hasFullDayJustification(day.dip_GG_Giustificativi)
    };
  }

  onPeriodChange(period: { year: number, month: number } | undefined): void {
    if (period) {
      this.currYear = period.year;
      this.currMonth = period.month - 1;
      this.loadMonth();
    }
  }
  onCurrentUserChanged(userId: string[] | undefined): void {
    this.currUserId = userId[0];
    this.loadMonth();
  }
  // Metodi per gestire altri eventi dall'app-sedi-reparto-user-navigation
  onSedeChanged(sediId: number | undefined): void { }
  onRepartiChanged(repartoIds: number[] | undefined): void { }
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void {
    this.allUsersId = userIds ?? [];
  }

  //////

  isActionSheetOpen = false;
  private readonly actionSheetButtonsTutti = [
    {
      text: this.transloco.translate('attendance.pages.timeSheetAdmin.approveRequest'),
      role: 'approva',
      data: {
        action: 'approva',
      },
    },
    {
      text: this.transloco.translate('attendance.pages.timeSheetAdmin.rejectRequest'),
      role: 'rifiuta',
      data: {
        action: 'rifiuta',
      },
    },
  
  ];
  public actionSheetButtons = this.actionSheetButtonsTutti;

  private actionSheetOpenSelectObj: Dip_GG_GiustificativiModel | Dip_GG_TimbraturaModel;
  private actionSheetNotaSpese: Dip_GG_RichiestaModel | null = null;
  public actionSheetHeader = '';
  public actionSheetSubHeader = '';

  /** Nota spese del giorno iniziale: il menu apre il dettaglio, da cui si approva o si rifiuta. */
  actionSheetOpenNotaSpese(richiesta: Dip_GG_RichiestaModel) {
    this.actionSheetNotaSpese = richiesta;
    this.actionSheetHeader = this.transloco.translate('attendance.pages.timeSheetAdmin.headerNotaSpese', { date: richiesta.data, total: this.timeSheetService.notaSpeseTotaleTesto(richiesta) });
    this.actionSheetSubHeader = null;
    this.actionSheetButtons = [
      {
        // pulsante Dettaglio della collezione (UserInterfaceService)
          text: this.userInterfaceService.Btn_Dettaglio.text,
        role: 'dettaglio',
        data: {
          action: 'dettaglio',
        },
      },
    ];
    this.isActionSheetOpen = true;
  }

  actionSheetOpen(obj: any) {
    this.actionSheetNotaSpese = null;
    if ('idPar_Giustificativi' in obj) {

      const parGiustificativiToLongTextPipe = new ParGiustificativiToLongTextPipe(this.sharedParameterGestionePresenzeService);

      const giustificativo = obj as Dip_GG_GiustificativiModel;
      this.actionSheetOpenSelectObj = giustificativo;

      //const dataDa = this.dateTimeUtilService.DateTo_ggmmyyyy(giustificativo.data);
      

      this.actionSheetHeader = this.transloco.translate('attendance.pages.timeSheetAdmin.headerJustification', { type: parGiustificativiToLongTextPipe.transform(giustificativo.idPar_Giustificativi), date: this.dateTimeUtilService.DateTo_ggmmyyyy(giustificativo.data) });
      this.actionSheetSubHeader = null;

    } else if ('timbraturaTipo' in obj) {

      const tipoTimbraturaToLongTextPipe = new TipoTimbraturaToLongTextPipe();

      const timbratura = obj as Dip_GG_TimbraturaModel;
      this.actionSheetOpenSelectObj = timbratura;
      this.actionSheetHeader = this.transloco.translate('attendance.pages.timeSheetAdmin.headerClocking', { type: tipoTimbraturaToLongTextPipe.transform(timbratura.timbraturaTipo), date: this.dateTimeUtilService.DateTo_ggmmyyyy_hhmm(timbratura.timbratura) });
      this.actionSheetSubHeader = null;
    }

    // giorno fuori dal rapporto di lavoro: la richiesta si può solo rifiutare
    const fuori = this.timeSheetService.fuoriRapporto(this.currentMonth, 'timbraturaTipo' in obj ? obj.giornoCompetenza : obj.data);
    this.actionSheetButtons = fuori
      ? this.actionSheetButtonsTutti.filter(b => b.role === 'rifiuta')
      : this.actionSheetButtonsTutti;
    if (fuori)
      this.actionSheetSubHeader = this.transloco.translate('attendance.pages.timeSheetAdmin.outsideEmployment');

    this.isActionSheetOpen = true;
  }

  actionSheetExecute(event: any) {
    this.isActionSheetOpen = false;

    // dettaglio della nota spese con approva / rifiuta (il mese si rilegge dal RefresherService)
    if (event?.detail?.data?.action === 'dettaglio' && this.actionSheetNotaSpese != null) {
      this.notaSpesaDettaglioDialogService.Apri(this.actionSheetNotaSpese.id, this.dipendenteCorrente, true);
      return;
    }

    if (event?.detail?.data?.action === 'approva' || event?.detail?.data?.action === 'rifiuta') {
      let IdDip_GG_Richiesta: number[] = [];
      if ('idPar_Giustificativi' in this.actionSheetOpenSelectObj) {
        const giustificativo = this.actionSheetOpenSelectObj as Dip_GG_GiustificativiModel;
        IdDip_GG_Richiesta.push(giustificativo.idDip_GG_Richiesta);

      } else if ('timbraturaTipo' in this.actionSheetOpenSelectObj) {
        const timbratura = this.actionSheetOpenSelectObj as Dip_GG_TimbraturaModel;
        IdDip_GG_Richiesta.push(timbratura.idDip_GG_Richiesta);
      }

      if (IdDip_GG_Richiesta.length > 0) {
        let request: GenericRequest<Dip_GG_Richiesta_SetState_InModel> = new GenericRequest<Dip_GG_Richiesta_SetState_InModel>(Dip_GG_Richiesta_SetState_InModel);
        if (event?.detail?.data?.action === 'approva') {
          request.data.richiestaStato = StatoRichiesta.Approvata;
        } else if (event?.detail?.data?.action === 'rifiuta') {
          request.data.richiestaStato = StatoRichiesta.Rifiutata;
        }

        

        request.data.idDip_GG_Richiesta = IdDip_GG_Richiesta;
        this.dipGGRichiestaService.SetState(request).subscribe(res => {
          this.loadMonth();
        });
      }

    }



  }

  /** Cognome e nome del dipendente scelto. */
  private get dipendenteCorrente(): string {
    const a = (this.sharedParameterGestionePresenzeService.Dip_Anagrafica ?? []).find(x => x.idAspNetUsers === this.currUserId);
    return a ? `${a.cognome ?? ''} ${a.nome ?? ''}`.trim() : '';
  }

  public get isMobile(): boolean {
    return this.platform.width() < 576;
  }

  /** Richiesta ancora da decidere (approvabile o rifiutabile): il chip si apre al clic. */
  public Decidibile(idDip_GG_Richiesta?: number): boolean {
    const req = this.get_Dip_GG_Richiesta(idDip_GG_Richiesta);
    return !!req && (this.timeSheetService.Dip_GG_Richiesta_Admin_Can_Approve(req) || this.timeSheetService.Dip_GG_Richiesta_Admin_Can_Reject(req));
  }

  /** Intensita' del chip: la stessa regola del calendario del dipendente (ShowActionSheet), cosi' i due
   *  calendari si vedono uguali; attenuate le richieste su cui il dipendente non puo' piu' agire. */
  public Attiva(idDip_GG_Richiesta?: number): boolean {
    const req = this.get_Dip_GG_Richiesta(idDip_GG_Richiesta);
    return !!req && this.timeSheetService.Dip_GG_Richiesta_User_Can_Delete(req);
  }

  get_Dip_GG_Richiesta(idDip_GG_Richiesta?: number): Dip_GG_RichiestaModel|null{

    const req = this.currentMonth.dip_GG_Richiesta.find(x => x.id === idDip_GG_Richiesta);
    return req;

  }

}
