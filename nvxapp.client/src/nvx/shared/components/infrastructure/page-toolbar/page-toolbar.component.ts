import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';
import { UserNavigationService } from '../../../../Utility/infrastructure/user-navigation.service';
import { LongJobNotifierService, LongJobProgressUpdate, LongJobCategory } from '../../../../Utility/infrastructure/long-job-notifier.service';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { NavController, Platform } from '@ionic/angular';
import { AuthService } from '../../../../Utility/infrastructure/auth.service';

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
  public queryText: string;

  public activeJobs$: Observable<LongJobProgressUpdate[]>;
  public LongJobCategory = LongJobCategory;

  constructor(public userNavigationService: UserNavigationService,
    public longJobNotifierService: LongJobNotifierService,
              private authService: AuthService,
              private navCtrl: NavController,
              private platform: Platform,
              private http: HttpClient)
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

  // username dell'utente impersonato (l'ultimo della cronologia), solo se diverso da quello del login
  public get impersonatedUserName(): string | null {
    const cronologia = this.userNavigationService.userCronology;
    if (!cronologia || cronologia.length < 2)
      return null;
    return cronologia[cronologia.length - 1]?.userData?.userName ?? null;
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
