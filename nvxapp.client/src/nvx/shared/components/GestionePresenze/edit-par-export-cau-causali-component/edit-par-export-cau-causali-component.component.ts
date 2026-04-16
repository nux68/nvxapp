import { Component, Input, OnInit } from '@angular/core';
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
import { Par_ExportCau_CausaliModel } from '../../../../ClientServer-Service/GestionePresenze/Par_ExportCau_Causali/Models/par-export-cau-causali-model';
import { Par_CausaliModel } from '../../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';


@Component({
  selector: 'app-edit-par-export-cau-causali-component',
  templateUrl: './edit-par-export-cau-causali-component.component.html',
  styleUrls: ['./edit-par-export-cau-causali-component.component.scss'],
  standalone: false
})
export class EditParExportCauCausaliComponentComponent extends BaseDialogConfirmCancelComponent<Par_ExportCau_CausaliModel> {

  @Input() par_ExportCau_Causali: Par_ExportCau_CausaliModel;


  public _par_CausaliList: Par_CausaliModel[] = [];

  constructor(
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    protected override modalCtrl: ModalController,
    public sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
  ) {
    super(userInterfaceService, fb, modalCtrl);
    this._par_CausaliList = this.sharedParameterGestionePresenzeService.Par_Causali;
  }



  //override ionViewWillEnter() {
  //  super.ionViewWillEnter();
  //}

  get Title(): string { return "Causale export"; }

  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required]],
      idCausale: [null, [Validators.required]],
    });
  }

  LoadData = (): Observable<Par_ExportCau_CausaliModel | null> => {
    return of(this.par_ExportCau_Causali);
  };

  SaveData = (editModel: Par_ExportCau_CausaliModel): Observable<Par_ExportCau_CausaliModel> => {
    return of(this._editModel);
  };

}
