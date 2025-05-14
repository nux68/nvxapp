import { Component, OnInit } from '@angular/core';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';

@Component({
  selector: 'app-time-sheet-admin-page',
  templateUrl: './time-sheet-admin-page.component.html',
  styleUrls: ['./time-sheet-admin-page.component.scss'],
  standalone:false
})
export class TimeSheetAdminPageComponent implements OnInit {

  

  public title!: string;

  constructor(public userNavigationService: UserNavigationService) {
    this.title = 'TimeSheetAdmin';
  }

  ionViewWillEnter() {
  }


  ngOnInit() { }


  onPeriodChange(period: { year: number, month: number } | undefined): void {
    //this.selectedSedeId = sediId;
    console.log('Parent: Period changed to:', period.year + period.month);
    //this.fetchRelevantData();
  }

  onSedeChanged(sediId: number | undefined): void {
    //this.selectedSedeId = sediId;
    //console.log('Parent: Sede ID changed to:', sediId);
    //this.fetchRelevantData();
  }
  onRepartiChanged(repartoIds: number[] | undefined): void {
    //this.selectedRepartoIds = repartoIds;
    //console.log('Parent: Reparto IDs changed to:', repartoIds);
    // Note: User list will be re-evaluated by the navigation component,
    // leading to onCurrentUserChanged potentially being called.
    // We might not need to call fetchRelevantData() here if onCurrentUserChanged also calls it.
    // However, if we want to show data aggregated by reparti even if no user is selected,
    // then we might fetch here. For this example, let's assume we act on user change.
  }
  onCurrentUserChanged(userId: string | undefined): void {
    //this.currentNavigationUserId = userId;
    //console.log('Parent: Current User ID changed to:', userId);
    //this.fetchRelevantData();
  }

  onAllUsersInSelectionChanged(userIds: string[] | undefined): void {
    //console.log('Parent: All available User IDs in current selection:', userIds);

  }



}
