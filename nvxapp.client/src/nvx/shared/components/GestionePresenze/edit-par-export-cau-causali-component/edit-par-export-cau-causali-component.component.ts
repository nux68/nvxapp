import { Component, Input, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Par_ProfiloOrarioModel } from '../../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrario/Models/par-profilo-orario-model';
import { Par_ProfiloOrarioGGModel } from '../../../../ClientServer-Service/GestionePresenze/Par_ProfiloOrarioGG/Models/par-profilo-orario-gg-model';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';
import { Observable, of } from 'rxjs';
import { Az_ClienteModel } from '../../../../ClientServer-Service/GestionePresenze/Az_Cliente/Models/az-cliente-model';
import { Par_OrarioModel } from '../../../../ClientServer-Service/GestionePresenze/Par_Orario/Models/par-orario-model';
import { Par_Export_TipoElaborazione, Par_Export_TipoUnita, Par_Export_Verso, Par_ExportCau_CausaliModel } from '../../../../ClientServer-Service/GestionePresenze/Par_ExportCau_Causali/Models/par-export-cau-causali-model';
import { Par_CausaliModel, TipoValore } from '../../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';


@Component({
  selector: 'app-edit-par-export-cau-causali-component',
  templateUrl: './edit-par-export-cau-causali-component.component.html',
  styleUrls: ['./edit-par-export-cau-causali-component.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class EditParExportCauCausaliComponentComponent extends BaseDialogConfirmCancelComponent<Par_ExportCau_CausaliModel> {

  @Input() par_ExportCau_Causali: Par_ExportCau_CausaliModel;

  public par_Export_TipoElaborazione= Par_Export_TipoElaborazione;
  public par_Export_TipoUnita= Par_Export_TipoUnita;
  public par_Export_Verso = Par_Export_Verso;

  public _par_CausaliList: Par_CausaliModel[] = [];

  constructor(
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    protected override modalCtrl: ModalController,
    private transloco: TranslocoService,
    public sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
  ) {
    super(userInterfaceService, fb, modalCtrl);
    this._par_CausaliList = this.sharedParameterGestionePresenzeService.Par_Causali;
  }




  get Title(): string { return this.transloco.translate('attendance.components.editParExportCauCausaliComponent.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required]],
      idCausale: [null, [Validators.required]],
      tipoElaborazione: [null, [Validators.required]],
      tipoUnita: [null, [Validators.required]],
      // segno con cui il valore entra nel codice di output (righe con lo stesso codice si sommano)
      verso: [Par_Export_Verso.Sommatoria, [Validators.required]],
    });
  }

  /** causale scelta a importo: l'unita' e' per forza Importo (e Importo vale solo per loro) */
  public get causaleAImporto(): boolean {
    const id = this._editForm?.get('idCausale')?.value;
    return (this._par_CausaliList ?? []).find(c => c.id === id)?.tipo === TipoValore.Importo;
  }

  public onCausaleChange(): void {
    const unita = this._editForm.get('tipoUnita')?.value;
    if (this.causaleAImporto && unita !== Par_Export_TipoUnita.Importo)
      this._editForm.patchValue({ tipoUnita: Par_Export_TipoUnita.Importo });
    else if (!this.causaleAImporto && unita === Par_Export_TipoUnita.Importo)
      this._editForm.patchValue({ tipoUnita: Par_Export_TipoUnita.Ore });
  }

  LoadData = (): Observable<Par_ExportCau_CausaliModel | null> => {
    return of(this.par_ExportCau_Causali);
  };

  SaveData = (editModel: Par_ExportCau_CausaliModel): Observable<Par_ExportCau_CausaliModel> => {
    return of(this._editModel);
  };

}
