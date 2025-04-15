import { Component, OnInit } from '@angular/core';
import { NvxHttpInterceptorService } from '../../../infrastructure/http-interceptor';
import { ButtonItem, UserInterfaceService } from '../../../Utility/user-interface.service';
import { MessageType } from '../../../ClientServer-Service/ModelsBase/message';
import { NavController } from '@ionic/angular';

@Component({
  selector: 'app-http-background-error-dialog',
  templateUrl: './http-background-error-dialog.component.html',
  styleUrls: ['./http-background-error-dialog.component.scss'],
  standalone: false
})
export class HttpBackgroundErrorDialogComponent  implements OnInit {

  

  public isModalOpen: boolean = false;
  public buttonbar: ButtonItem[] = [];

  constructor(protected navCtrl: NavController,
              protected userInterfaceService: UserInterfaceService,
              public nvxHttpInterceptorService: NvxHttpInterceptorService)
  {
    this.buttonbar.push(userInterfaceService.Btn_Chiudi);
    this.buttonbar[0].event = this._handleButtonConfirmClick;
  }

  ngOnInit() {

    this.nvxHttpInterceptorService.isHttpErr$.subscribe(res => {
      this.isModalOpen = res;
    });

  }

  close() {
    this.nvxHttpInterceptorService.HttpError_Clear();
    this.isModalOpen = false;
  }

  onWillDismissErr(event: any) {
    this.close();
    this.navCtrl.back();
  }

  _handleButtonConfirmClick = (param: object) => {
    this.close();
  }

  getIcon(msgType: MessageType): string {
    switch (msgType) {
      case MessageType.Exception:
        return 'alert-circle-outline'; // Icona di errore grave
      case MessageType.Error:
        return 'close-circle-outline'; // Icona di errore
      case MessageType.Warning:
        return 'warning-outline'; // Icona di avviso
      case MessageType.Information:
        return 'information-circle-outline'; // Icona di informazione
      default:
        return 'help-circle-outline'; // Icona di default
    }
  }

  getDescription(msgType: MessageType): string {
    switch (msgType) {
      case MessageType.Exception:
        return 'Errore Critico';
      case MessageType.Error:
        return 'Errore';
      case MessageType.Warning:
        return 'Avviso';
      case MessageType.Information:
        return 'Informazione';
      default:
        return 'Sconosciuto';
    }
  }

}
