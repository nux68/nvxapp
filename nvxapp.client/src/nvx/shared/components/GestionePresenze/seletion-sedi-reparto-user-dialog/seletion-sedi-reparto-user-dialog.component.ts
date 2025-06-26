import { Component, OnInit } from '@angular/core';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { ModalController, NavController } from '@ionic/angular';

@Component({
  selector: 'app-seletion-sedi-reparto-user-dialog',
  templateUrl: './seletion-sedi-reparto-user-dialog.component.html',
  styleUrls: ['./seletion-sedi-reparto-user-dialog.component.scss'],
  standalone: false
})
export class SeletionSediRepartoUserDialogComponent  implements OnInit {

  public title!: string;
  public buttonbar: ButtonItem[] = [];

  result: SeletionSediRepartoUserDialogResult = {
    idSede: 0,   
    idReparto: 0 
  };

  constructor(protected userInterfaceService: UserInterfaceService,
              private modalCtrl: ModalController
              )
  {
    this.title = 'Seleziona';

    this.buttonbar = userInterfaceService.Btn_ConfermaAnnulla;
    this.buttonbar[0].event = this._handleButtonConfirmClick;
    this.buttonbar[1].event = this._handleButtonCancelClick;

  }

  ngOnInit() { }

  private _handleButtonConfirmClick = (param: object) => {
    return this.modalCtrl.dismiss(this.result, 'confirm');
  }

  private _handleButtonCancelClick = (param: object) => {
    return this.modalCtrl.dismiss(null, 'confirm');
  }



  onPeriodChange(period: { year: number, month: number } | undefined): void { }
  onCurrentUserChanged(userId: string[] | undefined): void { }
  onSedeChanged(sediId: number | undefined): void {
    this.result.idSede = sediId;
  }
  onRepartiChanged(repartoIds: number[] | undefined): void {
  
    if (repartoIds.length == 0)
      this.result.idReparto = 0;
    else
      this.result.idReparto = repartoIds[0];

  

  }
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void { }



}

export interface SeletionSediRepartoUserDialogResult {
  idSede: number;
  idReparto: number;
}
