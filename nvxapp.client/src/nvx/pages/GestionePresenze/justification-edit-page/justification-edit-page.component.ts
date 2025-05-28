import { Component, OnInit } from '@angular/core';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NavController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { Observable } from 'rxjs/internal/Observable';
import { map, catchError } from 'rxjs';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { ParameterService } from '../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { Par_GiustificativiGetInModel, Par_GiustificativiInModel, Par_GiustificativiModel, Par_GiustificativiPutInModel } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/Models/par-giustificativi-model';
import { ParGiustificativiService } from '../../../ClientServer-Service/GestionePresenze/Par_Giustificativi/par-giustificativi.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';

@Component({
  selector: 'app-justification-edit-page',
  templateUrl: './justification-edit-page.component.html',
  styleUrls: ['./justification-edit-page.component.scss'],
  standalone: false
}) 
export class JustificationEditPageComponent extends BasePageConfirmCancelComponent<Par_GiustificativiModel> {

  

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parameterService: ParameterService,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private stringHelperService: StringHelperService,
    private parGiustificativiService: ParGiustificativiService,
    private refresherService: RefresherService) {

    super(navCtrl, userInterfaceService, fb);

  }


  get Title(): string { return "Justification"; }
  get EditForm(): FormGroup {
    return this.fb.group({

      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      codice: [null, [Validators.required, Validators.maxLength(5)]],
      backgroundColor: [null, []],
      textColor: [null, []],

      //roleId: [null, [Validators.required]],

    });
  }

  LoadData = (): Observable<Par_GiustificativiModel | null> => {
    const state = history.state;


    if (state && state.id) {
      let request: GenericRequest<Par_GiustificativiGetInModel> = new GenericRequest<Par_GiustificativiGetInModel>(Par_GiustificativiGetInModel);
      request.data.id = state.id;

      return this.parGiustificativiService.Par_GiustificativiGet(request).pipe(
        map((res) => res.data.par_Giustificativi), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null]; // Restituisce null in caso di errore
        })
      );
    }
    else {
      return new Observable<Par_GiustificativiModel | null>((subscriber) => {
        //aggiunge campi solo per le new
        //this._editForm.addControl('mail', this.fb.control(null, [Validators.required, Validators.email]));
        //this._editForm.addControl('pw', this.fb.control(null, [Validators.required]));
        //this._editForm.addControl('confirmPassword', this.fb.control(null, [Validators.required]));
        this._editForm.setValidators(matchData);
        this._editForm.updateValueAndValidity();

        subscriber.next(new Par_GiustificativiModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Par_GiustificativiModel): Observable<boolean> => {
    let request: GenericRequest<Par_GiustificativiPutInModel> =
      new GenericRequest<Par_GiustificativiPutInModel>(Par_GiustificativiPutInModel);
    request.data.par_Giustificativi = editModel;

    return this.parGiustificativiService.Par_GiustificativiPut(request).pipe(
      map(() => {

        this.refresherService.SharedParameterGestionePresenze_triggerRefresh(); 

        //TODO DISAB SHARED DATA
        // Aggiorno il dato condiviso
        //let request: GenericRequest<Par_GiustificativiInModel> = new GenericRequest<Par_GiustificativiInModel>(Par_GiustificativiInModel);
        //this.parGiustificativiService.GetAll(request).subscribe(res => {
        //  this.sharedParameterGestionePresenzeService.Par_Giustificativi =  res.data.par_Giustificativi;
        //});

        return true;
      }), // Restituisce true in caso di successo
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false]; // Restituisce false in caso di errore
      })
    );
  };






}


const matchData: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  //const password = control.get('pw')?.value;
  //const confirmPassword = control.get('confirmPassword')?.value;

  //return password === confirmPassword ? null : { notMatching: true };

  return null;
};


