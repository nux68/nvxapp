import { Injectable } from '@angular/core';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';
import { TimbraturaEvolutaService } from '../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/timbratura-evoluta.service';
import { TimbraturaEvoluta_StampInModel } from '../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/Models/timbratura-evoluta-model';

/** timbratura in coda: dati della richiesta con l'ora del dispositivo */
export interface TimbraturaInCoda {
  id: string;
  dati: TimbraturaEvoluta_StampInModel;
  dataDispositivo: string;
  scartata?: string;      // motivo del rifiuto del server (resta visibile finche' l'utente non la toglie)
}

/*
 Timbrature offline (AT-TIMB.4, ANALISI_ATTENDANCETRACKING_TIMBRATURA.md §4.4): se la chiamata fallisce per la rete
 (o il browser e' offline) la timbratura resta nel localStorage con l'ora del dispositivo; all'evento "online", all'apertura
 della pagina e con "Invia ora" si inviano in ordine con Offline = true. Il server le accetta entro il limite della sede
 e le marca come differite; quelle rifiutate restano in elenco come scartate.
 Il caricamento dell'app senza rete richiederebbe un service worker (non installato): estensione.
*/
@Injectable({
  providedIn: 'root'
})
export class TimbraturaOfflineService {

  private static readonly Chiave = 'nvx.timbrature.offline';
  private invioInCorso = false;
  public readonly coda$ = new BehaviorSubject<TimbraturaInCoda[]>(this.leggi());

  constructor(private timbraturaEvolutaService: TimbraturaEvolutaService) {
    window.addEventListener('online', () => { this.inviaCoda(); });
  }

  private leggi(): TimbraturaInCoda[] {
    try { return JSON.parse(localStorage.getItem(TimbraturaOfflineService.Chiave) ?? '[]'); }
    catch { return []; }
  }

  private scrivi(coda: TimbraturaInCoda[]) {
    try { localStorage.setItem(TimbraturaOfflineService.Chiave, JSON.stringify(coda)); } catch { }
    this.coda$.next(coda);
  }

  static oraLocale(d: Date): string {
    const p = (n: number) => n.toString().padStart(2, '0');
    return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}:${p(d.getSeconds())}`;
  }

  accoda(dati: TimbraturaEvoluta_StampInModel) {
    const ora = TimbraturaOfflineService.oraLocale(new Date());
    const copia = { ...dati, offline: true, dataDispositivo: ora };
    this.scrivi([...this.leggi(), { id: Date.now() + '-' + Math.random().toString(36).substring(2, 8), dati: copia, dataDispositivo: ora }]);
  }

  togli(id: string) {
    this.scrivi(this.leggi().filter(x => x.id !== id));
  }

  /** invia in ordine le timbrature in coda; si ferma al primo errore di rete */
  async inviaCoda(): Promise<void> {
    if (this.invioInCorso || !navigator.onLine)
      return;
    this.invioInCorso = true;
    try {
      for (const item of this.leggi().filter(x => !x.scartata)) {
        const request = new GenericRequest<TimbraturaEvoluta_StampInModel>(TimbraturaEvoluta_StampInModel);
        request.data = item.dati;
        try {
          const res = await firstValueFrom(this.timbraturaEvolutaService.Stamp(request));
          const coda = this.leggi();
          if (res.success)
            this.scrivi(coda.filter(x => x.id !== item.id));
          else
            this.scrivi(coda.map(x => x.id === item.id ? { ...x, scartata: res.messages?.map(m => m.text).join(' ') || 'KO' } : x));
        }
        catch {
          break;   // ancora senza rete
        }
      }
    }
    finally {
      this.invioInCorso = false;
    }
  }
}
