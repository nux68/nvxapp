import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GG_ResultStato } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Result/Models/dip-gg-result-model';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Observable, of, Subject } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { NavController } from '@ionic/angular';
import { ActivityStatisticsService } from '../../../ClientServer-Service/GestionePresenze/ActivityStatisticsService/activity-statistics.service';
import { PresentStaff_GetInModel } from '../../../ClientServer-Service/GestionePresenze/PresentStaffService/Models/present-staff-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import {
  ActivityStatistics_GetInModel,
  ActivityStatistics_GiornataEsclusaModel,
  ActivityStatistics_RowModel,
  ActivityStatistics_TotaleAttivitaModel
} from '../../../ClientServer-Service/GestionePresenze/ActivityStatisticsService/Models/activity-statistics-model';

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

  public righePerDipendente: ActivityStatistics_RowModel[]             = [];
  public totaliPerAttivita:  ActivityStatistics_TotaleAttivitaModel[]  = [];
  public giornateEscluse:    ActivityStatistics_GiornataEsclusaModel[] = [];

  public selectedSegment: string = 'dipendente';

  get hasResults(): boolean {
    return this.righePerDipendente.length > 0 ||
           this.totaliPerAttivita.length > 0  ||
           this.giornateEscluse.length > 0;
  }

  get righePerDipendente_Raggruppate(): { userId: string; nomeDipendente: string; righe: ActivityStatistics_RowModel[]; totaleMinuti: number }[] {
    const groups = new Map<string, ActivityStatistics_RowModel[]>();
    for (const r of this.righePerDipendente) {
      if (!groups.has(r.userId)) groups.set(r.userId, []);
      groups.get(r.userId)!.push(r);
    }
    return Array.from(groups.entries())
      .map(([userId, righe]) => ({
        userId,
        nomeDipendente: righe[0].nomeDipendente,
        righe: [...righe].sort((a, b) =>
          a.nomeCliente.localeCompare(b.nomeCliente) ||
          a.nomeCommessa.localeCompare(b.nomeCommessa) ||
          a.nomeAttivita.localeCompare(b.nomeAttivita)),
        totaleMinuti: righe.reduce((acc, r) => acc + r.totaleMinuti, 0)
      }))
      .sort((a, b) => a.nomeDipendente.localeCompare(b.nomeDipendente));
  }

  get totaleCompletivoMinuti(): number {
    return this.totaliPerAttivita.reduce((acc, t) => acc + t.totaleMinuti, 0);
  }

  minutiToHHmm(minuti: number): string {
    const h = Math.floor(minuti / 60);
    const m = minuti % 60;
    return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}`;
  }

  statoColor(stato: GG_ResultStato): string {
    if (stato & GG_ResultStato.Err)  return 'danger';
    if (stato & GG_ResultStato.Init) return 'medium';
    return 'warning';
  }

  statoIcon(stato: GG_ResultStato): string {
    if (stato & GG_ResultStato.Err)  return 'close-circle-outline';
    if (stato & GG_ResultStato.Init) return 'hourglass-outline';
    return 'warning-outline';
  }

  statoLabel(stato: GG_ResultStato): string {
    if (stato & GG_ResultStato.Err)  return 'Errore';
    if (stato & GG_ResultStato.Init) return 'Non elaborata';
    return 'Warning';
  }

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
      if (x?.data) {
        this.righePerDipendente = x.data.righePerDipendente;
        this.totaliPerAttivita  = x.data.totaliPerAttivita;
        this.giornateEscluse    = x.data.giornateEscluse;
        this.selectedSegment    = 'dipendente';
      }
    });
  };

}

export class ActivityStatisticsFormData {
  public dal: string;
  public al: string;
}
