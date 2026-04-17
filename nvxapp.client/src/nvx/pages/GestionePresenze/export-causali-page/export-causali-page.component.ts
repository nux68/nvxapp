import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Observable, map, catchError } from 'rxjs';
import { Az_ClienteModel, Az_ClienteGetInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Cliente/Models/az-cliente-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { NavController } from '@ionic/angular';
import { Par_CausaliModel } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { Par_ExportCauModel } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau/Models/par-export-cau-model';
import { TimeSheetExportService } from '../../../ClientServer-Service/GestionePresenze/TimeSheet_ExportService/time-sheet-export.service';
import { MyMokeLongJobInModel } from '../../../ClientServer-Service/Infrastructure/MyMokeLongJob/Models/my-moke-long-job-model';
import { TimeSheet_ExportInModel } from '../../../ClientServer-Service/GestionePresenze/TimeSheet_ExportService/Models/time-sheet-export-model';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

@Component({
  selector: 'app-export-causali-page',
  templateUrl: './export-causali-page.component.html',
  styleUrls: ['./export-causali-page.component.scss'],
  providers: [DatePipe],
  standalone: false
})
export class ExportCausaliPageComponent extends BasePageConfirmCancelComponent<ExportCausaliFormData> implements OnInit {

  public btnTask: ButtonItem;
  public par_ExportCauList: Par_ExportCauModel[] = [];
  public currUserId: string[] | undefined;
  public year: number;
  public month: number;
  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private collectionDialogService: CollectionDialogService,
              private timeSheetExportService: TimeSheetExportService,
              public sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
             )
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

  get Title(): string { return "Export causali"; }

  LoadData = (): Observable<ExportCausaliFormData | null> => {

    this.par_ExportCauList = this.sharedParameterGestionePresenzeService.Par_ExportCau;

    return new Observable<ExportCausaliFormData | null>((subscriber) => {
        subscriber.next(new ExportCausaliFormData());
        subscriber.complete();
      });
  };

  SaveData = (editModel: ExportCausaliFormData): Observable<boolean> => {
    return of(true);
  };

  get EditForm(): FormGroup {
    return this.fb.group({
      dal: [null, [Validators.required ]],
      al: [null, [Validators.required]],
      idPar_ExportCau: [null, [Validators.required]]
    });
  }

  onPeriodChange(period: { year: number, month: number } | undefined): void {
    if (!period) return;

    this.year = period.year;
    this.month = period.month;

    const firstDay = new Date(period.year, period.month - 1, 1);
    const lastDay = new Date(period.year, period.month, 0);  // giorno 0 del mese successivo = ultimo del mese corrente

    const toIso = (d: Date) =>
      `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;

    this._editForm.patchValue({
      dal: toIso(firstDay),
      al: toIso(lastDay)
    });
  }
  onCurrentUserChanged(userId: string[] | undefined): void {}
  onSedeChanged(sediId: number | undefined): void {}
  onRepartiChanged(repartoIds: number[] | undefined): void { }
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void {
    this.currUserId = userIds;
  }
  handleButtontaskClick = async (item: any) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog('Confermi l\'export delle causali? ');
    if (result) {

      let request: GenericRequest<TimeSheet_ExportInModel> = new GenericRequest<TimeSheet_ExportInModel>(TimeSheet_ExportInModel);

      request.data.timeSheet_Export.year = this.year;
      request.data.timeSheet_Export.month = this.month;
      request.data.timeSheet_Export.dal = this._editForm.get('dal')?.value;
      request.data.timeSheet_Export.al = this._editForm.get('al')?.value;
      request.data.timeSheet_Export.idPar_ExportCau = this._editForm.get('idPar_ExportCau')?.value;
      request.data.timeSheet_Export.selectedUserId = this.currUserId ? this.currUserId : [];

      this.timeSheetExportService.Export(request).subscribe(x => {

        let c = 0;

      });

    }

  }

}

export class ExportCausaliFormData {
  public dal: string;
  public al: string;
}

function of(arg0: boolean): Observable<boolean> {
    throw new Error('Function not implemented.');
}
