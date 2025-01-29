import { Component, Input, OnInit } from '@angular/core';


@Component({
  selector: 'app-background-working',
  templateUrl: './background-working.component.html',
  styleUrls: ['./background-working.component.scss'],
  standalone: false
})
export class BackgroundWorkingComponent   {

  @Input() show: boolean = false;

}
