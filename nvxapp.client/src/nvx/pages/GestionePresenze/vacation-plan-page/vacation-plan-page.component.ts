import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Observable, of, Subject } from 'rxjs';
import { debounceTime, takeUntil } from 'rxjs/operators';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { NavController } from '@ionic/angular';
import { VacationPlan_DaySlot, VacationPlan_GetInModel } from '../../../ClientServer-Service/GestionePresenze/VacationPlanService/Models/vacation-plan-model';
import { Dip_GG_GiustificativiModel } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Giustificativi/Models/dip-gg-giustificativi-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { VacationPlanService } from '../../../ClientServer-Service/GestionePresenze/VacationPlanService/vacation-plan.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { TimeSheetService } from '../../../Utility/GestionePresenze/time-sheet.service';
import { Dip_GG_Richiesta_SetState_InModel, Dip_GG_RichiestaModel, StatoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { ParGiustificativiToLongTextPipe } from '../../../shared/pipe/GestionePresenze/par-giustificativi-to-long-text.pipe';
import { DateTimeUtilService } from '../../../Utility/infrastructure/date-time-util.service';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';

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
              private vacationPlanService: VacationPlanService,
              private dipGGRichiestaService: DipGGRichiestaService,
              public dateTimeUtilService: DateTimeUtilService,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              public timeSheetService: TimeSheetService
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

  /** Restituisce i numeri dei giorni del mese corrente (1..28/29/30/31) */
  getDaysInMonth(): number[] {
    if (!this.year || !this.month) return [];
    const count = new Date(this.year, this.month, 0).getDate();
    return Array.from({ length: count }, (_, i) => i + 1);
  }

  /** Restituisce i giustificativi del dipendente per il giorno dato (vuoto se assente) */
  getGiustificativiForDay(slot: VacationPlan_DaySlot, day: number): Dip_GG_GiustificativiModel[] {
    if (!slot.dip_GG_Giustificativi?.length) return [];
    return slot.dip_GG_Giustificativi.filter(g => {
      const d = new Date(g.data);
      return d.getFullYear() === this.year &&
             d.getMonth() + 1  === this.month &&
             d.getDate()        === day;
    });
  }

  /** Restituisce il nome abbreviato del giorno della settimana (Lun, Mar, ...) */
  getDayLabel(day: number): string {
    const date = new Date(this.year, this.month - 1, day);
    return date.toLocaleDateString('it-IT', { weekday: 'short' });
  }

  /** true se il giorno è sabato o domenica */
  isWeekend(day: number): boolean {
    const dow = new Date(this.year, this.month - 1, day).getDay();
    return dow === 0 || dow === 6;
  }

  get_StatoDay_icon(vacationPlan: VacationPlan_DaySlot): string {

    //if (vacationPlan.isPresent)
    //  return 'checkmark-done-outline';
    //else
      return 'close-outline';

  }

  get_StatoDay_color(vacationPlan: VacationPlan_DaySlot): string {

    //if (vacationPlan.isPresent)
    //  return "var(--ion-color-success)"
    //else
      return "var(--ion-color-danger)"

  }


  isActionSheetOpen = false;

  private actionSheetOpenSelectObj: Dip_GG_GiustificativiModel ;
  public actionSheetHeader = '';
  public actionSheetSubHeader = '';

  public actionSheetButtons = [{
    text: '',
    role: '',
    data: {
      action: '',
    },
  }
  ];

  public actionSheetButtonsRequest = [
    {
      text: 'Approva richiesta',
      role: vacationPlan_Action.richieste_PREFIX + "_" + vacationPlan_Action.approva,
      data: {
        action: vacationPlan_Action.richieste_PREFIX + "_" + vacationPlan_Action.approva,
      },
    },
    {
      text: 'Rifiuta richiesta',
      role: vacationPlan_Action.richieste_PREFIX + "_" + vacationPlan_Action.rifiuta,
      data: {
        action: vacationPlan_Action.richieste_PREFIX + "_" + vacationPlan_Action.rifiuta,
      },
    },

  ];

  get_Dip_GG_Richiesta(vacationPlan_DaySlot: VacationPlan_DaySlot,idDip_GG_Richiesta?: number ): Dip_GG_RichiestaModel | null {

    const req = vacationPlan_DaySlot.dip_GG_Richieste.find(x => x.id === idDip_GG_Richiesta);
    return req;

  }

  CanOpenMenuActionGiustificativi(vacationPlan_DaySlot: VacationPlan_DaySlot,just: Dip_GG_GiustificativiModel): boolean {

    let retval = this.timeSheetService.Dip_GG_Richiesta_Admin_Can_Approve(this.get_Dip_GG_Richiesta(vacationPlan_DaySlot,just.idDip_GG_Richiesta)) ||
      this.timeSheetService.Dip_GG_Richiesta_Admin_Can_Reject(this.get_Dip_GG_Richiesta(vacationPlan_DaySlot,just.idDip_GG_Richiesta)) ||
      just.idDip_GG_Richiesta == null;;

    return retval;


  }

  actionSheetOpen(obj: any) {
    if ('idPar_Giustificativi' in obj) { //JUST
      const giustificativo = obj as Dip_GG_GiustificativiModel;

      this.actionSheetButtons = this.actionSheetButtonsRequest;

      const parGiustificativiToLongTextPipe = new ParGiustificativiToLongTextPipe(this.sharedParameterGestionePresenzeService);


      this.actionSheetOpenSelectObj = giustificativo;
      this.actionSheetHeader = `Giustificativo : ${parGiustificativiToLongTextPipe.transform(giustificativo.idPar_Giustificativi)} ${this.dateTimeUtilService.DateTo_ggmmyyyy(giustificativo.data)}`;
      this.actionSheetSubHeader = null;
    }
    this.isActionSheetOpen = true;
  }

  actionSheetExecute(event: any) {
    this.isActionSheetOpen = false;
    if (event?.detail?.data?.action?.startsWith(vacationPlan_Action.richieste_PREFIX)) {
      let IdDip_GG_Richiesta: number[] = [];

      const giustificativo = this.actionSheetOpenSelectObj as Dip_GG_GiustificativiModel;
      IdDip_GG_Richiesta.push(giustificativo.idDip_GG_Richiesta);

      if (IdDip_GG_Richiesta.length > 0) {
        let request: GenericRequest<Dip_GG_Richiesta_SetState_InModel> = new GenericRequest<Dip_GG_Richiesta_SetState_InModel>(Dip_GG_Richiesta_SetState_InModel);

        request.data.fromHR = true;


        if (event?.detail?.data?.action.includes(vacationPlan_Action.approva)) {
          request.data.richiestaStato = StatoRichiesta.Approvata;
        } else if (event?.detail?.data?.action.includes(vacationPlan_Action.rifiuta)) {
          request.data.richiestaStato = StatoRichiesta.Rifiutata;
        }

        request.data.idDip_GG_Richiesta = IdDip_GG_Richiesta;
        this.dipGGRichiestaService.SetState(request).subscribe(res => {
          this._selectionChanged$.next();
        });
      }
    }
  }

}

enum vacationPlan_Action {

  richieste_PREFIX = "richieste",
  approva = "approva",
  rifiuta = "rifiuta",
}


export class VacationPlanFormData {
  public dal: string;
  public al: string;
}
