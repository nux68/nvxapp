import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CrmService } from '../../ClientServer-Service/Crm/crm.service';
import { CrmEmptyInModel, CrmParModel, CrmSetupGetOutModel } from '../../ClientServer-Service/Crm/Models/crm-model';
import { GenericRequest } from '../../ClientServer-Service/ModelsBase/generic-request';

/*
 Configurazione del CRM per le pagine (tabelle, utenti dell'azienda, permessi): si rilegge a ogni ingresso in pagina
 (la chiamata e' leggera e cosi' segue impersonificazioni e modifiche della configurazione).
*/
@Injectable({
  providedIn: 'root'
})
export class CrmSetupService {

  public setup: CrmSetupGetOutModel | null = null;

  constructor(private service: CrmService) { }

  async load(): Promise<CrmSetupGetOutModel | null> {
    const res = await firstValueFrom(this.service.SetupGet(new GenericRequest<CrmEmptyInModel>(CrmEmptyInModel)));
    this.setup = res.success ? res.data : null;
    return this.setup;
  }

  /** valori attivi (piu' quello gia' scelto, anche se disattivato) */
  attivi(lista: CrmParModel[] | undefined, scelto: number | null = null): CrmParModel[] {
    return (lista ?? []).filter(x => x.attivo || x.id === scelto);
  }

  utente(id: string | null): string {
    return this.setup?.utenti.find(u => u.id === id)?.userName ?? '';
  }

  descrizione(lista: CrmParModel[] | undefined, id: number | null): string {
    return (lista ?? []).find(x => x.id === id)?.descrizione ?? '';
  }
}

// importi e date nelle liste
export function crmImporto(v: number | null | undefined, valuta = 'EUR'): string {
  return (v ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ' + valuta;
}

export function crmData(v: string | null | undefined): string {
  return v ? new Date(v).toLocaleDateString() : '';
}

export function crmDataOra(v: string | null | undefined): string {
  return v ? new Date(v).toLocaleString(undefined, { dateStyle: 'short', timeStyle: 'short' }) : '';
}

// valore per <ion-input type="date"> e "datetime-local"
export function crmPerInputData(v: string | null | undefined): string | null {
  return v ? v.substring(0, 10) : null;
}

export function crmPerInputDataOra(v: string | null | undefined): string | null {
  return v ? v.substring(0, 16) : null;
}

// data/ora locale nel formato del server (senza fuso)
export function crmAdesso(minuti = 0): string {
  const d = new Date(Date.now() + minuti * 60000);
  const p = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}T${p(d.getHours())}:${p(d.getMinutes())}`;
}
