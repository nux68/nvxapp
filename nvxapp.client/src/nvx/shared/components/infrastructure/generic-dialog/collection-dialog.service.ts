import { Injectable, TemplateRef } from '@angular/core';
import { GenericDialogService } from './generic-dialog.service';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';

@Injectable({
  providedIn: 'root'
})
export class CollectionDialogService {

  constructor(private genericDialogService: GenericDialogService,
              private userInterfaceService: UserInterfaceService)
  {
  }

  public async ConfirmCancelDialog(message: string| TemplateRef<any> = 'Sei sicuro di voler procedere?'):Promise<any> {

    let TMP_buttonbar: ButtonItem[] = [];

    let btn_conferma = this.userInterfaceService.Btn_Conferma;
    btn_conferma.event = () => { return true; }
    let btn_annulla = this.userInterfaceService.Btn_Annulla;
    btn_annulla.event = () => { return false; }

    TMP_buttonbar.push(btn_conferma);
    TMP_buttonbar.push(btn_annulla);

    const result = await this.genericDialogService.show({
      title: 'Conferma Operazione',
      message: message,
      buttons: TMP_buttonbar
    });

    return result;

  }

}
