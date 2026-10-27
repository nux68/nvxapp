import { Injectable } from '@angular/core';

/*
 Funzionalita' opzionali della gestione presenze, in un unico punto.
 Per ora sono tutte attive: quando ci sara' una configurazione (es. in Az_Cfg) basta leggerla qui,
 le pagine chiedono solo se la funzionalita' e' attiva e funzionano anche senza.
*/
@Injectable({
  providedIn: 'root'
})
export class FunzionalitaGestionePresenzeService {

  /**
   * Sezione "Spesa" del rapporto di lavoro (autovetture per i rimborsi km).
   * Se disattiva l'anagrafica dipendente non mostra la scheda e al salvataggio non manda le autovetture,
   * cosi' il server le lascia come sono.
   */
  get sezioneSpesaRapporto(): boolean {
    return true;
  }
}
