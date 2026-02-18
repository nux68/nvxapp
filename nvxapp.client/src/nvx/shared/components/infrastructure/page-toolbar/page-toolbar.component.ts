import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { UserNavigationService } from '../../../../Utility/infrastructure/user-navigation.service';
import { LongJobNotifierService, LongJobProgressUpdate } from '../../../../Utility/infrastructure/long-job-notifier.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-page-toolbar',
  templateUrl: './page-toolbar.component.html',
  styleUrls: ['./page-toolbar.component.scss'],
  standalone: false
})
export class PageToolbarComponent  implements OnInit {


  @Input() showFilter: boolean;
  @Input() showBreadcrumbs: boolean;
  @Input() headerColor: string;

  @Output('ev_Filter') _emFilter: EventEmitter<string> = new EventEmitter();
  @Input() title: string;
  public queryText: string;

  public activeJobs$: Observable<LongJobProgressUpdate[]>;

  constructor(public userNavigationService: UserNavigationService,
              public longJobNotifierService: LongJobNotifierService)
  {
    this.activeJobs$ = this.longJobNotifierService.activeJobs$;
  }

  ngOnInit() {
    
  }

  filter(ev: any) {

    let val = ev.target.value;
    this._emFilter.emit(val);

  }

}
