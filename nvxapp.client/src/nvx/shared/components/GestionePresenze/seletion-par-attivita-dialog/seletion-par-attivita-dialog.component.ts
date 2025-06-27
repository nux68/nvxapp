import { Component, OnInit } from '@angular/core';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { ModalController } from '@ionic/angular';
import { Par_AttivitaModel } from '../../../../ClientServer-Service/GestionePresenze/Par_Attivita/Models/par-attivita-model';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';
import { CheckObjOn_Id_Number } from '../../../../ClientServer-Service/ModelsBase/check-obj';

@Component({
  selector: 'app-seletion-par-attivita-dialog',
  templateUrl: './seletion-par-attivita-dialog.component.html',
  styleUrls: ['./seletion-par-attivita-dialog.component.scss'],
  standalone: false
})
export class SeletionParAttivitaDialogComponent implements OnInit {

  public searchText!: string;
  public title!: string;
  public buttonbar: ButtonItem[] = [];

  result: SeletionParAttivitaDialogResult = {
    id: []
  };

  public par_Attivita: Par_AttivitaModel[] = [];
  public par_Attivita_checked: CheckObjOn_Id_Number[] = [];

  constructor(protected userInterfaceService: UserInterfaceService,
              private modalCtrl: ModalController,
              private sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              )
  {
    this.title = 'Seleziona Attività';
    this.buttonbar = userInterfaceService.Btn_ConfermaAnnulla;
    this.buttonbar[0].event = this._handleButtonConfirmClick;
    this.buttonbar[1].event = this._handleButtonCancelClick;
  }

  ionViewWillEnter() {
    this.par_Attivita = this.sharedParameterGestionePresenzeService.Par_Attivita;
  }

  ngOnInit() { }

  private _handleButtonConfirmClick = (param: object) => {
    
    this.result.id = this.par_Attivita_checked.filter(item => item.checked)
      .map(item => item.id);

    return this.modalCtrl.dismiss(this.result, 'confirm');
  }

  private _handleButtonCancelClick = (param: object) => {
    return this.modalCtrl.dismiss(null, 'confirm');
  }

  
  public getPar_Attivita(): CheckObjOn_Id_Number[] {

    let retVal: CheckObjOn_Id_Number[] = [];

    this.par_Attivita.filter(rep =>
      rep.id > 0
    ).forEach(item => {
      let appo = { id: item.id, checked: false };
      retVal.push(appo);
    });


    return retVal;
  }

  Par_Attivita_IsSelected(itemId: number): boolean {

    return this.par_Attivita_checked.find(entry => entry.id === itemId)?.checked ?? false;

  }

  Par_Attivita_Toggle(itemId: number, event: any) {

    const existingEntry = this.par_Attivita_checked.find(entry => entry.id === itemId);

    if (existingEntry) {
      // Se l'elemento esiste, aggiorna solo lo stato selected
      existingEntry.checked = event.detail.checked;
    } else {
      // Se l'elemento non è presente, lo aggiunge alla lista
      this.par_Attivita_checked.push({ id: itemId, checked: event.detail.checked });
    }

  }


}

export interface SeletionParAttivitaDialogResult {
  id: number[];
}
