import { Injectable } from '@angular/core';
import { ModalController } from '@ionic/angular';
import { ButtonItem } from '../../../../Utility/infrastructure/user-interface.service';
import { NvxTextPopupComponent } from './nvx-text-popup.component';

export interface NvxTextPopupOptions {
  title: string;
  subtitle?: string | null;
  text?: string | null;
  buttons: ButtonItem[];
}

@Injectable({
  providedIn: 'root'
})
export class NvxTextPopupService {

  constructor(private modalCtrl: ModalController) { }

  /** apre il popup; si chiude con un pulsante o toccando fuori */
  async show(options: NvxTextPopupOptions): Promise<void> {
    const modal = await this.modalCtrl.create({
      component: NvxTextPopupComponent,
      componentProps: { title: options.title, subtitle: options.subtitle, text: options.text, buttons: options.buttons },
      cssClass: 'nvx-text-popup-modal' // dimensioni in global.scss
    });
    await modal.present();
  }
}
