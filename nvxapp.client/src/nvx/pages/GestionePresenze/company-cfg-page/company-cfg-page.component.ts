import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable, map, catchError } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { AzCfgService } from '../../../ClientServer-Service/GestionePresenze/Az_Cfg/az-cfg.service';
import { Az_Cfg_Get_InModel, Az_Cfg_Put_InModel, Az_CfgModel, TipoApprovazione } from '../../../ClientServer-Service/GestionePresenze/Az_Cfg/Models/az-cfg-model';

interface TipoApprovazioneOption {
  value: TipoApprovazione;
  label: string;
}

@Component({
  selector: 'app-company-cfg-page',
  templateUrl: './company-cfg-page.component.html',
  styleUrls: ['./company-cfg-page.component.scss'],
  standalone: false
})
export class CompanyCfgPageComponent extends BasePageConfirmCancelComponent<Az_CfgModel> {

  public currSection: string = "first";
  modifiedDescription: string | null = null;
  tipoApprovazioneOptions: TipoApprovazioneOption[] = [
    { value: TipoApprovazione.SigleAdmin, label: 'Single Admin' },
    { value: TipoApprovazione.AllAdmin, label: 'All Admin' },
    { value: TipoApprovazione.AllAdminHierarchy, label: 'All Admin Hierarchy' }
  ];

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private stringHelperService: StringHelperService,
    private azCfgService: AzCfgService) {

    super(navCtrl, userInterfaceService, fb);

  }

  getTipoApprovazioneLabel(value: TipoApprovazione): string {
    const option = this.tipoApprovazioneOptions.find(opt => opt.value === value);
    return option ? option.label : '';
  }

  get Title(): string { return "Company cfg"; }
  get EditForm(): FormGroup {
    return this.
      fb.group({

        approvazioneTipo: [null, [Validators.required]],

      });
  }

  LoadData = (): Observable<Az_CfgModel | null> => {
    

      let request: GenericRequest<Az_Cfg_Get_InModel> = new GenericRequest<Az_Cfg_Get_InModel>(Az_Cfg_Get_InModel);
      //request.data.id = state.id;

      return this.azCfgService.Az_CfgGet(request).pipe(
        map((res) => res.data.az_Cfg), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null]; // Restituisce null in caso di errore
        })
      );
    
    
  };

  SaveData = (editModel: Az_CfgModel): Observable<boolean> => {
    let request: GenericRequest<Az_Cfg_Put_InModel> = new GenericRequest<Az_Cfg_Put_InModel>(Az_Cfg_Put_InModel);
    request.data.az_Cfg = editModel;

    return this.azCfgService.Az_CfgPut(request).pipe(
      map(() => true), // Restituisce true in caso di successo
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false]; // Restituisce false in caso di errore
      })
    );
  };

  segmentChanged(event: any) {
    console.log('Segment cambiato:', event.detail.value);
    this.currSection = event.detail.value;
  }

  //UpdateDescription() {
  //  const descrizione = this._editForm.get('descrizione')?.value;

  //  if (descrizione) {
  //    this.modifiedDescription = "Attenzione per accedere a questa utenza verrano creati i seguenti user    ->   " + this.stringHelperService.removeSpecialCharacters(descrizione) + "_Admin" + " / " + this.stringHelperService.removeSpecialCharacters(descrizione) + "_PowerAdmin";
  //  } else {
  //    this.modifiedDescription = null;
  //  }
  //}

}


//const matchPasswords: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
//  const password = control.get('pw')?.value;
//  const confirmPassword = control.get('confirmPassword')?.value;

//  return password === confirmPassword ? null : { notMatching: true };
//};
