import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, of } from 'rxjs';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { FORMULA_FASE_TESTO, FormulaFase, Par_FormuleModel } from '../../../../ClientServer-Service/GestionePresenze/Par_Formule/Models/par-formule-model';
import { Par_ProfiloFormuleDettModel } from '../../../../ClientServer-Service/GestionePresenze/Par_ProfiloFormule/Models/par-profilo-formule-model';

// riga di un profilo formule: formula, ordine di esecuzione, fase, periodo di validita', sospesa
@Component({
  selector: 'app-edit-par-profilo-formule-dett-dialog',
  templateUrl: './edit-par-profilo-formule-dett-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class EditParProfiloFormuleDettDialogComponent extends BaseDialogConfirmCancelComponent<Par_ProfiloFormuleDettModel> {

  @Input() riga!: Par_ProfiloFormuleDettModel;
  @Input() formule: Par_FormuleModel[] = [];

  public readonly fasi = [FormulaFase.Finale, FormulaFase.Giorno];
  public readonly faseTesto = FORMULA_FASE_TESTO;

  constructor(protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              protected override modalCtrl: ModalController,
              private transloco: TranslocoService) {
    super(userInterfaceService, fb, modalCtrl);
  }

  get Title(): string { return this.transloco.translate('attendance.components.editParProfiloFormuleDettDialog.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      idPar_Formule: [null, [Validators.required, Validators.min(1)]],
      ordine: [1, [Validators.required, Validators.min(1)]],
      fase: [FormulaFase.Finale, [Validators.required]],
      dal: [null, [Validators.required]],
      al: [null, [Validators.required]],
      sospesa: [false],
    });
  }

  LoadData = (): Observable<Par_ProfiloFormuleDettModel | null> => {
    this.riga.dal = (this.riga.dal ?? '').substring(0, 10);
    this.riga.al = (this.riga.al ?? '').substring(0, 10);
    return of(this.riga);
  };

  SaveData = (editModel: Par_ProfiloFormuleDettModel): Observable<Par_ProfiloFormuleDettModel> => of(this._editModel!);
}
