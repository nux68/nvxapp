import { SITE_NAME_INFRASTRUCTURE } from './site-name-infrastructure';
import { SITE_NAME_ATTENDANCE_TRACKING } from './site-name-attendance-tracking';

/*
 Nome del sito (titolo della pagina, intestazione del menu, login, showcase, ...): fare sempre
 riferimento a SITE_NAME, mai al nome scritto nel codice. Vale quello del ramo applicativo se lo
 ridefinisce, altrimenti quello di Infrastructure.
*/
export const SITE_NAME: string = SITE_NAME_ATTENDANCE_TRACKING ?? SITE_NAME_INFRASTRUCTURE;
