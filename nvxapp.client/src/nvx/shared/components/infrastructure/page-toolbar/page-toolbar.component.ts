import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../../Utility/infrastructure/user-navigation.service';
import { LongJobNotifierService, LongJobProgressUpdate, LongJobCategory } from '../../../../Utility/infrastructure/long-job-notifier.service';
import { Observable } from 'rxjs';
import { HttpClient, HttpEventType } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { NavController, Platform, ToastController } from '@ionic/angular';
import { apriDownload } from '../../../../Utility/infrastructure/scarica-file';
import { AuthService } from '../../../../Utility/infrastructure/auth.service';
import { nomeUtente } from '../../../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { TranslocoService } from '@jsverse/transloco';
import { MessageType } from '../../../../ClientServer-Service/ModelsBase/message';
import { HoverPopupData } from '../hover-popup/hover-popup.component';
import { MenuLayoutService } from '../../../../Utility/infrastructure/menu-layout.service';

@Component({
  selector: 'app-page-toolbar',
  templateUrl: './page-toolbar.component.html',
  styleUrls: ['./page-toolbar.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class PageToolbarComponent {


  @Input() showFilter: boolean;
  @Input() showBreadcrumbs: boolean;
  @Input() showActiveJobs: boolean = true;
  @Input() headerColor: string = ""; // barra neutra anche su mobile (stile "chat AI")

  @Output() ev_Filter: EventEmitter<string> = new EventEmitter();
  @Input() title: string;
  /** utente collegato a destra del titolo (false nei dialog: azienda e utente vanno nel corpo del messaggio) */
  @Input() showUser: boolean = true;
  /** pulsante del menu laterale (false nei dialog) */
  @Input() showMenuButton: boolean = true;
  public queryText: string;

  private readonly ui = inject(UserInterfaceService);

  // percorso dell'impersonificazione: c'e' solo con piu' utenti nella cronologia, e su mobile non si mostra
  public get mostraBreadcrumbs(): boolean {
    return !!this.showBreadcrumbs && this.userNavigationService.userCronology.length > 1 && !this.ui.isMobileSignal();
  }

  // seconda barra solo se ha qualcosa da mostrare (cerca o percorso): niente barra vuota sotto il titolo
  public get mostraSecondaBarra(): boolean {
    return !!this.showFilter || this.mostraBreadcrumbs;
  }

  public activeJobs$: Observable<LongJobProgressUpdate[]>;
  public LongJobCategory = LongJobCategory;
  /** job con il download in corso */
  private readonly scaricando = new Set<string>();

  /*
   Codici dei collegamenti temporanei ai file dei job (Account/DownloadCode), per nome del file, comuni a tutte le barre.
   Si chiedono appena il job finisce: al tocco il download parte subito, dentro il gesto dell'utente. Chiedendo il codice
   al tocco il download arrivava dopo un'attesa e Chrome sul telefono (app installata) a volte lo bloccava senza avvisi.
   Validi 15 minuti sul server: qui si considerano scaduti un paio di minuti prima.
  */
  private static readonly codiciDownload = new Map<string, { code: string, scade: number }>();
  private static readonly codiciInArrivo = new Set<string>();
  private static readonly validitaCodiceMs = 13 * 60_000;

  /*
   Telefono: il file pronto si scarica subito in memoria e il tocco apre il menu Condividi di Android/iOS (lettore PDF,
   Drive, "Salva in File", email...). Nell'app installata i download diretti erano inaffidabili; la condivisione no.
   Serve un tocco (il menu si apre solo dal gesto), per questo il file deve essere gia' arrivato. I tipi che il
   browser non sa condividere (es. Excel su Chrome) restano al download con il collegamento temporaneo.
  */
  private static readonly fileCondivisibili = new Map<string, File>();
  private static readonly fileInArrivo = new Set<string>();
  /** byte del file ricevuti e totali (se il server li dice), durante la ricezione in memoria */
  private static readonly ricezione = new Map<string, { ricevuti: number, totale: number | null }>();

  constructor(public userNavigationService: UserNavigationService,
    public longJobNotifierService: LongJobNotifierService,
              private authService: AuthService,
              private navCtrl: NavController,
              private platform: Platform,
              private http: HttpClient,
              private transloco: TranslocoService,
              private toastCtrl: ToastController,
              public menuLayoutService: MenuLayoutService)
  {
    this.activeJobs$ = this.longJobNotifierService.activeJobs$;
    // file pronto: si chiede subito il codice del collegamento, cosi' il tocco scarica senza attese
    this.longJobNotifierService.activeJobs$.pipe(takeUntilDestroyed(inject(DestroyRef))).subscribe(jobs => {
      for (const job of jobs)
        if (job.isFinished && job.category === LongJobCategory.FileGeneration && job.downloadUrl && this.statoJob(job) !== 'errore') {
          this.preparaCodice(PageToolbarComponent.nomeFileJob(job));
          if (this.condivisioneDisponibile)
            this.preparaFile(job);
        }
    });
  }


  // c'e' un login attivo (stesso criterio della guardia della dashboard)
  public get isLoggedIn(): boolean {
    return this.authService.Token != null;
  }

  // username dell'utente del login (il primo della cronologia)
  public get loggedUserName(): string | null {
    return this.userNavigationService.userCronology?.[0]?.userData?.userName ?? null;
  }

  // utente impersonato (l'ultimo della cronologia), solo se diverso da quello del login:
  // Cognome e Nome dell'anagrafica se disponibili (utenti di tipo User), altrimenti lo username
  public get impersonatedUserName(): string | null {
    const cronologia = this.userNavigationService.userCronology;
    if (!cronologia || cronologia.length < 2)
      return null;
    return nomeUtente(cronologia[cronologia.length - 1]?.userData);
  }

  // username dell'utente impersonato, per il suggerimento quando la toolbar mostra Cognome e Nome
  public get impersonatedLogin(): string | null {
    const cronologia = this.userNavigationService.userCronology;
    if (!cronologia || cronologia.length < 2)
      return null;
    return cronologia[cronologia.length - 1]?.userData?.userName ?? null;
  }

  public get userTooltip(): string | null {
    const impersonato = this.impersonatedUserName;
    if (!impersonato)
      return this.loggedUserName;
    const login = this.impersonatedLogin;
    return this.loggedUserName + ' → ' + impersonato + (login && login !== impersonato ? ' (' + login + ')' : '');
  }

  goToDashboard(): void {
    this.navCtrl.navigateRoot('/dashboard');
  }

  public get isMobile(): boolean {
    return this.platform.width() < 576;
  }

  filter(ev: any) {
    let val = ev.target.value;
    this.ev_Filter.emit(val);
  }

  dismissJob(jobId: string): void {
    this.longJobNotifierService.dismissJob(jobId);
  }

  /** Stato dell'anello: in corso, finito, file pronto da scaricare, finito con errore. */
  statoJob(job: LongJobProgressUpdate): 'corso' | 'ok' | 'file' | 'errore' {
    if (!job.isFinished)
      return 'corso';
    const tipo = job.message?.msgType;
    if (tipo === MessageType.Error || tipo === MessageType.Exception)
      return 'errore';
    if (job.category === LongJobCategory.FileGeneration && job.downloadUrl)
      return this.inPreparazione(job) ? 'corso' : 'file';
    return 'ok';
  }

  /*
   Telefono: il file e' pronto sul server ma non ancora arrivato in memoria per il menu Condividi. L'icona resta
   "in corso" (anello che gira): toccandola prima il menu non si apre e il download diretto, nell'app installata,
   non funziona. Diventa "file pronto" quando il tocco funziona di sicuro (o se il file non si puo' condividere).
  */
  /** percentuale del file arrivato sul telefono; null se non si sa (totale non dato dal server) */
  percentualeRicezione(job: LongJobProgressUpdate): number | null {
    const r = PageToolbarComponent.ricezione.get(PageToolbarComponent.nomeFileJob(job));
    return r && r.totale ? Math.min(100, Math.round(r.ricevuti * 100 / r.totale)) : null;
  }

  private static kb(byte: number): string {
    return byte >= 1024 * 1024 ? (byte / (1024 * 1024)).toFixed(1) + ' MB' : Math.max(1, Math.round(byte / 1024)) + ' KB';
  }

  inPreparazione(job: LongJobProgressUpdate): boolean {
    if (!job.isFinished || job.category !== LongJobCategory.FileGeneration || !job.downloadUrl || !this.condivisioneDisponibile)
      return false;
    return PageToolbarComponent.fileInArrivo.has(PageToolbarComponent.nomeFileJob(job));
  }

  /** stroke-dasharray dell'anello (circonferenza 100): avanzamento; finito: pieno; senza avanzamento: arco che gira */
  trattoJob(job: LongJobProgressUpdate): string {
    if (this.inPreparazione(job)) {
      const p = this.percentualeRicezione(job);
      return p != null && p > 0 ? `${p} 100` : '25 100';
    }
    if (job.isFinished)
      return '100 100';
    const p = Math.max(0, Math.min(100, job.progressPercentage ?? 0));
    return p > 0 ? `${p} 100` : '25 100';
  }

  popupJob(job: LongJobProgressUpdate): HoverPopupData {
    const stato = this.statoJob(job);
    const t = (k: string) => this.transloco.translate('components.pageToolbar.' + k);
    const extraInfo: Record<string, string> = {};
    const preparazione = this.inPreparazione(job);
    if (stato === 'corso' && !preparazione)
      extraInfo[t('jobProgress')] = `${Math.round(job.progressPercentage ?? 0)}%`;
    if (preparazione) {
      // ricezione del file: percentuale e byte (es. "45% (120 KB di 260 KB)")
      const r = PageToolbarComponent.ricezione.get(PageToolbarComponent.nomeFileJob(job));
      const p = this.percentualeRicezione(job);
      extraInfo[t('jobReceiving')] = !r || r.ricevuti === 0 ? '...'
        : p != null ? `${p}% (${PageToolbarComponent.kb(r.ricevuti)} / ${PageToolbarComponent.kb(r.totale!)})`
                    : PageToolbarComponent.kb(r.ricevuti);
    }
    else if (stato === 'file')
      extraInfo[t('jobAction')] = t('jobClickDownload');
    else
      extraInfo[t('jobAction')] = t('jobClickDismiss');
    return {
      title: t(preparazione ? 'jobPreparing' : stato === 'corso' ? 'jobRunning' : stato === 'file' ? 'jobFileReady' : stato === 'errore' ? 'jobError' : 'jobDone'),
      content: job.message?.text ?? '',
      extraInfo
    };
  }

  /** Clic sull'anello: file pronto -> download; job finito -> chiude la notifica; in corso -> niente. */
  onJobClick(job: LongJobProgressUpdate): void {
    // in preparazione (telefono): il tocco aspetta che il file arrivi
    if (!job.isFinished || this.inPreparazione(job))
      return;
    if (job.category === LongJobCategory.FileGeneration)
      this.onJobLabelClick(job);
    else
      this.dismissJob(job.jobId);
  }

  private static nomeFileJob(job: LongJobProgressUpdate): string {
    return (job.downloadUrl ?? '').split('/').pop() || '';
  }

  private preparaCodice(nomeFile: string): void {
    const pronto = PageToolbarComponent.codiciDownload.get(nomeFile);
    if (!nomeFile || (pronto && pronto.scade > Date.now()) || PageToolbarComponent.codiciInArrivo.has(nomeFile))
      return;
    PageToolbarComponent.codiciInArrivo.add(nomeFile);
    this.http.get<{ code: string }>(environment.remoteData.apiUri + 'Account/DownloadCode/' + encodeURIComponent(nomeFile))
      .subscribe({
        next: res => {
          PageToolbarComponent.codiciInArrivo.delete(nomeFile);
          PageToolbarComponent.codiciDownload.set(nomeFile, { code: res.code, scade: Date.now() + PageToolbarComponent.validitaCodiceMs });
        },
        // senza codice si riprova al tocco
        error: () => PageToolbarComponent.codiciInArrivo.delete(nomeFile)
      });
  }

  // condivisione dei file: solo su telefono (sul PC resta il download) e se il browser la offre
  private get condivisioneDisponibile(): boolean {
    return this.ui.isMobileSignal() && typeof navigator !== 'undefined' && typeof navigator.share === 'function'
           && typeof navigator.canShare === 'function';
  }

  private preparaFile(job: LongJobProgressUpdate): void {
    const nomeFile = PageToolbarComponent.nomeFileJob(job);
    if (!nomeFile || PageToolbarComponent.fileCondivisibili.has(nomeFile) || PageToolbarComponent.fileInArrivo.has(nomeFile))
      return;
    PageToolbarComponent.fileInArrivo.add(nomeFile);
    // con l'avanzamento della ricezione: l'anello si riempie di nuovo e il popup dice quanto e' arrivato
    PageToolbarComponent.ricezione.set(nomeFile, { ricevuti: 0, totale: null });
    this.http.get(environment.remoteData.apiUri + job.downloadUrl, { responseType: 'blob', observe: 'events', reportProgress: true })
      .subscribe({
        next: evento => {
          if (evento.type === HttpEventType.DownloadProgress) {
            PageToolbarComponent.ricezione.set(nomeFile, { ricevuti: evento.loaded, totale: evento.total ?? null });
            return;
          }
          if (evento.type !== HttpEventType.Response || !evento.body)
            return;
          PageToolbarComponent.fileInArrivo.delete(nomeFile);
          PageToolbarComponent.ricezione.delete(nomeFile);
          const blob = evento.body;
          const file = new File([blob], job.fileName || nomeFile, { type: blob.type || 'application/octet-stream' });
          // solo se il browser sa condividere questo tipo di file; altrimenti si usera' il download
          if (navigator.canShare({ files: [file] }))
            PageToolbarComponent.fileCondivisibili.set(nomeFile, file);
        },
        error: () => {
          PageToolbarComponent.fileInArrivo.delete(nomeFile);
          PageToolbarComponent.ricezione.delete(nomeFile);
        }
      });
  }

  private avviaDownload(nomeFile: string, code: string, job: LongJobProgressUpdate): void {
    PageToolbarComponent.codiciDownload.delete(nomeFile);
    apriDownload(environment.remoteData.apiUri + 'Account/DownloadFile/' + encodeURIComponent(nomeFile) + '?code=' + encodeURIComponent(code),
                 job.fileName || nomeFile || 'export');
    this.dismissJob(job.jobId);
  }

  onJobLabelClick(job: LongJobProgressUpdate): void {
    const downloadUrl = job.downloadUrl;
    if (job.category !== LongJobCategory.FileGeneration || !job.isFinished) return;
    // job finito senza file (es. niente da esportare): il messaggio spiega il motivo, il click lo chiude
    if (!downloadUrl) {
      this.dismissJob(job.jobId);
      return;
    }

    // un download alla volta per job: con tocchi ripetuti sul telefono non parte piu' volte
    if (this.scaricando.has(job.jobId))
      return;
    this.scaricando.add(job.jobId);
    // collegamento temporaneo del server (Account/DownloadCode -> Account/DownloadFile) invece del file in memoria:
    // nell'app installata sul telefono il blob a volte apriva il file o non faceva nulla, l'allegato si scarica sempre
    const nomeFile = PageToolbarComponent.nomeFileJob(job);

    // telefono con il file gia' arrivato: menu Condividi (chiuso senza scegliere: la notifica resta)
    const file = PageToolbarComponent.fileCondivisibili.get(nomeFile);
    if (file && this.condivisioneDisponibile) {
      this.scaricando.delete(job.jobId);
      navigator.share({ files: [file], title: file.name })
        .then(() => {
          PageToolbarComponent.fileCondivisibili.delete(nomeFile);
          this.dismissJob(job.jobId);
        })
        .catch((e: any) => {
          // chiuso senza scegliere (AbortError): nessun avviso, la notifica resta per riprovare
          if (e?.name !== 'AbortError')
            this.toastCtrl.create({ message: this.transloco.translate('components.pageToolbar.jobDownloadError'), duration: 4000, color: 'danger', position: 'top' })
              .then(x => x.present());
        });
      return;
    }

    const pronto = PageToolbarComponent.codiciDownload.get(nomeFile);
    if (pronto && pronto.scade > Date.now()) {
      // codice gia' arrivato: download subito, nel tocco
      this.scaricando.delete(job.jobId);
      this.avviaDownload(nomeFile, pronto.code, job);
      return;
    }
    this.http.get<{ code: string }>(environment.remoteData.apiUri + 'Account/DownloadCode/' + encodeURIComponent(nomeFile))
      .subscribe({
        next: res => {
          this.scaricando.delete(job.jobId);
          this.avviaDownload(nomeFile, res.code, job);
        },
        // prima non succedeva nulla: ora un avviso, e la notifica resta per riprovare
        error: () => {
          this.scaricando.delete(job.jobId);
          this.toastCtrl.create({ message: this.transloco.translate('components.pageToolbar.jobDownloadError'), duration: 4000, color: 'danger', position: 'top' })
            .then(t => t.present());
        }
      });
  }

}
