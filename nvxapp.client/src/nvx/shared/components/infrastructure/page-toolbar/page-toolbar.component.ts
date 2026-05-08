import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { UserNavigationService } from '../../../../Utility/infrastructure/user-navigation.service';
import { LongJobNotifierService, LongJobProgressUpdate, LongJobCategory } from '../../../../Utility/infrastructure/long-job-notifier.service';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-page-toolbar',
  templateUrl: './page-toolbar.component.html',
  styleUrls: ['./page-toolbar.component.scss'],
  standalone: false
})
export class PageToolbarComponent  implements OnInit {


  @Input() showFilter: boolean;
  @Input() showBreadcrumbs: boolean;
  @Input() showActiveJobs: boolean = true;
  @Input() headerColor: string = "primary";

  @Output('ev_Filter') _emFilter: EventEmitter<string> = new EventEmitter();
  @Input() title: string;
  public queryText: string;

  public activeJobs$: Observable<LongJobProgressUpdate[]>;
  public LongJobCategory = LongJobCategory;

  constructor(public userNavigationService: UserNavigationService,
              public longJobNotifierService: LongJobNotifierService,
              private http: HttpClient)
  {
    this.activeJobs$ = this.longJobNotifierService.activeJobs$;
  }

  ngOnInit() {
    
  }

  filter(ev: any) {
    let val = ev.target.value;
    this._emFilter.emit(val);
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
        a.download = downloadUrl.split('/').pop() || 'export';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.dismissJob(job.jobId);
      });
  }

}
