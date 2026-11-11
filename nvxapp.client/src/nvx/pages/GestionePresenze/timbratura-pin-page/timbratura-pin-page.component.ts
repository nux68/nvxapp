import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AlertController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';
import { NvxListAction, NvxListColumn } from '../../../shared/components/infrastructure/nvx-list/nvx-list.component';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { TimbraturaEvolutaService } from '../../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/timbratura-evoluta.service';
import { Dip_TimbraturaPinModel, TimbraturaEvoluta_PinGetAllInModel, TimbraturaEvoluta_PinPutInModel } from '../../../ClientServer-Service/GestionePresenze/TimbraturaEvoluta/Models/timbratura-evoluta-model';

// PIN e badge dei dipendenti per il kiosk (AT-TIMB.3): l'HR li imposta, il PIN non si rilegge
@Component({
  selector: 'app-timbratura-pin-page',
  templateUrl: './timbratura-pin-page.component.html',
  styleUrls: ['./timbratura-pin-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TimbraturaPinPageComponent {

  public title: string;
  public searchText!: string;
  public lista: Dip_TimbraturaPinModel[] | null = null;
  public readonly columns: NvxListColumn<Dip_TimbraturaPinModel>[];
  public actions: NvxListAction<Dip_TimbraturaPinModel>[] = [];
  public btnEdit: ButtonItem;

  constructor(private timbraturaEvolutaService: TimbraturaEvolutaService,
              private userInterfaceService: UserInterfaceService,
              private alertCtrl: AlertController,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('attendance.pages.timbraturaPin.title');
    this.columns = [
      { key: 'nominativo', title: this.transloco.translate('attendance.pages.timbraturaPin.employee') },
      { key: 'pinImpostato', title: 'PIN', value: x => x.pinImpostato ? this.transloco.translate('attendance.pages.timbraturaPin.set') : '-' },
      { key: 'badge', title: this.transloco.translate('attendance.pages.timbraturaPin.badge'), value: x => x.badge ?? '-' },
    ];
    this.btnEdit = userInterfaceService.Btn_Modifica;
    this.btnEdit.event = this.modifica;
    this.actions = [{ button: this.btnEdit }];
  }

  ionViewWillEnter() {
    this.carica();
  }

  private carica() {
    this.timbraturaEvolutaService.PinGetAll(new GenericRequest<TimbraturaEvoluta_PinGetAllInModel>(TimbraturaEvoluta_PinGetAllInModel))
      .subscribe(res => this.lista = res.data?.dipendenti ?? []);
  }

  modifica = async (item: Dip_TimbraturaPinModel) => {
    const alert = await this.alertCtrl.create({
      header: item.nominativo ?? '',
      message: this.transloco.translate('attendance.pages.timbraturaPin.hint'),
      inputs: [
        { name: 'pin', type: 'password', placeholder: 'PIN', attributes: { inputmode: 'numeric', maxlength: 8 } },
        { name: 'badge', type: 'text', placeholder: this.transloco.translate('attendance.pages.timbraturaPin.badge'), value: item.badge ?? '' }
      ],
      buttons: [
        { text: this.transloco.translate('common.buttons.cancel'), role: 'cancel' },
        {
          text: 'OK', handler: (v: any) => {
            const request = new GenericRequest<TimbraturaEvoluta_PinPutInModel>(TimbraturaEvoluta_PinPutInModel);
            request.data.idAspNetUsers = item.idAspNetUsers;
            request.data.pin = v.pin ? v.pin : null;
            request.data.badge = v.badge ?? '';
            this.timbraturaEvolutaService.PinPut(request).subscribe(() => this.carica());
          }
        }
      ]
    });
    await alert.present();
  }

  Filter(CurrFilter: any) {
    this.searchText = CurrFilter;
  }
}
