import { ModelResult } from "../../../ModelsBase/model-result";

/** documento del modello di export (senza contenuto) */
export class Par_ExportCau_DocsModel {
  id: number = 0;
  idPar_ExportCau: number = 0;
  nomeFile: string = '';
  contentType: string = '';
  dimensione: number = 0;
  descrizione: string | null = null;
  ordine: number = 0;
  /** lunghezza del testo estratto: e' quello che riceve l'AI */
  lunghezzaTesto: number = 0;
  creationDate: string | null = null;
}

/** come sul server: 5 MB per file, 10 documenti per modello */
export const EXPORT_DOC_MAX_BYTE = 5 * 1024 * 1024;
export const EXPORT_DOC_MAX_PER_MODELLO = 10;
export const EXPORT_DOC_ESTENSIONI = ['.pdf', '.docx', '.odt', '.xlsx', '.txt', '.csv', '.xml', '.json', '.md'];

export class Par_ExportCau_Docs_GetAll_InModel {
  idPar_ExportCau: number = 0;
}
export class Par_ExportCau_Docs_GetAll_OutModel extends ModelResult {
  par_ExportCau_Docs: Par_ExportCau_DocsModel[] = [];
}

export class Par_ExportCau_Docs_Get_InModel {
  id: number = 0;
}
export class Par_ExportCau_Docs_Get_OutModel extends ModelResult {
  par_ExportCau_Doc: Par_ExportCau_DocsModel | null = null;
  /** contenuto in base64 */
  contenuto: string = '';
  /** testo estratto (quello che riceve l'AI) */
  testo: string | null = null;
}

export class Par_ExportCau_Docs_Put_InModel {
  idPar_ExportCau: number = 0;
  nomeFile: string = '';
  contentType: string | null = null;
  /** contenuto in base64 */
  contenuto: string = '';
  descrizione: string | null = null;
}
export class Par_ExportCau_Docs_Put_OutModel extends ModelResult {
  par_ExportCau_Doc: Par_ExportCau_DocsModel | null = null;
}

export class Par_ExportCau_Docs_Delete_InModel {
  id: number = 0;
}
export class Par_ExportCau_Docs_Delete_OutModel extends ModelResult { }
