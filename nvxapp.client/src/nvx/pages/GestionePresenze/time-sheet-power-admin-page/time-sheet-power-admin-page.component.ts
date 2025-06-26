import { Component, OnDestroy, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { TimeSheetService } from '../../../Utility/GestionePresenze/time-sheet.service';
import { MonthData } from '../../../Utility/GestionePresenze/time-sheet-common-data';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { Dip_GG_TimbraturaModel, TipoTimbratura } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Timbratura/Models/dip-gg-timbratura-model';
import { Dip_GG_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';
import { Dip_GG_Richiesta_SetState_InModel, Dip_GG_RichiestaModel, StatoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParGiustificativiToLongTextPipe } from '../../../shared/pipe/GestionePresenze/par-giustificativi-to-long-text.pipe';
import { TipoTimbraturaToLongTextPipe } from '../../../shared/pipe/GestionePresenze/tipo-timbratura-to-long-text.pipe';
import { DateTimeUtilService } from '../../../Utility/infrastructure/date-time-util.service';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { Subscription } from 'rxjs';

interface DayData {
  date: Date;
  dayOfMonth: number;
  dip_GG_Timbratura: Dip_GG_TimbraturaModel[];
  dip_GG_Giustificativi: Dip_GG_GiustificativiModel[];
}

@Component({
  selector: 'app-time-sheet-power-admin-page',
  templateUrl: './time-sheet-power-admin-page.component.html',
  styleUrls: ['./time-sheet-power-admin-page.component.scss'],
  standalone: false
}) 
export class TimeSheetPowerAdminPageComponent implements OnInit, OnDestroy {

  public currYear: number;
  public currMonth: number;
  public currUserId: string | undefined;

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
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
  ) {
    this.title = 'Calendario HR';
    this.currentMonth = { year: 0, month: 0, days: {}, dip_GG_Richiesta: [] };
  }

  ionViewWillEnter() {

  
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

  loadMonth() {
    if (!this.currUserId) {
      // Se non c'è un utente selezionato, resettiamo i dati
      this.currentDays = [];
      return;
    }

    // Chiama il servizio per ottenere i dati del mese
    this.timeSheetService.getMonthData(this.currYear, this.currMonth, this.currUserId).subscribe(monthData => {
      this.currentMonth = monthData;
      this.buildDaysList();
    });
  }

  buildDaysList() {
    this.currentDays = [];

    // Ottiene il numero di giorni nel mese
    const daysInMonth = new Date(this.currYear, this.currMonth + 1, 0).getDate();

    // Crea un array di oggetti giorno per tutti i giorni del mese
    for (let i = 1; i <= daysInMonth; i++) {
      const date = new Date(this.currYear, this.currMonth, i);

      // Controlla se ci sono dati per questo giorno
      const dayData = this.currentMonth.days[i];

      this.currentDays.push({
        date: date,
        dayOfMonth: i,
        dip_GG_Timbratura: dayData?.dip_GG_Timbratura || [],
        dip_GG_Giustificativi: dayData?.dip_GG_Giustificativi || []
      });
    }
  }

  isWeekend(date: Date): boolean {
    const day = date.getDay();
    return day === 0 || day === 6; // 0 = domenica, 6 = sabato
  }

  


 
  onPeriodChange(period: { year: number, month: number } | undefined): void {
    if (period) {
      this.currYear = period.year;
      this.currMonth = period.month - 1;
      this.loadMonth();
    }
  }

  onCurrentUserChanged(userId: string[] | undefined): void {
    if (userId == undefined)
      return;
    this.currUserId = userId[0];
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

        request.data.fromHR = true;

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


  get_Dip_GG_Richiesta(idDip_GG_Richiesta?: number): Dip_GG_RichiestaModel | null {

    const req = this.currentMonth.dip_GG_Richiesta.find(x => x.id === idDip_GG_Richiesta);
    return req;

  }



}
