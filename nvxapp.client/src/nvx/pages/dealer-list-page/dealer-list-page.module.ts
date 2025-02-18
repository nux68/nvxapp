import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { DealerListPageComponent } from './dealer-list-page.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    RouterModule.forChild([
      {
        path: '',
        component: DealerListPageComponent
      }
    ])
  ],

  declarations: [DealerListPageComponent],

})
export class DealerListPageModule { }



//@NgModule({
//  declarations: [],
//  imports: [
//    CommonModule
//  ]
//})
//export class DealerListPageModule { }
