
# Elenco 1

Questo documento fornisce linee guida per creare una pagina di elenco (lista) nel client nvxapp.



- [Client](./CONTRIBUTING_nvxapp_client.md)
- [Home](./CONTRIBUTING.md)



## Componet
```linguaggio

import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { FabMenuItem, FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { UserNavigationService } from '../../../Utility/infrastructure/user-navigation.service';
import { Par_CausaliInModel, Par_CausaliModel } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/Models/par-causali-model';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { ParCausaliService } from '../../../ClientServer-Service/GestionePresenze/Par_Causali/par-causali.service';
import { SharedParameterGestionePresenzeService } from '../../../shared/shared-parameter-gestione-presenze.service';
import { CollectionDialogService } from '../../../shared/components/infrastructure/generic-dialog/collection-dialog.service';

@Component({
  selector: 'app-causali-list-page',
  templateUrl: './causali-list-page.component.html',
  styleUrls: ['./causali-list-page.component.scss'],
  standalone: false
})
export class CausaliListPageComponent implements OnInit {

  public title!: string;
  public searchText!: string;
  public par_CausaliList: Par_CausaliModel[] | null = null;
  public btnEdit: ButtonItem;

  constructor(private navCtrl: NavController,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              public fabMenuService: FabMenuService,
              private parCausaliService: ParCausaliService,
              private userInterfaceService: UserInterfaceService,
              private collectionDialogService: CollectionDialogService,
              private userNavigationService: UserNavigationService) {

    this.title = 'Causali';
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.handleButtonEditClick;
  }

  ionViewWillEnter() {
    let request: GenericRequest<Par_CausaliInModel> = new GenericRequest<Par_CausaliInModel>(Par_CausaliInModel);
    this.parCausaliService.GetAll(request).subscribe(res => {
      this.par_CausaliList = res.data.par_Causali;
    });

    this.fabMenuService.fabMenuItem = [
      new FabMenuItem('Nuova Causale', 'add-circle-outline', () => {
        this.navCtrl.navigateForward('/causaliedit', {
          state: { id: 0 }
        });
      }),
    ];
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  ngOnInit() { }

  handleButtonEditClick = (item: any) => {
    this.navCtrl.navigateForward('/causaliedit', {
      state: { id: item.id }
    });
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }

  isAdmin(item: any) {
    return false;
  }

  getAll() {
    if (!this.par_CausaliList) {
      return [];
    }
    const sorted_CausaliList = this.par_CausaliList.sort((a, b) =>
      a.descrizione.localeCompare(b.descrizione)
    );
    return sorted_CausaliList;
  }
}


```



## HTML
```linguaggio

<app-page-toolbar [title]="title"
                  [showFilter]="true"
                  [showBreadcrumbs]="true"
                  (ev_Filter)="Filter($event)">
</app-page-toolbar>

<ion-content [fullscreen]="true">

  <ion-card>
    <ion-card-content>

      <ion-grid *ngIf="par_CausaliList!=null">

        <ion-row *ngFor="let item of getAll() | genericFilter: 'descrizione' : searchText" class="nvx-ion-row-4table">

          <ion-item-sliding class="ion-hide-sm-up">
            <ion-item>
              <ion-icon [name]="isAdmin(item) ? 'server-outline' : 'server-outline'"></ion-icon>
              <ion-label [ngStyle]="{ color: isAdmin(item) ? 'var(--ion-color-primary)' : '' }">
                <h2>{{ item.descrizione }}</h2>
                <p>{{ item.codice }}</p>
              </ion-label>
            </ion-item>

            <ion-item-options side="end">
              <ion-item-option color="{{btnEdit.color}}" (click)="btnEdit.event(item)">
                <ion-icon slot="start" name="{{btnEdit.image}}"></ion-icon>
                {{ btnEdit.text }}
              </ion-item-option>

            </ion-item-options>
          </ion-item-sliding>


          <ion-col size="10" class="ion-hide-sm-down">
            <ion-icon [name]="isAdmin(item) ? 'server-outline' : 'server-outline'"></ion-icon>
            <ion-label [ngStyle]="{ color: isAdmin(item) ? 'var(--ion-color-primary)' : '' }">
              {{ item.descrizione }}
            </ion-label>
          </ion-col>
          <ion-col size="2" class="ion-hide-sm-down">
            <ion-button size="small" fill="outline" shape="round" (click)="btnEdit.event(item)" color="{{btnEdit.color}}">
              <ion-icon name="{{btnEdit.image}}"></ion-icon>
              &nbsp;{{btnEdit.text}}
            </ion-button>
          </ion-col>

        </ion-row>

      </ion-grid>

    </ion-card-content>
  </ion-card>

</ion-content>



```




## CSS
```linguaggio




```


## MODULE
```linguaggio

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { CausaliListPageComponent } from './causali-list-page.component';
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
        component: CausaliListPageComponent
      }
    ]),
    SharedComponentInfrastructureModule,
  ],
  declarations: [CausaliListPageComponent]
})
export class CausaliListPageModule {}



```