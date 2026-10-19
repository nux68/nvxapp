import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NavController } from '@ionic/angular/lazy';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { Dip_GG_Richiesta_Body_NotaSpesa, Dip_GG_Richiesta_Body_NotaSpesa_Allegato, Dip_GG_Richiesta_Body_NotaSpesa_Spesa, Dip_GG_Richiesta_NotaSpesaUpdate_InModel, Dip_GG_Richiesta_Send_InModel, StatoRichiesta, TipoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { ParTipoSpesaService } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/par-tipo-spesa.service';
import { Par_TipoSpesaInModel, Par_TipoSpesaModel } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/Models/par-tipo-spesa-model';
import { Dip_GG_NotaSpesa_AllegatoModel, Dip_GG_NotaSpesa_Get4Richiesta_InModel, NOTA_SPESA_ALLEGATI_MAX_BYTE_RICHIESTA, NOTA_SPESA_ALLEGATI_MAX_PER_SPESA, NOTA_SPESA_ALLEGATO_MAX_BYTE, NOTA_SPESA_ALLEGATO_TIPI } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_NotaSpesa/Models/dip-gg-nota-spesa-model';
import { DipGGNotaSpesaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_NotaSpesa/dip-gg-nota-spesa.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';


/** spesa in compilazione, con gli allegati letti dal dispositivo (in modifica anche quelli gia' salvati, con id) */
class SpesaInCompilazione {
  id: number | null = null;
  data: string = '';
  idPar_TipoSpesa: number | null = null;
  importo: number | null = null;
  descrizione: string = '';
  allegati: (Dip_GG_Richiesta_Body_NotaSpesa_Allegato & { dimensione: number })[] = [];
}

/*
 Richiesta di rimborso spese (utente): una nota spese contiene piu' spese, ognuna con data, tipo,
 importo, descrizione e i documenti (scontrini, fatture, ricevute...). Si invia come richiesta
 di tipo NotaSpesa e segue il ciclo standard di approvazione.
*/
@Component({
  selector: 'app-nota-spesa-request-user-page',
  templateUrl: './nota-spesa-request-user-page.component.html',
  styleUrls: ['./nota-spesa-request-user-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class NotaSpesaRequestUserPageComponent {

  /** nota spese in modifica (id della richiesta), null per una nuova */
  public idRichiesta: number | null = null;
  public get title(): string {
    return this.idRichiesta ? 'Modifica nota spese' : 'Richiesta rimborso spese';
  }
  public readonly accept = NOTA_SPESA_ALLEGATO_TIPI.join(',');

  public descrizione = '';
  public spese: SpesaInCompilazione[] = [];
  public tipiSpesa: Par_TipoSpesaModel[] = [];
  public invio = false;

  public buttonbar: ButtonItem[] = [];

  constructor(private navCtrl: NavController,
              private dipGGRichiestaService: DipGGRichiestaService,
              private dipGGNotaSpesaService: DipGGNotaSpesaService,
              private parTipoSpesaService: ParTipoSpesaService,
              private collectionDialogService: CollectionDialogService,
              private refresherService: RefresherService,
              userInterfaceService: UserInterfaceService) {

    const btnInvia = userInterfaceService.Btn_Conferma;
    btnInvia.text = 'Invia';
    btnInvia.event = () => this.invia();
    const btnAnnulla = userInterfaceService.Btn_Annulla;
    btnAnnulla.event = () => this.navCtrl.back();
    this.buttonbar = [btnInvia, btnAnnulla];
  }

  ionViewWillEnter() {
    this.descrizione = '';
    this.spese = [];
    this.idRichiesta = history.state?.idRichiesta ?? null;
    this.buttonbar[0].text = this.idRichiesta ? 'Salva' : 'Invia';
    if (this.idRichiesta)
      this.caricaNota(this.idRichiesta);
    else
      this.aggiungiSpesa();
    this.parTipoSpesaService.GetAll(new GenericRequest<Par_TipoSpesaInModel>(Par_TipoSpesaInModel)).subscribe(res => {
      this.tipiSpesa = res.data?.par_TipoSpesa ?? [];
    });
  }

  /** modifica: spese e allegati gia' salvati (gli allegati restano sul server, qui solo nome e dimensione) */
  private caricaNota(idRichiesta: number): void {
    const request = new GenericRequest<Dip_GG_NotaSpesa_Get4Richiesta_InModel>(Dip_GG_NotaSpesa_Get4Richiesta_InModel);
    request.data.idDip_GG_Richiesta = idRichiesta;
    this.dipGGNotaSpesaService.Get4Richiesta(request).subscribe(res => {
      const spese = res.data?.dip_GG_NotaSpesa ?? [];
      this.descrizione = spese[0]?.richiestaDescrizione ?? '';
      this.spese = spese.map(s => {
        const spesa = new SpesaInCompilazione();
        spesa.id = s.id;
        spesa.data = String(s.data).substring(0, 10);
        spesa.idPar_TipoSpesa = s.idPar_TipoSpesa;
        spesa.importo = s.importo;
        spesa.descrizione = s.descrizione ?? '';
        spesa.allegati = s.allegati.map(a => ({ id: a.id, nomeFile: a.nomeFile, contentType: a.contentType, contenuto: '', dimensione: a.dimensione }));
        return spesa;
      });
      if (this.spese.length === 0)
        this.aggiungiSpesa();
    });
  }

  /** allegato gia' salvato: si apre dal server */
  public apriAllegato(allegato: Dip_GG_Richiesta_Body_NotaSpesa_Allegato & { dimensione: number }): void {
    if (!allegato.id)
      return;
    const a = new Dip_GG_NotaSpesa_AllegatoModel();
    a.id = allegato.id;
    a.nomeFile = allegato.nomeFile;
    a.contentType = allegato.contentType;
    this.dipGGNotaSpesaService.ApriAllegato(a);
  }

  public aggiungiSpesa(): void {
    const spesa = new SpesaInCompilazione();
    // la nuova spesa riparte dalla data dell'ultima (trasferte di piu' giorni), altrimenti oggi
    spesa.data = this.spese.length > 0 ? this.spese[this.spese.length - 1].data : this.oggi();
    this.spese.push(spesa);
  }

  public togliSpesa(index: number): void {
    this.spese.splice(index, 1);
  }

  public get totale(): number {
    return this.spese.reduce((t, s) => t + (Number(s.importo) || 0), 0);
  }

  private get byteAllegati(): number {
    return this.spese.reduce((t, s) => t + s.allegati.reduce((a, x) => a + x.dimensione, 0), 0);
  }

  public dimensione(byte: number): string {
    return byte >= 1024 * 1024 ? (byte / (1024 * 1024)).toFixed(1).replace('.', ',') + ' MB' : Math.max(1, Math.round(byte / 1024)) + ' KB';
  }

  /** file scelti dal dispositivo (o scattati con la fotocamera sul telefono) */
  public async allega(spesa: SpesaInCompilazione, event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    input.value = '';
    for (const file of files) {
      const tipo = (file.type || '').toLowerCase();
      if (!NOTA_SPESA_ALLEGATO_TIPI.includes(tipo)) {
        await this.avviso(`"${file.name}": sono ammessi solo PDF e immagini.`);
        continue;
      }
      if (file.size > NOTA_SPESA_ALLEGATO_MAX_BYTE) {
        await this.avviso(`"${file.name}" supera i ${NOTA_SPESA_ALLEGATO_MAX_BYTE / (1024 * 1024)} MB.`);
        continue;
      }
      if (spesa.allegati.length >= NOTA_SPESA_ALLEGATI_MAX_PER_SPESA) {
        await this.avviso(`Al massimo ${NOTA_SPESA_ALLEGATI_MAX_PER_SPESA} allegati per spesa.`);
        break;
      }
      if (this.byteAllegati + file.size > NOTA_SPESA_ALLEGATI_MAX_BYTE_RICHIESTA) {
        await this.avviso(`Gli allegati della nota spese superano i ${NOTA_SPESA_ALLEGATI_MAX_BYTE_RICHIESTA / (1024 * 1024)} MB complessivi.`);
        break;
      }
      spesa.allegati.push({ nomeFile: file.name, contentType: tipo, contenuto: await this.base64(file), dimensione: file.size });
    }
  }

  public togliAllegato(spesa: SpesaInCompilazione, index: number): void {
    spesa.allegati.splice(index, 1);
  }

  private base64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(String(reader.result).split(',')[1] ?? '');
      reader.onerror = () => reject(reader.error);
      reader.readAsDataURL(file);
    });
  }

  private oggi(): string {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  private avviso(messaggio: string): Promise<any> {
    return this.collectionDialogService.ConfirmCancelDialog(messaggio);
  }

  /** controlli prima dell'invio (il server li ripete) */
  private errore(): string | null {
    if (this.spese.length === 0)
      return 'Aggiungi almeno una spesa.';
    for (const [i, s] of this.spese.entries()) {
      const n = `Spesa ${i + 1}`;
      if (!s.data) return `${n}: indica la data.`;
      if (!s.idPar_TipoSpesa) return `${n}: scegli il tipo di spesa.`;
      if (!(Number(s.importo) > 0)) return `${n}: indica un importo maggiore di zero.`;
    }
    return null;
  }

  public async invia(): Promise<void> {
    const errore = this.errore();
    if (errore) {
      await this.avviso(errore);
      return;
    }

    const body = new Dip_GG_Richiesta_Body_NotaSpesa();
    body.descrizione = this.descrizione.trim();
    body.spese = this.spese.map(s => {
      const spesa = new Dip_GG_Richiesta_Body_NotaSpesa_Spesa();
      spesa.id = s.id;
      spesa.data = s.data;
      spesa.idPar_TipoSpesa = s.idPar_TipoSpesa!;
      spesa.importo = Math.round(Number(s.importo) * 100) / 100;
      spesa.descrizione = s.descrizione.trim();
      // gli allegati gia' salvati si mandano solo con l'id (restano sul server)
      spesa.allegati = s.allegati.map(a => a.id
        ? { id: a.id, nomeFile: a.nomeFile, contentType: a.contentType, contenuto: '' }
        : { nomeFile: a.nomeFile, contentType: a.contentType, contenuto: a.contenuto });
      return spesa;
    });

    // modifica di una nota gia' inviata
    if (this.idRichiesta) {
      const update = new GenericRequest<Dip_GG_Richiesta_NotaSpesaUpdate_InModel>(Dip_GG_Richiesta_NotaSpesaUpdate_InModel);
      update.data.idDip_GG_Richiesta = this.idRichiesta;
      update.data.dati = JSON.stringify(body);
      this.invio = true;
      this.dipGGRichiestaService.NotaSpesaUpdate(update).subscribe({
        next: res => {
          this.invio = false;
          if (res.success) {
            this.refresherService.Dip_GG_Richiesta_triggerRefresh();
            this.navCtrl.back();
          }
        },
        error: () => { this.invio = false; }
      });
      return;
    }

    const date = this.spese.map(s => s.data).sort();
    const request = new GenericRequest<Dip_GG_Richiesta_Send_InModel>(Dip_GG_Richiesta_Send_InModel);
    request.data.dip_GG_Richiesta.id = 0;
    request.data.dip_GG_Richiesta.idDip_RapportoLavoro = 0;
    request.data.dip_GG_Richiesta.richiestaStato = StatoRichiesta.Immessa;
    request.data.dip_GG_Richiesta.richiestaTipo = TipoRichiesta.NotaSpesa;
    // il server ricalcola il periodo dalle spese
    request.data.dip_GG_Richiesta.data = this.ddmmyyyy(date[0]);
    request.data.dip_GG_Richiesta.dataA = this.ddmmyyyy(date[date.length - 1]);
    request.data.dip_GG_Richiesta.dati = JSON.stringify(body);

    this.invio = true;
    this.dipGGRichiestaService.Send(request).subscribe({
      next: res => {
        this.invio = false;
        if (res.success) {
          this.refresherService.Dip_GG_Richiesta_triggerRefresh();
          this.navCtrl.navigateRoot('/notaspeseuser');
        }
      },
      error: () => { this.invio = false; }
    });
  }

  private ddmmyyyy(iso: string): string {
    const [aaaa, mm, gg] = iso.split('-');
    return `${gg}/${mm}/${aaaa}`;
  }
}
