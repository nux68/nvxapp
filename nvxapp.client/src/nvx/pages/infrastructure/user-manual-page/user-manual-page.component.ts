import { Component, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { TranslocoService } from '@jsverse/transloco';
import { Subscription, filter, take } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { UserManualGenerateInModel, UserManualService } from '../../../ClientServer-Service/Infrastructure/UserManual/user-manual.service';
import { LongJobNotifierService, LongJobProgressUpdate } from '../../../Utility/infrastructure/long-job-notifier.service';
import { SITE_NAME } from '../../../Utility/infrastructure/site-name';

/*
 Manuale utente in PDF (codice nell'url: /usermanual/<codice>). Il PDF si prepara solo premendo "Scarica": il server lo
 compone per i ruoli dell'utente con un job e, appena pronto, il file si scarica da solo (resta anche nella barra dei job).
 Titolo e descrizione: chiavi pages.userManual.manuals.<codice> (ogni ramo aggiunge le sue nelle proprie traduzioni).
*/
@Component({
  selector: 'app-user-manual-page',
  templateUrl: './user-manual-page.component.html',
  styleUrls: ['./user-manual-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class UserManualPageComponent implements OnDestroy {

  public code = '';
  public inPreparazione = false;
  private jobId: string | null = null;
  private readonly sub: Subscription;

  constructor(private route: ActivatedRoute,
              private http: HttpClient,
              private service: UserManualService,
              private longJobNotifierService: LongJobNotifierService,
              private transloco: TranslocoService) {
    // job del manuale terminato: si scarica il file e si toglie la notifica
    this.sub = this.longJobNotifierService.jobFinished$.pipe(filter(j => !!this.jobId && j.jobId === this.jobId)).subscribe(job => this.scaricaJob(job));
  }

  private scaricaJob(job: LongJobProgressUpdate) {
      this.inPreparazione = false;
      this.jobId = null;
      if (!job.downloadUrl)
        return;
      this.http.get(environment.remoteData.apiUri + job.downloadUrl, { responseType: 'blob' }).subscribe(blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = (job.fileName || this.title) + (job.fileName?.toLowerCase().endsWith('.pdf') ? '' : '.pdf');
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.longJobNotifierService.dismissJob(job.jobId);
      });
  }

  get title(): string { return this.testo('title'); }
  get description(): string { return this.testo('description'); }

  // testo del manuale con ripiego su quello generico
  private testo(campo: string): string {
    const chiave = `pages.userManual.manuals.${this.code}.${campo}`;
    const t = this.transloco.translate(chiave);
    return t !== chiave ? t : this.transloco.translate('pages.userManual.' + campo);
  }

  ionViewWillEnter() {
    this.code = this.route.snapshot.paramMap.get('code') ?? '';
  }

  scarica() {
    if (!this.code || this.inPreparazione)
      return;
    const request = new GenericRequest<UserManualGenerateInModel>(UserManualGenerateInModel);
    request.data = { code: this.code, siteName: SITE_NAME };
    this.inPreparazione = true;
    this.service.Generate(request).subscribe({
      next: res => {
        if (res.success && res.data?.jobId) {
          this.jobId = res.data.jobId;
          // un manuale breve puo' essere gia' pronto prima della risposta (il server risponde dopo una breve attesa)
          this.longJobNotifierService.activeJobs$.pipe(take(1)).subscribe(jobs => {
            const pronto = jobs.find(j => j.jobId === this.jobId && j.isFinished);
            if (pronto)
              this.scaricaJob(pronto);
          });
        }
        else
          this.inPreparazione = false;
      },
      error: () => this.inPreparazione = false
    });
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }
}
