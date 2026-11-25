import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';

/** pulsanti visibili su mobile; gli altri stanno nel menu a tre puntini */
const MAX_MOBILE = 2;

/*
 Barra dei pulsanti in fondo a pagine e dialog.
 Desktop: tutti i pulsanti, con icona e testo.
 Mobile: pulsanti rotondi con la sola icona, al massimo due; dal terzo in poi si aggiunge il bottone a tre puntini,
 che apre gli altri con icona e testo. Restano in vista Conferma e Annulla; dove mancano, le azioni principali (colore
 primary), poi l'ordine della pagina; le cancellazioni (cestino) per ultime.
*/
@Component({
  selector: 'app-page-buttonbar',
  templateUrl: './page-buttonbar.component.html',
  styleUrls: ['./page-buttonbar.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class PageButtonbarComponent {

  @Input() buttonbar: ButtonItem[]=[];

  constructor(public ui: UserInterfaceService) { }

  // ricalcolati solo quando cambiano i pulsanti: le pagine possono anche modificare l'array ricevuto
  private chiave = '';
  private visibili: ButtonItem[] = [];
  private altri: ButtonItem[] = [];

  get mobileVisibili(): ButtonItem[] {
    this.dividi();
    return this.visibili;
  }

  get mobileAltri(): ButtonItem[] {
    this.dividi();
    return this.altri;
  }

  /** stile del pulsante rotondo: pieno se principale, altrimenti solo bordo (senza bordo non si vedrebbe la forma) */
  mobileFill(item: ButtonItem): 'solid' | 'outline' {
    return item.fill === 'solid' || (!item.fill && item.color === 'primary') ? 'solid' : 'outline';
  }

  private dividi(): void {
    const tutti = this.buttonbar ?? [];
    const uguali = this.chiave === tutti.map(b => b.color + '|' + b.fill).join(',')
      && this.visibili.length + this.altri.length === tutti.length
      && tutti.every(b => this.visibili.includes(b) || this.altri.includes(b));
    if (uguali)
      return;

    this.chiave = tutti.map(b => b.color + '|' + b.fill).join(',');
    if (tutti.length <= MAX_MOBILE) {
      this.visibili = [...tutti];
      this.altri = [];
      return;
    }
    // sort stabile: a parita' di peso resta l'ordine della pagina
    const scelti = [...tutti].sort((a, b) => this.peso(a) - this.peso(b)).slice(0, MAX_MOBILE);
    this.visibili = tutti.filter(b => scelti.includes(b));
    this.altri = tutti.filter(b => !scelti.includes(b));
  }

  private peso(item: ButtonItem): number {
    if (item.main)
      return 0;
    if (this.mobileFill(item) === 'solid')
      return 1;
    return item.image?.startsWith('trash') ? 3 : 2;
  }
}
