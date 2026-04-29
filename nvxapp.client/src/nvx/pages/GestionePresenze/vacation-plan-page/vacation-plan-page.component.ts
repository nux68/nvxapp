import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Observable, of } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { NavController } from '@ionic/angular';

@Component({
  selector: 'app-vacation-plan-page',
  templateUrl: './vacation-plan-page.component.html',
  styleUrls: ['./vacation-plan-page.component.scss'],
  providers: [DatePipe],
  standalone: false
})
export class VacationPlanPageComponent extends BasePageConfirmCancelComponent<VacationPlanFormData> implements OnInit {

  public btnTask: ButtonItem;
  public currUserId: string[] | undefined;
  public year: number;
  public month: number;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder)
  {
    super(navCtrl, userInterfaceService, fb);

    this.btnTask = userInterfaceService.Btn_Esegui;
    this.btnTask.event = this.handleButtontaskClick;
  }

  override ngOnInit() {
    super.ngOnInit();
    this._editForm.statusChanges.subscribe(() => {
      this.btnTask.disabled = !this._editForm.valid;
    });
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
  }

  onCurrentUserChanged(userId: string[] | undefined): void {}
  onSedeChanged(sediId: number | undefined): void {}
  onRepartiChanged(repartoIds: number[] | undefined): void {}
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void {
    this.currUserId = userIds;
  }

  handleButtontaskClick = async (_item: any) => {
    // TODO: chiamare il servizio piano ferie
  };

}

export class VacationPlanFormData {
  public dal: string;
  public al: string;
}
