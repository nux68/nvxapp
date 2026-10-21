import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption, provideIonicAngular } from '@ionic/angular';

import { RequestClockingUserPageComponent } from './request-clocking-user-page.component';

describe('RequestClockingUserPageComponent', () => {
  let component: RequestClockingUserPageComponent;
  let fixture: ComponentFixture<RequestClockingUserPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ RequestClockingUserPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption]
    }).compileComponents();

    fixture = TestBed.createComponent(RequestClockingUserPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
