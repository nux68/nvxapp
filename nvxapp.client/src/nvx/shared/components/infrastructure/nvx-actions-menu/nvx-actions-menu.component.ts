import { Component, ChangeDetectionStrategy, computed, inject, input, output, signal } from '@angular/core';
import { IonButton, IonIcon, IonItem, IonLabel, IonList, IonPopover } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { ButtonItem } from '../../../../Utility/infrastructure/user-interface.service';
import { HoverPopupDirective } from '../hover-popup/hover-popup.directive';
import { HoverPopupService } from '../hover-popup/hover-popup.service';

/*
 Bottone "altre azioni" (tre puntini in verticale): premuto apre un menu a discesa con le opzioni ricevute.
 Ogni opzione e' un ButtonItem (testo, icona, colore, disabilitato, evento): alla scelta il menu si chiude
 e viene chiamato button.event(param), poi emesso ev_Click.
   <nvx-actions-menu [buttons]="[btnCancella]" [param]="item"></nvx-actions-menu>
*/
@Component({
  selector: 'nvx-actions-menu',
  templateUrl: './nvx-actions-menu.component.html',
  styleUrls: ['./nvx-actions-menu.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: true,
  imports: [IonButton, IonIcon, IonItem, IonLabel, IonList, IonPopover, HoverPopupDirective]
})
export class NvxActionsMenuComponent {

  /** opzioni del menu, nell'ordine in cui compaiono */
  readonly buttons = input<ButtonItem[]>([]);
  /** valore passato all'evento dell'opzione (es. la riga della lista) */
  readonly param = input<any>(null);
  /** testo accessibile e tooltip del bottone */
  readonly label = input<string>(inject(TranslocoService).translate('components.nvxActionsMenu.more'));

  readonly ev_Click = output<{ button: ButtonItem; param: any }>();

  readonly isOpen = signal(false);
  private readonly hoverPopup = inject(HoverPopupService);
  /** evento del click: il popover si apre allineato al bottone premuto */
  openEvent: Event | null = null;

  /** bottone disabilitato se nessuna opzione e' utilizzabile */
  readonly allDisabled = computed(() => this.buttons().every(b => b.disabled));

  open(ev: Event): void {
    ev.stopPropagation();
    this.hoverPopup.hide();   // il popup del passaggio del mouse non resta sopra il menu
    this.openEvent = ev;
    this.isOpen.set(true);
  }

  async select(popover: IonPopover, button: ButtonItem): Promise<void> {
    if (button.disabled) return;
    // prima si chiude il menu, cosi' un'eventuale finestra di conferma non resta sotto il popover
    await popover.dismiss();
    button.event(this.param());
    this.ev_Click.emit({ button, param: this.param() });
  }
}
