import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable, map, catchError } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { AzSediRepartoService } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/az-sedi-reparto.service';
import { Az_SediRepartoGetInModel, Az_SediRepartoModel, Az_SediRepartoPutInModel, CheckObjOn_Id_Text_4ApprovalZorder } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { DealerGetInModel, DealerPutInModel } from '../../../ClientServer-Service/Infrastructure/Account/Models/dealer-model';
import { Dip_AnagraficaModel } from '../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { CheckObjOn_Id_Text } from '../../../ClientServer-Service/ModelsBase/check-obj';
import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';

@Component({
  selector: 'app-department-edit-page',
  templateUrl: './department-edit-page.component.html',
  styleUrls: ['./department-edit-page.component.scss'],
  standalone: false
}) 
export class DepartmentEditPageComponent extends BasePageConfirmCancelComponent<Az_SediRepartoModel> {

  modifiedDescription: string | null = null;

  public searchText!: string;

  public dip_Anagrafica: Dip_AnagraficaModel[];
  

  selectedAdmin: CheckObjOn_Id_Text_4ApprovalZorder[] = [];
  selectedUser: CheckObjOn_Id_Text_4ApprovalZorder[] = [];

  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private stringHelperService: StringHelperService,
    private azSediRepartoService: AzSediRepartoService) {

    super(navCtrl, userInterfaceService, fb);

  }

  override ionViewWillEnter() {
    super.ionViewWillEnter();
    this.dip_Anagrafica = this.sharedParameterGestionePresenzeService.Dip_Anagrafica;
  }

  get Title(): string { return "Department"; }
  get EditForm(): FormGroup {
    return this.
      fb.group({

        descrizione: [null, [Validators.required, Validators.maxLength(20)]],

      });
  }

  LoadData = (): Observable<Az_SediRepartoModel | null> => {
    const state = history.state;

    if (state && state.id) {
      let request: GenericRequest<Az_SediRepartoGetInModel> = new GenericRequest<Az_SediRepartoGetInModel>(Az_SediRepartoGetInModel);
      request.data.id = state.id;

      return this.azSediRepartoService.Az_SediRepartoGet(request).pipe(
        map((res) => {
          this.selectedAdmin = res.data.selectedAdmin;
          this.selectedUser = res.data.selectedUser;
          return res.data.az_SediReparto;

        }), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return [null]; // Restituisce null in caso di errore
        })
      );
    }
    else {
      return new Observable<Az_SediRepartoModel | null>((subscriber) => {

        this.selectedAdmin = [];
        this.selectedUser = [];

        subscriber.next(new Az_SediRepartoModel());
        subscriber.complete();
      });
    }
  };

  SaveData = (editModel: Az_SediRepartoModel): Observable<boolean> => {
    let request: GenericRequest<Az_SediRepartoPutInModel> = new GenericRequest<Az_SediRepartoPutInModel>(Az_SediRepartoPutInModel);
    request.data.az_SediReparto = editModel;
    request.data.selectedAdmin = this.selectedAdmin;
    request.data.selectedUser = this.selectedUser;
    

    return this.azSediRepartoService.Az_SediRepartoPut(request).pipe(
      map(() => true), // Restituisce true in caso di successo
      catchError((error) => {
        console.error('Errore durante la chiamata API:', error);
        return [false]; // Restituisce false in caso di errore
      })
    );
  };

  UpdateDescription() {
    const descrizione = this._editForm.get('descrizione')?.value;

    if (descrizione) {
      this.modifiedDescription = "Attenzione per accedere a questa utenza verrano creati i seguenti user    ->   " + this.stringHelperService.removeSpecialCharacters(descrizione) + "_Admin" + " / " + this.stringHelperService.removeSpecialCharacters(descrizione) + "_PowerAdmin";
    } else {
      this.modifiedDescription = null;
    }
  }

  public getAdmin(): CheckObjOn_Id_Text_4ApprovalZorder[] {

    let retVal: CheckObjOn_Id_Text_4ApprovalZorder[] = [];

    this.dip_Anagrafica.filter(dip =>
      dip.roleCode.includes(RoleCode.CompanyAdmin) ||
      dip.roleCode.includes(RoleCode.CompanyPowerAdmin)
    ).forEach(item => {
      let appo = { id: item.idAspNetUsers, checked: false, enabledToApproval: false, approvalZOrder: 0 };
      retVal.push(appo);
    });


    return retVal;

  }

  public getUser(): CheckObjOn_Id_Text_4ApprovalZorder[] {

    let retVal: CheckObjOn_Id_Text_4ApprovalZorder[] = [];

    this.dip_Anagrafica.filter(dip =>
      dip.roleCode.includes(RoleCode.User)
    ).forEach(item => {
      let appo = { id: item.idAspNetUsers, checked: false, enabledToApproval: false, approvalZOrder: 0 };
      retVal.push(appo);
    });
    

    return retVal;
  }


  toggleSelectionAdmin(itemId: string, event: any) {
    

    const existingEntry = this.selectedAdmin.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.checked = event.detail.checked;
    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this.selectedAdmin.push({ id: itemId, checked: event.detail.checked, enabledToApproval: false, approvalZOrder:0 });
    }

  }

  isSelectedAdmin(itemId: string): boolean {
    
    return this.selectedAdmin.find(entry => entry.id === itemId)?.checked ?? false;
  }

  toggleSelectionUser(itemId: string, event: any) {

    

    const existingEntry = this.selectedUser.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.checked = event.detail.checked;
    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this.selectedUser.push({ id: itemId, checked: event.detail.checked, enabledToApproval: false, approvalZOrder: 0 });
    }

  }

  isSelectedUser(itemId: string): boolean {
    
    return this.selectedUser.find(entry => entry.id === itemId)?.checked ?? false;

  }



  toggleSelectionApproval(itemId: string, event: any) {


    const existingEntry = this.selectedAdmin.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.enabledToApproval = event.detail.checked;
    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this.selectedAdmin.push({ id: itemId, checked: false, enabledToApproval: event.detail.checked, approvalZOrder: 0 });
    }

  }

  isSelectedApproval(itemId: string): boolean {

    return this.selectedAdmin.find(entry => entry.id === itemId)?.enabledToApproval ?? false;
  }

}


const matchPasswords: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('pw')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  return password === confirmPassword ? null : { notMatching: true };
};
