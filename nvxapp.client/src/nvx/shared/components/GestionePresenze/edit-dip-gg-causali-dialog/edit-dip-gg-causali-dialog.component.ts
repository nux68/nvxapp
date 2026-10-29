import { Component, Input, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';
import { Observable, of } from 'rxjs';
import { StringHelperService } from '../../../../Utility/infrastructure/string-helper.service';
import { Dip_GG_CausaliModel } from '../../../../ClientServer-Service/GestionePresenze/Dip_GG_Causali/Models/dip-gg-causali-model';
import { Par_CausaliModel, TipoValore } from '../../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';

@Component({
  selector: 'app-edit-dip-gg-causali-dialog',
  templateUrl: './edit-dip-gg-causali-dialog.component.html',
  styleUrls: ['./edit-dip-gg-causali-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
}) 
export class EditDipGGCausaliDialogComponent extends BaseDialogConfirmCancelComponent<Dip_GG_CausaliModel> {

  @Input() dip_GG_Causali: Dip_GG_CausaliModel;


  public _par_CausaliList: Par_CausaliModel[] = [];

  public dateTime: string;
  public formattedDate: string;
  public formattedTime: string;
  public originalId: number;

  constructor(
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    protected override modalCtrl: ModalController,
    private transloco: TranslocoService,
    private stringHelperService: StringHelperService,
    public sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
  ) {
    super(userInterfaceService, fb, modalCtrl);
  }

  override ionViewWillEnter() {
    super.ionViewWillEnter();
    this.originalId = this.dip_GG_Causali.id;
    this._par_CausaliList = this.sharedParameterGestionePresenzeService.Par_Causali;
  }

  get Title(): string { return this.transloco.translate('attendance.components.editDipGgCausaliDialog.title'); }

  get EditForm(): FormGroup {
    // ore per le causali a ore, un solo importo (euro) per quelle a importo
    return this.fb.group({
      idPar_Causali: [null, [Validators.required]],
      valore: [null],
      importo: [null, [Validators.min(0.01), Validators.max(99999999.99)]],
    }, { validators: (g: AbstractControl) => this.valoreValidator(g) });
  }

  /** causale scelta a importo */
  public get aImporto(): boolean {
    return this.tipoDi(this._editForm?.get('idPar_Causali')?.value) === TipoValore.Importo;
  }

  private tipoDi(idPar_Causali: number | null): TipoValore {
    const par = (this.sharedParameterGestionePresenzeService.Par_Causali ?? []).find(x => x.id === idPar_Causali);
    return par?.tipo ?? TipoValore.Orario;
  }

  /** obbligatorio il valore del tipo della causale: ore oppure importo */
  private valoreValidator(g: AbstractControl): ValidationErrors | null {
    if (this.tipoDi(g.get('idPar_Causali')?.value) === TipoValore.Importo)
      return g.get('importo')?.value ? null : { importoRequired: true };
    return g.get('valore')?.value ? null : { valoreRequired: true };
  }

  LoadData = (): Observable<Dip_GG_CausaliModel | null> => {
    
    return of(this.dip_GG_Causali);
  };

  SaveData = (editModel: Dip_GG_CausaliModel): Observable<Dip_GG_CausaliModel> => {
    return of(this._editModel);
  };



 



}
