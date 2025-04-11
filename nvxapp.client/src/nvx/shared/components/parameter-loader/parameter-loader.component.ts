import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core';
import { ParameterService } from '../../../ClientServer-Service/Parameter/parameter.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { RolesListInModel } from '../../../ClientServer-Service/Parameter/Models/roles-model';
import { concatMap, tap } from 'rxjs';
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
    setTimeout(() => {
      this.isModalOpen = true;  // ✅ Apri il modal dopo 2 secondi
      this.startProgress();
    }, 2000); // 2000 ms = 2 secondi
  }

  async startProgress() {
    this.isModalOpen = true;

    this.progress = 0; // Resetta la progress bar
    let request: GenericRequest<RolesListInModel> = new GenericRequest<RolesListInModel>(RolesListInModel);

    //TEMPOARNEO SOLO DEMO
    for (var i = 0; i < 50;i++)
      this.calls.push(this.parameterService.Load_Roles(request).pipe(tap(() => this.updateProgress())));
    

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
