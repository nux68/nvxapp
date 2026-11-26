import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy, inject } from '@angular/core';
import { UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../../Utility/infrastructure/user-navigation.service';
import { LongJobNotifierService, LongJobProgressUpdate, LongJobCategory } from '../../../../Utility/infrastructure/long-job-notifier.service';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { NavController, Platform } from '@ionic/angular';
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

  constructor(public userNavigationService: UserNavigationService,
    public longJobNotifierService: LongJobNotifierService,
              private authService: AuthService,
              private navCtrl: NavController,
              private platform: Platform,
              private http: HttpClient,
              private transloco: TranslocoService,
              public menuLayoutService: MenuLayoutService)
  {
    this.activeJobs$ = this.longJobNotifierService.activeJobs$;
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

  onJobLabelClick(job: LongJobProgressUpdate): void {
    const downloadUrl = job.downloadUrl;
    if (job.category !== LongJobCategory.FileGeneration || !job.isFinished || !downloadUrl) return;

    this.http.get(environment.remoteData.apiUri + downloadUrl, { responseType: 'blob' })
      .subscribe(blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = job.fileName || downloadUrl.split('/').pop() || 'export';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.dismissJob(job.jobId);
      });
  }

}
