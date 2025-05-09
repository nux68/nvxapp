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
import { Dip_AnagraficaModel } from '../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { ItemReorderEventDetail } from '@ionic/angular';
import { TipoApprovazione } from '../../../ClientServer-Service/GestionePresenze/Az_Cfg/Models/az-cfg-model';

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

  // Flag per abilitare/disabilitare il riordino
  reorderEnabled: boolean = false;

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
    this.reorderEnabled = (this.sharedParameterGestionePresenzeService.Az_Cfg.approvazioneTipo == TipoApprovazione.AllAdminHierarchy);
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

  // Restituisce gli admin ordinati in base al campo approvalZOrder
  public getAdminSorted(): CheckObjOn_Id_Text_4ApprovalZorder[] {
    // Prima ottieni tutti gli amministratori
    const allAdmins = this.getAdmin();

    // Combina quelli già selezionati con quelli non selezionati
    const combinedAdmins: CheckObjOn_Id_Text_4ApprovalZorder[] = [];

    // Prima aggiungi tutti quelli che hanno un approvalZOrder > 0 (indipendentemente da enabledToApproval)
    this.selectedAdmin
      .filter(admin => admin.approvalZOrder > 0)
      .sort((a, b) => a.approvalZOrder - b.approvalZOrder)
      .forEach(admin => {
        combinedAdmins.push(admin);
      });

    // Poi aggiungi quelli che non hanno ancora un approvalZOrder assegnato
    allAdmins.forEach(admin => {
      const existingAdmin = combinedAdmins.find(a => a.id === admin.id);
      if (!existingAdmin) {
        // Cerca se esiste già nel selectedAdmin
        const selectedOne = this.selectedAdmin.find(a => a.id === admin.id);
        if (selectedOne) {
          combinedAdmins.push(selectedOne);
        } else {
          combinedAdmins.push(admin);
        }
      }
    });

    return combinedAdmins;
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
      this.selectedAdmin.push({ id: itemId, checked: event.detail.checked, enabledToApproval: false, approvalZOrder: 0 });
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

  // Gestisce l'evento di riordinamento
  handleReorderAdmin(event: CustomEvent<ItemReorderEventDetail>) {
    // Ottiene tutti gli amministratori dal metodo getAdminSorted()
    const adminList = this.getAdminSorted();

    // Esegue lo spostamento dell'elemento visualizzato nell'array
    const fromIndex = event.detail.from;
    const toIndex = event.detail.to;

    // Esegue lo spostamento nella lista visualizzata
    const movedItem = adminList[fromIndex];

    // Aggiorna l'ordine in selectedAdmin basandosi sulla nuova posizione
    this.updateAdminOrder(movedItem.id, toIndex);

    // Completa il reorder
    event.detail.complete();

    // Debug
    console.log('Nuovo ordine di amministratori:', this.selectedAdmin
      .sort((a, b) => a.approvalZOrder - b.approvalZOrder)
      .map(admin => `${admin.id}: ${admin.approvalZOrder}`));
  }

  // Aggiorna l'ordine di tutti gli admin dopo un riordinamento
  updateAdminOrder(movedItemId: string, newPosition: number) {
    // Ottieni l'elenco attuale ordinato
    const currentOrder = this.getAdminSorted();

    // Rimuovi l'elemento spostato dalla sua posizione attuale
    const elementToMove = currentOrder.find(item => item.id === movedItemId);
    const filteredList = currentOrder.filter(item => item.id !== movedItemId);

    // Inserisci l'elemento nella nuova posizione
    if (elementToMove) {
      filteredList.splice(newPosition, 0, elementToMove);

      // Aggiorna l'ordine di approvazione per tutti gli elementi
      filteredList.forEach((item, index) => {
        // Cerca l'elemento corrispondente in selectedAdmin
        const adminEntry = this.selectedAdmin.find(admin => admin.id === item.id);

        if (adminEntry) {
          // Aggiorna l'ordine
          adminEntry.approvalZOrder = index + 1;
        } else {
          // Se non esiste, aggiungilo a selectedAdmin con l'ordine corretto
          this.selectedAdmin.push({
            id: item.id,
            checked: false,
            enabledToApproval: false,
            approvalZOrder: index + 1
          });
        }
      });
    }
  }

  
}


const matchPasswords: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('pw')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  return password === confirmPassword ? null : { notMatching: true };
};
