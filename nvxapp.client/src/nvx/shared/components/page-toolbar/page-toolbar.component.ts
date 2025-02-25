import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { UserNavigationService } from '../../../Utility/user-navigation.service';

@Component({
  selector: 'app-page-toolbar',
  templateUrl: './page-toolbar.component.html',
  styleUrls: ['./page-toolbar.component.scss'],
  standalone: false
})
export class PageToolbarComponent  implements OnInit {


  @Input() showFilter: boolean;
  @Input() showBreadcrumbs: boolean;

  @Output('ev_Filter') _emFilter: EventEmitter<string> = new EventEmitter();
  @Input() title: string;
  public queryText: string;

  constructor(public userNavigationService: UserNavigationService) { }

  ngOnInit() {
    
  }

  filter(ev: any) {

    let val = ev.target.value;
    this._emFilter.emit(val);

  }

}
