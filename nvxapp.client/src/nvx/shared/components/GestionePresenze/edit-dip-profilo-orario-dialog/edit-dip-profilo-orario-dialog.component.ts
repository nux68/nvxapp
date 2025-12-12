import { Component, OnInit } from '@angular/core';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { ModalController } from '@ionic/angular';

@Component({
  selector: 'app-edit-dip-profilo-orario-dialog',
  templateUrl: './edit-dip-profilo-orario-dialog.component.html',
  styleUrls: ['./edit-dip-profilo-orario-dialog.component.scss'],
  standalone: false
})
export class EditDipProfiloOrarioDialogComponent  implements OnInit {

  public title!: string;
  public buttonbar: ButtonItem[] = [];

  result: EditDipProfiloOrarioDialogComponentResult = {
  };

  constructor(protected userInterfaceService: UserInterfaceService,
    private modalCtrl: ModalController
  )

  {
    this.title = 'Profilo orario';

    this.buttonbar = userInterfaceService.Btn_ConfermaAnnulla;
    this.buttonbar[0].event = this._handleButtonConfirmClick;
    this.buttonbar[1].event = this._handleButtonCancelClick;
  }

  ngOnInit() {}

  private _handleButtonConfirmClick = (param: object) => {
    return this.modalCtrl.dismiss(this.result, 'confirm');
  }

  private _handleButtonCancelClick = (param: object) => {
    return this.modalCtrl.dismiss(null, 'confirm');
  }

}


export interface EditDipProfiloOrarioDialogComponentResult {
  //idSede: number;
  //idReparto: number;
}
