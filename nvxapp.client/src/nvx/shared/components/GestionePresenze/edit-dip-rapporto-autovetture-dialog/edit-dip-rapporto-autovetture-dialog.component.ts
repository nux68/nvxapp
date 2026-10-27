import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { ModalController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { Dip_RapportoAutovettureModel } from '../../../../ClientServer-Service/GestionePresenze/Dip_RapportoAutovetture/Models/dip-rapporto-autovetture-model';

/** autovettura del rapporto di lavoro (sezione "Spesa" dell'anagrafica dipendente) */
@Component({
  selector: 'app-edit-dip-rapporto-autovetture-dialog',
  templateUrl: './edit-dip-rapporto-autovetture-dialog.component.html',
  styleUrls: ['./edit-dip-rapporto-autovetture-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class EditDipRapportoAutovettureDialogComponent extends BaseDialogConfirmCancelComponent<Dip_RapportoAutovettureModel> {

  @Input() dip_RapportoAutovetture: Dip_RapportoAutovettureModel;

  constructor(protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              protected override modalCtrl: ModalController,
              private transloco: TranslocoService) {
    super(userInterfaceService, fb, modalCtrl);
  }

  get Title(): string { return this.transloco.translate('attendance.components.editDipRapportoAutovettureDialog.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      targa: [null, [Validators.maxLength(15)]],
      rimborsoKM: [null, [Validators.required, Validators.min(0), Validators.max(999999)]],
    });
  }

  LoadData = (): Observable<Dip_RapportoAutovettureModel | null> => {
    return of(this.dip_RapportoAutovetture);
  };

  SaveData = (editModel: Dip_RapportoAutovettureModel): Observable<Dip_RapportoAutovettureModel> => {
    return of(this._editModel);
  };

  public errore(campo: string, tipo: string): boolean {
    const ctrl = this._editForm.get(campo);
    return !!ctrl && ctrl.hasError(tipo) && ctrl.touched;
  }
}
