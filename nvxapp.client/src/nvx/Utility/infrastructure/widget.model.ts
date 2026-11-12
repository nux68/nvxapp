import { Type } from '@angular/core';
import { ApplicationType } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';

/*
 Widget della dashboard (ANALISI_INFRASTRUCTURE_WIDGET.md).
 Ogni applicativo dichiara i propri widget in un fornitore (iWidgetProvider) nel proprio ramo:
   - widget-infrastructure.service.ts       : Infrastructure (sempre presente)
   - widget-attendance-tracking.service.ts  : Presenze      (in Infrastructure e' vuoto, lo riempie il ramo AttendanceTracking)
   - widget-crm.service.ts                  : CRM           (in Infrastructure e' vuoto, lo riempie il ramo CRM)
   - widget-document-management.service.ts  : Documentale   (in Infrastructure e' vuoto, lo riempie il ramo DocumentManagement)
 Il catalogo dipende quindi dal ramo compilato: AllFunction li ha tutti, il ramo presenze solo Infrastructure + Presenze.
*/

/** larghezza nella griglia della dashboard (12 colonne; sotto i 768px ogni widget occupa tutta la riga) */
export type WidgetSize = 'small' | 'medium' | 'large';

export interface WidgetDefinition {
  /** chiave unica e stabile (si salva sul server): "<applicativo>.<nome>", es. "documents.storage" */
  key: string;
  /** applicativo a cui appartiene (assente = Infrastructure): il widget c'e' solo se l'applicativo e' attivo per l'azienda */
  application?: ApplicationType;
  /** widget di Infrastructure disponibile solo se l'azienda ha almeno uno di questi applicativi (es. le richieste) */
  anyApplication?: ApplicationType[];
  /** chiavi di traduzione del titolo e della descrizione (pagina di configurazione) */
  title: string;
  description: string;
  icon: string;
  /** ruoli che possono usarlo (assente = tutti) */
  roles?: string[];
  size: WidgetSize;
  /** nella dashboard di chi non l'ha ancora configurata */
  default?: boolean;
  /** ordine nel catalogo e tra i predefiniti */
  zorder: number;
  /** senza cornice (titolo e bordo li disegna il widget) */
  frameless?: boolean;
  /** componente standalone che mostra il widget: carica da solo i propri dati */
  component: Type<unknown>;
}

/** fornitore dei widget di un applicativo (uno per ramo) */
export interface iWidgetProvider {
  readonly Widgets: WidgetDefinition[];
}
