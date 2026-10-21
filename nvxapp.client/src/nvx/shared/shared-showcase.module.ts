import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShowcaseFooterComponent } from './components/infrastructure/showcase-footer/showcase-footer.component';


// componenti comuni alle pagine dello showcase (sito pubblico prima del login)
@NgModule({
  imports: [
    CommonModule,
  ],
  declarations: [ShowcaseFooterComponent],
  exports: [ShowcaseFooterComponent]
})
export class SharedShowcaseModule { }
