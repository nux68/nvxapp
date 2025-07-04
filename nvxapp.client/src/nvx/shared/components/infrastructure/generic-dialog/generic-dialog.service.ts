import { Injectable, TemplateRef } from '@angular/core';
import { ModalController } from '@ionic/angular';
import { GenericDialogComponent } from './generic-dialog.component';
import { ButtonItem } from '../../../../Utility/infrastructure/user-interface.service';

@Injectable({
  providedIn: 'root'
})
export class GenericDialogService {

  constructor(private modalCtrl: ModalController) { }

  async show(options: DialogOptions): Promise<any> {
    const modal = await this.modalCtrl.create({
      component: GenericDialogComponent,
      // Passiamo i dati al nostro componente
      componentProps: {
        title: options.title,
        message: options.message,
        buttons: options.buttons,
      },
      // Impedisce la chiusura cliccando sullo sfondo, forzando una scelta
      backdropDismiss: options.backdropDismiss === undefined ? false : options.backdropDismiss,
      cssClass: 'custom-dialog-modal' // Classe per styling globale
    });

    await modal.present();

    // Attende che il modale venga chiuso (tramite un bottone)
    const { data } = await modal.onDidDismiss();

    // Ritorna il valore passato da dismiss() nel componente
    return data;
  }

}



export interface DialogOptions {
  title: string;
  message: string | TemplateRef<any>;
  buttons: ButtonItem[];
  backdropDismiss?: boolean; // Permette di chiudere il dialogo cliccando sullo sfondo
}
