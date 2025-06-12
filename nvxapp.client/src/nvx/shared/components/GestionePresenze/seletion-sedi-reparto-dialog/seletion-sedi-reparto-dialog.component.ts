import { Component, OnInit } from '@angular/core';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { ModalController, NavController } from '@ionic/angular';

@Component({
  selector: 'app-seletion-sedi-reparto-dialog',
  templateUrl: './seletion-sedi-reparto-dialog.component.html',
  styleUrls: ['./seletion-sedi-reparto-dialog.component.scss'],
  standalone: false
})
export class SeletionSediRepartoDialogComponent  implements OnInit {

  public title!: string;
  public buttonbar: ButtonItem[] = [];

  constructor(protected userInterfaceService: UserInterfaceService,
              private modalCtrl: ModalController
    //private navCtrl: NavController
    )
  {
    this.title = 'Seleziona';

    this.buttonbar = userInterfaceService.Btn_LogInAnnulla;
    this.buttonbar[0].event = this._handleButtonConfirmClick;
    this.buttonbar[1].event = this._handleButtonCancelClick;

  }

  ngOnInit() { }

  private _handleButtonConfirmClick = (param: object) => {
    //this.navCtrl.back();

    

    return this.modalCtrl.dismiss({}, 'confirm');
  }

  private _handleButtonCancelClick = (param: object) => {
    //this.navCtrl.back();
    return this.modalCtrl.dismiss({}, 'confirm');
  }



  onPeriodChange(period: { year: number, month: number } | undefined): void { }
  onCurrentUserChanged(userId: string | undefined): void { }
  onSedeChanged(sediId: number | undefined): void { }
  onRepartiChanged(repartoIds: number[] | undefined): void { }
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void { }



}
