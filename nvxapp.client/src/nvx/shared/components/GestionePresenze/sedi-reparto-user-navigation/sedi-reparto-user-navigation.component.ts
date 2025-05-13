import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AzSediRepartoService } from '../../../../ClientServer-Service/GestionePresenze/Az_SediReparto/az-sedi-reparto.service';
import { AzSediService } from '../../../../ClientServer-Service/GestionePresenze/Az_Sedi/az-sedi.service';
import { AzSediRepartoUserServiceService } from '../../../../ClientServer-Service/GestionePresenze/Az_SediRepartoUser/az-sedi-reparto-user-service.service';
import { Az_SediModel, Az_Sedi_GetAll_InModel } from '../../../../ClientServer-Service/GestionePresenze/Az_Sedi/Models/az-sedi-model';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { Az_SediReparto_GetAll_InModel, Az_SediRepartoModel } from '../../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { Az_SediRepartoUser_GetAll_Period_InModel, Az_SediRepartoUserModel } from '../../../../ClientServer-Service/GestionePresenze/Az_SediRepartoUser/Models/az-reparto-user-model';
import { catchError, forkJoin, map, Observable, throwError } from 'rxjs';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';
import { RoleCode } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';

@Component({
  selector: 'app-sedi-reparto-user-navigation',
  templateUrl: './sedi-reparto-user-navigation.component.html',
  styleUrls: ['./sedi-reparto-user-navigation.component.scss'],
  standalone: false
})
export class SediRepartoUserNavigationComponent implements OnInit {
  // Input parameters to hide/show selects
  @Input() hideSediSelect: boolean = false;
  @Input() hideRepartoSelect: boolean = false;
  @Input() hideUserSelect: boolean = false;

  // Output parameters for selected values
  @Output() sediIdChange = new EventEmitter<number | undefined>();
  @Output() repartoIdChange = new EventEmitter<number[] | undefined>();  // lista reparti selezionati 
  @Output() allUsersIdChange = new EventEmitter<string[] | undefined>();   //  lista degli user per i reparti selezionati
  @Output() currUserIdChange = new EventEmitter<string | undefined>();  //  user corrente



  // Data lists
  public az_SediList: Az_SediModel[] = [];
  public az_SediRepartoList: Az_SediRepartoModel[] = [];
  public az_SediRepartoUserList: Az_SediRepartoUserModel[] = [];
  public filteredReparti: Az_SediRepartoModel[] = [];

  // Selected values
  public selectedSediId: number | null = null;
  public selectedRepartoId: number[] | null = null;
  public selectedUserId: string | null = null;

  // Index for user navigation
  public currentUserIndex: number = -1;

  constructor(
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private azSediService: AzSediService,
    private azSediRepartoService: AzSediRepartoService,
    private azSediRepartoUserServiceService: AzSediRepartoUserServiceService
  ) { }

  ngOnInit() {
    this.Load_Init().subscribe({
      error: (err) => console.error("Error during SediRepartoUserNavigation initialization:", err)
    });
  }

  public ColumnSize():string {
    let NumColumn = 0;

    if (!this.hideSediSelect) NumColumn++;
    if (!this.hideRepartoSelect) NumColumn++;
    if (!this.hideUserSelect) NumColumn++;

    return (12 / NumColumn).toString();
  }

  private Load_Init(): Observable<boolean> {
    let request1: GenericRequest<Az_Sedi_GetAll_InModel> = new GenericRequest<Az_Sedi_GetAll_InModel>(Az_Sedi_GetAll_InModel);
    const azSediResultObservable$ = this.azSediService.GetAll(request1);

    let request2: GenericRequest<Az_SediReparto_GetAll_InModel> = new GenericRequest<Az_SediReparto_GetAll_InModel>(Az_SediReparto_GetAll_InModel);
    const azSediRepartoResultObservable$ = this.azSediRepartoService.GetAll(request2);

    return forkJoin({
      sediResult: azSediResultObservable$,
      sediRepartoResult: azSediRepartoResultObservable$
    }).pipe(
      map(results => {
        this.az_SediList = results.sediResult?.data?.az_Sedi || [];
        this.az_SediRepartoList = results.sediRepartoResult?.data?.az_SediReparto || [];

        if (this.hideSediSelect) {
          this.selectedSediId = null;
          this.filteredReparti = [...this.az_SediRepartoList];
          if (!this.hideRepartoSelect && this.filteredReparti.length > 0) {
            this.selectedRepartoId = this.filteredReparti.map(reparto => reparto.id);
            this.onRepartoChange(); // This will handle user list loading and selection reset
          } else {
            this.selectedRepartoId = null;
            this.onRepartoChange(); // This will clear users and call onUserChange
          }
          this.sediIdChange.emit(undefined);
        } else {
          // Sedi select is visible
          this.updateFilteredReparti(); // Initially selectedSediId is null, so filteredReparti is empty
          if (this.az_SediList.length > 0) {
            this.selectedSediId = this.az_SediList[0].id;
            this.onSediChange();
          } else {
            // No Sedi to select, or Sedi select hidden
            this.selectedSediId = null;
            this.onSediChange(); // Will cascade to onRepartoChange -> onUserChange with null selections
          }
        }
        return true;
      }),
      catchError(error => {
        console.error("SediRepartoUserNavigation Error fetching data.", error);
        this.az_SediList = [];
        this.az_SediRepartoList = [];
        this.updateFilteredReparti();
        this.selectedRepartoId = null; // Ensure reset on error
        this.onRepartoChange(); // Cascade reset
        return throwError(() => new Error('SediRepartoUserNavigation Failed to load data'));
      })
    );
  }

  private updateFilteredReparti() {
    if (this.selectedSediId !== null && !this.hideSediSelect) { // Don't filter if SediSelect is hidden
      this.filteredReparti = this.az_SediRepartoList.filter(
        reparto => reparto.idAz_Sedi === this.selectedSediId
      );
    } else if (this.hideSediSelect) {
      this.filteredReparti = [...this.az_SediRepartoList]; // Use all reparti if Sedi select is hidden
    }
    else {
      this.filteredReparti = [];
    }
  }

  public onSediChange() {
    this.updateFilteredReparti();
    this.selectedRepartoId = null;

    this.sediIdChange.emit(this.selectedSediId !== null ? this.selectedSediId : undefined);

    if (!this.hideRepartoSelect && this.selectedSediId !== null && this.filteredReparti.length > 0) {
      this.selectedRepartoId = this.filteredReparti.map(reparto => reparto.id);
      this.onRepartoChange();
    } else {
      // No reparti to auto-select (e.g. Sede deselected, no reparti for Sede, or Reparto select hidden)
      // Call onRepartoChange to clear dependent selections (users)
      this.onRepartoChange();
    }
  }

  public onRepartoChange() {
    // Reset user selection and list first
    this.selectedUserId = null;
    this.az_SediRepartoUserList = [];
    this.allUsersIdChange.emit(this.az_SediRepartoUserList.map(x => x.idAspNetUsers));

    // Call onUserChange to update currentUserIndex to -1 and emit undefined for userId
    this.onUserChange();

    if (this.selectedRepartoId && this.selectedRepartoId.length > 0) {
      this.LoadAz_SediRepartoUser(this.selectedRepartoId);
    }
    // If no reparti selected, user list is already cleared and onUserChange has been called.

    this.repartoIdChange.emit(this.selectedRepartoId && this.selectedRepartoId.length > 0 ? this.selectedRepartoId : undefined);

  }

  public onUserChange() {
    if (this.selectedUserId && this.az_SediRepartoUserList.length > 0) {
      const newIndex = this.az_SediRepartoUserList.findIndex(user => user.idAspNetUsers === this.selectedUserId);
      if (newIndex !== -1) {
        this.currentUserIndex = newIndex;
      } else {
        // selectedUserId is not in the current list (e.g., list was reloaded and user is no longer valid)
        this.selectedUserId = null; // Deselect
        this.currentUserIndex = -1;
      }
    } else {
      // No user selected (selectedUserId is null) or user list is empty
      this.currentUserIndex = -1;
    }
    this.currUserIdChange.emit(this.selectedUserId !== null ? this.selectedUserId : undefined);
  }

  private LoadAz_SediRepartoUser(idAz_SediRepartoList: number[]) {
    let request: GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel> = new GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel>(Az_SediRepartoUser_GetAll_Period_InModel);
    request.data.idAz_SediReparto = [...idAz_SediRepartoList];

    this.azSediRepartoUserServiceService.GetAllPeriod(request).subscribe({
      next: res => {
        var repUser = res.data?.az_RepartoUser || [];
        var idAspNetUsers = this.sharedParameterGestionePresenzeService.Dip_Anagrafica_OnRoles([RoleCode.User]).map(x => x.idAspNetUsers);
        this.az_SediRepartoUserList = repUser.filter(x => idAspNetUsers.includes(x.idAspNetUsers));

        this.allUsersIdChange.emit(this.az_SediRepartoUserList.map(x => x.idAspNetUsers));

        // selectedUserId was reset to null in onRepartoChange.
        // onUserChange was also called there, setting currentUserIndex to -1 and emitting undefined.
        // Now, if users are loaded, select the first one.
        if (this.az_SediRepartoUserList.length > 0) {
          this.selectedUserId = this.az_SediRepartoUserList[0].idAspNetUsers;
          this.onUserChange(); // Update currentUserIndex and emit the newly selected userId
        }
        // If az_SediRepartoUserList is empty, selectedUserId remains null,
        // and the state set by onUserChange in onRepartoChange (index -1, emitted undefined) is still correct.
      },
      error: err => {
        console.error("Error loading users for reparti:", err);
        this.az_SediRepartoUserList = []; // Ensure list is empty on error
        this.allUsersIdChange.emit(this.az_SediRepartoUserList.map(x => x.idAspNetUsers));
        // selectedUserId is already null from onRepartoChange.
        // onUserChange was already called in onRepartoChange to reset index and emit.
      }
    });
  }

  // User navigation methods
  public navigateToPreviousUser(): void {
    if (this.canNavigatePrevious) {
      this.currentUserIndex--;
      this.selectedUserId = this.az_SediRepartoUserList[this.currentUserIndex].idAspNetUsers;
      this.onUserChange(); // Update select display and emit change
    }
  }

  public navigateToNextUser(): void {
    if (this.canNavigateNext) {
      this.currentUserIndex++;
      this.selectedUserId = this.az_SediRepartoUserList[this.currentUserIndex].idAspNetUsers;
      this.onUserChange(); // Update select display and emit change
    }
  }

  // Getters for button disabled states
  public get canNavigatePrevious(): boolean {
    return this.az_SediRepartoUserList.length > 0 && this.currentUserIndex > 0;
  }

  public get canNavigateNext(): boolean {
    return this.az_SediRepartoUserList.length > 0 && this.currentUserIndex < this.az_SediRepartoUserList.length - 1;
  }
}
