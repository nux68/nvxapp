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

@Component({
  selector: 'app-sedi-reparto-user-navigation',
  templateUrl: './sedi-reparto-user-navigation.component.html',
  styleUrls: ['./sedi-reparto-user-navigation.component.scss'],
  standalone: false
})
export class SediRepartoUserNavigationComponent implements OnInit {
  // Input parameters to show/hide selects
  
  @Input() showPeriodSelect: boolean = true;
  @Input() showSediSelect: boolean = true;
  @Input() showRepartoSelect: boolean = true;
  @Input() showUserSelect: boolean = true;

  @Input() singleFieldOnRow: boolean = false;
  @Input() singleSelect: boolean = false;
  


  // Output parameters for selected values
  @Output() sediIdChange = new EventEmitter<number | undefined>();
  @Output() repartoIdChange = new EventEmitter<number[] | undefined>();
  @Output() allUsersIdChange = new EventEmitter<string[] | undefined>();
  @Output() currUserIdChange = new EventEmitter<string | undefined>();
  @Output() periodChange = new EventEmitter<{ year: number, month: number }>(); // Output for year/month period

  // Data lists
  public az_SediList: Az_SediModel[] = [];
  public az_SediRepartoList: Az_SediRepartoModel[] = [];
  public az_SediRepartoUserList: Az_SediRepartoUserModel[] = [];
  public filteredReparti: Az_SediRepartoModel[] = [];

  // Selected values
  public selectedSediId: number | null = null;
  public selectedRepartoId: number[] | null = null;
  public selectedUserId: string | null = null;

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
    private azSediRepartoUserServiceService: AzSediRepartoUserServiceService
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
            if (this.singleSelect) 
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

      if (this.singleSelect)
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

  public onUserChange() {
    if (this.selectedUserId && this.az_SediRepartoUserList.length > 0) {
      const newIndex = this.az_SediRepartoUserList.findIndex(user => user.idAspNetUsers === this.selectedUserId);
      if (newIndex !== -1) {
        this.currentUserIndex = newIndex;
      } else {
        this.selectedUserId = null;
        this.currentUserIndex = -1;
      }
    } else {
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
        this.az_SediRepartoUserList = repUser.filter(x => idAspNetUsers.includes(x.idAspNetUsers) && x.userInDepartment==true);
        this.allUsersIdChange.emit(this.az_SediRepartoUserList.map(x => x.idAspNetUsers));

        if (this.showUserSelect && this.az_SediRepartoUserList.length > 0) {
          this.selectedUserId = this.az_SediRepartoUserList[0].idAspNetUsers;
          this.onUserChange();
        }
      },
      error: err => {
        console.error("Error loading users for reparti:", err);
        this.az_SediRepartoUserList = [];
        this.allUsersIdChange.emit(this.az_SediRepartoUserList.map(x => x.idAspNetUsers));
      }
    });
  }

  public navigateToPreviousUser(): void {
    if (this.canNavigatePrevious) {
      this.currentUserIndex--;
      this.selectedUserId = this.az_SediRepartoUserList[this.currentUserIndex].idAspNetUsers;
      this.onUserChange();
    }
  }

  public navigateToNextUser(): void {
    if (this.canNavigateNext) {
      this.currentUserIndex++;
      this.selectedUserId = this.az_SediRepartoUserList[this.currentUserIndex].idAspNetUsers;
      this.onUserChange();
    }
  }

  public get canNavigatePrevious(): boolean {
    return this.az_SediRepartoUserList.length > 0 && this.currentUserIndex > 0;
  }

  public get canNavigateNext(): boolean {
    return this.az_SediRepartoUserList.length > 0 && this.currentUserIndex < this.az_SediRepartoUserList.length - 1;
  }

  public  columSize_4Sede():string {
    if (this.singleFieldOnRow)
      
      return 'col-sm-12' ;
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

  


}
