import { NavController } from '@ionic/angular';

/*
 Navigazione dei widget di Compliance (ANALISI_COMPLIANCE_WIDGET.md): le pagine elenco (cmp-list-page) aprono la sezione
 indicata nello stato di navigazione { sezione }, come le scadenze e le notifiche.
*/

/** tipo di scadenza (fonte senza "cmp.") -> pagina, sezione e icona */
export const CMP_WIDGET_FONTI: Record<string, { url: string, sezione: string, icona: string }> = {
  formazione:   { url: '/cmp81',   sezione: 'formazione',   icona: 'school-outline' },
  visita:       { url: '/cmp81',   sezione: 'visite',       icona: 'medkit-outline' },
  nomina:       { url: '/cmp81',   sezione: 'nomine',       icona: 'ribbon-outline' },
  documento:    { url: '/cmp81',   sezione: 'documenti',    icona: 'document-text-outline' },
  responsabile: { url: '/cmpgdpr', sezione: 'responsabili', icona: 'briefcase-outline' },
  violazione:   { url: '/cmpgdpr', sezione: 'violazioni',   icona: 'warning-outline' },
};

export function cmpApriSezione(navCtrl: NavController, url: string, sezione?: string): void {
  navCtrl.navigateForward(url, sezione ? { state: { sezione } } : undefined);
}
