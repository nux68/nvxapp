import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { NvxHttpInterceptorService } from '../../../../http/http-interceptor';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { MessageType } from '../../../../ClientServer-Service/ModelsBase/message';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';

@Component({
  selector: 'app-http-background-error-dialog',
  templateUrl: './http-background-error-dialog.component.html',
  styleUrls: ['./http-background-error-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class HttpBackgroundErrorDialogComponent  implements OnInit {

  

  public isModalOpen: boolean = false;
  public buttonbar: ButtonItem[] = [];

  constructor(protected navCtrl: NavController,
              protected userInterfaceService: UserInterfaceService,
              public nvxHttpInterceptorService: NvxHttpInterceptorService,
              private transloco: TranslocoService)
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
        return this.transloco.translate('components.httpBackgroundErrorDialog.severityException');
      case MessageType.Error:
        return this.transloco.translate('components.httpBackgroundErrorDialog.severityError');
      case MessageType.Warning:
        return this.transloco.translate('components.httpBackgroundErrorDialog.severityWarning');
      case MessageType.Information:
        return this.transloco.translate('components.httpBackgroundErrorDialog.severityInformation');
      default:
        return this.transloco.translate('components.httpBackgroundErrorDialog.severityUnknown');
    }
  }

}
