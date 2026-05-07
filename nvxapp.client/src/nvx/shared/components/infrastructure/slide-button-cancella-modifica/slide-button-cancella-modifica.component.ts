import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-slide-button-cancella-modifica',
  templateUrl: './slide-button-cancella-modifica.component.html',
  styleUrls: ['./slide-button-cancella-modifica.component.scss'],
  standalone: false
})
export class SlideButtonCancellaModificaComponent  implements OnInit {

  @Input() item: any;

  @Input() btnDelete: any;
  @Input() showDelete: boolean = true;

  @Input() btnEdit: any;
  @Input() showEdit: boolean = true;

  constructor() { }

  ngOnInit() {}

}
