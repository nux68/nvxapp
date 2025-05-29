import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { AuthService } from '../infrastructure/auth.service';
import { SharedParameterGestionePresenzeService } from '../../shared/shared-parameter-gestione-presenze.service';

@Injectable({
  providedIn: 'root'
})
export class RefresherService {

  private SharedParameterGestionePresenze_refreshSubject = new BehaviorSubject<void>(undefined);

  private Dip_GG_Richiesta_refreshSubject = new BehaviorSubject<void>(undefined);

  // Observable a cui i componenti possono iscriversi
  public Dip_GG_Richiesta_refresh$: Observable<void> = this.Dip_GG_Richiesta_refreshSubject.asObservable();
  public SharedParameterGestionePresenze_refresh$: Observable<void> = this.SharedParameterGestionePresenze_refreshSubject.asObservable();

  constructor(private authService: AuthService,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService) { }

  // Chiamare questo metodo per notificare il refresh
  public Dip_GG_Richiesta_triggerRefresh(): void {
    this.Dip_GG_Richiesta_refreshSubject.next();
  }

  public SharedParameterGestionePresenze_triggerRefresh(): void {
    this.sharedParameterGestionePresenzeService.IsLoad = false;
    this.authService.forceRolesEmission(); // Forza l'emissione dei ruoli per aggiornare i parametri
    this.SharedParameterGestionePresenze_refreshSubject.next();
  }

}
