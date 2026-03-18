import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ModalController } from '@ionic/angular';
import { BaseDialogConfirmCancelComponent } from '../../../../pages/_BASE/base-dialog-confirm-cancel/base-dialog-confirm-cancel.component';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';
import { Observable, of } from 'rxjs';
import { StringHelperService } from '../../../../Utility/infrastructure/string-helper.service';
import { Dip_GG_CausaliModel } from '../../../../ClientServer-Service/GestionePresenze/Dip_GG_Causali/Models/dip-gg-causali-model';

@Component({
  selector: 'app-edit-dip-gg-causali-dialog',
  templateUrl: './edit-dip-gg-causali-dialog.component.html',
  styleUrls: ['./edit-dip-gg-causali-dialog.component.scss'],
  standalone: false
}) 
export class EditDipGGCausaliDialogComponent extends BaseDialogConfirmCancelComponent<Dip_GG_CausaliModel> {

  @Input() dip_GG_Causali: Dip_GG_CausaliModel;


  public dateTime: string;
  public formattedDate: string;
  public formattedTime: string;
  

  constructor(
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    protected override modalCtrl: ModalController,
    private stringHelperService: StringHelperService,
    public sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService
  ) {
    super(userInterfaceService, fb, modalCtrl);
  }

  override ionViewWillEnter() {
    super.ionViewWillEnter();
  }

  get Title(): string { return "Causale"; }

  get EditForm(): FormGroup {
    return this.fb.group({
      valore: [null, [Validators.required]],
    });
  }

  LoadData = (): Observable<Dip_GG_CausaliModel | null> => {
    
    return of(this.dip_GG_Causali);
  };

  SaveData = (editModel: Dip_GG_CausaliModel): Observable<Dip_GG_CausaliModel> => {
    return of(this._editModel);
  };



 



}
