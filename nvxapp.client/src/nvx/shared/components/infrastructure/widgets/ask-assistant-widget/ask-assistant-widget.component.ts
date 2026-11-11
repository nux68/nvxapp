import { Component, ChangeDetectionStrategy } from '@angular/core';
import { IonIcon } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';
import { FabMenuService } from '../../../../../Utility/infrastructure/fab-menu.service';

/*
 Widget "Chiedi all'assistente" (Infrastructure, ANALISI_INFRASTRUCTURE_WIDGET.md): campo a pillola che apre la chat del FAB.
*/
@Component({
  selector: 'app-ask-assistant-widget',
  templateUrl: './ask-assistant-widget.component.html',
  styleUrls: ['./ask-assistant-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [IonIcon, TranslocoPipe]
})
export class AskAssistantWidgetComponent {

  constructor(private fabMenuService: FabMenuService) { }

  openChat() {
    this.fabMenuService.openChat();
  }
}
