import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Observable, of, Subject } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { NavController } from '@ionic/angular';
import { PresentStaffService } from '../../../ClientServer-Service/GestionePresenze/PresentStaffService/present-staff.service';
import { PresentStaff_DaySlot, PresentStaff_GetInModel } from '../../../ClientServer-Service/GestionePresenze/PresentStaffService/Models/present-staff-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { TimeSheetService } from '../../../Utility/GestionePresenze/time-sheet.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';

@Component({
  selector: 'app-present-staff-page',
  templateUrl: './present-staff-page.component.html',
  styleUrls: ['./present-staff-page.component.scss'],
  providers: [DatePipe],
  standalone: false
})
export class PresentStaffPageComponent extends BasePageConfirmCancelComponent<PresentStaffFormData> implements OnInit, OnDestroy {

  public currUserId: string[] | undefined;
  public year: number;
  public month: number;
  public daySlots: PresentStaff_DaySlot[] = [];

  private _selectionChanged$ = new Subject<void>();
  private _destroy$          = new Subject<void>();

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              private presentStaffService: PresentStaffService,
              public timeSheetService: TimeSheetService,
              private sharedParams: SharedParameterGestionePresenzeService,
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
    

    let request: GenericRequest<PresentStaff_GetInModel> = new GenericRequest<PresentStaff_GetInModel>(PresentStaff_GetInModel);


    //request.data.presentStaff.year = this.year;
    //request.data.presentStaff.month = this.month;
    request.data.presentStaff.selectedUserId = this.currUserId ? this.currUserId : [];

    this.presentStaffService.PresentStaffGet(request).subscribe(x => {

      this.daySlots = x.data.daySlots;

    });


  };

  getAll(): PresentStaff_DaySlot[] {
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
