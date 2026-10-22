import { I18N_FOLDER_ATTENDANCE_TRACKING } from './i18n-attendance-tracking';

/*
 Cartelle dei file di traduzione, in src/assets/i18n/<cartella>/<lingua>.json.
 Infrastructure ha la sua; ogni ramo applicativo aggiunge la propria (es. i18n-attendance-tracking.ts),
 con chiavi sotto un proprio prefisso, cosi' i file dei vari rami non si toccano nei merge.
 I file vengono uniti nell'ordine dell'elenco: a parita' di chiave vale quello del ramo applicativo.
*/
export const I18N_FOLDERS: string[] = [
  'infrastructure',
  I18N_FOLDER_ATTENDANCE_TRACKING,
].filter((x): x is string => !!x);
