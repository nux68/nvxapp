import { Component, Input, OnInit } from '@angular/core';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { ModalController } from '@ionic/angular';
import { Dip_ProfiloOrarioModel } from '../../../../ClientServer-Service/GestionePresenze/Dip_ProfiloOrario/Models/dip-profilo-orario-model';
import { SharedParameterGestionePresenzeService } from '../../../shared-parameter-gestione-presenze.service';

@Component({
  selector: 'app-edit-dip-profilo-orario-dialog',
  templateUrl: './edit-dip-profilo-orario-dialog.component.html',
  styleUrls: ['./edit-dip-profilo-orario-dialog.component.scss'],
  standalone: false
})
export class EditDipProfiloOrarioDialogComponent  implements OnInit {

  @Input() dip_ProfiloOrario: Dip_ProfiloOrarioModel;

  public title!: string;
  public buttonbar: ButtonItem[] = [];

 
  constructor(protected userInterfaceService: UserInterfaceService,
              public sharedParameterGestionePresenzeService: SharedParameterGestionePresenzeService,
              private modalCtrl: ModalController)
  {
    this.title = 'Profilo orario';

    this.buttonbar = userInterfaceService.Btn_ConfermaAnnulla;
    this.buttonbar[0].event = this._handleButtonConfirmClick;
    this.buttonbar[1].event = this._handleButtonCancelClick;
  }

  ngOnInit() {
    let mimmo = this.dip_ProfiloOrario;

    

  }

  private _handleButtonConfirmClick = (param: object) => {
    return this.modalCtrl.dismiss(this.dip_ProfiloOrario, 'confirm');
  }

  private _handleButtonCancelClick = (param: object) => {
    return this.modalCtrl.dismiss(null, 'confirm');
  }


  getDayProf(): number {

    if (this.dip_ProfiloOrario.idPar_ProfiloOrario == 0)
      return 1;

    let prof = this.sharedParameterGestionePresenzeService.Par_ProfiloOrario.find(x => x.id == this.dip_ProfiloOrario.idPar_ProfiloOrario);

    if (prof != null)
      return prof.numGiorniCiclo;

    return 1;
    

  }


}

