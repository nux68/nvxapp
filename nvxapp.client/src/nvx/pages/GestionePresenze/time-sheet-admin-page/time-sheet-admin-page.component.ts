import { Component, OnInit, OnDestroy } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { TimeSheetService } from '../../../Utility/GestionePresenze/time-sheet.service';
import { MonthData } from '../../../Utility/GestionePresenze/time-sheet-common-data';
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
import { NavController } from '@ionic/angular';
import { FabMenuService, FabMenuItem } from '../../../Utility/infrastructure/fab-menu.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { Subscription } from 'rxjs';



@Component({
  selector: 'app-time-sheet-admin-page',
  templateUrl: './time-sheet-admin-page.component.html',
  styleUrls: ['./time-sheet-admin-page.component.scss'],
  standalone: false
})
export class TimeSheetAdminPageComponent implements OnInit, OnDestroy {
  public title!: string;

  TipoTimbratura = TipoTimbratura;
  StatoRichiesta = StatoRichiesta;

  private Dip_GG_Richiesta_refreshSub: Subscription;

  currentMonth: MonthData; // Usa l'interfaccia importata
  // Usa le interfacce importate nella definizione di 'weeks'
  weeks: Array<Array<{
    day: number,
    isCurrentMonth: boolean,
    dip_GG_Timbratura: Dip_GG_TimbraturaModel[],
    dip_GG_Giustificativi: Dip_GG_GiustificativiModel[]
  }>>;


  public currYear: number
  public currMonth: number
  public currUserId: string | undefined;

  constructor(private navCtrl: NavController,
              private refresherService: RefresherService,
              public fabMenuService: FabMenuService,
              public monthNavigatorService: MonthNavigatorService,
              public timeSheetService: TimeSheetService,
              private dipGGRichiestaService: DipGGRichiestaService,
              public dateTimeUtilService: DateTimeUtilService,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
  ) {
    this.title = 'TimeSheetUser';
    this.weeks = [];
    // Inizializza con una struttura valida ma vuota
    this.currentMonth = { year: 0, month: 0, days: {}, dip_GG_Richiesta:[] };

  }

  ionViewWillEnter() {

  
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

    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  ngOnInit() {

    // Registrazione all'observable per ricevere notifiche di ref
    this.Dip_GG_Richiesta_refreshSub = this.refresherService.Dip_GG_Richiesta_refresh$.subscribe(() => {
      this.loadMonth();
    });

  }

  ngOnDestroy() {
    // Deregistrazione per evitare memory leak
    if (this.Dip_GG_Richiesta_refreshSub) {
      this.Dip_GG_Richiesta_refreshSub.unsubscribe();
    }

  }



  loadMonth() {

    console.log(`UserPageComponent: Loading data for: ${this.currYear}-${this.currMonth + 1} via CalendarDataService`);

    // Chiama il metodo del servizio dati
    this.timeSheetService.getMonthData(this.currYear, this.currMonth, this.currUserId).subscribe(monthData => {
      this.currentMonth = monthData; // monthData è già del tipo corretto MonthData
      this.buildCalendarWeeks();     // Costruisce la UI dopo aver ricevuto i dati
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
      dip_GG_Giustificativi: Dip_GG_GiustificativiModel[]
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
        dip_GG_Giustificativi: dayData?.dip_GG_Giustificativi || []
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
  onCurrentUserChanged(userId: string | undefined): void {
    this.currUserId = userId;
    this.loadMonth();
  }
  // Metodi per gestire altri eventi dall'app-sedi-reparto-user-navigation
  onSedeChanged(sediId: number | undefined): void { }
  onRepartiChanged(repartoIds: number[] | undefined): void { }
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void { }

  //////

  isActionSheetOpen = false;
  public actionSheetButtons = [
    {
      text: 'Approva richiesta',
      role: 'approva',
      data: {
        action: 'approva',
      },
    },
    {
      text: 'Rifiuta richiesta',
      role: 'rifiuta',
      data: {
        action: 'rifiuta',
      },
    },
  
  ];

  private actionSheetOpenSelectObj: Dip_GG_GiustificativiModel | Dip_GG_TimbraturaModel;
  public actionSheetHeader = '';
  public actionSheetSubHeader = '';

  actionSheetOpen(obj: any) {
    if ('idPar_Giustificativi' in obj) {

      const parGiustificativiToLongTextPipe = new ParGiustificativiToLongTextPipe(this.sharedParameterGestionePresenzeService);

      const giustificativo = obj as Dip_GG_GiustificativiModel;
      this.actionSheetOpenSelectObj = giustificativo;

      //const dataDa = this.dateTimeUtilService.DateTo_ggmmyyyy(giustificativo.data);
      

      this.actionSheetHeader = `Giustificativo : ${parGiustificativiToLongTextPipe.transform(giustificativo.idPar_Giustificativi)} ${this.dateTimeUtilService.DateTo_ggmmyyyy(giustificativo.data)}`;
      this.actionSheetSubHeader = null;

    } else if ('timbraturaTipo' in obj) {

      const tipoTimbraturaToLongTextPipe = new TipoTimbraturaToLongTextPipe();

      const timbratura = obj as Dip_GG_TimbraturaModel;
      this.actionSheetOpenSelectObj = timbratura;
      this.actionSheetHeader = `Timbratura : ${tipoTimbraturaToLongTextPipe.transform(timbratura.timbraturaTipo)} ${this.dateTimeUtilService.DateTo_ggmmyyyy_hhmm(timbratura.timbratura)}`;
      this.actionSheetSubHeader = null;
    }
    this.isActionSheetOpen = true;
  }

  actionSheetExecute(event: any) {
    this.isActionSheetOpen = false;

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

        

        request.data.IdDip_GG_Richiesta = IdDip_GG_Richiesta;
        this.dipGGRichiestaService.SetState(request).subscribe(res => {
          this.loadMonth();
        });
      }

    }



  }

  get_Dip_GG_Richiesta(idDip_GG_Richiesta?: number): Dip_GG_RichiestaModel|null{

    const req = this.currentMonth.dip_GG_Richiesta.find(x => x.id === idDip_GG_Richiesta);
    return req;

  }

}
