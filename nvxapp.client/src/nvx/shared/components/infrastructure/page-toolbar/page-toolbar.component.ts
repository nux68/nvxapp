import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../../Utility/infrastructure/user-navigation.service';
import { LongJobNotifierService, LongJobProgressUpdate, LongJobCategory } from '../../../../Utility/infrastructure/long-job-notifier.service';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
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
        if (this.statoJob(job) === 'file')
          this.preparaCodice(PageToolbarComponent.nomeFileJob(job));
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
    return job.category === LongJobCategory.FileGeneration && job.downloadUrl ? 'file' : 'ok';
  }

  /** stroke-dasharray dell'anello (circonferenza 100): avanzamento; finito: pieno; senza avanzamento: arco che gira */
  trattoJob(job: LongJobProgressUpdate): string {
    if (job.isFinished)
      return '100 100';
    const p = Math.max(0, Math.min(100, job.progressPercentage ?? 0));
    return p > 0 ? `${p} 100` : '25 100';
  }

  popupJob(job: LongJobProgressUpdate): HoverPopupData {
    const stato = this.statoJob(job);
    const t = (k: string) => this.transloco.translate('components.pageToolbar.' + k);
    const extraInfo: Record<string, string> = {};
    if (stato === 'corso')
      extraInfo[t('jobProgress')] = `${Math.round(job.progressPercentage ?? 0)}%`;
    else if (stato === 'file')
      extraInfo[t('jobAction')] = t('jobClickDownload');
    else
      extraInfo[t('jobAction')] = t('jobClickDismiss');
    return {
      title: t(stato === 'corso' ? 'jobRunning' : stato === 'file' ? 'jobFileReady' : stato === 'errore' ? 'jobError' : 'jobDone'),
      content: job.message?.text ?? '',
      extraInfo
    };
  }

  /** Clic sull'anello: file pronto -> download; job finito -> chiude la notifica; in corso -> niente. */
  onJobClick(job: LongJobProgressUpdate): void {
    if (!job.isFinished)
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
