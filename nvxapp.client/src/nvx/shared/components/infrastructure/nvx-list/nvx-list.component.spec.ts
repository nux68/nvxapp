import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideIonicAngular } from '@ionic/angular';
import { TranslocoTestingModule } from '@jsverse/transloco';

import { NvxListAction, NvxListColumn, NvxListComponent } from './nvx-list.component';
import { ButtonItem } from '../../../../Utility/infrastructure/user-interface.service';

interface Row { id: number; descrizione: string; codice?: string; disabled?: boolean; }

describe('NvxListComponent', () => {
  let fixture: ComponentFixture<NvxListComponent<Row>>;
  let component: NvxListComponent<Row>;

  const items: Row[] = [
    { id: 1, descrizione: 'Beta', codice: 'B2' },
    { id: 2, descrizione: 'alfa', codice: 'A10', disabled: true },
    { id: 3, descrizione: 'Gamma', codice: 'A9' },
  ];
  const columns: NvxListColumn<Row>[] = [
    { key: 'descrizione', title: 'Descrizione' },
    { key: 'codice', title: 'Codice' },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TranslocoTestingModule.forRoot({ langs: {} }), NvxListComponent],
      providers: [provideIonicAngular()]
    }).compileComponents();

    fixture = TestBed.createComponent(NvxListComponent<Row>);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('items', items);
    fixture.componentRef.setInput('columns', columns);
  });

  const ids = () => component.rows().map(r => r.id);

  it('applica l\'ordinamento di default senza modificare i dati originali', () => {
    fixture.componentRef.setInput('sort', (a: Row, b: Row) => a.descrizione.localeCompare(b.descrizione));
    expect(ids()).toEqual([2, 1, 3]);
    expect(items.map(r => r.id)).toEqual([1, 2, 3]);
  });

  it('ordina per colonna: crescente, decrescente, poi torna al default', () => {
    const codice = columns[1];
    component.toggleSort(codice);
    expect(ids()).toEqual([3, 2, 1]);   // A9 < A10 < B2 (ordinamento numerico)
    component.toggleSort(codice);
    expect(ids()).toEqual([1, 2, 3]);
    component.toggleSort(codice);
    expect(component.sortKey()).toBeNull();
  });

  it('cerca su tutte le colonne, senza distinguere maiuscole', () => {
    fixture.componentRef.setInput('searchText', 'a1');
    expect(ids()).toEqual([2]);
    fixture.componentRef.setInput('searchText', 'GAM');
    expect(ids()).toEqual([3]);
  });

  it('nasconde le righe disabilitate finche\' non si spunta la casella', () => {
    fixture.componentRef.setInput('rowDisabled', (r: Row) => !!r.disabled);
    fixture.componentRef.setInput('disabledToggleLabel', 'Mostra disabilitati');
    expect(ids()).not.toContain(2);
    component.showDisabled.set(true);
    expect(ids()).toContain(2);
  });

  it('mostra la tabella su desktop', () => {
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;
    if (!component.ui.isMobile) {
      expect(el.querySelectorAll('table tbody tr').length).toBe(3);
      expect(el.querySelector('ion-item-sliding')).toBeNull();
    }
  });

  it('su desktop raccoglie le azioni inMenu in un unico menu, in fondo', () => {
    const del = new ButtonItem('Cancella', 'trash-outline', 'danger');
    const other = new ButtonItem('Altro', 'flash-outline', 'medium');
    const edit = new ButtonItem('Modifica', 'create-outline', 'medium');
    const actions: NvxListAction<Row>[] = [
      { button: del, inMenu: true },
      { button: edit },
      { button: other, inMenu: true, visible: r => r.id !== 3 },
    ];
    fixture.componentRef.setInput('actions', actions);

    const d1 = component.desktopActions(items[0]);
    expect(d1.map(d => d.kind)).toEqual(['button', 'menu']);
    expect(d1[1].kind === 'menu' ? d1[1].buttons : []).toEqual([del, other]);

    const d3 = component.desktopActions(items[2]);
    expect(d3[1].kind === 'menu' ? d3[1].buttons : []).toEqual([del]);
    // su mobile restano tutte opzioni a scorrimento
    expect(component.visibleActions(items[0]).length).toBe(3);
  });

  it('su desktop il bottone a tre puntini risulta allineato al bottone solo icona accanto', async () => {
    if (component.ui.isMobile) return;
    fixture.componentRef.setInput('actions', [
      { button: new ButtonItem('Modifica', 'create-outline', 'medium'), iconOnly: true },
      { button: new ButtonItem('Cancella', 'trash-outline', 'danger'), inMenu: true },
    ] as NvxListAction<Row>[]);
    fixture.detectChanges();
    await customElements.whenDefined('ion-button');
    await new Promise(r => setTimeout(r, 300));
    const td: HTMLElement = fixture.nativeElement.querySelector('td.actions');
    const [edit, menu] = Array.from(td.querySelectorAll('ion-button')).map(b => b.getBoundingClientRect());
    expect(menu).toBeTruthy();
    expect(Math.abs(edit.top - menu.top)).toBeLessThanOrEqual(1);
    expect(Math.abs(edit.height - menu.height)).toBeLessThanOrEqual(1);
  });
});
