import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Observable, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { NavController } from '@ionic/angular';
import { PresentStaffService } from '../../../ClientServer-Service/GestionePresenze/PresentStaffService/present-staff.service';
import { PresentStaff_DaySlot, PresentStaff_GetInModel } from '../../../ClientServer-Service/GestionePresenze/PresentStaffService/Models/present-staff-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { TimeSheetService } from '../../../Utility/GestionePresenze/time-sheet.service';

@Component({
  selector: 'app-present-staff-page',
  templateUrl: './present-staff-page.component.html',
  styleUrls: ['./present-staff-page.component.scss'],
  providers: [DatePipe],
  standalone: false
})
export class PresentStaffPageComponent extends BasePageConfirmCancelComponent<PresentStaffFormData> implements OnInit {

  
  public currUserId: string[] | undefined;
  public year: number;
  public month: number;
  public daySlots: PresentStaff_DaySlot[] = [];

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              private presentStaffService: PresentStaffService,
              public timeSheetService: TimeSheetService,
              protected override fb: FormBuilder)
  {
    super(navCtrl, userInterfaceService, fb);

    
  }

  

  get Title(): string { return 'Personale presente'; }

  LoadData = (): Observable<PresentStaffFormData | null> => {
    return new Observable<PresentStaffFormData | null>((subscriber) => {
      subscriber.next(new PresentStaffFormData());
      subscriber.complete();
    });
  };

  SaveData = (editModel: PresentStaffFormData): Observable<boolean> => {
    return of(true);
  };

  get EditForm(): FormGroup {
    return this.fb.group({
      dal: [null, [Validators.required]],
      al:  [null, [Validators.required]]
    });
  }

  onPeriodChange(period: { year: number, month: number } | undefined): void {
    //if (!period) return;

    //this.year  = period.year;
    //this.month = period.month;

    //const firstDay = new Date(period.year, period.month - 1, 1);
    //const lastDay  = new Date(period.year, period.month, 0);

    //const toIso = (d: Date) =>
    //  `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;

    //this._editForm.patchValue({
    //  dal: toIso(firstDay),
    //  al:  toIso(lastDay)
    //});

    //this.handleButtontaskClick({});

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
    

    let request: GenericRequest<PresentStaff_GetInModel> = new GenericRequest<PresentStaff_GetInModel>(PresentStaff_GetInModel);


    request.data.presentStaff.year = this.year;
    request.data.presentStaff.month = this.month;
    request.data.presentStaff.selectedUserId = this.currUserId ? this.currUserId : [];

    this.presentStaffService.PresentStaffGet(request).subscribe(x => {

      this.daySlots = x.data.daySlots;

    });


  };

  getAll() {
    return this.daySlots;
  }

  get_StatoDay_icon(presentStaff: PresentStaff_DaySlot): string {

    if (presentStaff.isPresent)
      return 'checkmark-done-outline';
    else
      return 'close-outline';

  }

  get_StatoDay_color(presentStaff: PresentStaff_DaySlot): string {

    if (presentStaff.isPresent)
      return "var(--ion-color-success)"
    else
      return "var(--ion-color-danger)"

  }

}

export class PresentStaffFormData {
  public dal: string;
  public al: string;
}
