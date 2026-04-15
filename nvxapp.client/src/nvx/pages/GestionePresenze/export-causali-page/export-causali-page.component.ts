import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { Observable, map, catchError } from 'rxjs';
import { Az_ClienteModel, Az_ClienteGetInModel } from '../../../ClientServer-Service/GestionePresenze/Az_Cliente/Models/az-cliente-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { NavController } from '@ionic/angular';

@Component({
  selector: 'app-export-causali-page',
  templateUrl: './export-causali-page.component.html',
  styleUrls: ['./export-causali-page.component.scss'],
  providers: [DatePipe],
  standalone: false
})
export class ExportCausaliPageComponent extends BasePageConfirmCancelComponent<ExportCausaliFormData> {

  
  

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder
             )
  {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return "Export causali"; }

  LoadData = (): Observable<ExportCausaliFormData | null> => {
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
      al: [null, [Validators.required]]
    });
  }

  onPeriodChange(period: { year: number, month: number } | undefined): void {
    if (!period) return;

    const firstDay = new Date(period.year, period.month - 1, 1);
    const lastDay = new Date(period.year, period.month, 0);  // giorno 0 del mese successivo = ultimo del mese corrente

    const toIso = (d: Date) =>
      `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;

    this._editForm.patchValue({
      dal: toIso(firstDay),
      al: toIso(lastDay)
    });
  }
  onCurrentUserChanged(userId: string[] | undefined): void { }
  onSedeChanged(sediId: number | undefined): void {}
  onRepartiChanged(repartoIds: number[] | undefined): void { }
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void {}


}

export class ExportCausaliFormData {
  public dal: string;
  public al: string;
}

function of(arg0: boolean): Observable<boolean> {
    throw new Error('Function not implemented.');
}
