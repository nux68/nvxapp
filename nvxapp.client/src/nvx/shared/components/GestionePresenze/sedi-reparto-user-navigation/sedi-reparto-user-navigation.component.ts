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
  @Output() repartoIdChange = new EventEmitter<number[] | undefined>(); // Changed to number[]
  @Output() userIdChange = new EventEmitter<string | undefined>();

  // Data lists
  public az_SediList: Az_SediModel[] = [];
  public az_SediRepartoList: Az_SediRepartoModel[] = [];
  public az_SediRepartoUserList: Az_SediRepartoUserModel[] = [];
  public filteredReparti: Az_SediRepartoModel[] = [];

  // Selected values
  public selectedSediId: number | null = null;
  public selectedRepartoId: number[] | null = null; // Changed to number[]
  public selectedUserId: string | null = null;

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

        // Initialize filtered reparti list (will be empty if selectedSediId is null)
        this.updateFilteredReparti();

        // If Sedi select is visible and there are sedi, select the first one
        if (!this.hideSediSelect && this.az_SediList.length > 0) {
          this.selectedSediId = this.az_SediList[0].id;
          // This will trigger filtering reparti, selecting all of them (if applicable), and loading users
          this.onSediChange();
        } else {
          // If no auto-selection of Sede (e.g., Sedi select hidden or no Sedi available)
          // Emit initial state based on current selectedSediId (which might be null)
          this.sediIdChange.emit(this.selectedSediId !== null ? this.selectedSediId : undefined);
          this.repartoIdChange.emit(undefined); // No reparti selected yet
          this.userIdChange.emit(undefined);    // No user selected yet
        }
        return true;
      }),
      catchError(error => {
        console.error("SediRepartoUserNavigation Error fetching data.", error);
        this.az_SediList = [];
        this.az_SediRepartoList = [];
        this.updateFilteredReparti(); // Ensure lists are empty on error
        return throwError(() => new Error('SediRepartoUserNavigation Failed to load data'));
      })
    );
  }

  // Filter reparti based on selected sede
  private updateFilteredReparti() {
    if (this.selectedSediId !== null) {
      this.filteredReparti = this.az_SediRepartoList.filter(
        reparto => reparto.idAz_Sedi === this.selectedSediId
      );
    } else {
      this.filteredReparti = [];
    }
  }

  // Handler for sede selection change
  public onSediChange() {
    // Update filtered reparti list based on the new selectedSediId
    this.updateFilteredReparti();

    // Reset dependent selections
    this.selectedRepartoId = null;
    this.az_SediRepartoUserList = [];
    this.selectedUserId = null;

    // Emit the selected sede ID
    this.sediIdChange.emit(this.selectedSediId !== null ? this.selectedSediId : undefined);

    // If Reparto select is visible, a Sede is selected, and there are filtered Reparti, select all of them
    if (!this.hideRepartoSelect && this.selectedSediId !== null && this.filteredReparti.length > 0) {
      this.selectedRepartoId = this.filteredReparti.map(reparto => reparto.id);
      // Trigger onRepartoChange to load users for these reparti and emit event
      this.onRepartoChange();
    } else {
      // No reparti to select, or reparto select is hidden
      this.repartoIdChange.emit(undefined);
      this.userIdChange.emit(undefined);
    }
  }

  // Handler for reparto selection change
  public onRepartoChange() {
    // Reset user selection
    this.selectedUserId = null;
    this.az_SediRepartoUserList = []; // Clear previous user list

    // Load users for the selected reparti if any are selected
    if (this.selectedRepartoId && this.selectedRepartoId.length > 0) {
      this.LoadAz_SediRepartoUser(this.selectedRepartoId);
    }

    // Emit the selected reparto IDs (or undefined if none)
    this.repartoIdChange.emit(this.selectedRepartoId && this.selectedRepartoId.length > 0 ? this.selectedRepartoId : undefined);
    // User is reset, so emit undefined for userId
    this.userIdChange.emit(undefined);
  }

  // Handler for user selection change
  public onUserChange() {
    // Emit the selected user ID
    this.userIdChange.emit(this.selectedUserId !== null ? this.selectedUserId : undefined);
  }

  private LoadAz_SediRepartoUser(idAz_SediRepartoList: number[]) {
    let request: GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel> = new GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel>(Az_SediRepartoUser_GetAll_Period_InModel);

    // Set the array of reparto IDs in the request
    request.data.idAz_SediReparto = [...idAz_SediRepartoList];

    this.azSediRepartoUserServiceService.GetAllPeriod(request).subscribe({
      next: res => {
         var repUser = res.data?.az_RepartoUser || [];

        var idAspNetUsers = this.sharedParameterGestionePresenzeService.Dip_Anagrafica_OnRoles([RoleCode.User]).map(x => x.idAspNetUsers);

        this.az_SediRepartoUserList = repUser.filter(x => idAspNetUsers.includes(x.idAspNetUsers));

      },
      error: err => {
        console.error("Error loading users for reparti:", err);
        this.az_SediRepartoUserList = []; // Clear list on error
      }
    });
  }
}
