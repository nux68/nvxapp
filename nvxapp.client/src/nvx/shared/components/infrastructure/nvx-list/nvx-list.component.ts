import { Component, ChangeDetectionStrategy, Directive, TemplateRef, computed, contentChild, contentChildren, inject, input, signal } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { IonButton, IonCheckbox, IonIcon, IonItem, IonItemOption, IonItemOptions, IonItemSliding, IonLabel, IonList, IonNote } from '@ionic/angular';
import { ButtonItem, UserInterfaceService } from '../../../../Utility/infrastructure/user-interface.service';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';

/** Colonna della lista: in tabella (desktop) e' una colonna, nella riga mobile titolo / sottotitolo / nota. */
export interface NvxListColumn<T = any> {
  key: string;
  title?: string;
  /** testo mostrato (default item[key]); usato anche per ricerca e ordinamento */
  value?: (item: T) => any;
  /** valore per l'ordinamento, se diverso dal testo (es. date) */
  sortValue?: (item: T) => any;
  sortable?: boolean;      // default true
  searchable?: boolean;    // default true
  width?: string;
  align?: 'start' | 'center' | 'end';
  cellClass?: string;
  /** posizione nella riga mobile; default: prima colonna 'title', le altre 'subtitle' */
  mobile?: 'title' | 'subtitle' | 'note' | 'hidden';
  /** false = solo nella riga mobile, non in tabella */
  desktop?: boolean;
}

/** Azione di riga: bottone in tabella, opzione a scorrimento su mobile. */
export interface NvxListAction<T = any> {
  button: ButtonItem;
  visible?: (item: T) => boolean;
  /** classe dell'ion-item-option (default dal colore del bottone) */
  mobileClass?: string;
}

/** Cella personalizzata: <ng-template nvxCell="key" let-item>...</ng-template> */
@Directive({ selector: 'ng-template[nvxCell]', standalone: true })
export class NvxListCellDirective {
  readonly nvxCell = input.required<string>();
  readonly template = inject(TemplateRef);
}

/** Contenuto della riga mobile al posto di titolo/sottotitoli: <ng-template nvxMobile let-item>...</ng-template> */
@Directive({ selector: 'ng-template[nvxMobile]', standalone: true })
export class NvxListMobileDirective {
  readonly template = inject(TemplateRef);
}

const MOBILE_OPTION_CLASS: Record<string, string> = {
  danger: 'item-option-text-only_cancella',
  medium: 'item-option-text-only_modifica',
  primary: 'item-option-text-only_primary',
  warning: 'item-option-text-only_warning',
};

// Lista condivisa delle pagine elenco: <table> su desktop, ion-item-sliding su mobile.
// Viene disegnata solo la versione del dispositivo (non entrambe nascoste via CSS) e filtro/ordinamento
// sono ricalcolati solo quando cambiano dati, ricerca o ordinamento.
@Component({
  selector: 'nvx-list',
  templateUrl: './nvx-list.component.html',
  styleUrls: ['./nvx-list.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: true,
  imports: [NgTemplateOutlet, IonButton, IonCheckbox, IonIcon, IonItem, IonItemOption, IonItemOptions, IonItemSliding, IonLabel, IonList, IonNote, TranslocoPipe]
})
export class NvxListComponent<T = any> {

  readonly items = input<T[] | null | undefined>(null);
  readonly columns = input<NvxListColumn<T>[]>([]);
  readonly actions = input<NvxListAction<T>[]>([]);
  readonly trackKey = input<string>('id');
  readonly icon = input<string | ((item: T) => string) | null>(null);
  readonly highlight = input<((item: T) => boolean) | null>(null);
  readonly searchText = input<string | null | undefined>(null);
  /** colonne su cui cercare (default: tutte quelle con searchable != false) */
  readonly searchKeys = input<string[] | null>(null);
  /** ordinamento di default, finche' non si clicca un'intestazione */
  readonly sort = input<((a: T, b: T) => number) | null>(null);
  readonly rowDisabled = input<((item: T) => boolean) | null>(null);
  /** se valorizzato compare la casella "mostra disabilitati" e le righe disabilitate sono nascoste finche' non si spunta */
  readonly disabledToggleLabel = input<string | null>(null);
  readonly emptyText = input<string>(inject(TranslocoService).translate('components.nvxList.empty'));

  readonly showDisabled = signal(false);
  readonly sortKey = signal<string | null>(null);
  readonly sortDir = signal<1 | -1>(1);

  private readonly cellTemplates = contentChildren(NvxListCellDirective);
  readonly mobileTemplate = contentChild(NvxListMobileDirective);

  readonly ui = inject(UserInterfaceService);

  readonly cellTemplate = computed(() => {
    const map = new Map<string, TemplateRef<any>>();
    for (const c of this.cellTemplates()) map.set(c.nvxCell(), c.template);
    return map;
  });

  readonly mobileTitle = computed(() => this.columns().find((c, i) => (c.mobile ?? (i === 0 ? 'title' : 'subtitle')) === 'title'));
  readonly mobileSubtitles = computed(() => this.columns().filter((c, i) => (c.mobile ?? (i === 0 ? 'title' : 'subtitle')) === 'subtitle'));
  readonly mobileNotes = computed(() => this.columns().filter(c => c.mobile === 'note'));

  /** righe da mostrare: copia dei dati, mai ordinati sul posto (possono essere la cache condivisa) */
  readonly rows = computed(() => {
    let rows = [...(this.items() ?? [])];

    const disabled = this.rowDisabled();
    if (disabled && this.disabledToggleLabel() && !this.showDisabled()) {
      rows = rows.filter(r => !disabled(r));
    }

    const text = (this.searchText() ?? '').trim().toLowerCase();
    if (text) {
      const keys = this.searchKeys();
      const cols = this.columns().filter(c => keys ? keys.includes(c.key) : c.searchable !== false);
      rows = rows.filter(r => cols.some(c => this.text(c, r).toLowerCase().includes(text)));
    }

    const key = this.sortKey();
    const col = key ? this.columns().find(c => c.key === key) : null;
    if (col) {
      const dir = this.sortDir();
      rows.sort((a, b) => dir * NvxListComponent.compare(this.sortValueOf(col, a), this.sortValueOf(col, b)));
    }
    else if (this.sort()) {
      rows.sort(this.sort());
    }
    return rows;
  });

  readonly tableColumns = computed(() => this.columns().filter(c => c.desktop !== false));

  readonly hasActions = computed(() => this.actions().length > 0);

  value(col: NvxListColumn<T>, item: T): any {
    return col.value ? col.value(item) : (item as any)?.[col.key];
  }

  text(col: NvxListColumn<T>, item: T): string {
    const v = this.value(col, item);
    return v == null ? '' : String(v);
  }

  private sortValueOf(col: NvxListColumn<T>, item: T): any {
    return col.sortValue ? col.sortValue(item) : this.value(col, item);
  }

  private static compare(a: any, b: any): number {
    if (a == null || a === '') return (b == null || b === '') ? 0 : 1; // vuoti in fondo
    if (b == null || b === '') return -1;
    if (typeof a === 'number' && typeof b === 'number') return a - b;
    if (a instanceof Date && b instanceof Date) return a.getTime() - b.getTime();
    return String(a).localeCompare(String(b), undefined, { numeric: true, sensitivity: 'base' });
  }

  /** click su un'intestazione: crescente, decrescente, poi di nuovo l'ordinamento di default */
  toggleSort(col: NvxListColumn<T>): void {
    if (col.sortable === false) return;
    if (this.sortKey() !== col.key) {
      this.sortKey.set(col.key);
      this.sortDir.set(1);
    }
    else if (this.sortDir() === 1) {
      this.sortDir.set(-1);
    }
    else {
      this.sortKey.set(null);
    }
  }

  ariaSort(col: NvxListColumn<T>): string | null {
    if (col.sortable === false) return null;
    if (this.sortKey() !== col.key) return 'none';
    return this.sortDir() === 1 ? 'ascending' : 'descending';
  }

  trackOf(item: T): any {
    return (item as any)?.[this.trackKey()] ?? item;
  }

  iconOf(item: T): string | null {
    const icon = this.icon();
    return typeof icon === 'function' ? icon(item) : icon;
  }

  isHighlighted(item: T): boolean {
    return this.highlight()?.(item) ?? false;
  }

  isDisabled(item: T): boolean {
    return this.rowDisabled()?.(item) ?? false;
  }

  visibleActions(item: T): NvxListAction<T>[] {
    return this.actions().filter(a => !a.visible || a.visible(item));
  }

  optionClass(action: NvxListAction<T>): string {
    return action.mobileClass ?? MOBILE_OPTION_CLASS[action.button.color ?? ''] ?? 'item-option-text-only_medium';
  }
}
