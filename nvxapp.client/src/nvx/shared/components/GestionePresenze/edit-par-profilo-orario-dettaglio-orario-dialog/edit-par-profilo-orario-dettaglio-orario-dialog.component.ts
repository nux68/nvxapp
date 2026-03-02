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


@Component({
  selector: 'app-edit-par-profilo-orario-dettaglio-orario-dialog',
  templateUrl: './edit-par-profilo-orario-dettaglio-orario-dialog.component.html',
  styleUrls: ['./edit-par-profilo-orario-dettaglio-orario-dialog.component.scss'],
  standalone: false
})

export class EditParProfiloOrarioDettaglioOrarioDialogComponent extends BaseDialogConfirmCancelComponent<Par_ProfiloOrarioGGModel> {

  @Input() par_ProfiloOrarioGG: Par_ProfiloOrarioGGModel;


  public _par_OrarioModelList: Par_OrarioModel[] = [];

  constructor(
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    protected override modalCtrl: ModalController,
    public sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
  ) {
    super(userInterfaceService, fb, modalCtrl);
  }

  

  override ionViewWillEnter() {
    super.ionViewWillEnter();
    this._par_OrarioModelList = this.sharedParameterGestionePresenzeService.Par_Orario;
  }

  get Title(): string { return "Orario"; }

  get EditForm(): FormGroup {
    return this.fb.group({
      idPar_Orario: [null, [Validators.required]],
    });
  }

  LoadData = (): Observable<Par_ProfiloOrarioGGModel | null> => {
    return of(this.par_ProfiloOrarioGG);
  };

  SaveData = (editModel: Par_ProfiloOrarioGGModel): Observable<Par_ProfiloOrarioGGModel> => {
    return of(this._editModel);
  };

}
