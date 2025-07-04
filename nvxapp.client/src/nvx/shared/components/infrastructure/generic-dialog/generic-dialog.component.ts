import { Component, Input, OnInit } from '@angular/core';
import { ModalController } from '@ionic/angular';
import { ButtonItem } from '../../../../Utility/infrastructure/user-interface.service';

@Component({
  selector: 'app-generic-dialog',
  templateUrl: './generic-dialog.component.html',
  styleUrls: ['./generic-dialog.component.scss'],
  standalone: false
}) 
export class GenericDialogComponent {

  @Input() title: string;
  @Input() message: string;
  @Input() buttons: ButtonItem[] = [];
  @Input() local_buttons: ButtonItem[] = [];

  get Title(): string { return "User"; }


  constructor(private modalCtrl: ModalController) {

  }

  ionViewWillEnter() {
    this.buttons.forEach(item => {


      this.local_buttons.push(
        new ButtonItem(
          item.text,
          item.image,
          item.color,
          item.disabled,
          () => {
            this.modalCtrl.dismiss( item.event({}) );
          }
        )
      );

    });

  }

  ///**
  // * Chiude il modale, ritornando il valore associato al bottone.
  // * @param button Il bottone cliccato
  // */
  //async onButtonClick(button: DialogButton) {
  //  // Se è definito un handler, lo eseguiamo
  //  if (button.handler) {
  //    // Se l'handler ritorna `false`, non chiudiamo il modale
  //    const result = await Promise.resolve(button.handler(button.value));
  //    if (result === false) {
  //      return;
  //    }
  //  }
  //  // Chiudiamo il modale e passiamo il valore del bottone
  //  this.modalCtrl.dismiss(button.value, button.role);
  //}
}


