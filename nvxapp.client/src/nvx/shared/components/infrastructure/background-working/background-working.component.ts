import { Component, Input, OnInit, ChangeDetectionStrategy } from '@angular/core';


@Component({
  selector: 'app-background-working',
  templateUrl: './background-working.component.html',
  styleUrls: ['./background-working.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class BackgroundWorkingComponent   {

  @Input() show: boolean = false;

}
