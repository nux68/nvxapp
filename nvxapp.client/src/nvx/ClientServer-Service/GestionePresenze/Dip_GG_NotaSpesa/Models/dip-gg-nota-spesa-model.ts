import { ModelResult } from "../../../ModelsBase/model-result";
import { Dip_GG_RichiestaModel, StatoRichiesta } from "../../Dip_GG_Richiesta/Models/dip-gg-richiesta-model";


export class Dip_GG_NotaSpesaInModel {
}

export class Dip_GG_NotaSpesaOutModel extends ModelResult {
}

// spesa (dettaglio) di una nota spese, con i dati della richiesta che la contiene
export class Dip_GG_NotaSpesaModel {
  id: number = 0;
  idDip_RapportoLavoro: number = 0;
  idAspNetUsers: string = '';
  idDip_GG_Richiesta: number | null = null;
  richiestaDescrizione: string = '';
  data: string = '';
  idPar_TipoSpesa: number = 0;
  importo: number = 0;
  descrizione: string | null = null;
  richiestaStato: StatoRichiesta = StatoRichiesta.Immessa;
  allegati: Dip_GG_NotaSpesa_AllegatoModel[] = [];
}

// allegato senza contenuto (si scarica con AllegatoGet)
export class Dip_GG_NotaSpesa_AllegatoModel {
  id: number = 0;
  nomeFile: string = '';
  contentType: string = '';
  dimensione: number = 0;
}

export class Dip_GG_NotaSpesa_Get4Richiesta_InModel {
  idDip_GG_Richiesta: number = 0;
}
export class Dip_GG_NotaSpesa_Get4Richiesta_OutModel extends ModelResult {
  dip_GG_Richiesta: Dip_GG_RichiestaModel | null = null;   // stato e revoca della nota
  dip_GG_NotaSpesa: Dip_GG_NotaSpesaModel[] = [];
}

// ricerca (CompanyPowerAdmin): periodo, dipendenti, tipo di spesa, stati
export class Dip_GG_NotaSpesa_Search_InModel {
  dal: string = '';
  al: string = '';
  usersId: string[] = [];
  idPar_TipoSpesa: number | null = null;
  stati: StatoRichiesta[] | null = null;
}
export class Dip_GG_NotaSpesa_Search_OutModel extends ModelResult {
  dip_GG_NotaSpesa: Dip_GG_NotaSpesaModel[] = [];
}

export class Dip_GG_NotaSpesa_Allegato_Get_InModel {
  id: number = 0;
}
export class Dip_GG_NotaSpesa_Allegato_Get_OutModel extends ModelResult {
  nomeFile: string = '';
  contentType: string = '';
  contenuto: string = '';  // base64
}

// limiti degli allegati (uguali a quelli del server, Dip_GG_NotaSpesaService)
export const NOTA_SPESA_ALLEGATO_MAX_BYTE = 5 * 1024 * 1024;
export const NOTA_SPESA_ALLEGATI_MAX_PER_SPESA = 10;
export const NOTA_SPESA_ALLEGATI_MAX_BYTE_RICHIESTA = 15 * 1024 * 1024;
export const NOTA_SPESA_ALLEGATO_TIPI = ['application/pdf', 'image/jpeg', 'image/png', 'image/webp', 'image/heic', 'image/heif'];
