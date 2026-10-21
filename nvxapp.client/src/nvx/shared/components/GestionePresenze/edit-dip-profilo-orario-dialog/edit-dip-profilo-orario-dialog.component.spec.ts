import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonHeader, IonItem, IonLabel, IonModal, IonRange, IonSelect, IonSelectOption, IonTitle, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { EditDipProfiloOrarioDialogComponent } from './edit-dip-profilo-orario-dialog.component';

describe('EditDipProfiloOrarioDialogComponent', () => {
  let component: EditDipProfiloOrarioDialogComponent;
  let fixture: ComponentFixture<EditDipProfiloOrarioDialogComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ EditDipProfiloOrarioDialogComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonHeader, IonItem, IonLabel, IonModal, IonRange, IonSelect, IonSelectOption, IonTitle, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(EditDipProfiloOrarioDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
