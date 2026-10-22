import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { ModalController } from '@ionic/angular';
import { Par_CompetenzaModel } from '../../../../ClientServer-Service/GestionePresenze/Par_Competenza/Models/par-competenza-model';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';
import { CheckObjOn_Id_Number } from '../../../../ClientServer-Service/ModelsBase/check-obj';

@Component({
  selector: 'app-seletion-par-competenza-dialog',
  templateUrl: './seletion-par-competenza-dialog.component.html',
  styleUrls: ['./seletion-par-competenza-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class SeletionParCompetenzaDialogComponent {

  /** Competenze gia' assegnate: non vengono proposte. */
  @Input() idPar_Competenza_Escluse: number[] = [];

  public searchText!: string;
  public title!: string;
  public buttonbar: ButtonItem[] = [];

  result: SeletionParCompetenzaDialogResult = {
    id: []
  };

  public par_Competenza: Par_CompetenzaModel[] = [];
  public par_Competenza_checked: CheckObjOn_Id_Number[] = [];

  constructor(protected userInterfaceService: UserInterfaceService,
              private modalCtrl: ModalController,
              private transloco: TranslocoService,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              )
  {
    this.title = this.transloco.translate('attendance.components.seletionParCompetenzaDialog.title');
    this.buttonbar = userInterfaceService.Btn_ConfermaAnnulla;
    this.buttonbar[0].event = this._handleButtonConfirmClick;
    this.buttonbar[1].event = this._handleButtonCancelClick;
  }

  ionViewWillEnter() {
    this.par_Competenza = this.sharedParameterGestionePresenzeService.Par_Competenza ?? [];
  }


  private _handleButtonConfirmClick = (param: object) => {

    this.result.id = this.par_Competenza_checked.filter(item => item.checked)
      .map(item => item.id);

    return this.modalCtrl.dismiss(this.result, 'confirm');
  }

  private _handleButtonCancelClick = (param: object) => {
    return this.modalCtrl.dismiss(null, 'confirm');
  }


  public getPar_Competenza(): CheckObjOn_Id_Number[] {

    let retVal: CheckObjOn_Id_Number[] = [];

    this.par_Competenza.filter(comp =>
      comp.id > 0 && !this.idPar_Competenza_Escluse.includes(comp.id)
    ).forEach(item => {
      let appo = { id: item.id, checked: false };
      retVal.push(appo);
    });


    return retVal;
  }

  Par_Competenza_IsSelected(itemId: number): boolean {

    return this.par_Competenza_checked.find(entry => entry.id === itemId)?.checked ?? false;

  }

  Par_Competenza_Toggle(itemId: number, event: any) {

    const existingEntry = this.par_Competenza_checked.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.checked = event.detail.checked;
    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this.par_Competenza_checked.push({ id: itemId, checked: event.detail.checked });
    }

  }


}

export interface SeletionParCompetenzaDialogResult {
  id: number[];
}
