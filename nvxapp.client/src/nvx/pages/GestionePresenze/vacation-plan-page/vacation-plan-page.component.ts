import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Observable, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { NavController } from '@ionic/angular';
import { VacationPlan_DaySlot, VacationPlan_GetInModel } from '../../../ClientServer-Service/GestionePresenze/VacationPlanService/Models/vacation-plan-model';
import { PresentStaff_GetInModel } from '../../../ClientServer-Service/GestionePresenze/PresentStaffService/Models/present-staff-model';
import { PresentStaffService } from '../../../ClientServer-Service/GestionePresenze/PresentStaffService/present-staff.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { VacationPlanService } from '../../../ClientServer-Service/GestionePresenze/VacationPlanService/vacation-plan.service';

@Component({
  selector: 'app-vacation-plan-page',
  templateUrl: './vacation-plan-page.component.html',
  styleUrls: ['./vacation-plan-page.component.scss'],
  providers: [DatePipe],
  standalone: false
})
export class VacationPlanPageComponent extends BasePageConfirmCancelComponent<VacationPlanFormData> implements OnInit {

  
  public currUserId: string[] | undefined;
  public year: number;
  public month: number;
  public daySlots: VacationPlan_DaySlot[] = [];

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private vacationPlanService: VacationPlanService
  )
  {
    super(navCtrl, userInterfaceService, fb);

    
  }


  get Title(): string { return 'Piano ferie'; }

  LoadData = (): Observable<VacationPlanFormData | null> => {
    return new Observable<VacationPlanFormData | null>((subscriber) => {
      subscriber.next(new VacationPlanFormData());
      subscriber.complete();
    });
  };

  SaveData = (editModel: VacationPlanFormData): Observable<boolean> => {
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

    const firstDay = new Date(period.year, period.month - 1, 1);
    const lastDay  = new Date(period.year, period.month, 0);

    const toIso = (d: Date) =>
      `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;

    this._editForm.patchValue({
      dal: toIso(firstDay),
      al:  toIso(lastDay)
    });

    this.handleButtontaskClick({});

  }

  onCurrentUserChanged(userId: string[] | undefined): void {}
  onSedeChanged(sediId: number | undefined): void {}
  onRepartiChanged(repartoIds: number[] | undefined): void {
    this.handleButtontaskClick({});
  }
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void {
    this.currUserId = userIds;
    this.handleButtontaskClick({});
  }

  handleButtontaskClick = async (_item: any) => {
    let request: GenericRequest<VacationPlan_GetInModel> = new GenericRequest<VacationPlan_GetInModel>(VacationPlan_GetInModel);


    request.data.vacationPlan.year = this.year;
    request.data.vacationPlan.month = this.month;
    request.data.vacationPlan.selectedUserId = this.currUserId ? this.currUserId : [];

    this.vacationPlanService.VacationPlanGet(request).subscribe(x => {

      this.daySlots = x.data.daySlots;

    });
  };

  getAll() {
    return this.daySlots;
  }

  get_StatoDay_icon(vacationPlan: VacationPlan_DaySlot): string {

    if (vacationPlan.isPresent)
      return 'checkmark-done-outline';
    else
      return 'close-outline';

  }

  get_StatoDay_color(vacationPlan: VacationPlan_DaySlot): string {

    if (vacationPlan.isPresent)
      return "var(--ion-color-success)"
    else
      return "var(--ion-color-danger)"

  }

}

export class VacationPlanFormData {
  public dal: string;
  public al: string;
}
