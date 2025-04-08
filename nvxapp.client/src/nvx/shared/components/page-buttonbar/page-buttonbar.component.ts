import { Component, Input, OnInit } from '@angular/core';
import { ButtonItem } from '../../../Utility/user-interface.service';

@Component({
  selector: 'app-page-buttonbar',
  templateUrl: './page-buttonbar.component.html',
  styleUrls: ['./page-buttonbar.component.scss'],
  standalone: false
})
export class PageButtonbarComponent  implements OnInit {

  @Input() buttonbar: ButtonItem[]=[];

  constructor() { }

  ngOnInit() {}

}



