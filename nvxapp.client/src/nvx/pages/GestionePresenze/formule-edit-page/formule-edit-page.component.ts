import { Component, ChangeDetectionStrategy, ElementRef, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParFormuleService } from '../../../ClientServer-Service/GestionePresenze/Par_Formule/par-formule.service';
import {
  FORMULA_FASE_TESTO, FormulaErrore, FormulaFase, FormulaLogTipo, FormulaMetodo, FormulaProvaRisultato,
  Par_Formule_CtrlSintassiInModel, Par_Formule_DuplicaInModel, Par_Formule_GeneraAIInModel, Par_Formule_GeneraAIOutModel,
  Par_Formule_LogInModel, Par_Formule_LogRigaModel, Par_Formule_MetodiInModel, Par_Formule_ProvaInModel,
  Par_Formule_RapportiProvaInModel, Par_Formule_RapportoProvaModel, Par_FormuleGetInModel, Par_FormuleModel, Par_FormulePutInModel
} from '../../../ClientServer-Service/GestionePresenze/Par_Formule/Models/par-formule-model';

/*
 Modifica di una formula personalizzata del calcolo giornata (ANALISI_ATTENDANCETRACKING_FORMULE.md).
 La formula e' JavaScript eseguito SUL SERVER (Jint) durante il calcolo: qui si scrive, si controlla,
 si prova su un dipendente e un giorno senza salvare nulla, e si leggono i log dei calcoli.
*/
@Component({
  selector: 'app-formule-edit-page',
  templateUrl: './formule-edit-page.component.html',
  styleUrls: ['./formule-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class FormuleEditPageComponent extends BasePageConfirmCancelComponent<Par_FormuleModel> {

  @ViewChild('editor') editor?: ElementRef<HTMLTextAreaElement>;

  public readonly SEG_DATI = 'dati';
  public readonly SEG_FORMULA = 'formula';
  public readonly SEG_PROVA = 'prova';
  public readonly SEG_LOG = 'log';
  public segmento = this.SEG_FORMULA;

  public readonly fasi = [FormulaFase.Finale, FormulaFase.Giorno];
  public readonly faseTesto = FORMULA_FASE_TESTO;
  public readonly logTipo = FormulaLogTipo;

  // editor
  public posizione = '';
  public errori: FormulaErrore[] = [];
  public sintassiOk = false;

  // lista metodi
  public metodi: FormulaMetodo[] = [];
  public categorie: string[] = [];
  public filtroMetodi = '';
  public categoriaMetodi = '';
  public soloEvidenza = false;
  public mostraMetodi = true;
  private daAutocomplete = false;

  // duplica
  public duplicaCodice = '';
  public duplicaTitolo = '';

  // AI
  public richiestaAI = '';
  public risultatoAI: Par_Formule_GeneraAIOutModel | null = null;
  public attesaAI = false;

  // prova
  public rapporti: Par_Formule_RapportoProvaModel[] = [];
  public provaRapporto = 0;
  public provaGiorno = this.isoGiorno(new Date());
  public provaFase = FormulaFase.Finale;
  public provaRisultato: FormulaProvaRisultato | null = null;
  public provaErrori: FormulaErrore[] = [];

  // log
  public logDal = this.isoGiorno(new Date(new Date().getFullYear(), new Date().getMonth(), 1));
  public logAl = this.isoGiorno(new Date());
  public logRighe: Par_Formule_LogRigaModel[] | null = null;

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private parFormuleService: ParFormuleService,
    private transloco: TranslocoService
  ) {
    super(navCtrl, userInterfaceService, fb);
  }

  get Title(): string { return this.transloco.translate('attendance.pages.formuleEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required, Validators.maxLength(20)]],
      titolo: [null, [Validators.required, Validators.maxLength(60)]],
      descrizione: [null],
      testo: [null, [Validators.required, Validators.maxLength(20000)]],
    });
  }

  override ionViewWillEnter() {
    super.ionViewWillEnter();
    const request = new GenericRequest<Par_Formule_MetodiInModel>(Par_Formule_MetodiInModel);
    this.parFormuleService.Metodi(request).subscribe(res => {
      this.metodi = res.data?.metodi ?? [];
      this.categorie = [...new Set(this.metodi.map(m => m.categoria))];
    });
  }

  LoadData = (): Observable<Par_FormuleModel | null> => {
    const request = new GenericRequest<Par_FormuleGetInModel>(Par_FormuleGetInModel);
    request.data.id = history.state?.id ?? 0;
    return this.parFormuleService.Par_FormuleGet(request).pipe(
      map(res => res.data?.par_Formule ?? null),
      catchError(error => { console.error('Errore durante la chiamata API:', error); return of(null); })
    );
  };

  SaveData = (editModel: Par_FormuleModel): Observable<boolean> => {
    const request = new GenericRequest<Par_FormulePutInModel>(Par_FormulePutInModel);
    request.data.par_Formule = editModel;
    return this.parFormuleService.Par_FormulePut(request).pipe(
      map(res => {
        this.errori = res.data?.errori ?? [];
        if (this.errori.length > 0) this.segmento = this.SEG_FORMULA;
        return res.success && this.errori.length === 0;
      }),
      catchError(error => { console.error('Errore durante la chiamata API:', error); return of(false); })
    );
  };

  // con errori (formula non valida, codice duplicato) si resta sulla pagina
  override AfterSave(saved: boolean): void {
    if (saved) this.navCtrl.back();
  }

  get testo(): string { return this._editForm?.get('testo')?.value ?? ''; }
  set testo(valore: string) {
    this._editForm.get('testo')?.setValue(valore);
    this._editForm.get('testo')?.markAsDirty();
  }

  // ── editor ────────────────────────────────────────────────────────────────────

  onEditorKeyUp(event: KeyboardEvent) {
    this.aggiornaPosizione();
    this.sintassiOk = false;
    // "x." digitato: si apre la lista dei metodi (come nel legacy)
    if (event.key === '.') {
      const ta = this.editor?.nativeElement;
      if (!ta) return;
      const prima = ta.value.substring(0, ta.selectionStart);
      if (/(^|[\s;{}()=!&|+\-*/,])x\.$/.test(prima)) {
        this.daAutocomplete = true;
        this.mostraMetodi = true;
        this.filtroMetodi = '';
      }
    }
  }

  aggiornaPosizione() {
    const ta = this.editor?.nativeElement;
    if (!ta) return;
    const righe = ta.value.substring(0, ta.selectionStart).split('\n');
    this.posizione = this.transloco.translate('attendance.pages.formuleEdit.posizione', { riga: righe.length, colonna: righe[righe.length - 1].length + 1 });
  }

  inserisciMetodo(m: FormulaMetodo) {
    const ta = this.editor?.nativeElement;
    const testo = this.testo;
    const inizio = ta ? ta.selectionStart : testo.length;
    const fine = ta ? ta.selectionEnd : testo.length;
    const giaX = this.daAutocomplete || testo.substring(Math.max(0, inizio - 2), inizio) === 'x.';
    const daInserire = (giaX ? '' : 'x.') + m.testo;
    this.testo = testo.substring(0, inizio) + daInserire + testo.substring(fine);
    this.daAutocomplete = false;
    setTimeout(() => {
      if (!ta) return;
      ta.focus();
      // cursore dentro le parentesi, se il metodo ha parametri
      const par = daInserire.indexOf('(');
      const pos = par >= 0 && daInserire[par + 1] !== ')' ? inizio + par + 1 : inizio + daInserire.length;
      ta.setSelectionRange(pos, par >= 0 && daInserire[par + 1] !== ')' ? inizio + daInserire.lastIndexOf(')') : pos);
      this.aggiornaPosizione();
    });
  }

  get metodiFiltrati(): FormulaMetodo[] {
    const f = this.filtroMetodi.trim().toLowerCase();
    return this.metodi.filter(m =>
      m.tipo !== 'Oggetto' &&
      (!this.soloEvidenza || m.inEvidenza) &&
      (!this.categoriaMetodi || m.categoria === this.categoriaMetodi) &&
      (!f || m.testo.toLowerCase().includes(f) || m.descrizione.toLowerCase().includes(f)));
  }

  vaiAllaRiga(e: FormulaErrore) {
    this.segmento = this.SEG_FORMULA;
    setTimeout(() => {
      const ta = this.editor?.nativeElement;
      if (!ta) return;
      const righe = ta.value.split('\n');
      let pos = 0;
      for (let i = 0; i < Math.min(e.riga - 1, righe.length); i++) pos += righe[i].length + 1;
      pos += Math.max(0, e.colonna - 1);
      ta.focus();
      ta.setSelectionRange(pos, pos);
      this.aggiornaPosizione();
    });
  }

  ctrlSintassi() {
    const request = new GenericRequest<Par_Formule_CtrlSintassiInModel>(Par_Formule_CtrlSintassiInModel);
    request.data.testo = this.testo;
    this.parFormuleService.CtrlSintassi(request).subscribe(res => {
      this.errori = res.data?.errori ?? [];
      this.sintassiOk = res.success && this.errori.length === 0;
    });
  }

  // ── duplica ───────────────────────────────────────────────────────────────────

  duplica() {
    if (!this._editModel?.id || !this.duplicaCodice.trim()) return;
    const request = new GenericRequest<Par_Formule_DuplicaInModel>(Par_Formule_DuplicaInModel);
    request.data.id = this._editModel.id;
    request.data.codice = this.duplicaCodice.trim();
    request.data.titolo = this.duplicaTitolo.trim();
    this.parFormuleService.Par_FormuleDuplica(request).subscribe(res => {
      if (res.success && res.data?.par_Formule?.id) {
        this.navCtrl.navigateRoot('/formulelist').then(() =>
          this.navCtrl.navigateForward('/formuleedit', { state: { id: res.data.par_Formule.id } }));
      }
    });
  }

  // ── AI ────────────────────────────────────────────────────────────────────────

  generaAI() {
    if (!this.richiestaAI.trim()) return;
    const request = new GenericRequest<Par_Formule_GeneraAIInModel>(Par_Formule_GeneraAIInModel);
    request.data.richiesta = this.richiestaAI;
    request.data.testoAttuale = this.testo;
    this.attesaAI = true;
    this.risultatoAI = null;
    this.parFormuleService.GeneraAI(request).subscribe({
      next: res => { this.risultatoAI = res.success ? res.data : null; this.attesaAI = false; },
      error: () => { this.attesaAI = false; }
    });
  }

  sostituisciAI() {
    if (this.risultatoAI) this.testo = this.risultatoAI.testo;
  }

  accodaAI() {
    if (this.risultatoAI) this.testo = (this.testo ? this.testo + '\n' : '') + this.risultatoAI.testo;
  }

  copiaAI() {
    if (this.risultatoAI) navigator.clipboard?.writeText(this.risultatoAI.testo);
  }

  // ── prova ─────────────────────────────────────────────────────────────────────

  segmentoCambiato(event: any) {
    this.segmento = event.detail.value;
    if (this.segmento === this.SEG_PROVA && this.rapporti.length === 0) {
      const request = new GenericRequest<Par_Formule_RapportiProvaInModel>(Par_Formule_RapportiProvaInModel);
      this.parFormuleService.RapportiProva(request).subscribe(res => {
        this.rapporti = res.data?.rapporti ?? [];
        if (!this.provaRapporto && this.rapporti.length > 0) this.provaRapporto = this.rapporti[0].idDip_RapportoLavoro;
      });
    }
    if (this.segmento === this.SEG_LOG && this.logRighe === null && this._editModel?.id)
      this.caricaLog();
  }

  eseguiProva() {
    if (!this.provaRapporto || !this.provaGiorno) return;
    const request = new GenericRequest<Par_Formule_ProvaInModel>(Par_Formule_ProvaInModel);
    request.data.testo = this.testo;
    request.data.idDip_RapportoLavoro = this.provaRapporto;
    request.data.giorno = this.provaGiorno;
    request.data.fase = this.provaFase;
    this.provaRisultato = null;
    this.parFormuleService.Prova(request).subscribe(res => {
      this.provaErrori = res.data?.errori ?? [];
      this.provaRisultato = res.success && this.provaErrori.length === 0 ? res.data.risultato : null;
    });
  }

  minuti(min: number): string {
    return `${String(Math.floor(min / 60)).padStart(2, '0')}:${String(min % 60).padStart(2, '0')}`;
  }

  valoreCausale(c: { minuti: number; importo: number | null }): string {
    return c.importo != null ? `€ ${c.importo.toFixed(2)}` : this.minuti(c.minuti);
  }

  /** causali con il valore prima e dopo la formula, per il confronto */
  get confrontoProva(): { codice: string; prima: string; dopo: string; cambiata: boolean }[] {
    if (!this.provaRisultato) return [];
    const codici = [...new Set([...this.provaRisultato.causaliPrima, ...this.provaRisultato.causaliDopo].map(c => c.codice))].sort();
    return codici.map(codice => {
      const p = this.provaRisultato!.causaliPrima.find(c => c.codice === codice);
      const d = this.provaRisultato!.causaliDopo.find(c => c.codice === codice);
      const prima = p ? this.valoreCausale(p) : '-';
      const dopo = d ? this.valoreCausale(d) : '-';
      return { codice, prima, dopo, cambiata: prima !== dopo };
    });
  }

  // ── log ───────────────────────────────────────────────────────────────────────

  caricaLog() {
    const request = new GenericRequest<Par_Formule_LogInModel>(Par_Formule_LogInModel);
    request.data.idPar_Formule = this._editModel?.id || null;
    request.data.dal = this.logDal;
    request.data.al = this.logAl;
    this.parFormuleService.Log(request).subscribe(res => {
      this.logRighe = res.data?.log ?? [];
    });
  }

  coloreLog(tipo: FormulaLogTipo): string {
    return tipo === FormulaLogTipo.Errore ? 'danger' : tipo === FormulaLogTipo.Warning ? 'warning' : tipo === FormulaLogTipo.Debug ? 'medium' : 'success';
  }

  private isoGiorno(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }
}
