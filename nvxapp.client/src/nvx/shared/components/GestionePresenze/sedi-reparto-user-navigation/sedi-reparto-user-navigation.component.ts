import { Component, OnInit } from '@angular/core';
import { AzSediRepartoService } from '../../../../ClientServer-Service/GestionePresenze/Az_SediReparto/az-sedi-reparto.service';
import { AzSediService } from '../../../../ClientServer-Service/GestionePresenze/Az_Sedi/az-sedi.service';
import { AzSediRepartoUserServiceService } from '../../../../ClientServer-Service/GestionePresenze/Az_SediRepartoUser/az-sedi-reparto-user-service.service';
import { Az_SediModel, Az_Sedi_GetAll_InModel } from '../../../../ClientServer-Service/GestionePresenze/Az_Sedi/Models/az-sedi-model';
import { GenericRequest } from '../../../../ClientServer-Service/ModelsBase/generic-request';
import { Az_SediReparto_GetAll_InModel, Az_SediRepartoModel } from '../../../../ClientServer-Service/GestionePresenze/Az_SediReparto/Models/az-sedi-reparto-model';
import { Az_SediRepartoUser_GetAll_Period_InModel, Az_SediRepartoUserModel } from '../../../../ClientServer-Service/GestionePresenze/Az_SediRepartoUser/Models/az-reparto-user-model';
import { catchError, forkJoin, map, Observable, throwError } from 'rxjs';

@Component({
  selector: 'app-sedi-reparto-user-navigation',
  templateUrl: './sedi-reparto-user-navigation.component.html',
  styleUrls: ['./sedi-reparto-user-navigation.component.scss'],
})
export class SediRepartoUserNavigationComponent  implements OnInit {

  public az_SediList: Az_SediModel[] = [];
  public az_SediRepartoList: Az_SediRepartoModel[] = [];
  public az_SediRepartoUserList: Az_SediRepartoUserModel[] = [];

  

  constructor(private azSediService: AzSediService,
              private azSediRepartoService: AzSediRepartoService,
              private azSediRepartoUserServiceService: AzSediRepartoUserServiceService)
  {

  }

  ngOnInit() {
    this.Load_Init().subscribe(x => {});
  }

  

  private Load_Init(): Observable<boolean> {

    let request1: GenericRequest<Az_Sedi_GetAll_InModel> = new GenericRequest<Az_Sedi_GetAll_InModel>(Az_Sedi_GetAll_InModel);
    const azSediResultObservable$ = this.azSediService.GetAll(request1);

    let request2: GenericRequest<Az_SediReparto_GetAll_InModel> = new GenericRequest<Az_SediReparto_GetAll_InModel>(Az_SediReparto_GetAll_InModel);
    const azSediRepartoResultObservable$ =this.azSediRepartoService.GetAll(request2);

    return forkJoin({
      sediResult: azSediResultObservable$,
      sediRepartoResult: azSediRepartoResultObservable$
    }).pipe(
        map(results => {

          this.az_SediList        = results.sediResult?.data?.az_Sedi || [];
          this.az_SediRepartoList = results.sediRepartoResult?.data?.az_SediReparto || [];
        
          return true; 
        }),
        // 5. Optional: Add error handling for the forkJoin
        catchError(error => {
          console.error("SediRepartoUserNavigation Error fetching data .", error);
          return throwError(() => new Error('SediRepartoUserNavigation Failed to load data ' ));
        })
    );

  }



  private LoadAz_SediRepartoUser(idAz_Reparto: number) {


    let request: GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel> = new GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel>(Az_SediRepartoUser_GetAll_Period_InModel);
    this.azSediRepartoUserServiceService.GetAllPeriod(request).subscribe(res=>{
      this.az_SediRepartoUserList = res.data.az_RepartoUser;
    });


  }

}
