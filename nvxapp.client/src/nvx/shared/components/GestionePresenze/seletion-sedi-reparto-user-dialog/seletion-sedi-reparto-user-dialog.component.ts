import { Component, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { ModalController, NavController } from '@ionic/angular';

@Component({
  selector: 'app-seletion-sedi-reparto-user-dialog',
  templateUrl: './seletion-sedi-reparto-user-dialog.component.html',
  styleUrls: ['./seletion-sedi-reparto-user-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class SeletionSediRepartoUserDialogComponent {

  public title!: string;
  public buttonbar: ButtonItem[] = [];

  /** Se valorizzato si propongono solo questi utenti (es. con le competenze per le attività della sub commessa). */
  public filterIdAspNetUsers: string[] | null = null;

  result: SeletionSediRepartoUserDialogResult = {
    idSede: 0,
    idReparto: 0,
    userIds: []
  };

  constructor(protected userInterfaceService: UserInterfaceService,
              private modalCtrl: ModalController,
              private transloco: TranslocoService
              )
  {
    this.title = this.transloco.translate('attendance.components.seletionSediRepartoUserDialog.title');

    this.buttonbar = userInterfaceService.Btn_ConfermaAnnulla;
    this.buttonbar[0].event = this._handleButtonConfirmClick;
    this.buttonbar[1].event = this._handleButtonCancelClick;

  }


  private _handleButtonConfirmClick = (param: object) => {
    return this.modalCtrl.dismiss(this.result, 'confirm');
  }

  private _handleButtonCancelClick = (param: object) => {
    return this.modalCtrl.dismiss(null, 'confirm');
  }



  onPeriodChange(period: { year: number, month: number } | undefined): void { }
  onCurrentUserChanged(userId: string[] | undefined): void {
    this.result.userIds = userId;
  }
  onSedeChanged(sediId: number | undefined): void {
    this.result.idSede = sediId;
  }
  onRepartiChanged(repartoIds: number[] | undefined): void {

    if (!repartoIds || repartoIds.length == 0)
      this.result.idReparto = 0;
    else
      this.result.idReparto = repartoIds[0];

  }
  onAllUsersInSelectionChanged(userIds: string[] | undefined): void {
    //this.result.userIds = userIds;
  }



}

export interface SeletionSediRepartoUserDialogResult {
  idSede: number;
  idReparto: number;
  userIds: string[] | undefined;
}
