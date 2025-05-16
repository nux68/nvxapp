import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { TimeSheetService } from '../../../Utility/GestionePresenze/time-sheet.service';
import { MonthData } from '../../../Utility/GestionePresenze/time-sheet-common-data';

@Component({
  selector: 'app-time-sheet-admin-page',
  templateUrl: './time-sheet-admin-page.component.html',
  styleUrls: ['./time-sheet-admin-page.component.scss'],
  standalone:false
})
export class TimeSheetAdminPageComponent implements OnInit {

  
  public currYear: number
  public currMonth: number
  public currUserId: string | undefined;

  public title!: string;

  currentMonth: MonthData; // Usa l'interfaccia importata

  constructor(private timeSheetService: TimeSheetService,
              public userNavigationService: UserNavigationService) {
    this.title = 'TimeSheetAdmin';
  }

  ionViewWillEnter() {
  }


  ngOnInit() { }

  loadMonth(/*year: number, month: number*/) {

    if (this.currUserId) {

      // Chiama il metodo del servizio dati
      this.timeSheetService.getMonthData(this.currYear, this.currMonth ).subscribe(monthData => {
        this.currentMonth = monthData; // monthData è già del tipo corretto MonthData
        //this.buildCalendarWeeks();     // Costruisce la UI dopo aver ricevuto i dati
      });

    }

  }


  onPeriodChange(period: { year: number, month: number } | undefined): void {

    this.currYear = period.year;
    this.currMonth = period.month-1;

    this.loadMonth(/*this.currYear, this.currMonth*/);

    console.log('Parent: Period changed to:', period.year + period.month);
  }

  onCurrentUserChanged(userId: string | undefined): void {

    this.currUserId = userId;

    this.loadMonth(/*this.currYear, this.currMonth*/);

  }



  onSedeChanged(sediId: number | undefined): void {}
  onRepartiChanged(repartoIds: number[] | undefined): void {}
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void {}



}
