import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { IonButton, IonIcon, ModalController } from '@ionic/angular';
import { ButtonItem } from '../../../../Utility/infrastructure/user-interface.service';

/*
 Popup con un testo per intero (titolo, riga secondaria, testo, pulsanti), alto quanto il contenuto: per i testi che
 su mobile le liste mostrano accorciati. Si apre con NvxTextPopupService; ogni pulsante chiude il popup e poi esegue il suo evento.
*/
@Component({
  selector: 'nvx-text-popup',
  templateUrl: './nvx-text-popup.component.html',
  styleUrls: ['./nvx-text-popup.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: true,
  imports: [IonButton, IonIcon]
})
export class NvxTextPopupComponent {

  @Input() title = '';
  @Input() subtitle: string | null = null;
  @Input() text: string | null = null;
  @Input() buttons: ButtonItem[] = [];

  constructor(private modalCtrl: ModalController) { }

  async premuto(button: ButtonItem) {
    await this.modalCtrl.dismiss();
    button.event({});
  }
}
