import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { NavController, ToastController } from '@ionic/angular';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable, map, catchError, of, forkJoin } from 'rxjs';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { StringHelperService } from '../../../Utility/infrastructure/string-helper.service';
import { AzSediRepartoService } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/az-sedi-reparto.service';
import { Az_SediRepartoGetInModel, Az_SediRepartoGetOutModel, Az_SediRepartoModel, Az_SediRepartoPutInModel, CheckObjOn_Id_Text_4ApprovalZorder } from '../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { Dip_AnagraficaModel } from '../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model';
import { ParTable, SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { ItemReorderEventDetail } from '@ionic/angular';
import { TipoApprovazione } from '../../../ClientServer-Service/GestionePresenze/Az_Cfg/Models/az-cfg-model';
import { Par_AttivitaModel } from '../../../ClientServer-Service/GestionePresenze/Par_Attivita/Models/par-attivita-model';
import { CheckObjOn_Id_Number } from '../../../ClientServer-Service/ModelsBase/check-obj';
import { AzSubCommessaService } from '../../../ClientServer-Service/GestionePresenze/Az_SubCommessa/az-subcommessa.service';
import { Az_SubCommessa_GetCompatibili_InModel } from '../../../ClientServer-Service/GestionePresenze/Az_SubCommessa/Models/az-subcommessa-model';

@Component({
  selector: 'app-department-edit-page',
  templateUrl: './department-edit-page.component.html',
  styleUrls: ['./department-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DepartmentEditPageComponent extends BasePageConfirmCancelComponent<Az_SediRepartoGetOutModel> {

  modifiedDescription: string | null = null;

  public searchText!: string;
  public currSection: string ="sez1";
  public dip_Anagrafica: Dip_AnagraficaModel[];
  public _par_Attivita: Par_AttivitaModel[] = [];

  // Flag per abilitare/disabilitare il riordino
  reorderEnabled: boolean = false;

  /** Utenti con almeno una competenza richiesta dalle attività di reparto selezionate (null = non ancora letti). */
  public idAspNetUsers_Compatibili: string[] | null = null;
  // vale solo l'ultima richiesta: una risposta arrivata in ritardo si scarta
  private _compatibiliRichiesta: number = 0;




  constructor(protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private stringHelperService: StringHelperService,
    private azSediRepartoService: AzSediRepartoService,
    private azSubCommessaService: AzSubCommessaService,
    private toastCtrl: ToastController,
    private refresherService: RefresherService) {

    super(navCtrl, userInterfaceService, fb);

  }

  override ionViewWillEnter() {

    super.ionViewWillEnter();
    this.reorderEnabled = (this.sharedParameterGestionePresenzeService.Az_Cfg.approvazioneTipo == TipoApprovazione.AllAdminHierarchy);
    this.dip_Anagrafica = this.sharedParameterGestionePresenzeService.Dip_Anagrafica;
    this._par_Attivita = this.sharedParameterGestionePresenzeService.Par_Attivita;
  }

  get Title(): string { return "Reparti"; }
  get EditForm(): FormGroup {
    return this.
      fb.group({

        //SUB FORM PER OGGETTi DI OGGETTI
        az_SediReparto: this.fb.group({
          descrizione: [null, [Validators.required, Validators.maxLength(20)]],
        })

      });
  }

  LoadData = (): Observable<Az_SediRepartoGetOutModel | null> => {
      const state = history.state;
 
      let request: GenericRequest<Az_SediRepartoGetInModel> = new GenericRequest<Az_SediRepartoGetInModel>(Az_SediRepartoGetInModel);
      request.data.id = state.id;
      request.data.idAz_Sedi = state.idAz_Sedi;

      return this.azSediRepartoService.Az_SediRepartoGet(request).pipe(
        map((res) => {

          if (res.data) {
            this._editModel = res.data;
            this.Load_UserCompatibili();
          }
          return res.data;

        }), // Estrae il dato richiesto
        catchError((error) => {
          console.error('Errore durante la chiamata API:', error);
          return of(null); // Restituisce null in caso di errore
        })
      );
  };

  SaveData = (editModel: Az_SediRepartoGetOutModel): Observable<boolean> => {
    let request: GenericRequest<Az_SediRepartoPutInModel> = new GenericRequest<Az_SediRepartoPutInModel>(Az_SediRepartoPutInModel);
    request.data.az_SediReparto = editModel.az_SediReparto;
    request.data.selectedAdmin = editModel.selectedAdmin;
    request.data.selectedUser = editModel.selectedUser;
    request.data.az_SediRepartoAttivita = editModel.az_SediRepartoAttivita;
    

    return this.azSediRepartoService.Az_SediRepartoPut(request).pipe(
      map(() => {
        // la cache dei reparti (selettori, liste) va riletta dopo il salvataggio
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Az_SediReparto);
        return true;
      }),
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
    this._editModel.selectedAdmin
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
        const selectedOne = this._editModel.selectedAdmin.find(a => a.id === admin.id);
        if (selectedOne) {
          combinedAdmins.push(selectedOne);
        } else {
          combinedAdmins.push(admin);
        }
      }
    });

    return combinedAdmins;
  }

  // Utenti proposti: solo quelli compatibili con le attività di reparto selezionate,
  // piu' quelli gia' collegati (per poterli scollegare anche se non piu' compatibili)
  public getUser(): CheckObjOn_Id_Text_4ApprovalZorder[] {

    let retVal: CheckObjOn_Id_Text_4ApprovalZorder[] = [];

    this.dip_Anagrafica.filter(dip =>
      dip.roleCode.includes(RoleCode.User) &&
      (this.isUserCompatibile(dip.idAspNetUsers) || this.isSelectedUser(dip.idAspNetUsers))
    ).forEach(item => {
      let appo = { id: item.idAspNetUsers, checked: false, enabledToApproval: false, approvalZOrder: 0 };
      retVal.push(appo);
    });


    return retVal;
  }

  toggleSelectionAdmin(itemId: string, event: any) {


    const existingEntry = this._editModel.selectedAdmin.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.checked = event.detail.checked;
    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this._editModel.selectedAdmin.push({ id: itemId, checked: event.detail.checked, enabledToApproval: false, approvalZOrder: 0 });
    }

  }

  isSelectedAdmin(itemId: string): boolean {

    return this._editModel.selectedAdmin.find(entry => entry.id === itemId)?.checked ?? false;
  }

  toggleSelectionUser(itemId: string, event: any) {



    const existingEntry = this._editModel.selectedUser.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.checked = event.detail.checked;
    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this._editModel.selectedUser.push({ id: itemId, checked: event.detail.checked, enabledToApproval: false, approvalZOrder: 0 });
    }

  }

  isSelectedUser(itemId: string): boolean {

    return this._editModel.selectedUser.find(entry => entry.id === itemId)?.checked ?? false;

  }

  isUserCompatibile(itemId: string): boolean {
    return this.idAspNetUsers_Compatibili?.includes(itemId) ?? false;
  }

  toggleSelectionApproval(itemId: string, event: any) {


    const existingEntry = this._editModel.selectedAdmin.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.enabledToApproval = event.detail.checked;
    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this._editModel.selectedAdmin.push({ id: itemId, checked: false, enabledToApproval: event.detail.checked, approvalZOrder: 0 });
    }

  }

  isSelectedApproval(itemId: string): boolean {

    return this._editModel.selectedAdmin.find(entry => entry.id === itemId)?.enabledToApproval ?? false;
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
    console.log('Nuovo ordine di amministratori:', this._editModel.selectedAdmin
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
        const adminEntry = this._editModel.selectedAdmin.find(admin => admin.id === item.id);

        if (adminEntry) {
          // Aggiorna l'ordine
          adminEntry.approvalZOrder = index + 1;
        } else {
          // Se non esiste, aggiungilo a selectedAdmin con l'ordine corretto
          this._editModel.selectedAdmin.push({
            id: item.id,
            checked: false,
            enabledToApproval: false,
            approvalZOrder: index + 1
          });
        }
      });
    }
  }

  segmentChanged(event: any) {
    console.log('Segment cambiato:', event.detail.value);
    this.currSection = event.detail.value;
  }

  public getPar_Attivita(): CheckObjOn_Id_Number[] {
    let retVal: CheckObjOn_Id_Number[] = [];

    if (this._editModel == null)
      return retVal;

    // Ottieni gli id delle attività selezionate
    const selectedIds = this._editModel.az_SediAttivita?.map((a: any) => a.idPar_Attivita) ?? [];

    this._par_Attivita
      .filter(item => selectedIds.includes(item.id))
      .forEach(item => {
        let appo = { id: item.id, checked: false };
        retVal.push(appo);
      });

    return retVal;
  }

  isSelectedaz_Az_SediRepartoAttivita(itemId: number): boolean {

    return this._editModel.az_SediRepartoAttivita.find(entry => entry.id === itemId)?.checked ?? false;

  }

  toggleSelectionaz_Az_SediRepartoAttivita(itemId: number, event: any) {

    // attività selezionate prima della modifica: servono per capire chi era collegato tramite quella tolta
    const idPar_Attivita_Prima = this.getIdPar_Attivita_Selezionate();

    const existingEntry = this._editModel.az_SediRepartoAttivita.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.checked = event.detail.checked;
    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this._editModel.az_SediRepartoAttivita.push({ id: itemId, checked: event.detail.checked });
    }

    if (event.detail.checked)
      this.Load_UserCompatibili();
    else
      this.ScollegaUserNonPiuCompatibili(idPar_Attivita_Prima);

  }

  private getIdPar_Attivita_Selezionate(): number[] {
    return this._editModel.az_SediRepartoAttivita.filter(x => x.checked).map(x => x.id);
  }

  private GetCompatibili(idPar_Attivita: number[]): Observable<string[]> {
    if (idPar_Attivita.length === 0) return of([]);

    let request: GenericRequest<Az_SubCommessa_GetCompatibili_InModel> = new GenericRequest<Az_SubCommessa_GetCompatibili_InModel>(Az_SubCommessa_GetCompatibili_InModel);
    request.data.idPar_Attivita = idPar_Attivita;
    return this.azSubCommessaService.GetCompatibili(request).pipe(
      map(res => res.success && res.data ? res.data.idAspNetUsers : [])
    );
  }

  /** Rilegge gli utenti compatibili con le attività di reparto selezionate. */
  private Load_UserCompatibili(): void {
    const richiesta = ++this._compatibiliRichiesta;
    this.GetCompatibili(this.getIdPar_Attivita_Selezionate()).subscribe({
      next: idAspNetUsers => {
        if (richiesta === this._compatibiliRichiesta)
          this.idAspNetUsers_Compatibili = idAspNetUsers;
      },
      error: error => console.error('Errore durante la chiamata API:', error)
    });
  }

  /**
   * Tolta un'attività, scollega dal reparto gli utenti che erano compatibili prima
   * e non lo sono più con le attività rimaste.
   */
  private ScollegaUserNonPiuCompatibili(idPar_Attivita_Prima: number[]): void {
    const richiesta = ++this._compatibiliRichiesta;
    forkJoin({
      prima: this.GetCompatibili(idPar_Attivita_Prima),
      dopo: this.GetCompatibili(this.getIdPar_Attivita_Selezionate())
    }).subscribe({
      next: res => {
        if (richiesta !== this._compatibiliRichiesta) return;
        this.idAspNetUsers_Compatibili = res.dopo;

        const daScollegare = this._editModel.selectedUser
          .filter(x => x.checked && res.prima.includes(x.id) && !res.dopo.includes(x.id));
        if (daScollegare.length === 0) return;

        daScollegare.forEach(x => x.checked = false);
        const nomi = daScollegare.map(x => {
          const dip = this.dip_Anagrafica?.find(d => d.idAspNetUsers === x.id);
          return dip ? `${dip.cognome} ${dip.nome}` : x.id;
        });
        this.toastCtrl.create({
          message: `Scollegati dal reparto, non hanno le competenze per le attività rimaste: ${nomi.join(', ')}`,
          duration: 4000,
          position: 'bottom'
        }).then(t => t.present());
      },
      error: error => console.error('Errore durante la chiamata API:', error)
    });
  }

}


const matchPasswords: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('pw')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  return password === confirmPassword ? null : { notMatching: true };
};
