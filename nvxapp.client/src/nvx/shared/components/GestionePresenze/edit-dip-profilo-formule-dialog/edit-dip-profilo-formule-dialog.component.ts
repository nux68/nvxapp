import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, of } from 'rxjs';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { Dip_ProfiloFormuleModel, Par_ProfiloFormuleModel } from '../../../../ClientServer-Service/GestionePresenze/Par_ProfiloFormule/Models/par-profilo-formule-model';

// periodo del profilo formule di un rapporto di lavoro (anagrafica dipendente): i periodi non si sovrappongono
@Component({
  selector: 'app-edit-dip-profilo-formule-dialog',
  templateUrl: './edit-dip-profilo-formule-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class EditDipProfiloFormuleDialogComponent extends BaseDialogConfirmCancelComponent<Dip_ProfiloFormuleModel> {

  @Input() dip_ProfiloFormule!: Dip_ProfiloFormuleModel;
  @Input() profili: Par_ProfiloFormuleModel[] = [];
  /** altri periodi dello stesso rapporto, per il controllo di sovrapposizione */
  @Input() altriPeriodi: Dip_ProfiloFormuleModel[] = [];

  constructor(protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              protected override modalCtrl: ModalController,
              private transloco: TranslocoService) {
    super(userInterfaceService, fb, modalCtrl);
  }

  get Title(): string { return this.transloco.translate('attendance.components.editDipProfiloFormuleDialog.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      idPar_ProfiloFormule: [null, [Validators.required, Validators.min(1)]],
      dal: [null, [Validators.required]],
      al: [null, [Validators.required]],
    }, { validators: (g: AbstractControl) => this.controllaPeriodo(g) });
  }

  private controllaPeriodo(g: AbstractControl): ValidationErrors | null {
    const dal = (g.get('dal')?.value ?? '').substring(0, 10);
    const al = (g.get('al')?.value ?? '').substring(0, 10);
    if (!dal || !al) return null;
    if (al < dal) return { periodo: true };
    const sovrapposto = this.altriPeriodi.some(p => p.id !== this.dip_ProfiloFormule?.id &&
      (p.dal ?? '').substring(0, 10) <= al && (p.al ?? '').substring(0, 10) >= dal);
    return sovrapposto ? { sovrapposto: true } : null;
  }

  LoadData = (): Observable<Dip_ProfiloFormuleModel | null> => {
    this.dip_ProfiloFormule.dal = (this.dip_ProfiloFormule.dal ?? '').substring(0, 10);
    this.dip_ProfiloFormule.al = (this.dip_ProfiloFormule.al ?? '').substring(0, 10);
    return of(this.dip_ProfiloFormule);
  };

  SaveData = (editModel: Dip_ProfiloFormuleModel): Observable<Dip_ProfiloFormuleModel> => of(this._editModel!);
}
