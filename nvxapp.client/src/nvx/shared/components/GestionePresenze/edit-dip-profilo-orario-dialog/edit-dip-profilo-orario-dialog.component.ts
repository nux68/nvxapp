import { Component, Input, OnInit } from '@angular/core';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { ModalController } from '@ionic/angular';
import { Dip_ProfiloOrarioModel } from '../../../../ClientServer-Service/GestionePresenze/Dip_ProfiloOrario/Models/dip-profilo-orario-model';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';
import { BasePageConfirmCancelComponent } from '../../../../pages/_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable  } from 'rxjs';
import { of } from 'rxjs/internal/observable/of';

@Component({
  selector: 'app-edit-dip-profilo-orario-dialog',
  templateUrl: './edit-dip-profilo-orario-dialog.component.html',
  styleUrls: ['./edit-dip-profilo-orario-dialog.component.scss'],
  standalone: false
})
export class EditDipProfiloOrarioDialogComponent extends BaseDialogConfirmCancelComponent<Dip_ProfiloOrarioModel>  {

  @Input() dip_ProfiloOrario: Dip_ProfiloOrarioModel;

 
  constructor(
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              protected override modalCtrl: ModalController,
              public sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
              )
  {
    super( userInterfaceService, fb, modalCtrl);
  }


  override ionViewWillEnter() {

    if (this.dip_ProfiloOrario.idPar_ProfiloOrario == 0 && this.sharedParameterGestionePresenzeService.Par_ProfiloOrario.length>0) {

      this.dip_ProfiloOrario.idPar_ProfiloOrario = this.sharedParameterGestionePresenzeService.Par_ProfiloOrario[0].id;

    }

    super.ionViewWillEnter();
  }

  get Title(): string { return "Profilo orario"; }

  get EditForm(): FormGroup {
    return this.fb.group({
      idPar_ProfiloOrario: [this.dip_ProfiloOrario.idPar_ProfiloOrario, [Validators.required, Validators.min(1)]],
      numGiornoPartenzaCiclo: [null, [Validators.required, Validators.min(1), Validators.max(this.getDayProf())]],
    });

    

  }

  LoadData = (): Observable<Dip_ProfiloOrarioModel | null> => {
    return of(this.dip_ProfiloOrario);
  };

  SaveData = (editModel: Dip_ProfiloOrarioModel): Observable<Dip_ProfiloOrarioModel> => {
    return of(this._editModel);
  };


  getDayProf(): number {

    if (this.dip_ProfiloOrario.idPar_ProfiloOrario == 0)
      return 1;

    let prof = this.sharedParameterGestionePresenzeService.Par_ProfiloOrario.find(x => x.id == this.dip_ProfiloOrario.idPar_ProfiloOrario);

    if (prof != null)
      return prof.numGiorniCiclo;

    return 1;
    

  }


}

