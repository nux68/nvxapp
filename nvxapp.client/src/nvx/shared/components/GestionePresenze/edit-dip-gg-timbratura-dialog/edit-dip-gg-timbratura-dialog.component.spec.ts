import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonCardHeader, IonCardTitle, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonHeader, IonItem, IonLabel, IonModal, IonNote, IonRow, IonSelect, IonSelectOption, IonTitle, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { EditDipGGTimbraturaDialogComponent } from './edit-dip-gg-timbratura-dialog.component';

describe('EditDipGGTimbraturaDialogComponent', () => {
  let component: EditDipGGTimbraturaDialogComponent;
  let fixture: ComponentFixture<EditDipGGTimbraturaDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ EditDipGGTimbraturaDialogComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonCardHeader, IonCardTitle, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonHeader, IonItem, IonLabel, IonModal, IonNote, IonRow, IonSelect, IonSelectOption, IonTitle, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(EditDipGGTimbraturaDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
