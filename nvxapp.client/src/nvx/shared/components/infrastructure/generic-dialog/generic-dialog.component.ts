import { Component, Input, OnInit, TemplateRef } from '@angular/core';
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

  public isTemplateRef(value: any): value is TemplateRef<any> {
    return value instanceof TemplateRef;
  }


}


