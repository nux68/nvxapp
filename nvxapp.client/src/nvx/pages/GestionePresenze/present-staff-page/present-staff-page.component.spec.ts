import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonButton, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonLabel, IonRow, provideIonicAngular } from '@ionic/angular';

import { PresentStaffPageComponent } from './present-staff-page.component';

describe('PresentStaffPageComponent', () => {
  let component: PresentStaffPageComponent;
  let fixture: ComponentFixture<PresentStaffPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ PresentStaffPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonButton, IonCard, IonCardContent, IonChip, IonCol, IonContent, IonGrid, IonIcon, IonLabel, IonRow]
    }).compileComponents();

    fixture = TestBed.createComponent(PresentStaffPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
