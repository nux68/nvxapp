import { Component, ChangeDetectionStrategy, computed, input, model, signal } from '@angular/core';
import { IonCheckbox, IonIcon } from '@ionic/angular';
import { TranslocoPipe } from '@jsverse/transloco';

/*
 Opzioni di una pagina elenco, in una sezione espandibile sopra la lista (chiusa all'inizio):
    - "Mostra disabilitati" (se showDisabledLabel e' valorizzato): collegato alla lista con
      [(showDisabled)] e <nvx-list [disabledToggleExternal]="true" [(showDisabled)]="...">
    - "Abilita cancellazioni" (se deleteAvailable, cioe' l'utente puo' cancellare): la pagina mostra il
      pulsante Cancella solo con [(deleteEnabled)] a true
    - note (facoltativa): spiegazione della lista (es. il significato delle icone), in fondo alle opzioni
 Nuove opzioni delle liste vanno aggiunte qui. Senza opzioni disponibili la sezione non compare.
*/
@Component({
  selector: 'nvx-list-options',
  templateUrl: './nvx-list-options.component.html',
  styleUrls: ['./nvx-list-options.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: true,
  imports: [IonCheckbox, IonIcon, TranslocoPipe]
})
export class NvxListOptionsComponent {

  readonly showDisabledLabel = input<string | null>(null);
  readonly showDisabled = model(false);

  readonly deleteAvailable = input(false);
  readonly deleteEnabled = model(false);

  readonly note = input<string | null>(null);

  readonly open = signal(false);

  readonly hasOptions = computed(() => !!this.showDisabledLabel() || this.deleteAvailable() || !!this.note());

  toggle(): void {
    this.open.update(v => !v);
  }
}
