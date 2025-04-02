import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-dealer-edit-page',
  templateUrl: './dealer-edit-page.component.html',
  styleUrls: ['./dealer-edit-page.component.scss'],
  standalone: false
})
export class DealerEditPageComponent  implements OnInit {

  public title!: string;

  constructor(/*private route: ActivatedRoute*/
    private activatedRoute: ActivatedRoute) {
    this.title = 'DealerEditPage';
  }

  ionViewWillEnter() {
    //this.route.queryParams.subscribe(params => {
    //  const id = params['id']; // Recupera il parametro 'id'
    //  console.log(id); // Usa l'id come necessario
    //});

    //const navigation = this.activatedRoute.routerState.snapshot.root.queryParams;
    //const itemId = this.activatedRoute.routerState.snapshot.root;
    //if (itemId) {
    //  console.log(itemId);
    //}

    const state = history.state;
    if (state && state.id) {
      
      console.log('Item ID:', state.id); // Usa l'itemId come necessario
    }

  }

   


  ngOnInit() {}

}
