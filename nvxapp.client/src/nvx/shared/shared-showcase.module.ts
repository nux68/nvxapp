import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular/lazy';
import { ShowcaseFooterComponent } from './components/infrastructure/showcase-footer/showcase-footer.component';


// componenti comuni alle pagine dello showcase (sito pubblico prima del login)
@NgModule({
  imports: [
    CommonModule,
    IonicModule
  ],
  declarations: [ShowcaseFooterComponent],
  exports: [ShowcaseFooterComponent]
})
export class SharedShowcaseModule { }
