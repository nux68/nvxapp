import { Dip_GG_ResultModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Result/Models/dip-gg-result-model';
import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, HostListener } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { environment } from '../../../../environments/environment';
import { MonthNavigatorService } from '../../../Utility/infrastructure/month-navigator.service';
import { SignalrService } from '../../../Utility/infrastructure/signalr.service';
import { TimeSheetService } from '../../../Utility/GestionePresenze/time-sheet.service';
import { MonthData, SegmentoItem, SegmentoRichiesta } from '../../../Utility/GestionePresenze/time-sheet-common-data';
import { Dip_GG_TimbraturaModel, TipoTimbratura } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { Dip_GG_GiustificativiModel, JustificationInputType } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';
import { Dip_GG_Richiesta_SetState_InModel, Dip_GG_RichiestaModel, StatoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { ParGiustificativiToLongTextPipe } from '../../../shared/pipe/GestionePresenze/par-giustificativi-to-long-text.pipe';
import { DateTimeUtilService } from '../../../Utility/infrastructure/date-time-util.service';
import { TipoTimbraturaToLongTextPipe } from '../../../shared/pipe/GestionePresenze/tipo-timbratura-to-long-text.pipe';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { Subscription } from 'rxjs';
import { JobNotifierService } from '../../../Utility/infrastructure/job-notifier.service';
import { GestionePresenze_JobType } from '../../../Utility/GestionePresenze/GestionePresenze_JobType';
import { ModalController, NavController, Platform } from '@ionic/angular';
import { NotaSpesaDettaglioDialogService } from '../../../shared/components/GestionePresenze/nota-spesa-dettaglio-dialog/nota-spesa-dettaglio-dialog.service';
import { TranslocoService } from '@jsverse/transloco';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { TimeSheetExportService } from '../../../ClientServer-Service/GestionePresenze/TimeSheet_ExportService/time-sheet-export.service';
import { TimeSheet_ExportPdf_4User_InModel } from '../../../ClientServer-Service/GestionePresenze/TimeSheet_ExportService/Models/time-sheet-export-model';


@Component({
  selector: 'app-time-sheet-user-page',
  templateUrl: './time-sheet-user-page.component.html',
  styleUrls: ['./time-sheet-user-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})

export class TimeSheetUserPageComponent implements OnInit, OnDestroy {
  public title!: string;

  TipoTimbratura = TipoTimbratura;
  StatoRichiesta = StatoRichiesta;

  private Dip_GG_Richiesta_refreshSub: Subscription;
  private commandExecutedSub: Subscription;

  currentMonth: MonthData; // Usa l'interfaccia importata

  /** Pezzi delle richieste su più giorni (inizio/mezzo/fine) e richiesta evidenziata al passaggio del mouse. */
  public segmenti = new Map<SegmentoItem, SegmentoRichiesta>();
  public richiestaEvidenziata: number | null = null;
  /** Note spese del mese per giorno: compaiono solo nel giorno iniziale, con il totale. */
  public noteSpese = new Map<number, Dip_GG_RichiestaModel[]>();
  // Usa le interfacce importate nella definizione di 'weeks'
  weeks: Array<Array<{
    day: number,
    isCurrentMonth: boolean,
    dip_GG_Timbratura: Dip_GG_TimbraturaModel[],
    dip_GG_Giustificativi: Dip_GG_GiustificativiModel[],
    dip_GG_Result?: Dip_GG_ResultModel
  }>>;

  
  public currYear: number
  public currMonth: number
  // aggiornamento del mese quando un altro utente ricalcola il cartellino (notifica JobNotifierAsync)
  private jobSub: Subscription | null = null;
  public refreshCartellino = 0;  // fa rileggere il riepilogo del mese
  // Ionic tiene la pagina in memoria e la riusa (es. dopo un'impersonificazione): mentre e' nascosta non si ricarica
  // (l'utente del token puo' essere un altro) e al rientro il mese si rilegge con l'utente attuale
  private visibile = false;
  private daRicaricare = false;

  

  constructor(private navCtrl: NavController,
              private signalrService: SignalrService,
              private refresherService: RefresherService,
              public monthNavigatorService: MonthNavigatorService,
              public timeSheetService: TimeSheetService,
              public fabMenuService: FabMenuService,
              public dateTimeUtilService: DateTimeUtilService,
              private dipGGRichiestaService: DipGGRichiestaService,
              private jobNotifierService: JobNotifierService, /* RICVEVE LE NOTIFICHE  */
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private platform: Platform,
              private notaSpesaDettaglioDialogService: NotaSpesaDettaglioDialogService,
              private userInterfaceService: UserInterfaceService,
              private transloco: TranslocoService,
              private timeSheetExportService: TimeSheetExportService
  ) {
    this.title = this.transloco.translate('attendance.pages.timeSheetUser.title');
    this.weeks = [];
    // Inizializza con una struttura valida ma vuota
    this.currentMonth = { year: 0, month: 0, days: {}, dip_GG_Richiesta: [], daySlot: [], contatori_Anno_Mese: []};
    
  }

  // letto una volta e riletto al ridimensionamento della finestra: platform.width() (window.innerWidth)
  // forza il ricalcolo del layout e il template lo usa in ogni riga (lento, es. all'apertura dei dialog)
  private _isMobile: boolean | null = null;
  public get isMobile(): boolean {
    return this._isMobile ??= this.platform.width() < 576;
  }

  @HostListener('window:resize')
  onWindowResize(): void {
    this._isMobile = null;
  }

  ionViewWillEnter() {

    this.visibile = true;
    if (this.daRicaricare) {
      this.daRicaricare = false;
      if (this.currYear !== undefined && this.currMonth !== undefined)
        this.loadMonth();
    }

    // ricalcolo fatto da altri (es. l'HR approva o rifiuta una richiesta): il server lo notifica al dipendente
    // e il mese si ricarica se e' quello visualizzato. Una sola sottoscrizione: Ionic tiene la pagina in memoria
    if (!this.jobSub) {
      this.jobSub = this.jobNotifierService.jobFinished$.subscribe(jobUpdate => {
        if (this.visibile &&
            jobUpdate.jobType === GestionePresenze_JobType.TimeSheet_Engine_Calculate &&
            jobUpdate.payload?.year == this.currYear &&
            jobUpdate.payload?.month == (this.currMonth + 1))
          this.loadMonth();
      });
    }


    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'calendar-number-outline', () => {
        this.navCtrl.navigateForward('/requestjustificationuser', {
          state: {}
        });
      }),

      new FabMenuItem('Elemento 1', 'time-outline', () => {
        this.navCtrl.navigateForward('/requestclockinguser', {
          state: {}
        });
      }),

      // cartellino del mese visualizzato in PDF (job in background, si scarica dalla notifica di fine job)
      new FabMenuItem(this.userInterfaceService.Btn_Pdf.text, this.userInterfaceService.Btn_Pdf.image, () => {
        this.exportPdf();
      }),

    ];

    if (environment.signalR.useSignalR) {
      this.signalrService.send("SendMessage", { 'text': "ciao" });
    }

  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
    this.visibile = false;
    this.daRicaricare = true;
  }

  exportPdf() {
    if (this.currYear === undefined || this.currMonth === undefined)
      return;

    const request = new GenericRequest<TimeSheet_ExportPdf_4User_InModel>(TimeSheet_ExportPdf_4User_InModel);
    request.data.timeSheet_ExportPdf.year = this.currYear;
    request.data.timeSheet_ExportPdf.month = this.currMonth + 1;   // currMonth e' 0-11

    this.timeSheetExportService.ExportPdf_4User(request).subscribe();
  }

  ngOnInit() {

    // Registrazione all'observable per ricevere notifiche di ref
    this.Dip_GG_Richiesta_refreshSub = this.refresherService.Dip_GG_Richiesta_refresh$.subscribe(() => {
      if (this.visibile)
        this.loadMonth();
    });

    // comando dell'assistente (chat o voce) eseguito: il mese si rilegge subito, senza aspettare
    // la notifica SignalR di fine ricalcolo (che aggiorna poi anche le ore)
    this.commandExecutedSub = this.fabMenuService.commandExecuted$.subscribe(() => {
      if (this.visibile)
        this.loadMonth();
    });

  }

  ngOnDestroy() {
    // Deregistrazione per evitare memory leak
    if (this.Dip_GG_Richiesta_refreshSub) {
      this.Dip_GG_Richiesta_refreshSub.unsubscribe();
    }
    this.commandExecutedSub?.unsubscribe();
    this.jobSub?.unsubscribe();

  }


  loadMonth() {

    console.log(`UserPageComponent: Loading data for: ${this.currYear}-${this.currMonth + 1} via CalendarDataService`);

    // Chiama il metodo del servizio dati
    this.timeSheetService.getMonthData(this.currYear, this.currMonth,null,false).subscribe(monthData => {
      this.currentMonth = monthData; // monthData è già del tipo corretto MonthData
      this.buildCalendarWeeks();     // Costruisce la UI dopo aver ricevuto i dati
      this.segmenti = this.timeSheetService.segmentiDelMese(this.currentMonth);
      this.noteSpese = this.timeSheetService.noteSpeseDelMese(this.currentMonth);
      this.refreshCartellino++;
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

    this.currYear = period.year;
    this.currMonth = period.month-1;

    this.loadMonth();
  }

  onSedeChanged(sediId: number | undefined): void {}
  onRepartiChanged(repartoIds: number[] | undefined): void {}
  onCurrentUserChanged(userId: string[] | undefined): void {}
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void {}


  isActionSheetOpen = false;
  //public actionSheetButtons = [
  //  {
  //    text: 'Cancella richiesta',
  //    role: 'delete',
  //    data: {
  //      action: 'delete',
  //    },
  //  },

  //];

  public getActionSheetButtons(): any {

    // nota spese: si apre il dettaglio (spese e allegati)
    if (this.actionSheetNotaSpese != null) {
      return [
        {
          // pulsante Dettaglio della collezione (UserInterfaceService)
          text: this.userInterfaceService.Btn_Dettaglio.text,
          role: 'dettaglio',
          data: {
            action: 'dettaglio',
          },
        },
      ];
    }

    let text = this.transloco.translate('attendance.pages.timeSheetUser.deleteRequest');
    if (this.actionSheet_Dip_GG_Richiesta != null) {
      if (this.actionSheet_Dip_GG_Richiesta.revocaStato == null)
        text = this.transloco.translate('attendance.pages.timeSheetUser.deleteRequest');
      else
        text = this.transloco.translate('attendance.pages.timeSheetUser.deleteRevocation');

    }


    let actionSheetButtons = [
      {
        text: text,
        role: 'delete',
        data: {
          action: 'delete',
        },
      },

    ];

    return actionSheetButtons;

  }

  private actionSheet_Dip_GG_Richiesta: Dip_GG_RichiestaModel | null;
  private actionSheetNotaSpese: Dip_GG_RichiestaModel | null = null;
  private actionSheetOpenSelectObj: Dip_GG_GiustificativiModel | Dip_GG_TimbraturaModel;
  public actionSheetHeader = '';
  public actionSheetSubHeader = '';

  public ShowActionSheet(idDip_GG_Richiesta?: number) {

    if (idDip_GG_Richiesta != null) {

      const req = this.currentMonth.dip_GG_Richiesta.find(x => x.id === idDip_GG_Richiesta);
      if (req != null) {

        return this.timeSheetService.Dip_GG_Richiesta_User_Can_Delete(req);

      }

      return false;
    }

    return false;


  }

  

  /** Nota spese del giorno iniziale: il menu apre il dettaglio. */
  actionSheetOpenNotaSpese(richiesta: Dip_GG_RichiestaModel) {
    this.actionSheetNotaSpese = richiesta;
    this.actionSheet_Dip_GG_Richiesta = richiesta;
    this.actionSheetHeader = this.transloco.translate('attendance.pages.timeSheetUser.headerNotaSpese', { date: richiesta.data, total: this.timeSheetService.notaSpeseTotaleTesto(richiesta) });
    this.actionSheetSubHeader = null;
    this.isActionSheetOpen = true;
  }

  actionSheetOpen(obj: any) {
    this.actionSheetNotaSpese = null;
    if ('idPar_Giustificativi' in obj) {

      const parGiustificativiToLongTextPipe = new ParGiustificativiToLongTextPipe(this.sharedParameterGestionePresenzeService);

      const giustificativo = obj as Dip_GG_GiustificativiModel;
      this.actionSheetOpenSelectObj = giustificativo;
      this.actionSheetHeader = this.transloco.translate('attendance.pages.timeSheetUser.headerJustification', { type: parGiustificativiToLongTextPipe.transform(giustificativo.idPar_Giustificativi), date: this.dateTimeUtilService.DateTo_ggmmyyyy(giustificativo.data) });
      this.actionSheetSubHeader = null;

      this.actionSheet_Dip_GG_Richiesta = this.get_Dip_GG_Richiesta(giustificativo.idDip_GG_Richiesta);


    } else if ('timbraturaTipo' in obj) {

      const tipoTimbraturaToLongTextPipe = new TipoTimbraturaToLongTextPipe();

      const timbratura = obj as Dip_GG_TimbraturaModel;
      this.actionSheetOpenSelectObj = timbratura;
      this.actionSheetHeader = this.transloco.translate('attendance.pages.timeSheetUser.headerClocking', { type: tipoTimbraturaToLongTextPipe.transform(timbratura.timbraturaTipo), date: this.dateTimeUtilService.DateTo_ggmmyyyy_hhmm(timbratura.timbratura) });
      this.actionSheetSubHeader = null;

      this.actionSheet_Dip_GG_Richiesta = this.get_Dip_GG_Richiesta(timbratura.idDip_GG_Richiesta);
    }
    this.isActionSheetOpen = true;
  }

  actionSheetExecute(event: any) {
    this.isActionSheetOpen = false;

    // dettaglio della nota spese (il dipendente la vede soltanto: approva e rifiuta l'HR)
    if (event?.detail?.data?.action === 'dettaglio' && this.actionSheetNotaSpese != null) {
      this.notaSpesaDettaglioDialogService.Apri(this.actionSheetNotaSpese.id);
      return;
    }

    if (event?.detail?.data?.action === 'delete') {

      let tipoReq = this.StatoRichiesta.Cancellata;


      let IdDip_GG_Richiesta: number[] = [];
      if ('idPar_Giustificativi' in this.actionSheetOpenSelectObj) {
        const giustificativo = this.actionSheetOpenSelectObj as Dip_GG_GiustificativiModel;
        IdDip_GG_Richiesta.push(giustificativo.idDip_GG_Richiesta);

      } else if ('timbraturaTipo' in this.actionSheetOpenSelectObj) {
        const timbratura = this.actionSheetOpenSelectObj as Dip_GG_TimbraturaModel;
        IdDip_GG_Richiesta.push(timbratura.idDip_GG_Richiesta);
      }

      if (IdDip_GG_Richiesta.length> 0) {
        let request: GenericRequest<Dip_GG_Richiesta_SetState_InModel> = new GenericRequest<Dip_GG_Richiesta_SetState_InModel>(Dip_GG_Richiesta_SetState_InModel);

        const item = this.currentMonth.dip_GG_Richiesta.find(x => x.id === IdDip_GG_Richiesta[0]);
        if (item != null) {

          if (item.richiestaStato == this.StatoRichiesta.Approvata && (item.revocaStato == null || (item.revocaStato !== null && item.revocaStato == this.StatoRichiesta.Cancellata)))
            tipoReq = StatoRichiesta.Immessa; //revoca
          else
            tipoReq = StatoRichiesta.Cancellata;
        }

        request.data.richiestaStato = tipoReq; 

        request.data.idDip_GG_Richiesta = IdDip_GG_Richiesta;
        this.dipGGRichiestaService.SetState(request).subscribe(res => {
          this.loadMonth();
        });
      }

    }

    

  }

  get_Dip_GG_Richiesta(idDip_GG_Richiesta?: number): Dip_GG_RichiestaModel | null {

    const req = this.currentMonth.dip_GG_Richiesta.find(x => x.id === idDip_GG_Richiesta);
    return req;

  }

}
