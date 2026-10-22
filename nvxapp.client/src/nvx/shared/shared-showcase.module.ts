import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslocoPipe } from '@jsverse/transloco';
import { ShowcaseFooterComponent } from './components/infrastructure/showcase-footer/showcase-footer.component';


// componenti comuni alle pagine dello showcase (sito pubblico prima del login)
@NgModule({
  imports: [
    CommonModule,
    TranslocoPipe,
  ],
  declarations: [ShowcaseFooterComponent],
  exports: [ShowcaseFooterComponent, TranslocoPipe]
})
export class SharedShowcaseModule { }
