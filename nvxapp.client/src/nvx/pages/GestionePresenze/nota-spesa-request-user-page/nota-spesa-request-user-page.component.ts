import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ModalController, NavController } from '@ionic/angular';
import { Observable, map, of } from 'rxjs';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DipGGRichiestaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/dip-gg-richiesta.service';
import { Dip_GG_Richiesta_Body_NotaSpesa, Dip_GG_Richiesta_Body_NotaSpesa_Allegato, Dip_GG_Richiesta_Body_NotaSpesa_Spesa, Dip_GG_Richiesta_NotaSpesaUpdate_InModel, Dip_GG_Richiesta_Send_InModel, StatoRichiesta, TipoRichiesta } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_Richiesta/Models/dip-gg-richiesta-model';
import { ParTipoSpesaService } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/par-tipo-spesa.service';
import { Par_TipoSpesaInModel, Par_TipoSpesaModel, TipoSpesaTipo } from '../../../ClientServer-Service/GestionePresenze/Par_TipoSpesa/Models/par-tipo-spesa-model';
import { Dip_GG_NotaSpesa_AllegatoModel, Dip_GG_NotaSpesa_Get4Richiesta_InModel, kmTesto, NOTA_SPESA_ALLEGATI_MAX_BYTE_RICHIESTA, NOTA_SPESA_ALLEGATI_MAX_PER_SPESA, NOTA_SPESA_ALLEGATO_MAX_BYTE, NOTA_SPESA_ALLEGATO_TIPI } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_NotaSpesa/Models/dip-gg-nota-spesa-model';
import { DipGGNotaSpesaService } from '../../../ClientServer-Service/GestionePresenze/Dip_GG_NotaSpesa/dip-gg-nota-spesa.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { TranslocoService } from '@jsverse/transloco';
import { DipRapportoAutovettureService } from '../../../ClientServer-Service/GestionePresenze/Dip_RapportoAutovetture/dip-rapporto-autovetture.service';
import { autovetturaTesto, Dip_RapportoAutovetture_GetAll4User_InModel, Dip_RapportoAutovettureModel } from '../../../ClientServer-Service/GestionePresenze/Dip_RapportoAutovetture/Models/dip-rapporto-autovetture-model';
import { RimborsoKmDialogComponent, RimborsoKmDialogModel } from '../../../shared/components/GestionePresenze/rimborso-km-dialog/rimborso-km-dialog.component';


/** allegato in compilazione: letto dal dispositivo (con contenuto) o gia' salvato (in modifica, con id) */
type AllegatoInCompilazione = Dip_GG_Richiesta_Body_NotaSpesa_Allegato & { dimensione: number };

/*
 Richiesta di rimborso spese (utente): una nota spese contiene piu' spese, ognuna con data, tipo,
 importo, descrizione e i documenti (scontrini, fatture, ricevute...). Si invia come richiesta
 di tipo NotaSpesa e segue il ciclo standard di approvazione.
 Form: descrizione della nota e "spese" (FormArray, una riga per spesa; gli allegati sono il valore
 del controllo "allegati" della riga).
 Rimborso km (tipo di spesa RimborsoKm): km e autovettura si indicano nel dialog dedicato, l'importo
 e' calcolato (km x rimborso per km dell'autovettura) e non si modifica; il server lo ricalcola.
*/
@Component({
  selector: 'app-nota-spesa-request-user-page',
  templateUrl: './nota-spesa-request-user-page.component.html',
  styleUrls: ['./nota-spesa-request-user-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class NotaSpesaRequestUserPageComponent extends BasePageConfirmCancelComponent<Dip_GG_Richiesta_Body_NotaSpesa> {

  /** nota spese in modifica (id della richiesta), null per una nuova */
  public idRichiesta: number | null = null;
  public readonly accept = NOTA_SPESA_ALLEGATO_TIPI.join(',');

  public tipiSpesa: Par_TipoSpesaModel[] = [];
  /** autovetture del proprio rapporto di lavoro, per i rimborsi km */
  public autovetture: Dip_RapportoAutovettureModel[] = [];
  public invio = false;

  constructor(protected override navCtrl: NavController,
              protected override userInterfaceService: UserInterfaceService,
              protected override fb: FormBuilder,
              private dipGGRichiestaService: DipGGRichiestaService,
              private dipGGNotaSpesaService: DipGGNotaSpesaService,
              private parTipoSpesaService: ParTipoSpesaService,
              private collectionDialogService: CollectionDialogService,
              private refresherService: RefresherService,
              private transloco: TranslocoService,
              private modalCtrl: ModalController,
              private dipRapportoAutovettureService: DipRapportoAutovettureService) {

    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string {
    return this.transloco.translate(this.idRichiesta ? 'attendance.pages.notaSpesaRequestUser.titleModifica' : 'attendance.pages.notaSpesaRequestUser.title');
  }

  get EditForm(): FormGroup {
    return this.fb.group({
      descrizione: ['', [Validators.maxLength(200)]],
      spese: this.fb.array([], [Validators.required]),
    });
  }

  public get speseArray(): FormArray {
    return this._editForm.get('spese') as FormArray;
  }

  public get spese(): FormGroup[] {
    return this.speseArray.controls as FormGroup[];
  }

  private nuovaSpesa(spesa: Partial<Dip_GG_Richiesta_Body_NotaSpesa_Spesa> = {}, allegati: AllegatoInCompilazione[] = []): FormGroup {
    const riga = this.fb.group({
      id: [spesa.id ?? null],
      data: [spesa.data ?? '', [Validators.required]],
      idPar_TipoSpesa: [spesa.idPar_TipoSpesa || null, [Validators.required]],
      importo: [spesa.importo ?? null, [Validators.required, Validators.min(0.01)]],
      descrizione: [spesa.descrizione ?? '', [Validators.maxLength(200)]],
      km: [spesa.km ?? null],
      idDip_RapportoAutovetture: [spesa.idDip_RapportoAutovetture ?? null],
      allegati: this.fb.control<AllegatoInCompilazione[]>(allegati),
    });
    // scelto un tipo rimborso km senza km: si chiedono subito km e autovettura
    riga.get('idPar_TipoSpesa')?.valueChanges.subscribe(() => {
      this.aggiornaTipo(riga);
      if (this.isRimborsoKm(riga) && !riga.get('km')?.value)
        this.apriRimborsoKm(riga);
    });
    return riga;
  }

  LoadData = (): Observable<Dip_GG_Richiesta_Body_NotaSpesa> => {
    this.speseArray.clear();
    this.idRichiesta = history.state?.idRichiesta ?? null;
    this.btnConfirm.text = this.transloco.translate(this.idRichiesta ? 'attendance.pages.notaSpesaRequestUser.salva' : 'common.buttons.send');
    this.parTipoSpesaService.GetAll(new GenericRequest<Par_TipoSpesaInModel>(Par_TipoSpesaInModel)).subscribe(res => {
      this.tipiSpesa = res.data?.par_TipoSpesa ?? [];
      this.spese.forEach(s => this.aggiornaTipo(s));
    });
    this.dipRapportoAutovettureService.GetAll4User(new GenericRequest<Dip_RapportoAutovetture_GetAll4User_InModel>(Dip_RapportoAutovetture_GetAll4User_InModel)).subscribe(res => {
      this.autovetture = res.data?.dip_RapportoAutovetture ?? [];
      this.spese.filter(s => this.isRimborsoKm(s)).forEach(s => this.ricalcolaImporto(s));
    });

    if (!this.idRichiesta) {
      this.aggiungiSpesa();
      return of(this.modello(''));
    }

    // modifica: spese e allegati gia' salvati (gli allegati restano sul server, qui solo nome e dimensione)
    const request = new GenericRequest<Dip_GG_NotaSpesa_Get4Richiesta_InModel>(Dip_GG_NotaSpesa_Get4Richiesta_InModel);
    request.data.idDip_GG_Richiesta = this.idRichiesta;
    return this.dipGGNotaSpesaService.Get4Richiesta(request).pipe(map(res => {
      const spese = res.data?.dip_GG_NotaSpesa ?? [];
      for (const s of spese) {
        this.speseArray.push(this.nuovaSpesa(
          { id: s.id, data: String(s.data).substring(0, 10), idPar_TipoSpesa: s.idPar_TipoSpesa, importo: s.importo, descrizione: s.descrizione ?? '', km: s.km, idDip_RapportoAutovetture: s.idDip_RapportoAutovetture },
          s.allegati.map(a => ({ id: a.id, nomeFile: a.nomeFile, contentType: a.contentType, contenuto: '', dimensione: a.dimensione }))));
      }
      if (spese.length === 0)
        this.aggiungiSpesa();
      this.spese.forEach(s => this.aggiornaTipo(s));
      return this.modello(spese[0]?.richiestaDescrizione ?? '');
    }));
  };

  /** modello con le spese gia' nel form (il patch del form non aggiunge righe al FormArray) */
  private modello(descrizione: string): Dip_GG_Richiesta_Body_NotaSpesa {
    const model = new Dip_GG_Richiesta_Body_NotaSpesa();
    model.descrizione = descrizione;
    model.spese = this.speseArray.getRawValue();
    return model;
  }

  SaveData = (editModel: Dip_GG_Richiesta_Body_NotaSpesa): Observable<boolean> => {
    // getRawValue: l'importo dei rimborsi km e' un controllo disabilitato (calcolato)
    const righe: (Dip_GG_Richiesta_Body_NotaSpesa_Spesa & { allegati: AllegatoInCompilazione[] })[] = this.speseArray.getRawValue();

    const body = new Dip_GG_Richiesta_Body_NotaSpesa();
    body.descrizione = (editModel.descrizione ?? '').trim();
    body.spese = righe.map(s => {
      const spesa = new Dip_GG_Richiesta_Body_NotaSpesa_Spesa();
      spesa.id = s.id;
      spesa.data = s.data;
      spesa.idPar_TipoSpesa = s.idPar_TipoSpesa;
      spesa.importo = Math.round(Number(s.importo) * 100) / 100;
      spesa.descrizione = (s.descrizione ?? '').trim();
      spesa.km = s.km != null && s.km !== ('' as any) ? Number(s.km) : null;
      spesa.idDip_RapportoAutovetture = s.idDip_RapportoAutovetture ?? null;
      // gli allegati gia' salvati si mandano solo con l'id (restano sul server)
      spesa.allegati = s.allegati.map(a => a.id
        ? { id: a.id, nomeFile: a.nomeFile, contentType: a.contentType, contenuto: '' }
        : { nomeFile: a.nomeFile, contentType: a.contentType, contenuto: a.contenuto });
      return spesa;
    });

    this.invio = true;
    this.btnConfirm.disabled = true;
    const fine = (success: boolean) => {
      this.invio = false;
      this.btnConfirm.disabled = !this._editForm.valid;
      if (success)
        this.refresherService.Dip_GG_Richiesta_triggerRefresh();
      return success;
    };

    // modifica di una nota gia' inviata
    if (this.idRichiesta) {
      const update = new GenericRequest<Dip_GG_Richiesta_NotaSpesaUpdate_InModel>(Dip_GG_Richiesta_NotaSpesaUpdate_InModel);
      update.data.idDip_GG_Richiesta = this.idRichiesta;
      update.data.dati = JSON.stringify(body);
      return this.dipGGRichiestaService.NotaSpesaUpdate(update).pipe(map(res => fine(res.success)));
    }

    const date = righe.map(s => s.data).sort();
    const request = new GenericRequest<Dip_GG_Richiesta_Send_InModel>(Dip_GG_Richiesta_Send_InModel);
    request.data.dip_GG_Richiesta.id = 0;
    request.data.dip_GG_Richiesta.idDip_RapportoLavoro = 0;
    request.data.dip_GG_Richiesta.richiestaStato = StatoRichiesta.Immessa;
    request.data.dip_GG_Richiesta.richiestaTipo = TipoRichiesta.NotaSpesa;
    // il server ricalcola il periodo dalle spese
    request.data.dip_GG_Richiesta.data = this.ddmmyyyy(date[0]);
    request.data.dip_GG_Richiesta.dataA = this.ddmmyyyy(date[date.length - 1]);
    request.data.dip_GG_Richiesta.dati = JSON.stringify(body);
    return this.dipGGRichiestaService.Send(request).pipe(map(res => fine(res.success)));
  };

  // modifica: si torna da dove si e' venuti; nuova nota: all'elenco delle proprie note spese
  override AfterSave(saved: boolean): void {
    if (!saved)
      return;
    if (this.idRichiesta)
      this.navCtrl.back();
    else
      this.navCtrl.navigateRoot('/notaspeseuser');
  }

  /** allegato gia' salvato: si apre dal server */
  public apriAllegato(allegato: AllegatoInCompilazione): void {
    if (!allegato.id)
      return;
    const a = new Dip_GG_NotaSpesa_AllegatoModel();
    a.id = allegato.id;
    a.nomeFile = allegato.nomeFile;
    a.contentType = allegato.contentType;
    this.dipGGNotaSpesaService.ApriAllegato(a);
  }

  public aggiungiSpesa(): void {
    // la nuova spesa riparte dalla data dell'ultima (trasferte di piu' giorni), altrimenti oggi
    const ultima = this.spese[this.spese.length - 1];
    this.speseArray.push(this.nuovaSpesa({ data: ultima ? ultima.get('data')?.value : this.oggi() }));
  }

  // ------------------------------------------------------------------ rimborso km

  public isRimborsoKm(spesa: FormGroup): boolean {
    const id = spesa.get('idPar_TipoSpesa')?.value;
    return this.tipiSpesa.find(t => t.id === id)?.tipo === TipoSpesaTipo.RimborsoKm;
  }

  /**
   * Adegua la riga al tipo di spesa: per il rimborso km l'importo e' calcolato (non modificabile)
   * e km e autovettura sono obbligatori; per gli altri tipi km e autovettura non servono.
   * Non fa nulla finche' non sono arrivati i tipi di spesa (non si sa ancora quali sono rimborsi km).
   */
  private aggiornaTipo(spesa: FormGroup): void {
    if (this.tipiSpesa.length === 0)
      return;
    const importo = spesa.get('importo')!;
    const km = spesa.get('km')!;
    const autovettura = spesa.get('idDip_RapportoAutovetture')!;
    if (this.isRimborsoKm(spesa)) {
      importo.disable({ emitEvent: false });
      km.setValidators([Validators.required, Validators.min(0.01)]);
      autovettura.setValidators([Validators.required]);
      this.ricalcolaImporto(spesa);
    } else {
      importo.enable({ emitEvent: false });
      km.clearValidators();
      autovettura.clearValidators();
      km.setValue(null, { emitEvent: false });
      autovettura.setValue(null, { emitEvent: false });
    }
    km.updateValueAndValidity({ emitEvent: false });
    autovettura.updateValueAndValidity({ emitEvent: false });
    // aggiorna la validita' del form (abilitazione del pulsante di conferma)
    this._editForm.updateValueAndValidity();
  }

  /** importo = km x rimborso per km dell'autovettura (finche' le autovetture non sono arrivate resta quello salvato) */
  private ricalcolaImporto(spesa: FormGroup): void {
    const auto = this.autovetture.find(a => a.id === spesa.get('idDip_RapportoAutovetture')?.value);
    if (!auto)
      return;
    const km = Number(spesa.get('km')?.value) || 0;
    spesa.get('importo')?.setValue(RimborsoKmDialogComponent.calcolaImporto(km, auto.rimborsoKM), { emitEvent: false });
  }

  /** dialog dedicato: autovettura e km della spesa */
  public async apriRimborsoKm(spesa: FormGroup): Promise<void> {
    const rimborsoKm = new RimborsoKmDialogModel();
    rimborsoKm.idDip_RapportoAutovetture = spesa.get('idDip_RapportoAutovetture')?.value ?? null;
    rimborsoKm.km = spesa.get('km')?.value ?? null;

    const modal = await this.modalCtrl.create({
      component: RimborsoKmDialogComponent,
      componentProps: {
        rimborsoKm: rimborsoKm,
        autovetture: this.autovetture
      },
    });
    await modal.present();

    const { data, role } = await modal.onWillDismiss<RimborsoKmDialogModel | null>();
    spesa.get('km')?.markAsTouched();
    if (role === 'confirm' && data) {
      spesa.patchValue({ km: Number(data.km) || null, idDip_RapportoAutovetture: data.idDip_RapportoAutovetture });
      this.ricalcolaImporto(spesa);
    }
  }

  /** "120 km · Fiat Panda (AB123CD)", vuoto se km e autovettura non sono ancora indicati */
  public rimborsoKmTesto(spesa: FormGroup): string {
    const km = spesa.get('km')?.value;
    const auto = this.autovetture.find(a => a.id === spesa.get('idDip_RapportoAutovetture')?.value);
    if (!km || !auto)
      return '';
    return this.transloco.translate('attendance.pages.notaSpesaRequestUser.rimborsoKmRiepilogo',
      { km: kmTesto(km), autovettura: autovetturaTesto(auto.descrizione, auto.targa) });
  }

  public togliSpesa(index: number): void {
    this.speseArray.removeAt(index);
  }

  public allegati(spesa: FormGroup): AllegatoInCompilazione[] {
    return spesa.get('allegati')?.value ?? [];
  }

  /** errore di un campo della riga, dopo che e' stato toccato */
  public errore(spesa: FormGroup, campo: string): boolean {
    const ctrl = spesa.get(campo);
    return !!ctrl && ctrl.invalid && (ctrl.dirty || ctrl.touched);
  }

  public get totale(): number {
    return this.spese.reduce((t, s) => t + (Number(s.get('importo')?.value) || 0), 0);
  }

  private get byteAllegati(): number {
    return this.spese.reduce((t, s) => t + this.byteDi(this.allegati(s)), 0);
  }

  private byteDi(allegati: AllegatoInCompilazione[]): number {
    return allegati.reduce((a, x) => a + x.dimensione, 0);
  }

  public dimensione(byte: number): string {
    return byte >= 1024 * 1024 ? (byte / (1024 * 1024)).toFixed(1).replace('.', ',') + ' MB' : Math.max(1, Math.round(byte / 1024)) + ' KB';
  }

  /** file scelti dal dispositivo (o scattati con la fotocamera sul telefono) */
  public async allega(spesa: FormGroup, event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    input.value = '';
    const allegati = [...this.allegati(spesa)];
    // allegati delle altre spese della nota (il limite e' sull'intera richiesta)
    const byteAltreSpese = this.byteAllegati - this.byteDi(allegati);
    for (const file of files) {
      const tipo = (file.type || '').toLowerCase();
      if (!NOTA_SPESA_ALLEGATO_TIPI.includes(tipo)) {
        await this.avviso(this.transloco.translate('attendance.pages.notaSpesaRequestUser.erroreTipoFile', { nome: file.name }));
        continue;
      }
      if (file.size > NOTA_SPESA_ALLEGATO_MAX_BYTE) {
        await this.avviso(this.transloco.translate('attendance.pages.notaSpesaRequestUser.erroreDimensioneFile', { nome: file.name, mb: NOTA_SPESA_ALLEGATO_MAX_BYTE / (1024 * 1024) }));
        continue;
      }
      if (allegati.length >= NOTA_SPESA_ALLEGATI_MAX_PER_SPESA) {
        await this.avviso(this.transloco.translate('attendance.pages.notaSpesaRequestUser.erroreMaxAllegati', { max: NOTA_SPESA_ALLEGATI_MAX_PER_SPESA }));
        break;
      }
      if (byteAltreSpese + this.byteDi(allegati) + file.size > NOTA_SPESA_ALLEGATI_MAX_BYTE_RICHIESTA) {
        await this.avviso(this.transloco.translate('attendance.pages.notaSpesaRequestUser.erroreDimensioneTotale', { mb: NOTA_SPESA_ALLEGATI_MAX_BYTE_RICHIESTA / (1024 * 1024) }));
        break;
      }
      allegati.push({ nomeFile: file.name, contentType: tipo, contenuto: await this.base64(file), dimensione: file.size });
    }
    spesa.get('allegati')?.setValue(allegati);
  }

  public togliAllegato(spesa: FormGroup, index: number): void {
    spesa.get('allegati')?.setValue(this.allegati(spesa).filter((_, i) => i !== index));
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

  private ddmmyyyy(iso: string): string {
    const [aaaa, mm, gg] = iso.split('-');
    return `${gg}/${mm}/${aaaa}`;
  }
}
