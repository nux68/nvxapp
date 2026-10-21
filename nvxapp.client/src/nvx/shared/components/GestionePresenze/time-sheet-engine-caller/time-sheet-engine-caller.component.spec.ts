import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonCard, IonCardContent, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonHeader, IonItem, IonLabel, IonModal, IonRow, IonSegment, IonSegmentButton, IonTitle, IonToggle, IonToolbar, provideIonicAngular } from '@ionic/angular';

import { TimeSheetEngineCallerComponent } from './time-sheet-engine-caller.component';

describe('TimeSheetEngineCallerComponent', () => {
  let component: TimeSheetEngineCallerComponent;
  let fixture: ComponentFixture<TimeSheetEngineCallerComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ TimeSheetEngineCallerComponent ],
      providers: [provideIonicAngular()],
      imports: [IonCard, IonCardContent, IonCol, IonContent, IonDatetime, IonDatetimeButton, IonHeader, IonItem, IonLabel, IonModal, IonRow, IonSegment, IonSegmentButton, IonTitle, IonToggle, IonToolbar]
    }).compileComponents();

    fixture = TestBed.createComponent(TimeSheetEngineCallerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
