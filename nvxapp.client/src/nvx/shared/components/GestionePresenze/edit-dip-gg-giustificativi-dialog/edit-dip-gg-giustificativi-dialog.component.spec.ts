import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonCardHeader, IonCardTitle, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonHeader, IonItem, IonLabel, IonModal, IonNote, IonRow, IonSelect, IonSelectOption, IonTitle, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { EditDipGGGiustificativiDialogComponent } from './edit-dip-gg-giustificativi-dialog.component';

describe('EditDipGGGiustificativiDialogComponent', () => {
  let component: EditDipGGGiustificativiDialogComponent;
  let fixture: ComponentFixture<EditDipGGGiustificativiDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ EditDipGGGiustificativiDialogComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonCardHeader, IonCardTitle, IonContent, IonDatetime, IonDatetimeButton, IonGrid, IonHeader, IonItem, IonLabel, IonModal, IonNote, IonRow, IonSelect, IonSelectOption, IonTitle, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(EditDipGGGiustificativiDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
