import { Component, OnDestroy, OnInit, ViewEncapsulation } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { TimeSheetService } from '../../../Utility/GestionePresenze/time-sheet.service';
import { MonthData } from '../../../Utility/GestionePresenze/time-sheet-common-data';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { Dip_GG_Timbratura_DeleteInModel, Dip_GG_TimbraturaGetInModel, Dip_GG_TimbraturaModel, Dip_GG_TimbraturaPutInModel, TipoTimbratura } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { Dip_GG_Giustificativi_DeleteInModel, Dip_GG_GiustificativiGetInModel, Dip_GG_GiustificativiModel, Dip_GG_GiustificativiPutInModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';
import { Dip_GG_Richiesta_SetState_InModel, Dip_GG_RichiestaModel, StatoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParGiustificativiToLongTextPipe } from '../../../shared/pipe/GestionePresenze/par-giustificativi-to-long-text.pipe';
import { TipoTimbraturaToLongTextPipe } from '../../../shared/pipe/GestionePresenze/tipo-timbratura-to-long-text.pipe';
import { DateTimeUtilService } from '../../../Utility/infrastructure/date-time-util.service';
import { ModalController, NavController, Platform } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { catchError, map, Subscription } from 'rxjs';
import { DatePipe } from '@angular/common';
import { TimeSheetEngineCallerComponent, TimeSheetEngineCallerData } from '../../../shared/components/GestionePresenze/time-sheet-engine-caller/time-sheet-engine-caller.component';
import { TimeSheetEngineService } from '../../../ClientServer-Service/GestionePresenze/TimeSheet_EngineService/time-sheet-engine.service';
import { Timesheet_AllData_InModel, TimeSheet_CalculateInModel } from '../../../ClientServer-Service/GestionePresenze/TimeSheet_EngineService/Models/time-sheet-engine-model';
import { LongJobNotifierService } from '../../../Utility/infrastructure/long-job-notifier.service';
import { GestionePresenze_JobType } from '../../../Utility/GestionePresenze/GestionePresenze_JobType';
import { Dip_GG_ResultModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Result/Models/dip-gg-result-model';
import { Dip_GG_Causali_DeleteInModel, Dip_GG_CausaliGetInModel, Dip_GG_CausaliModel, Dip_GG_CausaliPutInModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Causali/Models/dip-gg-causali-model';
import { ParCausaliToShortTextPipe } from '../../../shared/pipe/GestionePresenze/par-causali-to-short-text.pipe';
import { Par_Causali_DeleteInModel } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { DipGGCausaliService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Causali/dip-gg-causali.service';
import { Par_OrarioIntervalloHHModel } from '../../../ClientServer-Service/GestionePresenze/Par_OrarioIntervalloHH/Models/par-orario-intervallo-hh-model';
import { EditDipGGCausaliDialogComponent } from '../../../shared/components/GestionePresenze/edit-dip-gg-causali-dialog/edit-dip-gg-causali-dialog.component';
import { DipGGTimbraturaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/dip-gg-timbratura.service';
import { EditDipGGTimbraturaDialogComponent } from '../../../shared/components/GestionePresenze/edit-dip-gg-timbratura-dialog/edit-dip-gg-timbratura-dialog.component';
import { DipGGGiustificativiService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/dip-gg-giustificativi.service';
import { EditDipGGGiustificativiDialogComponent } from '../../../shared/components/GestionePresenze/edit-dip-gg-giustificativi-dialog/edit-dip-gg-giustificativi-dialog.component';
import { JobNotifierService } from '../../../Utility/infrastructure/job-notifier.service';


interface DayData {
  date: Date;
  dayOfMonth: number;
  dip_GG_Timbratura: Dip_GG_TimbraturaModel[];
  dip_GG_Giustificativi: Dip_GG_GiustificativiModel[];
  dip_GG_Result: Dip_GG_ResultModel;
  dip_GG_Causali: Dip_GG_CausaliModel[];
}

@Component({
  selector: 'app-time-sheet-power-admin-page',
  templateUrl: './time-sheet-power-admin-page.component.html',
  styleUrls: ['./time-sheet-power-admin-page.component.scss'],
  encapsulation: ViewEncapsulation.None,
  providers: [DatePipe],
  standalone: false
})
export class TimeSheetPowerAdminPageComponent implements OnInit, OnDestroy {


  public loadCounter: boolean;
  public currYear: number;
  public currMonth: number;
  public currUserId: string | undefined;
  public repartoIds: number[];

  public title: string;

  currentMonth: MonthData; // Interfaccia importata per i dati mensili
  currentDays: DayData[] = []; // Array ordinato per i giorni del mese

  TipoTimbratura = TipoTimbratura;
  StatoRichiesta = StatoRichiesta;

  private Dip_GG_Richiesta_refreshSub: Subscription;

  constructor(private navCtrl: NavController,
              private refresherService: RefresherService,
              public fabMenuService: FabMenuService,
              public timeSheetService: TimeSheetService,
              private dipGGRichiestaService: DipGGRichiestaService,
              public dateTimeUtilService: DateTimeUtilService,
              public userNavigationService: UserNavigationService,
              private modalCtrl: ModalController,
              private dipGGCausaliService: DipGGCausaliService,
              private dipGGTimbraturaService: DipGGTimbraturaService,
              private dipGGGiustificativiService: DipGGGiustificativiService,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private datePipe: DatePipe,
              private collectionDialogService: CollectionDialogService,
              private longJobNotifier: LongJobNotifierService,
              private jobNotifierService: JobNotifierService,
              public timeSheetEngineService: TimeSheetEngineService,
              private platform: Platform
  ) {
    this.title = 'Calendario HR';
    this.currentMonth = { year: 0, month: 0, days: {}, dip_GG_Richiesta: [], daySlot: [], contatori_Anno_Mese:[] };
  }
  
  public get isMobile(): boolean {
    return this.platform.width() < 576;
  }

  ionViewWillEnter() {
    
    //riceve le notifiche di aggiornamento dei job in corso
    this.longJobNotifier.jobFinished$.subscribe(jobUpdate => {
      console.log('Job finished:', jobUpdate);

      if (jobUpdate.jobType === GestionePresenze_JobType.TimeSheet_Engine_Calculate) {

        if ( jobUpdate.payload.year == this.currYear &&
             jobUpdate.payload.month == (this.currentMonth.month + 1) &&
             jobUpdate.payload.selectedUserId.includes(this.currUserId) ) 

        this.loadMonth();
      }

      
    });

    this.jobNotifierService.jobFinished$.subscribe(jobUpdate => {
      console.log('Job finished:', jobUpdate);

      if (jobUpdate.jobType === GestionePresenze_JobType.TimeSheet_Engine_Calculate) {

        if (jobUpdate.payload.year == this.currYear &&
          jobUpdate.payload.month == (this.currentMonth.month + 1) &&
          jobUpdate.payload.selectedUserId.includes(this.currUserId))

          this.loadMonth();
      }


    });

    




  
    this.fabMenuService.fabMenuItem = [

      new FabMenuItem('Elemento 1', 'calendar-number-outline', () => {
        this.navCtrl.navigateForward('/requestjustificationuser', {
          state: { currUserId: this.currUserId, fromHR:true }
        });
      }),

      new FabMenuItem('Elemento 1', 'time-outline', () => {
        this.navCtrl.navigateForward('/requestclockinguser', {
          state: { currUserId: this.currUserId, fromHR: true }
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

  trackByDay(index: number, day: DayData): number {
    return day.dayOfMonth;
  }

 


  loadMonth() {
    if (!this.currUserId) {
      // Se non c'è un utente selezionato, resettiamo i dati
      this.currentDays = [];
      return;
    }

    // Chiama il servizio per ottenere i dati del mese
    this.timeSheetService.getMonthData(this.currYear, this.currMonth, this.currUserId, this.loadCounter).subscribe(monthData => {
      this.loadCounter = false;
      this.currentMonth = monthData;
      this.buildDaysList();
    });

    //////////////////////////
    //let request: GenericRequest<Timesheet_AllData_InModel> = new GenericRequest<Timesheet_AllData_InModel>(Timesheet_AllData_InModel);
    //request.data = new Timesheet_AllData_InModel();
    //request.data.dal = new Date(Date.UTC(this.currYear, this.currMonth , 1)).toISOString();
    //request.data.al = new Date(Date.UTC(this.currYear, this.currMonth+1, 0)).toISOString();
    //request.data.usersId = [this.currUserId];
    //this.timeSheetEngineService.Get_Timesheet_AllData(request).subscribe(x => {
    //  var c = x;
    //});
    //////////////////////////




  }

  buildDaysList() {
    const daysInMonth = new Date(this.currYear, this.currMonth + 1, 0).getDate();

    // Se il numero di giorni è cambiato (mese diverso), ricrea l'array
    if (this.currentDays.length !== daysInMonth) {
      this.currentDays = [];
      for (let i = 1; i <= daysInMonth; i++) {
        const date = new Date(this.currYear, this.currMonth, i);
        const dayData = this.currentMonth.days[i];
        this.currentDays.push({
          date: date,
          dayOfMonth: i,
          dip_GG_Timbratura: dayData?.dip_GG_Timbratura || [],
          dip_GG_Giustificativi: dayData?.dip_GG_Giustificativi || [],
          dip_GG_Result: dayData?.dip_GG_Result,
          dip_GG_Causali: dayData?.dip_GG_Causali || [],
        });
      }
    } else {
      // Stesso mese: aggiorna i dati in-place senza toccare i riferimenti dell'array
      for (let i = 0; i < daysInMonth; i++) {
        const dayData = this.currentMonth.days[i + 1];
        this.currentDays[i].dip_GG_Timbratura = dayData?.dip_GG_Timbratura || [];
        this.currentDays[i].dip_GG_Giustificativi = dayData?.dip_GG_Giustificativi || [];
        this.currentDays[i].dip_GG_Result = dayData?.dip_GG_Result;
        this.currentDays[i].dip_GG_Causali = dayData?.dip_GG_Causali || [];
      }
    }
  }

  isWeekend(date: Date): boolean {
    const day = date.getDay();
    return day === 0 || day === 6; // 0 = domenica, 6 = sabato
  }

  


 
  onPeriodChange(period: { year: number, month: number } | undefined): void {
    if (period) {

      if (this.currYear != period.year)
        this.loadCounter = true;

      this.currYear = period.year;
      this.currMonth = period.month - 1;
      this.loadMonth();
    }
  }

  onCurrentUserChanged(userId: string[] | undefined): void {
    if (userId == undefined)
      return;

    if (this.currUserId != userId[0])
      this.loadCounter = true;

    this.currUserId = userId[0];
    this.loadMonth();
  }

  // Metodi per gestire altri eventi dall'app-sedi-reparto-user-navigation
  onSedeChanged(sediId: number | undefined): void { }
  onRepartiChanged(repartoIds: number[] | undefined): void {
    this.repartoIds = repartoIds;
  }
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void {
  }
  //////


//   enum Color {
//  Red = "RED",
//  Green = "GREEN",
//  Blue = "BLUE"
//}

  

  isActionSheetOpen = false;

  public actionSheetButtons = [{
          text: '',
          role: '',
          data: {
            action: '',
          },
        }
    ];

  public actionSheetButtonsRequest = [
    {
      text: 'Approva richiesta',
      role: actionSheet_Action.richieste_PREFIX + "_" + actionSheet_Action.approva,
      data: {
        action: actionSheet_Action.richieste_PREFIX + "_" + actionSheet_Action.approva,
      },
    },
    {
      text: 'Rifiuta richiesta',
      role: actionSheet_Action.richieste_PREFIX + "_" + actionSheet_Action.rifiuta,
      data: {
        action: actionSheet_Action.richieste_PREFIX + "_" + actionSheet_Action.rifiuta,
      },
    },

  ];
  
  public actionSheetButtonsCausali = [
    {
      text: 'Modifica causale',
      role: actionSheet_Action.Causali_PREFIX + "_" + actionSheet_Action.modificacausale,
      data: {
        action: actionSheet_Action.Causali_PREFIX + "_" + actionSheet_Action.modificacausale,
      },
    },
    {
      text: 'Cancella causale',
      role: actionSheet_Action.Causali_PREFIX + "_" + actionSheet_Action.cancellacausale,
      data: {
        action: actionSheet_Action.Causali_PREFIX + "_" + actionSheet_Action.cancellacausale,
      },
    },
    {
      text: 'Aggiungi causale',
      role: actionSheet_Action.Causali_PREFIX + "_" + actionSheet_Action.aggiungicausale,
      data: {
        action: actionSheet_Action.Causali_PREFIX + "_" + actionSheet_Action.aggiungicausale,
      },
    },

  ];

  public actionSheetButtonsTimbrature = [
    {
      text: 'Modifica timbratura',
      role: actionSheet_Action.Timbrature_PREFIX + "_" + actionSheet_Action.modificatimbratura,
      data: {
        action: actionSheet_Action.Timbrature_PREFIX + "_" + actionSheet_Action.modificatimbratura,
      },
    },
    {
      text: 'Cancella timbratura',
      role: actionSheet_Action.Timbrature_PREFIX + "_" + actionSheet_Action.cancellatimbratura,
      data: {
        action: actionSheet_Action.Timbrature_PREFIX + "_" + actionSheet_Action.cancellatimbratura,
      },
    },
    {
      text: 'Aggiungi timbratura',
      role: actionSheet_Action.Timbrature_PREFIX + "_" + actionSheet_Action.aggiungitimbratura,
      data: {
        action: actionSheet_Action.Timbrature_PREFIX + "_" + actionSheet_Action.aggiungitimbratura,
      },
    },

  ];


  public actionSheetButtonsGiustificativi = [
    {
      text: 'Modifica giustificativo',
      role: actionSheet_Action.Giustificativi_PREFIX + "_" + actionSheet_Action.modificagiustificativi,
      data: {
        action: actionSheet_Action.Giustificativi_PREFIX + "_" + actionSheet_Action.modificagiustificativi,
      },
    },
    {
      text: 'Cancella giustificativo',
      role: actionSheet_Action.Giustificativi_PREFIX + "_" + actionSheet_Action.cancellagiustificativi,
      data: {
        action: actionSheet_Action.Giustificativi_PREFIX + "_" + actionSheet_Action.cancellagiustificativi,
      },
    },
    {
      text: 'Aggiungi giustificativo',
      role: actionSheet_Action.Giustificativi_PREFIX + "_" + actionSheet_Action.aggiungigiustificativi,
      data: {
        action: actionSheet_Action.Giustificativi_PREFIX + "_" + actionSheet_Action.aggiungigiustificativi,
      },
    },

  ];



  public actionSheetButtonsDay = [


    {
      text: 'Aggiungi giustificativo al giono ###',
      role: actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Day_Add_Giustificativo,
      data: {
        action: actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Day_Add_Giustificativo,
      },
    },

    {
      text: 'Aggiungi timbratura al giono ###',
      role: actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Day_Add_Timbratura,
      data: {
        action: actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Day_Add_Timbratura,
      },
    },

    {
      text: 'Aggiungi causale al giono ###',
      role: actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Day_Add_Causale,
      data: {
        action: actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Day_Add_Causale,
      },
    },

    {
      text: 'Calcola ###',
      role: actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_X,
      data: {
        action: actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_X,
      },
    },
    {
      text: 'Calcola ###',
      role: actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_To,
      data: {
        action: actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_To,
      },
    },
    {
      text: 'Calcola ###',
      role: actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_From,
      data: {
        action: actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_From,
      },
    },
    {
      text: 'Calcola tutto il mese',
      role: actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_All,
      data: {
        action: actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_All,
      },
    },
    

  ];


  private actionSheetOpenSelectObj: Dip_GG_GiustificativiModel | Dip_GG_TimbraturaModel | DayData | Dip_GG_CausaliModel;
  public actionSheetHeader = '';
  public actionSheetSubHeader = '';

  actionSheetOpen(obj: any) {
    if ('idPar_Giustificativi' in obj) { //JUST

      const giustificativo = obj as Dip_GG_GiustificativiModel;

      if (giustificativo.idDip_GG_Richiesta == null) 
      {
        //just
        this.actionSheetButtons = this.actionSheetButtonsGiustificativi;
      }
      else
      {
        // approvazione richieste
        this.actionSheetButtons = this.actionSheetButtonsRequest;
      }
      

      const parGiustificativiToLongTextPipe = new ParGiustificativiToLongTextPipe(this.sharedParameterGestionePresenzeService);

      
      this.actionSheetOpenSelectObj = giustificativo;
      this.actionSheetHeader = `Giustificativo : ${parGiustificativiToLongTextPipe.transform(giustificativo.idPar_Giustificativi)} ${this.dateTimeUtilService.DateTo_ggmmyyyy(giustificativo.data)}`;
      this.actionSheetSubHeader = null;

    }
    else if ('timbraturaTipo' in obj) { //TIMBRATURA


      const timbratura = obj as Dip_GG_TimbraturaModel;

      if (timbratura.idDip_GG_Richiesta == null) {
        // timbrature
        this.actionSheetButtons = this.actionSheetButtonsTimbrature;
      }
      else {
        // approvazione richieste
        this.actionSheetButtons = this.actionSheetButtonsRequest;
      }

      

      const tipoTimbraturaToLongTextPipe = new TipoTimbraturaToLongTextPipe();
      
      this.actionSheetOpenSelectObj = timbratura;
      this.actionSheetHeader = `Timbratura : ${tipoTimbraturaToLongTextPipe.transform(timbratura.timbraturaTipo)} ${this.dateTimeUtilService.DateTo_ggmmyyyy_hhmm(timbratura.timbratura)}`;
      this.actionSheetSubHeader = null;


    }
    else if ('idPar_Causali' in obj) {  //CAUSALI

      this.actionSheetButtons = this.actionSheetButtonsCausali;

      const parCausaliToShortTextPipe = new ParCausaliToShortTextPipe(this.sharedParameterGestionePresenzeService);

      const causale = obj as Dip_GG_CausaliModel;
      this.actionSheetOpenSelectObj = causale;
      this.actionSheetHeader = `Giorno : ${this.dateTimeUtilService.DateTo_ggmmyyyy(new Date(causale.data))}`;
      this.actionSheetSubHeader = null;

      const dynamicButtons = JSON.parse(JSON.stringify(this.actionSheetButtonsCausali));
      dynamicButtons.find((b: any) => b.role === actionSheet_Action.Causali_PREFIX + "_" + actionSheet_Action.cancellacausale).text = `Cancella causale  :${parCausaliToShortTextPipe.transform(causale.idPar_Causali)}   ${this.dateTimeUtilService.DateTo_ggmmyyyy(new Date(causale.data))}  ${causale.valore.slice(0, 5)}`;
      dynamicButtons.find((b: any) => b.role === actionSheet_Action.Causali_PREFIX + "_" + actionSheet_Action.modificacausale).text = `Modifica causale  :${parCausaliToShortTextPipe.transform(causale.idPar_Causali)}   ${this.dateTimeUtilService.DateTo_ggmmyyyy(new Date(causale.data))}  ${causale.valore.slice(0, 5)}`;

      

      this.actionSheetButtons = dynamicButtons;
    }
    else if ('dayOfMonth' in obj) {
      //menu giorno
      
      const currDay = obj as DayData;
      this.actionSheetOpenSelectObj = currDay;
      this.actionSheetHeader = "Giorno " + this.datePipe.transform(currDay.date, 'dd EEE');

      //this.actionSheetButtons = this.actionSheetButtonsDay;

      // Clona l'array per evitare di modificare l'originale
      const dynamicButtons = JSON.parse(JSON.stringify(this.actionSheetButtonsDay));

      // Formatta il giorno per il testo del pulsante
      const dayText = this.datePipe.transform(currDay.date, 'dd');
      const dayTextExt = this.datePipe.transform(currDay.date, 'dd EEE');

      // Aggiorna dinamicamente il testo dei pulsanti
      dynamicButtons.find((b: any) => b.role === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_X).text = `Calcola ${dayTextExt}`;
      dynamicButtons.find((b: any) => b.role === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_To).text = `Calcola dal 01 al ${dayText}`;
      const lastDayText = this.datePipe.transform(new Date(this.currYear, this.currMonth + 1, 0), 'dd');
      dynamicButtons.find((b: any) => b.role === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_From).text = `Calcola dal ${dayText} al ${lastDayText}`;
      dynamicButtons.find((b: any) => b.role === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Day_Add_Causale).text = `Aggiungi causale al giorno ${dayTextExt}`;
      dynamicButtons.find((b: any) => b.role === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Day_Add_Timbratura).text = `Aggiungi timbratura al giorno ${dayTextExt}`;
      dynamicButtons.find((b: any) => b.role === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Day_Add_Giustificativo).text = `Aggiungi giustificativo al giorno ${dayTextExt}`;
     
      


      this.actionSheetButtons = dynamicButtons;
      

      this.actionSheetSubHeader = null;

    }
    this.isActionSheetOpen = true;
  }

  actionSheetExecute(event: any) {
    this.isActionSheetOpen = false;

    /*RICHIESTE*/
    if (event?.detail?.data?.action?.startsWith(actionSheet_Action.richieste_PREFIX))
    {
      let IdDip_GG_Richiesta: number[] = [];
      if ('idPar_Giustificativi' in this.actionSheetOpenSelectObj)
      {
        const giustificativo = this.actionSheetOpenSelectObj as Dip_GG_GiustificativiModel;
        IdDip_GG_Richiesta.push(giustificativo.idDip_GG_Richiesta);

      } else if ('timbraturaTipo' in this.actionSheetOpenSelectObj) {
        const timbratura = this.actionSheetOpenSelectObj as Dip_GG_TimbraturaModel;
        IdDip_GG_Richiesta.push(timbratura.idDip_GG_Richiesta);
      }

      
 


      if (IdDip_GG_Richiesta.length > 0)
      {
        let request: GenericRequest<Dip_GG_Richiesta_SetState_InModel> = new GenericRequest<Dip_GG_Richiesta_SetState_InModel>(Dip_GG_Richiesta_SetState_InModel);

        request.data.fromHR = true;


        if (event?.detail?.data?.action.includes(actionSheet_Action.approva)) {
          request.data.richiestaStato = StatoRichiesta.Approvata;
        } else if (event?.detail?.data?.action.includes(actionSheet_Action.rifiuta)) {
          request.data.richiestaStato = StatoRichiesta.Rifiutata;
        }

        request.data.idDip_GG_Richiesta = IdDip_GG_Richiesta;
        this.dipGGRichiestaService.SetState(request).subscribe(res => {
          this.loadMonth();
        });
      }

    }
    /*MENU DAY*/
    else if (event?.detail?.data?.action?.startsWith(actionSheet_Action.Calcola_Day_PREFIX))
    {

      const selectedDay = this.actionSheetOpenSelectObj as DayData;

      /* CALCOLI*  */
      if (event?.detail?.data?.action === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_All ||
          event?.detail?.data?.action === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_X ||
          event?.detail?.data?.action === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_From ||
          event?.detail?.data?.action === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_To)
      {
        let dal = "";
        let al = "";

        if (event?.detail?.data?.action === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_All)
        {
          dal = this.datePipe.transform(new Date(this.currYear, this.currMonth, 1), 'yyyy-MM-dd');
          al = this.datePipe.transform(new Date(this.currYear, this.currMonth + 1, 0), 'yyyy-MM-dd');
        } else if (event?.detail?.data?.action === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_X) {
          dal = this.datePipe.transform(selectedDay.date, 'yyyy-MM-dd');
          al = this.datePipe.transform(selectedDay.date, 'yyyy-MM-dd');
        } else if (event?.detail?.data?.action === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_From) {
          dal = this.datePipe.transform(selectedDay.date, 'yyyy-MM-dd');
          al = this.datePipe.transform(new Date(this.currYear, this.currMonth + 1, 0), 'yyyy-MM-dd');
        } else if (event?.detail?.data?.action === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Calcola_Day_To) {
          dal = this.datePipe.transform(new Date(this.currYear, this.currMonth, 1), 'yyyy-MM-dd');
          al = this.datePipe.transform(selectedDay.date, 'yyyy-MM-dd');
        }

        this.TimeSheetEngineCallerDialog_Open(dal, al);
      }


      var idDip_RapportoLavoro = this.timeSheetService.get_IdDip_RapportoLavoro(this.currentMonth, this.currUserId, selectedDay.date);
      if (idDip_RapportoLavoro == 0) {
        this.collectionDialogService.ConfirmCancelDialog(`Nessu rapporto di lavoro attivo per il ${this.datePipe.transform(selectedDay.date, "dd-MM-yyyy")}`);
      }
      else {

        /*AGGIUNGI CAUSALI*/
        if (event?.detail?.data?.action === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Day_Add_Causale) {
          const request: GenericRequest<Dip_GG_CausaliGetInModel> = new GenericRequest<Dip_GG_CausaliGetInModel>(Dip_GG_CausaliGetInModel);
          request.data.id = 0;

          request.data.data = this.datePipe.transform(selectedDay.date, "yyyy-MM-dd'T'HH:mm:ss");
          request.data.idDip_RapportoLavoro = idDip_RapportoLavoro;

          this.dipGGCausaliService.Dip_GG_Causali_Get(request).subscribe(res => {
            this.handleButtonModificaCausaleClick(res.data.dip_GG_Causali);
          });


        }

        /*AGGIUNGI TIMBRATURE*/
        if (event?.detail?.data?.action === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Day_Add_Timbratura) {
          const request: GenericRequest<Dip_GG_TimbraturaGetInModel> = new GenericRequest<Dip_GG_TimbraturaGetInModel>(Dip_GG_TimbraturaGetInModel);
          request.data.id = 0;

          request.data.data = this.datePipe.transform(selectedDay.date, "yyyy-MM-dd'T'HH:mm:ss");
          request.data.idDip_RapportoLavoro = idDip_RapportoLavoro;
          
          this.dipGGTimbraturaService.Dip_GG_Timbratura_Get(request).subscribe(res => {
            this.handleButtonModificaTimbraturaClick(res.data.dip_GG_Timbratura);
          });

        }

        /*AGGIUNGI GIUSTIFICATIVO*/
        if (event?.detail?.data?.action === actionSheet_Action.Calcola_Day_PREFIX + "_" + actionSheet_Action.Day_Add_Giustificativo) {
          const request: GenericRequest<Dip_GG_GiustificativiGetInModel> = new GenericRequest<Dip_GG_GiustificativiGetInModel>(Dip_GG_GiustificativiGetInModel);
          request.data.id = 0;

          request.data.data = this.datePipe.transform(selectedDay.date, "yyyy-MM-dd'T'HH:mm:ss");
          request.data.idDip_RapportoLavoro = idDip_RapportoLavoro;
          
          this.dipGGGiustificativiService.Dip_GG_Giustificativi_Get(request).subscribe(res => {
            this.handleButtonModificaGiustificativoClick(res.data.dip_GG_Giustificativi);
          });

        }


      }



      

      

      
    }
    /*MENU CAUSALI*/
    else if (event?.detail?.data?.action?.startsWith(actionSheet_Action.Causali_PREFIX))
    {

      if (event?.detail?.data?.action === actionSheet_Action.Causali_PREFIX + "_" + actionSheet_Action.cancellacausale)
        this.handleButtonCancellaCausaleClick(this.actionSheetOpenSelectObj);
      if (event?.detail?.data?.action === actionSheet_Action.Causali_PREFIX + "_" + actionSheet_Action.modificacausale)
        this.handleButtonModificaCausaleClick(this.actionSheetOpenSelectObj);
      if (event?.detail?.data?.action === actionSheet_Action.Causali_PREFIX + "_" + actionSheet_Action.aggiungicausale) {

        const causale = this.actionSheetOpenSelectObj as Dip_GG_CausaliModel;

        const request: GenericRequest<Dip_GG_CausaliGetInModel> = new GenericRequest<Dip_GG_CausaliGetInModel>(Dip_GG_CausaliGetInModel);
        request.data.id = 0;
        
        request.data.data = this.datePipe.transform(causale.data,"yyyy-MM-dd'T'HH:mm:ss");
        request.data.idDip_RapportoLavoro = this.timeSheetService.get_IdDip_RapportoLavoro(this.currentMonth,this.currUserId, new Date(request.data.data));

        if (request.data.idDip_RapportoLavoro == 0) {
          this.collectionDialogService.ConfirmCancelDialog(`Nessu rapporto di lavoro attivo per il ${request.data.data}`);
        }
        else {
          this.dipGGCausaliService.Dip_GG_Causali_Get(request).subscribe(res => {
            this.handleButtonModificaCausaleClick(res.data.dip_GG_Causali);
          });
        }
        

        
      }
        

    }
    /*MENU TIMBRATURA*/
    else if (event?.detail?.data?.action?.startsWith(actionSheet_Action.Timbrature_PREFIX)) {

      if (event?.detail?.data?.action === actionSheet_Action.Timbrature_PREFIX + "_" + actionSheet_Action.cancellatimbratura)
        this.handleButtonCancellaTimbraturaClick(this.actionSheetOpenSelectObj);
      if (event?.detail?.data?.action === actionSheet_Action.Timbrature_PREFIX + "_" + actionSheet_Action.modificatimbratura)
        this.handleButtonModificaTimbraturaClick(this.actionSheetOpenSelectObj);
      if (event?.detail?.data?.action === actionSheet_Action.Timbrature_PREFIX + "_" + actionSheet_Action.aggiungitimbratura) {

        const timbratura = this.actionSheetOpenSelectObj as Dip_GG_TimbraturaModel;
        

        const request: GenericRequest<Dip_GG_CausaliGetInModel> = new GenericRequest<Dip_GG_CausaliGetInModel>(Dip_GG_CausaliGetInModel);
        request.data.id = 0;
        
        request.data.data = this.datePipe.transform( timbratura.giornoCompetenza,"yyyy-MM-dd'T'HH:mm:ss");
        request.data.idDip_RapportoLavoro = this.timeSheetService.get_IdDip_RapportoLavoro(this.currentMonth,this.currUserId, new Date(request.data.data));

        this.dipGGTimbraturaService.Dip_GG_Timbratura_Get(request).subscribe(res => {
          this.handleButtonModificaTimbraturaClick(res.data.dip_GG_Timbratura);
        });

      }
      

    }
    /*MENU GIUSTIFICATIVO*/
    else if (event?.detail?.data?.action?.startsWith(actionSheet_Action.Giustificativi_PREFIX)) {

      if (event?.detail?.data?.action === actionSheet_Action.Giustificativi_PREFIX + "_" + actionSheet_Action.cancellagiustificativi)
        this.handleButtonCancellaGiustificativoClick(this.actionSheetOpenSelectObj);
      if (event?.detail?.data?.action === actionSheet_Action.Giustificativi_PREFIX + "_" + actionSheet_Action.modificagiustificativi)
        this.handleButtonModificaGiustificativoClick(this.actionSheetOpenSelectObj);
      if (event?.detail?.data?.action === actionSheet_Action.Giustificativi_PREFIX + "_" + actionSheet_Action.aggiungigiustificativi) {

        const timbratura = this.actionSheetOpenSelectObj as Dip_GG_GiustificativiModel;

        const request: GenericRequest<Dip_GG_GiustificativiGetInModel> = new GenericRequest<Dip_GG_GiustificativiGetInModel>(Dip_GG_GiustificativiGetInModel);
        request.data.id = 0;

        request.data.data = this.datePipe.transform(timbratura.data, "yyyy-MM-dd'T'HH:mm:ss");
        request.data.idDip_RapportoLavoro = this.timeSheetService.get_IdDip_RapportoLavoro(this.currentMonth, this.currUserId, new Date(request.data.data));

        this.dipGGGiustificativiService.Dip_GG_Giustificativi_Get(request).subscribe(res => {
          this.handleButtonModificaGiustificativoClick(res.data.dip_GG_Giustificativi);
        });

      }


    }



  }


  get_Dip_GG_Richiesta(idDip_GG_Richiesta?: number): Dip_GG_RichiestaModel | null {

    const req = this.currentMonth.dip_GG_Richiesta.find(x => x.id === idDip_GG_Richiesta);
    return req;

  }

  get_label_day_color(day: any): string | null {

    if (this.isWeekend(day))
      return "var(--ion-color-primary)";
    else
      return "var(--ion-color-medium)";

  }



  async TimeSheetEngineCallerDialog_Open(dal: string, al: string) {


    
    let timeSheetEngineCallerData: TimeSheetEngineCallerData = new TimeSheetEngineCallerData();

    timeSheetEngineCallerData.initialSelectedUserId = this.currUserId;
    timeSheetEngineCallerData.dal = dal;
    timeSheetEngineCallerData.al = al;
    timeSheetEngineCallerData.approva_Richieste_Timbrature = true;
    timeSheetEngineCallerData.approva_Richieste_Giustificativo = true;
    timeSheetEngineCallerData.genera_Timbrature_Mancanti = true;
    timeSheetEngineCallerData.genera_Giustificativo_Assenza = true;


    const modal = await this.modalCtrl.create({
      component: TimeSheetEngineCallerComponent,
      componentProps: {
        timeSheetEngineCallerData: timeSheetEngineCallerData
      },
    });

    await modal.present();

    const { data, role } = await modal.onWillDismiss<TimeSheetEngineCallerData | null>();

    if (role === 'confirm' && data) {
      

      let request: GenericRequest<TimeSheet_CalculateInModel> = new GenericRequest<TimeSheet_CalculateInModel>(TimeSheet_CalculateInModel);
      request.data = new TimeSheet_CalculateInModel();

      request.data.timeSheet_Calculate.year = this.currYear;
      request.data.timeSheet_Calculate.month = this.currMonth + 1;
      request.data.timeSheet_Calculate.dal = data.dal;
      request.data.timeSheet_Calculate.al = data.al;

      request.data.timeSheet_Calculate.approva_Richieste_Giustificativo = data.approva_Richieste_Giustificativo;
      request.data.timeSheet_Calculate.approva_Richieste_Timbrature = data.approva_Richieste_Timbrature;
      request.data.timeSheet_Calculate.genera_Timbrature_Mancanti = data.genera_Timbrature_Mancanti;
      request.data.timeSheet_Calculate.genera_Giustificativo_Assenza = data.genera_Giustificativo_Assenza;

      request.data.timeSheet_Calculate.selectedUserId = Array.isArray(data.currSelectedUserId)
        ? data.currSelectedUserId
        : data.currSelectedUserId !== null
          ? [data.currSelectedUserId]
          : null;


      this.timeSheetEngineService.Calculate(request).subscribe();
      

    }
  }


  handleButtonModificaCausaleClick = async (item: any) => {

    const modal = await this.modalCtrl.create({
      component: EditDipGGCausaliDialogComponent,
      componentProps: {
        dip_GG_Causali: item
      },
    });

    await modal.present();

    const { data, role } = await modal.onWillDismiss<Dip_GG_CausaliModel | null>();

    if (role === 'confirm' && data) {

      const request: GenericRequest<Dip_GG_CausaliPutInModel> = new GenericRequest<Dip_GG_CausaliPutInModel>(Dip_GG_CausaliPutInModel);
      request.data.dip_GG_Causali = data;
      this.dipGGCausaliService.Dip_GG_Causali_Put(request).subscribe(res => {
        this.loadMonth();
      });

    }

  }

  handleButtonCancellaCausaleClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione della causale');
    if (result) {

      const request: GenericRequest<Dip_GG_Causali_DeleteInModel> = new GenericRequest<Dip_GG_Causali_DeleteInModel>(Dip_GG_Causali_DeleteInModel);
      request.data.id = item.id;
      this.dipGGCausaliService.Dip_GG_Causali_Delete(request).subscribe(res => {
        this.loadMonth();
      });

    }

  }



  handleButtonModificaTimbraturaClick = async (item: any) => {

    const modal = await this.modalCtrl.create({
      component: EditDipGGTimbraturaDialogComponent,
      componentProps: {
        dip_GG_Timbratura: item
      },
    });

    await modal.present();

    const { data, role } = await modal.onWillDismiss<Dip_GG_TimbraturaModel | null>();

    if (role === 'confirm' && data) {

      const request: GenericRequest<Dip_GG_TimbraturaPutInModel> = new GenericRequest<Dip_GG_TimbraturaPutInModel>(Dip_GG_TimbraturaPutInModel);
      request.data.dip_GG_Timbratura = data;
      this.dipGGTimbraturaService.Dip_GG_Timbratura_Put(request).subscribe(res => {
        this.loadMonth();
      });

    }

  }

  handleButtonCancellaTimbraturaClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione della timbratura');
    if (result) {

      const request: GenericRequest<Dip_GG_Timbratura_DeleteInModel> = new GenericRequest<Dip_GG_Timbratura_DeleteInModel>(Dip_GG_Timbratura_DeleteInModel);
      request.data.id = item.id;
      this.dipGGTimbraturaService.Dip_GG_Timbratura_Delete(request).subscribe(res => {
        this.loadMonth();
      });

    }

  }



  handleButtonModificaGiustificativoClick = async (item: any) => {

    const modal = await this.modalCtrl.create({
      component: EditDipGGGiustificativiDialogComponent,
      componentProps: {
        dip_GG_Giustificativi: item
      },
    });

    await modal.present();

    const { data, role } = await modal.onWillDismiss<Dip_GG_GiustificativiModel | null>();

    if (role === 'confirm' && data) {

      const request: GenericRequest<Dip_GG_GiustificativiPutInModel> = new GenericRequest<Dip_GG_GiustificativiPutInModel>(Dip_GG_GiustificativiPutInModel);
      request.data.dip_GG_Giustificativi = data;
      this.dipGGGiustificativiService.Dip_GG_Giustificativi_Put(request).subscribe(res => {
        this.loadMonth();
      });

    }

  }

  handleButtonCancellaGiustificativoClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi la cancellazione del gistificativo');
    if (result) {

      const request: GenericRequest<Dip_GG_Giustificativi_DeleteInModel> = new GenericRequest<Dip_GG_Giustificativi_DeleteInModel>(Dip_GG_Giustificativi_DeleteInModel);
      request.data.id = item.id;
      this.dipGGGiustificativiService.Dip_GG_Giustificativi_Delete(request).subscribe(res => {
        this.loadMonth();
      });

    }

  }



  CanOpenMenuActionTimbratura(record: Dip_GG_TimbraturaModel): boolean {

    let retval = this.timeSheetService.Dip_GG_Richiesta_Admin_Can_Approve(this.get_Dip_GG_Richiesta(record.idDip_GG_Richiesta)) ||
                 this.timeSheetService.Dip_GG_Richiesta_Admin_Can_Reject(this.get_Dip_GG_Richiesta(record.idDip_GG_Richiesta)) ||
                 record.idDip_GG_Richiesta == null;

    return retval;
    

  }

  CanOpenMenuActionGiustificativi(just: Dip_GG_GiustificativiModel): boolean {

    let retval = this.timeSheetService.Dip_GG_Richiesta_Admin_Can_Approve(this.get_Dip_GG_Richiesta(just.idDip_GG_Richiesta)) ||
                 this.timeSheetService.Dip_GG_Richiesta_Admin_Can_Reject(this.get_Dip_GG_Richiesta(just.idDip_GG_Richiesta)) ||
                 just.idDip_GG_Richiesta == null;;

    return retval;


  }

  

}

enum actionSheet_Action {

  richieste_PREFIX = "richieste",
  approva = "approva",
  rifiuta = "rifiuta",


  Causali_PREFIX = "causali",
  modificacausale = "modificacausale",
  cancellacausale = "cancellacausale",
  aggiungicausale = "aggiungicausale",


  Timbrature_PREFIX = "timbrature",
  modificatimbratura = "modificatimbratura",
  cancellatimbratura = "cancellatimbratura",
  aggiungitimbratura = "aggiungitimbratura",

  Giustificativi_PREFIX = "giustificativi",
  modificagiustificativi = "modificagiustificativi",
  cancellagiustificativi = "cancellagiustificativi",
  aggiungigiustificativi = "aggiungigiustificativi",

  Calcola_Day_PREFIX = "Calcola_Day",  // definisce il gruppo
  Calcola_Day_X = "Calcola_Day_X",
  Calcola_Day_From = "Calcola_Day_From",
  Calcola_Day_To = "Calcola_Day_To",
  Calcola_Day_All = "Calcola_Day_All",
  Day_Add_Causale = "Day_Add_Causale",
  Day_Add_Timbratura = "Day_Add_Timbratura",
  Day_Add_Giustificativo = "Day_Add_Giustificativo",

}
