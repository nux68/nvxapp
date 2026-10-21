import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideIonicAngular } from '@ionic/angular';

import { NvxListColumn, NvxListComponent } from './nvx-list.component';

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
      imports: [NvxListComponent],
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
});
