import { Component, OnInit } from '@angular/core';
import { NvxHttpInterceptorService } from '../../../infrastructure/http-interceptor';

@Component({
  selector: 'app-http-background-working-component',
  templateUrl: './http-background-working-component.component.html',
  styleUrls: ['./http-background-working-component.component.scss'],
  standalone:false
})
export class HttpBackgroundWorkingComponentComponent  implements OnInit {

  public _loading: boolean = false;

  constructor(private nvxHttpInterceptorService: NvxHttpInterceptorService) { }

  ngOnInit() {

    this.nvxHttpInterceptorService.isActiveCall$.subscribe(res => {
      this._loading = res;
    });

  }

}
