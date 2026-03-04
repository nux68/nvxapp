import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonicModule } from '@ionic/angular';

import { EditParOrarioDettaglioOrarioIntervalloHHDialogComponent } from './edit-par-orario-dettaglio-orario-intervallo-hhdialog.component';

describe('EditParOrarioDettaglioOrarioIntervalloHHDialogComponent', () => {
  let component: EditParOrarioDettaglioOrarioIntervalloHHDialogComponent;
  let fixture: ComponentFixture<EditParOrarioDettaglioOrarioIntervalloHHDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ EditParOrarioDettaglioOrarioIntervalloHHDialogComponent ],
      imports: [IonicModule.forRoot()]
    }).compileComponents();

    fixture = TestBed.createComponent(EditParOrarioDettaglioOrarioIntervalloHHDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
