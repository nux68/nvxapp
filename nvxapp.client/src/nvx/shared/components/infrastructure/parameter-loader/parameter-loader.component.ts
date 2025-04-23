import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core';
import { ParameterService } from '../../../../ClientServer-Service/Infrastructure/Parameter/parameter.service';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { catchError, concatMap, delay, of, retry, tap, timer } from 'rxjs';
import { merge } from 'rxjs/internal/observable/merge';
import { concat } from 'rxjs/internal/observable/concat';
import { from } from 'rxjs/internal/observable/from';
import { RolesListInModel } from '../../../../ClientServer-Service/Infrastructure/Parameter/Models/roles-model';
import { AuthService } from '../../../../Utility/infrastructure/auth.service';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';

@Component({
  selector: 'app-parameter-loader',
  templateUrl: './parameter-loader.component.html',
  styleUrls: ['./parameter-loader.component.scss'],
  standalone:false
})
export class ParameterLoaderComponent  implements OnInit {
  public isModalOpen = false;
  public canClose = false;
  public progress = 0;
  public currStep = 0;


  constructor(
    private parameterService: ParameterService,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
    private authService: AuthService
    ){}



  ngOnInit() {
    
    this.isModalOpen = true;  

    //caricamento parametri iniziali
    this.startProgress( this.AddCallInfrastructure() );
    

    //caricameno parametri azie/user
    this.authService.Roles$.subscribe(res => {

      if (this.authService.IsUser || this.authService.IsInGroupCompanyAdmin) {

        if (this.sharedParameterGestionePresenzeService.IsLoad == false) {

          let presCall: any[] = [];
          presCall = this.sharedParameterGestionePresenzeService.InitCall(this.updateProgress.bind(this));
          this.currStep = 0;
          this.startProgress(presCall);
          this.sharedParameterGestionePresenzeService.IsLoad = true;
        }
      }
      else {
        this.sharedParameterGestionePresenzeService.IsLoad = false;
      }
    });

  }

  private AddCallInfrastructure(): any[]  {

    let calls: any[] = [];

    for (let i = 0; i < 50; i++) {
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
            // Puoi decidere cosa fare in caso di fallimento finale
            return of(null); // Continua per evitare il blocco
          })
        )

      );
    }

    return calls;

  }


  async startProgress(calls: any[]) {
    this.isModalOpen = true;

    this.progress = 0; // Resetta la progress bar

    from(calls).pipe(
      concatMap(call => call)  
    ).subscribe({
      complete: () => {
        this.isModalOpen = false; 
        this.canClose = true;
      }
    });
  }

  updateProgress(calls: any[]):void {
    this.currStep = this.currStep + 1;
    this.progress = Math.floor(this.currStep * (100 / calls.length)); // Aggiorna la barra progressivamente
  }

}
