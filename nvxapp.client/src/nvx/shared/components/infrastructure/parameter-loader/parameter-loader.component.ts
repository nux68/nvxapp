import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core';
import { ParameterService } from '../../../../ClientServer-Service/Parameter/parameter.service';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { RolesListInModel } from '../../../../ClientServer-Service/Parameter/Models/roles-model';
import { catchError, concatMap, delay, of, retry, retryWhen, tap, timer } from 'rxjs';
import { merge } from 'rxjs/internal/observable/merge';
import { concat } from 'rxjs/internal/observable/concat';
import { from } from 'rxjs/internal/observable/from';

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

  private calls: any[] =[];

  constructor(
    private parameterService: ParameterService,
    )
  {
    
  }



  ngOnInit() {
    //setTimeout(() => {
      this.isModalOpen = true;  // ✅ Apri il modal dopo 2 secondi
      this.startProgress();
    //}, 1000); // 2000 ms = 2 secondi
  }

  async startProgress() {
    this.isModalOpen = true;

    this.progress = 0; // Resetta la progress bar
    let request: GenericRequest<RolesListInModel> = new GenericRequest<RolesListInModel>(RolesListInModel);

    //TEMPOARNEO SOLO DEMO
    //for (let i = 0; i < 50; i++) {
    //  this.calls.push(
    //    this.parameterService.Load_Roles(request).pipe(
    //      tap(() => this.updateProgress()),
    //      retryWhen(errors =>
    //        errors.pipe(
    //          tap((error) => {
    //            console.error(`Errore rilevato, ritento dopo qualche secondo:`, error);
    //          }),
    //          delay(3000) // Ritenta dopo 3 secondi
    //        )
    //      ),
    //      catchError((error) => {
    //        console.error(`Errore durante il caricamento del ruolo:`, error);
    //        // Puoi decidere cosa fare in caso di fallimento finale
    //        return of(null); // Continua per evitare il blocco
    //      })
    //    )
    //  );
    //}

    for (let i = 0; i < 50; i++) {
      this.calls.push(
        this.parameterService.Load_Roles(request).pipe(
          tap(() => this.updateProgress()),
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

    from(this.calls).pipe(
      concatMap(call => call)  
    ).subscribe({
      complete: () => {
        this.isModalOpen = false; 
        this.canClose = true;
      }
    });
  }

  updateProgress() {
    this.currStep = this.currStep + 1;
    this.progress = Math.floor(this.currStep * (100 / this.calls.length)); // Aggiorna la barra progressivamente
  }

}
