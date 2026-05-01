import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Observable, of, Subject } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { NavController } from '@ionic/angular';
import { ActivityStatisticsService } from '../../../ClientServer-Service/GestionePresenze/ActivityStatisticsService/activity-statistics.service';
import { PresentStaff_GetInModel } from '../../../ClientServer-Service/GestionePresenze/PresentStaffService/Models/present-staff-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ActivityStatistics_GetInModel } from '../../../ClientServer-Service/GestionePresenze/ActivityStatisticsService/Models/activity-statistics-model';

@Component({
  selector: 'app-activity-statistics-page',
  templateUrl: './activity-statistics-page.component.html',
  styleUrls: ['./activity-statistics-page.component.scss'],
  providers: [DatePipe],
  standalone: false
})
export class ActivityStatisticsPageComponent extends BasePageConfirmCancelComponent<ActivityStatisticsFormData> implements OnInit, OnDestroy {

  public currUserId: string[] | undefined;
  public year: number;
  public month: number;

  private _selectionChanged$ = new Subject<void>();
  private _destroy$          = new Subject<void>();

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              private activityStatisticsService: ActivityStatisticsService,
              protected override fb: FormBuilder)
  {
    super(navCtrl, userInterfaceService, fb);
  }

  override ngOnInit() {
    super.ngOnInit();

    this._selectionChanged$
      .pipe(debounceTime(300), takeUntil(this._destroy$))
      .subscribe(() => this.handleButtontaskClick(null));
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }

  get Title(): string { return 'Statistiche attività'; }

  LoadData = (): Observable<ActivityStatisticsFormData | null> => {
    return new Observable<ActivityStatisticsFormData | null>((subscriber) => {
      subscriber.next(new ActivityStatisticsFormData());
      subscriber.complete();
    });
  };

  SaveData = (editModel: ActivityStatisticsFormData): Observable<boolean> => {
    return of(true);
  };

  get EditForm(): FormGroup {
    return this.fb.group({
      dal: [null, [Validators.required]],
      al:  [null, [Validators.required]]
    });
  }

  onPeriodChange(period: { year: number, month: number } | undefined): void {
    if (!period) return;

    this.year  = period.year;
    this.month = period.month;

    this._selectionChanged$.next();
  }

  onCurrentUserChanged(userId: string[] | undefined): void {
    this._selectionChanged$.next();
  }

  onSedeChanged(sediId: number | undefined): void {
    this._selectionChanged$.next();
  }

  onRepartiChanged(repartoIds: number[] | undefined): void {
    this._selectionChanged$.next();
  }

  onAllUsersInSelectionChanged(userIds: string[] | undefined): void {
    this.currUserId = userIds;
    this._selectionChanged$.next();
  }

  handleButtontaskClick = async (_item: any) => {
    let request: GenericRequest<ActivityStatistics_GetInModel> = new GenericRequest<ActivityStatistics_GetInModel>(ActivityStatistics_GetInModel);


    request.data.activityStatistics.year = this.year;
    request.data.activityStatistics.month = this.month;
    request.data.activityStatistics.selectedUserId = this.currUserId ? this.currUserId : [];

    this.activityStatisticsService.ActivityStatisticsGet(request).subscribe(x => {

      //this.daySlots = x.data.daySlots;

    });
  };

}

export class ActivityStatisticsFormData {
  public dal: string;
  public al: string;
}
