import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonHeader, IonItem, IonLabel, IonModal, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonTitle, IonToggle, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { EditParOrarioDettaglioOrarioIntervalloHHDialogComponent } from './edit-par-orario-dettaglio-orario-intervallo-hhdialog.component';

describe('EditParOrarioDettaglioOrarioIntervalloHHDialogComponent', () => {
  let component: EditParOrarioDettaglioOrarioIntervalloHHDialogComponent;
  let fixture: ComponentFixture<EditParOrarioDettaglioOrarioIntervalloHHDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ EditParOrarioDettaglioOrarioIntervalloHHDialogComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonHeader, IonItem, IonLabel, IonModal, IonNote, IonRow, IonSegment, IonSegmentButton, IonSelect, IonSelectOption, IonTitle, IonToggle, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(EditParOrarioDettaglioOrarioIntervalloHHDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
