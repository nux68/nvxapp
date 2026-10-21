import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { IonActionSheet, IonCard, IonCardContent, IonChip, IonContent, IonIcon, provideIonicAngular } from '@ionic/angular';

import { TimeSheetUserPageComponent } from './time-sheet-user-page.component';

describe('TimeSheetUserComponent', () => {
  let component: TimeSheetUserPageComponent;
  let fixture: ComponentFixture<TimeSheetUserPageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ TimeSheetUserPageComponent ],
      providers: [provideIonicAngular()],
      imports: [IonActionSheet, IonCard, IonCardContent, IonChip, IonContent, IonIcon]
    }).compileComponents();

    fixture = TestBed.createComponent(TimeSheetUserPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
