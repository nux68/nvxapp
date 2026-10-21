import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ParameterService } from '../../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { catchError, combineLatest, concatMap, from, of, retry, tap, timer } from 'rxjs';
import { RolesListInModel } from '../../../../ClientServer-Service/Infrastructure/Parameter/Models/roles-model';
import { AuthService } from '../../../../Utility/infrastructure/auth.service';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';
import { ApplicationType } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';

@Component({
  selector: 'app-parameter-loader',
  templateUrl: './parameter-loader.component.html',
  styleUrls: ['./parameter-loader.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone:false
})
export class ParameterLoaderComponent  implements OnInit {
  public isModalOpen = false;
  public canClose = false;
  public progress = 0;
  public currStep = 0;

  // ruoli (Parameter/Roles) gia' caricati: servono solo alle pagine dopo il login
  private rolesLoaded = false;

  // chiamate ancora da completare e totale della barra di avanzamento
  private pendingCalls = 0;
  private totalSteps = 0;


  constructor(
    private parameterService: ParameterService,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private authService: AuthService
    ){}



  ngOnInit() {

    // All'avvio non si carica nulla: le pagine vetrina (prima del login) non usano i parametri.
    // Tutto viene caricato quando arrivano i ruoli dell'utente, cioe' al login.

    //caricameno parametri azie/user
    // parametri delle presenze solo se l'applicativo AttendanceTracking e' attivo per l'azienda:
    // altrimenti il server rifiuta le chiamate (ApplicationNotActiveException).
    // Ricontrollato a ogni cambio di ruoli o di applicativi attivi (login, impersonificazione).
    combineLatest([this.authService.Roles$, this.authService.ActiveApplications$]).subscribe(([roles]) => {

      let calls: any[] = [];

      //caricamento parametri iniziali (ruoli), una sola volta dopo il login
      if (roles.length > 0 && this.rolesLoaded == false) {
        this.rolesLoaded = true;
        calls = calls.concat(this.AddCallInfrastructure());
      }

      if ((this.authService.IsUser || this.authService.IsInGroupCompanyAdmin) &&
          this.authService.hasApplication(ApplicationType.AttendanceTracking)) {

        if (this.sharedParameterGestionePresenzeService.IsLoad == false) {
          calls = calls.concat(this.sharedParameterGestionePresenzeService.InitCall(this.updateProgress.bind(this)));
          this.sharedParameterGestionePresenzeService.IsLoad = true;
        }
      }
      else {
        this.sharedParameterGestionePresenzeService.IsLoad = false;
      }

      if (calls.length > 0) {
        this.startProgress(calls);
      }
    });

  }

  private AddCallInfrastructure(): any[]  {

    let calls: any[] = [];

    //for (let i = 0; i < 50; i++) {
      calls.push(

        this.parameterService.Load_Roles(new GenericRequest<RolesListInModel>(RolesListInModel)).pipe(
          tap(() => this.updateProgress(calls)),
          retry({
            count: 20, // Numero massimo di tentativi
            delay: (error, retryCount) => {
              console.error(`Errore rilevato, ritento dopo ${retryCount} secondi:`, error);
              return timer(3000); // Ritenta dopo 3 secondi
            }
          }),
          catchError((error) => {
            console.error(`Errore durante il caricamento del ruolo:`, error);
            this.rolesLoaded = false; // si ritenta al prossimo login
            // Puoi decidere cosa fare in caso di fallimento finale
            return of(null); // Continua per evitare il blocco
          })
        )

      );
    //}

    return calls;

  }


  async startProgress(calls: any[]) {
    // un caricamento puo' partire mentre un altro e' in corso (ruoli e applicativi arrivano separati):
    // la barra conta tutte le chiamate e il riquadro si chiude solo quando sono finite tutte
    if (this.pendingCalls == 0) {
      this.currStep = 0;
      this.totalSteps = 0;
      this.progress = 0; // Resetta la progress bar
    }
    this.pendingCalls += calls.length;
    this.totalSteps += calls.length;
    this.canClose = false;
    this.isModalOpen = true;

    from(calls).pipe(
      concatMap(call => call)
    ).subscribe({
      complete: () => {
        this.pendingCalls -= calls.length;
        if (this.pendingCalls == 0) {
          this.isModalOpen = false;
          this.canClose = true;
        }
      }
    });
  }

  updateProgress(calls: any[]):void {
    this.currStep = this.currStep + 1;
    this.progress = Math.min(100, Math.floor(this.currStep * (100 / this.totalSteps))); // Aggiorna la barra progressivamente
  }

}
