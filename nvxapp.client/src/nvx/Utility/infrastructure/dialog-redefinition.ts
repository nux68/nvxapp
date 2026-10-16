import { InjectionToken, Type } from '@angular/core';
import { ApplicationType } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';

/**
 * Ridefinizione dei dialog di Infrastructure da parte degli applicativi.
 *
 * Una pagina di Infrastructure non apre direttamente il proprio dialog ma lo chiede per nome:
 *   const component = await mainMenuService.RedefineDialog(DialogName.AddUserCompany, AddUserCompanyComponent);
 *
 * Ogni applicativo (ramo) che vuole sostituire uno o piu' dialog registra la propria implementazione
 * in app.module, tra i providers:
 *   { provide: DIALOG_REDEFINITION, useClass: <ClasseDellApplicativo>, multi: true }
 *
 * MainMenuService.RedefineDialog interroga solo le ridefinizioni i cui applicativi sono TUTTI attivi
 * per l'azienda dell'utente, dalla priorita' piu' alta: vince la prima che restituisce un componente.
 * Senza ridefinizioni (es. sul ramo Infrastructure) si apre il dialog predefinito.
 *
 * Rami che uniscono piu' applicativi (es. presenze + un altro argomento): oltre alle ridefinizioni
 * dei singoli applicativi (portate dai merge) si registra una ridefinizione combinata, con tutti gli
 * applicativi in `applications` e priorita' piu' alta. Vale solo quando sono attivi insieme; se ne e'
 * attivo uno solo si usa la ridefinizione di quell'applicativo.
 *
 * Priorita' consigliate: 10 per un singolo applicativo, 100 per le combinazioni.
 * A parita' di priorita' vale l'ordine di registrazione in app.module.
 *
 * I componenti restituiti devono essere standalone: la pagina di Infrastructure che li apre
 * non importa i moduli dell'applicativo.
 */
export interface DialogRedefinition {
  /** applicativi richiesti: la ridefinizione vale solo se sono tutti attivi per l'azienda dell'utente */
  readonly applications: ApplicationType[];
  /** con piu' ridefinizioni valide per lo stesso dialog vince la priorita' piu' alta */
  readonly priority: number;
  /** componente che sostituisce il dialog indicato, null se l'applicativo non lo ridefinisce */
  redefineDialog(dialogName: string): Promise<Type<any> | null>;
}

export const DIALOG_REDEFINITION = new InjectionToken<DialogRedefinition[]>('DIALOG_REDEFINITION');

/** Nomi dei dialog di Infrastructure che un applicativo puo' ridefinire. */
export const DialogName = {
  /** inserimento di un nuovo utente nell'azienda (lista utenti) */
  AddUserCompany: 'addusercompany',
} as const;
