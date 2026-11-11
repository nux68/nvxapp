import { formatDate } from '@angular/common';
import { TranslocoService } from '@jsverse/transloco';
import { CmpApi } from '../../ClientServer-Service/Compliance/cmp.service';
import { CmpRiga } from '../../ClientServer-Service/Compliance/Models/cmp-model';

/*
 Sezioni di Compliance (ANALISI_COMPLIANCE_APPLICATIVO.md): ogni sezione e' un elenco con salvataggio e cancellazione sul server
 (Cmp_<server>List / Put / Delete) e la descrizione di colonne (pagina elenco) e campi (pagina di modifica).
 Le stesse pagine servono le tre aree:
    - '81'     : sicurezza sul lavoro (CMP-81), amministratori dell'azienda; le visite anche agli abilitati ai dati sanitari
    - 'gdpr'   : registro GDPR (CMP-GDPR), amministratori dell'azienda
    - 'studio' : registro GDPR e scadenze del modulo delle aziende dello studio (CMP-GDPR.4)
 Testi: cmp.sections.<key>, cmp.fields.<campo>, cmp.enums.<enum>.<valore> (assets/i18n/compliance).
*/

export type CmpArea = '81' | 'gdpr' | 'studio';
export type CmpLookup = 'dipendenti' | 'mansioni' | 'dpi' | 'sedi';

export interface CmpCampo {
  key: string;
  tipo: 'text' | 'textarea' | 'number' | 'date' | 'datetime' | 'select' | 'toggle';
  max?: number;
  min?: number;
  required?: boolean;
  /** select da enumerato: chiavi cmp.enums.<enumKey>.<valore> */
  enumKey?: string;
  enumValues?: number[];
  /** select da tabella (Cmp_Lookup) */
  lookup?: CmpLookup;
  /** select con la voce "nessuno" */
  nullable?: boolean;
  /** solo per gli utenti abilitati ai dati sanitari */
  sanitario?: boolean;
  /** nota sotto il campo (chiave di traduzione) */
  nota?: string;
}

export interface CmpColonna {
  key: string;
  tipo?: 'text' | 'date' | 'datetime' | 'enum' | 'bool' | 'presaVisione' | 'number';
  enumKey?: string;
  mobile?: 'title' | 'subtitle' | 'note' | 'hidden';
  align?: 'start' | 'center' | 'end';
  width?: string;
}

export interface CmpSezione {
  key: string;
  api: CmpApi;
  /** nome sul server: Cmp_<server>List / Put / Delete */
  server: string;
  icona: string;
  colonne: CmpColonna[];
  /** vuoto = sezione in sola lettura */
  campi: CmpCampo[];
  nuovo?: () => any;
  /** visibile anche agli abilitati ai dati sanitari che non sono amministratori */
  sanitario?: boolean;
  /** elenco con un'API propria */
  speciale?: 'stato' | 'scadenze';
}

const range = (da: number, a: number) => Array.from({ length: a - da + 1 }, (_, i) => da + i);
const oggi = () => new Date().toISOString().substring(0, 10);
const adesso = () => { const d = new Date(); d.setMinutes(d.getMinutes() - d.getTimezoneOffset()); return d.toISOString().substring(0, 16); };

const NOTE: CmpCampo = { key: 'note', tipo: 'textarea', max: 500 };
const DIPENDENTE: CmpCampo = { key: 'idDip_Anagrafica', tipo: 'select', lookup: 'dipendenti', required: true };

export const CMP_SEZIONI: Record<string, CmpSezione> = {
  // ------------------------------------------------------------------ CMP-81
  formazione: {
    key: 'formazione', api: 'Cmp_Sicurezza', server: 'Formazione', icona: 'school-outline',
    colonne: [
      { key: 'dipendente', mobile: 'title' }, { key: 'descrizione', mobile: 'subtitle' }, { key: 'tipoCorso', tipo: 'enum', enumKey: 'corso' },
      { key: 'dataCorso', tipo: 'date', width: '120px' }, { key: 'dataScadenza', tipo: 'date', mobile: 'note', width: '120px' }, { key: 'ente', mobile: 'hidden' },
    ],
    campi: [
      DIPENDENTE, { key: 'tipoCorso', tipo: 'select', enumKey: 'corso', enumValues: range(0, 10) },
      { key: 'descrizione', tipo: 'text', max: 200, required: true }, { key: 'dataCorso', tipo: 'date', required: true }, { key: 'ore', tipo: 'number', min: 0 },
      { key: 'ente', tipo: 'text', max: 100 }, { key: 'dataScadenza', tipo: 'date', nota: 'cmp.pages.edit.courseExpiryNote' },
      { key: 'attestato', tipo: 'text', max: 200, nota: 'cmp.pages.edit.referenceNote' }, NOTE,
    ],
    nuovo: () => ({ id: 0, tipoCorso: 0, dataCorso: oggi() }),
  },
  stato: {
    key: 'stato', api: 'Cmp_Sicurezza', server: 'FormazioneStato', icona: 'shield-checkmark-outline', speciale: 'stato',
    colonne: [
      { key: 'dipendente', mobile: 'title' }, { key: 'tipoCorso', tipo: 'enum', enumKey: 'corso', mobile: 'subtitle' }, { key: 'mansioni', mobile: 'hidden' },
      { key: 'dataCorso', tipo: 'date', width: '120px' }, { key: 'dataScadenza', tipo: 'date', width: '120px' },
      { key: 'stato', tipo: 'enum', enumKey: 'statoFormazione', mobile: 'note' },
    ],
    campi: [],
  },
  visite: {
    key: 'visite', api: 'Cmp_Sicurezza', server: 'Visita', icona: 'medkit-outline', sanitario: true,
    colonne: [
      { key: 'dipendente', mobile: 'title' }, { key: 'tipo', tipo: 'enum', enumKey: 'visita' }, { key: 'dataVisita', tipo: 'date', width: '120px' },
      { key: 'esito', tipo: 'enum', enumKey: 'esito', mobile: 'subtitle' }, { key: 'dataScadenza', tipo: 'date', mobile: 'note', width: '120px' },
    ],
    campi: [
      DIPENDENTE, { key: 'tipo', tipo: 'select', enumKey: 'visita', enumValues: range(0, 5) }, { key: 'dataVisita', tipo: 'date', required: true },
      { key: 'esito', tipo: 'select', enumKey: 'esito', enumValues: range(0, 3) }, { key: 'limitazioni', tipo: 'textarea', max: 500, nota: 'cmp.pages.edit.limitsNote' },
      { key: 'dataScadenza', tipo: 'date' }, { key: 'medico', tipo: 'text', max: 100 },
      { key: 'datiSanitari', tipo: 'textarea', max: 4000, sanitario: true, nota: 'cmp.pages.edit.healthDataNote' },
    ],
    nuovo: () => ({ id: 0, tipo: 1, esito: 0, dataVisita: oggi() }),
  },
  consegne: {
    key: 'consegne', api: 'Cmp_Sicurezza', server: 'DpiConsegna', icona: 'shirt-outline',
    colonne: [
      { key: 'dipendente', mobile: 'title' }, { key: 'dpi', mobile: 'subtitle' }, { key: 'dataConsegna', tipo: 'date', width: '120px' },
      { key: 'quantita', tipo: 'number', align: 'end', mobile: 'hidden' }, { key: 'taglia', mobile: 'hidden' },
      { key: 'dataPresaVisione', tipo: 'presaVisione', mobile: 'note' },
    ],
    campi: [
      DIPENDENTE, { key: 'idCmp_Dpi', tipo: 'select', lookup: 'dpi', required: true }, { key: 'dataConsegna', tipo: 'date', required: true },
      { key: 'quantita', tipo: 'number', min: 1, required: true }, { key: 'taglia', tipo: 'text', max: 20 }, NOTE,
    ],
    nuovo: () => ({ id: 0, quantita: 1, dataConsegna: oggi() }),
  },
  nomine: {
    key: 'nomine', api: 'Cmp_Sicurezza', server: 'Nomina', icona: 'ribbon-outline',
    colonne: [
      { key: 'tipo', tipo: 'enum', enumKey: 'nomina', mobile: 'title' }, { key: 'persona', mobile: 'subtitle' }, { key: 'sede', mobile: 'hidden' },
      { key: 'dataInizio', tipo: 'date', width: '120px' }, { key: 'dataFine', tipo: 'date', mobile: 'note', width: '120px' },
      { key: 'dataPresaVisione', tipo: 'presaVisione', mobile: 'hidden' },
    ],
    campi: [
      { key: 'tipo', tipo: 'select', enumKey: 'nomina', enumValues: range(0, 8) },
      { key: 'idDip_Anagrafica', tipo: 'select', lookup: 'dipendenti', nullable: true, nota: 'cmp.pages.edit.appointeeNote' },
      { key: 'nominativoEsterno', tipo: 'text', max: 100 }, { key: 'idAz_Sedi', tipo: 'select', lookup: 'sedi', nullable: true },
      { key: 'dataInizio', tipo: 'date', required: true }, { key: 'dataFine', tipo: 'date' },
      { key: 'documento', tipo: 'text', max: 200, nota: 'cmp.pages.edit.referenceNote' }, NOTE,
    ],
    nuovo: () => ({ id: 0, tipo: 0, idDip_Anagrafica: null, idAz_Sedi: null, dataInizio: oggi() }),
  },
  documenti: {
    key: 'documenti', api: 'Cmp_Sicurezza', server: 'Documento', icona: 'document-text-outline',
    colonne: [
      { key: 'titolo', mobile: 'title' }, { key: 'tipo', tipo: 'enum', enumKey: 'documento', mobile: 'subtitle' }, { key: 'versione', mobile: 'hidden' },
      { key: 'dataRevisione', tipo: 'date', width: '120px' }, { key: 'dataScadenza', tipo: 'date', mobile: 'note', width: '120px' },
    ],
    campi: [
      { key: 'tipo', tipo: 'select', enumKey: 'documento', enumValues: range(0, 4) }, { key: 'titolo', tipo: 'text', max: 200, required: true },
      { key: 'versione', tipo: 'text', max: 20 }, { key: 'dataRevisione', tipo: 'date', required: true }, { key: 'dataScadenza', tipo: 'date' },
      { key: 'riferimento', tipo: 'text', max: 200, nota: 'cmp.pages.edit.referenceNote' }, NOTE,
    ],
    nuovo: () => ({ id: 0, tipo: 0, dataRevisione: oggi() }),
  },
  mansioni: {
    key: 'mansioni', api: 'Cmp_Sicurezza', server: 'Mansione', icona: 'construct-outline',
    colonne: [
      { key: 'descrizione', mobile: 'title' }, { key: 'rischio', tipo: 'enum', enumKey: 'rischio', mobile: 'subtitle' },
      { key: 'numeroCorsi', tipo: 'number', align: 'end', mobile: 'hidden' }, { key: 'numeroDipendenti', tipo: 'number', align: 'end', mobile: 'note' },
    ],
    campi: [
      { key: 'descrizione', tipo: 'text', max: 100, required: true }, { key: 'rischio', tipo: 'select', enumKey: 'rischio', enumValues: range(0, 2) },
      NOTE, { key: 'attivo', tipo: 'toggle' },
    ],
    nuovo: () => ({ id: 0, rischio: 0, attivo: true }),
  },
  corsiMansione: {
    key: 'corsiMansione', api: 'Cmp_Sicurezza', server: 'MansioneCorso', icona: 'list-outline',
    colonne: [
      { key: 'mansione', mobile: 'title' }, { key: 'tipoCorso', tipo: 'enum', enumKey: 'corso', mobile: 'subtitle' }, { key: 'descrizione', mobile: 'hidden' },
      { key: 'validitaMesi', tipo: 'number', align: 'end', mobile: 'note' }, { key: 'ore', tipo: 'number', align: 'end', mobile: 'hidden' },
    ],
    campi: [
      { key: 'idCmp_Mansione', tipo: 'select', lookup: 'mansioni', required: true }, { key: 'tipoCorso', tipo: 'select', enumKey: 'corso', enumValues: range(0, 10) },
      { key: 'descrizione', tipo: 'text', max: 100 }, { key: 'validitaMesi', tipo: 'number', min: 0, nota: 'cmp.pages.edit.validityNote' }, { key: 'ore', tipo: 'number', min: 0 },
    ],
    nuovo: () => ({ id: 0, tipoCorso: 0 }),
  },
  assegnazioni: {
    key: 'assegnazioni', api: 'Cmp_Sicurezza', server: 'DipendenteMansione', icona: 'people-outline',
    colonne: [
      { key: 'dipendente', mobile: 'title' }, { key: 'mansione', mobile: 'subtitle' },
      { key: 'dataInizio', tipo: 'date', width: '120px' }, { key: 'dataFine', tipo: 'date', mobile: 'note', width: '120px' },
    ],
    campi: [
      DIPENDENTE, { key: 'idCmp_Mansione', tipo: 'select', lookup: 'mansioni', required: true },
      { key: 'dataInizio', tipo: 'date', required: true }, { key: 'dataFine', tipo: 'date' }, NOTE,
    ],
    nuovo: () => ({ id: 0, dataInizio: oggi() }),
  },
  dpi: {
    key: 'dpi', api: 'Cmp_Sicurezza', server: 'Dpi', icona: 'hand-left-outline',
    colonne: [
      { key: 'descrizione', mobile: 'title' }, { key: 'categoria', tipo: 'enum', enumKey: 'dpiCategoria', mobile: 'subtitle' }, { key: 'norma', mobile: 'note' },
    ],
    campi: [
      { key: 'descrizione', tipo: 'text', max: 100, required: true }, { key: 'categoria', tipo: 'select', enumKey: 'dpiCategoria', enumValues: [1, 2, 3] },
      { key: 'norma', tipo: 'text', max: 50 }, NOTE, { key: 'attivo', tipo: 'toggle' },
    ],
    nuovo: () => ({ id: 0, categoria: 1, attivo: true }),
  },

  // ------------------------------------------------------------------ CMP-GDPR
  trattamenti: {
    key: 'trattamenti', api: 'Cmp_Gdpr', server: 'Trattamento', icona: 'folder-open-outline',
    colonne: [
      { key: 'denominazione', mobile: 'title' }, { key: 'baseGiuridica', tipo: 'enum', enumKey: 'baseGiuridica', mobile: 'subtitle' },
      { key: 'datiParticolari', tipo: 'bool', mobile: 'note' },
    ],
    campi: [
      { key: 'denominazione', tipo: 'text', max: 200, required: true }, { key: 'finalita', tipo: 'textarea', max: 2000 },
      { key: 'baseGiuridica', tipo: 'select', enumKey: 'baseGiuridica', enumValues: range(0, 5) },
      { key: 'categorieInteressati', tipo: 'textarea', max: 1000 }, { key: 'categorieDati', tipo: 'textarea', max: 1000 },
      { key: 'datiParticolari', tipo: 'toggle' }, { key: 'destinatari', tipo: 'textarea', max: 1000 }, { key: 'trasferimenti', tipo: 'textarea', max: 1000 },
      { key: 'terminiCancellazione', tipo: 'textarea', max: 500 }, { key: 'misureSicurezza', tipo: 'textarea', max: 2000 },
      { key: 'note', tipo: 'textarea', max: 1000 }, { key: 'attivo', tipo: 'toggle' },
    ],
    nuovo: () => ({ id: 0, baseGiuridica: 1, datiParticolari: false, attivo: true }),
  },
  responsabili: {
    key: 'responsabili', api: 'Cmp_Gdpr', server: 'Responsabile', icona: 'briefcase-outline',
    colonne: [
      { key: 'denominazione', mobile: 'title' }, { key: 'tipo', tipo: 'enum', enumKey: 'responsabile', mobile: 'subtitle' },
      { key: 'dataContratto', tipo: 'date', width: '120px', mobile: 'hidden' }, { key: 'dataScadenza', tipo: 'date', mobile: 'note', width: '120px' },
    ],
    campi: [
      { key: 'tipo', tipo: 'select', enumKey: 'responsabile', enumValues: range(0, 4) }, { key: 'denominazione', tipo: 'text', max: 200, required: true },
      { key: 'codiceFiscale', tipo: 'text', max: 20 }, { key: 'email', tipo: 'text', max: 100 }, { key: 'telefono', tipo: 'text', max: 30 },
      { key: 'trattamenti', tipo: 'textarea', max: 1000 }, { key: 'dataContratto', tipo: 'date' }, { key: 'dataScadenza', tipo: 'date' },
      { key: 'documento', tipo: 'text', max: 200, nota: 'cmp.pages.edit.referenceNote' }, { key: 'note', tipo: 'textarea', max: 1000 },
    ],
    nuovo: () => ({ id: 0, tipo: 0 }),
  },
  violazioni: {
    key: 'violazioni', api: 'Cmp_Gdpr', server: 'Violazione', icona: 'warning-outline',
    colonne: [
      { key: 'dataScoperta', tipo: 'datetime', width: '150px' }, { key: 'descrizione', mobile: 'title' },
      { key: 'rischio', tipo: 'enum', enumKey: 'rischioViolazione', mobile: 'subtitle' },
      { key: 'termineNotifica', tipo: 'datetime', width: '150px', mobile: 'hidden' }, { key: 'dataNotificaGarante', tipo: 'datetime', width: '150px', mobile: 'note' },
    ],
    campi: [
      { key: 'dataScoperta', tipo: 'datetime', required: true, nota: 'cmp.pages.edit.breachDiscoveryNote' }, { key: 'dataViolazione', tipo: 'datetime' },
      { key: 'descrizione', tipo: 'textarea', max: 2000, required: true }, { key: 'categorieDati', tipo: 'textarea', max: 1000 },
      { key: 'numeroInteressati', tipo: 'number', min: 0 }, { key: 'conseguenze', tipo: 'textarea', max: 2000 }, { key: 'misure', tipo: 'textarea', max: 2000 },
      { key: 'rischio', tipo: 'select', enumKey: 'rischioViolazione', enumValues: range(0, 2), nota: 'cmp.pages.edit.breachRiskNote' },
      { key: 'dataNotificaGarante', tipo: 'datetime' }, { key: 'motivoRitardo', tipo: 'textarea', max: 1000 },
      { key: 'dataComunicazioneInteressati', tipo: 'datetime' }, { key: 'note', tipo: 'textarea', max: 1000 },
    ],
    nuovo: () => ({ id: 0, rischio: 1, dataScoperta: adesso() }),
  },
  scadenze: {
    key: 'scadenze', api: 'Cmp_Gdpr', server: 'StudioScadenze', icona: 'calendar-outline', speciale: 'scadenze',
    colonne: [
      { key: 'dataScadenza', tipo: 'date', width: '120px', mobile: 'note' }, { key: 'descrizione', mobile: 'title' },
      { key: 'fonte', tipo: 'enum', enumKey: 'fonte', mobile: 'subtitle' }, { key: 'chiusa', tipo: 'bool', mobile: 'hidden' },
    ],
    campi: [],
  },
};

export const CMP_AREE: Record<CmpArea, string[]> = {
  '81': ['formazione', 'stato', 'visite', 'consegne', 'nomine', 'documenti', 'mansioni', 'corsiMansione', 'assegnazioni', 'dpi'],
  'gdpr': ['trattamenti', 'responsabili', 'violazioni'],
  'studio': ['trattamenti', 'responsabili', 'violazioni', 'scadenze'],
};

/** testo di una cella (date nella lingua attiva, enumerati tradotti) */
export function cmpValore(c: CmpColonna, item: CmpRiga, transloco: TranslocoService, locale: string): string {
  const v = item[c.key];
  switch (c.tipo) {
    case 'date': return v ? formatDate(v, 'shortDate', locale) : '';
    case 'datetime': return v ? formatDate(v, 'short', locale) : '';
    // fonte delle scadenze ("cmp.formazione"): senza il prefisso del modulo
    case 'enum': return v === null || v === undefined ? '' : transloco.translate('cmp.enums.' + c.enumKey + '.' + String(v).replace(/^cmp\./, ''));
    case 'bool': return transloco.translate(v ? 'cmp.common.yes' : 'cmp.common.no');
    case 'presaVisione':
      if (v) return transloco.translate('cmp.common.ackConfirmed', { data: formatDate(v, 'shortDate', locale) });
      return transloco.translate(item['presaVisioneRichiesta'] ? 'cmp.common.ackPending' : 'cmp.common.ackNone');
    default: return v === null || v === undefined ? '' : String(v);
  }
}

/** valore per l'ordinamento (le date come Date) */
export function cmpOrdine(c: CmpColonna, item: CmpRiga): any {
  const v = item[c.key];
  return (c.tipo === 'date' || c.tipo === 'datetime' || c.tipo === 'presaVisione') ? (v ? new Date(v) : null) : v;
}
