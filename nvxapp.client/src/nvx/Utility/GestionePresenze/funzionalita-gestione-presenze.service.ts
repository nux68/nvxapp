import { Injectable } from '@angular/core';
import { AuthService } from '../infrastructure/auth.service';
import { ApplicationType, LivelloLicenza } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';

/*
 Funzionalita' opzionali della gestione presenze, in un unico punto.
 Per ora sono tutte attive: quando ci sara' una configurazione (es. in Az_Cfg) basta leggerla qui,
 le pagine chiedono solo se la funzionalita' e' attiva e funzionano anche senza.
*/
@Injectable({
  providedIn: 'root'
})
export class FunzionalitaGestionePresenzeService {

  constructor(private authService: AuthService) { }

  /**
   * Formule personalizzate del calcolo giornata (ANALISI_ATTENDANCETRACKING_FORMULE.md): solo CompanyPowerAdmin con il piano Pro.
   * Senza, l'anagrafica dipendente non mostra la scheda "Profilo formule" (il server ricontrolla sempre).
   */
  get formulePersonalizzate(): boolean {
    return this.authService.IsCompanyPowerAdmin && this.authService.hasLicenseAtLeast(ApplicationType.AttendanceTracking, LivelloLicenza.Pro);
  }

  /**
   * Sezione "Spesa" del rapporto di lavoro (autovetture per i rimborsi km).
   * Se disattiva l'anagrafica dipendente non mostra la scheda e al salvataggio non manda le autovetture,
   * cosi' il server le lascia come sono.
   */
  get sezioneSpesaRapporto(): boolean {
    return true;
  }
}
