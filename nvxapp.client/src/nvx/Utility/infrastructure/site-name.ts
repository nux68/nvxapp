import { SITE_NAME_INFRASTRUCTURE } from './site-name-infrastructure';
import { SITE_NAME_ATTENDANCE_TRACKING } from './site-name-attendance-tracking';
import { SITE_NAME_CRM } from './site-name-crm';
import { SITE_NAME_DOCUMENT_MANAGEMENT } from './site-name-document-management';
import { SITE_NAME_ALL_FUNCTION } from './site-name-all-function';

/*
 Nome del sito (titolo della pagina, intestazione del menu, login, showcase, ...): fare sempre
 riferimento a SITE_NAME, mai al nome scritto nel codice. Vale quello del ramo applicativo se lo
 ridefinisce, altrimenti quello di Infrastructure.
*/
// L'applicativo completo (ramo AllFunction) ha il suo nome; altrimenti quello del ramo applicativo, poi Infrastructure.
export const SITE_NAME: string = SITE_NAME_ALL_FUNCTION ?? SITE_NAME_ATTENDANCE_TRACKING ?? SITE_NAME_CRM ?? SITE_NAME_DOCUMENT_MANAGEMENT ?? SITE_NAME_INFRASTRUCTURE;
