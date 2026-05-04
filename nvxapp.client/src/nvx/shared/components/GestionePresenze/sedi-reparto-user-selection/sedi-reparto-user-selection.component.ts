import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AzSediRepartoService } from '../../../../ClientServer-Service/GestionePresenze/Az_SediReparto/az-sedi-reparto.service';
import { AzSediService } from '../../../../ClientServer-Service/GestionePresenze/Az_Sedi/az-sedi.service';
import { AzSediRepartoUserServiceService } from '../../../../ClientServer-Service/GestionePresenze/Az_SediRepartoUser/az-sedi-reparto-user-service.service';
import { Az_SediModel, Az_Sedi_GetAll_InModel } from '../../../../ClientServer-Service/GestionePresenze/Az_Sedi/Models/az-sedi-model';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { Az_SediReparto_Get4Admin_InModel, Az_SediReparto_Get4User_InModel, Az_SediReparto_GetAll_InModel, Az_SediRepartoModel } from '../../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { Az_SediRepartoUser_GetAll_Period_InModel, Az_SediRepartoUserModel } from '../../../../ClientServer-Service/GestionePresenze/Az_SediRepartoUser/Models/az-reparto-user-model';
import { catchError, forkJoin, map, Observable, throwError } from 'rxjs';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';
import { RoleCode } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { ModalController } from '@ionic/angular';
import { DipSelectorModalComponent, DipSelectorResult } from '../dip-selector-modal/dip-selector-modal.component';


@Component({
  selector: 'app-sedi-reparto-user-selection',
  templateUrl: './sedi-reparto-user-selection.component.html',
  styleUrls: ['./sedi-reparto-user-selection.component.scss'],
  standalone: false
})
export class SediRepartoUserSelectionComponent implements OnInit {
  // Input parameters to show/hide selects

  @Input() showPeriodSelect: boolean = true;
  @Input() showSediSelect: boolean = true;
  @Input() showRepartoSelect: boolean = true;
  @Input() showUserSelect: boolean = true;

  @Input() singleFieldOnRow: boolean = false;
  @Input() singleSelectReparti: boolean = false;
  @Input() singleSelectUser: boolean = true;
  @Input() initialSelectedUserId: string | string[] | null = null;  // forza la selezione del utente all avvio


  // Output parameters for selected values
  @Output() sediIdChange = new EventEmitter<number | undefined>();
  @Output() repartoIdChange = new EventEmitter<number[] | undefined>();
  @Output() allUsersIdChange = new EventEmitter<string[] | undefined>();
  @Output() currUserIdChange = new EventEmitter<string[] | undefined>();
  @Output() periodChange = new EventEmitter<{ year: number, month: number }>(); // Output for year/month period

  // Data lists
  public az_SediList: Az_SediModel[] = [];
  public az_SediRepartoList: Az_SediRepartoModel[] = [];
  public az_SediRepartoUserList: Az_SediRepartoUserModel[] = [];
  public filteredReparti: Az_SediRepartoModel[] = [];

  // Selected values
  public selectedSediId: number | null = null;
  public selectedRepartoId: number[] | null = null;
  public selectedUserId: string | string[] | null = null;

  // Period selection state
  public selectedYear: number | null = null;
  public selectedMonth: number | null = null; // 1-12
  public yearList: number[] = [];
  public monthList: { value: number, name: string }[] = [
    { value: 1, name: 'Gennaio' }, { value: 2, name: 'Febbraio' },
    { value: 3, name: 'Marzo' }, { value: 4, name: 'Aprile' },
    { value: 5, name: 'Maggio' }, { value: 6, name: 'Giugno' },
    { value: 7, name: 'Luglio' }, { value: 8, name: 'Agosto' },
    { value: 9, name: 'Settembre' }, { value: 10, name: 'Ottobre' },
    { value: 11, name: 'Novembre' }, { value: 12, name: 'Dicembre' }
  ];

  // Index for user navigation
  public currentUserIndex: number = -1;

  constructor(
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private azSediService: AzSediService,
    private azSediRepartoService: AzSediRepartoService,
    private azSediRepartoUserServiceService: AzSediRepartoUserServiceService,
    private modalCtrl: ModalController
  ) { }

  ngOnInit() {
    this.initializePeriodSelection();

    if (this.showSediSelect || this.showRepartoSelect || this.showUserSelect) {
      this.Load_Init().subscribe({
        error: (err) => console.error("Error during SediRepartoUserNavigation initialization:", err)
      });
    }



  }

  private initializePeriodSelection(): void {
    const currentDate = new Date();
    const currentYear = currentDate.getFullYear();
    const currentMonth = currentDate.getMonth() + 1; // JavaScript months are 0-11

    this.selectedYear = currentYear;
    this.selectedMonth = currentMonth;

    this.yearList = [];
    // 5 previous years
    for (let i = 5; i >= 1; i--) {
      this.yearList.push(currentYear - i);
    }
    // Current year
    this.yearList.push(currentYear);
    // Next year
    this.yearList.push(currentYear + 1);

    // Emit initial period
    this.emitPeriodChange();
  }

  private emitPeriodChange(): void {
    if (this.selectedYear !== null && this.selectedMonth !== null) {
      this.periodChange.emit({ year: this.selectedYear, month: this.selectedMonth });
    }
  }

  public onYearOrMonthChange(): void {
    // This will be called by (ionChange) from year and month selects
    this.emitPeriodChange();
  }

  public navigateToPreviousMonth(): void {
    if (this.selectedMonth === null || this.selectedYear === null) return;

    this.selectedMonth--;
    if (this.selectedMonth < 1) {
      this.selectedMonth = 12;
      this.selectedYear--;
      // If year goes out of the initial list, add it to keep select options updated
      if (!this.yearList.includes(this.selectedYear)) {
        this.yearList.push(this.selectedYear);
        this.yearList.sort((a, b) => a - b);
      }
    }
    this.emitPeriodChange();
  }

  public navigateToNextMonth(): void {
    if (this.selectedMonth === null || this.selectedYear === null) return;

    this.selectedMonth++;
    if (this.selectedMonth > 12) {
      this.selectedMonth = 1;
      this.selectedYear++;
      if (!this.yearList.includes(this.selectedYear)) {
        this.yearList.push(this.selectedYear);
        this.yearList.sort((a, b) => a - b);
      }
    }
    this.emitPeriodChange();
  }

  public ColumnSize(): string {
    let numVisibleColumns = 0;

    if (this.showSediSelect) numVisibleColumns++;
    if (this.showRepartoSelect) numVisibleColumns++;
    if (this.showUserSelect) numVisibleColumns++;

    if (numVisibleColumns === 0) return '12';
    return (12 / numVisibleColumns).toString();
  }

  private Load_Init(): Observable<boolean> {
    let request1: GenericRequest<Az_Sedi_GetAll_InModel> = new GenericRequest<Az_Sedi_GetAll_InModel>(Az_Sedi_GetAll_InModel);
    const azSediResultObservable$ = this.azSediService.GetAll(request1);

    let request2: GenericRequest<Az_SediReparto_Get4Admin_InModel> = new GenericRequest<Az_SediReparto_Get4Admin_InModel>(Az_SediReparto_Get4Admin_InModel);
    const azSediRepartoResultObservable$ = this.azSediRepartoService.Get4Admin(request2);

    return forkJoin({
      sediResult: azSediResultObservable$,
      sediRepartoResult: azSediRepartoResultObservable$
    }).pipe(
      map(results => {
        this.az_SediList = results.sediResult?.data?.az_Sedi || [];
        this.az_SediRepartoList = results.sediRepartoResult?.data?.az_SediReparto || [];

        if (!this.showSediSelect) { // Sedi select is hidden
          this.selectedSediId = null;
          this.filteredReparti = [...this.az_SediRepartoList];
          if (this.showRepartoSelect && this.filteredReparti.length > 0) {
            if (this.singleSelectReparti)
              this.selectedRepartoId = [this.filteredReparti[0].id];
            else
              this.selectedRepartoId = this.filteredReparti.map(reparto => reparto.id);

            this.onRepartoChange();

            

          } else {
            this.selectedRepartoId = null;
            this.onRepartoChange();
          }
          this.sediIdChange.emit(undefined);
        } else { // Sedi select is visible
          this.updateFilteredReparti();
          if (this.az_SediList.length > 0) {
            this.selectedSediId = this.az_SediList[0].id;
            this.onSediChange();
          } else {
            this.selectedSediId = null;
            this.onSediChange();
          }
        }
        return true;
      }),
      catchError(error => {
        console.error("SediRepartoUserNavigation Error fetching data.", error);
        this.az_SediList = [];
        this.az_SediRepartoList = [];
        this.updateFilteredReparti();
        this.selectedRepartoId = null;
        this.onRepartoChange();
        return throwError(() => new Error('SediRepartoUserNavigation Failed to load data'));
      })
    );
  }

  private updateFilteredReparti() {
    if (this.selectedSediId !== null && this.showSediSelect) {
      this.filteredReparti = this.az_SediRepartoList.filter(
        reparto => reparto.idAz_Sedi === this.selectedSediId
      );
    } else if (!this.showSediSelect) {
      this.filteredReparti = [...this.az_SediRepartoList];
    }
    else {
      this.filteredReparti = [];
    }
  }

  public onSediChange() {
    this.updateFilteredReparti();
    this.selectedRepartoId = null;

    this.sediIdChange.emit(this.selectedSediId !== null ? this.selectedSediId : undefined);

    if (this.showRepartoSelect && this.selectedSediId !== null && this.filteredReparti.length > 0) {

      if (this.singleSelectReparti)
        this.selectedRepartoId = [this.filteredReparti[0].id];
      else
        this.selectedRepartoId = this.filteredReparti.map(reparto => reparto.id);

      this.onRepartoChange();
    } else {
      this.onRepartoChange();
    }
  }

  public onRepartoChange() {
    this.selectedUserId = null;
    this.az_SediRepartoUserList = [];
    this.allUsersIdChange.emit(this.az_SediRepartoUserList.map(x => x.idAspNetUsers));
    this.onUserChange();

    if (this.selectedRepartoId && this.selectedRepartoId.length > 0) {
      this.LoadAz_SediRepartoUser(this.selectedRepartoId);
    }
    //this.repartoIdChange.emit(this.selectedRepartoId && this.selectedRepartoId.length > 0 ? this.selectedRepartoId : undefined);

    let repartoIds: number[] | undefined;

    if (this.selectedRepartoId && Array.isArray(this.selectedRepartoId) && this.selectedRepartoId.length > 0) {
      repartoIds = this.selectedRepartoId;
    } else if (this.selectedRepartoId && !Array.isArray(this.selectedRepartoId)) {
      repartoIds = [this.selectedRepartoId];
    } else {
      repartoIds = undefined;
    }

    this.repartoIdChange.emit(repartoIds);

  }

  //public onUserChange() {
  //  if (this.singleSelectUser) {

  //    if (this.selectedUserId && this.selectedUserId.length > 0 && this.az_SediRepartoUserList.length > 0) {

  //      let currID = '';
  //      if (Array.isArray(this.selectedUserId))
  //        currID = this.selectedUserId[0];
  //      else
  //        currID = this.selectedUserId;


  //      const newIndex = this.az_SediRepartoUserList.findIndex(user => user.idAspNetUsers === currID);

  //      if (newIndex !== -1) {
  //        this.currentUserIndex = newIndex;
  //      } else {
  //        this.selectedUserId = null;
  //        this.currentUserIndex = -1;
  //      }
  //    } else {
  //      this.currentUserIndex = -1;
  //    }
  //  }
  //  else {
  //    this.currentUserIndex = -1;
  //  }

  //  let selectedUserId: string[] | undefined;

  //  if (this.selectedUserId && Array.isArray(this.selectedUserId) && this.selectedUserId.length > 0) {
  //    selectedUserId = this.selectedUserId;
  //  } else if (this.selectedUserId && !Array.isArray(this.selectedUserId)) {
  //    selectedUserId = [this.selectedUserId];
  //  } else {
  //    selectedUserId = undefined;
  //  }


  //  if (this.singleSelectUser) {
  //    this.currUserIdChange.emit(selectedUserId);
  //  }
  //  else {
  //    this.allUsersIdChange.emit(this.az_SediRepartoUserList.map(x => x.idAspNetUsers));
  //  }
  //}

  public onUserChange() {
    if (this.singleSelectUser) {

      if (this.selectedUserId && this.selectedUserId.length > 0 && this.az_SediRepartoUserList.length > 0) {

        let currID = '';
        if (Array.isArray(this.selectedUserId))
          currID = this.selectedUserId[0];
        else
          currID = this.selectedUserId;


        const newIndex = this.az_SediRepartoUserList.findIndex(user => user.idAspNetUsers === currID);

        if (newIndex !== -1) {
          this.currentUserIndex = newIndex;
        } else {
          this.selectedUserId = null;
          this.currentUserIndex = -1;
        }
      } else {
        this.currentUserIndex = -1;
      }
    }
    else {
      this.currentUserIndex = -1;
    }

    let finalSelectedUserIds: string[] | undefined;

    if (this.selectedUserId && Array.isArray(this.selectedUserId) && this.selectedUserId.length > 0) {
      finalSelectedUserIds = this.selectedUserId;
    } else if (this.selectedUserId && !Array.isArray(this.selectedUserId)) {
      finalSelectedUserIds = [this.selectedUserId];
    } else {
      finalSelectedUserIds = undefined;
    }


    if (this.singleSelectUser) {
      this.currUserIdChange.emit(finalSelectedUserIds);
      // Quando la selezione è singola, allUsersIdChange non deve emettere nulla o un array vuoto
      // per evitare di sovrascrivere la selezione nel componente padre.
      this.allUsersIdChange.emit(finalSelectedUserIds);
    }
    else {
      // Quando la selezione è multipla, emettiamo gli utenti selezionati.
      this.allUsersIdChange.emit(finalSelectedUserIds);
      this.currUserIdChange.emit(undefined); // Nessun utente singolo selezionato
    }
  }

  private LoadAz_SediRepartoUser(idAz_SediRepartoList: number[]) {
    let request: GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel> = new GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel>(Az_SediRepartoUser_GetAll_Period_InModel);
    request.data.idAz_SediReparto = [...idAz_SediRepartoList];

    this.azSediRepartoUserServiceService.GetAllPeriod(request).subscribe({
      next: res => {
        var repUser = res.data?.az_RepartoUser || [];
        var idAspNetUsers = this.sharedParameterGestionePresenzeService.Dip_Anagrafica_OnRoles([RoleCode.User]).map(x => x.idAspNetUsers);
        this.az_SediRepartoUserList = repUser.filter(x => idAspNetUsers.includes(x.idAspNetUsers) && x.userInDepartment == true);

        // NON emettere qui allUsersIdChange, verrà gestito da onUserChange

        if (this.showUserSelect && this.az_SediRepartoUserList.length > 0) {

          // Controlla se è stato fornito un utente iniziale da selezionare
          if (this.initialSelectedUserId) {
            const userExists = this.az_SediRepartoUserList.some(user =>
              Array.isArray(this.initialSelectedUserId)
                ? this.initialSelectedUserId.includes(user.idAspNetUsers)
                : this.initialSelectedUserId === user.idAspNetUsers
            );

            if (userExists) {
              this.selectedUserId = this.initialSelectedUserId;
              // Resetta initialSelectedUserId per non forzare la selezione nelle successive chiamate
              this.initialSelectedUserId = null;
            }
          }

          // Se nessun utente è stato preselezionato, applica la logica di default
          if (!this.selectedUserId) {
            if (this.singleSelectUser)
              this.selectedUserId = [this.az_SediRepartoUserList[0].idAspNetUsers];
            else
              this.selectedUserId = this.az_SediRepartoUserList.map(x => x.idAspNetUsers);
          }

          this.onUserChange();
        } else {
          // Se non ci sono utenti o il select è nascosto, emetti un valore vuoto
          this.allUsersIdChange.emit([]);
          this.currUserIdChange.emit(undefined);
        }
      },
      error: err => {
        console.error("Error loading users for reparti:", err);
        this.az_SediRepartoUserList = [];
        this.allUsersIdChange.emit([]);
        this.currUserIdChange.emit(undefined);
      }
    });
  }

  //private LoadAz_SediRepartoUser(idAz_SediRepartoList: number[]) {
  //  let request: GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel> = new GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel>(Az_SediRepartoUser_GetAll_Period_InModel);
  //  request.data.idAz_SediReparto = [...idAz_SediRepartoList];

  //  this.azSediRepartoUserServiceService.GetAllPeriod(request).subscribe({
  //    next: res => {
  //      var repUser = res.data?.az_RepartoUser || [];
  //      var idAspNetUsers = this.sharedParameterGestionePresenzeService.Dip_Anagrafica_OnRoles([RoleCode.User]).map(x => x.idAspNetUsers);
  //      this.az_SediRepartoUserList = repUser.filter(x => idAspNetUsers.includes(x.idAspNetUsers) && x.userInDepartment == true);
  //      this.allUsersIdChange.emit(this.az_SediRepartoUserList.map(x => x.idAspNetUsers));

  //      if (this.showUserSelect && this.az_SediRepartoUserList.length > 0) {

  //        // Controlla se è stato fornito un utente iniziale da selezionare
  //        if (this.initialSelectedUserId) {
  //          const userExists = this.az_SediRepartoUserList.some(user =>
  //            Array.isArray(this.initialSelectedUserId)
  //              ? this.initialSelectedUserId.includes(user.idAspNetUsers)
  //              : this.initialSelectedUserId === user.idAspNetUsers
  //          );

  //          if (userExists) {
  //            this.selectedUserId = this.initialSelectedUserId;
  //            // Resetta initialSelectedUserId per non forzare la selezione nelle successive chiamate
  //            this.initialSelectedUserId = null;
  //          }
  //        }

  //        // Se nessun utente è stato preselezionato, applica la logica di default
  //        if (!this.selectedUserId) {
  //          if (this.singleSelectUser)
  //            this.selectedUserId = [this.az_SediRepartoUserList[0].idAspNetUsers];
  //          else
  //            this.selectedUserId = this.az_SediRepartoUserList.map(x => x.idAspNetUsers);
  //        }

  //        this.onUserChange();
  //      }
  //    },
  //    error: err => {
  //      console.error("Error loading users for reparti:", err);
  //      this.az_SediRepartoUserList = [];
  //      this.allUsersIdChange.emit(this.az_SediRepartoUserList.map(x => x.idAspNetUsers));
  //    }
  //  });
  //}


  //private LoadAz_SediRepartoUser(idAz_SediRepartoList: number[]) {
  //  let request: GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel> = new GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel>(Az_SediRepartoUser_GetAll_Period_InModel);
  //  request.data.idAz_SediReparto = [...idAz_SediRepartoList];

  //  this.azSediRepartoUserServiceService.GetAllPeriod(request).subscribe({
  //    next: res => {
  //      var repUser = res.data?.az_RepartoUser || [];
  //      var idAspNetUsers = this.sharedParameterGestionePresenzeService.Dip_Anagrafica_OnRoles([RoleCode.User]).map(x => x.idAspNetUsers);
  //      this.az_SediRepartoUserList = repUser.filter(x => idAspNetUsers.includes(x.idAspNetUsers) && x.userInDepartment == true);
  //      this.allUsersIdChange.emit(this.az_SediRepartoUserList.map(x => x.idAspNetUsers));

  //      if (this.showUserSelect && this.az_SediRepartoUserList.length > 0) {
  //        if (this.singleSelectUser)
  //          this.selectedUserId = [this.az_SediRepartoUserList[0].idAspNetUsers];
  //        else
  //          this.selectedUserId = this.az_SediRepartoUserList.map(x => x.idAspNetUsers);


  //        this.onUserChange();
  //      }
  //    },
  //    error: err => {
  //      console.error("Error loading users for reparti:", err);
  //      this.az_SediRepartoUserList = [];
  //      this.allUsersIdChange.emit(this.az_SediRepartoUserList.map(x => x.idAspNetUsers));
  //    }
  //  });
  //}

  public navigateToPreviousUser(): void {
    if (this.canNavigatePrevious) {
      this.currentUserIndex--;
      this.selectedUserId = [this.az_SediRepartoUserList[this.currentUserIndex].idAspNetUsers];
      this.onUserChange();
    }
  }

  public navigateToNextUser(): void {
    if (this.canNavigateNext) {
      this.currentUserIndex++;
      this.selectedUserId = [this.az_SediRepartoUserList[this.currentUserIndex].idAspNetUsers];
      this.onUserChange();
    }
  }

  public get canNavigatePrevious(): boolean {
    return this.az_SediRepartoUserList.length > 0 && this.currentUserIndex > 0;
  }

  public get canNavigateNext(): boolean {
    return this.az_SediRepartoUserList.length > 0 && this.currentUserIndex < this.az_SediRepartoUserList.length - 1;
  }

  public columSize_4Sede(): string {
    if (this.singleFieldOnRow)

      return 'col-sm-12';
    else

      return 'col-sm-2';
  }

  public columSize_4Reparto(): string {
    if (this.singleFieldOnRow)
      return 'col-sm-12';
    else
      return 'col-sm-5';
  }

  public columSize_4User(): string {
    if (this.singleFieldOnRow)
      return 'col-sm-12';
    else
      return 'col-sm-5';
  }

  public get selectedUserLabel(): string {
    if (!this.selectedUserId || (Array.isArray(this.selectedUserId) && this.selectedUserId.length === 0))
      return this.singleSelectUser ? 'Seleziona dipendente' : 'Seleziona dipendenti';

    const ids: string[] = Array.isArray(this.selectedUserId) ? this.selectedUserId : [this.selectedUserId];
    const anagrafica = this.sharedParameterGestionePresenzeService.Dip_Anagrafica_OnRoles([RoleCode.User]);

    if (this.singleSelectUser) {
      const dip = anagrafica.find(a => a.idAspNetUsers === ids[0]);
      return dip ? `${dip.cognome} ${dip.nome}` : ids[0];
    } else {
      return `${ids.length} dipendenti selezionati`;
    }
  }

  public async openDipSelectorModal(): Promise<void> {
    const preselected: string[] = this.selectedUserId
      ? (Array.isArray(this.selectedUserId) ? this.selectedUserId : [this.selectedUserId])
      : [];

    const modal = await this.modalCtrl.create({
      component: DipSelectorModalComponent,
      componentProps: {
        mode: this.singleSelectUser ? 'single' : 'multi',
        userList: this.az_SediRepartoUserList,
        preselected
      }
    });

    await modal.present();

    const { data, role } = await modal.onWillDismiss<DipSelectorResult>();
    if (role === 'confirm' && data) {
      this.selectedUserId = data.selected;
      this.onUserChange();
    }
  }

}
