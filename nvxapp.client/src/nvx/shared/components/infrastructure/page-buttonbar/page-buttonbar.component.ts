import { Component, Input, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ButtonItem } from '../../../../Utility/infrastructure/user-interface.service';

@Component({
  selector: 'app-page-buttonbar',
  templateUrl: './page-buttonbar.component.html',
  styleUrls: ['./page-buttonbar.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class PageButtonbarComponent  implements OnInit {

  @Input() buttonbar: ButtonItem[]=[];

  constructor() { }

  ngOnInit() {}

}



