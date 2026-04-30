import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Observable, of, Subject } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { NavController } from '@ionic/angular';
import { VacationPlan_DaySlot, VacationPlan_GetInModel } from '../../../ClientServer-Service/GestionePresenze/VacationPlanService/Models/vacation-plan-model';
import { PresentStaff_GetInModel } from '../../../ClientServer-Service/GestionePresenze/PresentStaffService/Models/present-staff-model';
import { PresentStaffService } from '../../../ClientServer-Service/GestionePresenze/PresentStaffService/present-staff.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { VacationPlanService } from '../../../ClientServer-Service/GestionePresenze/VacationPlanService/vacation-plan.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';

@Component({
  selector: 'app-vacation-plan-page',
  templateUrl: './vacation-plan-page.component.html',
  styleUrls: ['./vacation-plan-page.component.scss'],
  providers: [DatePipe],
  standalone: false
})
export class VacationPlanPageComponent extends BasePageConfirmCancelComponent<VacationPlanFormData> implements OnInit, OnDestroy {

  public currUserId: string[] | undefined;
  public year: number;
  public month: number;
  public daySlots: VacationPlan_DaySlot[] = [];

  private _selectionChanged$ = new Subject<void>();
  private _destroy$          = new Subject<void>();

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private sharedParams: SharedParameterGestionePresenzeService,
              private vacationPlanService: VacationPlanService
  )
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
    let request: GenericRequest<VacationPlan_GetInModel> = new GenericRequest<VacationPlan_GetInModel>(VacationPlan_GetInModel);


    request.data.vacationPlan.year = this.year;
    request.data.vacationPlan.month = this.month;
    request.data.vacationPlan.selectedUserId = this.currUserId ? this.currUserId : [];

    this.vacationPlanService.VacationPlanGet(request).subscribe(x => {

      this.daySlots = x.data.daySlots;

    });
  };

  getAll(): VacationPlan_DaySlot[] {
    return [...this.daySlots].sort((a, b) => {
      const resolve = (id: string) => {
        const dip = this.sharedParams.Dip_Anagrafica?.find(d => d.idAspNetUsers === id);
        return { cognome: dip?.cognome ?? dip?.userName ?? '', nome: dip?.nome ?? '' };
      };
      const ra = resolve(a.idAspNetUsers);
      const rb = resolve(b.idAspNetUsers);
      const cmp = ra.cognome.localeCompare(rb.cognome, 'it', { sensitivity: 'base' });
      return cmp !== 0 ? cmp : ra.nome.localeCompare(rb.nome, 'it', { sensitivity: 'base' });
    });
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
