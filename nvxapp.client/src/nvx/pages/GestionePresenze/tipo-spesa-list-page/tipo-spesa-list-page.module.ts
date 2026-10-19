import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular/lazy';
import { TipoSpesaListPageComponent } from './tipo-spesa-list-page.component';
import { RouterModule } from '@angular/router';
import { SharedComponentInfrastructureModule } from '../../../shared/shared-component-infrastructure.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: TipoSpesaListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [TipoSpesaListPageComponent]
})
export class TipoSpesaListPageModule {}
