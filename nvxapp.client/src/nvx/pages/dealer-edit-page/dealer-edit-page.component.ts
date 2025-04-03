import { Component, OnInit } from '@angular/core';
import { NavController } from '@ionic/angular';
import { ButtonItem, UserInterfaceService } from '../../Utility/user-interface.service';


@Component({
  selector: 'app-dealer-edit-page',
  templateUrl: './dealer-edit-page.component.html',
  styleUrls: ['./dealer-edit-page.component.scss'],
  standalone: false
})
export class DealerEditPageComponent  implements OnInit {

  public title!: string;
  public buttonbar: ButtonItem[]=[];

  constructor(private navCtrl: NavController,
              private userInterfaceService: UserInterfaceService)
  {

    this.title = 'DealerEditPage';

    this.buttonbar = userInterfaceService.Btn_ConfermaAnnulla;
    this.buttonbar[0].event = this.handleButtonConfirmClick;
    this.buttonbar[1].event = this.handleButtonCancelClick;
  }

  ionViewWillEnter() {

    const state = history.state;
    if (state && state.id) {
      console.log('Item ID:', state.id); // Usa l'itemId come necessario
    }

  }


  handleButtonConfirmClick = (param: object) => {
    this.navCtrl.back();
  }

  handleButtonCancelClick = (param: object) => {
    this.navCtrl.back();
  }


  ngOnInit() {}

}
