import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ValidatorFn, AbstractControl, ValidationErrors } from '@angular/forms';
import { NavController, ModalController } from '@ionic/angular';
import { Observable, map, catchError, of, firstValueFrom } from 'rxjs';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { RefresherService } from '../../../Utility/GestionePresenze/refresher.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { BasePageConfirmCancelComponent } from '../../_BASE/base-page-confirm-cancel/base-page-confirm-cancel.component';
import { ParExportCauService } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau/par-export-cau.service';
import { Par_ExportCau_Causali_Get_InModel, Par_ExportCau_CausaliModel } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau_Causali/Models/par-export-cau-causali-model';
import { Par_ExportCau_Get_InModel, Par_ExportCau_Put_InModel, Par_ExportCauModel } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau/Models/par-export-cau-model';
import { ParExportCauDocsService } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau_Docs/par-export-cau-docs.service';
import {
  EXPORT_DOC_ESTENSIONI, EXPORT_DOC_MAX_BYTE, EXPORT_DOC_MAX_PER_MODELLO, Par_ExportCau_DocsModel,
  Par_ExportCau_Docs_Delete_InModel, Par_ExportCau_Docs_Get_InModel, Par_ExportCau_Docs_GetAll_InModel, Par_ExportCau_Docs_Put_InModel
} from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau_Docs/Models/par-export-cau-docs-model';
import { FabMenuService, FabMenuItem } from '../../../Utility/infrastructure/fab-menu.service';
import { EditParExportCauCausaliComponentComponent } from '../../../shared/components/GestionePresenze/edit-par-export-cau-causali-component/edit-par-export-cau-causali-component.component';
import { ParExportCauCausaliService } from '../../../ClientServer-Service/GestionePresenze/Par_ExportCau_Causali/par-export-cau-causali.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';
import { ParTable, SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { TimeSheetExportService } from '../../../ClientServer-Service/GestionePresenze/TimeSheet_ExportService/time-sheet-export.service';
import { TimeSheet_Export_Anteprima_InModel, TimeSheet_Export_Anteprima_OutModel } from '../../../ClientServer-Service/GestionePresenze/TimeSheet_ExportService/Models/time-sheet-export-model';
import { Dip_AnagraficaModel } from '../../../ClientServer-Service/GestionePresenze/Dip_Anagrafica/Models/dip-anagrafica-model';
import { RoleCode } from '../../../ClientServer-Service/Infrastructure/Account/Models/user-roles-model';
import { TranslocoService } from '@jsverse/transloco';

@Component({
  selector: 'app-export-cau-edit-page',
  templateUrl: './export-cau-edit-page.component.html',
  styleUrls: ['./export-cau-edit-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class ExportCauEditPageComponent extends BasePageConfirmCancelComponent<Par_ExportCauModel> implements OnInit {

  public readonly SEGMENT_sez1 = 'sez1';
  public readonly SEGMENT_sez2 = 'sez2';
  public readonly SEGMENT_sez3 = 'sez3';

  public searchText: string = '';
  public par_ExportCau_Causali: Par_ExportCau_CausaliModel[];
  public currSection: string = this.SEGMENT_sez1;
  public btnEdit: ButtonItem;
  public btnDelete: ButtonItem;
  public TMP_counter: number = 0;

  // ── Documentazione per l'AI (salvata subito sul server, indipendente dal salvataggio del modello) ──
  public documenti: Par_ExportCau_DocsModel[] = [];
  public readonly accept = EXPORT_DOC_ESTENSIONI.join(',');
  public caricamentoDoc = false;
  public erroreDoc: string | null = null;
  /** testo estratto mostrato nella pagina (quello che riceve l'AI) */
  public testoAperto: { nomeFile: string; testo: string } | null = null;

  // ── Anteprima del file: un dipendente, un mese ──
  /** mese "YYYY-MM" (default: il mese scorso, di solito quello da mandare alle paghe) */
  public anteprimaMese: string = this.meseScorso();
  public anteprimaDip: string | null = null;
  public anteprimaInCorso = false;
  public anteprima: TimeSheet_Export_Anteprima_OutModel | null = null;
  public erroreAnteprima: string | null = null;
  public mostraRicetta = false;

  constructor(
    protected override navCtrl: NavController,
    protected override userInterfaceService: UserInterfaceService,
    protected override fb: FormBuilder,
    private refresherService: RefresherService,
    private parExportCauService: ParExportCauService,
    private parExportCauCausaliService: ParExportCauCausaliService,
    private modalCtrl: ModalController,
    private collectionDialogService: CollectionDialogService,
    public fabMenuService: FabMenuService,
    private transloco: TranslocoService,
    private parExportCauDocsService: ParExportCauDocsService,
    private cdr: ChangeDetectorRef,
    private timeSheetExportService: TimeSheetExportService,
    private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
  ) {
    super(navCtrl, userInterfaceService, fb);

    this.btnEdit = this.userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
    this.currSection = this.SEGMENT_sez1;
    this.btnEdit = this.userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;

    this.btnDelete = userInterfaceService.Btn_Cancella;
    this.btnDelete.event = this.handleButtonDeleteClick;
  }

  override ionViewWillEnter() {
    super.ionViewWillEnter();
    this.setfabMenuService();
  }

  setfabMenuService() {
    this.fabMenuService.fabMenuItem = [];

    if (this.currSection == this.SEGMENT_sez1) {
      // non faccio nulla
    }
    else if (this.currSection == this.SEGMENT_sez2) {
        this.fabMenuService.fabMenuItem = [
          new FabMenuItem('xxx', 'add-circle-outline', () => {

            this.Load_Par_ExportCau_Causali(0).subscribe(res => {
              this.EdiProfiloOrarioDettDialog_Open(res);
            })
          }),
      ];

    }
  }



  get Title(): string { return this.transloco.translate('attendance.pages.exportCauEdit.title'); }

  get EditForm(): FormGroup {
    return this.fb.group({
      codice: [null, [Validators.required, Validators.maxLength(10)]],
      descrizione: [null, [Validators.required, Validators.maxLength(50)]],
      noteAI: [null, [Validators.maxLength(1000)]],
    });
  }

  LoadData = (): Observable<Par_ExportCauModel | null> => {
    const state = history.state;

    if (state) {

      let request = new GenericRequest<Par_ExportCau_Get_InModel>(Par_ExportCau_Get_InModel);
      request.data.id = state.id;

      return this.parExportCauService.Par_ExportCauGet(request).pipe(
        map((res) => {
          this.par_ExportCau_Causali = res.data.par_ExportCau_Causali;
          this.caricaDocumenti(res.data.par_ExportCau?.id ?? 0);
          return res.data.par_ExportCau;
        }
        ),
        catchError((error) => {
          console.error('Errore durante il caricamento dei dati:', error);
          return of(null);
        })
      );
    }
    else {
      return null;
    }

  };

  SaveData = (editModel: Par_ExportCauModel): Observable<boolean> => {

    let request: GenericRequest<Par_ExportCau_Put_InModel> = new GenericRequest<Par_ExportCau_Put_InModel>(Par_ExportCau_Put_InModel);
    request.data.par_ExportCau = editModel;
    request.data.par_ExportCau_Causali = this.par_ExportCau_Causali;

    return this.parExportCauService.Par_ExportCauPut(request).pipe(
      map(() => {
        this.refresherService.SharedParameterGestionePresenze_triggerRefresh(ParTable.Par_ExportCau);
        return true;
      }),
      catchError((error) => {
        console.error('Errore durante il salvataggio:', error);
        return [false];
      })
    );
  };

  // ── Documentazione ──────────────────────────────────────────────────────

  /** id del modello salvato (0 = nuovo: i documenti si caricano dopo il primo salvataggio) */
  public get idModello(): number {
    return this._editModel?.id ?? 0;
  }

  private caricaDocumenti(idPar_ExportCau: number): void {
    this.documenti = [];
    this.testoAperto = null;
    if (!idPar_ExportCau) return;
    const request = new GenericRequest<Par_ExportCau_Docs_GetAll_InModel>(Par_ExportCau_Docs_GetAll_InModel);
    request.data.idPar_ExportCau = idPar_ExportCau;
    this.parExportCauDocsService.Par_ExportCau_Docs_GetAll(request).subscribe(res => {
      this.documenti = res?.data?.par_ExportCau_Docs ?? [];
      this.cdr.detectChanges();
    });
  }

  /** caricamento dei file scelti: controllo di tipo, dimensione e numero, poi uno alla volta al server */
  async carica(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    input.value = '';
    this.erroreDoc = null;
    for (const file of files) {
      const ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
      if (!EXPORT_DOC_ESTENSIONI.includes(ext)) {
        this.erroreDoc = this.transloco.translate('attendance.pages.exportCauEdit.docTipoNonAmmesso', { nome: file.name, tipi: EXPORT_DOC_ESTENSIONI.join(', ') });
        continue;
      }
      if (file.size > EXPORT_DOC_MAX_BYTE) {
        this.erroreDoc = this.transloco.translate('attendance.pages.exportCauEdit.docTroppoGrande', { nome: file.name, mb: EXPORT_DOC_MAX_BYTE / (1024 * 1024) });
        continue;
      }
      if (this.documenti.length >= EXPORT_DOC_MAX_PER_MODELLO) {
        this.erroreDoc = this.transloco.translate('attendance.pages.exportCauEdit.docTroppi', { max: EXPORT_DOC_MAX_PER_MODELLO });
        break;
      }

      const request = new GenericRequest<Par_ExportCau_Docs_Put_InModel>(Par_ExportCau_Docs_Put_InModel);
      request.data.idPar_ExportCau = this.idModello;
      request.data.nomeFile = file.name;
      request.data.contentType = file.type || null;
      request.data.contenuto = await this.base64(file);

      this.caricamentoDoc = true;
      this.cdr.detectChanges();
      try {
        const res = await firstValueFrom(this.parExportCauDocsService.Par_ExportCau_Docs_Put(request));
        if (res?.success && res.data?.par_ExportCau_Doc)
          this.documenti = [...this.documenti, res.data.par_ExportCau_Doc];
        else
          this.erroreDoc = res?.messages?.map(m => m.text).join(' ') || this.transloco.translate('attendance.pages.exportCauEdit.docErroreCaricamento', { nome: file.name });
      } catch (error) {
        console.error('Errore durante il caricamento del documento:', error);
        this.erroreDoc = this.transloco.translate('attendance.pages.exportCauEdit.docErroreCaricamento', { nome: file.name });
      } finally {
        this.caricamentoDoc = false;
        this.cdr.detectChanges();
      }
    }
  }

  private base64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(String(reader.result).split(',')[1] ?? '');
      reader.onerror = () => reject(reader.error);
      reader.readAsDataURL(file);
    });
  }

  /** formati che il browser sa mostrare: si aprono in una scheda; gli altri (docx, odt, xlsx) si vedono come testo letto */
  private mostrabileNelBrowser(doc: Par_ExportCau_DocsModel): boolean {
    return /\.(pdf|txt|csv|xml|json|md)$/i.test(doc.nomeFile);
  }

  /** vedi: il file nel browser se possibile, altrimenti il testo estratto */
  async vedi(doc: Par_ExportCau_DocsModel): Promise<void> {
    if (!this.mostrabileNelBrowser(doc)) {
      await this.vediTesto(doc);
      return;
    }
    // la scheda si apre subito (nel gesto dell'utente), il contenuto arriva dopo
    const scheda = window.open('', '_blank');
    const file = await this.leggi(doc);
    if (!file) { scheda?.close(); return; }
    // i file di testo si mostrano come testo, qualunque sia il tipo dichiarato
    const tipo = /\.pdf$/i.test(doc.nomeFile) ? 'application/pdf' : 'text/plain;charset=utf-8';
    const url = URL.createObjectURL(new Blob([this.bytes(file.contenuto)], { type: tipo }));
    if (scheda) scheda.location.href = url;
    setTimeout(() => URL.revokeObjectURL(url), 60000);
  }

  /** testo letto dal file: e' quello che riceve l'AI */
  async vediTesto(doc: Par_ExportCau_DocsModel): Promise<void> {
    const file = await this.leggi(doc);
    if (!file) return;
    this.testoAperto = { nomeFile: doc.nomeFile, testo: file.testo ?? '' };
    this.cdr.detectChanges();
  }

  async scarica(doc: Par_ExportCau_DocsModel): Promise<void> {
    const file = await this.leggi(doc);
    if (!file) return;
    const url = URL.createObjectURL(new Blob([this.bytes(file.contenuto)], { type: doc.contentType || 'application/octet-stream' }));
    const a = document.createElement('a');
    a.href = url;
    a.download = doc.nomeFile;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    setTimeout(() => URL.revokeObjectURL(url), 60000);
  }

  async eliminaDoc(doc: Par_ExportCau_DocsModel): Promise<void> {
    const conferma = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.exportCauEdit.docConfermaEliminazione', { nome: doc.nomeFile }));
    if (!conferma) return;
    const request = new GenericRequest<Par_ExportCau_Docs_Delete_InModel>(Par_ExportCau_Docs_Delete_InModel);
    request.data.id = doc.id;
    const res = await firstValueFrom(this.parExportCauDocsService.Par_ExportCau_Docs_Delete(request));
    if (res?.success) {
      this.documenti = this.documenti.filter(x => x.id !== doc.id);
      if (this.testoAperto?.nomeFile === doc.nomeFile) this.testoAperto = null;
      this.cdr.detectChanges();
    }
  }

  private async leggi(doc: Par_ExportCau_DocsModel) {
    const request = new GenericRequest<Par_ExportCau_Docs_Get_InModel>(Par_ExportCau_Docs_Get_InModel);
    request.data.id = doc.id;
    try {
      const res = await firstValueFrom(this.parExportCauDocsService.Par_ExportCau_Docs_Get(request));
      return res?.success && res.data ? res.data : null;
    } catch (error) {
      console.error('Errore durante la lettura del documento:', error);
      return null;
    }
  }

  private bytes(base64: string) {
    return Uint8Array.from(atob(base64), c => c.charCodeAt(0));
  }

  /** dipendenti su cui provare il formato */
  public get dipendentiAnteprima(): Dip_AnagraficaModel[] {
    return this.sharedParameterGestionePresenzeService.Dip_Anagrafica_OnRoles([RoleCode.User]) ?? [];
  }

  private meseScorso(): string {
    const d = new Date();
    d.setDate(1);
    d.setMonth(d.getMonth() - 1);
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
  }

  /**
   * Anteprima del file sul dipendente e sul mese scelti: con documentazione la ricetta dell'AI (salvata, o nuova se
   * documenti o note sono cambiati o con "rigenera") applicata ai dati reali; senza documentazione il JSON dei dati.
   */
  async provaFormato(rigenera: boolean): Promise<void> {
    if (!this.idModello || !this.anteprimaDip || !this.anteprimaMese) return;
    const [anno, mese] = this.anteprimaMese.split('-').map(x => +x);
    const ultimo = new Date(anno, mese, 0).getDate();

    const request = new GenericRequest<TimeSheet_Export_Anteprima_InModel>(TimeSheet_Export_Anteprima_InModel);
    request.data.idPar_ExportCau = this.idModello;
    request.data.idAspNetUsers = this.anteprimaDip;
    request.data.dal = `${this.anteprimaMese}-01T00:00:00`;
    request.data.al = `${this.anteprimaMese}-${String(ultimo).padStart(2, '0')}T00:00:00`;
    request.data.rigenera = rigenera;

    this.anteprimaInCorso = true;
    this.erroreAnteprima = null;
    this.anteprima = null;
    this.cdr.detectChanges();
    try {
      const res = await firstValueFrom(this.timeSheetExportService.Export_Anteprima(request));
      if (res?.success && res.data)
        this.anteprima = res.data;
      else
        this.erroreAnteprima = res?.messages?.map(m => m.text).join(' ') || this.transloco.translate('attendance.pages.exportCauEdit.anteprimaErrore');
    } catch (error) {
      console.error("Errore durante l'anteprima:", error);
      this.erroreAnteprima = this.transloco.translate('attendance.pages.exportCauEdit.anteprimaErrore');
    } finally {
      this.anteprimaInCorso = false;
      this.cdr.detectChanges();
    }
  }

  /** ricetta in JSON leggibile */
  public get ricettaLeggibile(): string {
    try {
      return this.anteprima?.ricetta ? JSON.stringify(JSON.parse(this.anteprima.ricetta), null, 2) : '';
    } catch {
      return this.anteprima?.ricetta ?? '';
    }
  }

  dimensioneTesto(byte: number): string {
    return byte >= 1024 * 1024 ? `${(byte / (1024 * 1024)).toFixed(1)} MB` : `${Math.max(1, Math.round(byte / 1024))} KB`;
  }

  segmentChanged(event: any) {
    console.log('Segment cambiato:', event.detail.value);
    this.currSection = event.detail.value;
    this.setfabMenuService();
  }

  public get_par_ExportCau_Causali(): Par_ExportCau_CausaliModel[] {
    if (!this.par_ExportCau_Causali) {
      return [];
    }

    //let retVal = this.par_ExportCau_Causali.sort((a, b) => a.numCoppia - b.numCoppia);

    let retVal = this.par_ExportCau_Causali;
    

    return retVal;
  }

  handleButtonEditClick = (item: Par_ExportCau_CausaliModel) => {

    this.Load_Par_ExportCau_Causali(item.id).subscribe(res => {
      this.EdiProfiloOrarioDettDialog_Open(res);
    })

  }

  handleButtonDeleteClick = async (item: Par_ExportCau_CausaliModel) => {

    const result = await this.collectionDialogService.ConfirmCancelDialog(this.transloco.translate('attendance.pages.exportCauEdit.confermaCancellazione'));
    if (result) {

      var idx = this.par_ExportCau_Causali.findIndex(p => p.id === item.id);
      if (idx > -1) {
        this.par_ExportCau_Causali.splice(idx, 1);
      }
    }

  }

  /* Legge i dati locali o inizilizza con il server */
  Load_Par_ExportCau_Causali = (id: number): Observable<Par_ExportCau_CausaliModel | null> => {
    const state = history.state;

    if (id==0) {  // se nuovo, faccio inizializazre dal server

      let request: GenericRequest<Par_ExportCau_Causali_Get_InModel> = new GenericRequest<Par_ExportCau_Causali_Get_InModel>(Par_ExportCau_Causali_Get_InModel);
      request.data.id = id;

      return this.parExportCauCausaliService.Par_ExportCau_Causali_Get(request).pipe(
        map((res) => {
          
          return res.data.par_ExportCau_Causali;
        }
        ),
        catchError((error) => {
          console.error('Errore durante il caricamento dei dati:', error);
          return of(null);
        })
      );
    }
    else {
      var idx = this.par_ExportCau_Causali.findIndex(p => p.id === id);
      if (idx > -1) {
        return of(this.par_ExportCau_Causali[idx]);
      }
      else {
        return of(null);
      }
    }

  };



  async EdiProfiloOrarioDettDialog_Open(par_ExportCau_Causali: Par_ExportCau_CausaliModel) {

      const modal = await this.modalCtrl.create({
        component: EditParExportCauCausaliComponentComponent,
        componentProps: {
          par_ExportCau_Causali: par_ExportCau_Causali
        },
      });

      await modal.present();

      const { data, role } = await modal.onWillDismiss<Par_ExportCau_CausaliModel | null>();

      if (role === 'confirm' && data) {

        const index = this.par_ExportCau_Causali.findIndex(p => p.id === data.id);

        if (index > -1) {
          this.par_ExportCau_Causali[index] = data;
        } else {
          this.par_ExportCau_Causali.push(data);
        }

      }

  }




}


const matchData: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  //const password = control.get('pw')?.value;
  //const confirmPassword = control.get('confirmPassword')?.value;

  //return password === confirmPassword ? null : { notMatching: true };

  return null;
};
