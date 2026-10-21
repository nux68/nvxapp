import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption, provideIonicAngular } from '@ionic/angular';

import { TimeClockUserPageComponent } from './time-clock-user-page.component';

describe('TimeClockUserPageComponent', () => {
  let component: TimeClockUserPageComponent;
  let fixture: ComponentFixture<TimeClockUserPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ TimeClockUserPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonContent, IonDatetime, IonDatetimeButton, IonItem, IonLabel, IonModal, IonNote, IonSelect, IonSelectOption]
    }).compileComponents();

    fixture = TestBed.createComponent(TimeClockUserPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
